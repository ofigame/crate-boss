using CrateBoss.Meta;
using UnityEngine;

namespace CrateBoss.View
{
    /// <summary>
    /// Toy placeholder models of the Boss's items (Ana Belge §12.3, §19.2 priority 4: real models come later). Built in code
    /// from rounded shapes in the item's colour, filling its footprint; a prefab at Resources/Models/Item_&lt;id&gt; replaces it.
    /// The root is the footprint's centre on the ground; the front (door) faces −z before turning.
    /// </summary>
    public static class ItemShapes
    {
        private static Color Hex(string hex) => ColorUtility.TryParseHtmlString(hex, out var c) ? c : Color.magenta;
        private static Material M(Color c, float s = 0.5f) => MaterialLibrary.Shared(c, s);

        public static Transform Build(Transform parent, ItemDef item)
        {
            var root = new GameObject("Item " + item.Id).transform;
            root.SetParent(parent, false);
            if (VisualCatalog.TryInstantiate("Item_" + item.Id, root) != null) return root;
            var c = Hex(item.Color);
            float w = item.W * 0.92f, d = item.D * 0.92f;
            string id = item.Id;
            switch (item.Kind)
            {
                case "building":
                    if (id.Contains("tower") || id.Contains("skyscraper") || id.Contains("clock")) Tower(root, w, d, c, id.Contains("clock") ? 2.6f : 3.8f);
                    else if (id.Contains("windmill")) Windmill(root, c);
                    else if (id.Contains("greenhouse")) Glasshouse(root, w, d);
                    else if (id.Contains("ferris")) Ferris(root, w, c);
                    else House(root, w, d, c, item.Big ? 2 : 1);
                    break;
                case "vehicle":
                    if (item.D >= 2) ObstacleBuilder.ToyCar(root, Vector3.zero, true, M(c), id.GetHashCode());
                    else Scooter(root, c);
                    break;
                case "water": Boat(root, w, d, c); break;
                case "shore": Pier(root, w, d); break;
                case "ground": Ground(root, w, d, c, id); break;
                default: Garden(root, w, d, c, id); break;
            }
            return root;
        }

        static void House(Transform t, float w, float d, Color wall, int floors)
        {
            float h = 0.9f + floors * 0.75f;
            Shapes.Rounded(t, new Vector3(0, h / 2f, 0), new Vector3(w, h, d), M(wall), 0.12f, "Walls");
            var roof = M(Hex("#EF4444"));
            for (int s = -1; s <= 1; s += 2)
            {
                var r = Shapes.Rounded(t, new Vector3(0, h + 0.28f, s * d * 0.25f), new Vector3(w + 0.1f, 0.14f, d * 0.58f), roof, 0.05f, "Roof");
                r.transform.localRotation = Quaternion.Euler(s * -30f, 0, 0);
            }
            Shapes.Rounded(t, new Vector3(0, 0.42f, -d / 2f - 0.02f), new Vector3(0.42f, 0.84f, 0.06f), M(Hex("#8A5A3B")), 0.04f, "Door");
            var glass = M(Hex("#9FD8F5"), 0.8f);
            for (int i = -1; i <= 1; i += 2) Shapes.Rounded(t, new Vector3(i * w * 0.3f, h * 0.6f, -d / 2f - 0.02f), new Vector3(0.36f, 0.36f, 0.05f), glass, 0.03f, "Window");
        }

        static void Tower(Transform t, float w, float d, Color c, float h)
        {
            Shapes.Rounded(t, new Vector3(0, h / 2f, 0), new Vector3(w * 0.8f, h, d * 0.8f), M(c), 0.14f, "Tower");
            var glass = M(Hex("#9FD8F5"), 0.8f);
            for (int f = 0; f < (int)(h / 0.6f); f++) Shapes.Rounded(t, new Vector3(0, 0.4f + f * 0.6f, -d * 0.4f - 0.02f), new Vector3(w * 0.6f, 0.3f, 0.05f), glass, 0.03f, "Windows");
            Shapes.Rounded(t, new Vector3(0, h + 0.1f, 0), new Vector3(w * 0.85f, 0.2f, d * 0.85f), M(Hex("#FFD23F")), 0.06f, "Top");
        }

        static void Windmill(Transform t, Color c)
        {
            Shapes.Make(PrimitiveType.Cylinder, t, new Vector3(0, 0.8f, 0), new Vector3(0.6f, 0.8f, 0.6f), M(c), "Mill");
            for (int i = 0; i < 4; i++)
            {
                var blade = Shapes.Rounded(t, new Vector3(0, 1.4f, -0.34f), new Vector3(0.12f, 0.9f, 0.04f), M(Hex("#C98A5A")), 0.03f, "Blade");
                blade.transform.localRotation = Quaternion.Euler(0, 0, 45 + 90 * i);
            }
        }

        static void Glasshouse(Transform t, float w, float d)
        {
            Shapes.Rounded(t, new Vector3(0, 0.55f, 0), new Vector3(w, 1.1f, d), MaterialLibrary.Shared(new Color(0.75f, 0.92f, 0.82f), 0.85f), 0.1f, "Glass");
            Shapes.Rounded(t, new Vector3(0, 0.15f, 0), new Vector3(w * 0.8f, 0.3f, d * 0.8f), M(Hex("#4CB86A")), 0.06f, "Plants");
        }

        static void Ferris(Transform t, float w, Color c)
        {
            var wheel = Shapes.Make(PrimitiveType.Cylinder, t, new Vector3(0, 1.5f, 0), new Vector3(2.4f, 0.08f, 2.4f), M(c), "Wheel");
            wheel.transform.localRotation = Quaternion.Euler(90, 0, 0);
            Shapes.Rounded(t, new Vector3(0, 0.75f, 0), new Vector3(0.2f, 1.5f, 0.2f), M(Hex("#4E5462")), 0.05f, "Stand");
        }

        static void Scooter(Transform t, Color c)
        {
            Shapes.Rounded(t, new Vector3(0, 0.22f, 0), new Vector3(0.24f, 0.2f, 0.6f), M(c), 0.06f, "Body");
            Shapes.Rounded(t, new Vector3(0, 0.45f, 0.24f), new Vector3(0.36f, 0.06f, 0.06f), M(Hex("#4E5462")), 0.02f, "Bar");
            for (int s = -1; s <= 1; s += 2)
                Shapes.Make(PrimitiveType.Cylinder, t, new Vector3(0, 0.1f, s * 0.24f), new Vector3(0.18f, 0.04f, 0.18f), M(Hex("#222428")), "Wheel").transform.localRotation = Quaternion.Euler(0, 0, 90);
        }

        static void Boat(Transform t, float w, float d, Color c)
        {
            Shapes.Rounded(t, new Vector3(0, 0.18f, 0), new Vector3(w * 0.8f, 0.36f, d * 0.9f), M(c), 0.18f, "Hull");
            Shapes.Rounded(t, new Vector3(0, 0.5f, d * 0.1f), new Vector3(w * 0.5f, 0.3f, d * 0.4f), M(Hex("#5AA9F0")), 0.08f, "Cabin");
        }

        static void Pier(Transform t, float w, float d)
        {
            var wood = M(Hex("#C98A5A"), 0.3f);
            Shapes.Rounded(t, new Vector3(0, 0.12f, 0), new Vector3(w, 0.08f, d), wood, 0.03f, "Deck");
            for (int i = 0; i < 4; i++) Shapes.Make(PrimitiveType.Cylinder, t, new Vector3((i % 2 - 0.5f) * w * 0.8f, 0.05f, (i / 2 - 0.5f) * d * 0.8f), new Vector3(0.12f, 0.1f, 0.12f), wood, "Post");
        }

        static void Ground(Transform t, float w, float d, Color c, string id)
        {
            bool water = id.Contains("pool") || id.Contains("fountain");
            Shapes.Rounded(t, new Vector3(0, 0.06f, 0), new Vector3(w, 0.12f, d), M(water ? Hex("#F7F7F9") : Hex("#D6D9DE")), 0.06f, "Edge");
            Shapes.Rounded(t, new Vector3(0, 0.1f, 0), new Vector3(w * 0.84f, 0.06f, d * 0.84f), MaterialLibrary.Shared(c, water ? 0.9f : 0.4f), 0.04f, "Surface");
            if (id.Contains("fountain")) Shapes.Make(PrimitiveType.Cylinder, t, new Vector3(0, 0.4f, 0), new Vector3(0.3f, 0.35f, 0.3f), M(Hex("#C9CDD2")), "Spout");
            if (id.Contains("heli") || id.Contains("jet")) Shapes.Rounded(t, new Vector3(0, 0.4f, 0), new Vector3(w * 0.4f, 0.4f, d * 0.6f), M(Hex("#F7F7F9")), 0.15f, "Craft");
            if (id.Contains("play") || id.Contains("tramp")) Shapes.Rounded(t, new Vector3(0, 0.5f, 0), new Vector3(w * 0.3f, 0.8f, 0.2f), M(Hex("#FFD23F")), 0.06f, "Frame");
        }

        static void Garden(Transform t, float w, float d, Color c, string id)
        {
            if (id.Contains("tree") || id.Contains("palm"))
            {
                Shapes.Make(PrimitiveType.Cylinder, t, new Vector3(0, 0.35f, 0), new Vector3(0.16f, 0.35f, 0.16f), M(Hex("#8A5A3B")), "Trunk");
                Shapes.Make(PrimitiveType.Sphere, t, new Vector3(0, 0.95f, 0), new Vector3(0.8f, 0.75f, 0.8f) * (id.Contains("house") ? 1.6f : 1f), M(c), "Crown");
                if (id.Contains("house")) Shapes.Rounded(t, new Vector3(0, 1.3f, 0), new Vector3(0.8f, 0.5f, 0.8f), M(Hex("#C98A5A")), 0.06f, "Hut");
                return;
            }
            if (id.Contains("lamp")) { Shapes.Rounded(t, new Vector3(0, 0.7f, 0), new Vector3(0.1f, 1.4f, 0.1f), M(c), 0.04f, "Pole"); Shapes.Make(PrimitiveType.Sphere, t, new Vector3(0, 1.45f, 0), Vector3.one * 0.26f, M(Hex("#FFE7A8"), 0.8f), "Light"); return; }
            if (id.Contains("fence")) { for (int i = 0; i < 3; i++) Shapes.Rounded(t, new Vector3((i - 1) * w * 0.4f, 0.3f, 0), new Vector3(0.1f, 0.6f, 0.1f), M(c), 0.03f, "Post"); Shapes.Rounded(t, new Vector3(0, 0.4f, 0), new Vector3(w, 0.1f, 0.06f), M(c), 0.03f, "Rail"); return; }
            if (id.Contains("statue")) { Shapes.Rounded(t, new Vector3(0, 0.3f, 0), new Vector3(w * 0.7f, 0.6f, d * 0.7f), M(Hex("#C9CDD2")), 0.06f, "Plinth"); Shapes.Rounded(t, new Vector3(0, 0.95f, 0), new Vector3(w * 0.5f, w * 0.5f, w * 0.5f), MaterialLibrary.Shared(c, 0.85f), 0.1f, "GoldCrate"); return; }
            // Benches, planters, mailboxes, small things: a chunky block in the item colour on a little base.
            Shapes.Rounded(t, new Vector3(0, 0.05f, 0), new Vector3(w * 0.8f, 0.1f, d * 0.8f), M(Hex("#D6D9DE")), 0.04f, "Base");
            Shapes.Rounded(t, new Vector3(0, 0.32f, 0), new Vector3(w * 0.6f, 0.45f, d * 0.6f), M(c), 0.1f, "Body");
            if (id.Contains("flower") || id.Contains("planter")) for (int i = 0; i < 3; i++) Shapes.Make(PrimitiveType.Sphere, t, new Vector3((i - 1) * 0.18f, 0.62f, 0), Vector3.one * 0.2f, M(Hex(i == 1 ? "#FFD23F" : "#F2558A")), "Bloom");
        }
    }
}
