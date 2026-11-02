using System.Collections.Generic;
using CrateBoss.Core;

namespace CrateBoss.Solver
{
    /// <summary>Solves a level and measures it: pushes, used mechanics, extra boxes and the trap ratio.</summary>
    public sealed class LevelAnalyzer
    {
        public const int DefaultTrapStateLimit = 400000;

        private readonly RulesEngine rules = new RulesEngine();
        private readonly LevelSolver solver;
        private readonly int trapStateLimit;

        public LevelAnalyzer(int solveLimit = LevelSolver.DefaultMaxStates, int trapStateLimit = DefaultTrapStateLimit)
        {
            // Unpruned: the score counts explored states and must stay the doc's measure.
            solver = new LevelSolver(solveLimit) { PruneDeadCrates = false };
            this.trapStateLimit = trapStateLimit;
        }

        public LevelMetrics Analyze(GameState start, bool measureTraps = true)
        {
            var solution = solver.Solve(start);
            int pushes = 0;
            var used = MechanicFlags.None;
            if (solution.IsSolved)
            {
                var state = start.Clone();
                foreach (var dir in solution.Moves)
                {
                    var r = rules.TryMove(state, dir);
                    if (r.BoxMoves.Count > 0) pushes++;
                    foreach (var m in r.BoxMoves) used |= Uses(m);
                    foreach (var drop in r.CraneDrops)
                        if (drop.FellInto == FallType.Hole) used |= MechanicFlags.Hole;
                    if (r.Sunk.Count > 0 || r.CraneSunk.Count > 0) used |= MechanicFlags.Match;
                }
            }
            // The crane counts even when no drop happens in time: it is the clock the solution races.
            if (start.HasAnyActiveCrane) used |= MechanicFlags.Crane;
            used |= GoalPieces(start);
            double traps = measureTraps ? TrapRatio(start) : -1;
            return new LevelMetrics(solution, pushes, traps, start.Boxes.Count - CountTargets(start), used);
        }

        /// <summary>The first box push or car sent in the shortest solution from this state (for hints), or -1.</summary>
        public static int FirstPushIndex(GameState state, IReadOnlyList<PlayerAction> moves)
        {
            var rules = new RulesEngine();
            var s = state.Clone();
            for (int i = 0; i < moves.Count; i++)
                if (moves[i].IsCar || rules.TryMove(s, moves[i]).BoxMoves.Count > 0) return i; // sending a car counts as the next real move
            return -1;
        }

        /// <summary>Goal pieces present in the level (Ana Belge §5); each one is an element the player has to think about.</summary>
        private static MechanicFlags GoalPieces(GameState s)
        {
            var f = MechanicFlags.None;
            if (s.Truck != null) f |= MechanicFlags.Truck;
            return f;
        }

        private static MechanicFlags Uses(BoxMovement m)
        {
            var flags = m.BoxType == BoxType.Ice ? MechanicFlags.Ice
                : m.BoxType == BoxType.Heavy ? MechanicFlags.Heavy
                : MechanicFlags.Normal;
            if (m.FellInto == FallType.Hole) flags |= MechanicFlags.Hole;
            if (m.FellInto == FallType.Void) flags |= MechanicFlags.Void;
            return flags;
        }

        private static int CountTargets(GameState s)
        {
            int n = 0;
            for (int y = 0; y < s.Height; y++)
            for (int x = 0; x < s.Width; x++)
                if (s.GetTile(new GridPos(x, y)) == TileType.Target) n++;
            return n;
        }

        /// <summary>
        /// Explores every reachable state, then walks back from the won states.
        /// D = states that cannot reach a win / all reachable states. -1 if over the limit.
        /// </summary>
        private double TrapRatio(GameState start)
        {
            var index = new Dictionary<StateHash, int> { { start.GetKeyHash(), 0 } };
            var frontier = new Queue<GameState>();
            frontier.Enqueue(start.Clone());
            var reverse = new List<List<int>> { new List<int>() };
            var won = new List<int>();
            int current = 0;
            var order = new Queue<int>();
            order.Enqueue(0);

            while (frontier.Count > 0)
            {
                var state = frontier.Dequeue();
                current = order.Dequeue();
                if (state.IsSolved()) { won.Add(current); continue; } // play stops on a win
                foreach (var dir in DirectionExtensions.All)
                {
                    var next = state.Clone();
                    if (!rules.TryMove(next, dir).Success) continue;
                    var key = next.GetKeyHash();
                    if (!index.TryGetValue(key, out int id))
                    {
                        id = index.Count;
                        if (id >= trapStateLimit) return -1;
                        index[key] = id;
                        reverse.Add(new List<int>());
                        frontier.Enqueue(next);
                        order.Enqueue(id);
                    }
                    reverse[id].Add(current);
                }
            }

            var canWin = new bool[index.Count];
            var stack = new Stack<int>(won);
            foreach (var w in won) canWin[w] = true;
            while (stack.Count > 0)
            {
                foreach (var prev in reverse[stack.Pop()])
                {
                    if (canWin[prev]) continue;
                    canWin[prev] = true;
                    stack.Push(prev);
                }
            }
            int dead = 0;
            foreach (var ok in canWin) if (!ok) dead++;
            return (double)dead / index.Count;
        }
    }
}
