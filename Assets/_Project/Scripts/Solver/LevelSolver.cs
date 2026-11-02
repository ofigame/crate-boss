using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using CrateBoss.Core;

namespace CrateBoss.Solver
{
    /// <summary>
    /// Forward search over the real RulesEngine (Ana Belge §10.3): the same rules as the game, never a copy. Every action is
    /// tried: the four walks / pushes and sending each parked car along its axis. Breadth-first by default (the first win is a
    /// shortest solution, and the explored count is the difficulty measure); <see cref="UseAStar"/> adds a lower bound on the
    /// moves still needed so big yards solve faster with the same shortest answer. Only the frontier keeps whole states, bumps
    /// are skipped before any copy, and positions with a needed crate stuck for good are dropped (DeadCrates).
    /// </summary>
    public sealed class LevelSolver
    {
        public const int DefaultMaxStates = 200000;
        private readonly RulesEngine rules = new RulesEngine();

        public int MaxStates { get; }
        /// <summary>Optional rule on top of the game rules: moves it rejects are never tried (e.g. the monotone test, Ana Belge §10.5).</summary>
        public Func<MoveResult, bool> MoveFilter { get; set; }
        /// <summary>
        /// Drop positions with a needed crate stuck in a corner (DeadCrates). The answer is the same either way; only the
        /// number of explored states changes, so the difficulty score (which counts them) turns this off.
        /// </summary>
        public bool PruneDeadCrates { get; set; } = true;
        /// <summary>A* with an admissible distance bound (Ana Belge §10.3): the same shortest length, fewer states on big yards.</summary>
        public bool UseAStar { get; set; }
        /// <summary>Gives up after this many seconds (Ana Belge §10.3: 60 s); 0 = no time limit.</summary>
        public double MaxSeconds { get; set; }

        public LevelSolver(int maxStates = DefaultMaxStates)
        {
            MaxStates = maxStates;
        }

        /// <summary>Stops every Solve(start) on this thread once cancelled. Default: never.</summary>
        [ThreadStatic] public static CancellationToken ThreadCancel;

        public SolveResult Solve(GameState start) => Solve(start, ThreadCancel);

        /// <summary>A cancelled or timed-out search stops early and reports HitLimit (no answer), never "unsolvable".</summary>
        public SolveResult Solve(GameState start, CancellationToken cancel)
        {
            var root = start.Clone();
            var none = new PlayerAction[0];
            if (root.IsSolved()) return new SolveResult(true, false, none, 1);
            var dead = PruneDeadCrates ? DeadCrates.For(root) : null;
            var bound = UseAStar ? DistanceBound.For(root) : null;
            var clock = MaxSeconds > 0 ? Stopwatch.StartNew() : null;
            var parents = new List<int> { -1 };
            var actions = new List<PlayerAction> { default };
            var depth = new List<int> { 0 };
            var seen = new HashSet<StateHash> { root.GetKeyHash() };
            var open = new StateQueue(bound != null);
            open.Push(root, 0, bound != null ? bound.Estimate(root) : 0);
            int expanded = 0;
            var tries = new List<PlayerAction>(8);

            while (open.Count > 0)
            {
                var (current, index) = open.Pop();
                if ((++expanded & 255) == 0 && (cancel.IsCancellationRequested || clock != null && clock.Elapsed.TotalSeconds > MaxSeconds))
                    return new SolveResult(false, true, none, parents.Count);
                Actions(current, tries);
                foreach (var action in tries)
                {
                    if (!action.IsCar)
                    {
                        // Walking into a wall or a car changes nothing: no copy needed.
                        var to = current.PlayerPos + action.Direction.ToOffset();
                        if (!GoalRules.PlayerCanEnter(current, to)) continue;
                    }
                    var next = current.Clone();
                    var result = rules.TryMove(next, action);
                    if (!result.Success) continue;
                    if (MoveFilter != null && !MoveFilter(result)) continue;
                    if (!seen.Add(next.GetKeyHash())) continue;
                    parents.Add(index);
                    actions.Add(action);
                    int g = depth[index] + 1;
                    depth.Add(g);
                    if (result.IsWon) return new SolveResult(true, false, BuildPath(parents, actions, parents.Count - 1), parents.Count);
                    if (parents.Count >= MaxStates) return new SolveResult(false, true, none, parents.Count);
                    if (dead != null && result.BoxMoves.Count > 0 && dead.IsDead(next)) continue; // counted as reached, never expanded
                    int h = bound != null ? bound.Estimate(next) : 0;
                    if (h < 0) continue; // the bound proves it can never be won
                    open.Push(next, parents.Count - 1, g + h);
                }
            }
            return new SolveResult(false, false, none, parents.Count);
        }

        /// <summary>Every action worth trying from here: the four walks, then each parked car both ways along its axis.</summary>
        public static void Actions(GameState s, List<PlayerAction> into)
        {
            into.Clear();
            foreach (var dir in DirectionExtensions.All) into.Add(dir);
            if (s.Parking == null) return;
            for (int i = 0; i < s.Parking.Cars.Count; i++)
            {
                if (!s.CarPresent(i)) continue;
                bool h = s.Parking.Cars[i].Horizontal;
                into.Add(PlayerAction.SendCar(i, h ? Direction.Left : Direction.Up));
                into.Add(PlayerAction.SendCar(i, h ? Direction.Right : Direction.Down));
            }
        }

        private static List<PlayerAction> BuildPath(List<int> parents, List<PlayerAction> actions, int last)
        {
            var path = new List<PlayerAction>();
            for (int i = last; parents[i] >= 0; i = parents[i]) path.Add(actions[i]);
            path.Reverse();
            return path;
        }
    }
}
