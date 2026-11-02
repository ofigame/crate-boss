using CrateBoss.Core;

namespace CrateBoss.Puzzle
{
    /// <summary>One push of a solution as a player thinks of it: where the worker stands, and the push (or a car sent).</summary>
    public readonly struct PushStep
    {
        public readonly GridPos Stand;
        public readonly PlayerAction Action;

        public PushStep(GridPos stand, PlayerAction action)
        {
            Stand = stand;
            Action = action;
        }
    }
}
