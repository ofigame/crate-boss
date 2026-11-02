using CrateBoss.Localization;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CrateBoss.View
{
    /// <summary>
    /// Low / Medium / High (Ana Belge §2.4). Picked automatically on first launch from the device, can be changed
    /// by the player. Low uses the "Mobile" renderer (no SSAO), Medium and High the "PC" renderer (SSAO);
    /// MSAA, soft shadows, depth of field and particle counts follow the tier.
    /// </summary>
    public static class QualityManager
    {
        private const string Key = "crateboss.quality"; // -1 = automatic

        public static QualityTier Current { get; private set; } = QualityTier.High;
        public static bool IsAutomatic => PlayerPrefs.GetInt(Key, -1) < 0;

        public static float ParticleScale => Current == QualityTier.Low ? 0.5f : Current == QualityTier.Medium ? 1f : 1.4f;
        public static bool DepthOfField => Current == QualityTier.High;
        public static bool SoftShadows => Current != QualityTier.Low;

        public static QualityTier Detect()
        {
            if (!Application.isMobilePlatform) return QualityTier.High;
            int memory = SystemInfo.systemMemorySize;
            int cores = SystemInfo.processorCount;
            if (memory >= 6000 && cores >= 8) return QualityTier.High;
            if (memory >= 3500 && cores >= 6) return QualityTier.Medium;
            return QualityTier.Low;
        }

        /// <summary>Applies the saved choice (or the detected tier).</summary>
        public static void ApplySaved()
        {
            int saved = PlayerPrefs.GetInt(Key, -1);
            Apply(saved < 0 ? Detect() : (QualityTier)saved);
        }

        /// <summary>Cycles Automatic → Low → Medium → High → Automatic. Returns the label to show.</summary>
        public static string Cycle()
        {
            int saved = PlayerPrefs.GetInt(Key, -1);
            saved = saved >= (int)QualityTier.High ? -1 : saved + 1;
            PlayerPrefs.SetInt(Key, saved);
            PlayerPrefs.Save();
            ApplySaved();
            return Label;
        }

        public static string Label =>
            IsAutomatic ? Loc.F("quality.auto", TierName) : TierName;

        private static string TierName => Loc.T(Current == QualityTier.Low ? "quality.low" : Current == QualityTier.Medium ? "quality.medium" : "quality.high");

        public static void Apply(QualityTier tier)
        {
            Current = tier;
            int level = FindLevel(tier == QualityTier.Low ? "Mobile" : "PC");
            if (level >= 0 && QualitySettings.GetQualityLevel() != level) QualitySettings.SetQualityLevel(level, true);
            var urp = QualitySettings.renderPipeline as UniversalRenderPipelineAsset ?? GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset;
            if (urp != null)
            {
                urp.msaaSampleCount = tier == QualityTier.Low ? 2 : 4;
                urp.shadowCascadeCount = 1;
            }
            GraphicsSettings.lightsUseLinearIntensity = true;
            GraphicsSettings.lightsUseColorTemperature = true;
            QualitySettings.vSyncCount = 0;
        }

        /// <summary>The board decides how far shadows need to reach: short distance = sharp and cheap.</summary>
        public static void SetShadowDistance(float distance)
        {
            var urp = QualitySettings.renderPipeline as UniversalRenderPipelineAsset ?? GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset;
            if (urp != null) urp.shadowDistance = distance;
        }

        private static int FindLevel(string name)
        {
            var names = QualitySettings.names;
            for (int i = 0; i < names.Length; i++)
                if (names[i] == name) return i;
            return -1;
        }
    }
}
