namespace CrateBoss.Ads
{
    /// <summary>
    /// The only place of the interstitial rules (Ana Belge §15.2): none in the first levels, only after a win when
    /// "Sonraki" is pressed (never mid-level, after a restart or on the home screen), every third
    /// level at most and two minutes apart, six per session at most, and none soon after a rewarded ad.
    /// </summary>
    public static class AdPolicy
    {
        /// <summary>A yard was won (counts toward "every third level").</summary>
        public static void LevelFinished(AdSessionState s) => s.LevelsSinceInterstitial++;

        /// <param name="levelsCleared">Main levels the player has cleared so far (this one included).</param>
        public static bool CanShowInterstitial(AdConfig cfg, AdSessionState s, int levelsCleared, double now)
        {
            if (levelsCleared <= cfg.freeLevels) return false;
            if (s.LevelsSinceInterstitial < cfg.levelsBetween) return false;
            if (now - s.LastInterstitialAt < cfg.secondsBetween) return false;
            if (s.InterstitialsShown >= cfg.maxPerSession) return false;
            if (now - s.LastRewardedAt < cfg.quietAfterRewarded) return false;
            return true;
        }

        public static void InterstitialShown(AdSessionState s, double now)
        {
            s.LevelsSinceInterstitial = 0;
            s.InterstitialsShown++;
            s.LastInterstitialAt = now;
        }

        public static void RewardedCompleted(AdSessionState s, double now) => s.LastRewardedAt = now;
    }
}
