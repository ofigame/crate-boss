using System.Collections.Generic;

namespace CrateBoss.Meta
{
    /// <summary>
    /// The ranks (Ana Belge §7), read from the data file Resources/Data/Ranks.txt so more ranks can be added without code:
    /// Öğrenci, Stajyer, Çırak, Kalfa, Usta, then Şef, Mühendis, Kıdemli Mühendis. Every 25 levels a promotion.
    /// </summary>
    public sealed class RankCatalog
    {
        public sealed class Rank
        {
            public int Number;
            public string Key;
            /// <summary>Crate models of this rank: Resources/Models/&lt;Box…&gt;_&lt;suffix&gt; (missing: the rank-1 models).</summary>
            public string ModelSuffix;
            /// <summary>Land the promotion to this rank opens (Ana Belge §12.5), or "-".</summary>
            public string Land;
            /// <summary>String Table key of the rank name.</summary>
            public string NameKey => "league." + Number;
        }

        public IReadOnlyList<Rank> Ranks { get; }

        public RankCatalog(string text)
        {
            var list = new List<Rank>();
            foreach (var raw in (text ?? "").Split('\n'))
            {
                var line = raw.Trim();
                if (line.Length == 0 || line[0] == '#') continue;
                var c = line.Split('\t');
                if (c.Length < 4 || !int.TryParse(c[0], out int n)) continue;
                list.Add(new Rank { Number = n, Key = c[1], ModelSuffix = c[2], Land = c[3] });
            }
            if (list.Count == 0) list.Add(new Rank { Number = 1, Key = "ogrenci", ModelSuffix = "R1", Land = "start" });
            Ranks = list;
        }

        public Rank Get(int number)
        {
            foreach (var r in Ranks) if (r.Number == number) return r;
            return Ranks[Ranks.Count - 1];
        }

        /// <summary>
        /// The player's rank (Ana Belge §7): rank n+1 once the last level of rank n is cleared and rank n holds at least 60 %
        /// of its stars. <paramref name="lastCleared"/>(n) and <paramref name="starsIn"/>(n) / <paramref name="starsNeeded"/>(n+1)
        /// describe rank n of the level set; <paramref name="leagues"/> is how many ranks the levels have.
        /// </summary>
        public static int Current(int leagues, System.Func<int, bool> lastCleared, System.Func<int, int> starsIn, System.Func<int, int> starsNeeded)
        {
            int rank = 1;
            for (int n = 1; n < leagues; n++)
            {
                if (!lastCleared(n) || starsIn(n) < starsNeeded(n + 1)) break;
                rank = n + 1;
            }
            // Clearing the very last rank's exam completes it (shown as a final promotion, Ana Belge §7 "125").
            if (rank == leagues && lastCleared(leagues)) rank = leagues + 1;
            return rank;
        }
    }
}
