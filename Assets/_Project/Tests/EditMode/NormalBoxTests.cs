using NUnit.Framework;
using CrateBoss.Core;

namespace CrateBoss.Tests
{
    public class NormalBoxTests
    {
        private readonly RulesEngine rules = new RulesEngine();

        [Test]
        public void Push_MovesBoxOneCell_PlayerTakesItsPlace()
        {
            var s = TestBoards.Row("....", "PB..");
            var r = rules.TryMove(s, Direction.Right);
            Assert.IsTrue(r.Success);
            Assert.AreEqual(TestBoards.At(1), s.PlayerPos);
            Assert.AreEqual(TestBoards.At(2), s.Boxes[0].Position);
            Assert.AreEqual(1, r.BoxMoves.Count);
            Assert.AreEqual(TestBoards.At(1), r.BoxMoves[0].From);
            Assert.AreEqual(TestBoards.At(2), r.BoxMoves[0].To);
            Assert.AreEqual(FallType.None, r.BoxMoves[0].FellInto);
        }

        [Test]
        public void Push_AgainstWall_Fails()
        {
            var s = TestBoards.Row("..", "PB");
            var r = rules.TryMove(s, Direction.Right);
            Assert.IsFalse(r.Success);
            Assert.AreEqual(0, s.MoveCount);
            Assert.AreEqual(TestBoards.At(1), s.Boxes[0].Position);
            Assert.AreEqual(TestBoards.At(0), s.PlayerPos);
        }

        [Test]
        public void TwoNormalBoxes_CannotBePushedTogether()
        {
            var s = TestBoards.Row(".....", "PBB..");
            Assert.IsFalse(rules.TryMove(s, Direction.Right).Success);
            Assert.AreEqual(TestBoards.At(1), s.Boxes[0].Position);
            Assert.AreEqual(TestBoards.At(2), s.Boxes[1].Position);
        }

        [Test]
        public void NormalThenIce_CannotBePushedTogether()
        {
            var s = TestBoards.Row(".....", "PBI..");
            Assert.IsFalse(rules.TryMove(s, Direction.Right).Success);
        }

        [Test]
        public void IceThenNormal_CannotBePushedTogether()
        {
            var s = TestBoards.Row(".....", "PIB..");
            Assert.IsFalse(rules.TryMove(s, Direction.Right).Success);
        }
    }
}
