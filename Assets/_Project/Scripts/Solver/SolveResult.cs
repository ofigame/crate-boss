using System.Collections.Generic;
using CrateBoss.Core;

namespace CrateBoss.Solver
{
    public sealed class SolveResult
    {
        public bool IsSolved { get; }
        /// <summary>True when the search stopped at the state limit (unknown, not proven unsolvable).</summary>
        public bool HitLimit { get; }
        public IReadOnlyList<PlayerAction> Moves { get; }
        public int MoveCount => Moves.Count;
        public int ExploredStates { get; }

        /// <summary>The solution as text (Ana Belge §6 "solution"): U D L R for the worker, "3r" for a car.</summary>
        public string Text => SolutionText.Write(Moves);

        public SolveResult(bool isSolved, bool hitLimit, IReadOnlyList<PlayerAction> moves, int exploredStates)
        {
            IsSolved = isSolved;
            HitLimit = hitLimit;
            Moves = moves;
            ExploredStates = exploredStates;
        }

        public override string ToString() =>
            IsSolved ? $"solved in {MoveCount} moves ({ExploredStates} states)"
            : HitLimit ? $"state limit reached ({ExploredStates} states)"
            : $"unsolvable ({ExploredStates} states)";
    }
}
