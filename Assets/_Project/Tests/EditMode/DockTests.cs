using NUnit.Framework;
using CrateBoss.Core;
using CrateBoss.Levels;

namespace CrateBoss.Tests
{
    /// <summary>
    /// Owner 2026-11-02: trucks may back in from up to four sides (parking.docks), each loading area with its own trucks; and
    /// candy may frame the yard, falling inward toward the worker from every side.
    /// </summary>
    public class DockTests
    {
        private const string TwoDocks = @"{
  ""id"": 9010, ""name"": ""İki rampa"",
  ""ground"": [
    ""#######"",
    ""#.....#"",
    ""......."",
    ""#.....#"",
    ""#######""
  ],
  ""objects"": [
    ""......."",
    ""...P..."",
    ""..Y.Y.."",
    ""......."",
    "".......""
  ],
  ""goal"": [""truck""],
  ""parking"": { ""cars"": [], ""exits"": [], ""truckRoute"": [[0,2]], ""bay"": [[1,2]], ""trucks"": 1,
    ""docks"": [{ ""truckRoute"": [[6,2]], ""bay"": [[5,2]], ""trucks"": 1 }] }
}";

        private static GameState Level(string json) => new LevelParser().Parse(LevelLoader.FromJson(json));

        [Test]
        public void TwoDocks_EachTakesItsCargo_AndBothTrucksWin()
        {
            var s = Level(TwoDocks);
            Assert.AreEqual(2, s.Bays.Length);
            Assert.AreEqual(2, s.TrucksLeft);
            var rules = new RulesEngine();
            // The first move brings both trucks (a truck backs in after a move, when its way is free).
            Assert.IsTrue(rules.TryMove(s, Direction.Down).Success);
            Assert.IsTrue(s.ParkedAt(0) && s.ParkedAt(1));
            var r = rules.TryMove(s, Direction.Left);                   // the left cargo into the left loading cell
            Assert.IsTrue(r.Success);
            Assert.AreEqual(1, s.TrucksLeft, "the left truck left");
            Assert.AreEqual(0, s.TrucksAt(0));
            Assert.AreEqual(1, s.TrucksAt(1));
            Assert.AreEqual(0, r.TruckEvents[0].Bay);
            // Over to the right one.
            Assert.IsTrue(rules.TryMove(s, Direction.Right).Success);
            r = rules.TryMove(s, Direction.Right);
            Assert.IsTrue(r.Success);
            Assert.AreEqual(0, s.TrucksLeft);
            Assert.IsTrue(r.IsWon);
        }

        [Test]
        public void AParkedTruckRoute_IsAWall_OnlyForItsOwnDock()
        {
            var s = Level(TwoDocks);
            new RulesEngine().TryMove(s, Direction.Down);
            Assert.IsTrue(s.IsTruckWall(new GridPos(0, 2)));
            Assert.IsTrue(s.IsTruckWall(new GridPos(6, 2)));
            Assert.IsFalse(s.IsTruckWall(new GridPos(1, 2)), "the loading cell takes crates");
            Assert.AreEqual(1, s.BayAt(new GridPos(5, 2)));
        }

        // A candy frame: a band on the left and one on the right, the worker in the middle.
        private const string Frame = @"{
  ""id"": 9011, ""name"": ""Çerçeve"",
  ""ground"": [
    ""#######"",
    ""#.....#"",
    ""#.....#"",
    ""#.....#"",
    ""#######""
  ],
  ""objects"": [
    ""......."",
    "".13.31."",
    "".45P54."",
    "".13.31."",
    "".......""
  ],
  ""goal"": [""targets""],
  ""candy"": { ""drops"": 0 }
}";

        [Test]
        public void FrameCandy_TheBandsEndInward_ButCratesFallDown()
        {
            var s = Level(Frame);
            // The inward field still tells the belts where each band ends.
            var fall = CandySystem.Fall(s);
            Assert.AreEqual((sbyte)Direction.Right, fall[s.Index(new GridPos(1, 2))], "the left band ends to its right");
            Assert.AreEqual((sbyte)Direction.Left, fall[s.Index(new GridPos(5, 2))], "the right band ends to its left");
            // Owner 2026-11-02 (later): the crates themselves always come down toward the bottom of the phone.
            s.RemoveBox(s.GetBoxAt(new GridPos(2, 2)));
            var wave = new CandyWave();
            CandySystem.Slide(s, wave);
            Assert.IsNotNull(s.GetBoxAt(new GridPos(2, 2)), "the crate above came down");
            Assert.IsNull(s.GetBoxAt(new GridPos(2, 1)), "the hole is now at the top");
            Assert.AreEqual(1, wave.Falls.Count);
        }
    }
}
