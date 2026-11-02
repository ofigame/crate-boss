using System.Collections.Generic;
using CrateBoss.Core;

namespace CrateBoss.Puzzle
{
    /// <summary>What a push search found.</summary>
    public sealed class PushSearchResult
    {
        public bool Solved;
        /// <summary>Every position was looked at and none wins: the start can never be won.</summary>
        public bool Exhausted;
        /// <summary>Pushes of the solution (solved) or the depth reached (not solved).</summary>
        public int Pushes;
        public int States;
        public List<PlayerAction> Moves = new List<PlayerAction>();
    }
}
