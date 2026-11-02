using CrateBoss.Core;
using CrateBoss.Levels;
using CrateBoss.Solver;
using NUnit.Framework;

namespace CrateBoss.Tests
{
    /// <summary>The solver's corner pruning only saves time: the same answers, a stuck crate is seen as lost.</summary>
    public class SolverPruningTests
    {
        [Test]
        public void CrateInAWallCorner_OffTarget_IsDead()
        {
            // One row inside a wall ring: a crate in the middle can still move, one at the end is in a corner.
            var free = TestBoards.Row("T...", ".PB.");
            Assert.IsTrue(DeadCrates.For(free).Enabled);
            Assert.IsFalse(DeadCrates.For(free).IsDead(free));
            var corner = TestBoards.Row("T...", ".P.B");
            Assert.IsTrue(DeadCrates.For(corner).IsDead(corner), "a crate in a corner off target can never reach the target");
        }

        [Test]
        public void Pruning_GivesTheSameShortestSolutions()
        {
            var loader = new LevelLoader();
            foreach (var name in new[] { "level_03", "level_08", "level_10", "level_12" })
            {
                var s = loader.BuildState(loader.Load(name));
                var pruned = new LevelSolver().Solve(s);
                var full = new LevelSolver { PruneDeadCrates = false }.Solve(s);
                Assert.AreEqual(full.IsSolved, pruned.IsSolved, name);
                Assert.AreEqual(full.MoveCount, pruned.MoveCount, name);
                Assert.LessOrEqual(pruned.ExploredStates, full.ExploredStates, name);
            }
        }
    }
}
