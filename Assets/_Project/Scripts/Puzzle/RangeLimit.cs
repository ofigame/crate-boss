namespace CrateBoss.Puzzle
{
    /// <summary>The least thinking a range of levels must ask for (brief §3; a row of the table).</summary>
    public sealed class RangeLimit
    {
        public int First, Last;
        public int ReversePlusPrep, TrapPushes, MaxSolutions;
        public bool HastyMustFail = true, OrderMustMatter = true;

        public RangeLimit(int first, int last, int reversePlusPrep, int trapPushes, int maxSolutions)
        {
            First = first; Last = last; ReversePlusPrep = reversePlusPrep; TrapPushes = trapPushes; MaxSolutions = maxSolutions;
        }
    }
}
