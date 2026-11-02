namespace CrateBoss.Meta
{
    /// <summary>The CB Coin a finished level pays (shown counting up on the win panel): the first clear, or a better replay.</summary>
    public readonly struct RewardBreakdown
    {
        /// <summary>First clear: the coins of its stars.</summary>
        public readonly int Clear;
        /// <summary>Replay: the extra of a better star result.</summary>
        public readonly int Better;
        /// <summary>The level was cleared for the first time (a delivery crate comes).</summary>
        public readonly bool FirstClear;

        public RewardBreakdown(int clear, int better, bool firstClear)
        {
            Clear = clear;
            Better = better;
            FirstClear = firstClear;
        }

        public int Total => Clear + Better;
        /// <summary>Kept for the win panel: the second line.</summary>
        public int ThreeStars => Better;
    }
}
