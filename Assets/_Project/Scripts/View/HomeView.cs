using System.Collections;
using System.Collections.Generic;
using CrateBoss.Meta;
using UnityEngine;

namespace CrateBoss.View
{
    /// <summary>
    /// The Boss's land, the home screen (Ana Belge §11.2): the toy city ground with roads, the open lands (grass, a pond or the
    /// sea), the paths and items the player placed, the pile of waiting delivery crates and the waving worker. Edit mode shows
    /// a thin grid and a green / red ghost of the item being placed. Cell (land, x, y) → world: x right, rows toward the camera.
    /// </summary>
    public class HomeView : MonoBehaviour
    {
        private Transform content, itemsRoot, ghost, gridRoot;
        private Material ghostOk, ghostBad;
        private SaveData data;
        private ItemCatalog catalog;
        private Transform pile;
        private WorkerView worker;
        public Bounds Area { get; private set; }

        /// <summary>The open lands with the road in front and the crate pile: what the home camera frames (Ana Belge §11.2).</summary>
        public Bounds OpenArea
        {
            get
            {
                if (!CityShown) return Area;
                Bounds b = default; bool any = false;
                foreach (var land in LandCatalog.Lands)
                {
                    if (!data.lands.Contains(land.Id)) continue;
                    var c0 = CellWorld(land, -1.5f, -0.5f);
                    var c1 = CellWorld(land, land.W + 2.5f, land.D + 2.5f);
                    if (!any) { b = new Bounds(c0, Vector3.zero); any = true; }
                    b.Encapsulate(c0); b.Encapsulate(c1);
                }
                if (!any) { var s = LandCatalog.Get("start"); b = new Bounds(CellWorld(s, s.W / 2f, s.D / 2f), new Vector3(s.W + 4f, 0, s.D + 4f)); }
                return b;
            }
        }

        private static Color Hex(string hex) => ColorUtility.TryParseHtmlString(hex, out var c) ? c : Color.magenta;

        public static Vector3 CellWorld(LandDef land, float x, float y) => new Vector3(land.OffsetX + x, 0f, -(land.OffsetY + y));

        /// <summary>
        /// Owner 2026-11-01 "kasalar ile boss'un şehrini kurma hiç güzel olmamış … şimdilik gizli kalsın": while false the home
        /// screen is only the delivery crate with the worker sitting on it; the city code stays for later.
        /// </summary>
        public static readonly bool CityShown = false;

        /// <summary>The crate and the sitting worker (the home camera must show all of it).</summary>
        public Bounds Figure { get; private set; }

        /// <summary>Everything of the home stage (left out of the backdrop picture).</summary>
        public GameObject Stage => content != null ? content.gameObject : gameObject;

        private Transform stageGround;

        /// <summary>The paved square goes with the city once the soft picture stands behind (BackdropBlur).</summary>
        public void HideGround() { if (stageGround != null) stageGround.gameObject.SetActive(false); }

        public void Build(SaveData save, ItemCatalog items)
        {
            data = save;
            catalog = items;
            if (content != null) Destroy(content.gameObject);
            if (stageGround != null) Destroy(stageGround.gameObject);
            content = new GameObject("Home").transform;
            content.SetParent(transform, false);
            if (!CityShown) { BuildStage(); return; }
            ghostOk = MaterialLibrary.Unlit(new Color(0.24f, 0.8f, 0.42f, 0.55f));
            ghostBad = MaterialLibrary.Unlit(new Color(0.94f, 0.27f, 0.27f, 0.55f));

            var b = new Bounds(Vector3.zero, Vector3.zero);
            bool any = false;
            foreach (var land in LandCatalog.Lands)
            {
                var c0 = CellWorld(land, -0.5f, -0.5f);
                var c1 = CellWorld(land, land.W - 0.5f, land.D + 3f);
                if (!any) { b = new Bounds((c0 + c1) / 2f, Vector3.zero); any = true; }
                b.Encapsulate(c0); b.Encapsulate(c1);
            }
            b.Expand(new Vector3(8f, 0f, 8f));
            Area = b;
            // City ground and roads in front of every land.
            Shapes.Cube(content, new Vector3(b.center.x, -0.08f, b.center.z), new Vector3(b.size.x + 30f, 0.1f, b.size.z + 30f), MaterialLibrary.Shared(Hex("#E4E6EA"), 0.3f), "CityGround");
            foreach (var land in LandCatalog.Lands)
            {
                var front = CellWorld(land, (land.W - 1) / 2f, land.D + 1.2f);
                Shapes.Cube(content, front + Vector3.down * 0.02f, new Vector3(land.W + 4f, 0.04f, 2.6f), MaterialLibrary.Shared(Hex("#4E5462"), 0.2f), "Road");
                for (int i = 0; i < land.W + 3; i += 2)
                    Shapes.Cube(content, front + new Vector3(-land.W / 2f - 1.5f + i, 0.005f, 0), new Vector3(0.8f, 0.01f, 0.12f), MaterialLibrary.Shared(Color.white, 0.2f), "Dash");
                BuildLand(land);
            }
            itemsRoot = new GameObject("Items").transform;
            itemsRoot.SetParent(content, false);
            RefreshItems();
            BuildPile();
            var start = LandCatalog.Get("start");
            if (WorkerView.Available)
            {
                worker = new GameObject("Worker").AddComponent<WorkerView>();
                worker.transform.SetParent(content, false);
                worker.transform.position = CellWorld(start, -1.2f, start.D - 1f);
                worker.Build();
                worker.Face(Core.Direction.Down);
            }
        }

        /// <summary>The home stage is a paved square of this many cells; the game builds the city round it (a full car park).</summary>
        public const int StageSize = 5;
        /// <summary>Where the crate stands: toward the back of the square, so nothing of the city stands in front of it.</summary>
        public static readonly Vector3 StageCenter = new Vector3(2f, 0f, 3f);

        // The home stage: the delivery crate on the paved square and the worker sitting on it, swinging his legs. Round it
        // stands the city with a full car park (WorldEnvironment, built by the game), blurred behind.
        void BuildStage()
        {
            content.position = StageCenter;
            // The paved square: in the soft picture only (put away with the city once the picture stands).
            stageGround = new GameObject("StageGround").transform;
            stageGround.SetParent(transform, false);
            Shapes.Cube(stageGround, new Vector3((StageSize - 1) / 2f, -0.07f, (StageSize - 1) / 2f), new Vector3(StageSize + 0.4f, 0.06f, StageSize + 0.4f),
                MaterialLibrary.Shared(Hex("#D9DCE2"), 0.3f), "Square");
            // Only the crate and the worker stay sharp; the ground all round is in the soft picture. A soft dark disc under the
            // crate stands it on that ground.
            var blob = Shapes.Quad(content, new Vector3(0f, 0.01f, 0f), new Vector3(1.7f, 1.7f, 1f), Quaternion.Euler(90f, 0f, 0f),
                MaterialLibrary.Unlit(new Color(0f, 0f, 0f, 0.28f), UI.IconFactory.Circle.texture), "Shadow");
            Shapes.NoShadows(blob);
            var crate = new GameObject("Crate").transform;
            crate.SetParent(content, false);
            float height = BoxModelFactory.NormalSize, depth = BoxModelFactory.NormalSize;
            if (VisualCatalog.TryInstantiateRanked("BoxDelivery", crate) is GameObject model && VisualCatalog.TryGetBounds("BoxDelivery", out var b))
            {
                height = b.size.y;
                depth = Mathf.Min(b.size.x, b.size.z);
            }
            else
                Shapes.Rounded(crate, Vector3.up * height / 2f, Vector3.one * height, MaterialLibrary.Shared(Hex("#EF4444"), 0.5f), 0.1f, "Crate");
            Area = new Bounds(StageCenter, new Vector3(3f, 0f, 3f));
            Figure = new Bounds(StageCenter + new Vector3(0f, 0.85f, 0.1f), new Vector3(1.1f, 1.7f, 1.1f));
            if (!WorkerView.Available) return;
            worker = new GameObject("Worker").AddComponent<WorkerView>();
            worker.transform.SetParent(content, false);
            // Facing the camera (it looks along its yaw), sitting at the front of the lid so the knees clear the edge and the
            // shins hang in front of the crate.
            var camera = Camera.main != null ? Camera.main.GetComponent<CameraFramer>() : null;
            float yaw = (camera != null ? camera.yaw : 25f) + 180f;
            var facing = Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
            worker.transform.localPosition = facing * (depth / 2f - 0.04f);
            worker.Build();
            worker.Sit(height, facing);
        }

        void BuildLand(LandDef land)
        {
            bool open = data.lands.Contains(land.Id);
            var root = new GameObject("Land " + land.Id).transform;
            root.SetParent(content, false);
            var grass = MaterialLibrary.Shared(open ? Hex("#7DBF5A") : Hex("#C9CDD3"), 0.3f);
            var water = MaterialLibrary.Shared(Hex("#5AA9F0"), 0.9f);
            for (int y = 0; y < land.D; y++)
            for (int x = 0; x < land.W; x++)
            {
                bool w = land.IsWater(x, y);
                Shapes.Cube(root, CellWorld(land, x, y) + Vector3.down * 0.02f, new Vector3(1f, 0.06f, 1f), w ? water : grass, w ? "Water" : "Grass");
            }
            // A low kerb round the plot; a closed land gets a "locked" fence of posts.
            var kerb = MaterialLibrary.Shared(Hex("#F7F7F9"), 0.4f);
            var a = CellWorld(land, -0.5f, -0.5f);
            var c = CellWorld(land, land.W - 0.5f, land.D - 0.5f);
            var mid = (a + c) / 2f;
            Shapes.Cube(root, new Vector3(mid.x, 0.04f, a.z), new Vector3(land.W, 0.1f, 0.12f), kerb, "Kerb");
            Shapes.Cube(root, new Vector3(mid.x, 0.04f, c.z), new Vector3(land.W, 0.1f, 0.12f), kerb, "Kerb");
            Shapes.Cube(root, new Vector3(a.x, 0.04f, mid.z), new Vector3(0.12f, 0.1f, land.D), kerb, "Kerb");
            Shapes.Cube(root, new Vector3(c.x, 0.04f, mid.z), new Vector3(0.12f, 0.1f, land.D), kerb, "Kerb");
            if (!open)
                for (int i = 0; i <= land.W; i += 2)
                    Shapes.Rounded(root, new Vector3(a.x + i, 0.3f, c.z), new Vector3(0.14f, 0.6f, 0.14f), MaterialLibrary.Shared(Hex("#FFC93C"), 0.5f), 0.04f, "LockPost");
        }

        /// <summary>Paths and placed items, again from the save.</summary>
        public void RefreshItems()
        {
            if (itemsRoot == null) return;
            for (int i = itemsRoot.childCount - 1; i >= 0; i--) Destroy(itemsRoot.GetChild(i).gameObject);
            var path = MaterialLibrary.Shared(Hex("#E2D2AE"), 0.3f);
            foreach (var key in data.paths)
            {
                var p = key.Split(':');
                var land = LandCatalog.Get(p[0]);
                if (land == null) continue;
                Shapes.Cube(itemsRoot, CellWorld(land, int.Parse(p[1]), int.Parse(p[2])) + Vector3.up * 0.015f, new Vector3(0.98f, 0.03f, 0.98f), path, "Path");
            }
            foreach (var placed in data.placed)
            {
                var item = catalog.Get(placed.id);
                var land = LandCatalog.Get(placed.land);
                if (item == null || land == null) continue;
                var t = ItemShapes.Build(itemsRoot, item);
                Pose(t, land, item, placed.x, placed.y, placed.rot);
            }
        }

        static void Pose(Transform t, LandDef land, ItemDef item, int x, int y, int rot)
        {
            var (w, d) = HomeRules.Size(item, rot);
            t.position = CellWorld(land, x + (w - 1) / 2f, y + (d - 1) / 2f);
            t.rotation = Quaternion.Euler(0, rot * 90f, 0); // rot 0: front toward the road (−z)
        }

        // ---------- crate pile (§11.2) ----------

        void BuildPile()
        {
            var start = LandCatalog.Get("start");
            pile = new GameObject("CratePile").transform;
            pile.SetParent(content, false);
            pile.position = CellWorld(start, start.W + 0.3f, start.D - 1.2f);
            RefreshPile();
        }

        public void RefreshPile()
        {
            if (pile == null) return;
            for (int i = pile.childCount - 1; i >= 0; i--) Destroy(pile.GetChild(i).gameObject);
            int n = Mathf.Min(6, data.crates + data.promoCrates.Count);
            for (int i = 0; i < n; i++)
            {
                var crate = new GameObject("Crate").transform;
                crate.SetParent(pile, false);
                crate.localPosition = new Vector3((i % 2) * 0.9f, 0.45f + (i / 2) * 0.82f, (i / 2 % 2) * 0.2f);
                if (VisualCatalog.TryInstantiateRanked("BoxDelivery", crate) is GameObject m) m.transform.localPosition = new Vector3(0, -0.45f, 0);
                else Shapes.Rounded(crate, Vector3.zero, Vector3.one * 0.8f, MaterialLibrary.Shared(Hex("#EF4444"), 0.5f), 0.1f, "Crate");
            }
        }

        public Vector3 PileTop => pile == null ? Vector3.zero : pile.position + Vector3.up * (0.6f + Mathf.Min(3, (data.crates + data.promoCrates.Count + 1) / 2) * 0.82f);

        /// <summary>A delivery truck rolls up, a crate drops onto the pile and bounces (Ana Belge §11.3).</summary>
        public IEnumerator DeliverCrate(EffectsView effects)
        {
            if (!CityShown) yield break;
            var truck = new GameObject("DeliveryTruck").transform;
            truck.SetParent(content, false);
            var start = LandCatalog.Get("start");
            var stop = CellWorld(start, start.W + 0.6f, start.D + 1.2f);
            ObstacleBuilder.ToyCar(truck, Vector3.zero, false, MaterialLibrary.Shared(Hex("#EF4444"), 0.5f), 3);
            truck.localScale = Vector3.one * 1.3f;
            for (float t = 0; t < 0.9f; t += Time.deltaTime) { truck.position = Vector3.Lerp(stop + Vector3.right * 14f, stop, 1f - Mathf.Pow(1f - t / 0.9f, 3)); yield return null; }
            truck.position = stop;
            RefreshPile();
            var top = pile.childCount > 0 ? pile.GetChild(pile.childCount - 1) : null;
            if (top != null)
            {
                var rest = top.localPosition;
                for (float t = 0; t < 0.35f; t += Time.deltaTime) { top.localPosition = rest + Vector3.up * (2.5f * (1f - t / 0.35f) * (1f - t / 0.35f)); yield return null; }
                top.localPosition = rest;
                effects?.ToyClouds(top.position + Vector3.down * 0.4f, 3, 0.25f);
            }
            for (float t = 0; t < 0.8f; t += Time.deltaTime) { truck.position = Vector3.Lerp(stop, stop + Vector3.left * 16f, t / 0.8f * t / 0.8f); yield return null; }
            Destroy(truck.gameObject);
        }

        // ---------- edit mode (§12.4) ----------

        public void ShowGrid(bool on)
        {
            if (gridRoot != null) Destroy(gridRoot.gameObject);
            if (!on) return;
            gridRoot = new GameObject("Grid").transform;
            gridRoot.SetParent(content, false);
            var line = MaterialLibrary.Unlit(new Color(1f, 1f, 1f, 0.35f));
            foreach (var land in LandCatalog.Lands)
            {
                if (!data.lands.Contains(land.Id)) continue;
                for (int x = 0; x <= land.W; x++) Shapes.Cube(gridRoot, CellWorld(land, x - 0.5f, (land.D - 1) / 2f) + Vector3.up * 0.03f, new Vector3(0.03f, 0.005f, land.D), line, "GridX");
                for (int y = 0; y <= land.D; y++) Shapes.Cube(gridRoot, CellWorld(land, (land.W - 1) / 2f, y - 0.5f) + Vector3.up * 0.03f, new Vector3(land.W, 0.005f, 0.03f), line, "GridY");
            }
        }

        /// <summary>The item being placed, over its footprint (green: allowed, red: not).</summary>
        public void ShowGhost(ItemDef item, LandDef land, int x, int y, int rot, bool ok)
        {
            HideGhost();
            ghost = new GameObject("Ghost").transform;
            ghost.SetParent(content, false);
            var model = ItemShapes.Build(ghost, item);
            Pose(model, land, item, x, y, rot);
            var (w, d) = HomeRules.Size(item, rot);
            var pad = Shapes.Cube(ghost, CellWorld(land, x + (w - 1) / 2f, y + (d - 1) / 2f) + Vector3.up * 0.04f, new Vector3(w, 0.02f, d), ok ? ghostOk : ghostBad, "Footprint");
            Shapes.NoShadows(pad);
        }

        public void HideGhost()
        {
            if (ghost != null) Destroy(ghost.gameObject);
            ghost = null;
        }

        /// <summary>The open land cell under a screen point, or null.</summary>
        public (LandDef land, int x, int y)? CellAtScreen(Camera cam, Vector2 screen)
        {
            var ray = cam.ScreenPointToRay(screen);
            if (Mathf.Abs(ray.direction.y) < 1e-4f) return null;
            var hit = ray.origin + ray.direction * (-ray.origin.y / ray.direction.y);
            foreach (var land in LandCatalog.Lands)
            {
                if (!data.lands.Contains(land.Id)) continue;
                int x = Mathf.RoundToInt(hit.x - land.OffsetX), y = Mathf.RoundToInt(-hit.z - land.OffsetY);
                if (land.InBounds(x, y)) return (land, x, y);
            }
            return null;
        }

        /// <summary>The delivery-crate pile was tapped.</summary>
        public bool PileAtScreen(Camera cam, Vector2 screen)
        {
            if (pile == null) return false;
            var ray = cam.ScreenPointToRay(screen);
            var b = new Bounds(pile.position + Vector3.up * 1f, new Vector3(2.4f, 2.6f, 2.4f));
            return b.IntersectRay(ray);
        }
    }
}
