using CrateBoss.Core;
using UnityEngine;

namespace CrateBoss.View
{
    /// <summary>
    /// Builds the three box looks from bevelled blocks (Ana Belge §2): each kind differs by silhouette,
    /// material and colour, so they read apart even in black and white.
    /// The model fills a unit box above the parent's origin; the caller scales it.
    /// </summary>
    public static class BoxModelFactory
    {
        public const float NormalSize = 0.84f;
        public const float IceSize = 0.84f;
        public static readonly Vector3 HeavySize = new Vector3(0.96f, 0.76f, 0.96f);
        public static readonly Vector3 ContainerSize = new Vector3(0.92f, 0.8f, 0.92f);

        /// <summary>Box size: the real model's own size when there is one (never stretched), else the built-in size.</summary>
        public static Vector3 SizeOf(BoxType type)
        {
            if (type == BoxType.Container) return ContainerSize; // always the built fixed crate
            if (VisualCatalog.TryGetBounds(ModelKey(type), out var b)) return b.size;
            return type == BoxType.Heavy ? HeavySize
                : type == BoxType.Container ? ContainerSize
                : Vector3.one * (type == BoxType.Ice ? IceSize : NormalSize);
        }

        private static string ModelKey(BoxType type) =>
            type == BoxType.Ice ? "BoxIce" : type == BoxType.Heavy ? "BoxHeavy" : type == BoxType.Container ? "BoxContainer"
            : type == BoxType.Delivery ? "BoxDelivery" : "BoxNormal";

        /// <summary>Builds the model under <paramref name="body"/> (a unit box centred at the origin). Returns the shell material.</summary>
        public static Material Build(Transform body, BoxType type, int variant)
        {
            // Owner 2026-10-31: the crane drops a fixed crate, the same crate that stands fixed in the yard (the owner's
            // rank-1 model when it is imported, else the slate-blue placeholder).
            if (type == BoxType.Container && VisualCatalog.TryInstantiateRanked("BoxFixed", body) is GameObject fixedModel)
            {
                var s = SizeOf(type);
                fixedModel.transform.localScale = new Vector3(1f / s.x, 1f / s.y, 1f / s.z);
                fixedModel.transform.localPosition = new Vector3(0, -0.5f, 0);
                return fixedModel.GetComponentInChildren<Renderer>()?.material;
            }
            if (type == BoxType.Container) return BuildFixed(body);
            var custom = VisualCatalog.TryInstantiateRanked(ModelKey(type), body);
            if (custom != null)
            {
                // The body is scaled to SizeOf (the model's own size), so undo that: the model keeps its proportions.
                var size = SizeOf(type);
                custom.transform.localScale = new Vector3(1f / size.x, 1f / size.y, 1f / size.z);
                custom.transform.localPosition = new Vector3(0, -0.5f, 0);
                if (type == BoxType.Ice)
                {
                    // A crisp white snowflake on the top face, readable from the game camera through the clear ice.
                    var flake = GoalTileBuilder.Decal(body, UI.IconFactory.Get(UI.IconKind.Snowflake), new Color(1f, 1f, 1f, 0.95f), 0.62f, 0.505f);
                    flake.name = "Snowflake";
                }
                return custom.GetComponentInChildren<Renderer>()?.material;
            }
            switch (type)
            {
                case BoxType.Ice: return BuildIce(body);
                case BoxType.Heavy: return BuildHeavy(body);
                case BoxType.Container: return BuildContainer(body);
                case BoxType.Delivery: return BuildDelivery(body);
                default: return BuildWood(body, variant);
            }
        }

        // Delivery crate (Ana Belge §4.2): a chunky red toy crate with a big white up arrow on the lid and on the front.
        private static Material BuildDelivery(Transform body)
        {
            var shell = MaterialLibrary.Create(new Color(0.94f, 0.27f, 0.27f), 0.5f);
            Shapes.Rounded(body, Vector3.zero, Vector3.one, shell, 0.14f, "Delivery");
            Shapes.Rounded(body, new Vector3(0, 0.45f, 0), new Vector3(0.86f, 0.12f, 0.86f), MaterialLibrary.Shared(new Color(1f, 0.45f, 0.42f), 0.5f), 0.05f, "Lid");
            var arrow = UI.IconFactory.Get(UI.IconKind.Next);
            var top = GoalTileBuilder.Decal(body, arrow, Color.white, 0.62f, 0.515f);
            top.transform.localRotation = Quaternion.Euler(90, -90, 0); // the icon points right: turn it to point up the board
            var front = Shapes.Quad(body, new Vector3(0, 0.02f, -0.505f), new Vector3(0.5f, 0.5f, 1f), Quaternion.Euler(0, 0, 90), MaterialLibrary.Unlit(Color.white, arrow.texture), "ArrowFront");
            Shapes.NoShadows(front);
            return shell;
        }

        // Warm honey wood crate with slats; three small variations so a row of crates does not look copied.
        private static Material BuildWood(Transform body, int variant)
        {
            float tint = 1f + (variant - 1) * 0.04f;
            var shell = MaterialLibrary.Create(new Color(Palette.Wood.r * tint, Palette.Wood.g * tint, Palette.Wood.b, 1f), 0.25f);
            Shapes.Rounded(body, Vector3.zero, Vector3.one, shell, 0.08f, "Shell");
            var slat = MaterialLibrary.Shared(Palette.WoodDark, 0.2f);
            float[] heights = variant == 0 ? new[] { -0.22f, 0.22f } : variant == 1 ? new[] { 0f } : new[] { -0.28f, 0f, 0.28f };
            foreach (float h in heights)
            {
                Shapes.Rounded(body, new Vector3(0, h, 0.5f), new Vector3(0.9f, 0.12f, 0.05f), slat, 0.02f, "Slat");
                Shapes.Rounded(body, new Vector3(0, h, -0.5f), new Vector3(0.9f, 0.12f, 0.05f), slat, 0.02f, "Slat");
                Shapes.Rounded(body, new Vector3(0.5f, h, 0), new Vector3(0.05f, 0.12f, 0.9f), slat, 0.02f, "Slat");
                Shapes.Rounded(body, new Vector3(-0.5f, h, 0), new Vector3(0.05f, 0.12f, 0.9f), slat, 0.02f, "Slat");
            }
            // Corner posts make the crate silhouette.
            for (int i = 0; i < 4; i++)
            {
                float x = (i & 1) == 0 ? -0.46f : 0.46f, z = (i & 2) == 0 ? -0.46f : 0.46f;
                Shapes.Rounded(body, new Vector3(x, 0, z), new Vector3(0.12f, 1.02f, 0.12f), slat, 0.03f, "Post");
            }
            Shapes.Rounded(body, new Vector3(0, 0.5f, 0), new Vector3(0.76f, 0.04f, 0.76f), MaterialLibrary.Shared(Palette.WoodScuff, 0.3f), 0.02f, "Lid");
            return shell;
        }

        // Rounder, translucent, saturated turquoise with frost on top and trapped bubbles.
        private static Material BuildIce(Transform body)
        {
            var shell = MaterialLibrary.Create(Palette.Ice, 0.95f);
            Shapes.Rounded(body, Vector3.zero, Vector3.one, shell, 0.2f, "Shell");
            Shapes.Rounded(body, new Vector3(0, 0.47f, 0), new Vector3(0.8f, 0.08f, 0.8f), MaterialLibrary.Shared(Palette.Frost, 0.4f), 0.04f, "Frost");
            var bubble = MaterialLibrary.Shared(Palette.IceCore, 0.9f);
            var rng = new System.Random(7);
            for (int i = 0; i < 6; i++)
            {
                var p = new Vector3((float)rng.NextDouble() - 0.5f, (float)rng.NextDouble() - 0.5f, (float)rng.NextDouble() - 0.5f) * 0.55f;
                float s = 0.06f + (float)rng.NextDouble() * 0.08f;
                Shapes.NoShadows(Shapes.Make(PrimitiveType.Sphere, body, p, Vector3.one * s, bubble, "Bubble"));
            }
            return shell;
        }

        // Cold, matte steel-blue container: corrugated sides and two door bars at the front. Reads like the walls.
        // Fixed crate (unit box): slate-blue shell, two dark straps, yellow hazard corners (as ObstacleBuilder's fixed crates).
        private static Material BuildFixed(Transform body)
        {
            var shell = MaterialLibrary.Create(new Color(0.36f, 0.42f, 0.52f), 0.45f);
            var steel = MaterialLibrary.Shared(new Color(0.24f, 0.26f, 0.3f), 0.45f);
            var yellow = MaterialLibrary.Shared(new Color(1f, 0.79f, 0.24f), 0.5f);
            Shapes.Rounded(body, Vector3.zero, Vector3.one, shell, 0.1f, "Shell");
            foreach (var x in new[] { -0.22f, 0.22f })
                Shapes.Rounded(body, new Vector3(x, 0, 0), new Vector3(0.11f, 1.02f, 1.02f), steel, 0.03f, "Strap");
            for (int i = 0; i < 4; i++)
            {
                float sx = (i & 1) == 0 ? -0.43f : 0.43f, sz = (i & 2) == 0 ? -0.43f : 0.43f;
                Shapes.Rounded(body, new Vector3(sx, 0, sz), new Vector3(0.17f, 1.04f, 0.17f), yellow, 0.04f, "Corner");
            }
            return shell;
        }

        private static Material BuildContainer(Transform body)
        {
            var shell = MaterialLibrary.Create(Palette.Container, 0.3f, 0.35f);
            Shapes.Rounded(body, Vector3.zero, Vector3.one, shell, 0.03f, "Shell");
            var rib = MaterialLibrary.Shared(Palette.ContainerDark, 0.25f, 0.35f);
            const int ribs = 7;
            for (int side = 0; side < 4; side++)
            {
                var rot = Quaternion.Euler(0, 90 * side, 0);
                for (int i = 0; i < ribs; i++)
                {
                    float x = -0.42f + i * (0.84f / (ribs - 1));
                    var pos = rot * new Vector3(x, 0, 0.505f);
                    var size = rot * new Vector3(0.04f, 0.86f, 0.03f);
                    Shapes.Cube(body, pos, new Vector3(Mathf.Abs(size.x), size.y, Mathf.Abs(size.z)), rib, "Rib");
                }
            }
            // Top and bottom rails, then the door bars on the camera side (-z).
            Shapes.Cube(body, new Vector3(0, 0.47f, 0), new Vector3(1.03f, 0.07f, 1.03f), rib, "TopRail");
            Shapes.Cube(body, new Vector3(0, -0.47f, 0), new Vector3(1.03f, 0.07f, 1.03f), rib, "BottomRail");
            var bar = MaterialLibrary.Shared(new Color(0.75f, 0.80f, 0.84f), 0.5f, 0.7f);
            for (int s = -1; s <= 1; s += 2)
                Shapes.Cube(body, new Vector3(s * 0.12f, 0, -0.54f), new Vector3(0.035f, 0.8f, 0.035f), bar, "DoorBar");
            return shell;
        }

        // Wide, low, cast iron with a yellow-black hazard band, rivets, side handles and a weight icon on top.
        private static Material BuildHeavy(Transform body)
        {
            var shell = MaterialLibrary.Create(Palette.Iron, 0.45f, 0.6f);
            Shapes.Rounded(body, Vector3.zero, Vector3.one, shell, 0.06f, "Shell");
            var yellow = MaterialLibrary.Shared(Palette.Hazard, 0.3f);
            var black = MaterialLibrary.Shared(Palette.HazardDark, 0.3f);
            const int segments = 6;
            for (int side = 0; side < 4; side++)
            {
                var rot = Quaternion.Euler(0, 90 * side, 0);
                for (int i = 0; i < segments; i++)
                {
                    float x = -0.5f + (i + 0.5f) / segments;
                    var pos = rot * new Vector3(x * 1.0f, -0.05f, 0.505f);
                    var size = rot * new Vector3(1f / segments, 0.22f, 0.03f);
                    Shapes.Cube(body, pos, new Vector3(Mathf.Abs(size.x), size.y, Mathf.Abs(size.z)), i % 2 == 0 ? yellow : black, "Hazard");
                }
                // Rivets along the top edge.
                var rivet = MaterialLibrary.Shared(Palette.IronEdge, 0.6f, 0.8f);
                for (int i = -1; i <= 1; i += 2)
                    Shapes.Make(PrimitiveType.Sphere, body, rot * new Vector3(i * 0.38f, 0.33f, 0.5f), Vector3.one * 0.07f, rivet, "Rivet");
            }
            // Handles on the left and right sides.
            var handle = MaterialLibrary.Shared(Palette.IronEdge, 0.5f, 0.8f);
            for (int s = -1; s <= 1; s += 2)
            {
                Shapes.Cube(body, new Vector3(s * 0.56f, 0.22f, 0), new Vector3(0.05f, 0.05f, 0.42f), handle, "Handle");
                Shapes.Cube(body, new Vector3(s * 0.535f, 0.22f, 0.19f), new Vector3(0.05f, 0.05f, 0.05f), handle, "HandleFoot");
                Shapes.Cube(body, new Vector3(s * 0.535f, 0.22f, -0.19f), new Vector3(0.05f, 0.05f, 0.05f), handle, "HandleFoot");
            }
            // Rust patches and a kettlebell-shaped weight mark on top.
            var rust = MaterialLibrary.Shared(Palette.Rust, 0.1f);
            Shapes.Cube(body, new Vector3(-0.3f, 0.502f, 0.25f), new Vector3(0.22f, 0.01f, 0.14f), rust, "Rust");
            Shapes.Cube(body, new Vector3(0.28f, -0.3f, 0.503f), new Vector3(0.16f, 0.12f, 0.01f), rust, "Rust");
            var mark = MaterialLibrary.Shared(Palette.Hazard, 0.3f);
            Shapes.Make(PrimitiveType.Sphere, body, new Vector3(0, 0.52f, 0), new Vector3(0.26f, 0.06f, 0.26f), mark, "WeightIcon");
            Shapes.Cube(body, new Vector3(0, 0.56f, 0), new Vector3(0.16f, 0.05f, 0.05f), mark, "WeightHandle");
            return shell;
        }
    }
}
