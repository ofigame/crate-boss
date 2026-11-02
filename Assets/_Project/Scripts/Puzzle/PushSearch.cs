using System;
using System.Collections.Generic;
using System.Diagnostics;
using CrateBoss.Core;
using CrateBoss.Solver;

namespace CrateBoss.Puzzle
{
    /// <summary>
    /// Search over pushes, the way a player thinks (walking is free, only crate moves and car sends count), on the real
    /// RulesEngine: a step walks the shortest way (real moves, so cranes tick and the truck can come) to the cell beside a
    /// crate and pushes it. Positions are the same when crates, machines and the worker's reachable area are the same.
    /// Breadth-first finds the fewest pushes (exact without cranes; with cranes only shortest walks are tried).
    /// </summary>
    public static class PushSearch
    {
        private static readonly RulesEngine Rules = new RulesEngine();

        /// <summary>Cells the worker reaches without pushing, each with the cell it is reached from (the start maps to itself).</summary>
        public static Dictionary<GridPos, GridPos> Reach(GameState s)
        {
            var prev = new Dictionary<GridPos, GridPos> { [s.PlayerPos] = s.PlayerPos };
            var q = new Queue<GridPos>();
            q.Enqueue(s.PlayerPos);
            while (q.Count > 0)
            {
                var p = q.Dequeue();
                foreach (var dir in DirectionExtensions.All)
                {
                    var n = p + dir.ToOffset();
                    if (prev.ContainsKey(n) || s.GetBoxAt(n) != null || !GoalRules.PlayerCanEnter(s, n)) continue;
                    prev[n] = p;
                    q.Enqueue(n);
                }
            }
            return prev;
        }

        public static List<PlayerAction> WalkTo(Dictionary<GridPos, GridPos> reach, GridPos from, GridPos to)
        {
            var path = new List<PlayerAction>();
            for (var c = to; c != from; c = reach[c])
            {
                var p = reach[c];
                path.Add(c.X > p.X ? Direction.Right : c.X < p.X ? Direction.Left : c.Y > p.Y ? Direction.Down : Direction.Up);
            }
            path.Reverse();
            return path;
        }

        /// <summary>Every push the worker can reach from here and every car send that leaves.</summary>
        public static List<PushMove> Expand(GameState s, Func<GameState, PushMove, bool> allow = null)
        {
            var list = new List<PushMove>();
            var reach = Reach(s);
            foreach (var stand in reach.Keys)
                foreach (var dir in DirectionExtensions.All)
                {
                    var target = stand + dir.ToOffset();
                    var box = s.GetBoxAt(target);
                    if (box == null) continue;
                    var moves = WalkTo(reach, s.PlayerPos, stand);
                    moves.Add(dir);
                    var after = s.Clone();
                    bool ok = true, won = false;
                    MoveResult last = null;
                    for (int i = 0; i < moves.Count; i++)
                    {
                        var r = Rules.TryMove(after, moves[i]);
                        if (!r.Success || i < moves.Count - 1 && r.BoxMoves.Count > 0) { ok = false; break; }
                        if (r.IsWon && i < moves.Count - 1) { ok = false; break; }
                        won = r.IsWon;
                        last = r;
                    }
                    if (!ok || last == null || last.BoxMoves.Count == 0) continue;
                    var m = new PushMove
                    {
                        Step = new PushStep(stand, dir), Moves = moves, After = after, Won = won,
                        BoxId = box.Id, From = target, To = last.BoxMoves[last.BoxMoves.Count - 1].BoxId == box.Id ? last.BoxMoves[last.BoxMoves.Count - 1].To : target + dir.ToOffset(),
                    };
                    foreach (var bm in last.BoxMoves) if (bm.BoxId == box.Id) m.To = bm.To;
                    if (allow == null || allow(s, m)) list.Add(m);
                }
            if (s.Parking != null)
                for (int i = 0; i < s.Parking.Cars.Count; i++)
                {
                    if (!s.CarPresent(i)) continue;
                    bool h = s.Parking.Cars[i].Horizontal;
                    foreach (var d in h ? new[] { Direction.Left, Direction.Right } : new[] { Direction.Up, Direction.Down })
                    {
                        var a = PlayerAction.SendCar(i, d);
                        var after = s.Clone();
                        var r = Rules.TryMove(after, a);
                        if (!r.Success) continue;
                        var m = new PushMove { Step = new PushStep(s.PlayerPos, a), Moves = new List<PlayerAction> { a }, After = after, Won = r.IsWon };
                        if (allow == null || allow(s, m)) list.Add(m);
                    }
                }
            return list;
        }

        /// <summary>The position with the worker moved to the first cell of his reachable area (walking inside it is free).</summary>
        public static StateHash Canon(GameState s)
        {
            var keep = s.PlayerPos;
            var best = keep;
            int bestIndex = s.Index(keep);
            foreach (var p in Reach(s).Keys)
            {
                int i = s.Index(p);
                if (i < bestIndex) { bestIndex = i; best = p; }
            }
            s.PlayerPos = best;
            var h = s.GetKeyHash();
            s.PlayerPos = keep;
            return h;
        }

        /// <summary>
        /// Breadth-first over pushes from <paramref name="start"/> until a win (or <paramref name="goal"/>), the state limit
        /// or the time limit. Exhausted = every position was seen and none wins.
        /// </summary>
        public static PushSearchResult Bfs(GameState start, int maxStates, double seconds, Func<GameState, bool> goal = null, Func<GameState, PushMove, bool> allow = null)
        {
            var clock = Stopwatch.StartNew();
            var res = new PushSearchResult();
            var dead = DeadCrates.For(start);
            var parent = new List<int> { -1 };
            var via = new List<List<PlayerAction>> { null };
            var seen = new HashSet<StateHash> { Canon(start) };
            var layer = new List<(GameState s, int node)> { (start.Clone(), 0) };
            int depth = 0;
            while (layer.Count > 0)
            {
                depth++;
                var next = new List<(GameState, int)>();
                foreach (var (s, node) in layer)
                    foreach (var m in Expand(s, allow))
                    {
                        if (!seen.Add(Canon(m.After))) continue;
                        parent.Add(node);
                        via.Add(m.Moves);
                        int id = parent.Count - 1;
                        if (m.Won || goal != null && goal(m.After))
                        {
                            res.Solved = true;
                            res.Pushes = depth;
                            res.States = parent.Count;
                            res.Moves = Path(parent, via, id);
                            return res;
                        }
                        if (dead != null && dead.IsDead(m.After) || Stuck(m.After)) continue;
                        next.Add((m.After, id));
                        if (parent.Count >= maxStates || clock.Elapsed.TotalSeconds > seconds)
                        {
                            res.Pushes = depth;
                            res.States = parent.Count;
                            return res;
                        }
                    }
                layer = next;
            }
            res.Exhausted = true;
            res.Pushes = depth;
            res.States = parent.Count;
            return res;
        }

        /// <summary>Best-first on pushes + weight × estimate: finds a short way fast (not always the shortest).</summary>
        public static PushSearchResult Greedy(GameState start, int maxStates, double seconds, int weight, Func<GameState, PushMove, bool> allow = null)
        {
            var clock = Stopwatch.StartNew();
            var res = new PushSearchResult();
            var parent = new List<int> { -1 };
            var via = new List<List<PlayerAction>> { null };
            var seen = new HashSet<StateHash> { Canon(start) };
            var open = new MinHeap<(GameState s, int node, int g)>();
            open.Push((start.Clone(), 0, 0), weight * Estimate(start));
            while (open.Count > 0)
            {
                var (s, node, g) = open.Pop();
                foreach (var m in Expand(s, allow))
                {
                    if (!seen.Add(Canon(m.After))) continue;
                    parent.Add(node);
                    via.Add(m.Moves);
                    int id = parent.Count - 1;
                    if (m.Won)
                    {
                        res.Solved = true;
                        res.Pushes = g + 1;
                        res.States = parent.Count;
                        res.Moves = Path(parent, via, id);
                        return res;
                    }
                    if (Stuck(m.After)) continue;
                    open.Push((m.After, id, g + 1), g + 1 + weight * Estimate(m.After));
                    if (parent.Count >= maxStates || clock.Elapsed.TotalSeconds > seconds) { res.States = parent.Count; return res; }
                }
            }
            res.Exhausted = true;
            res.States = parent.Count;
            return res;
        }

        private static List<PlayerAction> Path(List<int> parent, List<List<PlayerAction>> via, int last)
        {
            var parts = new List<List<PlayerAction>>();
            for (int i = last; parent[i] >= 0; i = parent[i]) parts.Add(via[i]);
            parts.Reverse();
            var all = new List<PlayerAction>();
            foreach (var p in parts) all.AddRange(p);
            return all;
        }

        /// <summary>How far the crate still has to go (Manhattan, walls ignored): delivery crates to their area, cargo to the
        /// truck, plain crates to the nearest target on target levels; -1 for a crate with no goal of its own.</summary>
        public static int GoalDistance(GameState s, Box b)
        {
            if (b.Locked) return 0;
            int best = int.MaxValue;
            if (b.Delivery >= 0)
            {
                if (s.AreaOf == null) return -1;
                int area = s.Deliveries[b.Delivery].Area;
                for (int i = 0; i < s.AreaOf.Length; i++) if (s.AreaOf[i] == area) best = Math.Min(best, b.Position.ManhattanTo(s.CellAt(i)));
            }
            else if (b.IsCargo && s.Truck != null)
                foreach (var c in s.Truck.Cells) best = Math.Min(best, b.Position.ManhattanTo(c));
            else if ((s.Goal & GoalType.Targets) != 0 && s.Truck == null && b.Symbol == 0 && b.Type != BoxType.Container)
                foreach (int i in s.TargetCells) best = Math.Min(best, b.Position.ManhattanTo(s.CellAt(i)));
            return best == int.MaxValue ? -1 : best;
        }

        /// <summary>Pushes still needed at least: every crate with a goal, its distance.</summary>
        public static int Estimate(GameState s)
        {
            int h = 0;
            foreach (var b in s.Boxes)
            {
                if (b.Delivery < 0 && !b.IsCargo) continue;
                int d = GoalDistance(s, b);
                if (d > 0) h += d;
            }
            return h;
        }

        /// <summary>A delivery or cargo crate in a corner of walls, off its goal: it can never move again, the level is lost.</summary>
        public static bool Stuck(GameState s)
        {
            foreach (var b in s.Boxes)
            {
                if (b.Type != BoxType.Delivery || b.Locked) continue;
                var p = b.Position;
                if (b.IsCargo ? s.Truck != null && s.Truck.Contains(p) : s.GetTile(p) == TileType.Arrival) continue;
                bool Wall(int dx, int dy) => s.GetTile(new GridPos(p.X + dx, p.Y + dy)) == TileType.Wall;
                if ((Wall(-1, 0) || Wall(1, 0)) && (Wall(0, -1) || Wall(0, 1))) return true;
            }
            return false;
        }

        /// <summary>A solution as its pushes and car sends, with the position before each.</summary>
        public static List<(GameState before, PushStep step, MoveResult result)> Steps(GameState start, IReadOnlyList<PlayerAction> solution)
        {
            var s = start.Clone();
            var list = new List<(GameState, PushStep, MoveResult)>();
            foreach (var a in solution)
            {
                var before = s.Clone();
                var r = Rules.TryMove(s, a);
                if (!r.Success) return null;
                if (a.IsCar || r.BoxMoves.Count > 0) list.Add((before, new PushStep(before.PlayerPos, a), r));
            }
            return list;
        }

        /// <summary>A solution as its pushes (where the worker stands, which way) and car sends.</summary>
        public static List<PushStep> ToPushSteps(GameState start, IReadOnlyList<PlayerAction> solution)
        {
            var list = new List<PushStep>();
            foreach (var (_, step, _) in Steps(start, solution) ?? new List<(GameState, PushStep, MoveResult)>()) list.Add(step);
            return list;
        }

        /// <summary>Drops pushes nobody needs: each push (newest first) is left out and the rest replayed with fresh shortest
        /// walks; when the level is still won it stays out. Returns the moves of the shorter solution (or the same).</summary>
        public static List<PlayerAction> Prune(GameState start, IReadOnlyList<PlayerAction> solution)
        {
            var steps = ToPushSteps(start, solution);
            if (steps.Count == 0) return new List<PlayerAction>(solution);
            for (int i = steps.Count - 1; i >= 0; i--)
            {
                var trial = new List<PushStep>(steps);
                trial.RemoveAt(i);
                if (Play(start, trial) != null) steps = trial;
            }
            return Play(start, steps) ?? new List<PlayerAction>(solution);
        }

        /// <summary>Plays pushes with shortest walks between them; the moves when it wins (or all of them with <paramref name="partial"/>), else null.</summary>
        public static List<PlayerAction> Play(GameState start, IReadOnlyList<PushStep> steps, bool partial = false)
        {
            var s = start.Clone();
            var moves = new List<PlayerAction>();
            foreach (var st in steps)
            {
                if (!st.Action.IsCar)
                {
                    var reach = Reach(s);
                    if (!reach.ContainsKey(st.Stand)) return null;
                    foreach (var w in WalkTo(reach, s.PlayerPos, st.Stand))
                    {
                        var rw = Rules.TryMove(s, w);
                        if (!rw.Success || rw.BoxMoves.Count > 0) return null;
                        moves.Add(w);
                        if (rw.IsWon) return moves;
                    }
                }
                var r = Rules.TryMove(s, st.Action);
                if (!r.Success) return null;
                moves.Add(st.Action);
                if (r.IsWon) return moves;
            }
            return partial ? moves : null;
        }
    }
}
