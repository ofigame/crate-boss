using System.Collections.Generic;
using UnityEngine;

namespace CrateBoss.View
{
    /// <summary>
    /// The depot concrete (Ana Belge §13) shared by the Boss's Depot lot and every yard. Each district tints it:
    /// warm grey in the city, white in the cold store, sandy at customs. Materials are cached per district and variant.
    /// </summary>
    public static class ConcreteFloor
    {
        /// <summary>How much darker than the theme's light floor colour the concrete is (matches the depot lot).</summary>
        private const float Shade = 0.84f;

        private static readonly Dictionary<string, Material> cache = new Dictionary<string, Material>();

        public static Color Tint(WorldTheme theme)
        {
            var c = theme.FloorLight * Shade;
            // A touch of warmth so it reads as poured concrete rather than plastic.
            return new Color(c.r * 1.02f, c.g, c.b * 0.95f, 1f);
        }

        /// <summary>One yard cell: dashed edges, variant picked per cell.</summary>
        public static Material Cell(WorldTheme theme, int variant)
        {
            variant = ((variant % ConcreteTexture.Variants) + ConcreteTexture.Variants) % ConcreteTexture.Variants;
            string key = $"{theme.Id}/{variant}";
            if (cache.TryGetValue(key, out var m) && m != null) return m;
            m = MaterialLibrary.Create(Tint(theme), 0.12f);
            MaterialLibrary.SetTexture(m, ConcreteTexture.Get(variant, true));
            cache[key] = m;
            return m;
        }

        /// <summary>A large plain sheet (the depot lot), the texture repeated once per cell.</summary>
        public static Material Sheet(WorldTheme theme, int cellsX, int cellsZ)
        {
            var m = MaterialLibrary.Create(Tint(theme), 0.12f);
            MaterialLibrary.SetTexture(m, ConcreteTexture.Get(0, false));
            m.mainTextureScale = new Vector2(cellsX, cellsZ);
            if (m.HasProperty("_BaseMap")) m.SetTextureScale("_BaseMap", new Vector2(cellsX, cellsZ));
            return m;
        }
    }
}
