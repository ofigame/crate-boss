using NUnit.Framework;
using CrateBoss.Core;
using CrateBoss.Levels;
using CrateBoss.Solver;

namespace CrateBoss.Tests
{
    public class LevelSolveTests
    {
        [TestCase("level_01")]
        [TestCase("level_02")]
        [TestCase("level_03")]
        [TestCase("level_04")]
        [TestCase("level_05")]
        [TestCase("level_06")]
        [TestCase("level_07")]
        [TestCase("level_08")]
        [TestCase("level_09")]
        [TestCase("level_10")]
        [TestCase("level_11")]
        [TestCase("level_12")]
        [TestCase("level_13")]
        [TestCase("level_14")]
        [TestCase("level_15")]
        public void Level_IsSolvable(string file)
        {
            var loader = new LevelLoader();
            var start = loader.BuildState(loader.Load(file));
            var result = new LevelSolver().Solve(start);
            Assert.IsTrue(result.IsSolved, $"{file}: {result}");

            // Replaying the solution with the real rules must win on the last move.
            var session = new GameSession(start.Clone());
            MoveResult last = null;
            foreach (var dir in result.Moves)
            {
                last = session.Move(dir);
                Assert.IsTrue(last.Success, $"{file}: solver move {dir} failed on replay");
            }
            Assert.IsNotNull(last);
            Assert.IsTrue(last.IsWon, $"{file}: replay did not win");
        }

        [Test]
        public void Solver_ReportsUnsolvable()
        {
            var s = TestBoards.Row("..T", "BP."); // box against the left wall can never reach the target
            var result = new LevelSolver().Solve(s);
            Assert.IsFalse(result.IsSolved);
            Assert.IsFalse(result.HitLimit);
        }

        [Test]
        public void Solver_FindsShortestPath()
        {
            var result = new LevelSolver().Solve(TestBoards.Row("....T", "PB..."));
            Assert.AreEqual(3, result.MoveCount);
        }

        [Test]
        public void Parser_RejectsBadLevels()
        {
            var parser = new LevelParser();
            Assert.Throws<LevelFormatException>(() => parser.Parse(new LevelData { ground = new[] { "#.#" }, objects = new[] { ".." } }));
            Assert.Throws<LevelFormatException>(() => parser.Parse(new LevelData { ground = new[] { "..." }, objects = new[] { "PP." } }));
            Assert.Throws<LevelFormatException>(() => parser.Parse(new LevelData { ground = new[] { "..." }, objects = new[] { "..." } }));
            Assert.Throws<LevelFormatException>(() => parser.Parse(new LevelData { ground = new[] { "#.." }, objects = new[] { "BP." } }));
        }
    }
}
