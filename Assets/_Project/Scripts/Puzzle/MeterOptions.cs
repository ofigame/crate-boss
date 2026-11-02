namespace CrateBoss.Puzzle
{
    /// <summary>Search budgets of the difficulty meter (adjustable).</summary>
    public sealed class MeterOptions
    {
        /// <summary>Positions of the solution whose every other push is tried for traps.</summary>
        public int TrapSteps = 20;
        /// <summary>Skip the searches (hasty player, mechanics): only what the solution itself shows (doors, labour, colour crates).</summary>
        public bool SolutionOnly;
        /// <summary>Budget of one trap check (states, seconds).</summary>
        public int TrapStates = 1200;
        public double TrapSeconds = 0.3;
        /// <summary>Budget of the hasty-player search.</summary>
        public int HastyStates = 30000;
        public double HastySeconds = 8;
        /// <summary>Budget of the "is the mechanic needed" search.</summary>
        public int MechanicStates = 20000;
        public double MechanicSeconds = 5;
    }
}
