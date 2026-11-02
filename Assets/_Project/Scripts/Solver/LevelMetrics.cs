using System;

namespace CrateBoss.Solver
{
    /// <summary>
    /// Measured difficulty of a level (Ana Belge §10).
    /// Score = 0.3·M + 0.5·P + 4·log10(S) + 6·E + 5·K; ScoreWithTraps adds 20·D when D is known.
    /// </summary>
    public sealed class LevelMetrics
    {
        public SolveResult Solution { get; }
        /// <summary>M: moves in the shortest solution.</summary>
        public int Moves => Solution.MoveCount;
        /// <summary>P: box pushes in the shortest solution.</summary>
        public int Pushes { get; }
        /// <summary>S: states explored until the solution was found.</summary>
        public int States => Solution.ExploredStates;
        /// <summary>D: share of reachable states that can no longer be solved; -1 when the space was too big to measure.</summary>
        public double TrapRatio { get; }
        /// <summary>E: boxes minus targets.</summary>
        public int ExtraBoxes { get; }
        public MechanicFlags Mechanics { get; }
        /// <summary>K: number of different elements used by the solution.</summary>
        public int MechanicCount { get; }

        public LevelMetrics(SolveResult solution, int pushes, double trapRatio, int extraBoxes, MechanicFlags mechanics)
        {
            Solution = solution;
            Pushes = pushes;
            TrapRatio = trapRatio;
            ExtraBoxes = extraBoxes;
            Mechanics = mechanics;
            int k = 0;
            for (int bits = (int)mechanics; bits != 0; bits >>= 1) k += bits & 1;
            MechanicCount = k;
        }

        public bool IsSolved => Solution.IsSolved;

        public double Score => 0.3 * Moves + 0.5 * Pushes + 4 * Math.Log10(Math.Max(1, States)) + 6 * ExtraBoxes + 5 * MechanicCount;

        public double ScoreWithTraps => TrapRatio >= 0 ? Score + 20 * TrapRatio : Score;

        public DifficultyTier Tier => TierOf(Score);

        public static DifficultyTier TierOf(double score) =>
            score < 45 ? DifficultyTier.Easy
            : score < 65 ? DifficultyTier.Medium
            : score <= 80 ? DifficultyTier.Hard
            : DifficultyTier.VeryHard;
    }
}
