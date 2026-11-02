using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CrateBoss.Core;
using CrateBoss.Levels;

namespace CrateBoss.Puzzle
{
    /// <summary>
    /// Makes an easy level think harder (owner 2026-11-01 brief §5), by hill climbing: a small change to the layout (a crate,
    /// an inner fixed crate, the worker's or a delivery crate's start, a colour swap, a crate more or less) → solve it on the
    /// real rules and measure it → keep it when the thinking score rose, the labour stays under the ceiling and the level
    /// keeps everything it must keep (LevelInvariants); else undo. Stuck for a while, a climb starts again from a new random
    /// layout of the same yard and numbers. Several climbs give several candidates with different ideas.
    /// </summary>
    public static class LevelHardener
    {
        private const string Pushable = "BIHY123456";

        public static List<HardenCandidate> Harden(LevelData original, int number, HardenOptions o, Func<bool> cancelled = null, Action<string> progress = null)
        {
            var results = new List<HardenCandidate>();
            var startReport = Evaluate(original, number, SolutionText.Parse(original.solution), o.Quick);
            var baseline = startReport != null ? Fitness(startReport) : double.MinValue;
            double perClimb = o.Seconds / Math.Max(1, o.Climbs);
            for (int c = 0; c < o.Climbs; c++)
            {
                var rng = new Random(o.Seed * 7919 + number * 131 + c * 17);
                var best = Climb(original, number, o, rng, perClimb, cancelled, progress);
                if (best == null) continue;
                // The full measure for the result.
                var full = DifficultyMeter.Measure(best.Data, number, SolutionText.Parse(best.Data.solution), o.Full);
                best.Report = full;
                best.Fitness = Fitness(full);
                best.ReachedTarget = !full.Easy;
                best.Aha = AhaText.For(full);
                if (best.Fitness > baseline || results.Count == 0) results.Add(best);
                if (cancelled != null && cancelled()) break;
            }
            return results.OrderByDescending(r => r.ReachedTarget).ThenByDescending(r => r.Fitness).ToList();
        }

        /// <summary>Thinking score, less for every rule the level breaks.</summary>
        public static double Fitness(DifficultyReport r) =>
            !r.Solvable ? double.MinValue : r.LogicScore - 4 * r.Problems.Count - 0.15 * r.EffortScore;

        private static HardenCandidate Climb(LevelData original, int number, HardenOptions o, Random rng, double seconds, Func<bool> cancelled, Action<string> progress)
        {
            var clock = Stopwatch.StartNew();
            var current = Evaluate(original, number, SolutionText.Parse(original.solution), o.Quick) != null ? Make(original, number, SolutionText.Parse(original.solution), o) : null;
            var best = current;
            int stale = 0;
            while (clock.Elapsed.TotalSeconds < seconds && (cancelled == null || !cancelled()))
            {
                LevelData next;
                if (current == null || stale > o.Patience)
                {
                    next = Shuffle(original, rng);
                    stale = 0;
                    var fresh = next == null ? null : Solve(next, o);
                    if (fresh == null) continue;
                    current = fresh;
                    if (best == null || current.Fitness > best.Fitness) best = current;
                    continue;
                }
                next = Mutate(current.Data, rng, 1 + rng.Next(3));
                if (next == null || LevelInvariants.Check(original, next).Count > 0) { stale++; continue; }
                var cand = Solve(next, o);
                if (cand == null) { stale++; continue; }
                if (cand.Fitness > current.Fitness || cand.Fitness == current.Fitness && rng.NextDouble() < 0.3)
                {
                    stale = cand.Fitness > current.Fitness ? 0 : stale + 1;
                    current = cand;
                    if (best == null || current.Fitness > best.Fitness)
                    {
                        best = current;
                        progress?.Invoke($"{number}: {best.Fitness:0.0}");
                    }
                }
                else stale++;
            }
            return best;
        }

        private static HardenCandidate Make(LevelData data, int number, List<PlayerAction> solution, HardenOptions o)
        {
            var r = Evaluate(data, number, solution, o.Quick);
            if (r == null) return null;
            var copy = LevelCopy.Of(data);
            copy.solution = SolutionText.Write(solution);
            return new HardenCandidate { Data = copy, Report = r, Fitness = Fitness(r) };
        }

        /// <summary>Solves a layout (greedy push search, unneeded pushes dropped) and measures it; null when it cannot be solved.</summary>
        private static HardenCandidate Solve(LevelData data, HardenOptions o)
        {
            GameState start;
            try { start = new LevelParser().Parse(data); } catch (LevelFormatException) { return null; }
            if (start.IsSolved()) return null;
            var found = PushSearch.Greedy(start, o.SolveStates, o.SolveSeconds, 2);
            if (!found.Solved) return null;
            var moves = PushSearch.Prune(start, found.Moves);
            if (!DifficultyMeter.Wins(start, moves)) return null;
            return Make(data, NumberOf(data), moves, o);
        }

        private static int NumberOf(LevelData d) => d.id > 1000 ? d.id - 1000 : d.id;

        private static DifficultyReport Evaluate(LevelData data, int number, List<PlayerAction> solution, MeterOptions m)
        {
            if (solution == null) return null;
            try
            {
                var r = DifficultyMeter.Measure(data, number, solution, m);
                return r.Solvable ? r : null;
            }
            catch (LevelFormatException) { return null; }
        }

        // ---------- layout changes ----------

        private sealed class Grid
        {
            public char[][] G, O;
            public LevelData Data;
            public HashSet<(int, int)> Fixed = new HashSet<(int, int)>(); // cells changes never touch (machines, cars)
            public bool[] Outer;
            public int W, H;

            public bool Floor(int x, int y) => G[y][x] == '.' && O[y][x] == '.' && !Fixed.Contains((x, y));
            public bool Inner(int x, int y) => G[y][x] == '#' && !Outer[y * W + x] && !Fixed.Contains((x, y));
        }

        private static Grid Open(LevelData d)
        {
            var g = new Grid { Data = LevelCopy.Of(d) };
            g.G = g.Data.ground.Select(r => r.ToCharArray()).ToArray();
            g.O = g.Data.objects.Select(r => r.ToCharArray()).ToArray();
            g.H = g.G.Length; g.W = g.G[0].Length;
            var s = new LevelParser().Parse(d);
            g.Outer = BoundaryMap.OuterWalls(s);
            foreach (var c in s.Cranes) foreach (var drop in c.Drops) g.Fixed.Add((drop.Cell.X, drop.Cell.Y));
            if (s.Parking != null)
            {
                foreach (var car in s.Parking.Cars) { g.Fixed.Add((car.A.X, car.A.Y)); g.Fixed.Add((car.B.X, car.B.Y)); }
                foreach (var e in s.Parking.Exits) g.Fixed.Add((e.X, e.Y));
            }
            if (s.Truck != null)
            {
                foreach (var c in s.Truck.Cells) g.Fixed.Add((c.X, c.Y));
                foreach (var c in s.Truck.Route) g.Fixed.Add((c.X, c.Y));
                // The cars must keep a clear way out (owner: no crate in front of a car): the car park stays as it is.
                if (s.Parking != null)
                    foreach (var car in s.Parking.Cars)
                        for (int y = 0; y <= Math.Max(car.A.Y, car.B.Y) + 1; y++)
                            for (int x = 0; x < g.W; x++) if (y <= Math.Max(car.A.Y, car.B.Y)) g.Fixed.Add((x, y));
            }
            return g;
        }

        private static LevelData Close(Grid g)
        {
            g.Data.ground = g.G.Select(r => new string(r)).ToArray();
            g.Data.objects = g.O.Select(r => new string(r)).ToArray();
            g.Data.solution = null;
            return g.Data;
        }

        private static List<(int x, int y)> Cells(Grid g, Func<int, int, bool> where)
        {
            var list = new List<(int, int)>();
            for (int y = 0; y < g.H; y++) for (int x = 0; x < g.W; x++) if (where(x, y)) list.Add((x, y));
            return list;
        }

        private static T Pick<T>(List<T> list, Random rng) => list[rng.Next(list.Count)];

        /// <summary>One to three small changes; null when none could be made.</summary>
        private static LevelData Mutate(LevelData d, Random rng, int changes)
        {
            Grid g;
            try { g = Open(d); } catch (LevelFormatException) { return null; }
            int done = 0;
            for (int i = 0; i < changes * 4 && done < changes; i++)
            {
                int kind = rng.Next(7);
                var floor = Cells(g, g.Floor);
                if (floor.Count == 0) return null;
                switch (kind)
                {
                    case 0: case 1: // move a crate
                    {
                        var crates = Cells(g, (x, y) => Pushable.IndexOf(g.O[y][x]) >= 0 && !g.Fixed.Contains((x, y)));
                        if (crates.Count == 0) break;
                        var (cx, cy) = Pick(crates, rng);
                        var (fx, fy) = rng.NextDouble() < 0.7 ? Near(g, cx, cy, rng) ?? Pick(floor, rng) : Pick(floor, rng);
                        g.O[fy][fx] = g.O[cy][cx]; g.O[cy][cx] = '.';
                        done++;
                        break;
                    }
                    case 2: // move an inner fixed crate
                    {
                        var walls = Cells(g, g.Inner);
                        if (walls.Count == 0) break;
                        var (wx, wy) = Pick(walls, rng);
                        var (fx, fy) = Near(g, wx, wy, rng) ?? Pick(floor, rng);
                        g.G[fy][fx] = '#'; g.G[wy][wx] = '.';
                        done++;
                        break;
                    }
                    case 3: // the worker's start
                    {
                        var p = Cells(g, (x, y) => g.O[y][x] == 'P');
                        if (p.Count == 0) break;
                        var (fx, fy) = Pick(floor, rng);
                        g.O[p[0].y][p[0].x] = '.'; g.O[fy][fx] = 'P';
                        done++;
                        break;
                    }
                    case 4: // swap two colour crates (or a colour crate and a plain one)
                    {
                        var candy = Cells(g, (x, y) => g.O[y][x] >= '1' && g.O[y][x] <= '6');
                        var other = Cells(g, (x, y) => (g.O[y][x] == 'B' || g.O[y][x] >= '1' && g.O[y][x] <= '6') && !g.Fixed.Contains((x, y)));
                        if (candy.Count == 0 || other.Count < 2) break;
                        var a = Pick(candy, rng); var b = Pick(other, rng);
                        if (g.O[a.y][a.x] == g.O[b.y][b.x]) break;
                        (g.O[a.y][a.x], g.O[b.y][b.x]) = (g.O[b.y][b.x], g.O[a.y][a.x]);
                        done++;
                        break;
                    }
                    case 5: // a delivery crate's start (its start mark goes with it)
                    {
                        var ds = Cells(g, (x, y) => g.O[y][x] == 'D' && g.G[y][x] == 'A');
                        if (ds.Count == 0 || g.Data.deliveries == null) break;
                        var (dx, dy) = Pick(ds, rng);
                        var to = Near(g, dx, dy, rng);
                        if (to == null) break;
                        var (fx, fy) = to.Value;
                        g.O[dy][dx] = '.'; g.G[dy][dx] = '.';
                        g.O[fy][fx] = 'D'; g.G[fy][fx] = 'A';
                        foreach (var del in g.Data.deliveries) if (del.box != null && del.box[0] == dx && del.box[1] == dy) del.box = new[] { fx, fy };
                        done++;
                        break;
                    }
                    default: // a plain crate more or less
                    {
                        if (rng.Next(2) == 0) { var (fx, fy) = Pick(floor, rng); g.O[fy][fx] = 'B'; }
                        else
                        {
                            var plain = Cells(g, (x, y) => g.O[y][x] == 'B' && !g.Fixed.Contains((x, y)));
                            if (plain.Count == 0) break;
                            var (px, py) = Pick(plain, rng);
                            g.O[py][px] = '.';
                        }
                        done++;
                        break;
                    }
                }
            }
            return done == 0 ? null : Close(g);
        }

        /// <summary>A free floor cell within two steps (the idea stays local, no long walks).</summary>
        private static (int x, int y)? Near(Grid g, int x, int y, Random rng)
        {
            var list = new List<(int, int)>();
            for (int dy = -2; dy <= 2; dy++)
                for (int dx = -2; dx <= 2; dx++)
                {
                    int nx = x + dx, ny = y + dy;
                    if ((dx != 0 || dy != 0) && nx >= 0 && ny >= 0 && nx < g.W && ny < g.H && g.Floor(nx, ny)) list.Add((nx, ny));
                }
            return list.Count == 0 ? ((int, int)?)null : Pick(list, rng);
        }

        /// <summary>The same yard and numbers, every crate and inner fixed crate placed anew (brief §5 "sıfırdan yeni dizilim").</summary>
        private static LevelData Shuffle(LevelData d, Random rng)
        {
            Grid g;
            try { g = Open(d); } catch (LevelFormatException) { return null; }
            var crates = new List<char>();
            var walls = 0;
            for (int y = 0; y < g.H; y++)
                for (int x = 0; x < g.W; x++)
                {
                    if (g.Fixed.Contains((x, y))) continue;
                    if (Pushable.IndexOf(g.O[y][x]) >= 0) { crates.Add(g.O[y][x]); g.O[y][x] = '.'; }
                    if (g.Inner(x, y)) { walls++; g.G[y][x] = '.'; }
                }
            var floor = Cells(g, g.Floor).OrderBy(_ => rng.Next()).ToList();
            int k = 0;
            for (int i = 0; i < walls && k < floor.Count; i++, k++) { var (x, y) = floor[k]; g.G[y][x] = '#'; }
            // Crates gather round the middle of the remaining floor (no far corners); colour crates in clumps.
            var rest = floor.Skip(k).ToList();
            if (rest.Count < crates.Count) return null;
            double cx = rest.Average(c => c.Item1), cy = rest.Average(c => c.Item2);
            rest = rest.OrderBy(c => Math.Abs(c.Item1 - cx) + Math.Abs(c.Item2 - cy) + rng.NextDouble() * 4).ToList();
            for (int i = 0; i < crates.Count; i++) { var (x, y) = rest[i]; g.O[y][x] = crates[i]; }
            return Close(g);
        }
    }
}
