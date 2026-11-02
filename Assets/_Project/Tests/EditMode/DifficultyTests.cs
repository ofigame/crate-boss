using NUnit.Framework;
using CrateBoss.Levels;
using CrateBoss.Solver;

namespace CrateBoss.Tests
{
    /// <summary>The four sketch levels in Resources/Levels keep exactly the measures recorded for them (difficulty score, Ana Belge §10.5).</summary>
    public class DifficultyTests
    {
        [TestCase("level_08", 28, 7, 36.6, DifficultyTier.Easy)]     // Duvar Freni
        [TestCase("level_10", 43, 11, 59.0, DifficultyTier.Medium)]  // Buzun Yolu
        [TestCase("level_13", 51, 9, 71.9, DifficultyTier.Hard)]     // Yanlış Yük
        [TestCase("level_15", 81, 29, 90.2, DifficultyTier.VeryHard)] // Ağır Fedakârlık
        public void SketchLevel_MatchesDesignDoc(string file, int moves, int pushes, double score, DifficultyTier tier)
        {
            var loader = new LevelLoader();
            var m = new LevelAnalyzer().Analyze(loader.BuildState(loader.Load(file)), measureTraps: false);
            Assert.AreEqual(moves, m.Moves);
            Assert.AreEqual(pushes, m.Pushes);
            Assert.AreEqual(score, m.Score, 0.06);
            Assert.AreEqual(tier, m.Tier);
        }

        [Test]
        public void TrapRatio_IsZeroWhenNothingCanGoWrong()
        {
            var m = new LevelAnalyzer().Analyze(TestBoards.Row("....T", "PB..."));
            Assert.AreEqual(0.0, m.TrapRatio, 1e-9);
            Assert.AreEqual(MechanicFlags.Normal, m.Mechanics);
        }

        [Test]
        public void TrapRatio_CountsDeadEnds()
        {
            // Pushing the box right first jams it in the right corner: some states are dead.
            var m = new LevelAnalyzer().Analyze(TestBoards.Make(
                new[] { "######", "#....#", "#T...#", "######" },
                new[] { "......", "......", "..BP..", "......" }));
            Assert.Greater(m.TrapRatio, 0.0);
        }
    }
}
