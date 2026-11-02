using NUnit.Framework;
using CrateBoss.Core;
using CrateBoss.Levels;

namespace CrateBoss.Tests
{
    /// <summary>Fill the truck (Ana Belge §5.5).</summary>
    public class TruckTests
    {
        private static readonly RulesEngine Rules = new RulesEngine();

        // Row: P B . . . T(bay) — a one-cell bay at the end of the corridor.
        private static GameState Board(int count) =>
            GoalBoards.Row(".....T", "PB....", d =>
            {
                d.goal = GoalBoards.Goal("trucks");
                d.trucks = new TruckData { count = count, bay = new[] { new[] { 6, 1 } } };
            });

        [Test]
        public void FullBay_TruckLeavesWithTheCrates_CountDrops()
        {
            var s = Board(2);
            for (int i = 0; i < 3; i++) Rules.TryMove(s, Direction.Right);
            Assert.AreEqual(2, s.TrucksLeft);
            var r = Rules.TryMove(s, Direction.Right);
            Assert.IsNotNull(r.TruckLeft);
            Assert.AreEqual(1, s.TrucksLeft);
            Assert.IsNull(s.GetBoxAt(new GridPos(6, 1)), "the crate left with the truck");
            Assert.AreEqual(0, s.Boxes.Count);
        }

        [Test]
        public void LastTruck_FinishesTheGoal_InTheSameMove()
        {
            var s = Board(1);
            MoveResult r = null;
            for (int i = 0; i < 4; i++) r = Rules.TryMove(s, Direction.Right);
            Assert.AreEqual(0, s.TrucksLeft);
            Assert.IsTrue(r.IsWon, "the truck step runs before the win check (§G.0)");
        }

        [Test]
        public void Undo_BringsTheTruckAndCratesBack()
        {
            var session = new GameSession(Board(2));
            for (int i = 0; i < 3; i++) session.Move(Direction.Right);
            var before = session.State.Clone();
            session.Move(Direction.Right);
            Assert.IsTrue(session.Undo());
            Assert.IsTrue(session.State.SameAs(before));
            Assert.AreEqual(2, session.State.TrucksLeft);
        }

        [Test]
        public void Parser_BayMustBeTargets()
        {
            Assert.Throws<LevelFormatException>(() => GoalBoards.Row("....T", "PB...", d =>
            {
                d.goal = GoalBoards.Goal("trucks");
                d.trucks = new TruckData { count = 1, bay = new[] { new[] { 3, 1 } } };
            }));
        }
    }
}
