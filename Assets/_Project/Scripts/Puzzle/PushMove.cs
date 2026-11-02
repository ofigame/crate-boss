using System.Collections.Generic;
using CrateBoss.Core;

namespace CrateBoss.Puzzle
{
    /// <summary>A push the worker can make from a position: the moves it takes (walk + push), the position after, what it moved.</summary>
    public sealed class PushMove
    {
        public PushStep Step;
        public List<PlayerAction> Moves;
        public GameState After;
        public bool Won;
        /// <summary>The crate pushed (-1 for a car).</summary>
        public int BoxId = -1;
        public GridPos From, To;
    }
}
