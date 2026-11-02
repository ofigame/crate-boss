using NUnit.Framework;
using CrateBoss.Core;

namespace CrateBoss.Tests
{
    public class HeavyBoxTests
    {
        private readonly RulesEngine rules = new RulesEngine();

        [Test]
        public void Heavy_CannotBePushedAlone()
        {
            var s = TestBoards.Row("....", "PH..");
            Assert.IsFalse(rules.TryMove(s, Direction.Right).Success);
            Assert.AreEqual(TestBoards.At(1), s.Boxes[0].Position);
        }

        [Test]
        public void NormalPushesHeavy_BothMoveOneCell()
        {
            var s = TestBoards.Row(".....", "PBH..");
            var r = rules.TryMove(s, Direction.Right);
            Assert.IsTrue(r.Success);
            Assert.AreEqual(TestBoards.At(1), s.PlayerPos);
            Assert.AreEqual(TestBoards.At(2), s.GetBoxById(0).Position, "normal box takes the heavy box's old cell");
            Assert.AreEqual(TestBoards.At(3), s.GetBoxById(1).Position);
            Assert.AreEqual(2, r.BoxMoves.Count);
        }

        [Test]
        public void NormalPushesHeavy_BlockedByWall_Fails()
        {
            var s = TestBoards.Row("...", "PBH");
            Assert.IsFalse(rules.TryMove(s, Direction.Right).Success);
        }

        [Test]
        public void NormalPushesHeavy_BlockedByBox_Fails()
        {
            var s = TestBoards.Row(".....", "PBHB.");
            Assert.IsFalse(rules.TryMove(s, Direction.Right).Success);
        }

        [Test]
        public void NormalPushesHeavy_HeavyFallsIntoHole()
        {
            var s = TestBoards.Row("...o.", "PBH..");
            var r = rules.TryMove(s, Direction.Right);
            Assert.IsTrue(r.Success);
            Assert.AreEqual(1, s.Boxes.Count);
            Assert.AreEqual(BoxType.Normal, s.Boxes[0].Type);
            Assert.AreEqual(TestBoards.At(2), s.Boxes[0].Position);
            Assert.AreEqual(TileType.FilledHole, s.GetTile(TestBoards.At(3)));
        }

        [Test]
        public void IcePushingHeavy_IsNotAChain()
        {
            var s = TestBoards.Row(".....", "PIH..");
            Assert.IsFalse(rules.TryMove(s, Direction.Right).Success);
        }
    }
}
