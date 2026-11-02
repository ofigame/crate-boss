using UnityEngine;

namespace CrateBoss.View
{
    /// <summary>Grey ribbed stripes used as the base map of container walls (tinted by the material colour).</summary>
    public static class CorrugatedTexture
    {
        private const int Ribs = 6, PerRib = 8;
        private static Texture2D texture;

        public static Texture2D Get()
        {
            if (texture != null) return texture;
            int w = Ribs * PerRib;
            texture = new Texture2D(w, 4, TextureFormat.RGBA32, true) { name = "Corrugated", wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Bilinear };
            for (int x = 0; x < w; x++)
            {
                // Lit ridge, shaded groove: a soft sine per rib.
                float k = Mathf.Sin((x % PerRib) / (float)PerRib * Mathf.PI * 2f);
                float v = 0.86f + 0.14f * k;
                for (int y = 0; y < 4; y++) texture.SetPixel(x, y, new Color(v, v, v, 1f));
            }
            texture.Apply();
            return texture;
        }
    }
}
