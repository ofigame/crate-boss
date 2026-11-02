using System.Collections.Generic;

namespace CrateBoss.Meta
{
    /// <summary>The Boss's items (Ana Belge §12.3), read from Resources/Data/Items.txt (tab separated: id rank size w d kind colour rotatable).</summary>
    public sealed class ItemCatalog
    {
        private readonly Dictionary<string, ItemDef> byId = new Dictionary<string, ItemDef>();
        public IReadOnlyList<ItemDef> Items { get; }

        public ItemCatalog(string text)
        {
            var list = new List<ItemDef>();
            foreach (var raw in (text ?? "").Split('\n'))
            {
                var line = raw.Trim();
                if (line.Length == 0 || line[0] == '#') continue;
                var c = line.Split('\t');
                if (c.Length < 7 || !int.TryParse(c[1], out int rank)) continue;
                var item = new ItemDef
                {
                    Id = c[0], Rank = rank, Size = c[2], W = int.Parse(c[3]), D = int.Parse(c[4]), Kind = c[5], Color = c[6],
                    Rotatable = c.Length < 8 || c[7] != "0",
                };
                list.Add(item);
                byId[item.Id] = item;
            }
            Items = list;
        }

        public ItemDef Get(string id) => id != null && byId.TryGetValue(id, out var item) ? item : null;
    }
}
