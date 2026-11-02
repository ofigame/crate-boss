namespace CrateBoss.Core
{
    /// <summary>One crane's place during play: the next drop in its list and the moves left until it.</summary>
    public sealed class CraneState
    {
        /// <summary>Index of the next drop in the crane's list.</summary>
        public int Index { get; set; }
        /// <summary>Successful moves left until the next drop (0 = due, waiting for a free cell).</summary>
        public int Counter { get; set; }

        public CraneState(int index, int counter)
        {
            Index = index;
            Counter = counter;
        }

        public CraneState Clone() => new CraneState(Index, Counter);
    }
}
