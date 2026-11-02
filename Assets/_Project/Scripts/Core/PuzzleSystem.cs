namespace CrateBoss.Core
{
    /// <summary>
    /// The classic puzzle in the middle of a candy yard (owner 2026-11-02, later leagues): its crates cannot be pushed while the
    /// candy part lasts; afterwards, once every target holds a crate, it pays hammers by how close to the fewest pushes it was
    /// solved, and the barriers (G) open so the cargo can go to the trucks.
    /// </summary>
    public static class PuzzleSystem
    {
        /// <summary>A plain crate of the puzzle, still locked because the candy part is not over.</summary>
        public static bool Locked(GameState s, Box box) =>
            s.HasPuzzle && s.IsCandyLevel && !s.CandyDone && box.Type == BoxType.Normal && box.Symbol == 0;

        /// <summary>Hammers for solving it in <paramref name="pushes"/>: all of them in par, then less and less.</summary>
        public static int Reward(int reward, int par, int pushes)
        {
            if (pushes <= par) return reward;
            if (pushes * 2 <= par * 3) return System.Math.Max(1, reward * 7 / 10);
            if (pushes <= par * 2) return System.Math.Max(1, reward * 45 / 100);
            return System.Math.Max(1, reward / 4);
        }

        /// <summary>After a successful move: counts the puzzle's pushes and, the first time every target is filled, pays and opens.</summary>
        public static void Step(GameState s, MoveResult result)
        {
            if (!s.HasPuzzle || s.PuzzleDone || !s.CandyDone) return;
            if (result.BoxMoves.Count > 0) s.PuzzlePushes++;
            bool any = false;
            foreach (int i in s.TargetCells)
            {
                var p = s.CellAt(i);
                if (!GoalRules.IsGoalTarget(s, p)) continue;
                any = true;
                if (!GoalRules.IsCovered(s, p)) return;
            }
            if (!any) return;
            s.PuzzleDone = true;
            s.GatesOpen = true;
            int pay = Reward(s.PuzzleReward, s.PuzzlePar, s.PuzzlePushes);
            s.Hammers += pay;
            result.PuzzleReward = pay;
            result.GatesOpened = true;
        }
    }
}
