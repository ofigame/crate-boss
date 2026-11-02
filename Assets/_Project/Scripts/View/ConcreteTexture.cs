using UnityEngine;

namespace CrateBoss.View
{
    /// <summary>
    /// One cell of yard concrete, made in code (grey, tinted by the material colour): soft blotches, fine grit, a few dark
    /// specks and, when dashed, a painted dash line along each edge. Each edge carries half the line's width, so two
    /// neighbouring cells together draw one even dashed line, as on a warehouse floor. Four variants break the repeat.
    /// </summary>
    public static class ConcreteTexture
    {
        public const int Variants = 4;
        private const int Size = 128;
        private const int Dashes = 4;           // per edge
        private const float DashLength = 0.15f; // of the edge
        private const float HalfLine = 2.6f;    // pixels on each side of the cell border
        private const float LineShade = 0.82f;

        private static readonly Texture2D[] plain = new Texture2D[Variants], dashed = new Texture2D[Variants];

        public static Texture2D Get(int variant, bool withDashes)
        {
            variant = ((variant % Variants) + Variants) % Variants;
            var cache = withDashes ? dashed : plain;
            if (cache[variant] != null) return cache[variant];
            var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, true)
            {
                name = $"Concrete{variant}{(withDashes ? "Dashed" : "")}",
                wrapMode = withDashes ? TextureWrapMode.Clamp : TextureWrapMode.Repeat,
                filterMode = FilterMode.Trilinear,
                anisoLevel = 4,
            };
            var rng = new System.Random(911 + variant * 37);
            float ox = (float)rng.NextDouble() * 100f, oy = (float)rng.NextDouble() * 100f;
            var pixels = new Color[Size * Size];
            for (int y = 0; y < Size; y++)
            for (int x = 0; x < Size; x++)
            {
                float u = x / (float)Size, v = y / (float)Size;
                // Large soft blotches + fine grit; tileable enough at cell size because the grit dominates the edges.
                float blot = Mathf.PerlinNoise(ox + u * 2.2f, oy + v * 2.2f) - 0.5f;
                float grit = (float)rng.NextDouble() - 0.5f;
                float g = 0.93f + blot * 0.07f + grit * 0.035f;
                if (withDashes) g *= Dash(x, y);
                pixels[y * Size + x] = new Color(g, g, g, 1f);
            }
            // Dark specks (small stones in the mix).
            for (int i = 0; i < 26; i++)
            {
                int sx = rng.Next(Size), sy = rng.Next(Size);
                float d = 0.8f + (float)rng.NextDouble() * 0.1f;
                pixels[sy * Size + sx] *= d;
                if (sx + 1 < Size) pixels[sy * Size + sx + 1] *= Mathf.Lerp(d, 1f, 0.5f);
            }
            for (int i = 0; i < pixels.Length; i++) pixels[i].a = 1f;
            tex.SetPixels(pixels);
            tex.Apply(true, false);
            cache[variant] = tex;
            return tex;
        }

        // Shade multiplier of the painted dash line near the cell's edges (1 = no paint).
        private static float Dash(int x, int y)
        {
            float shade = 1f;
            // Distance from the left/right and bottom/top borders, in pixels (pixel centres).
            float dx = Mathf.Min(x + 0.5f, Size - x - 0.5f);
            float dy = Mathf.Min(y + 0.5f, Size - y - 0.5f);
            if (dx < HalfLine + 1f && OnDash((y + 0.5f) / Size)) shade = Mathf.Min(shade, Paint(dx));
            if (dy < HalfLine + 1f && OnDash((x + 0.5f) / Size)) shade = Mathf.Min(shade, Paint(dy));
            return shade;
        }

        // Dashes centred in equal slots, so the pattern is symmetric and neighbours line up; corners stay bare.
        private static bool OnDash(float along)
        {
            float slot = along * Dashes;
            float inSlot = slot - Mathf.Floor(slot) - 0.5f;
            return Mathf.Abs(inSlot) * (1f / Dashes) < DashLength / 2f;
        }

        // Soft 1-px edge so the line does not shimmer when the camera moves.
        private static float Paint(float distance) => Mathf.Lerp(LineShade, 1f, Mathf.Clamp01(distance - HalfLine + 0.5f));
    }
}
