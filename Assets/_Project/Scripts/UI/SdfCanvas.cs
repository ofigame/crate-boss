using System;
using System.Collections.Generic;
using UnityEngine;

namespace CrateBoss.UI
{
    /// <summary>
    /// Tiny anti-aliased vector painter for UI icons and button shapes: every shape is a signed distance
    /// (negative inside), so edges come out smooth at any size without image files.
    /// </summary>
    public sealed class SdfCanvas
    {
        private readonly int size;
        private readonly Color[] pixels;

        public SdfCanvas(int size)
        {
            this.size = size;
            pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color(1, 1, 1, 0);
        }

        /// <summary>Coordinates are 0..1 with y up.</summary>
        public void Fill(Func<float, float, float> sdf, Color color)
        {
            float px = 1f / size;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = sdf((x + 0.5f) * px, (y + 0.5f) * px) / px;
                float a = Mathf.Clamp01(0.5f - d) * color.a;
                if (a <= 0f) continue;
                var p = pixels[y * size + x];
                float outA = a + p.a * (1f - a);
                var rgb = (new Vector3(color.r, color.g, color.b) * a + new Vector3(p.r, p.g, p.b) * p.a * (1f - a)) / Mathf.Max(outA, 1e-5f);
                pixels[y * size + x] = new Color(rgb.x, rgb.y, rgb.z, outA);
            }
        }

        public void Erase(Func<float, float, float> sdf)
        {
            float px = 1f / size;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = sdf((x + 0.5f) * px, (y + 0.5f) * px) / px;
                float keep = Mathf.Clamp01(d + 0.5f);
                var p = pixels[y * size + x];
                p.a *= keep;
                pixels[y * size + x] = p;
            }
        }

        public Sprite ToSprite(string name, Vector4 border = default)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, name = name };
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
        }

        // ---------- shapes ----------

        public static float Circle(float x, float y, float cx, float cy, float r) => Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy)) - r;

        public static float Segment(float x, float y, float ax, float ay, float bx, float by, float r)
        {
            float pax = x - ax, pay = y - ay, bax = bx - ax, bay = by - ay;
            float h = Mathf.Clamp01((pax * bax + pay * bay) / (bax * bax + bay * bay));
            float dx = pax - bax * h, dy = pay - bay * h;
            return Mathf.Sqrt(dx * dx + dy * dy) - r;
        }

        public static float RoundRect(float x, float y, float cx, float cy, float hw, float hh, float r)
        {
            float qx = Mathf.Abs(x - cx) - hw + r, qy = Mathf.Abs(y - cy) - hh + r;
            float outside = new Vector2(Mathf.Max(qx, 0), Mathf.Max(qy, 0)).magnitude;
            return outside + Mathf.Min(Mathf.Max(qx, qy), 0f) - r;
        }

        /// <summary>A ring arc from angle a0 to a1 (degrees, counter-clockwise, 0 = right).</summary>
        public static float Arc(float x, float y, float cx, float cy, float r, float thickness, float a0, float a1)
        {
            float angle = Mathf.Atan2(y - cy, x - cx) * Mathf.Rad2Deg;
            float span = Mathf.Repeat(a1 - a0, 360f);
            float rel = Mathf.Repeat(angle - a0, 360f);
            if (rel <= span) return Mathf.Abs(Circle(x, y, cx, cy, r)) - thickness;
            float e0 = a0 * Mathf.Deg2Rad, e1 = a1 * Mathf.Deg2Rad;
            float d0 = Circle(x, y, cx + Mathf.Cos(e0) * r, cy + Mathf.Sin(e0) * r, thickness);
            float d1 = Circle(x, y, cx + Mathf.Cos(e1) * r, cy + Mathf.Sin(e1) * r, thickness);
            return Mathf.Min(d0, d1);
        }

        public static float Polygon(float x, float y, IReadOnlyList<Vector2> pts)
        {
            float d = float.MaxValue;
            bool inside = false;
            for (int i = 0, j = pts.Count - 1; i < pts.Count; j = i++)
            {
                var a = pts[j];
                var b = pts[i];
                d = Mathf.Min(d, Segment(x, y, a.x, a.y, b.x, b.y, 0f));
                if ((b.y > y) != (a.y > y) && x < (a.x - b.x) * (y - b.y) / (a.y - b.y) + b.x) inside = !inside;
            }
            return inside ? -d : d;
        }

        public static Vector2[] Star(float cx, float cy, float outer, float inner, int points = 5)
        {
            var pts = new Vector2[points * 2];
            for (int i = 0; i < pts.Length; i++)
            {
                float r = i % 2 == 0 ? outer : inner;
                float a = Mathf.PI / 2f + i * Mathf.PI / points;
                pts[i] = new Vector2(cx + Mathf.Cos(a) * r, cy + Mathf.Sin(a) * r);
            }
            return pts;
        }
    }
}
