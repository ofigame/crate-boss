namespace CrateBoss.Ads
{
    /// <summary>Where an ad is shown (Ana Belge §15.1–15.2).</summary>
    public enum AdPlacement
    {
        HintRewarded,
        DoubleReward,
        /// <summary>The home screen's "+30 CB Coin" button.</summary>
        HomeCoins,
        DailyGift,
        LevelInterstitial,
        /// <summary>Opening a delivery crate without enough CB Coin (F7).</summary>
        OpenCrate,
    }
}
