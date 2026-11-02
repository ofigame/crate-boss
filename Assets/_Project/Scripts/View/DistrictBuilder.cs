using System.Collections.Generic;
using UnityEngine;

namespace CrateBoss.View
{
    /// <summary>
    /// The district around the yard (Ana Belge §2.5): the yard in the middle, a pavement ring, a road
    /// ring with a dashed line, an outer pavement, then lots with houses / container stacks / the apron. Tall things stand
    /// behind and beside the yard, only low things in front, so nothing hides the board. In the port the front side is
    /// the sea. Everything is decoration: the game never looks at it.
    /// </summary>
    public static class DistrictBuilder
    {
        private const float Pavement = 1.2f, Road = 2.6f, OuterPavement = 1.2f;

        /// <summary>Tower crane foundations of the level about to be built (board space); the game fills it before Build.</summary>
        public static readonly List<Vector3> CraneMasts = new List<Vector3>();

        /// <summary>Home screen backdrop (owner 2026-11-01): the next district stands in the middle of a full car park.</summary>
        public static bool FullCarPark;

        /// <summary>Candy levels (owner 2026-11-01): the main road runs right behind the yard, where the truck comes and goes.</summary>
        public static bool BackRoad;

        /// <summary>Candy levels with trucks on more sides (owner 2026-11-02): -x / -z sides get a road too (back and right have one).</summary>
        public static bool LeftRoad, FrontRoad;

        /// <summary>From the yard's back edge to the centre of the road behind it (layout with a back road).</summary>
        public const float BackRoadOffset = Pavement + Road / 2f;

        public static void Build(Transform root, WorldTheme theme, int w, int h, int seed)
        {
            var district = new GameObject("District").transform;
            district.SetParent(root, false);
            float l = -0.5f, r = w - 0.5f, f = -0.5f, k = h - 0.5f;
            bool sea = theme.Sea;
            float roadIn = Pavement, roadOut = Pavement + Road, lotEdge = roadOut + OuterPavement;

            // Ground: the yard's own pavement ring (rounded corners), then the district's open ground. Void cells inside the
            // map stay open (frames). The port has no front side: sea.
            var lots = theme.Grass;
            Ring(district, l, r, f, k, 0f, roadIn, -0.045f, theme.Sidewalk, sea, "Pavement");
            Ring(district, l, r, f, k, roadIn, 60f, -0.06f, lots, sea, "Lot");

            placed.Clear();
            keepClear = Rect.MinMaxRect(l - roadIn, f - (sea ? 0f : roadIn), r + roadIn, k + roadIn);
            var rng = new System.Random(seed);
            // Crane foundations (set by the game before the district is built): nothing may stand on them, and the road keeps off.
            foreach (var m in CraneMasts) placed.Add(new Bounds(m + Vector3.up * 0.4f, new Vector3(1.8f, 0.8f, 1.8f)));
            // Owner 2026-10-31: a car park may be sparse or full, never empty: 40–100% of the bays, spread evenly (below).
            parkFill = FullCarPark ? 1f : 0.4f + (float)rng.NextDouble() * 0.6f;
            // Owner 2026-10-31: no road loops round the yard; it runs past and on into side streets, and the layout varies:
            // 0 = back and side (an L), 1 = the yard in the middle of a car park with the road further off (and a side
            // street), 2 = back, side and front (a Z). The port: along the back and away from the sea, never into it.
            int layout = sea || BackRoad ? 0 : CraneMasts.Count > 0 || FullCarPark ? 1 : Mathf.Abs(seed * 7 + w * 3 + h) % 3;
            var roads = Roads(l, r, f, k, sea, layout, CraneMasts.Count > 0);
            foreach (var road in roads) RoadNetwork.Build(district, road, theme, placed);
            float near = layout == 1 ? roadIn + 0.2f : lotEdge;
            City(district, l, r, f, k, near, rng);
            Lamps(district, theme.District, l, r, f, k, sea);
            RoadSide(district, theme.District, roads[0], rng);
            Traffic(district, theme.District, roads[0], rng);
            MeshMerger.Merge(district, t => t.GetComponent<EnvVehicle>() != null);
        }

        /// <summary>Centre lines of the district's roads (rounded), the main one first.</summary>
        private static List<List<Vector3>> Roads(float l, float r, float f, float k, bool sea, int layout, bool cranes)
        {
            const float far = 60f;
            float half = Road / 2f;
            float back = k + Pavement + half, right = r + Pavement + half, front = f - Pavement - half;
            var list = new List<List<Vector3>>();
            if (sea)
            {
                // Along the back, then up a side street away from the quay.
                if (cranes) back += 6.2f; // the tower cranes stand right behind the yard
                float side = l - Pavement - 3f - half;
                list.Add(RoadNetwork.Smooth(new[] { new Vector3(far, 0, back), new Vector3(side, 0, back), new Vector3(side, 0, far) }));
                return list;
            }
            switch (layout)
            {
                case 1:
                {
                    float gap = 6.2f; // a car park between the yard and the road
                    float backFar = back + gap, left = l - Pavement - gap - half;
                    list.Add(RoadNetwork.Smooth(new[] { new Vector3(far, 0, backFar), new Vector3(left, 0, backFar), new Vector3(left, 0, -far) }));
                    float cx = (l + r) / 2f + 2f;
                    list.Add(RoadNetwork.Smooth(new[] { new Vector3(cx, 0, backFar + half), new Vector3(cx, 0, far) })); // side street
                    break;
                }
                case 2:
                    list.Add(RoadNetwork.Smooth(new[] { new Vector3(-far, 0, back), new Vector3(right, 0, back), new Vector3(right, 0, front), new Vector3(far, 0, front) }));
                    break;
                default:
                    list.Add(RoadNetwork.Smooth(new[] { new Vector3(-far, 0, back), new Vector3(right, 0, back), new Vector3(right, 0, -far) }));
                    if (LeftRoad) list.Add(RoadNetwork.Smooth(new[] { new Vector3(l - Pavement - half, 0, back + half), new Vector3(l - Pavement - half, 0, -far) }));
                    if (FrontRoad) list.Add(RoadNetwork.Smooth(new[] { new Vector3(-far, 0, front), new Vector3(right - half, 0, front) }));
                    break;
            }
            return list;
        }

        /// <summary>Street lamps and trees along the main road, on the far kerb (lamps) and the near kerb (trees).</summary>
        private static void RoadSide(Transform t, DistrictKind d, List<Vector3> road, System.Random rng)
        {
            float edge = Road / 2f + RoadNetwork.Kerb * 0.55f;
            foreach (var (p, right) in RoadNetwork.Along(road, 6.5f, 2f))
            {
                float yaw = Mathf.Atan2(right.x, right.z) * Mathf.Rad2Deg; // the arm (built along -z) reaches over the road
                Place(d, EnvPiece.StreetLamp, t, p + right * edge + Vector3.up * -0.045f, yaw, 0, false);
            }
            foreach (var (p, right) in RoadNetwork.Along(road, 4.2f, 1f))
                Place(d, EnvPiece.Tree, t, p - right * (edge + 0.4f) + Vector3.up * -0.06f, 0f, rng.Next(6), false);
        }

        /// <summary>Two vehicles each way, keeping to their lane through the curves and off screen at both ends.</summary>
        private static void Traffic(Transform t, DistrictKind d, List<Vector3> road, System.Random rng)
        {
            var piece = EnvPiece.Car;
            const float speed = 2.2f;
            var ahead = RoadNetwork.Lane(road, Road / 4f);
            var back = new List<Vector3>(road);
            back.Reverse();
            var other = RoadNetwork.Lane(back, Road / 4f);
            for (int i = 0; i < 2; i++)
            {
                Vehicle(t, d, piece, ahead, speed, i * 0.5f, rng);
                Vehicle(t, d, piece, other, speed * 0.9f, 0.25f + i * 0.5f, rng);
            }
        }

        // The band between two insets round the yard: back, left and right strips, plus the front strip unless it is sea.
        // The corners are quarter rings round the yard's corners, so pavements and the road turn in smooth curves (owner
        // 2026-10-31: "yolların dönüş kısımlarındaki köşeleri oval yap", like the toy parking reference).
        private static void Ring(Transform t, float l, float r, float f, float k, float inner, float outer, float topY, Color color, bool sea, string name)
        {
            Ground(t, l, r, k + inner, k + outer, topY, color, sea, name);
            Ground(t, l - outer, l - inner, f, k, topY, color, sea, name);
            Ground(t, r + inner, r + outer, f, k, topY, color, sea, name);
            Corner(t, new Vector3(l, 0, k), inner, outer, 90f, 180f, topY, color, sea, name);
            Corner(t, new Vector3(r, 0, k), inner, outer, 0f, 90f, topY, color, sea, name);
            if (sea) return;
            Ground(t, l, r, f - outer, f - inner, topY, color, sea, name);
            Corner(t, new Vector3(l, 0, f), inner, outer, 180f, 270f, topY, color, sea, name);
            Corner(t, new Vector3(r, 0, f), inner, outer, 270f, 360f, topY, color, sea, name);
        }

        private const int CornerSegments = 14;

        /// <summary>A quarter ring (radii inner..outer round <paramref name="centre"/>, angles in degrees from +x towards +z), top at topY.</summary>
        private static void Corner(Transform t, Vector3 centre, float inner, float outer, float a0, float a1, float topY, Color color, bool sea, string name)
        {
            float depth = sea ? 1.4f : 0.1f;
            int n = CornerSegments;
            var verts = new List<Vector3>();
            var tris = new List<int>();
            // Top surface: inner and outer arc, then the outer and inner side walls down to the slab's bottom.
            for (int i = 0; i <= n; i++)
            {
                float a = Mathf.Deg2Rad * Mathf.Lerp(a0, a1, i / (float)n);
                var d = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a));
                verts.Add(centre + d * inner + Vector3.up * topY);
                verts.Add(centre + d * outer + Vector3.up * topY);
                verts.Add(centre + d * outer + Vector3.up * (topY - depth));
                verts.Add(centre + d * inner + Vector3.up * (topY - depth));
            }
            for (int i = 0; i < n; i++)
            {
                int a = i * 4, b = a + 4;
                tris.AddRange(new[] { a, b, a + 1, a + 1, b, b + 1 });             // top
                tris.AddRange(new[] { a + 1, b + 1, a + 2, a + 2, b + 1, b + 2 }); // outer wall
                tris.AddRange(new[] { a + 3, b + 3, a, a, b + 3, b });             // inner wall
            }
            var mesh = new Mesh { name = name + "Corner" };
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            // Wind the faces to look up / outwards whichever way the angles run.
            if (Vector3.Cross(verts[4] - verts[0], verts[1] - verts[0]).y < 0f)
            {
                for (int i = 0; i < tris.Count; i += 3) (tris[i + 1], tris[i + 2]) = (tris[i + 2], tris[i + 1]);
                mesh.SetTriangles(tris, 0);
            }
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            var go = new GameObject(name + "Corner");
            go.transform.SetParent(t, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = MaterialLibrary.Shared(color, 0.08f);
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        // A flat slab whose top is at topY; the port's slabs are thick quay blocks down into the sea.
        private static void Ground(Transform t, float x0, float x1, float z0, float z1, float topY, Color color, bool sea, string name = "Ground")
        {
            if (x1 - x0 < 0.001f || z1 - z0 < 0.001f) return;
            float depth = sea ? 1.4f : 0.1f;
            var g = Shapes.Cube(t, new Vector3((x0 + x1) / 2f, topY - depth / 2f, (z0 + z1) / 2f), new Vector3(x1 - x0, depth, z1 - z0),
                MaterialLibrary.Shared(color, 0.08f), name); // Road / Pavement / Lot: Validate Layout reads the names
            g.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        private static float Lot(float edge, float depth) => edge + depth / 2f;

        // The toy parking city (Ana Belge §2.1): car parks full of toy cars right round the yard (low, so nothing hides the board),
        // benches and trees on the outer pavement, a corner shop, then houses and flats further out behind and beside.
        // Every piece goes through Place, so nothing stands in another or on the road.
        private static void City(Transform t, float l, float r, float f, float k, float lotEdge, System.Random rng)
        {
            const DistrictKind d = DistrictKind.City;
            float cx = (l + r) / 2f;
            const float park = 4.3f; // two rows of bays
            CarPark(t, l - lotEdge - 7f, r + lotEdge + 7f, k + lotEdge + 0.3f, k + lotEdge + 0.3f + park, true, rng);
            CarPark(t, l - lotEdge - 0.3f - park, l - lotEdge - 0.3f, f - 3f, k + lotEdge, false, rng);
            CarPark(t, r + lotEdge + 0.3f, r + lotEdge + 0.3f + park, f - 3f, k + lotEdge, false, rng);
            CarPark(t, l - lotEdge - 7f, r + lotEdge + 7f, f - lotEdge - 0.3f - park, f - lotEdge - 0.3f, true, rng);
            Trees(t, d, l, r, f, k, lotEdge, rng, false);
            for (float x = l + 1.5f; x <= r - 1.5f; x += 5f) Bench(t, new Vector3(x, -0.05f, k + lotEdge - 0.35f), 180f);
            float back = k + lotEdge + park + 0.8f, side = lotEdge + park + 0.8f;
            Place(d, EnvPiece.Shop, t, new Vector3(l - side - 1.8f, -0.06f, back + 1.6f), 0f, 0);
            for (int row = 0; row < 3; row++)
                for (float x = cx - 20f + (row % 2) * 1.5f; x <= cx + 20f; x += 1.5f)
                    Place(d, CityBlock(rng, row), t, new Vector3(x, -0.06f, back + 1.8f + row * 4.2f), 0f, rng.Next(12));
            for (int col = 0; col < 2; col++)
                for (float z = f - 8f; z <= back + 4f; z += 1.5f)
                {
                    Place(d, CityBlock(rng, col), t, new Vector3(l - side - 1.8f - col * 4.2f, -0.06f, z), -90f, rng.Next(12));
                    Place(d, CityBlock(rng, col), t, new Vector3(r + side + 1.8f + col * 4.2f, -0.06f, z), 90f, rng.Next(12));
                }
            // In front only low things: tree clumps beyond the front car park.
            for (float x = l - 8f; x <= r + 8f; x += 2.2f)
                Place(d, EnvPiece.Tree, t, new Vector3(x + (float)rng.NextDouble(), -0.06f, f - lotEdge - park - 1.6f - (float)rng.NextDouble()), 0f, rng.Next(6));
        }

        // ---------- placing without overlaps (Ana Belge §3, Validate Layout) ----------

        // Footprints (x/z boxes) already used in this district; a new piece that would touch one is left out.
        private static readonly List<Bounds> placed = new List<Bounds>();
        // The yard plus the inner pavement, the road and the outer pavement: nothing static stands there (lamps and the
        // pavement trees are placed on purpose and skip this check).
        private static Rect keepClear;

        private static Bounds Footprint(Transform t)
        {
            var rs = t.GetComponentsInChildren<Renderer>();
            if (rs.Length == 0) return new Bounds(t.position, Vector3.zero);
            var b = rs[0].bounds;
            foreach (var r in rs) b.Encapsulate(r.bounds);
            return b;
        }

        private static bool Clashes(Bounds b, bool checkClear)
        {
            const float gap = 0.06f;
            if (checkClear && b.min.x < keepClear.xMax && b.max.x > keepClear.xMin && b.min.z < keepClear.yMax && b.max.z > keepClear.yMin) return true;
            foreach (var o in placed)
                if (b.min.x + gap < o.max.x && b.max.x - gap > o.min.x && b.min.z + gap < o.max.z && b.max.z - gap > o.min.z) return true;
            return false;
        }

        /// <summary>Spawns a district piece unless it would overlap one already standing (or the road and pavements); returns it or null.</summary>
        private static Transform Place(DistrictKind d, EnvPiece piece, Transform t, Vector3 pos, float yaw, int variant, bool checkClear = true)
        {
            var go = EnvironmentPieces.Spawn(d, piece, t, pos, yaw, variant);
            var b = Footprint(go);
            if (Clashes(b, checkClear))
            {
                Object.Destroy(go.gameObject);
                go.gameObject.SetActive(false);
                return null;
            }
            placed.Add(b);
            return go;
        }

        /// <summary>
        /// A toy car park (Ana Belge §2.1, the parking reference): white bay lines on the light ground and a parked toy car in most
        /// bays, all facing the same aisle. Bays are 1.05 wide and 2.1 deep; <paramref name="alongX"/> = the row runs along x.
        /// </summary>
        private static void CarPark(Transform t, float x0, float x1, float z0, float z1, bool alongX, System.Random rng)
        {
            // In blocks of seven bays, so a road through the area only leaves its own blocks out.
            const float block = 7 * 1.05f;
            float a0 = alongX ? x0 : z0, a1 = alongX ? x1 : z1;
            for (float s = a0; s + 2.1f <= a1; s += block + 0.3f)
            {
                float e = Mathf.Min(s + block, a1);
                if (alongX) CarParkBlock(t, s, e, z0, z1, true, rng);
                else CarParkBlock(t, x0, x1, s, e, false, rng);
            }
        }

        // How full the car parks are (owner 2026-10-31: some sparse, some packed); set per level.
        private static float parkFill = 0.8f;

        private static void CarParkBlock(Transform t, float x0, float x1, float z0, float z1, bool alongX, System.Random rng)
        {
            var area = new Bounds(new Vector3((x0 + x1) / 2f, 0.4f, (z0 + z1) / 2f), new Vector3(x1 - x0, 0.8f, z1 - z0));
            if (Clashes(area, true)) return;
            placed.Add(area);
            var paint = MaterialLibrary.Shared(Color.white, 0.3f);
            float length = alongX ? x1 - x0 : z1 - z0, depth = alongX ? z1 - z0 : x1 - x0;
            int rows = Mathf.Max(1, Mathf.FloorToInt(depth / 2.1f));
            int bays = Mathf.FloorToInt(length / 1.05f);
            for (int row = 0; row < rows; row++)
            for (int i = 0; i <= bays; i++)
            {
                float u = (alongX ? x0 : z0) + i * 1.05f, v = (alongX ? z0 : x0) + row * 2.1f;
                // Bay divider line, 1.8 long, across the row.
                var line = Shapes.Cube(t, alongX ? new Vector3(u, -0.045f, v + 1.05f) : new Vector3(v + 1.05f, -0.045f, u),
                    alongX ? new Vector3(0.07f, 0.01f, 1.8f) : new Vector3(1.8f, 0.01f, 0.07f), paint, "BayLine");
                Shapes.NoShadows(line);
                if (i == bays || ((i * 7 + row * 3) % 10 + 0.5f) / 10f > parkFill) continue; // some bays empty, evenly spread
                var car = new GameObject("ParkedCar").transform;
                car.SetParent(t, false);
                car.localPosition = alongX ? new Vector3(u + 0.525f, -0.05f, v + 1.05f) : new Vector3(v + 1.05f, -0.05f, u + 0.525f);
                // The same toy cars as the traffic (drawn along +Z; owner 2026-10-31 liked them): nose into the bay, the two rows
                // facing each other across the line.
                float yaw = (alongX ? 0f : 90f) + (row % 2 == 0 ? 180f : 0f);
                car.localRotation = Quaternion.Euler(0, yaw, 0);
                EnvironmentPieces.Spawn(DistrictKind.City, EnvPiece.Car, car, Vector3.zero, 0f, rng.Next(12));
            }
        }

        private static void Scaled(Transform piece, float scale)
        {
            if (piece != null) piece.localScale = Vector3.one * scale;
        }

        /// <summary>A chunky toy bench (Ana Belge §2.1 street furniture), on a pavement or a square.</summary>
        private static void Bench(Transform t, Vector3 pos, float yaw)
        {
            var b = new GameObject("Bench").transform;
            b.SetParent(t, false);
            b.localPosition = pos;
            b.localRotation = Quaternion.Euler(0, yaw, 0);
            var wood = MaterialLibrary.Shared(new Color(0.78f, 0.5f, 0.28f), 0.5f);
            var leg = MaterialLibrary.Shared(new Color(0.25f, 0.27f, 0.3f), 0.5f);
            Shapes.Rounded(b, new Vector3(0, 0.26f, 0), new Vector3(0.9f, 0.08f, 0.32f), wood, 0.03f, "Seat");
            Shapes.Rounded(b, new Vector3(0, 0.46f, 0.14f), new Vector3(0.9f, 0.2f, 0.06f), wood, 0.03f, "Back");
            for (int s = -1; s <= 1; s += 2) Shapes.Rounded(b, new Vector3(s * 0.36f, 0.12f, 0), new Vector3(0.06f, 0.24f, 0.28f), leg, 0.02f, "Leg");
        }

        // A mix of houses, flats, small parks and car parks; flats more often further out (they are tall).
        private static EnvPiece CityBlock(System.Random rng, int ring)
        {
            int roll = rng.Next(100);
            if (roll < 12) return EnvPiece.Park;
            if (roll < 22) return EnvPiece.ParkingLot;
            return roll < 22 + 25 + ring * 15 ? EnvPiece.Apartment : EnvPiece.Building;
        }

        private static void Trees(Transform t, DistrictKind d, float l, float r, float f, float k, float lotEdge, System.Random rng, bool front)
        {
            float edge = lotEdge - OuterPavement / 2f;
            for (float x = l - 8f; x <= r + 8f; x += 2.4f)
            {
                Place(d, EnvPiece.Tree, t, new Vector3(x, -0.06f, k + edge), 0f, rng.Next(6), false);
                if (front) Place(d, EnvPiece.Tree, t, new Vector3(x + 1.2f, -0.06f, f - edge - 2.2f), 0f, rng.Next(6), false);
            }
            for (float z = f - 2f; z <= k + 2f; z += 2.4f)
            {
                Place(d, EnvPiece.Tree, t, new Vector3(l - edge, -0.06f, z), 0f, rng.Next(6), false);
                Place(d, EnvPiece.Tree, t, new Vector3(r + edge, -0.06f, z), 0f, rng.Next(6), false);
            }
        }

        // Street lamps on the inner pavement, arms reaching over it toward the yard (none in front: they would hide the board).
        private static void Lamps(Transform t, DistrictKind d, float l, float r, float f, float k, bool sea)
        {
            float edge = Pavement * 0.75f;
            for (float x = l + 1f; x <= r - 0.5f; x += 4f)
                Place(d, EnvPiece.StreetLamp, t, new Vector3(x, -0.045f, k + edge), 0f, 0, false);
            for (float z = f + 1f; z <= k; z += 4f)
            {
                Place(d, EnvPiece.StreetLamp, t, new Vector3(l - edge, -0.045f, z), -90f, 0, false);
                Place(d, EnvPiece.StreetLamp, t, new Vector3(r + edge, -0.045f, z), 90f, 0, false);
            }
        }

        private static void Vehicle(Transform t, DistrictKind d, EnvPiece piece, Vector3[] loop, float speed, float start, System.Random rng)
        {
            var v = EnvironmentPieces.Spawn(d, piece, t, Vector3.zero, 0f, rng.Next(12));
            v.gameObject.AddComponent<EnvVehicle>().Drive(loop, speed, start, true);
            v.localScale = Vector3.one * (piece == EnvPiece.Truck || piece == EnvPiece.ReeferTruck ? 0.8f : 1f);
        }
    }
}
