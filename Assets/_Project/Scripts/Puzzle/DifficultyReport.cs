using System.Collections.Generic;

namespace CrateBoss.Puzzle
{
    /// <summary>Everything measured about one level, its two scores and what it misses for its place (brief §2–§4).</summary>
    public sealed class DifficultyReport
    {
        public int Number;
        public string Name;
        public int Width, Height;
        public bool Solvable;
        public int Pushes, Moves;
        public LogicMeasures Logic = new LogicMeasures();
        public EffortMeasures Effort = new EffortMeasures();
        public DoorMeasures Doors = new DoorMeasures();
        public CandyMeasures Candy;
        public double LogicScore, EffortScore;
        /// <summary>What the level misses (empty = UYGUN). Turkish, for the report.</summary>
        public List<string> Problems = new List<string>();
        public bool Easy => Problems.Count > 0;
    }
}
