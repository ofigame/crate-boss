using System;

namespace CrateBoss.Ads
{
    /// <summary>A full-screen interstitial provider (Ana Belge §15.4). Shown only where AdPolicy allows.</summary>
    public interface IInterstitialAdService
    {
        bool IsReady { get; }
        void Load();
        void Show(Action onClosed);
    }
}
