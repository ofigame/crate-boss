using System.Collections.Generic;

namespace CrateBoss.Meta
{
    /// <summary>The wardrobe (Ana Belge §13.2): worker pieces bought with CB Coin (100–500) and worn; slot, colour and price.</summary>
    public static class WardrobeCatalog
    {
        public sealed class Piece
        {
            public string Id, Slot, Color;
            public int Price;
            public string NameKey => "wear." + Id;
        }

        public static readonly IReadOnlyList<Piece> Pieces = new[]
        {
            new Piece { Id = "hat_white", Slot = "worker.hat", Color = "#F7F7F9", Price = 100 },
            new Piece { Id = "hat_red", Slot = "worker.hat", Color = "#EF4444", Price = 150 },
            new Piece { Id = "hat_blue", Slot = "worker.hat", Color = "#5AA9F0", Price = 150 },
            new Piece { Id = "hat_gold", Slot = "worker.hat", Color = "#FFD23F", Price = 500 },
            new Piece { Id = "vest_green", Slot = "worker.vest", Color = "#3DCC6B", Price = 150 },
            new Piece { Id = "vest_pink", Slot = "worker.vest", Color = "#F2558A", Price = 250 },
            new Piece { Id = "vest_purple", Slot = "worker.vest", Color = "#9B5DE5", Price = 250 },
            new Piece { Id = "gloves_black", Slot = "worker.gloves", Color = "#2A2D34", Price = 100 },
            new Piece { Id = "gloves_yellow", Slot = "worker.gloves", Color = "#FFC93C", Price = 120 },
        };

        public static Piece Get(string id)
        {
            foreach (var p in Pieces) if (p.Id == id) return p;
            return null;
        }

        /// <summary>The piece worn in a slot ("worker.hat"), or null.</summary>
        public static Piece Worn(SaveData d, string slot)
        {
            foreach (var e in d.equipped)
            {
                var kv = e.Split('=');
                if (kv.Length == 2 && kv[0] == slot) return Get(kv[1]);
            }
            return null;
        }

        /// <summary>Buys (when the balance covers it) or wears an owned piece; false when nothing changed.</summary>
        public static bool BuyOrWear(SaveData d, Wallet wallet, string id)
        {
            var p = Get(id);
            if (p == null) return false;
            if (!d.owned.Contains(id))
            {
                if (!wallet.TrySpend(p.Price)) return false;
                d.owned.Add(id);
            }
            d.equipped.RemoveAll(e => e.StartsWith(p.Slot + "="));
            d.equipped.Add(p.Slot + "=" + id);
            return true;
        }
    }
}
