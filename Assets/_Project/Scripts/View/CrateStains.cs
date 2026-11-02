using System.Collections.Generic;
using UnityEngine;

namespace CrateBoss.View
{
    /// <summary>
    /// Black stains on the fixed crates (owner 2026-11-01: "sabit kasa ile boşluk açma kasaları çok benziyor, sabit kasalara
    /// siyah lekeler yap"). The stains are painted once into a copy of the model's texture, so they cost nothing per frame.
    /// </summary>
    public static class CrateStains
    {
        private static readonly int BaseMapId = Shader.PropertyToID("_BaseMap");
        private static readonly Dictionary<Material, Material> stained = new Dictionary<Material, Material>();
        private static Texture2D blob;

        /// <summary>Gives every renderer of a fixed crate the stained copy of its material.</summary>
        public static void Apply(GameObject go)
        {
            foreach (var r in go.GetComponentsInChildren<Renderer>(true))
            {
                var mats = r.sharedMaterials;
                for (int i = 0; i < mats.Length; i++) mats[i] = Of(mats[i]);
                r.sharedMaterials = mats;
            }
        }

        private static Material Of(Material source)
        {
            if (source == null) return null;
            if (stained.TryGetValue(source, out var m) && m != null) return m;
            m = new Material(source) { name = source.name + " (stained)" };
            var tex = source.HasProperty(BaseMapId) ? source.GetTexture(BaseMapId) : null;
            m.SetTexture(BaseMapId, Paint(tex, source.name.GetHashCode()));
            stained[source] = m;
            return m;
        }

        // The texture (or plain white) with dark soft blots drawn over it, on the GPU.
        private static Texture Paint(Texture source, int seed)
        {
            int w = source != null ? Mathf.Clamp(source.width, 256, 2048) : 256, h = source != null ? Mathf.Clamp(source.height, 256, 2048) : 256;
            var rt = new RenderTexture(w, h, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB) { name = "StainedCrate", useMipMap = true, autoGenerateMips = true };
            rt.Create();
            Graphics.Blit(source != null ? source : Texture2D.whiteTexture, rt);
            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, w, h, 0);
            var rng = new System.Random(seed);
            var ink = new Color(0.06f, 0.05f, 0.05f, 1f);
            for (int i = 0; i < 70; i++)
            {
                float size = (0.03f + (float)rng.NextDouble() * 0.07f) * Mathf.Min(w, h);
                float x = (float)rng.NextDouble() * w, y = (float)rng.NextDouble() * h;
                var c = ink;
                c.a = (0.55f + (float)rng.NextDouble() * 0.35f) * 0.5f; // DrawTexture: 0.5 is "as it is"
                Graphics.DrawTexture(new Rect(x - size / 2f, y - size / 2f, size, size * (0.6f + (float)rng.NextDouble() * 0.8f)), Blob, new Rect(0, 0, 1, 1), 0, 0, 0, 0, c);
            }
            GL.PopMatrix();
            RenderTexture.active = prev;
            return rt;
        }

        // A soft, uneven round blot (white with alpha; tinted when drawn).
        private static Texture2D Blob
        {
            get
            {
                if (blob != null) return blob;
                const int n = 64;
                blob = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, name = "StainBlob" };
                var px = new Color32[n * n];
                for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float dx = (x - n / 2f) / (n / 2f), dy = (y - n / 2f) / (n / 2f);
                    float angle = Mathf.Atan2(dy, dx);
                    float edge = 0.75f + 0.18f * Mathf.Sin(angle * 3f) + 0.08f * Mathf.Sin(angle * 7f + 1f);
                    float a = Mathf.Clamp01((edge - Mathf.Sqrt(dx * dx + dy * dy)) * 6f);
                    px[y * n + x] = new Color32(255, 255, 255, (byte)(a * 255));
                }
                blob.SetPixels32(px);
                blob.Apply();
                return blob;
            }
        }
    }
}
