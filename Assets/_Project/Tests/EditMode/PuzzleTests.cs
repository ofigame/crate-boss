using NUnit.Framework;
using CrateBoss.Core;
using CrateBoss.Levels;

namespace CrateBoss.Tests
{
    /// <summary>
    /// Owner 2026-11-02: a classic puzzle in the middle of a candy yard. Its crates move only once the candy part is over; solved
    /// it pays hammers by its pushes and the barrier (G) opens.
    /// </summary>
    public class PuzzleTests
    {
        // Row 1 candy; the puzzle room below: the worker, a crate, a target; the barrier at the room's right side.
        private const string Json = @"{
  ""id"": 9020, ""name"": ""Bulmaca"",
  ""ground"": [
    ""########"",
    ""#......#"",
    ""#......#"",
    ""#....TG#"",
    ""#......#"",
    ""########""
  ],
  ""objects"": [
    ""........"",
    "".1313..."",
    ""........"",
    "".PB....."",
    ""........"",
    ""........""
  ],
  ""goal"": [""targets""],
  ""candy"": { ""drops"": 0, ""colors"": [1, 3], ""puzzle"": { ""par"": 3, ""reward"": 20 } }
}";

        private static GameState Level() => new LevelParser().Parse(LevelLoader.FromJson(Json));

        [Test]
        public void ThePuzzleCrates_WaitForTheCandy_ThenPayAndOpenTheBarrier()
        {
            var s = Level();
            var rules = new RulesEngine();
            Assert.IsFalse(s.GatesOpen, "a barrier waits for the puzzle");
            Assert.IsFalse(GoalRules.PlayerCanEnter(s, new GridPos(6, 3)));
            var r = rules.TryMove(s, Direction.Right);
            Assert.IsFalse(r.Success, "no pushing while the candy part lasts");
            Assert.IsTrue(r.PuzzleLocked);
            s.CandyDone = true;
            s.Hammers = 0;
            Assert.IsTrue(rules.TryMove(s, Direction.Right).Success);
            Assert.IsTrue(rules.TryMove(s, Direction.Right).Success);
            r = rules.TryMove(s, Direction.Right);
            Assert.IsTrue(r.GatesOpened, "the crate is on the target");
            Assert.AreEqual(20, r.PuzzleReward, "solved in par: the whole reward");
            Assert.AreEqual(20, s.Hammers);
            Assert.IsTrue(GoalRules.PlayerCanEnter(s, new GridPos(6, 3)), "the barrier is open");
        }

        [Test]
        public void MorePushesThanPar_PayLess()
        {
            Assert.AreEqual(20, PuzzleSystem.Reward(20, 10, 10));
            Assert.AreEqual(14, PuzzleSystem.Reward(20, 10, 15));
            Assert.AreEqual(9, PuzzleSystem.Reward(20, 10, 20));
            Assert.AreEqual(5, PuzzleSystem.Reward(20, 10, 40));
        }
    }
}
