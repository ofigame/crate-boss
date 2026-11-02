using CrateBoss.Core;
using CrateBoss.UI;
using UnityEngine;

namespace CrateBoss.View
{
    /// <summary>
    /// Placeholder look of the delivery area (Ana Belge §4.3). A prefab at Resources/Models/Goal_&lt;TileType&gt;
    /// replaces the built-in shape.
    /// </summary>
    public static class GoalTileBuilder
    {
        private static Color Hex(string hex) => ColorUtility.TryParseHtmlString(hex, out var c) ? c : Color.magenta;

        public static readonly Color Red = Hex("#E8493F"), Blue = Hex("#2F7FD8"), Green = Hex("#3BAA5C"), Yellow = Hex("#F2C232");

        /// <summary>Builds the piece for a goal tile on top of its floor slab. Lamp: the glowing inner pad, or null.</summary>
        public static void Build(Transform t, TileType type, out Material lamp)
        {
            lamp = null;
            if (VisualCatalog.TryInstantiate($"Goal_{type}", t) != null) return;
            if (type == TileType.StartMark)
            {
                // A: where the delivery crate starts, a white painted bay with a big "A".
                Frame(t, Hex("#F7F7F9"));
                Letter(t, "A", Hex("#5AA9F0"));
                return;
            }
            if (type == TileType.Via)
            {
                // M: the way stop, a turquoise painted bay with a big "M".
                Frame(t, Hex("#3EC1D3"));
                Letter(t, "M", Hex("#3EC1D3"));
                return;
            }
            if (type != TileType.Arrival) return;
            // The delivery area (B): a glowing amber parking bay with a check mark inside a thick toy frame.
            Frame(t, Hex("#FFC93C"));
            lamp = MaterialLibrary.Create(Hex("#FFD23F"), 0.5f);
            MaterialLibrary.SetGlow(lamp, Hex("#FFC21A") * 0.6f);
            Shapes.Rounded(t, new Vector3(0, 0.012f, 0), new Vector3(0.7f, 0.02f, 0.7f), lamp, 0.02f, "ArrivalGlow");
            Decal(t, IconFactory.Get(IconKind.Check), Color.white, 0.4f, 0.03f);
        }

        private static void Frame(Transform t, Color color)
        {
            var frame = MaterialLibrary.Shared(color, 0.5f);
            for (int s = 0; s < 4; s++)
            {
                var rot = Quaternion.Euler(0, 90 * s, 0);
                var bar = Shapes.Rounded(t, rot * new Vector3(0, 0.02f, 0.41f), new Vector3(0.9f, 0.04f, 0.1f), frame, 0.02f, "BayFrame");
                bar.transform.localRotation = rot;
            }
        }

        private static void Letter(Transform t, string text, Color color)
        {
            var go = new GameObject("Letter " + text);
            go.transform.SetParent(t, false);
            go.transform.localPosition = new Vector3(0, 0.03f, 0);
            go.transform.localRotation = Quaternion.Euler(90, 0, 0);
            var tm = go.AddComponent<TextMesh>();
            tm.text = text;
            tm.font = UiFonts.Title;
            tm.fontSize = 80;
            tm.characterSize = 0.075f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = color;
            go.GetComponent<MeshRenderer>().sharedMaterial = tm.font.material;
        }

        public static GameObject Decal(Transform t, Sprite sprite, Color color, float size, float height)
        {
            var mat = MaterialLibrary.Unlit(color, sprite.texture);
            var q = Shapes.Quad(t, new Vector3(0, height, 0), new Vector3(size, size, 1f), Quaternion.Euler(90, 0, 0), mat, "Decal");
            Shapes.NoShadows(q);
            return q;
        }

        public static void Number(Transform t, int n, Vector3 at, Color color, float size)
        {
            var go = new GameObject("Number");
            go.transform.SetParent(t, false);
            go.transform.localPosition = at;
            go.transform.localRotation = Quaternion.Euler(90, 0, 0);
            var tm = go.AddComponent<TextMesh>();
            tm.text = n.ToString();
            tm.font = UiFonts.Title;
            tm.fontSize = 80;
            tm.characterSize = size;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = color;
            go.GetComponent<MeshRenderer>().sharedMaterial = tm.font.material;
        }
    }
}
