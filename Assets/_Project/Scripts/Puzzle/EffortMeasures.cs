namespace CrateBoss.Puzzle
{
    /// <summary>How much plain labour a level asks for (brief §2, "ZAHMET ÖLÇÜLERİ"; lower = better).</summary>
    public sealed class EffortMeasures
    {
        /// <summary>Walking moves (no push) per move.</summary>
        public double WalkRatio;
        /// <summary>Longest run of pushes of one crate the same way.</summary>
        public int StraightRun;
        /// <summary>Pushes per crate that is pushed at all.</summary>
        public double PushesPerCrate;
        /// <summary>Trips from one end of the yard to the other (yards 18 high and more).</summary>
        public int EndToEndTrips;
        public int Moves, Pushes, CratesPushed;
    }
}
