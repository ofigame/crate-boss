using NUnit.Framework;
using CrateBoss.Core;

namespace CrateBoss.Tests
{
    public class IceBoxTests
    {
        private readonly RulesEngine rules = new RulesEngine();

        [Test]
        public void Ice_SlidesUntilWall()
        {
            var s = TestBoards.Row("......", "PI....");
            var r = rules.TryMove(s, Direction.Right);
            Assert.IsTrue(r.Success);
            Assert.AreEqual(TestBoards.At(5), s.Boxes[0].Position);
            Assert.AreEqual(TestBoards.At(1), s.PlayerPos, "player only steps one cell");
            Assert.AreEqual(4, r.BoxMoves[0].Distance);
        }

        [Test]
        public void Ice_StopsInFrontOfBox()
        {
            var s = TestBoards.Row("......", "PI..B.");
            rules.TryMove(s, Direction.Right);
            Assert.AreEqual(TestBoards.At(3), s.GetBoxById(0).Position);
            Assert.AreEqual(TestBoards.At(4), s.GetBoxById(1).Position);
        }

        [Test]
        public void Ice_SlidesOverTarget()
        {
            var s = TestBoards.Row("..T...", "PI....");
            rules.TryMove(s, Direction.Right);
            Assert.AreEqual(TestBoards.At(5), s.Boxes[0].Position);
        }

        [Test]
        public void Ice_FallsIntoHole_AndFillsIt()
        {
            var s = TestBoards.Row("...o..", "PI....");
            var r = rules.TryMove(s, Direction.Right);
            Assert.IsTrue(r.Success);
            Assert.AreEqual(0, s.Boxes.Count);
            Assert.AreEqual(TileType.FilledHole, s.GetTile(TestBoards.At(3)));
            Assert.AreEqual(FallType.Hole, r.BoxMoves[0].FellInto);
            Assert.AreEqual(TestBoards.At(3), r.BoxMoves[0].To);
            CollectionAssert.Contains(r.FilledHoles, TestBoards.At(3));
        }

        [Test]
        public void Ice_FallsIntoVoid()
        {
            var s = TestBoards.Row("..._..", "PI....");
            var r = rules.TryMove(s, Direction.Right);
            Assert.IsTrue(r.Success);
            Assert.AreEqual(0, s.Boxes.Count);
            Assert.AreEqual(TileType.Void, s.GetTile(TestBoards.At(3)));
            Assert.AreEqual(FallType.Void, r.BoxMoves[0].FellInto);
        }

        [Test]
        public void Ice_AgainstWall_Fails()
        {
            var s = TestBoards.Row("..", "PI");
            Assert.IsFalse(rules.TryMove(s, Direction.Right).Success);
            Assert.AreEqual(0, s.MoveCount);
        }
    }
}
