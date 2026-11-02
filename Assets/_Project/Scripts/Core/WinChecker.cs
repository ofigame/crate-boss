namespace CrateBoss.Core
{
    /// <summary>Is the level won? Every goal type in <see cref="GameState.Goal"/> must be done (Ana Belge §5.7).</summary>
    public static class WinChecker
    {
        public static bool IsWon(GameState s)
        {
            var g = s.Goal;
            if ((g & GoalType.Targets) != 0 && !TargetsDone(s, g == GoalType.Targets)) return false;
            if ((g & GoalType.Trucks) != 0 && s.TrucksLeft > 0) return false;
            if ((g & GoalType.Deliver) != 0 && !DeliverySystem.AllDelivered(s)) return false;
            return true;
        }

        /// <param name="only">The classic goal alone: a level without targets is never won.</param>
        private static bool TargetsDone(GameState s, bool only)
        {
            bool any = false;
            foreach (int i in s.TargetCells)
            {
                var p = s.CellAt(i);
                if (!GoalRules.IsGoalTarget(s, p)) continue;
                any = true;
                if (!GoalRules.IsCovered(s, p)) return false;
            }
            return any || !only;
        }
    }
}
