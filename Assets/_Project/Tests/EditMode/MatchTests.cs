using NUnit.Framework;
using CrateBoss.Core;
using CrateBoss.Levels;
using CrateBoss.Solver;

namespace CrateBoss.Tests
{
    /// <summary>Colour crates (Ana Belge §5.4): three or more in a row sink, the cells become floor, nothing slides in.</summary>
    public class MatchTests
    {
        private static readonly RulesEngine Rules = new RulesEngine();

        // ---------- matches ----------

        [Test]
        public void ThreeInARow_Sink()
        {
            var s = GoalBoards.Row("......", "P1.11.");
            var r = Rules.TryMove(s, Direction.Right);
            Assert.IsTrue(r.Success);
            Assert.AreEqual(3, r.Sunk.Count);
            Assert.AreEqual(0, s.Boxes.Count);
        }

        [Test]
        public void VerticalRun_FromAPush_Sinks()
        {
            var s = GoalBoards.Make(
                new[] { "######", "#....#", "#....#", "#....#", "######" },
                new[] { "......", "...1..", ".P1...", "...1..", "......" });
            var r = Rules.TryMove(s, Direction.Right); // crate (2,2) -> (3,2): column x=3 rows 1-3
            Assert.AreEqual(3, r.Sunk.Count);
            Assert.AreEqual(0, s.Boxes.Count);
        }

        [Test]
        public void FourInARow_AllSink()
        {
            var s = GoalBoards.Row(".......", "P1.111.");
            var r = Rules.TryMove(s, Direction.Right);
            Assert.AreEqual(4, r.Sunk.Count);
        }

        [Test]
        public void TwoInARow_OrMixedSymbols_Stay()
        {
            var s = GoalBoards.Row("......", "P1.1..");
            Assert.AreEqual(0, Rules.TryMove(s, Direction.Right).Sunk.Count);
            s = GoalBoards.Row("......", "P1.12.");
            Assert.AreEqual(0, Rules.TryMove(s, Direction.Right).Sunk.Count);
            Assert.AreEqual(3, s.Boxes.Count);
        }

        [Test]
        public void SymbolCrate_DoesNotFillATarget()
        {
            var s = GoalBoards.Row("..T.", "P1..");
            Rules.TryMove(s, Direction.Right);
            var r = Rules.TryMove(s, Direction.Right);
            Assert.IsTrue(r.Success);
            Assert.IsFalse(r.IsWon);
            Assert.IsFalse(GoalRules.IsCovered(s, new GridPos(3, 1)));
        }
        [Test]
        public void SunkCells_BecomeFloor_AndNothingSlidesIn()
        {
            var s = GoalBoards.Row(".......", "P1.11B.");
            Rules.TryMove(s, Direction.Right);
            Assert.AreEqual(1, s.Boxes.Count, "only the plain crate is left");
            Assert.AreEqual(new GridPos(6, 1), s.Boxes[0].Position, "it did not move");
            Assert.AreEqual(TileType.Floor, s.GetTile(new GridPos(3, 1)));
        }

        [Test]
        public void LongRuns_PayTheBonus()
        {
            var s = GoalBoards.Row(".......", "P1.111.");
            Assert.AreEqual(5, Rules.TryMove(s, Direction.Right).MatchBonus, "4 in a row: +5");
            s = GoalBoards.Row("........", "P1.1111.");
            Assert.AreEqual(10, Rules.TryMove(s, Direction.Right).MatchBonus, "5 in a row: +10");
        }

        // ---------- undo, solver ----------

        [Test]
        public void Undo_RestoresSunkCrates()
        {
            var s = GoalBoards.Row("........", "P1.11...");
            var session = new GameSession(s);
            var before = s.Clone();
            session.Move(Direction.Right);
            Assert.AreEqual(0, session.State.Boxes.Count);
            session.Undo();
            Assert.IsTrue(session.State.SameAs(before));
        }

        [Test]
        public void Solver_OpensTheWayWithAMatch()
        {
            var s = GoalBoards.Make(
                new[] { "#######", "###T###", "#.....#", "#.....#", "#.....#", "#######" },
                new[] { ".......", ".......", "...11..", "..1B...", ".P.....", "......." });
            var m = new LevelAnalyzer(20000).Analyze(s, false);
            Assert.IsTrue(m.Solution.IsSolved);
            Assert.IsTrue((m.Mechanics & MechanicFlags.Match) != 0, "the shortest way sinks the three 1s");
        }
    }
}
