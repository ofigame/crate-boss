namespace CrateBoss.Core
{
    /// <summary>Goal names as written in level files ("targets", "trucks", "deliver"; Ana Belge §5.7).</summary>
    public static class LevelGoal
    {
        public static bool TryParse(string name, out GoalType goal)
        {
            switch (name)
            {
                case "targets": goal = GoalType.Targets; return true;
                case "trucks": case "truck": goal = GoalType.Trucks; return true;
                case "deliver": goal = GoalType.Deliver; return true;
                default: goal = GoalType.None; return false;
            }
        }

        public static string Name(GoalType goal)
        {
            switch (goal)
            {
                case GoalType.Trucks: return "trucks";
                case GoalType.Deliver: return "deliver";
                default: return "targets";
            }
        }
    }
}
