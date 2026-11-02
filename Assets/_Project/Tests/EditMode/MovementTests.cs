using NUnit.Framework;
using CrateBoss.Core;

namespace CrateBoss.Tests
{
    public class MovementTests
    {
        private readonly RulesEngine rules = new RulesEngine();

        [Test]
        public void WalkIntoWall_Fails_AndDoesNotCount()
        {
            var s = TestBoards.Row("...", "P..");
            var r = rules.TryMove(s, Direction.Left);
            Assert.IsFalse(r.Success);
            Assert.AreEqual(0, s.MoveCount);
            Assert.AreEqual(TestBoards.At(0), s.PlayerPos);
        }

        [Test]
        public void WalkOnFloor_MovesAndCounts()
        {
            var s = TestBoards.Row("...", "P..");
            var r = rules.TryMove(s, Direction.Right);
            Assert.IsTrue(r.Success);
            Assert.AreEqual(1, s.MoveCount);
            Assert.AreEqual(TestBoards.At(1), s.PlayerPos);
            Assert.AreEqual(TestBoards.At(0), r.PlayerFrom);
            Assert.AreEqual(TestBoards.At(1), r.PlayerTo);
        }

        [Test]
        public void UpIsRowMinusOne()
        {
            var s = TestBoards.Make(
                new[] { "###", "#.#", "#.#", "###" },
                new[] { "...", "...", ".P.", "..." });
            Assert.IsTrue(rules.TryMove(s, Direction.Up).Success);
            Assert.AreEqual(new GridPos(1, 1), s.PlayerPos);
        }

        [Test]
        public void PlayerCannotEnterHole()
        {
            var s = TestBoards.Row(".o.", "P..");
            Assert.IsFalse(rules.TryMove(s, Direction.Right).Success);
            Assert.AreEqual(TestBoards.At(0), s.PlayerPos);
        }

        [Test]
        public void PlayerCannotEnterVoid()
        {
            var s = TestBoards.Row("._.", "P..");
            Assert.IsFalse(rules.TryMove(s, Direction.Right).Success);
            Assert.AreEqual(0, s.MoveCount);
        }

        [Test]
        public void FailedMove_IsNotInUndoHistory()
        {
            var session = new GameSession(TestBoards.Row("...", "P.."));
            session.Move(Direction.Left);
            Assert.AreEqual(0, session.UndoCount);
            session.Move(Direction.Right);
            Assert.AreEqual(1, session.UndoCount);
        }
    }
}
