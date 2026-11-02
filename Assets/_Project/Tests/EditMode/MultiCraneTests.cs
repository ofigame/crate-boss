using NUnit.Framework;
using CrateBoss.Core;
using CrateBoss.Levels;
using CrateBoss.Solver;

namespace CrateBoss.Tests
{
    /// <summary>Several independent cranes (Ana Belge §5.3 "Çoklu vinç").</summary>
    public class MultiCraneTests
    {
        private static CraneDefinition Crane(string id, CraneSide side, int every, params (int x, BoxType type)[] drops)
        {
            var list = new CraneDrop[drops.Length];
            for (int i = 0; i < drops.Length; i++) list[i] = new CraneDrop(TestBoards.At(drops[i].x), drops[i].type);
            return new CraneDefinition(id, side, every, list);
        }

        [Test]
        public void TwoCranes_CountIndependently_AndBDropsWhileAIsHeldUp()
        {
            // A box parked on A's cell holds A up for good; B keeps its own count.
            var s = TestBoards.Row("........", ".B..P...");
            s.SetCranes(new[]
            {
                Crane("A", CraneSide.Left, 2, (1, BoxType.Container)),
                Crane("B", CraneSide.Right, 3, (7, BoxType.Container)),
            });
            var rules = new RulesEngine();
            rules.TryMove(s, Direction.Right);
            Assert.AreEqual(1, s.CraneStates[0].Counter);
            Assert.AreEqual(2, s.CraneStates[1].Counter);
            rules.TryMove(s, Direction.Left);
            Assert.IsTrue(CraneSystem.IsWaiting(s, 0));
            Assert.IsFalse(CraneSystem.IsWaiting(s, 1));
            Assert.IsFalse(rules.TryMove(s, Direction.Up).Success); // a failed move counts for no crane
            Assert.AreEqual(1, s.CraneStates[1].Counter);
            var r = rules.TryMove(s, Direction.Right);
            Assert.AreEqual(1, r.CraneDrops.Count);
            Assert.AreEqual(1, r.CraneDrops[0].Crane, "B drops although A is held up");
            Assert.AreEqual(BoxType.Container, s.GetBoxAt(TestBoards.At(7)).Type);
            Assert.IsTrue(CraneSystem.IsWaiting(s, 0));
            Assert.AreEqual(0, s.CraneStates[0].Index);
            Assert.AreEqual(1, s.CraneStates[1].Index);
        }

        [Test]
        public void SameCell_SameMove_BWaitsForWhatADropped()
        {
            var s = TestBoards.Row("......", "P.....");
            s.SetCranes(new[]
            {
                Crane("A", CraneSide.Left, 2, (4, BoxType.Normal)),
                Crane("B", CraneSide.Right, 2, (4, BoxType.Container)),
            });
            var rules = new RulesEngine();
            rules.TryMove(s, Direction.Right);
            var r = rules.TryMove(s, Direction.Left);
            Assert.AreEqual(1, r.CraneDrops.Count);
            Assert.AreEqual(0, r.CraneDrops[0].Crane);
            Assert.AreEqual(BoxType.Normal, s.GetBoxAt(TestBoards.At(4)).Type);
            Assert.IsTrue(CraneSystem.IsWaiting(s, 1));
            Assert.AreEqual(0, s.CraneStates[1].Counter);
        }

        [Test]
        public void OldCraneBlock_LoadsAsFirstCrane()
        {
            var loader = new LevelLoader();
            var data = loader.Load("level_06");
            Assert.IsTrue(data.HasOldCrane);
            var s = loader.BuildState(data);
            Assert.AreEqual(1, s.Cranes.Count);
            Assert.AreSame(s.Cranes[0], s.Crane);
            Assert.AreEqual("A", s.Crane.Id);
            Assert.AreEqual(CraneSide.Left, s.Crane.Side);
            Assert.AreEqual(6, s.CraneCounter);
            Assert.AreEqual(s.CraneStates[0].Counter, s.CraneCounter);
            Assert.AreEqual(0, s.CraneIndex);
            s.CraneCounter = 3;
            Assert.AreEqual(3, s.CraneStates[0].Counter);
        }

        private static LevelData Board(CraneData single, params CraneData[] list) => new LevelData
        {
            ground = new[] { "#######", "#....T#", "#######" },
            objects = new[] { ".......", ".PB....", "......." },
            crane = single,
            cranes = list,
        };

        private static CraneData C(string id, string side, int x) => new CraneData
        {
            id = id, side = side, every = 2, drops = new[] { new CraneDropData { x = x, y = 1, type = "K" } },
        };

        [Test]
        public void Parser_ChecksTheCraneList()
        {
            var parser = new LevelParser();
            Assert.DoesNotThrow(() => parser.Parse(Board(null, C("A", "left", 3), C("B", "right", 4), C("C", "top", 2))));
            Assert.Throws<LevelFormatException>(() => parser.Parse(Board(C("A", "left", 3), C("B", "right", 4))));          // both
            Assert.Throws<LevelFormatException>(() => parser.Parse(Board(null, C("A", "left", 3), C("B", "right", 4), C("C", "top", 2), C("D", "bottom", 4)))); // 4
            Assert.Throws<LevelFormatException>(() => parser.Parse(Board(null, C("A", "left", 3), C("A", "right", 4)))); // id twice
            Assert.Throws<LevelFormatException>(() => parser.Parse(Board(null, C("A", "left", 3), C("B", "left", 4))));  // side twice
            Assert.Throws<LevelFormatException>(() => parser.Parse(Board(null, C("A", "middle", 3))));                   // unknown side
            Assert.Throws<LevelFormatException>(() => parser.Parse(Board(null, C("A", "left", 5))));                     // drop on target
        }

        [Test]
        public void Parser_FillsMissingIdsAndSidesByPosition()
        {
            var s = new LevelParser().Parse(Board(null, C(null, null, 3), C(null, null, 4)));
            Assert.AreEqual("A", s.Cranes[0].Id);
            Assert.AreEqual(CraneSide.Left, s.Cranes[0].Side);
            Assert.AreEqual("B", s.Cranes[1].Id);
            Assert.AreEqual(CraneSide.Right, s.Cranes[1].Side);
        }

        [Test]
        public void WinningMove_RunsNoCrane()
        {
            var s = TestBoards.Row("...T..", "P.B...");
            s.SetCranes(new[]
            {
                Crane("A", CraneSide.Left, 2, (5, BoxType.Container)),
                Crane("B", CraneSide.Right, 2, (4, BoxType.Container)),
            });
            var rules = new RulesEngine();
            rules.TryMove(s, Direction.Right);
            var r = rules.TryMove(s, Direction.Right);
            Assert.IsTrue(r.IsWon);
            Assert.AreEqual(0, r.CraneDrops.Count);
            Assert.AreEqual(1, s.CraneStates[0].Counter);
            Assert.AreEqual(1, s.CraneStates[1].Counter);
        }

        [Test]
        public void Undo_RestoresEveryCrane()
        {
            var start = TestBoards.Row("......", "P.....");
            start.SetCranes(new[]
            {
                Crane("A", CraneSide.Left, 2, (5, BoxType.Container)),
                Crane("B", CraneSide.Right, 3, (4, BoxType.Normal)),
            });
            var session = new GameSession(start);
            session.Move(Direction.Right);
            session.Move(Direction.Right);
            var before = session.State.Clone();
            var r = session.Move(Direction.Right);
            Assert.AreEqual(1, r.CraneDrops.Count, "B drops on the third move");
            Assert.IsTrue(session.Undo());
            Assert.IsTrue(session.State.SameAs(before));
            Assert.AreEqual(1, session.State.CraneStates[0].Index);
            Assert.AreEqual(0, session.State.CraneStates[1].Index);
            Assert.AreEqual(1, session.State.CraneStates[1].Counter);
        }

        [Test]
        public void StateKey_IncludesEveryCrane()
        {
            var a = TestBoards.Row("....", "P...");
            a.SetCranes(new[] { Crane("A", CraneSide.Left, 3, (3, BoxType.Container)), Crane("B", CraneSide.Right, 4, (2, BoxType.Container)) });
            var b = a.Clone();
            b.CraneStates[1].Counter = 2;
            Assert.AreNotEqual(a.GetKey(), b.GetKey());
            Assert.IsFalse(a.SameAs(b));
            Assert.AreEqual(4, a.CraneStates[1].Counter, "Clone copies crane states instead of sharing them");
        }

        [Test]
        public void DemoLevel_TwoCranes_SolvesWithBothCranes()
        {
            var loader = new LevelLoader("DemoLevels");
            var data = loader.Load("demo_01");
            var start = loader.BuildState(data);
            Assert.AreEqual(2, start.Cranes.Count);
            Assert.AreEqual(CraneSide.Right, start.Cranes[1].Side);
            var solver = new LevelSolver();
            var result = solver.Solve(start);
            Assert.IsTrue(result.IsSolved, result.ToString());
            int slack = CraneTolerance.Measure(start, solver);
            Assert.GreaterOrEqual(slack, 0);
            TestContext.WriteLine($"demo_01: {result.MoveCount} moves, {result.ExploredStates} states, slack {slack}");
        }

        [Test]
        public void Writer_RoundTripsTheCraneList()
        {
            var loader = new LevelLoader("DemoLevels");
            var data = loader.Load("demo_01");
            var json = LevelJsonWriter.ToJson(data);
            var again = UnityEngine.JsonUtility.FromJson<LevelData>(json);
            Assert.IsFalse(again.HasOldCrane);
            Assert.AreEqual(2, again.cranes.Length);
            Assert.AreEqual("right", again.cranes[1].side);
            Assert.AreEqual(8, again.cranes[1].every);
            Assert.AreEqual("K", again.cranes[1].drops[0].type);
        }
    }
}
