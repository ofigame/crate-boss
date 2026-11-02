using System;

namespace CrateBoss.Ads
{
    /// <summary>
    /// Runs one rewarded ad (Ana Belge §15): logs it, and gives the reward only when the provider reports the ad was
    /// watched to the end. Not ready → the "no ad now" path, nothing given.
    /// </summary>
    public static class RewardedAds
    {
        public static void Show(IRewardedAdService service, AdPlacement placement, AdSessionState session, Func<double> clock,
            Action onReward, Action onUnavailable, Action onSkipped = null)
        {
            if (service == null || !service.IsReady)
            {
                AnalyticsHub.Log(placement, AdResult.NotLoaded);
                service?.Load();
                onUnavailable?.Invoke();
                return;
            }
            AnalyticsHub.Log(placement, AdResult.Shown);
            service.Show(completed =>
            {
                AnalyticsHub.Log(placement, completed ? AdResult.Completed : AdResult.Skipped);
                service.Load();
                if (!completed)
                {
                    onSkipped?.Invoke();
                    return;
                }
                AdPolicy.RewardedCompleted(session, clock());
                onReward?.Invoke();
            });
        }
    }
}
