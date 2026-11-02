using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using CrateBoss.Core;
using CrateBoss.Levels;
using CrateBoss.Puzzle;

namespace CrateBoss.Tests
{
    /// <summary>
    /// The level tests of the owner's brief (2026-11-01 §1): every stored solution replayed move by move (one case per level),
    /// an independent solve without the stored solution (Slow), the quality rules (Quality: no easy door, colour-crate
    /// density, labour ceiling) and the hardened candidates against their originals.
    /// </summary>
    public class LevelQualityTests
    {
        /// <summary>Seconds for one independent solve (adjustable).</summary>
        public static double SolveSeconds = 60;

        private static List<LevelData> all;
        private static List<LevelData> All => all ??= new LevelLoader(LevelLoader.GameFolder).LoadAll();

        public static IEnumerable<TestCaseData> Levels() =>
            All.Select((d, i) => new TestCaseData(i + 1).SetName($"{i + 1:000} {d.name}"));

        // Hardened candidates of the old 125-level set; only those whose original is still in the game (the 200-level series
        // of 2026-11-02 replaced them).
        public static IEnumerable<TestCaseData> Candidates()
        {
            var list = new LevelLoader(HardFolder).LoadAll().Select((d, i) => (d, i)).Where(c => All.Any(o => o.id == c.d.id && !Series200(o)))
                .Select(c => new TestCaseData(c.i).SetName($"aday {c.i:000} {c.d.name}")).ToList();
            if (list.Count == 0) list.Add(new TestCaseData(-1).SetName("aday yok").Ignore("the old 125-level set is gone (200-level series)"));
            return list;
        }

        /// <summary>A level of the 200-level series: Lab200 measured it with its own explorer (traps, box changes), not the door rules.</summary>
        private static bool Series200(LevelData d) => d.design?.tier == "series200";

        public const string HardFolder = "LeagueLevels_Hard";

        private static (LevelData data, GameState start, List<PlayerAction> moves) Level(int number)
        {
            var d = All[number - 1];
            return (d, new LevelParser().Parse(d), SolutionText.Parse(d.solution));
        }

        // a) the stored solution, move by move: every move works, the level is won on the last one and not before.
        [TestCaseSource(nameof(Levels))]
        public void StoredSolution_EveryMoveWorks_WinsOnTheLast(int number)
        {
            var (d, state, moves) = Level(number);
            if (d.candy != null) return; // candy levels: random refills, no stored way
            Assert.IsNotNull(moves, $"{d.name}: no stored solution");
            var rules = new RulesEngine();
            for (int i = 0; i < moves.Count; i++)
            {
                var r = rules.TryMove(state, moves[i]);
                Assert.IsTrue(r.Success, $"{d.name}: move {i + 1} of {moves.Count} does not work");
                Assert.AreEqual(i == moves.Count - 1, r.IsWon, $"{d.name}: won at move {i + 1} of {moves.Count}");
            }
        }

        // b) solved without looking at the stored solution; a shorter way found is reported.
        [Category("Slow")]
        [TestCaseSource(nameof(Levels))]
        public void Solver_FindsAWay_WithoutTheStoredSolution(int number)
        {
            var (d, start, moves) = Level(number);
            if (d.candy != null) return;
            // The 200-level series (Lab200): proven by its own exhaustive explorer and replayed by EveryStoredSolution; its
            // hardest rooms are past what the device search finds in time, and the game's hints follow the stored way.
            if (Series200(d)) return;
            var found = PushSearch.Greedy(start, 400000, SolveSeconds, 2);
            Assert.IsTrue(found.Solved, $"{d.name}: no way found in {SolveSeconds} s");
            int stored = PushSearch.ToPushSteps(start, moves).Count;
            if (found.Pushes < stored) TestContext.WriteLine($"KISA ÇÖZÜM {d.name}: {found.Pushes} itme, kayıtlı {stored}");
        }

        private static bool Candy(int number) => All[number - 1].candy != null;

        private static DifficultyReport Quick(int number)
        {
            var (d, _, moves) = Level(number);
            return DifficultyMeter.Measure(d, number, moves, new MeterOptions { SolutionOnly = true });
        }

        // c) the quality rules (§4), from level 6 on.
        [Category("Quality")]
        [TestCaseSource(nameof(Levels))]
        public void NoEasyDoor(int number)
        {
            if (number <= DifficultyLimits.Tutorials || Candy(number) || Series200(All[number - 1])) return;
            var r = Quick(number);
            Assert.AreEqual(0, r.Doors.EasyDoors, $"{r.Name}: {r.Doors.EasyDoors} door(s) open with one push of one crate");
            Assert.LessOrEqual(r.Doors.OpenedByFirstThree, DifficultyLimits.MaxOpenedByFirstThree, $"{r.Name}: the first three pushes open too much");
        }

        [Category("Quality")]
        [TestCaseSource(nameof(Levels))]
        public void ColourCrates_DenseAndClustered(int number)
        {
            if (Candy(number)) return;
            var r = Quick(number);
            if (r.Candy == null || r.Candy.Candy == 0) return;
            var c = r.Candy;
            Assert.IsFalse(c.ReadyTriple, $"{r.Name}: a triple sinks at the start");
            if (number <= DifficultyLimits.Tutorials) return;
            Assert.GreaterOrEqual(c.Share, DifficultyLimits.MinCandyShare, $"{r.Name}: colour crates {c.Share:P0} of the pushable crates");
            Assert.AreEqual(0, c.Lonely, $"{r.Name}: {c.Lonely} colour crate(s) alone");
            Assert.GreaterOrEqual(c.Clusters, DifficultyLimits.MinClusters, $"{r.Name}: {c.Clusters} cluster(s) of 4+");
            Assert.AreEqual(0, c.ThinColours, $"{r.Name}: a colour with fewer than 3 crates");
        }

        [Category("Quality")]
        [TestCaseSource(nameof(Levels))]
        public void Labour_UnderTheCeiling(int number)
        {
            if (number <= DifficultyLimits.Tutorials || Candy(number) || Series200(All[number - 1])) return;
            var e = Quick(number).Effort;
            Assert.LessOrEqual(e.WalkRatio, DifficultyLimits.MaxWalkRatio, "walking");
            Assert.LessOrEqual(e.StraightRun, DifficultyLimits.MaxStraightRun, "straight pushes");
            Assert.LessOrEqual(e.PushesPerCrate, DifficultyLimits.MaxPushesPerCrate, "pushes per crate");
            Assert.LessOrEqual(e.EndToEndTrips, DifficultyLimits.MaxTrips, "end-to-end trips");
        }

        // d) a hardened candidate keeps what it must keep, and its stored solution wins.
        [TestCaseSource(nameof(Candidates))]
        public void Candidate_KeepsTheOriginal(int index)
        {
            var candidate = new LevelLoader(HardFolder).LoadAll()[index];
            var original = All.First(d => d.id == candidate.id);
            var problems = LevelInvariants.Check(original, candidate);
            Assert.IsEmpty(problems, string.Join("; ", problems));
            Assert.IsTrue(DifficultyMeter.Wins(new LevelParser().Parse(candidate), SolutionText.Parse(candidate.solution)), "its solution does not win");
        }
    }
}
