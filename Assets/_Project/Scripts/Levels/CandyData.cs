using System.Collections.Generic;

namespace CrateBoss.Levels
{
    /// <summary>
    /// The "candy" block of a level file (owner 2026-11-01): how many colour crates the four cranes drop in all, which colours,
    /// the random seed, and the hammer hits each breakable crate ('F' in objects) needs: [[x, y, hits], ...] (missing = 1).
    /// The candy area is where the colour crates stand at the start.
    /// </summary>
    public class CandyData
    {
        public int drops;
        public int[] colors;
        public int seed;
        /// <summary>Most hits a left-over colour crate needs (0 = one per move it stood, no limit).</summary>
        public int hitCap;
        /// <summary>Waves in a row that sink by themselves before the next pay advanced hammers (owner 2026-11-02: 3, 4, 5, 6 by league).</summary>
        public int superAfter = 3;
        /// <summary>The level hands out paint guns (owner 2026-11-02: the easy candy levels).</summary>
        public bool paint;
        /// <summary>The classic puzzle in the middle of the candy yard (owner 2026-11-02): its fewest pushes and the hammers it pays; 0 = none.</summary>
        public int puzzlePar, puzzleReward;
        public int[][] hits;

        public static CandyData FromJson(object node)
        {
            if (!(node is Dictionary<string, object> obj)) throw new LevelFormatException("'candy' must be an object.");
            var c = new CandyData();
            if (obj.TryGetValue("drops", out var d) && d is double dn) c.drops = (int)dn;
            if (obj.TryGetValue("seed", out var s) && s is double sn) c.seed = (int)sn;
            if (obj.TryGetValue("hitCap", out var hc) && hc is double hcn) c.hitCap = (int)hcn;
            if (obj.TryGetValue("superAfter", out var sa) && sa is double san) c.superAfter = (int)san;
            if (obj.TryGetValue("paint", out var pt) && pt is bool pb) c.paint = pb;
            if (obj.TryGetValue("puzzle", out var pz) && pz is Dictionary<string, object> puzzle)
            {
                if (puzzle.TryGetValue("par", out var pp) && pp is double ppn) c.puzzlePar = (int)ppn;
                if (puzzle.TryGetValue("reward", out var pr) && pr is double prn) c.puzzleReward = (int)prn;
            }
            if (obj.TryGetValue("colors", out var cs) && cs is List<object> list)
            {
                c.colors = new int[list.Count];
                for (int i = 0; i < list.Count; i++) c.colors[i] = list[i] is double v ? (int)v : 1;
            }
            if (obj.TryGetValue("hits", out var h) && h is List<object> rows)
            {
                c.hits = new int[rows.Count][];
                for (int i = 0; i < rows.Count; i++)
                {
                    if (!(rows[i] is List<object> r) || r.Count != 3) throw new LevelFormatException($"candy.hits {i} must be [x, y, hits].");
                    c.hits[i] = new[] { (int)(double)r[0], (int)(double)r[1], (int)(double)r[2] };
                }
            }
            return c;
        }

        /// <summary>Hits the breakable crate at (x, y) needs.</summary>
        public int HitsAt(int x, int y)
        {
            if (hits != null)
                foreach (var h in hits)
                    if (h[0] == x && h[1] == y) return System.Math.Max(1, h[2]);
            return 1;
        }
    }
}
