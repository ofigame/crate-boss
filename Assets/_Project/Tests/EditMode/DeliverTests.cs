using NUnit.Framework;
using CrateBoss.Core;
using CrateBoss.Levels;

namespace CrateBoss.Tests
{
    /// <summary>Delivery crates (Ana Belge §5.1, §21 "Görev kasası"): B areas, locking, the way stop, several deliveries, the heavy one.</summary>
    public class DeliverTests
    {
        private static readonly RulesEngine Rules = new RulesEngine();

        private static DeliveryData D(int x, int y, string area = "B", string via = null, bool heavy = false) =>
            new DeliveryData { box = x < 0 ? null : new[] { x, y }, area = area, via = via, heavy = heavy };

        [Test]
        public void DeliveryCrate_OnItsArea_IsDeliveredAndLocked()
        {
            var s = GoalBoards.Row("...B.", "PD...", d => d.goal = GoalBoards.Goal("deliver"));
            Assert.IsFalse(Rules.TryMove(s, Direction.Right).IsWon);
            var r = Rules.TryMove(s, Direction.Right);
            Assert.IsTrue(r.IsWon);
            Assert.AreEqual(1, r.Deliveries.Count);
            Assert.IsTrue(r.Deliveries[0].Accepted);
            Assert.IsTrue(s.Boxes[0].Locked);
        }

        [Test]
        public void LockedCrate_CannotBePushedAgain()
        {
            var s = GoalBoards.Row("..BB.", "PD...", d => d.goal = GoalBoards.Goal("deliver"));
            s.Goal = GoalType.Targets; // keep playing after the delivery
            Rules.TryMove(s, Direction.Right); // locked on the first B
            var r = Rules.TryMove(s, Direction.Right);
            Assert.IsFalse(r.Success, "a delivered crate is a wall now");
            Assert.AreEqual(new GridPos(3, 1), s.Boxes[0].Position);
        }

        [Test]
        public void PlainCrate_OnTheArea_DoesNotCount()
        {
            var s = GoalBoards.Row("..B.", "PB.D", d => d.goal = GoalBoards.Goal("deliver"));
            Rules.TryMove(s, Direction.Right);
            Assert.IsFalse(Rules.TryMove(s, Direction.Right).IsWon);
        }

        [Test]
        public void WrongArea_IsRefused_AndTheCrateStaysPushable()
        {
            // Two deliveries: crate 0 to area 1, crate 1 to area 2. Crate 0 pushed onto area 2.
            var s = GoalBoards.Make(
                new[] { "#######", "#..2..#", "#.....#", "#..1..#", "#######" },
                new[] { ".......", ".......", ".PD..D.", ".......", "......." },
                d => { d.goal = GoalBoards.Goal("deliver"); d.deliveries = new[] { D(2, 2, "1"), D(5, 2, "2") }; });
            Rules.TryMove(s, Direction.Right);
            var c0 = s.GetBoxAt(new GridPos(3, 2));
            Assert.AreEqual(0, c0.Delivery);
            s.PlayerPos = new GridPos(3, 3);
            var r = Rules.TryMove(s, Direction.Up); // crate 0 onto area 2
            Assert.AreEqual(1, r.Deliveries.Count);
            Assert.IsFalse(r.Deliveries[0].Accepted, "only its own area counts");
            Assert.IsFalse(c0.Locked);
            s.PlayerPos = new GridPos(3, 0);
            Assert.IsTrue(Rules.TryMove(s, Direction.Down).Success, "still pushable");
        }

        [Test]
        public void WayStop_MustComeFirst()
        {
            var noVia = GoalBoards.Row("..M.B.", "PD....", d => { d.goal = GoalBoards.Goal("deliver"); d.deliveries = new[] { D(2, 1, via: "M") }; });
            for (int i = 0; i < 3; i++) Rules.TryMove(noVia, Direction.Right); // through M, then onto B
            Assert.IsTrue(noVia.Boxes[0].ViaDone);
            Assert.IsTrue(noVia.Boxes[0].Locked, "passed M, then B: delivered");

            var skipped = GoalBoards.Make(
                new[] { "#######", "#.B..M#", "#######" },
                new[] { ".......", "...DP..", "......." },
                d => { d.goal = GoalBoards.Goal("deliver"); d.deliveries = new[] { D(3, 1, via: "M") }; });
            var r = Rules.TryMove(skipped, Direction.Left);
            Assert.IsFalse(r.IsWon);
            Assert.IsFalse(r.Deliveries[0].Accepted, "B without M first is refused (red flash)");
            Assert.IsFalse(skipped.Boxes[0].Locked);
        }

        [Test]
        public void TwoDeliveries_BothMustBeLocked()
        {
            var s = GoalBoards.Make(
                new[] { "######", "#...1#", "#....#", "#...2#", "######" },
                new[] { "......", "..D...", ".P....", "..D...", "......" },
                d => d.goal = GoalBoards.Goal("deliver"));
            Assert.AreEqual(2, s.Deliveries.Count);
            s.PlayerPos = new GridPos(1, 1);
            Rules.TryMove(s, Direction.Right);
            Assert.IsFalse(Rules.TryMove(s, Direction.Right).IsWon, "one of two");
            s.PlayerPos = new GridPos(1, 3);
            Rules.TryMove(s, Direction.Right);
            Assert.IsTrue(Rules.TryMove(s, Direction.Right).IsWon);
        }

        [Test]
        public void HeavyDeliveryCrate_MovesOnlyAsAChain()
        {
            var s = GoalBoards.Row(".....B", "PBD...", d => { d.goal = GoalBoards.Goal("deliver"); d.deliveries = new[] { D(3, 1, heavy: true) }; });
            Assert.IsTrue(s.Boxes.Find(b => b.Delivery == 0).IsHeavy);
            var alone = GoalBoards.Row("....B", "PD...", d => { d.goal = GoalBoards.Goal("deliver"); d.deliveries = new[] { D(2, 1, heavy: true) }; });
            Assert.IsFalse(Rules.TryMove(alone, Direction.Right).Success, "not pushed alone");
            for (int i = 0; i < 2; i++) Assert.IsTrue(Rules.TryMove(s, Direction.Right).Success);
            var r = Rules.TryMove(s, Direction.Right);
            Assert.IsTrue(r.IsWon, "the chain pushes it onto B");
        }

        [Test]
        public void CraneBringsTheDeliveryCrate()
        {
            var s = GoalBoards.Make(
                new[] { "######", "#....#", "#..B.#", "######" },
                new[] { "......", ".P....", "......", "......" },
                d =>
                {
                    d.goal = GoalBoards.Goal("deliver");
                    d.deliveries = new[] { D(-1, 0) };
                    d.cranes = new[] { new CraneData { every = 2, drops = new[] { new CraneDropData { x = 2, y = 2, type = "D", deliveryIndex = 0 } } } };
                });
            Assert.AreEqual(0, s.Boxes.Count);
            Rules.TryMove(s, Direction.Right);
            var r = Rules.TryMove(s, Direction.Left);
            Assert.AreEqual(1, r.CraneDrops.Count);
            var crate = s.GetBoxById(r.CraneDrops[0].BoxId);
            Assert.AreEqual(BoxType.Delivery, crate.Type);
            Assert.AreEqual(0, crate.Delivery);
            s.PlayerPos = new GridPos(1, 2);
            Assert.IsTrue(Rules.TryMove(s, Direction.Right).IsWon);
        }

        [Test]
        public void DeliveryCrate_NeverMatches()
        {
            var s = GoalBoards.Row("......B", "P1.11D.", d => d.goal = GoalBoards.Goal("deliver"));
            var r = Rules.TryMove(s, Direction.Right);
            Assert.AreEqual(3, r.Sunk.Count);
            Assert.IsNotNull(s.GetBoxAt(new GridPos(6, 1)), "the delivery crate stays");
        }

        [Test]
        public void Undo_BringsTheLockBack()
        {
            var s = GoalBoards.Row("...B.", "PD...", d => d.goal = GoalBoards.Goal("deliver"));
            var session = new GameSession(s);
            session.Move(Direction.Right);
            var before = session.State.Clone();
            session.Move(Direction.Right);
            Assert.IsTrue(session.State.Boxes[0].Locked);
            session.Undo();
            Assert.IsTrue(session.State.SameAs(before));
            Assert.IsFalse(session.State.Boxes[0].Locked);
        }

        [Test]
        public void Parser_GoalsNeedTheirPieces()
        {
            Assert.Throws<LevelFormatException>(() => GoalBoards.Row("...", "PD.", d => d.goal = GoalBoards.Goal("deliver")));
            Assert.Throws<LevelFormatException>(() => GoalBoards.Row("..B", "P..", d => d.goal = GoalBoards.Goal("deliver")));
            Assert.Throws<LevelFormatException>(() => GoalBoards.Row("...", "P..", d => d.goal = GoalBoards.Goal("cats")), "removed goal");
            Assert.Throws<LevelFormatException>(() => GoalBoards.Row("..B", "PD.", d => d.deliveries = new[] { D(2, 1, via: "M") }), "via without M");
        }
    }
}
