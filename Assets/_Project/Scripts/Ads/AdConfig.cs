using UnityEngine;

namespace CrateBoss.Ads
{
    /// <summary>
    /// The interstitial rules' numbers (Ana Belge §15) in one asset, so they can be tuned without code.
    /// The game loads Resources/AdConfig; without the asset the defaults below are used.
    /// </summary>
    [CreateAssetMenu(menuName = "Crate Boss/Ad Config", fileName = "AdConfig")]
    public class AdConfig : ScriptableObject
    {
        [Header("Interstitial (§R.2)")]
        [Tooltip("Levels cleared before the first interstitial may show.")]
        public int freeLevels = 8;
        public int levelsBetween = 3;
        public float secondsBetween = 120f;
        public int maxPerSession = 6;
        [Tooltip("No interstitial this long after a rewarded ad.")]
        public float quietAfterRewarded = 300f;
        [Tooltip("The short 'next yard' screen before the ad, so a tap meant for a button does not land on it.")]
        public float transitionSeconds = 0.3f;

        [Header("Test ads (§7.3)")]
        public float testRewardedSeconds = 5f;
        public float testInterstitialSeconds = 3f;

        private static AdConfig loaded;

        public static AdConfig Current
        {
            get
            {
                if (loaded != null) return loaded;
                loaded = Resources.Load<AdConfig>("AdConfig");
                if (loaded == null) loaded = CreateInstance<AdConfig>();
                return loaded;
            }
        }
    }
}
