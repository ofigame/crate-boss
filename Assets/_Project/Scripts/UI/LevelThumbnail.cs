using System.Collections.Generic;
using CrateBoss.Levels;
using UnityEngine;

namespace CrateBoss.UI
{
    /// <summary>
    /// A bird's-eye picture of a level's yard for the level list (owner 2026-10-31: "her bölümdeki oyun alanının kuş bakışı
    /// şekli"), drawn straight from the level file in the toy palette: the parking-lot floor with white bay lines, the yellow
    /// barrier round it, slots, holes, the delivery area, and every crate and the worker as rounded toy shapes. Cached.
    /// </summary>
    public static class LevelThumbnail
    {
        private const int Cell = 32;
        private static readonly Dictionary<string, Texture2D> cache = new Dictionary<string, Texture2D>();

        private static Color Hex(string hex) => ColorUtility.TryParseHtmlString(hex, out var c) ? c : Color.magenta;

        private static readonly Color Ground = Hex("#D6D9DE"), Floor = Hex("#ECEEF1"), Line = Hex("#FFFFFF"), Barrier = Hex("#FFC93C"),
            Slot = Hex("#3EC1D3"), Arrival = Hex("#FFD23F"), Pit = Hex("#3A3F48"), Sea = Hex("#4FA8D8"),
            Wood = Hex("#F2A24A"), WoodDark = Hex("#C9762F"), Ice = Hex("#9FE3F2"), Heavy = Hex("#4A5260"),
            Delivery = Hex("#EF4444"), Worker = Hex("#FF8C42"), Fixed = Hex("#5C6B85");

        private static readonly Color[] Obstacles = { Hex("#F2558A"), Hex("#9B5DE5"), Hex("#3EC1D3"), Hex("#5AA9F0"), Hex("#3DCC6B") };

        /// <summary>The picture's size in pixels without drawing it.</summary>
        public static Vector2Int Size(LevelData d) => new Vector2Int(d.ground[0].Length * Cell, d.ground.Length * Cell);

        public static Texture2D Get(LevelData d)
        {
            string key = d.id + ":" + d.name + ":" + (d.ground != null ? string.Join("", d.ground) : "");
            if (cache.TryGetValue(key, out var tex) && tex != null) return tex;
            tex = Draw(d);
            cache[key] = tex;
            return tex;
        }

        private static Texture2D Draw(LevelData d)
        {
            int h = d.ground.Length, w = d.ground[0].Length;
            int pw = w * Cell, ph = h * Cell;
            var px = new Color32[pw * ph];
            for (int i = 0; i < px.Length; i++) px[i] = Ground;

            char G(int x, int y) => x >= 0 && y >= 0 && y < h && x < d.ground[y].Length ? d.ground[y][x] : '#';
            bool Open(int x, int y) { char c = G(x, y); return c != '#' && c != '_'; }
            // Floor and goal tiles.
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                char c = G(x, y);
                if (c == '#') continue;
                if (c == '_') { Rect(px, pw, ph, x * Cell, y * Cell, Cell, Cell, 0, Sea); continue; }
                Rect(px, pw, ph, x * Cell, y * Cell, Cell, Cell, 0, Floor);
                Rect(px, pw, ph, x * Cell, y * Cell, Cell, 2, 0, Line);
                Rect(px, pw, ph, x * Cell, y * Cell, 2, Cell, 0, Line);
                switch (c)
                {
                    case 'T': Frame(px, pw, ph, x, y, Slot); break;
                    case 'A': Frame(px, pw, ph, x, y, Arrival); break;
                    case 'o': Rect(px, pw, ph, x * Cell + 6, y * Cell + 6, Cell - 12, Cell - 12, 6, Pit); break;
                }
            }
            // Inner walls: candy toy obstacles; walls touching the floor get the yellow barrier on that side.
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                if (G(x, y) != '#') continue;
                bool inner = Open(x - 1, y) && Open(x + 1, y) || Open(x, y - 1) && Open(x, y + 1)
                    || (Open(x - 1, y) ? 1 : 0) + (Open(x + 1, y) ? 1 : 0) + (Open(x, y - 1) ? 1 : 0) + (Open(x, y + 1) ? 1 : 0) >= 3;
                if (inner) { Crate(px, pw, ph, x * Cell, y * Cell, Fixed, Barrier); continue; } // a fixed crate (yellow corners)
                if (Open(x, y + 1)) Rect(px, pw, ph, x * Cell, (y + 1) * Cell - 5, Cell, 5, 0, Barrier);
                if (Open(x, y - 1)) Rect(px, pw, ph, x * Cell, y * Cell, Cell, 5, 0, Barrier);
                if (Open(x + 1, y)) Rect(px, pw, ph, (x + 1) * Cell - 5, y * Cell, 5, Cell, 0, Barrier);
                if (Open(x - 1, y)) Rect(px, pw, ph, x * Cell, y * Cell, 5, Cell, 0, Barrier);
            }
            // Things on the floor.
            if (d.objects != null)
                for (int y = 0; y < h && y < d.objects.Length; y++)
                for (int x = 0; x < w && x < d.objects[y].Length; x++)
                {
                    char c = d.objects[y][x];
                    int ox = x * Cell, oy = y * Cell;
                    switch (c)
                    {
                        case 'B': Crate(px, pw, ph, ox, oy, Wood, WoodDark); break;
                        case 'I': Crate(px, pw, ph, ox, oy, Ice, Hex("#6CC4DC")); break;
                        case 'H': Crate(px, pw, ph, ox, oy, Heavy, Barrier); break;
                        case 'D': Crate(px, pw, ph, ox, oy, Delivery, Color.white); break;
                        case 'P':
                            Rect(px, pw, ph, ox + 10, oy + 10, Cell - 20, Cell - 20, 12, Worker);
                            Rect(px, pw, ph, ox + 15, oy + 14, Cell - 30, Cell - 30, 7, Barrier);
                            break;
                        default:
                            if (c >= '1' && c <= '6')
                            {
                                var col = View.SymbolArt.ColorOf(c - '0');
                                Crate(px, pw, ph, ox, oy, col, col * 0.7f);
                            }
                            break;
                    }
                }
            // The texture's rows run bottom-up; the level's rows top-down.
            var flipped = new Color32[px.Length];
            for (int y = 0; y < ph; y++) System.Array.Copy(px, y * pw, flipped, (ph - 1 - y) * pw, pw);
            var tex = new Texture2D(pw, ph, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            tex.SetPixels32(flipped);
            tex.Compress(true); // 100 yards stay light on a phone (about 4x smaller)
            tex.Apply(false, true);
            return tex;
        }

        private static void Crate(Color32[] px, int pw, int ph, int ox, int oy, Color body, Color edge)
        {
            Rect(px, pw, ph, ox + 4, oy + 4, Cell - 8, Cell - 8, 8, edge);
            Rect(px, pw, ph, ox + 8, oy + 8, Cell - 16, Cell - 16, 6, body);
        }

        private static void Frame(Color32[] px, int pw, int ph, int x, int y, Color c)
        {
            int ox = x * Cell, oy = y * Cell;
            Rect(px, pw, ph, ox + 5, oy + 5, Cell - 10, Cell - 10, 6, c);
            Rect(px, pw, ph, ox + 10, oy + 10, Cell - 20, Cell - 20, 3, Floor);
        }

        // A filled rectangle with rounded corners (radius r), top-left origin.
        private static void Rect(Color32[] px, int pw, int ph, int x0, int y0, int w, int h, int r, Color color)
        {
            Color32 c = color;
            for (int y = Mathf.Max(0, y0); y < Mathf.Min(ph, y0 + h); y++)
            for (int x = Mathf.Max(0, x0); x < Mathf.Min(pw, x0 + w); x++)
            {
                if (r > 0)
                {
                    int dx = x < x0 + r ? x0 + r - x : x >= x0 + w - r ? x - (x0 + w - r - 1) : 0;
                    int dy = y < y0 + r ? y0 + r - y : y >= y0 + h - r ? y - (y0 + h - r - 1) : 0;
                    if (dx * dx + dy * dy > r * r) continue;
                }
                px[y * pw + x] = c;
            }
        }
    }
}
