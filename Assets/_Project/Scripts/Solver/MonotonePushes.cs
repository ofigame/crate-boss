using System;
using System.Collections.Generic;
using CrateBoss.Core;

namespace CrateBoss.Solver
{
    /// <summary>
    /// The monotone test (Ana Belge §10.5): a restricted solver may never push a box so that its distance to
    /// the nearest target grows (Manhattan distance, walls ignored, so going round a corner counts as moving away).
    /// A level this restricted solver can still solve needs no "backwards" move: it has no idea in it.
    /// </summary>
    public static class MonotonePushes
    {
        public static Func<MoveResult, bool> Filter(GameState start)
        {
            var targets = new List<GridPos>();
            for (int y = 0; y < start.Height; y++)
            for (int x = 0; x < start.Width; x++)
                if (start.GetTile(new GridPos(x, y)) == TileType.Target) targets.Add(new GridPos(x, y));
            int Distance(GridPos p)
            {
                int best = int.MaxValue;
                foreach (var t in targets) best = Math.Min(best, p.ManhattanTo(t));
                return best;
            }
            return result =>
            {
                foreach (var m in result.BoxMoves)
                    if (Distance(m.To) > Distance(m.From)) return false;
                return true;
            };
        }

        /// <summary>True when the level can be won without ever pushing a box away from the targets.</summary>
        public static bool Solvable(GameState start, int maxStates = LevelSolver.DefaultMaxStates)
        {
            var solver = new LevelSolver(maxStates) { MoveFilter = Filter(start) };
            return solver.Solve(start).IsSolved;
        }
    }
}
