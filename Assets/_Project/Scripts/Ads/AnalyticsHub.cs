using UnityEngine;

namespace CrateBoss.Ads
{
    /// <summary>The single analytics point (Ana Belge §15.4). Writes to the console for now; a real backend plugs in here.</summary>
    public static class AnalyticsHub
    {
        public static void Log(AdPlacement placement, AdResult result) => Debug.Log($"[Analytics] ad {placement} {result}");
    }
}
