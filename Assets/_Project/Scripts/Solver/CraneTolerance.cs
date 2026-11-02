using CrateBoss.Core;

namespace CrateBoss.Solver
{
    /// <summary>
    /// Slack of a crane level (Ana Belge §10.3 "Tolerans"): how many wasted moves can be added before the shortest
    /// solution and the level is still solvable. Wasted moves are walked in pairs (there and back), so the
    /// player ends where it started; only the machines move on.
    /// </summary>
    public static class CraneTolerance
    {
        public const int NoCrane = -1;

        /// <summary>Largest even waste that still solves, <see cref="NoCrane"/>, or <paramref name="maxWaste"/> when even that solves.</summary>
        public static int Measure(GameState start, LevelSolver solver, int maxWaste = 20)
        {
            if (!start.HasActiveMachine) return NoCrane;
            for (int waste = 2; waste <= maxWaste; waste += 2)
            {
                var s = start.Clone();
                for (int i = 0; i < waste; i++) CraneSystem.StepAll(s);
                if (!solver.Solve(s).IsSolved) return waste - 2;
                if (!s.HasActiveMachine) return maxWaste; // every machine is done: more waste changes nothing
            }
            return maxWaste;
        }
    }
}
