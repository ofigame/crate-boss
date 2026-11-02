using System.Collections.Generic;

namespace CrateBoss.Core
{
    /// <summary>
    /// Match rule (Ana Belge §5.4): after a move, every horizontal or vertical run of at least three
    /// crates with the same colour sinks, all of it; the cells become floor and nothing slides into the gaps.
    /// </summary>
    public static class MatchSystem
    {
        private const int MinRun = 3;
        private static readonly List<SunkCrate> None = new List<SunkCrate>();

        /// <summary>Sinks every run of three or more; returns the sunk crates in reading order (an empty list when none).</summary>
        public static IReadOnlyList<SunkCrate> Step(GameState s) => Step(s, out _);

        /// <param name="bonus">CB Coin for long runs (Ana Belge §5.4): +5 for a run of 4, +10 for 5 or more.</param>
        public static IReadOnlyList<SunkCrate> Step(GameState s, out int bonus)
        {
            bonus = 0;
            int coins = 0;
            if (!s.HasSymbols) return None;
            int w = s.Width, h = s.Height;
            var symbol = new int[w * h];
            foreach (var b in s.Boxes)
                if (b.Symbol > 0) symbol[s.Index(b.Position)] = b.Symbol;

            bool[] sink = null;
            void MarkRuns(int count, int length, System.Func<int, int, int> cell)
            {
                for (int line = 0; line < count; line++)
                {
                    int start = 0;
                    for (int i = 1; i <= length; i++)
                    {
                        int prev = symbol[cell(line, i - 1)];
                        if (i < length && symbol[cell(line, i)] == prev) continue;
                        if (prev > 0 && i - start >= MinRun)
                        {
                            coins += i - start >= 5 ? 10 : i - start == 4 ? 5 : 0; // EconomyConfig.match4 / match5
                            sink ??= new bool[w * h];
                            for (int k = start; k < i; k++) sink[cell(line, k)] = true;
                        }
                        start = i;
                    }
                }
            }
            MarkRuns(h, w, (y, x) => y * w + x);
            MarkRuns(w, h, (x, y) => y * w + x);
            if (sink == null) return None;
            bonus = coins;

            var sunk = new List<SunkCrate>();
            for (int i = 0; i < sink.Length; i++)
            {
                if (!sink[i]) continue;
                var cell = s.CellAt(i);
                var box = s.GetBoxAt(cell);
                sunk.Add(new SunkCrate(box.Id, cell, box.Symbol));
                s.RemoveBox(box);
            }
            return sunk;
        }
    }
}
