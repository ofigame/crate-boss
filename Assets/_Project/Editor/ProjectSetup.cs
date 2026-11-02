using System;
using System.IO;
using System.Linq;
using CrateBoss.Game;
using CrateBoss.View;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CrateBoss.EditorTools
{
    /// <summary>
    /// Tools > Crate Boss > Setup Project (safe to run again): URP quality settings (soft shadows, HDR, MSAA, SSAO on
    /// the PC renderer), base materials, font fallbacks, the Game scene and portrait player settings.
    /// Batch entry points are used by the command-line scripts.
    /// </summary>
    public static class ProjectSetup
    {
        private const string Root = "Assets/_Project";
        private const string MaterialsDir = Root + "/Resources/Materials";
        private const string FontsDir = Root + "/Resources/Fonts";
        public const string ScenePath = Root + "/Scenes/Game.unity";

        [MenuItem("Tools/Crate Boss/Setup Project")]
        public static void Setup()
        {
            Directory.CreateDirectory(MaterialsDir);
            Directory.CreateDirectory(Root + "/Scenes");
            AssetDatabase.Refresh();
            SetupRenderPipeline();
            CreateMaterials();
            SetupFonts();
            ModelSetup.Run();
            EnvironmentPrefabs.Generate();
            CreateScene();
            ConfigurePlayer();
            AppIconGenerator.Generate();
            AssetDatabase.SaveAssets();
            Debug.Log("[CrateBoss] Setup complete.");
        }

        public static void SetupBatch()
        {
            try { Setup(); EditorApplication.Exit(0); }
            catch (Exception e) { Debug.LogException(e); EditorApplication.Exit(1); }
        }

        [MenuItem("Tools/Crate Boss/Build Windows (test)")]
        public static bool BuildWindows()
        {
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = "Builds/Windows/CrateBoss.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None,
            });
            Debug.Log($"[CrateBoss] Windows build: {report.summary.result}");
            return report.summary.result == BuildResult.Succeeded;
        }

        public static void BuildWindowsBatch()
        {
            try { Setup(); EditorApplication.Exit(BuildWindows() ? 0 : 1); }
            catch (Exception e) { Debug.LogException(e); EditorApplication.Exit(1); }
        }

        // Debug-signed test APK for sideloading.
        public static void BuildAndroidBatch()
        {
            try
            {
                Setup();
                var target = NamedBuildTarget.Android;
                PlayerSettings.SetScriptingBackend(target, ScriptingImplementation.IL2CPP);
                PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;
                PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = new[] { ScenePath },
                    locationPathName = "Builds/Android/CrateBoss.apk",
                    target = BuildTarget.Android,
                    options = BuildOptions.None,
                });
                Debug.Log($"[CrateBoss] Android build: {report.summary.result}");
                EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
            }
            catch (Exception e) { Debug.LogException(e); EditorApplication.Exit(1); }
        }

        // ---------- rendering (Ana Belge §2) ----------

        private static void SetupRenderPipeline()
        {
            for (int i = 0; i < QualitySettings.names.Length; i++)
            {
                var asset = QualitySettings.GetRenderPipelineAssetAt(i) as UniversalRenderPipelineAsset;
                if (asset == null) continue;
                bool pc = QualitySettings.names[i] == "PC";
                asset.supportsHDR = true;
                asset.msaaSampleCount = pc ? 4 : 2;
                asset.renderScale = 1f;
                asset.shadowDistance = 25f;
                asset.shadowCascadeCount = 1;
                var so = new SerializedObject(asset);
                Set(so, "m_SoftShadowsSupported", true);
                Set(so, "m_MainLightShadowmapResolution", pc ? 2048 : 1024);
                Set(so, "m_AdditionalLightsRenderingMode", 1); // per pixel (mine lanterns)
                Set(so, "m_SupportsCameraDepthTexture", true);
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(asset);

                var renderer = RendererOf(asset);
                if (renderer != null && pc) EnsureSsao(renderer);
            }
        }

        private static ScriptableRendererData RendererOf(UniversalRenderPipelineAsset asset)
        {
            var list = new SerializedObject(asset).FindProperty("m_RendererDataList");
            return list != null && list.arraySize > 0 ? list.GetArrayElementAtIndex(0).objectReferenceValue as ScriptableRendererData : null;
        }

        // Screen-space ambient occlusion: soft darkening in corners and seams that "seats" objects on the floor.
        private static void EnsureSsao(ScriptableRendererData data)
        {
            if (data.rendererFeatures.Any(f => f != null && f.GetType().Name == "ScreenSpaceAmbientOcclusion")) return;
            var type = typeof(UniversalRenderPipelineAsset).Assembly.GetType("UnityEngine.Rendering.Universal.ScreenSpaceAmbientOcclusion");
            if (type == null) { Debug.LogWarning("[CrateBoss] SSAO feature type not found."); return; }
            var feature = (ScriptableRendererFeature)ScriptableObject.CreateInstance(type);
            feature.name = "SSAO";
            AssetDatabase.AddObjectToAsset(feature, data);
            var so = new SerializedObject(data);
            var features = so.FindProperty("m_RendererFeatures");
            var map = so.FindProperty("m_RendererFeatureMap");
            features.arraySize++;
            features.GetArrayElementAtIndex(features.arraySize - 1).objectReferenceValue = feature;
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(feature, out _, out long localId);
            map.arraySize++;
            map.GetArrayElementAtIndex(map.arraySize - 1).longValue = localId;
            so.ApplyModifiedPropertiesWithoutUndo();

            var fso = new SerializedObject(feature);
            Set(fso, "m_Settings.Intensity", 1.2f);
            Set(fso, "m_Settings.Radius", 0.25f);
            Set(fso, "m_Settings.DirectLightingStrength", 0.3f);
            fso.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(data);
            Debug.Log("[CrateBoss] SSAO added to " + data.name);
        }

        private static void Set(SerializedObject so, string path, object value)
        {
            var p = so.FindProperty(path);
            if (p == null) return;
            switch (value)
            {
                case bool b: p.boolValue = b; break;
                case int i: if (p.propertyType == SerializedPropertyType.Enum) p.enumValueIndex = i; else p.intValue = i; break;
                case float f: p.floatValue = f; break;
            }
        }

        // ---------- materials ----------

        private static void CreateMaterials()
        {
            Material Make(string name, string shader, Action<Material> configure)
            {
                string path = $"{MaterialsDir}/{name}.mat";
                var m = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (m == null)
                {
                    m = new Material(Shader.Find(shader));
                    AssetDatabase.CreateAsset(m, path);
                }
                configure(m);
                EditorUtility.SetDirty(m);
                return m;
            }

            // URP drops _EMISSION when the colour is black, which would strip the variant: keep white in the base.
            Make("Lit", "Universal Render Pipeline/Lit", m =>
            {
                m.SetColor("_EmissionColor", Color.white);
                m.EnableKeyword("_EMISSION");
                m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
                m.enableInstancing = true;
            });
            Make("LitTransparent", "Universal Render Pipeline/Lit", m =>
            {
                MakeTransparent(m);
                m.SetColor("_EmissionColor", Color.white);
                m.EnableKeyword("_EMISSION");
                m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            });
            Make("Unlit", "Universal Render Pipeline/Unlit", m => { });
            Make("UnlitTransparent", "Universal Render Pipeline/Unlit", MakeTransparent);
            Make("Particle", "Universal Render Pipeline/Particles/Unlit", MakeTransparent);
        }

        private static void MakeTransparent(Material m)
        {
            m.SetFloat("_Surface", 1f);
            m.SetFloat("_Blend", 0f);
            m.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            m.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            m.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
            m.SetFloat("_DstBlendAlpha", (float)BlendMode.OneMinusSrcAlpha);
            m.SetFloat("_ZWrite", 0f);
            m.SetOverrideTag("RenderType", "Transparent");
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.renderQueue = (int)RenderQueue.Transparent;
        }

        // ---------- fonts (Ana Belge §2 and §L.3: Turkish letters, Arabic and Korean fallbacks; Japanese from the device) ----------
        // Noto Sans KR also carries kana and the common kanji; the OS names are a second net for Japanese (Windows, iOS, Android).
        private static readonly string[] JapaneseSystemFonts = { "Yu Gothic UI", "Meiryo", "Hiragino Sans", "Noto Sans CJK JP", "Droid Sans Fallback" };

        private static void SetupFonts()
        {
            var arabic = AssetDatabase.LoadAssetAtPath<Font>($"{FontsDir}/NotoSansArabic.ttf");
            var korean = AssetDatabase.LoadAssetAtPath<Font>($"{FontsDir}/NotoSansKR.ttf");
            foreach (var name in new[] { "Nunito-ExtraBold", "Nunito-SemiBold" })
            {
                var importer = AssetImporter.GetAtPath($"{FontsDir}/{name}.ttf") as TrueTypeFontImporter;
                if (importer == null) continue;
                importer.fontReferences = new[] { arabic, korean }.Where(f => f != null).ToArray();
                importer.fontNames = new[] { name.Split('-')[0] }.Concat(JapaneseSystemFonts).ToArray();
                importer.includeFontData = true;
                importer.SaveAndReimport();
            }
        }

        // ---------- scene and player ----------

        /// <summary>The scene is generated: rebuilt on every setup so it always matches the code.</summary>
        private static void CreateScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cam = new GameObject("Main Camera") { tag = "MainCamera" };
            cam.AddComponent<Camera>();
            cam.AddComponent<AudioListener>();
            cam.AddComponent<UniversalAdditionalCameraData>();
            cam.AddComponent<CameraFramer>();
            cam.transform.SetPositionAndRotation(new Vector3(3, 9, -4), Quaternion.Euler(55, 0, 0));

            var sun = new GameObject("Sun").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(40f, 135f, 0f);

            new GameObject("Game").AddComponent<GameController>();
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        }

        private static void ConfigurePlayer()
        {
            const string id = "com.ofigame.crateboss";
            PlayerSettings.companyName = "Ofigame";
            PlayerSettings.productName = "Crate Boss";
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, id);
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, id);
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Standalone, id);
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.SplashScreen.showUnityLogo = false;
            PlayerSettings.defaultScreenWidth = 540;
            PlayerSettings.defaultScreenHeight = 960;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = true;
            GraphicsSettings.lightsUseLinearIntensity = true;
            GraphicsSettings.lightsUseColorTemperature = true;
        }
    }
}
