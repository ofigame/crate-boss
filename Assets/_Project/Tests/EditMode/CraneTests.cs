using NUnit.Framework;
using CrateBoss.Core;
using CrateBoss.Levels;
using CrateBoss.Solver;

namespace CrateBoss.Tests
{
    /// <summary>Tower crane rules (Ana Belge §5.3).</summary>
    public class CraneTests
    {
        private static GameState WithCrane(GameState s, int every, int innerX, BoxType type)
        {
            s.SetCrane(new CraneDefinition(every, new[] { new CraneDrop(TestBoards.At(innerX), type) }));
            return s;
        }

        [Test]
        public void Counter_CountsOnlySuccessfulMoves()
        {
            var s = WithCrane(TestBoards.Row("......", "P....."), 2, 5, BoxType.Container);
            var rules = new RulesEngine();
            Assert.IsFalse(rules.TryMove(s, Direction.Left).Success);
            Assert.AreEqual(2, s.CraneCounter);
            rules.TryMove(s, Direction.Right);
            Assert.AreEqual(1, s.CraneCounter);
            var r = rules.TryMove(s, Direction.Right);
            Assert.IsNotNull(r.CraneDrop);
            var box = s.GetBoxAt(TestBoards.At(5));
            Assert.IsNotNull(box);
            Assert.AreEqual(BoxType.Container, box.Type);
            Assert.AreEqual(1, s.CraneIndex);
            Assert.IsFalse(s.HasActiveCrane);
        }

        [Test]
        public void Crane_WaitsWhileCellIsTaken_ThenDropsAfterNextMove()
        {
            var s = WithCrane(TestBoards.Row("......", "P....."), 2, 2, BoxType.Normal);
            var rules = new RulesEngine();
            rules.TryMove(s, Direction.Right);
            var r = rules.TryMove(s, Direction.Right); // player now stands on the drop cell
            Assert.IsNull(r.CraneDrop);
            Assert.AreEqual(0, s.CraneCounter);
            Assert.IsTrue(CraneSystem.IsWaiting(s));
            Assert.IsFalse(rules.TryMove(s, Direction.Up).Success); // a failed move does not retry
            Assert.IsTrue(CraneSystem.IsWaiting(s));
            r = rules.TryMove(s, Direction.Right);
            Assert.IsNotNull(r.CraneDrop);
            Assert.AreEqual(BoxType.Normal, s.GetBoxAt(TestBoards.At(2)).Type);
            Assert.AreEqual(2, s.CraneCounter);
        }

        [Test]
        public void Crane_WaitsForABoxOnTheCell()
        {
            var s = WithCrane(TestBoards.Row("......", "PB...."), 2, 2, BoxType.Container);
            var rules = new RulesEngine();
            rules.TryMove(s, Direction.Right); // box to cell 2
            rules.TryMove(s, Direction.Left);
            Assert.IsTrue(CraneSystem.IsWaiting(s));
            Assert.AreEqual(1, s.Boxes.Count);
        }

        [Test]
        public void WinningMove_DoesNotTriggerTheCrane()
        {
            var s = WithCrane(TestBoards.Row("...T..", "P.B..."), 2, 5, BoxType.Container);
            var rules = new RulesEngine();
            rules.TryMove(s, Direction.Right);
            var r = rules.TryMove(s, Direction.Right);
            Assert.IsTrue(r.IsWon);
            Assert.IsNull(r.CraneDrop);
            Assert.AreEqual(1, s.CraneCounter);
            Assert.AreEqual(1, s.Boxes.Count);
        }

        [Test]
        public void Container_CannotBePushed_JoinAChain_OrCoverATarget()
        {
            var rules = new RulesEngine();
            var s = TestBoards.Row("....", "P...");
            s.AddBox(BoxType.Container, TestBoards.At(1));
            Assert.IsFalse(rules.TryMove(s, Direction.Right).Success);

            s = TestBoards.Row("....", "PB..");
            s.AddBox(BoxType.Container, TestBoards.At(2));
            Assert.IsFalse(rules.TryMove(s, Direction.Right).Success);

            s = TestBoards.Row(".....", "PI...");
            s.AddBox(BoxType.Container, TestBoards.At(4));
            Assert.IsTrue(rules.TryMove(s, Direction.Right).Success);
            Assert.AreEqual(BoxType.Ice, s.GetBoxAt(TestBoards.At(3)).Type); // the ice stops in front of it

            s = TestBoards.Row("..T", "P..");
            s.AddBox(BoxType.Container, TestBoards.At(2));
            Assert.IsFalse(s.IsSolved());
        }

        [Test]
        public void DropIntoHole_FillsIt()
        {
            var s = WithCrane(TestBoards.Row("...o", "P..."), 2, 3, BoxType.Normal);
            var rules = new RulesEngine();
            rules.TryMove(s, Direction.Right);
            var r = rules.TryMove(s, Direction.Left);
            Assert.AreEqual(FallType.Hole, r.CraneDrop.FellInto);
            Assert.AreEqual(TileType.FilledHole, s.GetTile(TestBoards.At(3)));
            Assert.AreEqual(0, s.Boxes.Count);
        }

        [Test]
        public void Undo_RewindsTheCrane()
        {
            var session = new GameSession(WithCrane(TestBoards.Row("......", "P....."), 2, 5, BoxType.Container));
            session.Move(Direction.Right);
            var beforeDrop = session.State.Clone();
            Assert.IsNotNull(session.Move(Direction.Right).CraneDrop);
            Assert.IsTrue(session.Undo());
            Assert.IsTrue(session.State.SameAs(beforeDrop));
            Assert.AreEqual(0, session.State.CraneIndex);
            Assert.AreEqual(1, session.State.CraneCounter);
            Assert.AreEqual(0, session.State.Boxes.Count);
        }

        [Test]
        public void StateKey_IncludesTheCrane()
        {
            var a = WithCrane(TestBoards.Row("....", "P..."), 4, 3, BoxType.Container);
            var b = a.Clone();
            b.CraneCounter = 3;
            Assert.AreNotEqual(a.GetKey(), b.GetKey());
        }

        [TestCase("level_04", 9, 4)]  // İlk Vardiya
        [TestCase("level_06", 32, 4)] // Rıhtım
        public void SketchLevel_MatchesDesignDoc(string file, int moves, int tolerance)
        {
            var loader = new LevelLoader();
            var start = loader.BuildState(loader.Load(file));
            var solver = new LevelSolver();
            Assert.AreEqual(moves, solver.Solve(start).MoveCount);
            Assert.AreEqual(tolerance, CraneTolerance.Measure(start, solver));
        }

        [Test]
        public void Rihtim_IsUnsolvableWithoutTheCrane()
        {
            var loader = new LevelLoader();
            var start = loader.BuildState(loader.Load("level_06"));
            start.SetCrane(null);
            var result = new LevelSolver().Solve(start);
            Assert.IsFalse(result.IsSolved);
            Assert.IsFalse(result.HitLimit);
        }

        [Test]
        public void Parser_RejectsBadCranes()
        {
            var parser = new LevelParser();
            LevelData Make(int every, int x, string type, string objects = "P...") => new LevelData
            {
                ground = new[] { "#####", "#..T#", "#####" },
                objects = new[] { ".....", "." + objects.Substring(0, 3) + ".", "....." },
                crane = new CraneData { every = every, drops = new[] { new CraneDropData { x = x, y = 1, type = type } } },
            };
            Assert.DoesNotThrow(() => parser.Parse(Make(2, 2, "K")));
            Assert.Throws<LevelFormatException>(() => parser.Parse(Make(1, 2, "K")));  // every < 2
            Assert.Throws<LevelFormatException>(() => parser.Parse(Make(2, 0, "K")));  // wall
            Assert.Throws<LevelFormatException>(() => parser.Parse(Make(2, 3, "K")));  // target
            Assert.Throws<LevelFormatException>(() => parser.Parse(Make(2, 9, "K")));  // outside
            Assert.Throws<LevelFormatException>(() => parser.Parse(Make(2, 2, "X")));  // type
            Assert.Throws<LevelFormatException>(() => parser.Parse(Make(2, 2, "K", "PK."))); // K in objects
        }
    }
}
