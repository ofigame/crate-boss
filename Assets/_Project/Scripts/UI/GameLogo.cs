using CrateBoss.Localization;
using UnityEngine;
using UnityEngine.UI;

namespace CrateBoss.UI
{
    /// <summary>
    /// The game's title in the candy style (owner 2026-11-01): "CRATE BOSS" in fat pink-to-purple letters with a dark rim and
    /// a white glow, and the subtitle in yellow-to-orange under it, both slightly tilted up like a sticker.
    /// </summary>
    public static class GameLogo
    {
        public static RectTransform Build(Transform parent, float scale = 1f)
        {
            var root = UiFactory.Rect("GameLogo", parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            root.sizeDelta = new Vector2(1000f, 300f) * scale;
            root.localRotation = Quaternion.Euler(0f, 0f, 3f);
            Line(root, "CRATE BOSS", 150 * scale, new Vector2(0f, 0.42f), new Vector2(1f, 1f),
                new Color(1f, 0.55f, 0.95f), new Color(0.62f, 0.22f, 0.95f), new Color(0.24f, 0.08f, 0.42f), 9f * scale);
            Line(root, Loc.T("logo.sub"), 74 * scale, new Vector2(0f, 0f), new Vector2(1f, 0.44f),
                new Color(1f, 0.93f, 0.35f), new Color(1f, 0.55f, 0.12f), new Color(0.12f, 0.16f, 0.42f), 6f * scale);
            return root;
        }

        private static void Line(RectTransform root, string text, float size, Vector2 min, Vector2 max, Color top, Color bottom, Color rim, float thickness)
        {
            var rt = UiFactory.Rect("Line", root, min, max);
            var label = UiFactory.Label(rt, text, Mathf.RoundToInt(size), TextAnchor.MiddleCenter, true);
            var gradient = label.gameObject.AddComponent<UiGradient>();
            gradient.top = top;
            gradient.bottom = bottom;
            // A thick dark rim (two rings) and a soft drop below: the sticker edge.
            var inner = label.gameObject.AddComponent<Outline>();
            inner.effectColor = rim;
            inner.effectDistance = new Vector2(thickness * 0.55f, -thickness * 0.55f);
            var outer = label.gameObject.AddComponent<Outline>();
            outer.effectColor = rim;
            outer.effectDistance = new Vector2(thickness, -thickness);
            var drop = label.gameObject.AddComponent<Shadow>();
            drop.effectColor = new Color(0f, 0f, 0f, 0.35f);
            drop.effectDistance = new Vector2(0f, -thickness * 1.6f);
        }
    }
}
