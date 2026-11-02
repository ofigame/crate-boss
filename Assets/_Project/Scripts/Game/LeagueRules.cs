using System;
using System.Collections.Generic;
using CrateBoss.Levels;

namespace CrateBoss.Game
{
    /// <summary>
    /// Leagues (owner 2026-10-31): 25 levels each, every level of a league harder than every level of the one before; the
    /// next league opens when the player holds 60% of the stars of the current one (45 of 75). Names are localized
    /// ("league.1" = Öğrenci, then Stajyer, Çırak, Kalfa, Usta; more can follow: Şef, Mühendis, Kıdemli Mühendis).
    /// </summary>
    public static class LeagueRules
    {
        public const int PerLeague = 25;
        public const float StarShare = 0.6f;

        /// <summary>True when the level list is a league set (its levels carry a league number).</summary>
        public static bool IsLeagueSet(IReadOnlyList<LevelData> levels) => levels.Count > 0 && levels[0].league > 0;

        /// <summary>League of a level (1-based): its own number, else by position.</summary>
        public static int LeagueOf(IReadOnlyList<LevelData> levels, int index) =>
            levels[index].league > 0 ? levels[index].league : index / PerLeague + 1;

        /// <summary>Stars needed from the league before <paramref name="league"/> to open it (0 for the first).</summary>
        public static int StarsNeeded(IReadOnlyList<LevelData> levels, int league)
        {
            if (league <= 1) return 0;
            int count = 0;
            for (int i = 0; i < levels.Count; i++) if (LeagueOf(levels, i) == league - 1) count++;
            return (int)Math.Ceiling(count * 3 * StarShare);
        }

        public static int StarsIn(IReadOnlyList<LevelData> levels, int league, Func<LevelData, int> starsOf)
        {
            int sum = 0;
            for (int i = 0; i < levels.Count; i++) if (LeagueOf(levels, i) == league) sum += starsOf(levels[i]);
            return sum;
        }

        public static int LevelsIn(IReadOnlyList<LevelData> levels, int league)
        {
            int n = 0;
            for (int i = 0; i < levels.Count; i++) if (LeagueOf(levels, i) == league) n++;
            return n;
        }
    }
}
