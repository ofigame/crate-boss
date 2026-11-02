using NUnit.Framework;
using CrateBoss.Core;
using CrateBoss.Meta;
using CrateBoss.Levels;
using CrateBoss.Solver;

namespace CrateBoss.Tests
{
    /// <summary>Crane drop kinds, the order of a move, stars and the A* search (Ana Belge §5.3, §5.6, §5.8, §10.3, §21).</summary>
    public class RuleOrderTests
    {
        private static readonly RulesEngine Rules = new RulesEngine();

        private static GameState WithDrop(string type, int x = 3, int y = 2, string ground2 = "#.....#") => GoalBoards.Make(
            new[] { "#######", "#.....#", ground2, "#....T#", "#######" },
            new[] { ".......", ".P.....", ".......", ".B.....", "......." },
            d => d.cranes = new[] { new CraneData { every = 2, drops = new[] { new CraneDropData { x = x, y = y, type = type } } } });

        private static CraneDropEvent DropAfterTwoMoves(GameState s)
        {
            Rules.TryMove(s, Direction.Right);
            return Rules.TryMove(s, Direction.Left).CraneDrop;
        }

        [TestCase("K", BoxType.Container, 0)]
        [TestCase("B", BoxType.Normal, 0)]
        [TestCase("H", BoxType.Heavy, 0)]
        [TestCase("I", BoxType.Ice, 0)]
        [TestCase("4", BoxType.Normal, 4)]
        public void Crane_DropsEveryKind(string type, BoxType kind, int symbol)
        {
            var s = WithDrop(type);
            var e = DropAfterTwoMoves(s);
            Assert.IsNotNull(e);
            var box = s.GetBoxById(e.BoxId);
            Assert.AreEqual(kind, box.Type);
            Assert.AreEqual(symbol, box.Symbol);
            Assert.AreEqual(symbol, e.Symbol);
        }

        [Test]
        public void Crane_IntoAHole_MakesABridge()
        {
            var s = WithDrop("B", 3, 2, "#..o..#");
            var e = DropAfterTwoMoves(s);
            Assert.AreEqual(FallType.Hole, e.FellInto);
            Assert.AreEqual(TileType.FilledHole, s.GetTile(new GridPos(3, 2)));
        }

        [Test]
        public void CraneColourCrate_FinishesARun_InStep8()
        {
            var s = GoalBoards.Make(
                new[] { "#######", "#.....#", "#.....#", "#....T#", "#######" },
                new[] { ".......", ".P.....", "..2.2..", ".B.....", "......." },
                d => d.cranes = new[] { new CraneData { every = 2, drops = new[] { new CraneDropData { x = 3, y = 2, type = "2" } } } });
            Rules.TryMove(s, Direction.Right);
            var r = Rules.TryMove(s, Direction.Left);
            Assert.AreEqual(0, r.Sunk.Count, "nothing sank from the walk");
            Assert.AreEqual(3, r.CraneSunk.Count, "the crane's 2 joined the two 2s");
        }

        [Test]
        public void WinningMove_RunsNoCrane()
        {
            var s = GoalBoards.Make(
                new[] { "######", "#...T#", "######" },
                new[] { "......", ".PB...", "......" },
                d => d.cranes = new[] { new CraneData { every = 2, drops = new[] { new CraneDropData { x = 1, y = 1, type = "K" } } } });
            Rules.TryMove(s, Direction.Right);
            var r = Rules.TryMove(s, Direction.Right);
            Assert.IsTrue(r.IsWon);
            Assert.AreEqual(0, r.CraneDrops.Count, "step 6 stops before the cranes");
        }

        [Test]
        public void FailedMove_ChangesNothing_AndIsNotCounted()
        {
            var s = GoalBoards.Row("..T", "PBB");
            var before = s.Clone();
            Assert.IsFalse(Rules.TryMove(s, Direction.Right).Success, "two crates cannot be pushed");
            Assert.IsTrue(s.SameAs(before));
        }

        // ---------- stars (Ana Belge §5.8) ----------

        [Test]
        public void Stars_FollowTheBestSolution()
        {
            Assert.AreEqual(3, StarRating.Stars(20, 20));
            Assert.AreEqual(2, StarRating.Stars(21, 20));
            Assert.AreEqual(2, StarRating.Stars(25, 20), "+25 % still two");
            Assert.AreEqual(1, StarRating.Stars(26, 20));
            Assert.AreEqual(3, StarRating.Stars(22, 20, exact: false), "known best only: +10 % for three");
            Assert.AreEqual(3, StarRating.Stars(99, 0), "best not known yet: no star lost");
        }

        [Test]
        public void Stars_InACarPark_CountTheBumps()
        {
            Assert.AreEqual(3, StarRating.Stars(10, 10, 0, true));
            Assert.AreEqual(2, StarRating.Stars(10, 10, 2, true));
            Assert.AreEqual(1, StarRating.Stars(10, 10, 3, true));
            Assert.AreEqual(3, StarRating.Stars(10, 10, 5, false), "bumps only matter in a car park");
        }

        // ---------- solver (Ana Belge §10.3) ----------

        [Test]
        public void AStar_FindsTheSameShortestLength()
        {
            var s = TestBoards.Make(
                new[] { "########", "#......#", "#.##...#", "#..T.T.#", "#......#", "########" },
                new[] { "........", ".P......", "........", "..B..B..", "........", "........" });
            var bfs = new LevelSolver().Solve(s);
            var astar = new LevelSolver { UseAStar = true }.Solve(s);
            Assert.IsTrue(bfs.IsSolved);
            Assert.IsTrue(astar.IsSolved);
            Assert.AreEqual(bfs.MoveCount, astar.MoveCount);
            Assert.LessOrEqual(astar.ExploredStates, bfs.ExploredStates);
        }

        [Test]
        public void Solver_TimeLimit_ReportsUnknown()
        {
            var s = TestBoards.Row("..T", "PB.");
            var r = new LevelSolver { MaxSeconds = 0.000001 }.Solve(s);
            Assert.IsTrue(r.IsSolved || r.HitLimit, "never claims unsolvable when it ran out of time");
        }
    }
}
