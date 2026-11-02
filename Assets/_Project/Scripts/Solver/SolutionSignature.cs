using System;
using System.Collections.Generic;
using System.Text;
using CrateBoss.Core;

namespace CrateBoss.Solver
{
    /// <summary>
    /// A solution's "fingerprint" for the variety filter (Ana Belge §10.5): the order of pushes, one letter per push —
    /// which crate (by the order crates are first touched) and which way. Two levels that are solved by the same story
    /// (same crates in the same order, the same turns) get similar strings even on different rooms.
    /// </summary>
    public static class SolutionSignature
    {
        /// <summary>Above this, a candidate counts as a copy of an earlier level.</summary>
        public const double TooSimilar = 0.8;

        public static string Of(GameState start, IReadOnlyList<PlayerAction> moves)
        {
            var rules = new RulesEngine();
            var s = start.Clone();
            var order = new Dictionary<int, int>();
            var sb = new StringBuilder();
            foreach (var dir in moves)
            {
                var box = dir.IsCar ? null : s.GetBoxAt(s.PlayerPos + dir.Direction.ToOffset());
                var r = rules.TryMove(s, dir);
                if (!r.Success || box == null) continue;
                if (!order.TryGetValue(box.Id, out int n)) order[box.Id] = n = order.Count;
                sb.Append((char)('a' + Math.Min(n, 9))).Append(Letter(dir.Direction));
            }
            return sb.ToString();
        }

        /// <summary>1 = the same push story, 0 = nothing alike (normalised edit distance over push tokens).</summary>
        public static double Similarity(string a, string b)
        {
            if (string.IsNullOrEmpty(a) && string.IsNullOrEmpty(b)) return 1;
            int n = a.Length / 2, m = b.Length / 2;
            if (n == 0 || m == 0) return 0;
            var prev = new int[m + 1];
            var cur = new int[m + 1];
            for (int j = 0; j <= m; j++) prev[j] = j;
            for (int i = 1; i <= n; i++)
            {
                cur[0] = i;
                for (int j = 1; j <= m; j++)
                {
                    bool same = a[2 * i - 2] == b[2 * j - 2] && a[2 * i - 1] == b[2 * j - 1];
                    cur[j] = Math.Min(Math.Min(prev[j] + 1, cur[j - 1] + 1), prev[j - 1] + (same ? 0 : 1));
                }
                (prev, cur) = (cur, prev);
            }
            return 1.0 - (double)prev[m] / Math.Max(n, m);
        }

        private static char Letter(Direction d) => d == Direction.Up ? 'U' : d == Direction.Down ? 'D' : d == Direction.Left ? 'L' : 'R';
    }
}
