using CrateBoss.Core;
using UnityEngine;

namespace CrateBoss.View
{
    /// <summary>
    /// Walls inside the yard drawn as real obstacles of the district (Ana Belge §2.5, Ana Belge §2): City: pallet
    /// stack, concrete block, wheelie-bin skip; Port: one-high container, mooring bollard, concrete block; Airport: parked
    /// baggage cart, ULD container, concrete block; Cold Store: reefer crates, ice block, concrete block; Customs:
    /// barricade, sealed container, checkpoint booth. Matt, cool tones (never mistaken for crates). The kind comes from a
    /// seed made of the level id and the cell, so a level looks the same every time. A prefab at
    /// Resources/Models/Obstacle_&lt;district&gt;_&lt;n&gt; replaces the built-in shape.
    /// </summary>
    public static class ObstacleBuilder
    {
        private const float Slab = 0.2f;

        /// <summary>Builds one obstacle; returns its main material (see-through capable when it stands in front of the yard).</summary>
        public static Material Build(Transform parent, WorldTheme theme, GridPos cell, int seed, bool front, out Color color) =>
            Build(parent, theme, cell, seed, front, ObstaclePiece.Single, out color);

        // Toy car colours (Ana Belge §2 candy tones).
        private static readonly Color[] CarColors =
        {
            Hex("#F2558A"), Hex("#9B5DE5"), Hex("#3EC1D3"), Hex("#FFD23F"), Hex("#FF8C42"), Hex("#3DCC6B"), Hex("#EF4444"), Hex("#5AA9F0"), Hex("#F7F7F9"),
        };

        private static Color Hex(string hex) => ColorUtility.TryParseHtmlString(hex, out var c) ? c : Color.magenta;

        /// <summary>
        /// Builds this cell's part of an obstacle (Ana Belge §3): two cells side by side are one parked toy car, a 2x2 square a
        /// container stack; the head cell draws the whole piece, the other cells only their footing.
        /// </summary>
        public static Material Build(Transform parent, WorldTheme theme, GridPos cell, int seed, bool front, ObstaclePiece piece, out Color color)
        {
            int hash = Mathf.Abs((seed * 73856093) ^ (cell.X * 19349663) ^ (cell.Y * 83492791));
            if (piece.Kind == ObstaclePieceKind.Single)
            {
                // Owner 2026-10-31: no props in the yard (cars, planters...): every inner wall is a fixed crate.
                color = FixedCrateColor;
                var shell = front ? MaterialLibrary.CreateFadable(color, 0.45f) : MaterialLibrary.Shared(color, 0.45f);
                // The owner's rank-1 fixed crate model (2026-11-01) when imported; the code-drawn one otherwise.
                var model = VisualCatalog.TryInstantiateRanked("BoxFixed", parent);
                // A front crate fades on its own (TileView), so it takes a copy; the others share the toy material.
                var r = model != null ? model.GetComponentInChildren<Renderer>() : null;
                if (model != null) return r == null ? shell : front ? r.material : r.sharedMaterial;
                FixedCrate(parent, shell);
                return shell;
            }
            int kind = hash % 3;
            var tints = theme.ObstacleTints;
            color = tints[(hash / 3) % tints.Length];
            if (piece.Kind != ObstaclePieceKind.Single)
            {
                // Parking-lot floor under the big pieces (the car stands in its painted bay).
                Shapes.Cube(parent, new Vector3(0, -Slab / 2f, 0), new Vector3(1f, Slab, 1f), MaterialLibrary.Shared(theme.Sidewalk, 0.05f), "Footing");
                if (piece.Kind == ObstaclePieceKind.CarHead)
                {
                    color = CarColors[(hash / 7) % CarColors.Length];
                    var body = front ? MaterialLibrary.CreateFadable(color, 0.5f) : MaterialLibrary.Shared(color, 0.5f);
                    ToyCar(parent, piece.Offset, piece.Vertical, body, hash);
                    return body;
                }
                if (piece.Kind == ObstaclePieceKind.BigHead)
                {
                    var box = front ? MaterialLibrary.CreateFadable(color, 0.3f) : MaterialLibrary.Shared(color, 0.3f);
                    ContainerStack(parent, piece.Offset, box, MaterialLibrary.Shared(color * 0.75f, 0.3f));
                    return box;
                }
                return null; // tail / part: drawn by the head
            }
            // The corrugated containers get their own material.
            bool textured = false;
            var main = front ? MaterialLibrary.CreateFadable(color, 0.1f) : textured ? MaterialLibrary.Create(color, 0.1f) : MaterialLibrary.Shared(color, 0.1f);
            if (VisualCatalog.TryInstantiate($"Obstacle_{theme.District}_{kind}", parent) != null) return main;

            // A dark footing so every obstacle fills its cell the same way.
            Shapes.Cube(parent, new Vector3(0, -Slab / 2f, 0), new Vector3(0.98f, Slab, 0.98f), MaterialLibrary.Shared(theme.Sidewalk * 0.8f, 0.05f), "Footing");
            var dark = MaterialLibrary.Shared(color * 0.72f, 0.1f);
            switch (theme.District)
            {
                default:
                    if (kind == 0) Pallets(parent, main, dark);
                    else if (kind == 1) Concrete(parent, main, dark);
                    else Skip(parent, main, dark);
                    break;
            }
            return main;
        }

        /// <summary>
        /// Parked toy car over two cells (Ana Belge §3: 1 x 2 cells, ≈0.8 high): chunky rounded body, a white-roofed cabin with
        /// light blue windows, four big black wheels, round headlights. Faces up or right; the seed turns some round.
        /// </summary>
        private static readonly Color FixedCrateColor = new Color(0.36f, 0.42f, 0.52f);

        /// <summary>
        /// Fixed crate (owner 2026-10-31: "sabit kasalar"): a slate-blue plastic crate, a little bigger than the wooden ones,
        /// bolted down on a steel foot with yellow-black hazard corners and two dark straps, so it reads "does not move".
        /// </summary>
        private static void FixedCrate(Transform p, Material shell)
        {
            var steel = MaterialLibrary.Shared(new Color(0.24f, 0.26f, 0.3f), 0.45f);
            var yellow = MaterialLibrary.Shared(new Color(1f, 0.79f, 0.24f), 0.5f);
            Shapes.Rounded(p, new Vector3(0, 0.04f, 0), new Vector3(0.98f, 0.08f, 0.98f), steel, 0.03f, "Foot");
            Shapes.Rounded(p, new Vector3(0, 0.47f, 0), new Vector3(0.92f, 0.8f, 0.92f), shell, 0.1f, "FixedCrate");
            foreach (var x in new[] { -0.2f, 0.2f })
                Shapes.Rounded(p, new Vector3(x, 0.47f, 0), new Vector3(0.1f, 0.82f, 0.94f), steel, 0.03f, "Strap");
            for (int i = 0; i < 4; i++)
            {
                float sx = (i & 1) == 0 ? -0.4f : 0.4f, sz = (i & 2) == 0 ? -0.4f : 0.4f;
                Shapes.Rounded(p, new Vector3(sx, 0.47f, sz), new Vector3(0.16f, 0.84f, 0.16f), yellow, 0.04f, "Corner");
                Shapes.Make(PrimitiveType.Cylinder, p, new Vector3(sx * 1.12f, 0.1f, sz * 1.12f), new Vector3(0.09f, 0.03f, 0.09f), steel, "Bolt");
            }
        }

        public static void ToyCar(Transform p, Vector3 centre, bool vertical, Material body, int hash)
        {
            var car = new GameObject("ToyCar").transform;
            car.SetParent(p, false);
            car.localPosition = centre;
            // Built along local z (length); turned across for side-by-side cells, and half the cars face the other way.
            float turn = (vertical ? 0f : 90f) + ((hash >> 4) % 2 == 0 ? 0f : 180f);
            car.localRotation = Quaternion.Euler(0, turn, 0);
            var glass = MaterialLibrary.Shared(new Color(0.62f, 0.85f, 1f), 0.8f);
            var tyre = MaterialLibrary.Shared(new Color(0.13f, 0.13f, 0.15f), 0.3f);
            var light = MaterialLibrary.Shared(new Color(1f, 0.97f, 0.82f), 0.7f);
            Shapes.Rounded(car, new Vector3(0, 0.3f, 0), new Vector3(0.82f, 0.36f, 1.78f), body, 0.16f, "Body");
            Shapes.Rounded(car, new Vector3(0, 0.6f, -0.1f), new Vector3(0.72f, 0.32f, 0.95f), body, 0.14f, "Cabin");
            Shapes.Rounded(car, new Vector3(0, 0.62f, 0.38f), new Vector3(0.66f, 0.22f, 0.06f), glass, 0.04f, "Windscreen");
            Shapes.Rounded(car, new Vector3(0, 0.62f, -0.58f), new Vector3(0.62f, 0.2f, 0.05f), glass, 0.04f, "RearWindow");
            for (int s = -1; s <= 1; s += 2)
                Shapes.Rounded(car, new Vector3(s * 0.365f, 0.63f, -0.1f), new Vector3(0.03f, 0.2f, 0.78f), glass, 0.02f, "SideWindow");
            for (int s = -1; s <= 1; s += 2)
            for (int f = -1; f <= 1; f += 2)
            {
                var wheel = Shapes.Make(PrimitiveType.Cylinder, car, new Vector3(s * 0.38f, 0.17f, f * 0.56f), new Vector3(0.34f, 0.07f, 0.34f), tyre, "Wheel");
                wheel.transform.localRotation = Quaternion.Euler(0, 0, 90);
                Shapes.Make(PrimitiveType.Sphere, car, new Vector3(s * 0.25f, 0.33f, 0.885f), new Vector3(0.14f, 0.1f, 0.06f), light, "Headlight");
            }
        }

        // 2 x 2 cells: two toy containers side by side with a third on top (rounded, ribbed doors).
        private static void ContainerStack(Transform p, Vector3 centre, Material main, Material dark)
        {
            var stack = new GameObject("ContainerStack").transform;
            stack.SetParent(p, false);
            stack.localPosition = centre;
            for (int i = -1; i <= 1; i += 2)
            {
                Shapes.Rounded(stack, new Vector3(i * 0.48f, 0.36f, 0), new Vector3(0.9f, 0.72f, 1.9f), main, 0.1f, "Container");
                for (int r = 0; r < 4; r++)
                    Shapes.Rounded(stack, new Vector3(i * 0.48f, 0.36f, -0.55f + r * 0.37f), new Vector3(0.94f, 0.6f, 0.06f), dark, 0.03f, "Rib");
            }
            Shapes.Rounded(stack, new Vector3(0.1f, 1.06f, 0), new Vector3(1.6f, 0.66f, 0.9f), dark, 0.1f, "TopContainer");
        }

        // Cold store: two matt white reefer crates stacked, with vent slots.
        private static void ReeferStack(Transform p, Material main, Material dark)
        {
            Shapes.Rounded(p, new Vector3(0, 0.2f, 0), new Vector3(0.9f, 0.4f, 0.86f), main, 0.04f, "Reefer");
            Shapes.Rounded(p, new Vector3(0.03f, 0.6f, -0.02f), new Vector3(0.8f, 0.38f, 0.78f), main, 0.04f, "Reefer");
            for (int i = 0; i < 3; i++)
                Shapes.Cube(p, new Vector3(-0.2f + i * 0.2f, 0.62f, -0.415f), new Vector3(0.12f, 0.2f, 0.02f), dark, "Vent");
        }

        // Cold store: a big rough block of ice, opaque and frosted (never mistaken for the pushable ice crate).
        private static void IceBlock(Transform p, Material main, Material dark)
        {
            Shapes.Rounded(p, new Vector3(0, 0.34f, 0), new Vector3(0.9f, 0.68f, 0.9f), main, 0.12f, "IceBlock");
            Shapes.Rounded(p, new Vector3(0, 0.7f, 0), new Vector3(0.82f, 0.05f, 0.82f), MaterialLibrary.Shared(Color.white, 0.05f), 0.02f, "Frost");
        }

        // Customs: concrete road barricade with red-white bands.
        private static void Barricade(Transform p, Material main, Material dark)
        {
            Shapes.Rounded(p, new Vector3(0, 0.3f, 0), new Vector3(0.92f, 0.6f, 0.5f), main, 0.06f, "Barricade");
            Shapes.Cube(p, new Vector3(0, 0.08f, 0), new Vector3(0.96f, 0.16f, 0.82f), main, "Foot");
            for (int i = 0; i < 4; i++)
                Shapes.Cube(p, new Vector3(-0.34f + i * 0.23f, 0.45f, -0.26f), new Vector3(0.2f, 0.1f, 0.02f),
                    MaterialLibrary.Shared(i % 2 == 0 ? new Color(0.78f, 0.25f, 0.22f) : new Color(0.92f, 0.92f, 0.9f), 0.2f), "Band");
        }

        // Customs: a sealed container (one high) with a red seal on its doors.
        private static void SealedContainer(Transform p, Material main, Material dark)
        {
            Container(p, main, dark);
            Shapes.Cube(p, new Vector3(0, 0.4f, -0.49f), new Vector3(0.1f, 0.14f, 0.03f), MaterialLibrary.Shared(new Color(0.8f, 0.15f, 0.15f), 0.4f), "Seal");
        }

        // Customs: a small checkpoint booth with a window.
        private static void Booth(Transform p, Material main, Material dark)
        {
            Shapes.Rounded(p, new Vector3(0, 0.38f, 0), new Vector3(0.78f, 0.76f, 0.78f), main, 0.05f, "Booth");
            Shapes.Cube(p, new Vector3(0, 0.82f, 0), new Vector3(0.9f, 0.08f, 0.9f), dark, "Roof");
            Shapes.Cube(p, new Vector3(0, 0.5f, -0.4f), new Vector3(0.5f, 0.24f, 0.02f), MaterialLibrary.Shared(new Color(0.35f, 0.5f, 0.65f), 0.7f), "Window");
        }

        // Rounded jersey block with a slanted top edge.
        private static void Concrete(Transform p, Material main, Material dark)
        {
            Shapes.Rounded(p, new Vector3(0, 0.28f, 0), new Vector3(0.9f, 0.56f, 0.9f), main, 0.08f, "Concrete");
            Shapes.Rounded(p, new Vector3(0, 0.6f, 0), new Vector3(0.62f, 0.1f, 0.62f), main, 0.04f, "ConcreteTop");
            Shapes.Cube(p, new Vector3(0, 0.12f, 0), new Vector3(0.92f, 0.03f, 0.92f), dark, "Seam");
        }

        // Three pallets stacked, slats showing on the sides.
        private static void Pallets(Transform p, Material main, Material dark)
        {
            for (int i = 0; i < 3; i++)
            {
                float y = 0.06f + i * 0.2f;
                Shapes.Cube(p, new Vector3(0, y + 0.07f, 0), new Vector3(0.9f, 0.04f, 0.9f), main, "Deck");
                for (int b = -1; b <= 1; b++)
                    Shapes.Cube(p, new Vector3(b * 0.38f, y, 0), new Vector3(0.12f, 0.1f, 0.9f), dark, "Block");
            }
            Shapes.Cube(p, new Vector3(0, 0.72f, 0), new Vector3(0.84f, 0.12f, 0.84f), main, "Load");
        }

        // Builder's skip / big bin: tapered box with a lid and side lugs.
        private static void Skip(Transform p, Material main, Material dark)
        {
            Shapes.Rounded(p, new Vector3(0, 0.32f, 0), new Vector3(0.88f, 0.64f, 0.82f), main, 0.05f, "Bin");
            Shapes.Cube(p, new Vector3(0, 0.66f, 0), new Vector3(0.92f, 0.05f, 0.86f), dark, "Lid");
            for (int s = -1; s <= 1; s += 2)
                Shapes.Cube(p, new Vector3(s * 0.46f, 0.42f, 0), new Vector3(0.05f, 0.12f, 0.3f), dark, "Lug");
        }

        // One-high shipping container, corrugated.
        private static void Container(Transform p, Material main, Material dark)
        {
            var mat = main;
            MaterialLibrary.SetTexture(mat, CorrugatedTexture.Get());
            Shapes.Cube(p, new Vector3(0, 0.38f, 0), new Vector3(0.96f, 0.76f, 0.96f), mat, "Container");
            foreach (var z in new[] { 0.47f, -0.47f })
                Shapes.Cube(p, new Vector3(0, 0.77f, z), new Vector3(0.96f, 0.04f, 0.04f), dark, "Rail");
        }

        // Mooring bollard on a concrete plinth.
        private static void Bollard(Transform p, Material main, Material dark)
        {
            Shapes.Rounded(p, new Vector3(0, 0.12f, 0), new Vector3(0.9f, 0.24f, 0.9f), main, 0.05f, "Plinth");
            var post = MaterialLibrary.Shared(new Color(0.18f, 0.19f, 0.21f), 0.3f, 0.4f);
            Shapes.Make(PrimitiveType.Cylinder, p, new Vector3(0, 0.45f, 0), new Vector3(0.36f, 0.22f, 0.36f), post, "Post");
            Shapes.Make(PrimitiveType.Cylinder, p, new Vector3(0, 0.69f, 0), new Vector3(0.48f, 0.04f, 0.48f), post, "Cap");
        }

        // Parked baggage cart: flat bed, low rails, a tow bar.
        private static void BaggageCart(Transform p, Material main, Material dark)
        {
            Shapes.Cube(p, new Vector3(0, 0.22f, 0), new Vector3(0.84f, 0.06f, 0.9f), main, "Bed");
            for (int s = -1; s <= 1; s += 2)
            {
                Shapes.Cube(p, new Vector3(s * 0.41f, 0.42f, 0), new Vector3(0.04f, 0.38f, 0.9f), main, "Side");
                Shapes.Make(PrimitiveType.Cylinder, p, new Vector3(s * 0.34f, 0.09f, 0.32f), new Vector3(0.16f, 0.04f, 0.16f), dark, "Wheel").transform.localRotation = Quaternion.Euler(0, 0, 90);
                Shapes.Make(PrimitiveType.Cylinder, p, new Vector3(s * 0.34f, 0.09f, -0.32f), new Vector3(0.16f, 0.04f, 0.16f), dark, "Wheel").transform.localRotation = Quaternion.Euler(0, 0, 90);
            }
            Shapes.Cube(p, new Vector3(0, 0.5f, 0.2f), new Vector3(0.7f, 0.5f, 0.42f), dark, "Bags");
        }

        // Air cargo ULD: box with one slanted side.
        private static void Uld(Transform p, Material main, Material dark)
        {
            Shapes.Rounded(p, new Vector3(0.08f, 0.38f, 0), new Vector3(0.76f, 0.76f, 0.9f), main, 0.04f, "Uld");
            var slope = Shapes.Cube(p, new Vector3(-0.3f, 0.24f, 0), new Vector3(0.3f, 0.36f, 0.88f), main, "Slope");
            slope.transform.localRotation = Quaternion.Euler(0, 0, -28f);
            Shapes.Cube(p, new Vector3(0.08f, 0.04f, 0), new Vector3(0.86f, 0.06f, 0.94f), dark, "Base");
        }
    }
}
