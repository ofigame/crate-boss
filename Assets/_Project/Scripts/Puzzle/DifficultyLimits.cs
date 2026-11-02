using System;
using System.Collections.Generic;

namespace CrateBoss.Puzzle
{
    /// <summary>
    /// The adjustable limits of the brief (owner 2026-11-01): the least thinking per range (§3), the labour ceiling (§2), the
    /// colour-crate rules (§4) and the two scores.
    /// LOGIC = 3·reverse + 3·preparation + 2·traps + min(trap delay, 5) + 4·[order matters] + 5·[hasty player fails]
    ///         + 2·wrong gap choices + 2·indirect triples + 2·[triple order matters] + 2·car chain + car/crate locks
    ///         + 8 / solutions − 4·decorative mechanics − 5·easy doors.
    /// EFFORT = 30·walk ratio + 2·longest straight run + 1.5·pushes per crate + 3·end-to-end trips.
    /// </summary>
    public static class DifficultyLimits
    {
        public static readonly List<RangeLimit> Ranges = new List<RangeLimit>
        {
            new RangeLimit(6, 15, 1, 2, 6),
            new RangeLimit(16, 25, 2, 3, 4),
            new RangeLimit(26, 50, 3, 4, 4),
            new RangeLimit(51, 75, 4, 5, 3),
            new RangeLimit(76, 100, 5, 6, 3),
            new RangeLimit(101, 125, 6, 7, 2),
        };

        public const int Tutorials = 5;

        // Labour ceiling (§2).
        public static double MaxWalkRatio = 0.55;
        public static int MaxStraightRun = 4;
        public static double MaxPushesPerCrate = 8;
        public static int MaxTrips = 3, TripsFromHeight = 18;

        // Colour crates (§4).
        public static double MinCandyShare = 0.40;
        public static int MinClusters = 2, MinClusterSize = 4;

        // Doors (§4).
        public static int DoorGrowth = 4;
        public static double MaxOpenedByFirstThree = 0.5;

        /// <summary>The row for a level; the last level of a range of 25 must meet the next row (§3).</summary>
        public static RangeLimit For(int number)
        {
            int n = number % 25 == 0 && number < 125 ? number + 1 : number;
            foreach (var r in Ranges) if (n >= r.First && n <= r.Last) return r;
            return number == 125 ? Ranges[Ranges.Count - 1] : null;
        }

        public static double LogicScore(LogicMeasures l, DoorMeasures d) =>
            3 * l.ReversePushes + 3 * l.Preparations + 2 * l.TrapPushes + Math.Min(l.TrapDelay, 5) + (l.OrderMatters ? 4 : 0)
            + (l.HastySolves ? 0 : 5) + 2 * l.WrongGapChoices + 2 * l.IndirectTriples + (l.TripleOrderMatters ? 2 : 0) + 2 * l.CarChain
            + l.CarCrateLocks + 8.0 / Math.Max(1, l.Solutions) - 4 * Count(l.DecorativeMechanics) - 5 * d.EasyDoors;

        public static double EffortScore(EffortMeasures e) => 30 * e.WalkRatio + 2 * e.StraightRun + 1.5 * e.PushesPerCrate + 3 * e.EndToEndTrips;

        private static int Count(string list) => string.IsNullOrEmpty(list) ? 0 : list.Split(',').Length;

        /// <summary>Fills <see cref="DifficultyReport.Problems"/>: every rule of §3 and §4 the level breaks.</summary>
        public static void Judge(DifficultyReport r)
        {
            r.Problems.Clear();
            if (!r.Solvable) { r.Problems.Add("çözülemedi"); return; }
            if (r.Number <= Tutorials) return;
            var limit = For(r.Number);
            var l = r.Logic;
            if (limit != null)
            {
                if (limit.HastyMustFail && l.HastySolves) r.Problems.Add("aceleci oyuncu çözüyor");
                if (l.ReversePushes + l.Preparations < limit.ReversePlusPrep) r.Problems.Add($"ters itme + hazırlık {l.ReversePushes + l.Preparations} < {limit.ReversePlusPrep}");
                if (l.TrapPushes < limit.TrapPushes) r.Problems.Add($"tuzak itme {l.TrapPushes} < {limit.TrapPushes}");
                if (limit.OrderMustMatter && !l.OrderMatters) r.Problems.Add("sıra önemsiz");
                if (l.Solutions > limit.MaxSolutions) r.Problems.Add($"farklı çözüm {l.Solutions} > {limit.MaxSolutions}");
            }
            if (l.DecorativeMechanics.Length > 0) r.Problems.Add("süs mekanik: " + l.DecorativeMechanics);
            if (r.Doors.EasyDoors > 0) r.Problems.Add($"kolay kapı {r.Doors.EasyDoors}");
            if (r.Doors.OpenedByFirstThree > MaxOpenedByFirstThree) r.Problems.Add($"ilk 3 itme alanın %{r.Doors.OpenedByFirstThree * 100:0}'ini açıyor");
            var e = r.Effort;
            if (e.WalkRatio > MaxWalkRatio) r.Problems.Add($"yürüme %{e.WalkRatio * 100:0}");
            if (e.StraightRun > MaxStraightRun) r.Problems.Add($"düz itme {e.StraightRun}");
            if (e.PushesPerCrate > MaxPushesPerCrate) r.Problems.Add($"kasa başı itme {e.PushesPerCrate:0.0}");
            if (r.Height >= TripsFromHeight && e.EndToEndTrips > MaxTrips) r.Problems.Add($"uçtan uca {e.EndToEndTrips}");
            var c = r.Candy;
            if (c != null && c.Candy > 0)
            {
                if (c.Share < MinCandyShare) r.Problems.Add($"patlatma payı %{c.Share * 100:0}");
                if (c.Lonely > 0) r.Problems.Add($"yalnız patlatma kasası {c.Lonely}");
                if (c.Clusters < MinClusters) r.Problems.Add($"patlatma kümesi {c.Clusters}");
                if (c.MixedClusters < Math.Min(MinClusters, c.Clusters) || c.MixedClusters == 0) r.Problems.Add("kümelerde 3 renk karışık değil");
                if (c.ThinColours > 0) r.Problems.Add($"3'ten az kasalı renk {c.ThinColours}");
                if (c.ReadyTriple) r.Problems.Add("başta hazır üçlü");
                if (c.NextToPlace > 0) r.Problems.Add($"üçlüyü tamamlayan kasa hemen yanında {c.NextToPlace}");
            }
        }
    }
}
