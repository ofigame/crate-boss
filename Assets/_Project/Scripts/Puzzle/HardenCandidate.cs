using CrateBoss.Levels;

namespace CrateBoss.Puzzle
{
    /// <summary>A hardened layout of a level, measured, with its "aha" line.</summary>
    public sealed class HardenCandidate
    {
        public LevelData Data;
        public DifficultyReport Report;
        public double Fitness;
        public string Aha = "";
        /// <summary>It meets every rule of its place (brief §3–§4).</summary>
        public bool ReachedTarget;
    }
}
