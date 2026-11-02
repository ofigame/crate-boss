using System.Collections.Generic;
using NUnit.Framework;
using CrateBoss.Core;
using CrateBoss.Levels;

namespace CrateBoss.Tests
{
    /// <summary>
    /// The league levels (owner 2026-11-02: 8 leagues x 25 (was 5 x 25); crates, fixed crates, cranes, colour, ice and heavy crates, holes, belts): every file loads, the
    /// leagues come in order, and every stored solution wins with the real rules on its last move (a replay, no search).
    /// </summary>
    public class LeagueLevelTests
    {
        private static List<LevelData> Levels() => new LevelLoader(LevelLoader.GameFolder).LoadAll();

        [Test]
        public void LeagueSet_Has200Levels_InEightLeaguesOf25()
        {
            var levels = Levels();
            Assert.AreEqual(200, levels.Count);
            for (int i = 0; i < levels.Count; i++) Assert.AreEqual(i / 25 + 1, levels[i].league, levels[i].name);
        }

        [Test]
        public void EveryStoredSolution_WinsOnItsLastMove()
        {
            var loader = new LevelLoader(LevelLoader.GameFolder);
            foreach (var data in Levels())
            {
                if (data.candy != null) continue; // candy levels (owner 2026-11-01): random refills, no stored way
                Assert.IsFalse(string.IsNullOrEmpty(data.solution), data.name);
                var state = loader.BuildState(data);
                var rules = new RulesEngine();
                var moves = SolutionText.Parse(data.solution);
                Assert.IsNotNull(moves, data.name);
                for (int i = 0; i < moves.Count; i++)
                {
                    var r = rules.TryMove(state, moves[i]);
                    Assert.IsTrue(r.Success, $"{data.name}: move {i} failed");
                    Assert.AreEqual(i == moves.Count - 1, r.IsWon, $"{data.name}: won at move {i} of {moves.Count}");
                }
            }
        }

        [Test]
        public void OnlyAnaBelgeLetters()
        {
            foreach (var data in Levels())
            {
                foreach (var row in data.ground)
                    foreach (char c in row) Assert.IsTrue("#.Tov_BAM123G".IndexOf(c) >= 0, $"{data.name}: ground '{c}'");
                foreach (var row in data.objects)
                    foreach (char c in row) Assert.IsTrue(".BPIHDYF123456".IndexOf(c) >= 0, $"{data.name}: object '{c}'");
            }
        }
    }
}
