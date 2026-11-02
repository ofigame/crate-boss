using UnityEngine;
using UnityEngine.UI;

namespace CrateBoss.UI
{
    /// <summary>
    /// Candy look (owner 2026-11-01, "butonlar … bu tarzda"): shades a graphic from a lighter top to a deeper bottom, keeping
    /// its own colour in the middle, so a colour change later still works. Also used on the game logo's letters.
    /// </summary>
    [RequireComponent(typeof(Graphic))]
    public class UiGradient : BaseMeshEffect
    {
        /// <summary>How far the top goes toward white (0–1).</summary>
        public float lighten = 0.35f;
        /// <summary>How dark the bottom gets (1 = the colour itself).</summary>
        public float darken = 0.8f;
        /// <summary>When set, the top and bottom colours themselves (the logo letters), not shades of the graphic's colour.</summary>
        public Color? top, bottom;

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive() || vh.currentVertCount == 0) return;
            var v = new UIVertex();
            float min = float.MaxValue, max = float.MinValue;
            for (int i = 0; i < vh.currentVertCount; i++)
            {
                vh.PopulateUIVertex(ref v, i);
                min = Mathf.Min(min, v.position.y);
                max = Mathf.Max(max, v.position.y);
            }
            float span = Mathf.Max(1e-3f, max - min);
            for (int i = 0; i < vh.currentVertCount; i++)
            {
                vh.PopulateUIVertex(ref v, i);
                float k = (v.position.y - min) / span; // 0 bottom, 1 top
                Color c = v.color;
                Color shade;
                if (top.HasValue && bottom.HasValue) shade = Color.Lerp(bottom.Value, top.Value, k);
                else
                {
                    var hi = Color.Lerp(c, Color.white, lighten);
                    var lo = new Color(c.r * darken, c.g * darken, c.b * darken, c.a);
                    shade = Color.Lerp(lo, hi, k);
                }
                shade.a = c.a;
                v.color = shade;
                vh.SetUIVertex(v, i);
            }
        }
    }
}
