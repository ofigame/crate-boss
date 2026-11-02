namespace CrateBoss.Ads
{
    /// <summary>What the ad rules remember during one play session (times in seconds since start).</summary>
    public sealed class AdSessionState
    {
        public int LevelsSinceInterstitial;
        public int InterstitialsShown;
        public double LastInterstitialAt = double.NegativeInfinity;
        public double LastRewardedAt = double.NegativeInfinity;
    }
}
