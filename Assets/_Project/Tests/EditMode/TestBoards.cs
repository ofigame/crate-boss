using CrateBoss.Core;
using CrateBoss.Levels;

namespace CrateBoss.Tests
{
    /// <summary>Builds small boards from two layers, the same way level files do.</summary>
    internal static class TestBoards
    {
        public static GameState Make(string[] ground, string[] objects) =>
            new LevelParser().Parse(new LevelData { id = 0, name = "test", ground = ground, objects = objects });

        /// <summary>A single corridor row framed by walls. Ground and objects are the inner cells only.</summary>
        public static GameState Row(string ground, string objects)
        {
            string wall = new string('#', ground.Length + 2);
            string empty = new string('.', ground.Length + 2);
            return Make(new[] { wall, "#" + ground + "#", wall }, new[] { empty, "." + objects + ".", empty });
        }

        public static GridPos At(int innerX) => new GridPos(innerX + 1, 1);
    }
}
