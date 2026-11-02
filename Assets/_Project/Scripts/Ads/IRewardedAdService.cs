using System;

namespace CrateBoss.Ads
{
    /// <summary>A rewarded ad provider (test now, a real SDK later). Opens only when the player presses a button.</summary>
    public interface IRewardedAdService
    {
        bool IsReady { get; }
        void Load();
        /// <param name="onFinished">true = watched to the end, the reward is earned.</param>
        void Show(Action<bool> onFinished);
    }
}
