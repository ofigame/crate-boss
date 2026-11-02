using System;
using System.Collections.Generic;
using System.Linq;
using CrateBoss.Core;
using CrateBoss.Levels;

namespace CrateBoss.Puzzle
{
    /// <summary>
    /// Measures a level as the brief asks (owner 2026-11-01 §2): the thinking it needs (reverse pushes, preparations, traps,
    /// order, hasty player, decorative mechanics, gaps, triples, car chains) and the labour (walking, straight runs, pushes
    /// per crate, trips), the "door" rule and the colour-crate rules. Everything runs on the real rules (RulesEngine) from the
    /// level's best known solution; the trap checks are budgeted searches, so on big levels they find fewer traps than exist.
    /// </summary>
    public static class DifficultyMeter
    {
        private static readonly RulesEngine Rules = new RulesEngine();

        public static DifficultyReport Measure(LevelData data, int number, IReadOnlyList<PlayerAction> solution, MeterOptions options = null)
        {
            options ??= new MeterOptions();
            var start = new LevelParser().Parse(data);
            var r = new DifficultyReport { Number = number, Name = data.name, Width = start.Width, Height = start.Height };
            var steps = solution == null ? null : PushSearch.Steps(start, solution);
            r.Solvable = steps != null && Wins(start, solution);
            if (!r.Solvable) { DifficultyLimits.Judge(r); return r; }
            r.Moves = solution.Count;
            r.Pushes = steps.Count;

            MeasureEffort(start, solution, steps, r.Effort);
            MeasureOrderAndReverse(start, steps, r.Logic);
            if (!options.SolutionOnly)
            {
                MeasureTraps(start, steps, r.Logic, options);
                r.Logic.HastySolves = HastySolves(start, options);
                r.Logic.DecorativeMechanics = Decorative(data, start, solution, steps, options);
            }
            MeasureDoors(start, steps, r.Doors);
            r.Candy = start.HasSymbols ? MeasureCandy(start, steps, r.Logic) : null;
            MeasureCars(start, solution, r.Logic);
            r.LogicScore = DifficultyLimits.LogicScore(r.Logic, r.Doors);
            r.EffortScore = DifficultyLimits.EffortScore(r.Effort);
            DifficultyLimits.Judge(r);
            return r;
        }

        public static bool Wins(GameState start, IReadOnlyList<PlayerAction> solution)
        {
            var s = start.Clone();
            for (int i = 0; i < solution.Count; i++)
            {
                var res = Rules.TryMove(s, solution[i]);
                if (!res.Success) return false;
                if (res.IsWon) return i == solution.Count - 1;
            }
            return false;
        }

        // ---------- labour ----------

        private static void MeasureEffort(GameState start, IReadOnlyList<PlayerAction> solution, List<(GameState before, PushStep step, MoveResult result)> steps, EffortMeasures e)
        {
            e.Moves = solution.Count;
            e.Pushes = steps.Count;
            e.WalkRatio = solution.Count == 0 ? 0 : (solution.Count - steps.Count) / (double)solution.Count;
            int run = 0, best = 0, lastBox = -2;
            var lastDir = Direction.Up;
            var crates = new HashSet<int>();
            foreach (var (_, step, res) in steps)
            {
                if (step.Action.IsCar) { run = 0; lastBox = -2; continue; }
                int box = res.BoxMoves[res.BoxMoves.Count - 1].BoxId;
                foreach (var m in res.BoxMoves) crates.Add(m.BoxId);
                run = box == lastBox && step.Action.Direction == lastDir ? run + 1 : 1;
                best = Math.Max(best, run);
                lastBox = box;
                lastDir = step.Action.Direction;
            }
            e.StraightRun = best;
            e.CratesPushed = crates.Count;
            e.PushesPerCrate = crates.Count == 0 ? 0 : steps.Count(s => !s.step.Action.IsCar) / (double)crates.Count;
            // Trips: the worker goes from the top third to the bottom third or back.
            if (start.Height >= DifficultyLimits.TripsFromHeight)
            {
                var s = start.Clone();
                int third = start.Height / 3, side = 0;
                foreach (var a in solution)
                {
                    Rules.TryMove(s, a);
                    int now = s.PlayerPos.Y < third ? -1 : s.PlayerPos.Y >= start.Height - third ? 1 : 0;
                    if (now != 0 && side != 0 && now != side) e.EndToEndTrips++;
                    if (now != 0) side = now;
                }
            }
        }

        // ---------- reverse pushes and preparations ----------

        private static void MeasureOrderAndReverse(GameState start, List<(GameState before, PushStep step, MoveResult result)> steps, LogicMeasures l)
        {
            var visited = new Dictionary<int, HashSet<GridPos>>();
            foreach (var b in start.Boxes) visited[b.Id] = new HashSet<GridPos> { b.Position };
            var episodes = new Dictionary<int, int>();
            int lastBox = -1;
            int goalLast = -1;
            bool goalInterrupted = false;
            foreach (var (before, step, res) in steps)
            {
                if (step.Action.IsCar) continue;
                foreach (var m in res.BoxMoves)
                {
                    var box = before.GetBoxById(m.BoxId);
                    if (box == null) continue;
                    int d0 = PushSearch.GoalDistance(before, box);
                    if (d0 >= 0)
                    {
                        int d1 = Distance(before, box, m.To);
                        if (d1 > d0) l.ReversePushes++;
                    }
                    else if (visited.TryGetValue(m.BoxId, out var seen) && seen.Contains(m.To)) l.ReversePushes++;
                    if (visited.TryGetValue(m.BoxId, out var set)) set.Add(m.To);
                }
                int pushed = res.BoxMoves[res.BoxMoves.Count - 1].BoxId;
                var pushedBox = before.GetBoxById(pushed);
                bool goalCrate = pushedBox != null && PushSearch.GoalDistance(before, pushedBox) >= 0;
                if (pushed != lastBox)
                {
                    episodes[pushed] = episodes.TryGetValue(pushed, out int n) ? n + 1 : 1;
                    if (goalCrate && pushed == goalLast && goalInterrupted) l.Preparations++; // the goal crate waited for another
                    if (goalCrate) { goalLast = pushed; goalInterrupted = false; }
                    else if (goalLast >= 0) goalInterrupted = true;
                }
                lastBox = pushed;
            }
            // Crates in the way moved in two or more goes: set aside, then moved again.
            foreach (var kv in episodes)
            {
                var b = start.GetBoxById(kv.Key);
                if (b != null && PushSearch.GoalDistance(start, b) < 0 && kv.Value >= 2) l.Preparations += kv.Value - 1;
            }
        }

        private static int Distance(GameState s, Box b, GridPos at)
        {
            var keep = b.Position;
            b.Position = at;
            int d = PushSearch.GoalDistance(s, b);
            b.Position = keep;
            return d;
        }

        // ---------- traps, order, gaps, other solutions ----------

        private static void MeasureTraps(GameState start, List<(GameState before, PushStep step, MoveResult result)> steps, LogicMeasures l, MeterOptions o)
        {
            int checkedSteps = Math.Min(o.TrapSteps, steps.Count);
            l.CheckedSteps = checkedSteps;
            double delays = 0;
            int alternatives = 0;
            for (int k = 0; k < checkedSteps; k++)
            {
                var (before, step, res) = steps[k];
                int remaining = steps.Count - k;
                var later = new HashSet<int>();
                for (int j = k + 1; j < steps.Count; j++) foreach (var m in steps[j].result.BoxMoves) later.Add(m.BoxId);
                GridPos? solutionTo = res.BoxMoves.Count > 0 ? res.BoxMoves[res.BoxMoves.Count - 1].To : (GridPos?)null;
                int solutionBox = res.BoxMoves.Count > 0 ? res.BoxMoves[res.BoxMoves.Count - 1].BoxId : -1;
                foreach (var m in PushSearch.Expand(before))
                {
                    if (Same(m, step, solutionBox)) continue;
                    if (m.Won) { alternatives++; continue; }
                    bool dead = false;
                    int delay = 0;
                    if (PushSearch.Stuck(m.After)) dead = true;
                    else
                    {
                        var search = PushSearch.Bfs(m.After, o.TrapStates, o.TrapSeconds);
                        if (search.Exhausted) { dead = true; delay = search.Pushes; }
                        else if (search.Solved && search.Pushes + 1 <= remaining + 1) alternatives++;
                    }
                    if (!dead) continue;
                    l.TrapPushes++;
                    delays += delay;
                    if (m.BoxId >= 0 && later.Contains(m.BoxId)) l.OrderMatters = true;
                    if (m.BoxId >= 0 && solutionTo.HasValue && m.To == solutionTo.Value && m.BoxId != solutionBox) l.WrongGapChoices++;
                    var box = before.GetBoxById(m.BoxId);
                    if (box != null && box.Symbol > 0) l.TripleOrderMatters = true;
                }
            }
            l.TrapDelay = l.TrapPushes == 0 ? 0 : delays / l.TrapPushes;
            l.Solutions = Math.Min(99, 1 + alternatives);
        }

        private static bool Same(PushMove m, PushStep step, int box)
        {
            if (step.Action.IsCar) return m.BoxId < 0 && m.Step.Action.Equals(step.Action);
            return m.BoxId == box && m.Step.Action.Equals(step.Action) && m.Step.Stand == step.Stand;
        }

        // ---------- the hasty player ----------

        /// <summary>A player who never pushes a goal crate further from its goal (crates in the way he pushes anywhere).</summary>
        private static bool HastySolves(GameState start, MeterOptions o)
        {
            bool Allow(GameState before, PushMove m)
            {
                if (m.BoxId < 0) return true;
                var b = before.GetBoxById(m.BoxId);
                int d0 = b == null ? -1 : PushSearch.GoalDistance(before, b);
                return d0 < 0 || Distance(before, b, m.To) <= d0;
            }
            return PushSearch.Greedy(start, o.HastyStates, o.HastySeconds, 2, Allow).Solved;
        }

        // ---------- decorative mechanics ----------

        private static string Decorative(LevelData data, GameState start, IReadOnlyList<PlayerAction> solution, List<(GameState before, PushStep step, MoveResult result)> steps, MeterOptions o)
        {
            var list = new List<string>();
            // Cranes: needed when a drop lands where the solution has been before it, or a dropped crate is pushed.
            if (start.Cranes.Count > 0 && !CranesMatter(start, solution)) list.Add("vinç");
            // Colour crates: decoration when the level can be won with them as plain crates (nothing ever sinks).
            if (start.HasSymbols)
            {
                var plain = LevelCopy.Of(data);
                for (int y = 0; y < plain.objects.Length; y++)
                {
                    var row = plain.objects[y].ToCharArray();
                    for (int x = 0; x < row.Length; x++) if (row[x] >= '1' && row[x] <= '6') row[x] = 'B';
                    plain.objects[y] = new string(row);
                }
                GameState p = null;
                try { p = new LevelParser().Parse(plain); } catch (LevelFormatException) { }
                if (p != null && PushSearch.Greedy(p, o.MechanicStates, o.MechanicSeconds, 2).Solved) list.Add("patlatma");
            }
            // Holes, heavy and ice crates: used by the solution.
            bool hole = false, heavy = false, ice = false;
            foreach (var (before, _, res) in steps)
                foreach (var m in res.BoxMoves)
                {
                    if (m.FellInto == FallType.Hole) hole = true;
                    if (m.BoxType == BoxType.Heavy) heavy = true;
                    if (m.BoxType == BoxType.Ice) ice = true;
                }
            if (HasTile(start, TileType.Hole) && !hole) list.Add("çukur");
            if (start.Boxes.Any(b => b.Type == BoxType.Heavy) && !heavy) list.Add("ağır kasa");
            if (start.Boxes.Any(b => b.Type == BoxType.Ice) && !ice) list.Add("buz");
            return string.Join(",", list);
        }

        private static bool HasTile(GameState s, TileType t)
        {
            for (int y = 0; y < s.Height; y++) for (int x = 0; x < s.Width; x++) if (s.GetTile(new GridPos(x, y)) == t) return true;
            return false;
        }

        private static bool CranesMatter(GameState start, IReadOnlyList<PlayerAction> solution)
        {
            var s = start.Clone();
            var used = new Dictionary<GridPos, int>(); // cell -> last move it was used on
            var dropped = new HashSet<int>();
            for (int i = 0; i < solution.Count; i++)
            {
                var res = Rules.TryMove(s, solution[i]);
                used[s.PlayerPos] = i;
                foreach (var m in res.BoxMoves) { used[m.To] = i; if (dropped.Contains(m.BoxId)) return true; }
                foreach (var d in res.CraneDrops)
                {
                    if (used.ContainsKey(d.Cell)) return true; // the drop shuts a cell the way needed
                    if (d.BoxId >= 0) dropped.Add(d.BoxId);
                }
            }
            return false;
        }

        // ---------- doors ----------

        private static void MeasureDoors(GameState start, List<(GameState before, PushStep step, MoveResult result)> steps, DoorMeasures d)
        {
            var startArea = PushSearch.Reach(start).Keys.ToHashSet();
            var prevDoor = start;
            int maxArea = startArea.Count, afterThree = startArea.Count;
            for (int k = 0; k < steps.Count; k++)
            {
                var before = steps[k].before;
                var after = k + 1 < steps.Count ? steps[k + 1].before : null;
                if (after == null) break;
                var a0 = PushSearch.Reach(before).Keys.ToHashSet();
                var a1 = PushSearch.Reach(after).Keys.ToHashSet();
                maxArea = Math.Max(maxArea, a1.Count);
                if (k == 2) afterThree = a1.Count;
                if (a1.Count - a0.Count < DifficultyLimits.DoorGrowth) continue;
                d.Doors++;
                var opened = new HashSet<GridPos>(a1);
                opened.ExceptWith(a0);
                // Easy: one push of one crate from the position after the door before opens it too.
                foreach (var m in PushSearch.Expand(prevDoor))
                {
                    if (m.BoxId < 0) continue;
                    var reach = PushSearch.Reach(m.After);
                    if (opened.All(reach.ContainsKey)) { d.EasyDoors++; break; }
                }
                prevDoor = after;
            }
            int total = maxArea - startArea.Count;
            d.OpenedByFirstThree = total <= 0 ? 0 : Math.Max(0, afterThree - startArea.Count) / (double)total;
        }

        // ---------- colour crates ----------

        private static CandyMeasures MeasureCandy(GameState start, List<(GameState before, PushStep step, MoveResult result)> steps, LogicMeasures l)
        {
            var c = new CandyMeasures();
            var candy = start.Boxes.Where(b => b.Symbol > 0).ToList();
            c.Candy = candy.Count;
            c.Pushable = start.Boxes.Count(b => b.Type != BoxType.Container);
            c.Share = c.Pushable == 0 ? 0 : c.Candy / (double)c.Pushable;
            foreach (var b in candy)
                if (!candy.Any(o => o != b && Math.Max(Math.Abs(o.Position.X - b.Position.X), Math.Abs(o.Position.Y - b.Position.Y)) <= 2)) c.Lonely++;
            // Touching groups.
            var left = new HashSet<Box>(candy);
            while (left.Count > 0)
            {
                var first = left.First();
                var group = new List<Box> { first };
                left.Remove(first);
                for (int i = 0; i < group.Count; i++)
                    foreach (var o in left.ToList())
                        if (o.Position.ManhattanTo(group[i].Position) == 1) { group.Add(o); left.Remove(o); }
                if (group.Count >= DifficultyLimits.MinClusterSize)
                {
                    c.Clusters++;
                    if (group.Select(b => b.Symbol).Distinct().Count() >= 3) c.MixedClusters++;
                }
            }
            c.ThinColours = candy.GroupBy(b => b.Symbol).Count(g => g.Count() < 3);
            var probe = start.Clone();
            c.ReadyTriple = MatchSystem.Step(probe).Count > 0;
            // Triples: the crate whose push sank them, and how it got there.
            var pushesOf = new Dictionary<int, int>();
            foreach (var (_, step, res) in steps)
            {
                if (step.Action.IsCar || res.BoxMoves.Count == 0) continue;
                int id = res.BoxMoves[res.BoxMoves.Count - 1].BoxId;
                pushesOf[id] = pushesOf.TryGetValue(id, out int n) ? n + 1 : 1;
                if (res.Sunk.Count == 0) continue;
                if (pushesOf[id] <= 1) c.NextToPlace++;
                else l.IndirectTriples++;
            }
            return c;
        }

        // ---------- cars ----------

        private static void MeasureCars(GameState start, IReadOnlyList<PlayerAction> solution, LogicMeasures l)
        {
            if (start.Parking == null) return;
            var memo = new Dictionary<int, int>();
            int Depth(int car, HashSet<int> path)
            {
                if (memo.TryGetValue(car, out int known)) return known;
                if (!path.Add(car)) return 0;
                var pc = start.Parking.Cars[car];
                int best = int.MaxValue;
                foreach (var dir in DirectionExtensions.All)
                {
                    if (!pc.AlongAxis(dir)) continue;
                    var cell = pc.Front(dir);
                    var blockers = new List<int>();
                    bool wall = false;
                    while (!start.Parking.IsExit(cell))
                    {
                        cell = cell + dir.ToOffset();
                        if (!start.InBounds(cell) || start.GetTile(cell) == TileType.Wall) { wall = true; break; }
                        int other = start.CarAt(cell);
                        if (other >= 0 && other != car && !blockers.Contains(other)) blockers.Add(other);
                        if (start.GetBoxAt(cell) != null) l.CarCrateLocks++;
                    }
                    if (wall) continue;
                    int d = 1 + (blockers.Count == 0 ? 0 : blockers.Max(b => Depth(b, path)));
                    best = Math.Min(best, d);
                }
                path.Remove(car);
                return memo[car] = best == int.MaxValue ? 1 : best;
            }
            foreach (var a in solution)
                if (a.IsCar) l.CarChain = Math.Max(l.CarChain, Depth(a.Car, new HashSet<int>()));
        }
    }
}
