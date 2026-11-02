using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CrateBoss.View;
using UnityEngine;

namespace CrateBoss.Game
{
    /// <summary>
    /// Ana Belge §3.5 "Validate Layout": run the player with "-layoutcheck &lt;report.md&gt;" (optionally "-levels &lt;folder&gt;"). Every level
    /// is built as in play and the scene is checked: nothing floats, nothing overlaps, nothing but traffic stands on the road,
    /// the crane foundation stands on open ground, nothing tall hides a yard cell from the camera, sizes follow the §4.1 table.
    /// Bounds are the renderers' boxes, so the checks are on the safe side (a tree's box is bigger than the tree).
    /// </summary>
    public class LayoutValidator : MonoBehaviour
    {
        private const float Tolerance = 0.05f;
        private string reportPath;

        private enum Kind { Building, Tree, Lamp, Prop, Flat, Traffic }

        private struct Item
        {
            public string Name;
            public Kind Kind;
            public Bounds Box;
            /// <summary>The parts touching the ground (a tree's trunk, not its crown): what stands on the road.</summary>
            public Bounds Foot;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void StartIfAsked()
        {
            var args = System.Environment.GetCommandLineArgs();
            int i = System.Array.IndexOf(args, "-layoutcheck");
            if (i < 0 || i >= args.Length - 1) return;
            new GameObject("LayoutValidator").AddComponent<LayoutValidator>().reportPath = args[i + 1];
        }

        private IEnumerator Start()
        {
            var game = FindAnyObjectByType<GameController>();
            yield return new WaitForSeconds(1.2f);
            var report = new StringBuilder();
            report.AppendLine("# Yerleşim raporu (Validate Layout, Ana Belge §3.5)");
            report.AppendLine();
            report.AppendLine($"{System.DateTime.Now:dd.MM.yyyy HH:mm}. Kutular çizim sınırlarıdır (ağacın kutusu ağaçtan büyüktür): şüpheli durum hata sayılır.");
            report.AppendLine();
            report.AppendLine("| Bölüm | Hata | Uyarı | İlk bulgular |");
            report.AppendLine("|---|---|---|---|");
            int totalErrors = 0;
            for (int level = 0; level < game.LevelCount; level++)
            {
                game.LoadLevel(level);
                yield return new WaitForSeconds(1.5f); // the intro has landed, the camera is framed
                var errors = new List<string>();
                var warnings = new List<string>();
                Check(errors, warnings);
                totalErrors += errors.Count;
                var shown = new List<string>(errors);
                shown.AddRange(warnings);
                string first = shown.Count == 0 ? "—" : string.Join("; ", shown.GetRange(0, Mathf.Min(4, shown.Count))) + (shown.Count > 4 ? " …" : "");
                report.AppendLine($"| {level + 1} | {errors.Count} | {warnings.Count} | {first.Replace("|", "/")} |");
                Debug.Log($"[Layout] level {level + 1}: {errors.Count} errors, {warnings.Count} warnings");
            }
            report.AppendLine();
            report.AppendLine(totalErrors == 0 ? "**Sonuç: hata yok.**" : $"**Sonuç: {totalErrors} hata.** Hatalı bölüm onaya sunulmaz.");
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath));
            File.WriteAllText(reportPath, report.ToString(), new UTF8Encoding(false));
            Debug.Log($"[Layout] done, {totalErrors} errors -> {reportPath}");
            Application.Quit();
        }

        private static void Check(List<string> errors, List<string> warnings)
        {
            var items = new List<Item>();
            var roads = new List<Bounds>();
            var pavements = new List<Bounds>();
            var district = GameObject.Find("District");
            if (district != null)
                foreach (Transform t in district.transform)
                {
                    if (t.name == "Road") { roads.Add(BoundsOf(t)); continue; }
                    if (t.name == "Pavement") { pavements.Add(BoundsOf(t)); continue; }
                    if (!System.Enum.TryParse<EnvPiece>(t.name, out var piece)) continue;
                    items.Add(new Item { Name = t.name, Kind = KindOf(piece), Box = BoundsOf(t), Foot = FootOf(t) });
                }
            var foundations = new List<Bounds>();
            foreach (var t in FindObjectsByType<Transform>(FindObjectsSortMode.None))
                if (t.name == "Foundation" && t.parent != null && t.parent.name.StartsWith("Crane")) foundations.Add(BoundsOf(t));
            var posts = new List<Bounds>();
            var barrier = GameObject.Find("Barrier");
            if (barrier != null)
                foreach (Transform t in barrier.transform)
                    if (t.name == "Post") posts.Add(BoundsOf(t));

            foreach (var it in items)
            {
                if (it.Kind == Kind.Traffic || it.Kind == Kind.Flat) continue;
                // 1. floating: the district ground's top is at -0.06 .. 0
                if (it.Box.min.y > Tolerance) errors.Add($"havada: {it.Name} ({it.Box.min.y:0.00})");
                // 3. on the road
                foreach (var r in roads)
                    if (OverlapXZ(it.Foot, r, 0.02f)) { errors.Add($"yolda: {it.Name} @ {Where(it.Box)}"); break; }
                // 6. scale (§4.1)
                float height = it.Box.size.y;
                if (it.Kind == Kind.Tree && (height < 1.5f * 0.85f || height > 2.5f * 1.15f)) warnings.Add($"ölçek: ağaç boyu {height:0.0}");
                if (it.Kind == Kind.Lamp && Mathf.Abs(height - 2.2f) > 2.2f * 0.15f) warnings.Add($"ölçek: lamba boyu {height:0.0}");
                if (it.Kind == Kind.Building && (height < 2.5f * 0.85f || height > 4f * 1.15f)) warnings.Add($"ölçek: {it.Name} yüksekliği {height:0.0}");
            }
            // 2. overlapping pieces (traffic moves along its lane; flat lots may hold their own props)
            for (int a = 0; a < items.Count; a++)
            for (int b = a + 1; b < items.Count; b++)
            {
                if (items[a].Kind == Kind.Traffic || items[b].Kind == Kind.Traffic) continue;
                if (Overlap(items[a].Box, items[b].Box, Tolerance)) errors.Add($"iç içe: {items[a].Name} + {items[b].Name} @ {Where(items[a].Box)}");
            }
            // 4. crane foundation on open ground
            foreach (var f in foundations)
            {
                foreach (var r in roads) if (OverlapXZ(f, r, 0.02f)) { errors.Add($"vinç temeli yolda @ {Where(f)}"); break; }
                foreach (var p in pavements) if (OverlapXZ(f, p, 0.02f)) { errors.Add($"vinç temeli kaldırımda @ {Where(f)}"); break; }
                foreach (var p in posts) if (OverlapXZ(f, p, 0f)) { errors.Add("vinç temeli bariyerde"); break; }
                foreach (var it in items) if (it.Kind == Kind.Building && OverlapXZ(f, it.Box, 0f)) { errors.Add($"vinç temeli binada ({it.Name})"); break; }
            }
            // 5. nothing tall in front of a yard cell
            var cam = Camera.main;
            if (cam != null)
                foreach (var tile in FindObjectsByType<TileView>(FindObjectsSortMode.None))
                {
                    if (tile.Type == Core.TileType.Wall || tile.Type == Core.TileType.Void) continue;
                    var cell = tile.transform.position + Vector3.up * 0.4f;
                    var ray = new Ray(cam.transform.position, (cell - cam.transform.position).normalized);
                    float dist = Vector3.Distance(cam.transform.position, cell);
                    foreach (var it in items)
                    {
                        if (it.Kind == Kind.Flat || it.Kind == Kind.Traffic) continue;
                        if (it.Box.IntersectRay(ray, out float hit) && hit < dist - 0.5f) { errors.Add($"sahayı kapatıyor: {it.Name} → kare {tile.Cell}"); break; }
                    }
                }
        }

        private static Kind KindOf(EnvPiece p)
        {
            switch (p)
            {
                case EnvPiece.Tree: return Kind.Tree;
                case EnvPiece.StreetLamp: case EnvPiece.RunwayLight: return Kind.Lamp;
                case EnvPiece.Car: case EnvPiece.Truck: case EnvPiece.ReeferTruck: case EnvPiece.BaggageTrain: return Kind.Traffic;
                case EnvPiece.Park: case EnvPiece.ParkingLot: return Kind.Flat;
                case EnvPiece.Building: case EnvPiece.Shop: case EnvPiece.Apartment: case EnvPiece.Warehouse: case EnvPiece.ColdStorage:
                case EnvPiece.CustomsHouse: case EnvPiece.Tower: return Kind.Building;
                default: return Kind.Prop;
            }
        }

        private static Bounds BoundsOf(Transform t)
        {
            var renderers = t.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return new Bounds(t.position, Vector3.zero);
            var b = renderers[0].bounds;
            foreach (var r in renderers) b.Encapsulate(r.bounds);
            return b;
        }

        private static Bounds FootOf(Transform t)
        {
            var all = BoundsOf(t);
            Bounds? foot = null;
            foreach (var r in t.GetComponentsInChildren<Renderer>())
            {
                if (r.bounds.min.y > all.min.y + 0.25f) continue;
                if (foot.HasValue) { var b = foot.Value; b.Encapsulate(r.bounds); foot = b; }
                else foot = r.bounds;
            }
            return foot ?? all;
        }

        private static bool Overlap(Bounds a, Bounds b, float shrink)
        {
            a.Expand(-2f * shrink);
            b.Expand(-2f * shrink);
            return a.Intersects(b);
        }

        private static bool OverlapXZ(Bounds a, Bounds b, float shrink) =>
            a.min.x + shrink < b.max.x && a.max.x - shrink > b.min.x && a.min.z + shrink < b.max.z && a.max.z - shrink > b.min.z;

        private static string Where(Bounds b) => $"({b.center.x:0.0}, {b.center.z:0.0})";
    }
}
