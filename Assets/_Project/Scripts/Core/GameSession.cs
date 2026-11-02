namespace CrateBoss.Core
{
    /// <summary>One level being played: current state plus undo history.</summary>
    public sealed class GameSession
    {
        private readonly RulesEngine rules = new RulesEngine();
        private readonly UndoStack undo = new UndoStack();

        public GameState State { get; private set; }
        public int UndoCount => undo.Count;
        /// <summary>Earlier positions, the newest first.</summary>
        public System.Collections.Generic.IEnumerable<GameState> History => undo.Newest();

        public GameSession(GameState start)
        {
            State = start;
        }

        /// <summary>Cars sent into something this level (Ana Belge §5.8: none for three stars, at most two for two). Undo keeps it.</summary>
        public int Collisions { get; private set; }

        public MoveResult Move(Direction dir) => Move(PlayerAction.Walk(dir));

        /// <summary>Only a successful move saves the previous state for undo; a car that bumps only counts a collision.</summary>
        public MoveResult Move(PlayerAction action) => Move(action, true);

        /// <param name="undoable">False for the auto hammer's blows (owner 2026-11-02): they belong to the move before them.</param>
        public MoveResult Move(PlayerAction action, bool undoable)
        {
            var before = undoable ? State.Clone() : null;
            var result = rules.TryMove(State, action);
            if (result.Success && undoable) undo.Push(before);
            else if (result.Car != null) Collisions++;
            return result;
        }

        public bool Undo()
        {
            if (!undo.TryPop(out var previous)) return false;
            State = previous;
            return true;
        }
    }
}
