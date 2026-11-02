using NUnit.Framework;
using CrateBoss.Core;
using CrateBoss.Levels;

namespace CrateBoss.Tests
{
    /// <summary>The cargo belt (owner 2026-11-02): a cargo crate pushed onto it rides on as far as the way is free.</summary>
    public class ConveyorTests
    {
        private const string Json = @"{
  ""id"": 9002, ""name"": ""Bant deneme"",
  ""ground"": [
    ""########"",
    ""#......#"",
    ""#......#"",
    ""#......#"",
    ""########""
  ],
  ""objects"": [
    ""........"",
    ""........"",
    ""..PY...."",
    ""........"",
    ""........""
  ],
  ""goal"": [""targets""],
  ""conveyor"": [[4,2], [5,2], [6,2], [6,1]]
}";

        private static GameState Level() => new LevelParser().Parse(LevelLoader.FromJson(Json));

        [Test]
        public void CargoPushedOntoTheBelt_RidesToItsEnd()
        {
            var s = Level();
            var r = new RulesEngine().TryMove(s, Direction.Right);
            Assert.IsTrue(r.Success);
            Assert.IsNotNull(s.GetBoxAt(new GridPos(6, 1)), "it rode round the corner to the end");
            Assert.AreEqual(3, r.ConveyorMoves.Count);
        }

        [Test]
        public void TheBelt_StopsAtACrateInTheWay()
        {
            var s = Level();
            s.AddBox(BoxType.Normal, new GridPos(6, 2));
            var r = new RulesEngine().TryMove(s, Direction.Right);
            Assert.IsNotNull(s.GetBoxAt(new GridPos(5, 2)), "it rides up to the crate and waits");
            Assert.AreEqual(1, r.ConveyorMoves.Count);
        }

        // Owner 2026-11-02: no fixed belt; one lies by itself along an open way from under the candy to the truck.
        private const string Auto = @"{
  ""id"": 9003, ""name"": ""Kendi bandı"",
  ""ground"": [
    ""###.###"",
    ""#.....#"",
    ""#.....#"",
    ""#.....#"",
    ""#.....#"",
    ""#.....#"",
    ""#######""
  ],
  ""objects"": [
    ""......."",
    ""......."",
    "".12.13."",
    ""......."",
    ""...Y..."",
    ""...P..."",
    "".......""
  ],
  ""goal"": [""truck""],
  ""parking"": { ""cars"": [], ""exits"": [], ""truckRoute"": [[3,0]], ""bay"": [[3,1]], ""trucks"": 1 },
  ""candy"": { ""drops"": 0 },
  ""belts"": true
}";

        [Test]
        public void AnOpenWayUnderTheCandy_GetsABelt_AndTheCargoRidesIt()
        {
            var s = new LevelParser().Parse(LevelLoader.FromJson(Auto));
            var belts = ConveyorSystem.Belts(s);
            Assert.AreEqual(1, belts.Count, "one way through the gap at x 3");
            Assert.AreEqual(new GridPos(3, 3), belts[0][0]);
            var rules = new RulesEngine();
            rules.TryMove(s, Direction.Up); // the cargo onto the belt; the truck backs in this move
            Assert.IsNotNull(s.GetBoxAt(new GridPos(3, 2)), "it waits at the end until the truck stands there");
            var r = rules.TryMove(s, Direction.Left);
            Assert.IsTrue(r.IsWon, "then it rides in and the truck leaves full");
        }

        [Test]
        public void PlainCrates_DoNotRide()
        {
            var s = Level();
            var cargo = s.GetBoxAt(new GridPos(3, 2));
            s.RemoveBox(cargo);
            s.AddBox(BoxType.Normal, new GridPos(3, 2));
            new RulesEngine().TryMove(s, Direction.Right);
            Assert.IsNotNull(s.GetBoxAt(new GridPos(4, 2)));
        }
    }
}
