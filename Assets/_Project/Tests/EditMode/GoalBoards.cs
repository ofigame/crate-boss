using System;
using CrateBoss.Core;
using CrateBoss.Levels;

namespace CrateBoss.Tests
{
    /// <summary>Small boards with goal pieces (Ana Belge §5), built through the real parser.</summary>
    internal static class GoalBoards
    {
        public static GameState Make(string[] ground, string[] objects, Action<LevelData> setup = null)
        {
            var data = new LevelData { id = 0, name = "test", ground = ground, objects = objects };
            setup?.Invoke(data);
            return new LevelParser().Parse(data);
        }

        /// <summary>One corridor row framed by walls (like TestBoards.Row) with goals.</summary>
        public static GameState Row(string ground, string objects, Action<LevelData> setup = null)
        {
            string wall = new string('#', ground.Length + 2);
            string empty = new string('.', ground.Length + 2);
            return Make(new[] { wall, "#" + ground + "#", wall }, new[] { empty, "." + objects + ".", empty }, setup);
        }

        public static string[] Goal(params string[] names) => names;
    }
}
