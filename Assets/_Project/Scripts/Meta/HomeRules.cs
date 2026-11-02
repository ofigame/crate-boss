using System;
using System.Collections.Generic;

namespace CrateBoss.Meta
{
    /// <summary>
    /// The Boss's land (Ana Belge §11–§12): delivery crates earned and opened, the three choices of a crate, placing items on
    /// the lands by the placement rules, paths, the inventory. Plain C#, saved in <see cref="SaveData"/>.
    /// </summary>
    public static class HomeRules
    {
        // ---------- crates (§12.1, §12.2) ----------

        /// <summary>First clear of a level: one delivery crate; promotion levels add a promotion crate.</summary>
        public static void EarnCrate(SaveData d, bool promotionRank, int rank)
        {
            d.crates++;
            if (promotionRank) d.promoCrates.Add(rank);
        }

        public static bool HasCrate(SaveData d) => d.crates > 0 || d.promoCrates.Count > 0;

        /// <summary>
        /// The three items a crate offers (§12.2): from the rank's set and not owned yet; when the set is used up, missing
        /// items of earlier ranks; after that decor (which repeats). Every fifth crate offers at least one big item; usually
        /// one big, one medium, one small. A promotion crate offers the rank's special item and two big ones.
        /// </summary>
        public static List<string> Offer(ItemCatalog cat, SaveData d, int rank, Random rng, int promotionRank = 0)
        {
            bool Owned(ItemDef i) => !i.Decor && (d.owned.Contains(i.Id) || d.inventory.Contains(i.Id) || d.placed.Exists(p => p.id == i.Id));
            var pool = new List<ItemDef>();
            foreach (var i in cat.Items) if (i.Rank == rank && i.Size != "special" && !Owned(i)) pool.Add(i);
            if (pool.Count < 3)
                foreach (var i in cat.Items) if (i.Rank > 0 && i.Rank < rank && i.Size != "special" && !Owned(i)) pool.Add(i);
            var result = new List<string>();
            if (promotionRank > 0)
            {
                foreach (var i in cat.Items) if (i.Rank == promotionRank && i.Size == "special" && !Owned(i)) { result.Add(i.Id); break; }
                Pick(pool, rng, result, x => x.Big, 3);
            }
            else
            {
                bool needBig = (d.cratesOpened + 1) % 5 == 0;
                Pick(pool, rng, result, x => x.Big, 1);
                if (!needBig && result.Count > 0 && rng.Next(3) == 0) result.Clear(); // the guarantee holds every fifth crate, often earlier
                Pick(pool, rng, result, x => x.Size == "medium", result.Count + 1);
                Pick(pool, rng, result, x => x.Size == "small", result.Count + 1);
            }
            Pick(pool, rng, result, x => true, 3);
            Pick(new List<ItemDef>(cat.Items), rng, result, x => x.Decor, 3);
            return result;
        }

        static void Pick(List<ItemDef> pool, Random rng, List<string> into, Func<ItemDef, bool> want, int upTo)
        {
            var options = pool.FindAll(i => want(i) && !into.Contains(i.Id));
            while (into.Count < upTo && options.Count > 0)
            {
                var i = options[rng.Next(options.Count)];
                options.Remove(i);
                into.Add(i.Id);
            }
        }

        /// <summary>Opening a crate: costs CB Coin (unless free or paid by an ad); false when the balance is short. A promotion crate is free.</summary>
        public static bool Open(SaveData d, Wallet wallet, EconomyConfig cfg, bool promotion, bool free)
        {
            if (promotion)
            {
                if (d.promoCrates.Count == 0) return false;
                d.promoCrates.RemoveAt(0);
            }
            else
            {
                if (d.crates <= 0) return false;
                bool pay = !free && d.firstCrateOpened;
                if (pay && !wallet.TrySpend(cfg.crateOpen)) return false;
                d.crates--;
                d.firstCrateOpened = true; // the very first crate is free (§11.1)
            }
            d.cratesOpened++;
            return true;
        }

        /// <summary>The chosen item goes to the inventory (placed right away or later); the others are gone.</summary>
        public static void Choose(SaveData d, string id)
        {
            if (!string.IsNullOrEmpty(id)) d.inventory.Add(id);
        }

        // ---------- placement (§12.4) ----------

        /// <summary>Footprint in cells after turning (quarter turns swap width and depth).</summary>
        public static (int w, int d) Size(ItemDef item, int rot) => rot % 2 == 0 ? (item.W, item.D) : (item.D, item.W);

        public static bool IsPath(SaveData d, string land, int x, int y) => d.paths.Contains(PathKey(land, x, y));
        public static string PathKey(string land, int x, int y) => land + ":" + x + ":" + y;

        /// <summary>
        /// May the item stand here? Inside the land, on no other item, on the right ground for its kind (§12.4 table):
        /// buildings on land with the door side facing a path or the road (front row), vehicles on paths, boats on water,
        /// piers on land next to water, garden and ground pieces on land (not water, not path).
        /// </summary>
        public static bool CanPlace(SaveData d, ItemCatalog cat, LandDef land, ItemDef item, int x, int y, int rot, PlacedItem ignore = null)
        {
            if (land == null || item == null || !d.lands.Contains(land.Id)) return false;
            var (w, dd) = Size(item, rot);
            for (int cy = y; cy < y + dd; cy++)
            for (int cx = x; cx < x + w; cx++)
            {
                if (!land.InBounds(cx, cy)) return false;
                if (OccupiedBy(d, cat, land.Id, cx, cy, ignore) != null) return false;
                bool water = land.IsWater(cx, cy), path = IsPath(d, land.Id, cx, cy);
                switch (item.Kind)
                {
                    case "water": if (!water) return false; break;
                    case "vehicle": if (!path && cy != land.D - 1) return false; break; // a path, or the kerb along the road
                    default: if (water || path) return false; break;
                }
            }
            if (item.Kind == "building")
            {
                // The door side (rot 0: front, toward the road) must touch a path or the road.
                foreach (var (fx, fy) in DoorCells(x, y, w, dd, rot))
                {
                    if (rot % 4 == 0 && fy == land.D) return true;                       // straight onto the city road
                    if (land.InBounds(fx, fy) && IsPath(d, land.Id, fx, fy)) return true; // onto the Boss's own path
                }
                return false;
            }
            if (item.Kind == "shore")
            {
                for (int cy = y - 1; cy <= y + dd; cy++)
                for (int cx = x - 1; cx <= x + w; cx++)
                    if (land.InBounds(cx, cy) && land.IsWater(cx, cy)) return true;
                return false;
            }
            return true;
        }

        /// <summary>The row of cells just outside the door side.</summary>
        public static IEnumerable<(int, int)> DoorCells(int x, int y, int w, int d, int rot)
        {
            switch (rot % 4)
            {
                case 0: for (int i = 0; i < w; i++) yield return (x + i, y + d); break;   // front (toward the road)
                case 1: for (int i = 0; i < d; i++) yield return (x - 1, y + i); break;   // left
                case 2: for (int i = 0; i < w; i++) yield return (x + i, y - 1); break;   // back
                default: for (int i = 0; i < d; i++) yield return (x + w, y + i); break;  // right
            }
        }

        public static PlacedItem OccupiedBy(SaveData d, ItemCatalog cat, string land, int x, int y, PlacedItem ignore = null)
        {
            foreach (var p in d.placed)
            {
                if (p == ignore || p.land != land) continue;
                var item = cat.Get(p.id);
                if (item == null) continue;
                var (w, dd) = Size(item, p.rot);
                if (x >= p.x && x < p.x + w && y >= p.y && y < p.y + dd) return p;
            }
            return null;
        }

        /// <summary>Puts an item from the inventory on the land; false (and nothing changes) when the place is not allowed.</summary>
        public static bool Place(SaveData d, ItemCatalog cat, string id, LandDef land, int x, int y, int rot)
        {
            var item = cat.Get(id);
            if (!d.inventory.Contains(id) || !CanPlace(d, cat, land, item, x, y, rot)) return false;
            d.inventory.Remove(id);
            d.placed.Add(new PlacedItem { id = id, land = land.Id, x = x, y = y, rot = rot });
            if (!item.Decor && !d.owned.Contains(id)) d.owned.Add(id);
            return true;
        }

        /// <summary>An item goes back to the inventory (always free).</summary>
        public static void Store(SaveData d, PlacedItem p)
        {
            if (!d.placed.Remove(p)) return;
            d.inventory.Add(p.id);
        }

        /// <summary>Path tiles are free and unlimited (§12.4); not on an item or water.</summary>
        public static bool TogglePath(SaveData d, ItemCatalog cat, LandDef land, int x, int y)
        {
            if (!land.InBounds(x, y) || land.IsWater(x, y) || !d.lands.Contains(land.Id)) return false;
            string key = PathKey(land.Id, x, y);
            if (d.paths.Remove(key)) return true;
            if (OccupiedBy(d, cat, land.Id, x, y) != null) return false;
            d.paths.Add(key);
            return true;
        }

        /// <summary>Lands open with the rank (§12.5): the start plot after level 1, then one per promotion.</summary>
        public static void OpenLands(SaveData d, int rank, bool firstLevelDone)
        {
            foreach (var l in LandCatalog.Lands)
                if (!d.lands.Contains(l.Id) && (l.Rank == 1 ? firstLevelDone : l.Rank <= rank)) d.lands.Add(l.Id);
        }
    }
}
