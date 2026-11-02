using NUnit.Framework;
using CrateBoss.Levels;
using CrateBoss.Solver;

namespace CrateBoss.Tests
{
    /// <summary>
    /// Move to the Crate Boss project (Ana Belge §0): every level keeps the shortest solution
    /// the old Sokoban3D project measured (Docs/sokoban3d_validate_baseline.txt).
    /// </summary>
    public class MigrationTests
    {
        [TestCase("level_01", 3)]
        [TestCase("level_02", 6)]
        [TestCase("level_03", 11)]
        [TestCase("level_04", 9)]
        [TestCase("level_05", 5)]
        [TestCase("level_06", 32)]
        [TestCase("level_07", 8)]
        [TestCase("level_08", 28)]
        [TestCase("level_09", 7)]
        [TestCase("level_10", 43)]
        [TestCase("level_11", 3)]
        [TestCase("level_12", 34)]
        [TestCase("level_13", 51)]
        [TestCase("level_14", 74)]
        [TestCase("level_15", 81)]
        public void ShortestSolution_MatchesOldProject(string file, int moves)
        {
            var loader = new LevelLoader();
            var result = new LevelSolver().Solve(loader.BuildState(loader.Load(file)));
            Assert.IsTrue(result.IsSolved, file);
            Assert.AreEqual(moves, result.MoveCount, file);
        }
    }
}
