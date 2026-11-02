using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Rendering;

namespace CrateBoss.EditorTools
{
    /// <summary>
    /// Tools > Crate Boss > Normalize Models (Ana Belge §19), safe to run again:
    /// import settings, texture sizes (ASTC on phones), URP Lit materials with a metallic/smoothness map made from the
    /// Tripo metallic + roughness maps, and wrapper prefabs in Resources/Models (pivot at the bottom centre, sized from
    /// the model's own bounds, never stretched). The Mixamo worker gets a Humanoid avatar, its clips and an animator.
    /// Writes Docs/MODEL_RAPORU.md.
    /// </summary>
    public static class ModelSetup
    {
        private const string ModelsDir = "Assets/_Project/Art/Models";
        private const string WorkerDir = "Assets/_Project/Art/Characters/Worker";
        private const string WorkerFbx = WorkerDir + "/worker.fbx";
        private const string ControllerPath = WorkerDir + "/WorkerAnimator.controller";
        private const string PrefabDir = "Assets/_Project/Resources/Models";
        private const string ReportPath = "Docs/MODEL_RAPORU.md";
        public const float CraneJibHeight = 4.6f; // CraneView.JibHeight
        public const float WorkerHeight = 1.35f; // doc said ≈1.05; the owner asked for a bigger worker (it read smaller than a crate from the 55° camera)

        private static readonly ModelSpec[] Specs =
        {
            // The owner's rank-1 crates (3dModeller/crateboss/kasalar, 2026-11-01; Ana Belge §3.2: base 0.82–0.92).
            new ModelSpec { Name = "level1_normal", Key = "BoxNormal", Fit = ModelFit.Base, Target = 0.86f, TextureSize = 512, MaxTriangles = 2800 },
            new ModelSpec { Name = "level1_fixed", Key = "BoxFixed", Fit = ModelFit.Base, Target = 0.92f, TextureSize = 512, MaxTriangles = 2800 },
            new ModelSpec { Name = "level1_delivery", Key = "BoxDelivery", Fit = ModelFit.Base, Target = 0.86f, TextureSize = 512, MaxTriangles = 2800 },
            new ModelSpec { Name = "level1_ice", Key = "BoxIce", Fit = ModelFit.Base, Target = 0.86f, TextureSize = 512, MaxTriangles = 2800 },
            new ModelSpec { Name = "match_red", Key = "BoxMatch_1", Fit = ModelFit.Base, Target = 0.86f, TextureSize = 512, MaxTriangles = 2800 },
            new ModelSpec { Name = "match_teal", Key = "BoxMatch_2", Fit = ModelFit.Base, Target = 0.86f, TextureSize = 512, MaxTriangles = 2800 },
            new ModelSpec { Name = "match_white", Key = "BoxMatch_White", Fit = ModelFit.Base, Target = 0.86f, TextureSize = 512, MaxTriangles = 2800 },
            new ModelSpec { Name = "crate_heavy", Key = "BoxHeavy", Fit = ModelFit.Base, Target = 0.94f, TextureSize = 512, MaxTriangles = 1200 },
            new ModelSpec { Name = "target_plate", Key = "Target", Fit = ModelFit.Base, Target = 0.86f, Orient = ModelOrient.ThinAxisUp, TextureSize = 512, MaxTriangles = 800 },
            new ModelSpec { Name = "barrier_straight", Key = "BarrierStraight", Fit = ModelFit.Base, Target = 1f, Orient = ModelOrient.LongAlongX, TextureSize = 512, MaxTriangles = 1000 },
            new ModelSpec { Name = "barrier_corner", Key = "BarrierCorner", Fit = ModelFit.SameScale, ScaleFrom = "barrier_straight", Orient = ModelOrient.LCorner, TextureSize = 512, MaxTriangles = 1000 },
            new ModelSpec { Name = "barrier_end", Key = "BarrierEnd", Fit = ModelFit.Height, Target = 0.6f, Orient = ModelOrient.EndPost, TextureSize = 512, MaxTriangles = 1000 },
            new ModelSpec { Name = "crane_body", Key = "CraneBody", Fit = ModelFit.CraneJib, Target = CraneJibHeight, Orient = ModelOrient.CraneJib, TextureSize = 1024, MaxTriangles = 8000 },
            new ModelSpec { Name = "crane_trolley", Key = "CraneTrolley", Fit = ModelFit.Height, Target = 0.55f, PivotTop = true, TextureSize = 1024, MaxTriangles = 8000 },
            new ModelSpec { Name = "crane_hook", Key = "CraneHook", Fit = ModelFit.Height, Target = 0.42f, PivotTop = true, TextureSize = 1024, MaxTriangles = 8000 },
            // Ana Belge §3 obstacles (Tripo, toy style). Parked cars are drawn in code (owner 2026-10-31 liked those better).
            new ModelSpec { Name = "obstacle_pallets", Key = "Obstacle_City_0", Fit = ModelFit.Base, Target = 0.9f, TextureSize = 512, MaxTriangles = 800 },
            new ModelSpec { Name = "obstacle_concrete", Key = "Obstacle_City_1", Fit = ModelFit.Base, Target = 0.9f, TextureSize = 512, MaxTriangles = 800 },
            new ModelSpec { Name = "obstacle_bin", Key = "Obstacle_City_2", Fit = ModelFit.Base, Target = 0.86f, TextureSize = 512, MaxTriangles = 800 },
            // Owner 2026-11-01 (3dModeller/crateboss/aletler): the hammer the worker breaks crates with on candy levels.
            new ModelSpec { Name = "tool_hammer", Key = "Hammer", Fit = ModelFit.Base, Target = 0.8f, TextureSize = 512, MaxTriangles = 3000 },
        };

        /// <summary>Worker clips: file suffix = state name, and whether it loops.</summary>
        private static readonly (string name, bool loop)[] Clips =
        {
            ("Walk", true), ("Push", true), ("PushHeavy", true), ("LookAround", true), ("Happy", false), ("Victory", true), ("Think", false),
        };

        [MenuItem("Tools/Crate Boss/Normalize Models")]
        public static void Run()
        {
            if (!AssetDatabase.IsValidFolder(ModelsDir)) return;
            Directory.CreateDirectory(PrefabDir);
            var report = new StringBuilder();
            report.AppendLine("# Model raporu (otomatik)");
            report.AppendLine();
            report.AppendLine("`Tools > Crate Boss > Normalize Models` tarafından yazıldı. Ölçüler Unity biriminde (1 = bir kare). Modeller hiçbir yönde esnetilmez, tek ölçekle büyütülür.");
            report.AppendLine();
            report.AppendLine("| Model | Prefab | Üçgen (hedef) | Ham boyut (en × boy × derinlik) | Son boyut | Ölçek |");
            report.AppendLine("|---|---|---|---|---|---|");
            var scales = new Dictionary<string, float>();
            foreach (var spec in Specs)
            {
                string dir = $"{ModelsDir}/{spec.Name}";
                string fbx = $"{dir}/{spec.Name}.fbx";
                if (!File.Exists(fbx)) continue;
                ConfigureStaticModel(fbx);
                ConfigureTextures(dir, spec.Name, spec.TextureSize);
                var mat = BuildMaterial(dir, spec);
                BuildPrefab(spec, fbx, mat, scales, report);
            }
            if (File.Exists(WorkerFbx)) BuildWorker(report);
            report.AppendLine();
            report.AppendLine("Notlar:");
            report.AppendLine("- `forklift` (18.009), `crane_body` (8.878), `barrier_straight` (7.454) ve `barrier_corner` (2.568) Blender'da basitleştirildi (`E:\\CLAUDE\\scripts\\bl_cb_decimate.py`); önce/sonra görüntülerinde fark yok.");
            report.AppendLine("- `barrier_end` ve `target_plate` basitleştirilince şeritler ve yüzey bozuldu; orijinal kaldı. Bölümde az sayıda oldukları için yükleri küçük.");
            report.AppendLine("- `worker` 29.570 üçgen: tek karakter, basitleştirme yüz ve baret detayını bozabilir; şimdilik orijinal. İstenirse LOD ya da Blender'da ~%35 basitleştirme denenebilir.");
            report.AppendLine("- Kasalar küp değil, tek ölçekle büyütüldü: ahşap 0.79, buz 0.85, çelik 0.92 yüksek (belgedeki 0.77 / 0.74 / 0.62 ancak esnetmeyle olurdu).");
            report.AppendLine("- Hedef plakası kalın bir blok (0.29); zemine gömülü, yalnızca üstteki 3 cm görünür. Boş/dolu rengi kodla çizilen parlak çerçeveden gelir.");
            AssetDatabase.SaveAssets();
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));
            File.WriteAllText(ReportPath, report.ToString());
            Debug.Log("[CrateBoss] Models normalized.\n" + report);
        }

        public static void RunBatch()
        {
            try { Run(); EditorApplication.Exit(0); }
            catch (System.Exception e) { Debug.LogException(e); EditorApplication.Exit(1); }
        }

        // ---------- import settings ----------

        private static void ConfigureStaticModel(string path)
        {
            var mi = (ModelImporter)AssetImporter.GetAtPath(path);
            if (mi.materialImportMode == ModelImporterMaterialImportMode.None && !mi.importAnimation &&
                mi.animationType == ModelImporterAnimationType.None && !mi.importCameras && !mi.importLights) return;
            mi.materialImportMode = ModelImporterMaterialImportMode.None;
            mi.importAnimation = false;
            mi.animationType = ModelImporterAnimationType.None;
            mi.importCameras = false;
            mi.importLights = false;
            mi.SaveAndReimport();
        }

        private static void ConfigureTextures(string dir, string name, int maxSize)
        {
            ConfigureTexture($"{dir}/{name}_basecolor.png", maxSize, false, true);
            ConfigureTexture($"{dir}/{name}_normal.png", maxSize, true, false);
            ConfigureTexture($"{dir}/{name}_metallic.png", 64, false, false);   // only read from disk for the combined map
            ConfigureTexture($"{dir}/{name}_roughness.png", 64, false, false);
            ConfigureTexture($"{dir}/{name}_MetallicSmoothness.png", maxSize, false, false);
        }

        private static void ConfigureTexture(string path, int maxSize, bool normal, bool srgb)
        {
            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) return;
            bool changed = false;
            var type = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
            if (ti.textureType != type) { ti.textureType = type; changed = true; }
            if (!normal && ti.sRGBTexture != srgb) { ti.sRGBTexture = srgb; changed = true; }
            if (ti.maxTextureSize != maxSize) { ti.maxTextureSize = maxSize; changed = true; }
            foreach (var platform in new[] { "Android", "iPhone" })
            {
                var p = ti.GetPlatformTextureSettings(platform);
                if (p.overridden && p.maxTextureSize == maxSize && p.format == TextureImporterFormat.ASTC_6x6) continue;
                p.overridden = true;
                p.maxTextureSize = maxSize;
                p.format = TextureImporterFormat.ASTC_6x6;
                ti.SetPlatformTextureSettings(p);
                changed = true;
            }
            if (changed) ti.SaveAndReimport();
        }

        // ---------- materials ----------

        // URP wants metallic in R and smoothness (1 - roughness) in A of one map.
        private static string BuildMetallicSmoothness(string dir, string name)
        {
            string outPath = $"{dir}/{name}_MetallicSmoothness.png";
            if (File.Exists(outPath)) return outPath;
            var metal = new Texture2D(2, 2);
            var rough = new Texture2D(2, 2);
            metal.LoadImage(File.ReadAllBytes($"{dir}/{name}_metallic.png"));
            rough.LoadImage(File.ReadAllBytes($"{dir}/{name}_roughness.png"));
            int w = metal.width, h = metal.height;
            if (rough.width != w || rough.height != h)
                throw new System.Exception($"{name}: metallic and roughness maps differ in size.");
            var m = metal.GetPixels32();
            var r = rough.GetPixels32();
            var o = new Color32[m.Length];
            for (int i = 0; i < m.Length; i++) o[i] = new Color32(m[i].r, m[i].r, m[i].r, (byte)(255 - r[i].r));
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.SetPixels32(o);
            File.WriteAllBytes(outPath, tex.EncodeToPNG());
            Object.DestroyImmediate(metal);
            Object.DestroyImmediate(rough);
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(outPath);
            return outPath;
        }

        private static Material BuildMaterial(string dir, ModelSpec spec)
        {
            string msPath = BuildMetallicSmoothness(dir, spec.Name);
            ConfigureTexture(msPath, spec.TextureSize, false, false);
            string path = $"{dir}/{spec.Name}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(mat, path);
            }
            SetupLit(mat, AssetDatabase.LoadAssetAtPath<Texture2D>($"{dir}/{spec.Name}_basecolor.png"),
                AssetDatabase.LoadAssetAtPath<Texture2D>($"{dir}/{spec.Name}_normal.png"),
                AssetDatabase.LoadAssetAtPath<Texture2D>(msPath), spec.Alpha);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static void SetupLit(Material mat, Texture2D baseMap, Texture2D normal, Texture2D metallicSmoothness, float alpha)
        {
            mat.SetTexture("_BaseMap", baseMap);
            mat.SetColor("_BaseColor", new Color(1f, 1f, 1f, alpha));
            if (normal != null)
            {
                mat.SetTexture("_BumpMap", normal);
                mat.SetFloat("_BumpScale", 1f);
                mat.EnableKeyword("_NORMALMAP");
            }
            if (metallicSmoothness != null)
            {
                mat.SetTexture("_MetallicGlossMap", metallicSmoothness);
                mat.SetFloat("_Smoothness", 1f);
                mat.SetFloat("_SmoothnessTextureChannel", 0f);
                mat.EnableKeyword("_METALLICSPECGLOSSMAP");
            }
            else mat.SetFloat("_Smoothness", 0.3f);
            mat.enableInstancing = true; // many crates and barrier pieces share one material
            if (alpha < 1f)
            {
                mat.SetFloat("_Surface", 1f);
                mat.SetFloat("_Blend", 0f);
                mat.SetOverrideTag("RenderType", "Transparent");
                mat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                mat.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                mat.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
                mat.SetFloat("_DstBlendAlpha", (float)BlendMode.OneMinusSrcAlpha);
                mat.SetFloat("_ZWrite", 0f);
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.renderQueue = (int)RenderQueue.Transparent;
            }
        }

        // ---------- prefabs ----------

        private static void BuildPrefab(ModelSpec spec, string fbx, Material mat, Dictionary<string, float> scales, StringBuilder report)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(fbx);
            var root = new GameObject(spec.Key);
            var pivot = new GameObject("Pivot").transform;
            pivot.SetParent(root.transform, false);
            var model = (GameObject)Object.Instantiate(asset, pivot, false);
            model.name = spec.Name;
            foreach (var r in model.GetComponentsInChildren<Renderer>())
            {
                r.sharedMaterials = Enumerable.Repeat(mat, r.sharedMaterials.Length).ToArray();
                r.shadowCastingMode = ShadowCastingMode.On;
            }

            var raw = Vertices(root.transform);
            var rawSize = Size(raw);
            var q = Orientation(spec.Orient, raw);
            var rotated = raw.Select(v => q * v).ToList();
            var lo = Min(rotated);
            var hi = Max(rotated);
            var size = hi - lo;

            float s;
            Vector3 anchor; // point (in rotated model space) that lands on the prefab origin
            switch (spec.Fit)
            {
                case ModelFit.Height:
                    s = spec.Target / size.y;
                    anchor = new Vector3((lo.x + hi.x) / 2f, spec.PivotTop ? hi.y : lo.y, (lo.z + hi.z) / 2f);
                    break;
                case ModelFit.SameScale:
                    s = scales.TryGetValue(spec.ScaleFrom, out var other) ? other : 1f;
                    anchor = new Vector3((lo.x + hi.x) / 2f, lo.y, (lo.z + hi.z) / 2f);
                    break;
                case ModelFit.CraneJib:
                    anchor = CraneAnchor(rotated, lo, hi);
                    s = spec.Target / (anchor.y - lo.y);
                    break;
                default:
                    s = spec.Target / Mathf.Max(size.x, size.z);
                    anchor = new Vector3((lo.x + hi.x) / 2f, spec.PivotTop ? hi.y : lo.y, (lo.z + hi.z) / 2f);
                    break;
            }
            if (spec.Orient == ModelOrient.EndPost) anchor.x = lo.x + size.z / 2f; // the post's axis, not the box centre
            if (spec.Orient == ModelOrient.LCorner) { anchor.x = lo.x; anchor.z = lo.z; } // origin on the joint corner
            scales[spec.Name] = s;
            pivot.localRotation = q;
            pivot.localScale = Vector3.one * s;
            pivot.localPosition = -anchor * s;

            string path = $"{PrefabDir}/{spec.Key}.prefab";
            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);

            int tris = Triangles(asset);
            var final = size * s;
            report.AppendLine($"| `{spec.Name}` | `{spec.Key}` | {tris:N0} ({spec.MaxTriangles:N0}){(tris > spec.MaxTriangles ? " **aşıyor**" : "")} | " +
                              $"{F(rawSize.x)} × {F(rawSize.y)} × {F(rawSize.z)} | {F(final.x)} × {F(final.y)} × {F(final.z)} | {F(s)} |");
        }

        /// <summary>Tower centre (x, z) and the jib's underside (y), in rotated model space.</summary>
        private static Vector3 CraneAnchor(List<Vector3> v, Vector3 lo, Vector3 hi)
        {
            float h = hi.y - lo.y;
            var foot = v.Where(p => p.y < lo.y + 0.15f * h).ToList();
            float cx = foot.Average(p => p.x), cz = foot.Average(p => p.z);
            float reach = v.Max(p => p.z - cz);
            var jib = v.Where(p => p.y > lo.y + 0.5f * h && p.z - cz > 0.5f * reach).ToList();
            return new Vector3(cx, jib.Min(p => p.y), cz);
        }

        private static Quaternion Orientation(ModelOrient orient, List<Vector3> v)
        {
            var lo = Min(v);
            var hi = Max(v);
            var size = hi - lo;
            switch (orient)
            {
                case ModelOrient.SideDecalUp:
                    return Quaternion.Euler(0f, 0f, 90f);
                case ModelOrient.ThinAxisUp:
                {
                    int axis = size.x < size.y ? (size.x < size.z ? 0 : 2) : (size.y < size.z ? 1 : 2);
                    float band = size[axis] * 0.1f;
                    int nearMax = v.Count(p => p[axis] > hi[axis] - band);
                    int nearMin = v.Count(p => p[axis] < lo[axis] + band);
                    var face = Vector3.zero;
                    face[axis] = nearMax >= nearMin ? 1f : -1f; // the detailed face has more vertices than the flat back
                    return Quaternion.FromToRotation(face, Vector3.up);
                }
                case ModelOrient.LowEndForward:
                {
                    bool alongX = size.x >= size.z;
                    int axis = alongX ? 0 : 2;
                    float band = size[axis] * 0.15f;
                    float lowEndMin = v.Where(p => p[axis] < lo[axis] + band).Max(p => p.y);
                    float lowEndMax = v.Where(p => p[axis] > hi[axis] - band).Max(p => p.y);
                    var dir = Vector3.zero;
                    dir[axis] = lowEndMax < lowEndMin ? 1f : -1f;
                    return Quaternion.Inverse(Quaternion.LookRotation(dir, Vector3.up));
                }
                case ModelOrient.LCorner:
                {
                    // An L leaves one quadrant of its box empty: turn so that quadrant is (+x, +z).
                    var c = (lo + hi) / 2f;
                    var best = Quaternion.identity;
                    int fewest = int.MaxValue;
                    for (int k = 0; k < 4; k++)
                    {
                        var q = Quaternion.Euler(0f, 90f * k, 0f);
                        int n = v.Count(p => { var r = q * (p - c); return r.x > 0.1f * size.x && r.z > 0.1f * size.z; });
                        if (n < fewest) { fewest = n; best = q; }
                    }
                    return best;
                }
                case ModelOrient.EndPost:
                {
                    // The stubs stick out on the long footprint side; the side with more far vertices has them.
                    bool alongX = size.x >= size.z;
                    int axis = alongX ? 0 : 2;
                    float mid = (lo[axis] + hi[axis]) / 2f, reach = size[axis] * 0.3f;
                    int plus = v.Count(p => p[axis] > mid + reach), minus = v.Count(p => p[axis] < mid - reach);
                    var dir = Vector3.zero;
                    dir[axis] = plus >= minus ? 1f : -1f;
                    return Quaternion.Euler(0f, 90f, 0f) * Quaternion.Inverse(Quaternion.LookRotation(dir, Vector3.up)); // stubs: dir -> +Z -> +X
                }
                case ModelOrient.LongAlongX:
                    return size.z > size.x ? Quaternion.Euler(0f, 90f, 0f) : Quaternion.identity;
                case ModelOrient.CraneJib:
                {
                    float h = size.y;
                    var foot = v.Where(p => p.y < lo.y + 0.15f * h).ToList();
                    var c = new Vector3(foot.Average(p => p.x), 0f, foot.Average(p => p.z));
                    var far = v.Where(p => p.y > lo.y + 0.5f * h).OrderByDescending(p => new Vector2(p.x - c.x, p.z - c.z).sqrMagnitude).First();
                    var dir = new Vector3(far.x - c.x, 0f, far.z - c.z).normalized;
                    return Quaternion.Inverse(Quaternion.LookRotation(dir, Vector3.up));
                }
                default:
                    return Quaternion.identity;
            }
        }

        // ---------- worker (Mixamo) ----------

        private static void BuildWorker(StringBuilder report)
        {
            var mi = (ModelImporter)AssetImporter.GetAtPath(WorkerFbx);
            if (mi.animationType != ModelImporterAnimationType.Human || mi.avatarSetup != ModelImporterAvatarSetup.CreateFromThisModel ||
                mi.importAnimation || mi.materialImportMode != ModelImporterMaterialImportMode.None)
            {
                mi.animationType = ModelImporterAnimationType.Human;
                mi.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                mi.importAnimation = false;
                mi.materialImportMode = ModelImporterMaterialImportMode.None;
                mi.SaveAndReimport();
            }
            string texDir = WorkerDir + "/Textures";
            if (!Directory.Exists(texDir) || Directory.GetFiles(texDir).Count(f => !f.EndsWith(".meta")) == 0)
            {
                Directory.CreateDirectory(texDir);
                mi.ExtractTextures(texDir);
                AssetDatabase.Refresh();
            }
            var avatar = AssetDatabase.LoadAllAssetsAtPath(WorkerFbx).OfType<Avatar>().FirstOrDefault();

            var clips = new Dictionary<string, AnimationClip>();
            foreach (var (name, loop) in Clips)
            {
                string path = $"{WorkerDir}/worker@{name}.fbx";
                if (!File.Exists(path)) continue;
                clips[name] = ConfigureClip(path, name, loop, avatar);
            }

            // Material from the textures that were inside the FBX.
            var textures = Directory.GetFiles(texDir).Where(f => { var e = Path.GetExtension(f).ToLowerInvariant(); return e == ".png" || e == ".jpg" || e == ".jpeg"; })
                .Select(f => f.Replace('\\', '/')).ToList();
            string normalPath = textures.FirstOrDefault(f => Path.GetFileName(f).ToLowerInvariant().Contains("normal"));
            string basePath = textures.FirstOrDefault(f => f != normalPath);
            if (basePath != null) ConfigureTexture(basePath, 1024, false, true);
            if (normalPath != null) ConfigureTexture(normalPath, 1024, true, false);
            string matPath = WorkerDir + "/worker.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(mat, matPath);
            }
            SetupLit(mat, basePath != null ? AssetDatabase.LoadAssetAtPath<Texture2D>(basePath) : null,
                normalPath != null ? AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath) : null, null, 1f);
            mat.SetFloat("_Smoothness", 0.25f);
            EditorUtility.SetDirty(mat);

            var controller = BuildController(clips);

            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(WorkerFbx);
            var root = new GameObject("Worker");
            var pivot = new GameObject("Pivot").transform;
            pivot.SetParent(root.transform, false);
            var model = (GameObject)Object.Instantiate(asset, pivot, false);
            model.name = "worker";
            foreach (var r in model.GetComponentsInChildren<Renderer>())
            {
                r.sharedMaterials = Enumerable.Repeat(mat, r.sharedMaterials.Length).ToArray();
                r.shadowCastingMode = ShadowCastingMode.On;
            }
            var animator = model.GetComponent<Animator>() ?? model.AddComponent<Animator>();
            animator.avatar = avatar;
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false; // the game moves the worker cell by cell
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            var bounds = new Bounds();
            bool first = true;
            foreach (var r in model.GetComponentsInChildren<Renderer>())
            {
                if (first) { bounds = r.bounds; first = false; }
                else bounds.Encapsulate(r.bounds);
            }
            float s = WorkerHeight / bounds.size.y;
            pivot.localScale = Vector3.one * s;
            pivot.localPosition = new Vector3(-bounds.center.x * s, -bounds.min.y * s, -bounds.center.z * s);
            PrefabUtility.SaveAsPrefabAsset(root, $"{PrefabDir}/Worker.prefab");
            Object.DestroyImmediate(root);

            int tris = Triangles(asset);
            report.AppendLine($"| `worker` | `Worker` | {tris:N0} (10.000){(tris > 10000 ? " **aşıyor**" : "")} | boy {F(bounds.size.y)} | boy {F(WorkerHeight)} | {F(s)} |");
            report.AppendLine();
            report.AppendLine("Karakter klipleri: " + string.Join(", ", clips.Select(c => $"`{c.Key}` {F(c.Value.length)} sn{(c.Value.isLooping ? " (döngü)" : "")}")) + ".");
            report.AppendLine("`Push` = `karakter@Pushing.fbx` (kısa, tok itme döngüsü), `PushHeavy` = `karakter@Push.fbx` (uzun, yavaş itme). Bekleme: `Victory` %40 hızda (Breathing Idle gelene kadar).");
        }

        private static AnimationClip ConfigureClip(string path, string name, bool loop, Avatar avatar)
        {
            var mi = (ModelImporter)AssetImporter.GetAtPath(path);
            bool changed = false;
            if (mi.animationType != ModelImporterAnimationType.Human) { mi.animationType = ModelImporterAnimationType.Human; changed = true; }
            if (mi.avatarSetup != ModelImporterAvatarSetup.CopyFromOther || mi.sourceAvatar != avatar)
            {
                mi.avatarSetup = ModelImporterAvatarSetup.CopyFromOther;
                mi.sourceAvatar = avatar;
                changed = true;
            }
            if (mi.materialImportMode != ModelImporterMaterialImportMode.None) { mi.materialImportMode = ModelImporterMaterialImportMode.None; changed = true; }
            if (!mi.importAnimation) { mi.importAnimation = true; changed = true; }
            if (changed) mi.SaveAndReimport();

            var current = mi.clipAnimations;
            bool ok = current.Length == 1 && current[0].name == name && current[0].loopTime == loop && current[0].lockRootRotation && current[0].lockRootHeightY;
            if (!ok)
            {
                var defaults = mi.defaultClipAnimations;
                if (defaults.Length > 0)
                {
                    var c = defaults[0];
                    c.name = name;
                    c.loopTime = loop;
                    c.lockRootRotation = true;      // facing comes from the game
                    c.keepOriginalOrientation = true;
                    c.lockRootHeightY = true;
                    c.keepOriginalPositionY = true;
                    c.lockRootPositionXZ = false;   // root travel is dropped: applyRootMotion is off
                    mi.clipAnimations = new[] { c };
                    mi.SaveAndReimport();
                }
            }
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().FirstOrDefault(c => !c.name.StartsWith("__preview__"));
        }

        // States are switched from code (WorkerView, cross-fade 0.08 s); Happy and Think fall back to Idle on their own.
        private static AnimatorController BuildController(Dictionary<string, AnimationClip> clips)
        {
            if (File.Exists(ControllerPath)) AssetDatabase.DeleteAsset(ControllerPath);
            var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            var sm = controller.layers[0].stateMachine;
            AnimatorState Add(string state, string clip, float speed)
            {
                var st = sm.AddState(state);
                if (clips.TryGetValue(clip, out var c)) st.motion = c;
                st.speed = speed;
                return st;
            }
            // Owner 2026-11-01: the wait loop was the victory clip slowed down, so waiting and cheering looked the same.
            var idle = Add("Idle", "LookAround", 0.8f);
            sm.defaultState = idle;
            Add("Walk", "Walk", 2f);
            Add("Push", "Push", 1.6f);
            Add("PushHeavy", "PushHeavy", 1.3f);
            Add("LookAround", "LookAround", 1f);
            Add("Victory", "Victory", 1f);
            foreach (var oneShot in new[] { Add("Happy", "Happy", 1.2f), Add("Think", "Think", 1.3f) })
            {
                var back = oneShot.AddTransition(idle);
                back.hasExitTime = true;
                back.exitTime = 0.92f;
                back.duration = 0.08f;
                back.hasFixedDuration = true;
            }
            EditorUtility.SetDirty(controller);
            return controller;
        }

        // ---------- helpers ----------

        private static List<Vector3> Vertices(Transform root)
        {
            var list = new List<Vector3>();
            var toRoot = root.worldToLocalMatrix;
            foreach (var mf in root.GetComponentsInChildren<MeshFilter>())
            {
                if (mf.sharedMesh == null) continue;
                var m = toRoot * mf.transform.localToWorldMatrix;
                foreach (var v in mf.sharedMesh.vertices) list.Add(m.MultiplyPoint3x4(v));
            }
            return list;
        }

        private static int Triangles(GameObject asset)
        {
            long n = 0;
            foreach (var mf in asset.GetComponentsInChildren<MeshFilter>(true))
                if (mf.sharedMesh != null) for (int i = 0; i < mf.sharedMesh.subMeshCount; i++) n += mf.sharedMesh.GetIndexCount(i) / 3;
            foreach (var smr in asset.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                if (smr.sharedMesh != null) for (int i = 0; i < smr.sharedMesh.subMeshCount; i++) n += smr.sharedMesh.GetIndexCount(i) / 3;
            return (int)n;
        }

        private static Vector3 Min(List<Vector3> v) => new Vector3(v.Min(p => p.x), v.Min(p => p.y), v.Min(p => p.z));
        private static Vector3 Max(List<Vector3> v) => new Vector3(v.Max(p => p.x), v.Max(p => p.y), v.Max(p => p.z));
        private static Vector3 Size(List<Vector3> v) => Max(v) - Min(v);
        private static string F(float f) => f.ToString("0.00", CultureInfo.InvariantCulture);
    }
}
