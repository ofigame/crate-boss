namespace CrateBoss.Puzzle
{
    /// <summary>Budgets of the level hardener (adjustable; brief §5: 10 minutes a level at most).</summary>
    public sealed class HardenOptions
    {
        /// <summary>Seconds for one level, shared by its climbs.</summary>
        public double Seconds = 360;
        /// <summary>Independent climbs (one candidate each: _a, _b, _c).</summary>
        public int Climbs = 3;
        /// <summary>Climb steps without a gain before it starts again from a new random layout.</summary>
        public int Patience = 60;
        public int Seed = 1;
        /// <summary>Search for a candidate's solution.</summary>
        public int SolveStates = 30000;
        public double SolveSeconds = 6;
        /// <summary>Quick measure inside the climb; the full one is run on the results.</summary>
        public MeterOptions Quick = new MeterOptions { TrapSteps = 10, TrapStates = 500, TrapSeconds = 0.25, HastyStates = 12000, HastySeconds = 3, MechanicStates = 8000, MechanicSeconds = 2 };
        public MeterOptions Full = new MeterOptions();
    }
}
