using System;
using CrateBoss.Localization;

namespace CrateBoss.Ads
{
    /// <summary>The test rewarded ad (Ana Belge §15.4): "TEST REKLAMI" with a countdown; the reward only when it runs out.</summary>
    public sealed class TestRewardedAdService : IRewardedAdService
    {
        private readonly bool available;
        private readonly bool autoClose;

        /// <param name="available">false simulates "no ad loaded" (the -noads flag).</param>
        /// <param name="autoClose">The screenshot tour closes the ad by itself once the countdown ends.</param>
        public TestRewardedAdService(bool available = true, bool autoClose = false)
        {
            this.available = available;
            this.autoClose = autoClose;
        }

        public bool IsReady => available;

        public void Load() { }

        public void Show(Action<bool> onFinished) =>
            TestAdOverlay.Open(Loc.T("ad.test.rewarded"), AdConfig.Current.testRewardedSeconds, true, autoClose, onFinished);
    }
}
