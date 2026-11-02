using System.Collections.Generic;
using CrateBoss.Core;
using CrateBoss.Solver;

namespace CrateBoss.Game
{
    /// <summary>
    /// A level's stored solution (LevelData.solution) replayed once on load: the position after each move is kept, so a
    /// hint from anywhere on that way is the rest of it. Null when there is none or it does not win the level.
    /// </summary>
    public sealed class StoredSolution
    {
        private readonly List<PlayerAction> moves;
        private readonly Dictionary<string, int> stepOf = new Dictionary<string, int>();

        public SolveResult Result { get; }

        private StoredSolution(List<PlayerAction> moves)
        {
            this.moves = moves;
            Result = new SolveResult(true, false, moves, 0);
        }

        public static StoredSolution From(string text, GameState start)
        {
            var moves = SolutionText.Parse(text);
            if (moves == null) return null;
            var s = new StoredSolution(moves);
            var rules = new RulesEngine();
            var state = start.Clone();
            for (int i = 0; i < moves.Count; i++)
            {
                s.stepOf[state.GetKey()] = i;
                var r = rules.TryMove(state, moves[i]);
                if (!r.Success) return null;
                if (r.IsWon) return i == moves.Count - 1 ? s : null;
            }
            return null;
        }

        public bool IsOnWay(string key) => stepOf.ContainsKey(key);

        /// <summary>The rest of the solution from this position, or null when the position is not on its way.</summary>
        public SolveResult From(string key)
        {
            if (!stepOf.TryGetValue(key, out int i)) return null;
            return new SolveResult(true, false, moves.GetRange(i, moves.Count - i), 0);
        }
    }
}
