using System;
using CrateBoss.Localization;

namespace CrateBoss.Ads
{
    /// <summary>The test interstitial (Ana Belge §15.4): "TEST GEÇİŞ REKLAMI", "Kapat" after three seconds.</summary>
    public sealed class TestInterstitialAdService : IInterstitialAdService
    {
        private readonly bool available;
        private readonly bool autoClose;

        public TestInterstitialAdService(bool available = true, bool autoClose = false)
        {
            this.available = available;
            this.autoClose = autoClose;
        }

        public bool IsReady => available;

        public void Load() { }

        public void Show(Action onClosed) =>
            TestAdOverlay.Open(Loc.T("ad.test.interstitial"), AdConfig.Current.testInterstitialSeconds, false, autoClose, _ => onClosed?.Invoke());
    }
}
