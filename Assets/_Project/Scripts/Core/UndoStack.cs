using System.Collections.Generic;

namespace CrateBoss.Core
{
    /// <summary>Unlimited undo: full copies of the state (boards are tiny).</summary>
    public sealed class UndoStack
    {
        private readonly Stack<GameState> states = new Stack<GameState>();

        public int Count => states.Count;

        public void Push(GameState state) => states.Push(state.Clone());

        public bool TryPop(out GameState state)
        {
            if (states.Count == 0)
            {
                state = null;
                return false;
            }
            state = states.Pop();
            return true;
        }

        public void Clear() => states.Clear();

        /// <summary>The saved states, the newest first (the hint looks back for one that can still be won, Ana Belge §14).</summary>
        public IEnumerable<GameState> Newest() => states;
    }
}
