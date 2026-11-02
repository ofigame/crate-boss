using System.Collections.Generic;

namespace CrateBoss.Core
{
    /// <summary>
    /// The cargo belts (owner 2026-11-02, later candy levels). A level may lay one fixed belt ("conveyor"), or (owner
    /// 2026-11-02 "sabit bir yerde olmayacak") let them appear by themselves ("belts"): whenever a way of free floor runs from
    /// the row under the candy up to the truck's loading cell, a belt lies along it, as many as there are ways that share no
    /// cell (only the loading cell itself). A cargo crate on a belt rides it, cell by cell, as far as the cells ahead are free;
    /// the one at the end stands in the loading area. Runs after every move, before the truck step.
    /// </summary>
    public static class ConveyorSystem
    {
        private static readonly List<BoxMovement> None = new List<BoxMovement>();
        private static readonly List<List<GridPos>> NoBelts = new List<List<GridPos>>();
        private const int MaxBelts = 4;

        /// <summary>Every one-cell ride this move, in the order they happen (the front crate of each belt first).</summary>
        public static List<BoxMovement> Step(GameState s)
        {
            var belts = Belts(s);
            if (belts.Count == 0) return None;
            List<BoxMovement> moves = null;
            bool moved = true;
            while (moved)
            {
                moved = false;
                foreach (var belt in belts)
                    for (int i = belt.Count - 2; i >= 0; i--)
                    {
                        var box = s.GetBoxAt(belt[i]);
                        if (box == null || !box.IsCargo || !Free(s, belt[i + 1])) continue;
                        // Into the loading cell only while a truck stands there (else the next one could never back in).
                        int bay = s.BayAt(belt[i + 1]);
                        if (bay >= 0 && !s.ParkedAt(bay)) continue;
                        box.Position = belt[i + 1];
                        (moves ??= new List<BoxMovement>()).Add(new BoxMovement(box.Id, box.Type, belt[i], belt[i + 1], FallType.None));
                        moved = true;
                    }
            }
            return moves ?? None;
        }

        /// <summary>The belts lying now, each from its start to a truck's loading cell.</summary>
        public static List<List<GridPos>> Belts(GameState s)
        {
            if (s.Conveyor.Count >= 2) return new List<List<GridPos>> { new List<GridPos>(s.Conveyor) };
            if (!s.AutoBelts || s.Bays.Length == 0 || s.CandyArea == null) return NoBelts;
            // Starts: the free floor along the inner edge of every candy band (the row under a top block; inside a frame, the
            // line in front of each side), as far as that band reaches.
            var starts = new HashSet<GridPos>();
            var fall = CandySystem.Fall(s);
            for (int d = 0; d < 4; d++)
            {
                int edge = int.MinValue, lo = int.MaxValue, hi = int.MinValue;
                bool vertical = d <= 1; // up / down: the edge is a row
                for (int i = 0; i < fall.Length; i++)
                {
                    if (fall[i] != d) continue;
                    var c = s.CellAt(i);
                    int along = vertical ? c.Y : c.X, across = vertical ? c.X : c.Y;
                    int deep = d == 0 || d == 2 ? -along : along; // the farthest the band reaches toward the worker
                    if (deep > edge) edge = deep;
                    lo = System.Math.Min(lo, across); hi = System.Math.Max(hi, across);
                }
                if (edge == int.MinValue) continue;
                int line = (d == 0 || d == 2 ? -edge : edge) + (d == 0 || d == 2 ? -1 : 1);
                for (int t = lo; t <= hi; t++)
                {
                    var p = vertical ? new GridPos(t, line) : new GridPos(line, t);
                    if (s.InBounds(p) && !s.CandyArea[s.Index(p)] && Open(s, p)) starts.Add(p);
                }
            }
            if (starts.Count == 0) return NoBelts;
            var used = new HashSet<GridPos>();
            var belts = new List<List<GridPos>>();
            // The loading areas take turns, so each truck gets a belt before any gets a second.
            for (bool any = true; any && belts.Count < MaxBelts;)
            {
                any = false;
                foreach (var b in s.Bays)
                {
                    if (belts.Count >= MaxBelts || b.Cells.Count == 0) break;
                    var bay = b.Cells[0];
                    var path = Way(s, bay, starts, used);
                    if (path == null) continue;
                    belts.Add(path);
                    foreach (var c in path) if (c != bay) used.Add(c);
                    any = true;
                }
            }
            return belts;
        }

        // The shortest way of open floor from the bay back to a start cell, avoiding cells other belts use; it is returned
        // start first.
        private static List<GridPos> Way(GameState s, GridPos bay, HashSet<GridPos> starts, HashSet<GridPos> used)
        {
            var from = new Dictionary<GridPos, GridPos> { [bay] = bay };
            var queue = new Queue<GridPos>();
            queue.Enqueue(bay);
            while (queue.Count > 0)
            {
                var c = queue.Dequeue();
                if (starts.Contains(c) && c != bay)
                {
                    var path = new List<GridPos>();
                    for (var p = c; p != bay; p = from[p]) path.Add(p);
                    path.Add(bay);
                    return path;
                }
                foreach (var d in Steps)
                {
                    var next = c + d;
                    if (from.ContainsKey(next) || used.Contains(next) || !Open(s, next)) continue;
                    from[next] = c;
                    queue.Enqueue(next);
                }
            }
            return null;
        }

        private static readonly GridPos[] Steps = { new GridPos(0, 1), new GridPos(-1, 0), new GridPos(1, 0), new GridPos(0, -1) };

        // Floor a belt can lie on: no crate but cargo, no wall, hole or car, not the truck's way in.
        private static bool Open(GameState s, GridPos p)
        {
            if (!s.InBounds(p) || !GoalRules.IsWalkableTile(s.GetTile(p)) || GoalRules.ClosedGate(s, p)) return false;
            if (s.OnTruckRoute(p)) return false;
            var box = s.GetBoxAt(p);
            return (box == null || box.IsCargo) && s.CarAt(p) < 0;
        }

        private static bool Free(GameState s, GridPos p) => s.GetBoxAt(p) == null && s.PlayerPos != p && s.CarAt(p) < 0 && !s.IsTruckWall(p);
    }
}
