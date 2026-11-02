using NUnit.Framework;
using CrateBoss.Core;

namespace CrateBoss.Tests
{
    public class UndoTests
    {
        [Test]
        public void Undo_RestoresExactState_IncludingTypesAndFilledHoles()
        {
            var start = TestBoards.Row("..o....", "PB.I.H.");
            var original = start.Clone();
            var session = new GameSession(start);

            var afterFirst = session.State.Clone();
            Assert.IsTrue(session.Move(Direction.Right).Success); // B into the hole
            var afterHole = session.State.Clone();
            Assert.IsTrue(session.Move(Direction.Right).Success); // walk over the filled hole
            Assert.IsTrue(session.Move(Direction.Right).Success); // push ice, it slides to the heavy box
            Assert.AreEqual(3, session.State.MoveCount);

            Assert.IsTrue(session.Undo());
            Assert.IsTrue(session.Undo());
            Assert.IsTrue(session.State.SameAs(afterHole));
            Assert.AreEqual(TileType.FilledHole, session.State.GetTile(TestBoards.At(2)));

            Assert.IsTrue(session.Undo());
            Assert.IsTrue(session.State.SameAs(afterFirst));
            Assert.IsTrue(session.State.SameAs(original));
            Assert.AreEqual(TileType.Hole, session.State.GetTile(TestBoards.At(2)));
            Assert.AreEqual(BoxType.Ice, session.State.GetBoxById(1).Type);
            Assert.AreEqual(BoxType.Heavy, session.State.GetBoxById(2).Type);
            Assert.AreEqual(0, session.State.MoveCount);

            Assert.IsFalse(session.Undo(), "nothing left to undo");
        }

        [Test]
        public void UndoStack_StoresCopies()
        {
            var state = TestBoards.Row("...", "PB.");
            var stack = new UndoStack();
            stack.Push(state);
            state.Boxes[0].Position = TestBoards.At(2);
            Assert.IsTrue(stack.TryPop(out var saved));
            Assert.AreEqual(TestBoards.At(1), saved.Boxes[0].Position);
        }
    }
}
