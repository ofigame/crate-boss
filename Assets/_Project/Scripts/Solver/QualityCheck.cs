using System;
using System.Collections.Generic;
using CrateBoss.Core;

namespace CrateBoss.Solver
{
    /// <summary>
    /// The automatic quality filters (Ana Belge §10.5). Every level except the tutorials must pass them: the monotone
    /// ("hasty player") test, "do the machines matter?", a written idea and the size limits (§8). Thread-safe: one call uses its own solvers.
    /// </summary>
    public static class QualityCheck
    {
        /// <summary>Score term that makes a tight machine clock count as difficulty.</summary>
        public static double MachineTerm(int tolerance) => tolerance < 0 ? 0 : 4 * Math.Max(0, 5 - tolerance);

        /// <summary>Widest and longest playable yard (Ana Belge §8).</summary>
        public const int MaxWidth = 9, MaxHeight = 30;

        /// <param name="maxWaste">How far the tolerance is measured (20 = full).</param>
        public static QualityResult Evaluate(GameState start, string idea, bool tutorial = false, bool measureTraps = false, int maxStates = LevelSolver.DefaultMaxStates, int maxWaste = 20)
        {
            var q = new QualityResult();
            var solver = new LevelSolver(maxStates);
            var analyzer = new LevelAnalyzer(maxStates);
            q.Metrics = analyzer.Analyze(start, measureTraps);
            YardSize(start, out q.YardWidth, out q.YardHeight);
            q.HasMachines = start.Cranes.Count > 0;

            if (string.IsNullOrWhiteSpace(idea)) q.Failures.Add("fikir cümlesi (design.idea) yok");
            if (q.YardWidth > MaxWidth || q.YardHeight > MaxHeight)
                q.Failures.Add($"alan {q.YardWidth}×{q.YardHeight}, en fazla {MaxWidth}×{MaxHeight}");
            if (!q.Metrics.IsSolved)
            {
                q.Failures.Add(q.Metrics.Solution.HitLimit ? "çözücü limiti aşıldı" : "çözülemiyor");
                return q;
            }

            if (q.HasMachines) q.Tolerance = CraneTolerance.Measure(start, solver, maxWaste);
            q.Score = q.Metrics.ScoreWithTraps + MachineTerm(q.Tolerance);
            Replay(start, q.Metrics.Solution.Moves, q);

            q.MonotoneSolvable = MonotonePushes.Solvable(start, maxStates);
            if (q.HasMachines)
            {
                var plain = start.Clone();
                plain.SetCranes(null);
                var without = solver.Solve(plain);
                q.MovesWithoutMachines = without.IsSolved ? without.MoveCount : -1;
                bool same = without.IsSolved && SameMoves(without.Moves, q.Metrics.Solution.Moves);
                q.MachinesMatter = !(same && q.Tolerance >= 8);
            }

            if (tutorial) return q;
            if (q.MonotoneSolvable) q.Failures.Add("monoton test: kutuları hiç geri itmeden çözülüyor");
            if (!q.MachinesMatter) q.Failures.Add("makineler önemsiz: onlarsız aynı çözüm, tolerans ≥ 8");
            return q;
        }

        /// <summary>Plays the solution and notes the machine features it uses (holding a machine up, a load filling a hole).</summary>
        private static void Replay(GameState start, IReadOnlyList<PlayerAction> moves, QualityResult q)
        {
            var rules = new RulesEngine();
            var s = start.Clone();
            foreach (var dir in moves)
            {
                var r = rules.TryMove(s, dir);
                foreach (var d in r.CraneDrops) q.MachineFillsHole |= d.FellInto == FallType.Hole;
                for (int i = 0; i < s.Cranes.Count; i++)
                {
                    if (!CraneSystem.IsWaiting(s, i)) continue;
                    var cell = s.NextDropOf(i).Cell;
                    q.UsesLock |= s.GetBoxAt(cell) != null;
                }
            }
        }

        private static bool SameMoves(IReadOnlyList<PlayerAction> a, IReadOnlyList<PlayerAction> b)
        {
            if (a.Count != b.Count) return false;
            for (int i = 0; i < a.Count; i++) if (a[i] != b[i]) return false;
            return true;
        }

        /// <summary>Bounding box of the cells a player or box can use.</summary>
        public static void YardSize(GameState s, out int width, out int height)
        {
            int minX = int.MaxValue, maxX = -1, minY = int.MaxValue, maxY = -1;
            for (int y = 0; y < s.Height; y++)
            for (int x = 0; x < s.Width; x++)
            {
                if (!BoundaryMap.IsYard(s.GetTile(new GridPos(x, y)))) continue;
                minX = Math.Min(minX, x); maxX = Math.Max(maxX, x);
                minY = Math.Min(minY, y); maxY = Math.Max(maxY, y);
            }
            width = maxX < 0 ? 0 : maxX - minX + 1;
            height = maxY < 0 ? 0 : maxY - minY + 1;
        }
    }
}
