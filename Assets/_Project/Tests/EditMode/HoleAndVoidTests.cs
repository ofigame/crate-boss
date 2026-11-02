using NUnit.Framework;
using CrateBoss.Core;

namespace CrateBoss.Tests
{
    public class HoleAndVoidTests
    {
        private readonly RulesEngine rules = new RulesEngine();

        [Test]
        public void BoxFallsIntoHole_HoleFills_PlayerCanWalkOverIt()
        {
            var s = TestBoards.Row("..o..", "PB...");
            var r = rules.TryMove(s, Direction.Right);
            Assert.IsTrue(r.Success);
            Assert.AreEqual(0, s.Boxes.Count);
            Assert.AreEqual(TileType.FilledHole, s.GetTile(TestBoards.At(2)));
            Assert.AreEqual(FallType.Hole, r.BoxMoves[0].FellInto);

            Assert.IsTrue(rules.TryMove(s, Direction.Right).Success);
            Assert.AreEqual(TestBoards.At(2), s.PlayerPos);
            Assert.IsTrue(rules.TryMove(s, Direction.Right).Success);
        }

        [Test]
        public void BoxCanBePushedOverFilledHole()
        {
            var s = TestBoards.Row("..o...", "PB.B..");
            rules.TryMove(s, Direction.Right); // first box fills the hole
            rules.TryMove(s, Direction.Right); // player onto filled hole, pushing nothing yet
            var r = rules.TryMove(s, Direction.Right);
            Assert.IsTrue(r.Success);
            Assert.AreEqual(TestBoards.At(4), s.Boxes[0].Position);
        }

        [Test]
        public void BoxPushedIntoVoid_Disappears_VoidStays()
        {
            var s = TestBoards.Row(".._..", "PB...");
            var r = rules.TryMove(s, Direction.Right);
            Assert.IsTrue(r.Success);
            Assert.AreEqual(0, s.Boxes.Count);
            Assert.AreEqual(TileType.Void, s.GetTile(TestBoards.At(2)));
            Assert.AreEqual(FallType.Void, r.BoxMoves[0].FellInto);
            Assert.IsFalse(rules.TryMove(s, Direction.Right).Success, "player still cannot step into void");
        }
    }
}
