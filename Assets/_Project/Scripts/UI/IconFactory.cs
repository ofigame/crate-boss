using System.Collections.Generic;
using UnityEngine;

namespace CrateBoss.UI
{
    /// <summary>
    /// White icons drawn in code (Ana Belge §2: icons instead of words, less to translate),
    /// plus the rounded button and card shapes as 9-sliced sprites.
    /// </summary>
    public static class IconFactory
    {
        private const int Size = 128;
        private static readonly Dictionary<IconKind, Sprite> icons = new Dictionary<IconKind, Sprite>();
        private static Sprite rounded, circle;

        public static Sprite Get(IconKind kind)
        {
            if (icons.TryGetValue(kind, out var s) && s != null) return s;
            s = Draw(kind);
            icons[kind] = s;
            return s;
        }

        /// <summary>Rounded rectangle, 9-sliced so any button or card size keeps round corners.</summary>
        public static Sprite Rounded
        {
            get
            {
                if (rounded != null) return rounded;
                var c = new SdfCanvas(96);
                c.Fill((x, y) => SdfCanvas.RoundRect(x, y, 0.5f, 0.5f, 0.49f, 0.49f, 0.33f), Color.white);
                rounded = c.ToSprite("Rounded", new Vector4(36, 36, 36, 36));
                return rounded;
            }
        }

        /// <summary>
        /// A toy hammer in its own colours (owner 2026-11-01 "oyuncak gibi"): a red head with silver caps on a wooden handle,
        /// a dark outline and soft highlights; shown untinted (white image colour).
        /// </summary>
        public static Sprite ToyHammer
        {
            get
            {
                if (toyHammer != null) return toyHammer;
                var c = new SdfCanvas(192);
                var ink = new Color(0.23f, 0.13f, 0.08f);
                Vector2 h0 = new Vector2(0.24f, 0.12f), h1 = new Vector2(0.56f, 0.6f);
                Vector2 a = new Vector2(0.34f, 0.8f), b = new Vector2(0.86f, 0.5f);
                float Seg(float x, float y, Vector2 p, Vector2 q, float r) => SdfCanvas.Segment(x, y, p.x, p.y, q.x, q.y, r);
                // Outline first, a little wider than every part.
                c.Fill((x, y) => Seg(x, y, h0, h1, 0.085f), ink);
                c.Fill((x, y) => Seg(x, y, a, b, 0.155f), ink);
                // Handle: wood, a darker knob at the end, a light streak.
                c.Fill((x, y) => Seg(x, y, h0, h1, 0.06f), new Color(0.86f, 0.58f, 0.3f));
                c.Fill((x, y) => SdfCanvas.Circle(x, y, h0.x, h0.y, 0.075f), new Color(0.6f, 0.36f, 0.18f));
                c.Fill((x, y) => Seg(x, y, h0 + new Vector2(-0.015f, 0.03f), h1 + new Vector2(-0.015f, 0.01f), 0.016f), new Color(1f, 0.85f, 0.6f, 0.8f));
                // Head: silver caps at both ends, a red middle, a white shine along its top.
                c.Fill((x, y) => Seg(x, y, a, b, 0.13f), new Color(0.82f, 0.85f, 0.9f));
                var m0 = Vector2.Lerp(a, b, 0.22f);
                var m1 = Vector2.Lerp(a, b, 0.78f);
                c.Fill((x, y) => Seg(x, y, m0, m1, 0.118f), new Color(0.93f, 0.27f, 0.22f));
                c.Fill((x, y) => Seg(x, y, m0 + new Vector2(0.02f, 0.06f), m1 + new Vector2(0.02f, 0.06f), 0.03f), new Color(1f, 1f, 1f, 0.55f));
                c.Fill((x, y) => SdfCanvas.Circle(x, y, a.x - 0.03f, a.y + 0.06f, 0.025f), new Color(1f, 1f, 1f, 0.7f));
                toyHammer = c.ToSprite("ToyHammer");
                return toyHammer;
            }
        }

        private static Sprite toyHammer;

        /// <summary>
        /// A small frosted cloud (owner 2026-11-01: the hit numbers over crates): a white rim round icy blue glass with a shine,
        /// drawn in its own colours.
        /// </summary>
        public static Sprite FrostCloud
        {
            get
            {
                if (frostCloud != null) return frostCloud;
                var c = new SdfCanvas(160);
                float Cloud(float x, float y)
                {
                    float d = SdfCanvas.RoundRect(x, y, 0.5f, 0.38f, 0.38f, 0.17f, 0.16f);
                    d = Mathf.Min(d, SdfCanvas.Circle(x, y, 0.33f, 0.5f, 0.19f));
                    d = Mathf.Min(d, SdfCanvas.Circle(x, y, 0.53f, 0.6f, 0.23f));
                    d = Mathf.Min(d, SdfCanvas.Circle(x, y, 0.71f, 0.49f, 0.17f));
                    return d;
                }
                c.Fill((x, y) => Cloud(x, y) - 0.03f, new Color(1f, 1f, 1f, 0.95f));
                c.Fill((x, y) => Cloud(x, y) + 0.012f, new Color(0.7f, 0.86f, 1f, 0.78f));
                c.Fill((x, y) => SdfCanvas.Circle(x, y, 0.4f, 0.66f, 0.07f), new Color(1f, 1f, 1f, 0.6f));
                c.Fill((x, y) => SdfCanvas.Circle(x, y, 0.28f, 0.55f, 0.035f), new Color(1f, 1f, 1f, 0.5f));
                frostCloud = c.ToSprite("FrostCloud");
                return frostCloud;
            }
        }

        private static Sprite frostCloud;

        /// <summary>A fat yellow arrow pointing down with a dark rim (owner 2026-11-02: it bobs over the cargo crates).</summary>
        public static Sprite DownArrow
        {
            get
            {
                if (downArrow != null) return downArrow;
                var c = new SdfCanvas(128);
                var shape = new[] { new Vector2(0.38f, 0.92f), new Vector2(0.62f, 0.92f), new Vector2(0.62f, 0.5f), new Vector2(0.82f, 0.5f), new Vector2(0.5f, 0.1f), new Vector2(0.18f, 0.5f), new Vector2(0.38f, 0.5f) };
                c.Fill((x, y) => SdfCanvas.Polygon(x, y, shape) - 0.045f, new Color(0.35f, 0.2f, 0.05f));
                c.Fill((x, y) => SdfCanvas.Polygon(x, y, shape), new Color(1f, 0.82f, 0.15f));
                c.Fill((x, y) => SdfCanvas.Polygon(x, y, new[] { new Vector2(0.43f, 0.86f), new Vector2(0.5f, 0.86f), new Vector2(0.5f, 0.55f), new Vector2(0.43f, 0.55f) }), new Color(1f, 1f, 1f, 0.55f));
                downArrow = c.ToSprite("DownArrow");
                return downArrow;
            }
        }

        private static Sprite downArrow;

        public static Sprite Circle
        {
            get
            {
                if (circle != null) return circle;
                var c = new SdfCanvas(128);
                c.Fill((x, y) => SdfCanvas.Circle(x, y, 0.5f, 0.5f, 0.48f), Color.white);
                circle = c.ToSprite("Circle");
                return circle;
            }
        }

        private static Sprite Draw(IconKind kind)
        {
            var c = new SdfCanvas(Size);
            var w = Color.white;
            const float t = 0.055f; // stroke half-width
            switch (kind)
            {
                case IconKind.Undo:
                    c.Fill((x, y) => SdfCanvas.Arc(x, y, 0.55f, 0.45f, 0.24f, t, -90f, 160f), w);
                    c.Fill((x, y) => SdfCanvas.Polygon(x, y, new[] { new Vector2(0.14f, 0.56f), new Vector2(0.42f, 0.70f), new Vector2(0.36f, 0.40f) }), w);
                    break;
                case IconKind.Restart:
                    c.Fill((x, y) => SdfCanvas.Arc(x, y, 0.5f, 0.48f, 0.27f, t, 120f, 60f), w);
                    c.Fill((x, y) => SdfCanvas.Polygon(x, y, new[] { new Vector2(0.56f, 0.88f), new Vector2(0.80f, 0.70f), new Vector2(0.54f, 0.58f) }), w);
                    break;
                case IconKind.Levels:
                    for (int i = 0; i < 4; i++)
                    {
                        float cx = i % 2 == 0 ? 0.32f : 0.68f, cy = i < 2 ? 0.68f : 0.32f;
                        c.Fill((x, y) => SdfCanvas.RoundRect(x, y, cx, cy, 0.14f, 0.14f, 0.05f), w);
                    }
                    break;
                case IconKind.Hint:
                    c.Fill((x, y) => SdfCanvas.Circle(x, y, 0.5f, 0.6f, 0.22f), w);
                    c.Fill((x, y) => SdfCanvas.RoundRect(x, y, 0.5f, 0.33f, 0.11f, 0.08f, 0.03f), w);
                    c.Fill((x, y) => SdfCanvas.RoundRect(x, y, 0.5f, 0.2f, 0.08f, 0.035f, 0.03f), w);
                    c.Erase((x, y) => SdfCanvas.Arc(x, y, 0.5f, 0.6f, 0.12f, 0.025f, 100f, 170f));
                    break;
                case IconKind.Next:
                    c.Fill((x, y) => SdfCanvas.Polygon(x, y, new[] { new Vector2(0.32f, 0.2f), new Vector2(0.32f, 0.8f), new Vector2(0.8f, 0.5f) }) - 0.03f, w);
                    break;
                case IconKind.Close:
                    c.Fill((x, y) => Mathf.Min(SdfCanvas.Segment(x, y, 0.27f, 0.27f, 0.73f, 0.73f, t), SdfCanvas.Segment(x, y, 0.27f, 0.73f, 0.73f, 0.27f, t)), w);
                    break;
                case IconKind.Star:
                    c.Fill((x, y) => SdfCanvas.Polygon(x, y, SdfCanvas.Star(0.5f, 0.48f, 0.44f, 0.2f)) - 0.02f, w);
                    break;
                case IconKind.StarEmpty:
                    c.Fill((x, y) => Mathf.Abs(SdfCanvas.Polygon(x, y, SdfCanvas.Star(0.5f, 0.48f, 0.41f, 0.18f))) - 0.03f, w);
                    break;
                case IconKind.Lock:
                    c.Fill((x, y) => SdfCanvas.RoundRect(x, y, 0.5f, 0.36f, 0.24f, 0.19f, 0.05f), w);
                    c.Fill((x, y) => SdfCanvas.Arc(x, y, 0.5f, 0.55f, 0.15f, 0.045f, 0f, 180f), w);
                    c.Fill((x, y) => Mathf.Max(SdfCanvas.Segment(x, y, 0.35f, 0.55f, 0.35f, 0.5f, 0.045f), -1f), w);
                    c.Fill((x, y) => SdfCanvas.Segment(x, y, 0.65f, 0.55f, 0.65f, 0.5f, 0.045f), w);
                    c.Erase((x, y) => SdfCanvas.Circle(x, y, 0.5f, 0.38f, 0.05f));
                    break;
                case IconKind.Sound:
                case IconKind.SoundOff:
                    c.Fill((x, y) => SdfCanvas.Polygon(x, y, new[] { new Vector2(0.16f, 0.4f), new Vector2(0.16f, 0.6f), new Vector2(0.3f, 0.6f), new Vector2(0.48f, 0.76f), new Vector2(0.48f, 0.24f), new Vector2(0.3f, 0.4f) }), w);
                    if (kind == IconKind.Sound)
                    {
                        c.Fill((x, y) => SdfCanvas.Arc(x, y, 0.48f, 0.5f, 0.14f, 0.035f, -50f, 50f), w);
                        c.Fill((x, y) => SdfCanvas.Arc(x, y, 0.48f, 0.5f, 0.27f, 0.035f, -50f, 50f), w);
                    }
                    else
                    {
                        c.Fill((x, y) => Mathf.Min(SdfCanvas.Segment(x, y, 0.58f, 0.38f, 0.82f, 0.62f, 0.04f), SdfCanvas.Segment(x, y, 0.58f, 0.62f, 0.82f, 0.38f, 0.04f)), w);
                    }
                    break;
                case IconKind.Paint:
                    // A paint gun: tank on top, body, nozzle to the right, grip below, drops flying from the nozzle.
                    c.Fill((x, y) => SdfCanvas.RoundRect(x, y, 0.4f, 0.52f, 0.2f, 0.1f, 0.05f), w);
                    c.Fill((x, y) => SdfCanvas.RoundRect(x, y, 0.4f, 0.72f, 0.1f, 0.09f, 0.04f), w);
                    c.Fill((x, y) => SdfCanvas.RoundRect(x, y, 0.66f, 0.54f, 0.08f, 0.035f, 0.02f), w);
                    c.Fill((x, y) => SdfCanvas.Segment(x, y, 0.32f, 0.46f, 0.26f, 0.22f, 0.06f), w);
                    c.Fill((x, y) => SdfCanvas.Circle(x, y, 0.83f, 0.6f, 0.045f), w);
                    c.Fill((x, y) => SdfCanvas.Circle(x, y, 0.88f, 0.48f, 0.035f), w);
                    c.Fill((x, y) => SdfCanvas.Circle(x, y, 0.9f, 0.7f, 0.03f), w);
                    break;
                case IconKind.Hammer:
                    // Handle on a slant, a heavy head across its top.
                    c.Fill((x, y) => SdfCanvas.Segment(x, y, 0.3f, 0.14f, 0.6f, 0.62f, 0.06f), w);
                    c.Fill((x, y) => SdfCanvas.Segment(x, y, 0.42f, 0.84f, 0.84f, 0.58f, 0.13f), w);
                    break;
                case IconKind.Settings:
                    // A gear: eight teeth round a ring with a hole.
                    for (int i = 0; i < 8; i++)
                    {
                        float a = i * Mathf.PI / 4f;
                        float cx = Mathf.Cos(a), cy = Mathf.Sin(a);
                        c.Fill((x, y) => SdfCanvas.Segment(x, y, 0.5f + cx * 0.3f, 0.5f + cy * 0.3f, 0.5f + cx * 0.4f, 0.5f + cy * 0.4f, 0.075f), w);
                    }
                    c.Fill((x, y) => SdfCanvas.Circle(x, y, 0.5f, 0.5f, 0.32f), w);
                    c.Erase((x, y) => SdfCanvas.Circle(x, y, 0.5f, 0.5f, 0.13f));
                    break;
                case IconKind.Quality:
                    c.Fill((x, y) => SdfCanvas.Polygon(x, y, SdfCanvas.Star(0.5f, 0.5f, 0.42f, 0.13f, 4)), w);
                    c.Fill((x, y) => SdfCanvas.Polygon(x, y, SdfCanvas.Star(0.8f, 0.8f, 0.14f, 0.05f, 4)), w);
                    break;
                case IconKind.Crane:
                    // Mast, jib with counter-jib, rope and a hanging crate.
                    c.Fill((x, y) => SdfCanvas.Segment(x, y, 0.3f, 0.1f, 0.3f, 0.82f, 0.05f), w);
                    c.Fill((x, y) => SdfCanvas.Segment(x, y, 0.12f, 0.78f, 0.9f, 0.78f, 0.045f), w);
                    c.Fill((x, y) => SdfCanvas.Segment(x, y, 0.3f, 0.92f, 0.62f, 0.8f, 0.025f), w);
                    c.Fill((x, y) => SdfCanvas.Segment(x, y, 0.74f, 0.78f, 0.74f, 0.5f, 0.022f), w);
                    c.Fill((x, y) => SdfCanvas.RoundRect(x, y, 0.74f, 0.37f, 0.13f, 0.12f, 0.03f), w);
                    c.Fill((x, y) => SdfCanvas.RoundRect(x, y, 0.3f, 0.08f, 0.16f, 0.04f, 0.02f), w);
                    break;
                case IconKind.Heart:
                    c.Fill((x, y) => SdfCanvas.Circle(x, y, 0.35f, 0.62f, 0.19f), w);
                    c.Fill((x, y) => SdfCanvas.Circle(x, y, 0.65f, 0.62f, 0.19f), w);
                    c.Fill((x, y) => SdfCanvas.Polygon(x, y, new[] { new Vector2(0.17f, 0.56f), new Vector2(0.83f, 0.56f), new Vector2(0.5f, 0.14f) }), w);
                    break;
                case IconKind.Exclaim:
                    c.Fill((x, y) => SdfCanvas.Segment(x, y, 0.5f, 0.38f, 0.5f, 0.84f, 0.085f), w);
                    c.Fill((x, y) => SdfCanvas.Circle(x, y, 0.5f, 0.16f, 0.085f), w);
                    break;
                case IconKind.Question:
                    c.Fill((x, y) => SdfCanvas.Arc(x, y, 0.5f, 0.66f, 0.19f, 0.065f, -90f, 180f), w);
                    c.Fill((x, y) => SdfCanvas.Segment(x, y, 0.5f, 0.47f, 0.5f, 0.34f, 0.065f), w);
                    c.Fill((x, y) => SdfCanvas.Circle(x, y, 0.5f, 0.15f, 0.075f), w);
                    break;
                case IconKind.Truck:
                    c.Fill((x, y) => SdfCanvas.RoundRect(x, y, 0.38f, 0.5f, 0.28f, 0.2f, 0.03f), w);
                    c.Fill((x, y) => SdfCanvas.RoundRect(x, y, 0.8f, 0.44f, 0.12f, 0.14f, 0.04f), w);
                    c.Fill((x, y) => SdfCanvas.Circle(x, y, 0.28f, 0.22f, 0.09f), w);
                    c.Fill((x, y) => SdfCanvas.Circle(x, y, 0.74f, 0.22f, 0.09f), w);
                    break;
                case IconKind.Coin:
                    // CB Coin: a gold coin with a crate stamp on it.
                    c.Fill((x, y) => Mathf.Abs(SdfCanvas.Circle(x, y, 0.5f, 0.5f, 0.4f)) - 0.05f, w);
                    c.Fill((x, y) => SdfCanvas.RoundRect(x, y, 0.5f, 0.5f, 0.17f, 0.17f, 0.03f), w);
                    break;
                case IconKind.Calendar:
                    c.Fill((x, y) => Mathf.Abs(SdfCanvas.RoundRect(x, y, 0.5f, 0.44f, 0.34f, 0.3f, 0.05f)) - 0.04f, w);
                    c.Fill((x, y) => SdfCanvas.RoundRect(x, y, 0.5f, 0.66f, 0.34f, 0.08f, 0.03f), w);
                    c.Fill((x, y) => SdfCanvas.Segment(x, y, 0.35f, 0.72f, 0.35f, 0.86f, 0.035f), w);
                    c.Fill((x, y) => SdfCanvas.Segment(x, y, 0.65f, 0.72f, 0.65f, 0.86f, 0.035f), w);
                    c.Fill((x, y) => SdfCanvas.RoundRect(x, y, 0.5f, 0.38f, 0.08f, 0.08f, 0.02f), w);
                    break;
                case IconKind.Map:
                    c.Fill((x, y) => SdfCanvas.Polygon(x, y, new[] { new Vector2(0.14f, 0.24f), new Vector2(0.36f, 0.16f), new Vector2(0.36f, 0.76f), new Vector2(0.14f, 0.84f) }), w);
                    c.Fill((x, y) => SdfCanvas.Polygon(x, y, new[] { new Vector2(0.4f, 0.16f), new Vector2(0.6f, 0.24f), new Vector2(0.6f, 0.84f), new Vector2(0.4f, 0.76f) }), w);
                    c.Fill((x, y) => SdfCanvas.Polygon(x, y, new[] { new Vector2(0.64f, 0.24f), new Vector2(0.86f, 0.16f), new Vector2(0.86f, 0.76f), new Vector2(0.64f, 0.84f) }), w);
                    break;
                case IconKind.Ad:
                    c.Fill((x, y) => Mathf.Abs(SdfCanvas.RoundRect(x, y, 0.5f, 0.52f, 0.38f, 0.28f, 0.06f)) - 0.04f, w);
                    c.Fill((x, y) => SdfCanvas.Polygon(x, y, new[] { new Vector2(0.42f, 0.38f), new Vector2(0.64f, 0.52f), new Vector2(0.42f, 0.66f) }), w);
                    break;
                case IconKind.Check:
                    c.Fill((x, y) => Mathf.Min(SdfCanvas.Segment(x, y, 0.2f, 0.5f, 0.42f, 0.28f, 0.07f), SdfCanvas.Segment(x, y, 0.42f, 0.28f, 0.82f, 0.72f, 0.07f)), w);
                    break;
                case IconKind.Snowflake:
                    // Six arms, each with two pairs of side twigs (the ice crate's mark).
                    for (int a = 0; a < 6; a++)
                    {
                        float ang = a * Mathf.PI / 3f;
                        var dir = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));
                        var tip = new Vector2(0.5f, 0.5f) + dir * 0.4f;
                        c.Fill((x, y) => SdfCanvas.Segment(x, y, 0.5f, 0.5f, tip.x, tip.y, 0.04f), w);
                        foreach (float along in new[] { 0.2f, 0.3f })
                        {
                            var root = new Vector2(0.5f, 0.5f) + dir * along;
                            float twig = along < 0.25f ? 0.11f : 0.08f;
                            foreach (float side in new[] { -1f, 1f })
                            {
                                float b = ang + side * Mathf.PI / 4f;
                                var end = root + new Vector2(Mathf.Cos(b), Mathf.Sin(b)) * twig;
                                c.Fill((x, y) => SdfCanvas.Segment(x, y, root.x, root.y, end.x, end.y, 0.032f), w);
                            }
                        }
                    }
                    c.Fill((x, y) => SdfCanvas.Circle(x, y, 0.5f, 0.5f, 0.07f), w);
                    break;
                case IconKind.Hand:
                    // A pointing hand, finger up: palm, the index finger, two folded knuckles and the thumb.
                    c.Fill((x, y) => SdfCanvas.RoundRect(x, y, 0.53f, 0.34f, 0.2f, 0.2f, 0.08f), w);
                    c.Fill((x, y) => SdfCanvas.Segment(x, y, 0.42f, 0.42f, 0.42f, 0.86f, 0.075f), w);
                    c.Fill((x, y) => SdfCanvas.Segment(x, y, 0.56f, 0.46f, 0.56f, 0.58f, 0.07f), w);
                    c.Fill((x, y) => SdfCanvas.Segment(x, y, 0.68f, 0.44f, 0.68f, 0.53f, 0.065f), w);
                    c.Fill((x, y) => SdfCanvas.Segment(x, y, 0.34f, 0.3f, 0.24f, 0.46f, 0.065f), w);
                    break;
                case IconKind.Pause:
                    // Two bars: a crane held up ("kilitli").
                    c.Fill((x, y) => SdfCanvas.RoundRect(x, y, 0.34f, 0.5f, 0.1f, 0.32f, 0.04f), w);
                    c.Fill((x, y) => SdfCanvas.RoundRect(x, y, 0.66f, 0.5f, 0.1f, 0.32f, 0.04f), w);
                    break;
            }
            return c.ToSprite("Icon_" + kind);
        }
    }
}
