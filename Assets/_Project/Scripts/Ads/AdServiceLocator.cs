using System;

namespace CrateBoss.Ads
{
    /// <summary>
    /// Picks the ad providers in one place (Ana Belge §15.4). Test ads for now; the real SDK's services replace them for the
    /// store build (Docs/YAYIN_KONTROL_LISTESI.md). "-noads" simulates no ad loaded; "-shots" closes test ads itself.
    /// </summary>
    public static class AdServiceLocator
    {
        private static IRewardedAdService rewarded;
        private static IInterstitialAdService interstitial;
        private static IBannerAdService banner;

        public static IRewardedAdService Rewarded => rewarded ??= new TestRewardedAdService(!Flag("-noads"), Flag("-shots"));
        public static IInterstitialAdService Interstitial => interstitial ??= new TestInterstitialAdService(!Flag("-noads"), Flag("-shots"));
        public static IBannerAdService Banner => banner ??= new TestBannerAdService();

        /// <summary>Plug in other providers (a real SDK, or fakes in tests).</summary>
        public static void Use(IRewardedAdService rewardedService, IInterstitialAdService interstitialService)
        {
            rewarded = rewardedService;
            interstitial = interstitialService;
        }

        /// <summary>Plug in a real banner provider.</summary>
        public static void UseBanner(IBannerAdService service) => banner = service;

        private static bool Flag(string name) => Array.IndexOf(Environment.GetCommandLineArgs(), name) >= 0;
    }
}
