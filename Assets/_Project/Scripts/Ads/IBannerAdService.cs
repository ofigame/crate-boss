namespace CrateBoss.Ads
{
    /// <summary>A banner at the bottom of the screen (owner 2026-11-01: on the home screen only, never in a level).</summary>
    public interface IBannerAdService
    {
        /// <summary>Height the banner takes at the bottom, in the UI's reference units (1080 × 1920).</summary>
        float Height { get; }
        void Show();
        void Hide();
    }
}
