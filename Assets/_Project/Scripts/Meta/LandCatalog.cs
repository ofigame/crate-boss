using System.Collections.Generic;

namespace CrateBoss.Meta
{
    /// <summary>The five lands (Ana Belge §12.5), joined by the home city's roads.</summary>
    public static class LandCatalog
    {
        public static readonly IReadOnlyList<LandDef> Lands = new[]
        {
            new LandDef { Id = "start", W = 10, D = 10, OffsetX = 0, OffsetY = 0, Rank = 1 },
            new LandDef { Id = "garden", W = 12, D = 10, OffsetX = 12, OffsetY = 0, Rank = 2 },
            new LandDef { Id = "lake", W = 12, D = 12, OffsetX = -14, OffsetY = 0, Rank = 3, Water = "pond" },
            new LandDef { Id = "shore", W = 14, D = 12, OffsetX = 0, OffsetY = -14, Rank = 4, Water = "sea" },
            new LandDef { Id = "centre", W = 14, D = 14, OffsetX = 16, OffsetY = -16, Rank = 5 },
        };

        public static LandDef Get(string id)
        {
            foreach (var l in Lands) if (l.Id == id) return l;
            return null;
        }
    }
}
