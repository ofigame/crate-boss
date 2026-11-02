using UnityEngine;

namespace CrateBoss.View
{
    /// <summary>
    /// Simple-shape stand-ins for the surroundings (Ana Belge §2.5) in toy-like colours, until real models
    /// arrive. Front faces -Z. Sizes in cells; buildings are big and soft so the yard stays the brightest thing.
    /// </summary>
    public static class EnvShapes
    {
        private static Color Hex(string hex) => ColorUtility.TryParseHtmlString(hex, out var c) ? c : Color.magenta;

        private static readonly Color[] HouseWalls = { Hex("#F2D8B5"), Hex("#E9B9A0"), Hex("#CFE0E8"), Hex("#F5E7C6"), Hex("#D8E6C8"), Hex("#EBC7C7") };
        private static readonly Color[] Roofs = { Hex("#C8574A"), Hex("#8E5A46"), Hex("#5C6F84"), Hex("#B36A3C") };
        private static readonly Color[] CarColours = { Hex("#E8493F"), Hex("#3D7CC9"), Hex("#F2C232"), Hex("#F4F4F0"), Hex("#4DAF7C") };
        private static readonly Color[] ContainerColours = { Hex("#C9553F"), Hex("#2F6FA8"), Hex("#3E8A5C"), Hex("#D9A23A"), Hex("#7A5C8F"), Hex("#B8BEC4") };
        private static readonly Color Window = Hex("#4A6C8C");
        private static readonly Color Dark = Hex("#2E3236");

        public static void Build(DistrictKind district, EnvPiece piece, Transform t, int v)
        {
            switch (piece)
            {
                case EnvPiece.Building: Building(t, v); break;
                case EnvPiece.Shop: Shop(t); break;
                case EnvPiece.Tree: Tree(t, v); break;
                case EnvPiece.StreetLamp: Lamp(t, district); break;
                case EnvPiece.Car: Car(t, v); break;
                case EnvPiece.Truck: Truck(t, v); break;
                case EnvPiece.ContainerStack: ContainerStack(t, v); break;
                case EnvPiece.Warehouse: Warehouse(t, v); break;
                case EnvPiece.CargoPlane: Plane(t); break;
                case EnvPiece.BaggageTrain: BaggageTrain(t, v); break;
                case EnvPiece.Tower: Tower(t); break;
                case EnvPiece.RunwayLight: RunwayLight(t, v); break;
                case EnvPiece.ColdStorage: ColdStorage(t, v); break;
                case EnvPiece.ReeferTruck: Truck(t, 5); Shapes.Cube(t, new Vector3(0, 1.45f, -1.55f), new Vector3(0.8f, 0.3f, 0.2f), M(Hex("#8A939B")), "Reefer"); break;
                case EnvPiece.CustomsHouse: CustomsHouse(t); break;
                case EnvPiece.Checkpoint: Checkpoint(t); break;
                case EnvPiece.Flag: Flag(t, v); break;
                case EnvPiece.Apartment: Apartment(t, v); break;
                case EnvPiece.Park: Park(t, v); break;
                case EnvPiece.ParkingLot: ParkingLot(t, v); break;
                case EnvPiece.GantryCrane: GantryCrane(t, v); break;
                case EnvPiece.Ship: Ship(t, v); break;
            }
        }

        private static Material M(Color c, float smooth = 0.15f) => MaterialLibrary.Shared(c, smooth);

        // 3 × 3 house, 1–3 floors, gable or flat roof, a door and windows on the front.
        private static void Building(Transform t, int v)
        {
            // Ana Belge §3.2: a house is 2.5–4 tall with its roof, so one or two floors.
            int floors = 1 + v % 2;
            float h = 1.3f + floors * 0.95f;
            var wall = M(HouseWalls[v % HouseWalls.Length]);
            Shapes.Rounded(t, new Vector3(0, h / 2f, 0), new Vector3(3f, h, 2.8f), wall, 0.08f, "Walls");
            var roof = M(Roofs[(v / 2) % Roofs.Length]);
            if (v % 2 == 0)
            {
                for (int s = -1; s <= 1; s += 2)
                {
                    var r = Shapes.Cube(t, new Vector3(0, h + 0.45f, s * 0.72f), new Vector3(3.2f, 0.14f, 1.75f), roof, "Roof");
                    r.transform.localRotation = Quaternion.Euler(s * -32f, 0, 0);
                }
            }
            else Shapes.Cube(t, new Vector3(0, h + 0.08f, 0), new Vector3(3.1f, 0.16f, 2.9f), roof, "Roof");
            var glass = M(Window, 0.7f);
            for (int f = 0; f < floors + 1; f++)
                for (int c = -1; c <= 1; c++)
                {
                    if (f == 0 && c == 0) continue;
                    Shapes.Cube(t, new Vector3(c * 0.95f, 0.75f + f * 0.95f, -1.41f), new Vector3(0.5f, 0.55f, 0.04f), glass, "Window");
                }
            Shapes.Cube(t, new Vector3(0, 0.55f, -1.41f), new Vector3(0.6f, 1.1f, 0.04f), M(Roofs[v % Roofs.Length]), "Door");
        }

        // 3 × 3 block of flats, 2–3 floors (Ana Belge §3.2: at most 4 tall), flat roof with a water tank, balcony bands across the front.
        private static void Apartment(Transform t, int v)
        {
            int floors = 2 + v % 2;
            float h = 0.5f + floors * 0.95f;
            Shapes.Rounded(t, new Vector3(0, h / 2f, 0), new Vector3(3f, h, 2.8f), M(HouseWalls[(v + 3) % HouseWalls.Length]), 0.08f, "Walls");
            Shapes.Cube(t, new Vector3(0, h + 0.06f, 0), new Vector3(3.1f, 0.12f, 2.9f), M(Hex("#8A8F96")), "Roof");
            Shapes.Make(PrimitiveType.Cylinder, t, new Vector3(0.8f, h + 0.45f, 0.5f), new Vector3(0.6f, 0.35f, 0.6f), M(Hex("#B8BEC4"), 0.4f), "Tank");
            var glass = M(Window, 0.7f);
            var rail = M(Hex("#F4F4F0"), 0.3f);
            for (int f = 0; f < floors; f++)
            {
                float y = 0.75f + f * 0.95f;
                for (int c = -1; c <= 1; c++) Shapes.Cube(t, new Vector3(c * 0.95f, y, -1.41f), new Vector3(0.55f, 0.5f, 0.04f), glass, "Window");
                if (f > 0) Shapes.Cube(t, new Vector3(0, y - 0.38f, -1.55f), new Vector3(2.9f, 0.16f, 0.3f), rail, "Balcony");
            }
        }

        // 3 × 3 green square: lawn, trees, a bench and a small fountain.
        private static void Park(Transform t, int v)
        {
            Shapes.Cube(t, new Vector3(0, 0.03f, 0), new Vector3(3.2f, 0.06f, 3f), M(Hex("#7DBF5A")), "Lawn");
            Shapes.Cube(t, new Vector3(0, 0.065f, 0), new Vector3(0.5f, 0.02f, 3f), M(Hex("#E2D2AE")), "Path");
            foreach (var (x, z) in new[] { (-1.1f, -0.9f), (1.1f, 0.8f), (-1.0f, 1.0f), (1.15f, -1.0f) })
            {
                var tree = new GameObject("Tree").transform;
                tree.SetParent(t, false);
                tree.localPosition = new Vector3(x, 0, z);
                Tree(tree, Mathf.Abs(v + (int)(x * 3)));
            }
            var stone = M(Hex("#C9CDD2"), 0.3f);
            Shapes.Make(PrimitiveType.Cylinder, t, new Vector3(0, 0.15f, 0), new Vector3(0.9f, 0.12f, 0.9f), stone, "Fountain");
            Shapes.Make(PrimitiveType.Cylinder, t, new Vector3(0, 0.25f, 0), new Vector3(0.7f, 0.02f, 0.7f), M(Hex("#5FA8D8"), 0.9f), "Water");
            Shapes.Cube(t, new Vector3(-0.6f, 0.2f, 0.45f), new Vector3(0.18f, 0.08f, 0.7f), M(Hex("#8E5A46")), "Bench");
        }

        // 3 × 3 asphalt lot with bay lines and a few parked cars.
        private static void ParkingLot(Transform t, int v)
        {
            Shapes.Cube(t, new Vector3(0, 0.02f, 0), new Vector3(3.2f, 0.04f, 3f), M(Hex("#55595E")), "Asphalt");
            var paint = M(Hex("#F4F4F0"), 0.2f);
            for (int i = 0; i <= 3; i++) Shapes.Cube(t, new Vector3(-1.5f + i * 1f, 0.045f, -0.5f), new Vector3(0.05f, 0.01f, 1.6f), paint, "Line");
            for (int i = 0; i < 3; i++)
            {
                if ((v + i) % 3 == 2) continue; // an empty bay
                var car = new GameObject("Car").transform;
                car.SetParent(t, false);
                car.localPosition = new Vector3(-1f + i, 0.04f, -0.5f);
                car.localScale = Vector3.one * 0.9f;
                Car(car, v + i);
            }
        }

        // Ship-to-shore container crane: legs on the quay, a long boom out over the water, a cab.
        private static void GantryCrane(Transform t, int v)
        {
            var steel = M(v % 2 == 0 ? Hex("#E2563F") : Hex("#3D7CC9"), 0.3f);
            foreach (var x in new[] { -1.3f, 1.3f })
                foreach (var z in new[] { -1f, 1f })
                    Shapes.Cube(t, new Vector3(x, 2.6f, z), new Vector3(0.22f, 5.2f, 0.22f), steel, "Leg");
            Shapes.Cube(t, new Vector3(0, 5.3f, 0), new Vector3(3f, 0.35f, 2.4f), steel, "Portal");
            Shapes.Cube(t, new Vector3(0, 5.8f, -2.5f), new Vector3(0.6f, 0.3f, 9f), steel, "Boom");
            Shapes.Cube(t, new Vector3(0, 7f, 0.6f), new Vector3(0.4f, 2.2f, 0.4f), steel, "Apex");
            Shapes.Cube(t, new Vector3(0, 5.35f, -1.4f), new Vector3(0.8f, 0.6f, 0.8f), M(Hex("#F4F4F0")), "Cab");
        }

        // A container ship along +X: dark hull, white bridge at the stern, rows of containers on deck.
        private static void Ship(Transform t, int v)
        {
            Shapes.Rounded(t, new Vector3(0, 0.4f, 0), new Vector3(12f, 1.4f, 3f), M(Hex("#2E3E52")), 0.4f, "Hull");
            Shapes.Cube(t, new Vector3(0, 0.05f, 0), new Vector3(12.1f, 0.2f, 3.05f), M(Hex("#B8453A")), "Waterline");
            Shapes.Rounded(t, new Vector3(-4.8f, 1.9f, 0), new Vector3(1.6f, 1.8f, 2.4f), M(Hex("#F2F4F5")), 0.1f, "Bridge");
            for (int i = 0; i < 6; i++)
                for (int l = 0; l < 1 + (i + v) % 2; l++)
                {
                    var mat = MaterialLibrary.Create(ContainerColours[(i * 2 + l + v) % ContainerColours.Length], 0.2f);
                    MaterialLibrary.SetTexture(mat, CorrugatedTexture.Get());
                    Shapes.Cube(t, new Vector3(-2.8f + i * 1.35f, 1.6f + l * 0.75f, 0), new Vector3(1.25f, 0.72f, 2.6f), mat, "Container");
                }
        }

        // Corner shop with a striped awning and a sign.
        private static void Shop(Transform t)
        {
            Shapes.Rounded(t, new Vector3(0, 1.3f, 0), new Vector3(3.2f, 2.6f, 2.6f), M(Hex("#F5E7C6")), 0.08f, "Walls");
            Shapes.Cube(t, new Vector3(0, 2.7f, 0), new Vector3(3.3f, 0.2f, 2.7f), M(Hex("#3E8A5C")), "Roof");
            for (int i = 0; i < 6; i++)
                Shapes.Cube(t, new Vector3(-1.4f + i * 0.56f, 1.95f, -1.6f), new Vector3(0.56f, 0.08f, 0.7f), M(i % 2 == 0 ? Hex("#E8493F") : Hex("#F4F4F0")), "Awning");
            Shapes.Cube(t, new Vector3(0, 0.85f, -1.31f), new Vector3(2.2f, 1.2f, 0.04f), M(Window, 0.7f), "ShopWindow");
            Shapes.Cube(t, new Vector3(0, 2.35f, -1.32f), new Vector3(2f, 0.42f, 0.05f), M(Hex("#2F6FA8")), "Sign");
            var text = new GameObject("SignText");
            text.transform.SetParent(t, false);
            text.transform.localPosition = new Vector3(0, 2.35f, -1.36f);
            var tm = text.AddComponent<TextMesh>();
            tm.text = "BAKKAL";
            tm.font = CrateBoss.UI.UiFonts.Title;
            tm.fontSize = 64;
            tm.characterSize = 0.045f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.color = Color.white;
            text.GetComponent<MeshRenderer>().sharedMaterial = tm.font.material;
        }

        private static void Tree(Transform t, int v)
        {
            float s = 0.85f + (v % 3) * 0.15f;
            Shapes.Make(PrimitiveType.Cylinder, t, new Vector3(0, 0.45f * s, 0), new Vector3(0.18f, 0.45f * s, 0.18f), M(Hex("#7A5236")), "Trunk");
            var leaf = M(v % 2 == 0 ? Hex("#6FAF4E") : Hex("#5E9E48"));
            Shapes.Make(PrimitiveType.Sphere, t, new Vector3(0, 1.25f * s, 0), Vector3.one * 1.1f * s, leaf, "Crown");
            Shapes.Make(PrimitiveType.Sphere, t, new Vector3(0.25f * s, 1.65f * s, 0.1f), Vector3.one * 0.75f * s, leaf, "Crown");
        }

        private static void Lamp(Transform t, DistrictKind district)
        {
            var pole = M(Hex("#3C4248"), 0.4f);
            Shapes.Cube(t, new Vector3(0, 1.1f, 0), new Vector3(0.07f, 2.2f, 0.07f), pole, "Pole");
            Shapes.Cube(t, new Vector3(0, 2.18f, -0.22f), new Vector3(0.06f, 0.06f, 0.5f), pole, "Arm");
            var glow = MaterialLibrary.Create(Hex("#FFE7A8"), 0.6f);
            MaterialLibrary.SetGlow(glow, Hex("#FFD27A") * 0.6f);
            Shapes.Cube(t, new Vector3(0, 2.12f, -0.45f), new Vector3(0.22f, 0.07f, 0.16f), glow, "Lamp");
        }

        // Small car along +Z.
        private static void Car(Transform t, int v)
        {
            var body = M(CarColours[v % CarColours.Length], 0.5f);
            Shapes.Rounded(t, new Vector3(0, 0.3f, 0), new Vector3(0.8f, 0.34f, 1.5f), body, 0.1f, "Body");
            Shapes.Rounded(t, new Vector3(0, 0.6f, -0.08f), new Vector3(0.68f, 0.3f, 0.8f), M(Window, 0.7f), 0.08f, "Cabin");
            Wheels(t, 0.42f, 0.5f, 0.14f);
        }

        // Lorry: cab and a container trailer, along +Z.
        private static void Truck(Transform t, int v)
        {
            Shapes.Rounded(t, new Vector3(0, 0.55f, 1.35f), new Vector3(0.95f, 0.9f, 0.8f), M(CarColours[v % CarColours.Length], 0.4f), 0.08f, "Cab");
            Shapes.Cube(t, new Vector3(0, 0.75f, 1.62f), new Vector3(0.8f, 0.32f, 0.04f), M(Window, 0.7f), "Windscreen");
            var trailer = Shapes.Cube(t, new Vector3(0, 0.82f, -0.35f), new Vector3(1f, 1.05f, 2.5f), MaterialLibrary.Create(ContainerColours[v % ContainerColours.Length], 0.2f), "Container");
            MaterialLibrary.SetTexture(trailer.GetComponent<Renderer>().sharedMaterial, CorrugatedTexture.Get());
            Shapes.Cube(t, new Vector3(0, 0.25f, 0), new Vector3(0.9f, 0.1f, 3.4f), M(Dark), "Chassis");
            Wheels(t, 0.46f, 1.2f, 0.17f);
            Wheels(t, 0.46f, -0.9f, 0.17f);
        }

        private static void Wheels(Transform t, float x, float z, float r)
        {
            var tyre = M(Hex("#1C1E21"), 0.1f);
            foreach (var sx in new[] { -x, x })
                foreach (var sz in new[] { -z, z })
                {
                    var w = Shapes.Make(PrimitiveType.Cylinder, t, new Vector3(sx, r, sz), new Vector3(r * 2f, 0.06f, r * 2f), tyre, "Wheel");
                    w.transform.localRotation = Quaternion.Euler(0, 0, 90);
                }
        }

        // Two or three containers high, two wide, 2.6 long.
        private static void ContainerStack(Transform t, int v)
        {
            int levels = 1 + v % 3;
            for (int l = 0; l < levels; l++)
                for (int c = 0; c < 2; c++)
                {
                    var mat = MaterialLibrary.Create(ContainerColours[(v + l * 2 + c) % ContainerColours.Length], 0.2f);
                    MaterialLibrary.SetTexture(mat, CorrugatedTexture.Get());
                    Shapes.Cube(t, new Vector3(c * 1.18f - 0.59f, 0.56f + l * 1.12f, 0), new Vector3(1.1f, 1.1f, 2.6f), mat, "Container");
                }
        }

        // Big corrugated shed with a roller door.
        private static void Warehouse(Transform t, int v)
        {
            var mat = MaterialLibrary.Create(v % 2 == 0 ? Hex("#9FB1C0") : Hex("#B9B2A6"), 0.2f);
            MaterialLibrary.SetTexture(mat, CorrugatedTexture.Get());
            Shapes.Cube(t, new Vector3(0, 1.6f, 0), new Vector3(6f, 3.2f, 4f), mat, "Shed");
            for (int s = -1; s <= 1; s += 2)
            {
                var r = Shapes.Cube(t, new Vector3(0, 3.45f, s * 1.05f), new Vector3(6.2f, 0.14f, 2.3f), M(Hex("#6E7880")), "Roof");
                r.transform.localRotation = Quaternion.Euler(s * -14f, 0, 0);
            }
            Shapes.Cube(t, new Vector3(-1.2f, 1.1f, -2.01f), new Vector3(2.2f, 2.2f, 0.04f), M(Hex("#5C6670")), "Door");
        }

        // Parked cargo plane along +X: fuselage, wings, tail, engines.
        private static void Plane(Transform t)
        {
            var white = M(Hex("#F2F4F5"), 0.5f);
            var blue = M(Hex("#2F6FA8"), 0.4f);
            var body = Shapes.Make(PrimitiveType.Capsule, t, new Vector3(0, 1.9f, 0), new Vector3(1.8f, 6.5f, 1.8f), white, "Fuselage");
            body.transform.localRotation = Quaternion.Euler(0, 0, 90);
            Shapes.Cube(t, new Vector3(0.6f, 1.6f, 0), new Vector3(2.4f, 0.14f, 13f), white, "Wing");
            Shapes.Cube(t, new Vector3(-5.6f, 3.4f, 0), new Vector3(1.6f, 2.6f, 0.14f), blue, "Fin");
            Shapes.Cube(t, new Vector3(-5.8f, 2.3f, 0), new Vector3(1.2f, 0.12f, 4.4f), white, "Tailplane");
            Shapes.Cube(t, new Vector3(0, 1.9f, -0.91f), new Vector3(9f, 0.3f, 0.02f), blue, "Stripe");
            foreach (var z in new[] { -3.2f, 3.2f })
            {
                var e = Shapes.Make(PrimitiveType.Cylinder, t, new Vector3(1.2f, 1.1f, z), new Vector3(0.7f, 0.6f, 0.7f), M(Hex("#B9C0C6"), 0.5f), "Engine");
                e.transform.localRotation = Quaternion.Euler(0, 0, 90);
            }
            foreach (var p in new[] { new Vector3(3.6f, 0.5f, 0), new Vector3(0.6f, 0.5f, -1.4f), new Vector3(0.6f, 0.5f, 1.4f) })
                Shapes.Cube(t, p, new Vector3(0.12f, 1f, 0.12f), M(Dark), "Gear");
        }

        // Tug pulling three carts along +Z.
        private static void BaggageTrain(Transform t, int v)
        {
            Shapes.Rounded(t, new Vector3(0, 0.35f, 1.5f), new Vector3(0.8f, 0.5f, 0.9f), M(Hex("#F2C232"), 0.4f), 0.08f, "Tug");
            Wheels(t, 0.4f, 1.5f + 0.3f, 0.14f);
            int carts = 2 + v % 2;
            for (int i = 0; i < carts; i++)
            {
                float z = 0.6f - i * 1.1f;
                Shapes.Cube(t, new Vector3(0, 0.3f, z), new Vector3(0.8f, 0.08f, 0.95f), M(Hex("#8090A0")), "Cart");
                Shapes.Cube(t, new Vector3(0, 0.55f, z), new Vector3(0.66f, 0.42f, 0.8f), M(ContainerColours[(v + i) % ContainerColours.Length], 0.2f), "Bags");
            }
        }

        private static void Tower(Transform t)
        {
            Shapes.Make(PrimitiveType.Cylinder, t, new Vector3(0, 4f, 0), new Vector3(1.2f, 4f, 1.2f), M(Hex("#E9ECEE")), "Shaft");
            Shapes.Make(PrimitiveType.Cylinder, t, new Vector3(0, 8.6f, 0), new Vector3(2.6f, 0.6f, 2.6f), M(Window, 0.8f), "Cab");
            Shapes.Make(PrimitiveType.Cylinder, t, new Vector3(0, 9.3f, 0), new Vector3(2.9f, 0.12f, 2.9f), M(Hex("#C9CDD1")), "Roof");
            Shapes.Cube(t, new Vector3(0, 10.1f, 0), new Vector3(0.08f, 1.4f, 0.08f), M(Dark), "Mast");
        }

        // White cold-store shed with blue trim, two big doors and a little steam (white puffs) at the top of each.
        private static void ColdStorage(Transform t, int v)
        {
            var white = M(Hex("#F2F5F8"), 0.3f);
            var blue = M(Hex("#4E7FB8"), 0.3f);
            float h = 3.2f + (v % 2) * 0.8f;
            Shapes.Cube(t, new Vector3(0, h / 2f, 0), new Vector3(6f, h, 4f), white, "Shed");
            Shapes.Cube(t, new Vector3(0, h + 0.1f, 0), new Vector3(6.1f, 0.2f, 4.1f), blue, "Trim");
            var steam = MaterialLibrary.Unlit(new Color(1f, 1f, 1f, 0.55f));
            foreach (var x in new[] { -1.4f, 1.4f })
            {
                Shapes.Cube(t, new Vector3(x, 1.1f, -2.01f), new Vector3(1.8f, 2.2f, 0.04f), M(Hex("#B7C6D4"), 0.4f), "Door");
                Shapes.NoShadows(Shapes.Make(PrimitiveType.Sphere, t, new Vector3(x, 2.4f, -2.2f), new Vector3(1.2f, 0.4f, 0.4f), steam, "Steam"));
            }
            Shapes.Cube(t, new Vector3(0, 0.05f, -2.4f), new Vector3(6.2f, 0.1f, 0.6f), M(Color.white, 0.05f), "SnowKerb");
        }

        // Customs house: two floors, a flat roof, a sign band.
        private static void CustomsHouse(Transform t)
        {
            Shapes.Rounded(t, new Vector3(0, 1.6f, 0), new Vector3(7f, 3.2f, 3.4f), M(Hex("#E9E2D3")), 0.08f, "Building");
            Shapes.Cube(t, new Vector3(0, 3.3f, 0), new Vector3(7.2f, 0.2f, 3.6f), M(Hex("#7A5C4A")), "Roof");
            Shapes.Cube(t, new Vector3(0, 2.6f, -1.72f), new Vector3(4f, 0.5f, 0.05f), M(Hex("#2F4F7A")), "SignBand");
            var glass = M(Window, 0.7f);
            for (int f = 0; f < 2; f++)
                for (int c = -2; c <= 2; c++)
                    if (!(f == 0 && c == 0)) Shapes.Cube(t, new Vector3(c * 1.3f, 0.8f + f * 1.4f, -1.72f), new Vector3(0.7f, 0.7f, 0.04f), glass, "Window");
            Shapes.Cube(t, new Vector3(0, 0.6f, -1.72f), new Vector3(1f, 1.2f, 0.04f), M(Hex("#5A4636")), "Door");
            var text = new GameObject("SignText");
            text.transform.SetParent(t, false);
            text.transform.localPosition = new Vector3(0, 2.6f, -1.76f);
            var tm = text.AddComponent<TextMesh>();
            tm.text = "GÜMRÜK";
            tm.font = CrateBoss.UI.UiFonts.Title;
            tm.fontSize = 64;
            tm.characterSize = 0.05f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.color = Color.white;
            text.GetComponent<MeshRenderer>().sharedMaterial = tm.font.material;
        }

        // Checkpoint booth with a red-white barrier arm across the lane.
        private static void Checkpoint(Transform t)
        {
            Shapes.Rounded(t, new Vector3(0, 0.7f, 0), new Vector3(1.2f, 1.4f, 1.2f), M(Hex("#E9ECEE")), 0.06f, "Booth");
            Shapes.Cube(t, new Vector3(0, 1.45f, 0), new Vector3(1.5f, 0.1f, 1.5f), M(Hex("#2F4F7A")), "Roof");
            Shapes.Cube(t, new Vector3(0, 0.95f, -0.61f), new Vector3(0.8f, 0.4f, 0.02f), M(Window, 0.7f), "Window");
            for (int i = 0; i < 6; i++)
                Shapes.Cube(t, new Vector3(0.8f + i * 0.4f, 0.8f, 0), new Vector3(0.4f, 0.1f, 0.1f), M(i % 2 == 0 ? Hex("#E8493F") : Color.white, 0.4f), "Arm");
        }

        // Flag pole with a flag (alternating colours).
        private static void Flag(Transform t, int v)
        {
            Shapes.Cube(t, new Vector3(0, 2f, 0), new Vector3(0.07f, 4f, 0.07f), MaterialLibrary.Shared(Hex("#C9CDD1"), 0.6f, 0.6f), "Pole");
            var colour = v % 2 == 0 ? Hex("#D33A35") : Hex("#2F6FA8");
            Shapes.Cube(t, new Vector3(0.45f, 3.6f, 0), new Vector3(0.9f, 0.6f, 0.03f), M(colour, 0.2f), "Cloth");
        }

        private static void RunwayLight(Transform t, int v)
        {
            Shapes.Cube(t, new Vector3(0, 0.08f, 0), new Vector3(0.1f, 0.16f, 0.1f), M(Dark), "Base");
            var glow = MaterialLibrary.Create(v % 2 == 0 ? Hex("#7FB6FF") : Hex("#FFFFFF"), 0.6f);
            MaterialLibrary.SetGlow(glow, (v % 2 == 0 ? Hex("#7FB6FF") : Hex("#FFFFFF")) * 1.6f);
            Shapes.SmallSphere(t, new Vector3(0, 0.2f, 0), Vector3.one * 0.13f, glow, "Light");
        }
    }
}
