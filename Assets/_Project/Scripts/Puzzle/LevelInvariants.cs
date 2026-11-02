using System;
using System.Collections.Generic;
using System.Linq;
using CrateBoss.Core;
using CrateBoss.Levels;

namespace CrateBoss.Puzzle
{
    /// <summary>
    /// What a hardened level must keep from its original (owner 2026-11-01 brief, "DEĞİŞMEYECEKLER"): the size and the outer
    /// edge of the yard, every mechanic, the numbers of cranes, cars, deliveries and targets, crates within ±15 % and inner
    /// fixed crates within ±20 %. Returns what differs (Turkish), empty when the candidate keeps everything.
    /// </summary>
    public static class LevelInvariants
    {
        public static double CrateSlack = 0.15, WallSlack = 0.20;

        public static List<string> Check(LevelData original, LevelData candidate)
        {
            var problems = new List<string>();
            GameState a, b;
            try { a = new LevelParser().Parse(original); b = new LevelParser().Parse(candidate); }
            catch (LevelFormatException e) { problems.Add("okunamadı: " + e.Message); return problems; }
            if (a.Width != b.Width || a.Height != b.Height) { problems.Add("alan büyüklüğü değişti"); return problems; }
            var outerA = BoundaryMap.OuterWalls(a);
            var outerB = BoundaryMap.OuterWalls(b);
            for (int i = 0; i < outerA.Length; i++)
                if (outerA[i] != outerB[i]) { problems.Add("dış sınır değişti"); break; }
            if (Mechanics(a) != Mechanics(b)) problems.Add($"mekanik değişti ({Mechanics(a)} → {Mechanics(b)})");
            if (a.Cranes.Count != b.Cranes.Count) problems.Add("vinç sayısı değişti");
            if ((a.Parking?.Cars.Count ?? 0) != (b.Parking?.Cars.Count ?? 0)) problems.Add("araba sayısı değişti");
            if (a.Deliveries.Count != b.Deliveries.Count) problems.Add("teslim sayısı değişti");
            if (a.TargetCells.Length != b.TargetCells.Length) problems.Add("hedef sayısı değişti");
            int ca = Crates(a), cb = Crates(b);
            if (Math.Abs(cb - ca) > Math.Max(1, (int)Math.Floor(ca * CrateSlack))) problems.Add($"kasa sayısı {ca} → {cb}");
            int wa = InnerWalls(a, outerA), wb = InnerWalls(b, outerB);
            if (Math.Abs(wb - wa) > Math.Max(1, (int)Math.Floor(wa * WallSlack))) problems.Add($"iç engel sayısı {wa} → {wb}");
            return problems;
        }

        /// <summary>Which mechanics a level has, as one comparable text.</summary>
        public static string Mechanics(GameState s)
        {
            var list = new List<string>();
            if (s.Cranes.Count > 0) list.Add("vinç");
            if (s.HasSymbols) list.Add("patlatma");
            if (s.Deliveries.Count > 0) list.Add("görev");
            if (s.Parking != null) list.Add("otopark");
            if (s.Truck != null) list.Add("kamyon");
            if (s.Boxes.Any(x => x.Type == BoxType.Heavy) || s.Boxes.Any(x => x.HeavyDelivery)) list.Add("ağır");
            if (s.Boxes.Any(x => x.Type == BoxType.Ice)) list.Add("buz");
            bool hole = false, via = false;
            for (int y = 0; y < s.Height; y++)
                for (int x = 0; x < s.Width; x++)
                {
                    var t = s.GetTile(new GridPos(x, y));
                    hole |= t == TileType.Hole;
                    via |= t == TileType.Via;
                }
            if (hole) list.Add("çukur");
            if (via) list.Add("ara durak");
            return string.Join("+", list);
        }

        public static int Crates(GameState s) => s.Boxes.Count(x => x.Type != BoxType.Container);

        public static int InnerWalls(GameState s, bool[] outer)
        {
            int n = 0;
            for (int y = 0; y < s.Height; y++)
                for (int x = 0; x < s.Width; x++)
                    if (s.GetTile(new GridPos(x, y)) == TileType.Wall && !outer[y * s.Width + x]) n++;
            return n;
        }
    }
}
