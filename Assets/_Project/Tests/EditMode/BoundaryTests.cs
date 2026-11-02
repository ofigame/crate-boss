using NUnit.Framework;
using CrateBoss.Core;
using CrateBoss.Levels;

namespace CrateBoss.Tests
{
    /// <summary>Outer boundary vs obstacles (Ana Belge §2.5 "Görünüm").</summary>
    public class BoundaryTests
    {
        [Test]
        public void OuterRing_IsBoundary_InnerWalls_AreObstacles()
        {
            var s = TestBoards.Make(
                new[] { "#######", "#.....#", "#.#...#", "#...#.#", "#.....#", "#######" },
                new[] { ".......", ".P.....", ".......", ".......", ".......", "......." });
            var outer = BoundaryMap.OuterWalls(s);
            bool Outer(int x, int y) => outer[y * s.Width + x];
            Assert.IsTrue(Outer(0, 0));
            Assert.IsTrue(Outer(6, 2));
            Assert.IsTrue(Outer(3, 5));
            Assert.IsFalse(Outer(2, 2), "a lone wall inside the yard is an obstacle");
            Assert.IsFalse(Outer(4, 3), "an inner wall touching only the yard is an obstacle");
            Assert.IsFalse(Outer(1, 1), "floor is never boundary");
        }

        [Test]
        public void WallTouchingTheRing_AndWallsBehindVoid_AreBoundary()
        {
            var s = TestBoards.Make(
                new[] { "_######", "_#.#..#", "_#.#..#", "##.#..#", "#.....#", "#######" },
                new[] { ".......", "..P....", ".......", ".......", ".......", "......." });
            var outer = BoundaryMap.OuterWalls(s);
            bool Outer(int x, int y) => outer[y * s.Width + x];
            Assert.IsTrue(Outer(3, 2), "a wall stub joined to the ring is part of the boundary");
            Assert.IsTrue(Outer(3, 3));
            Assert.IsTrue(Outer(1, 1), "reached over the void column");
        }

        [Test]
        public void RealLevels_HaveNoObstacleOnTheirBorder()
        {
            var loader = new LevelLoader();
            foreach (var data in loader.LoadAll())
            {
                var s = loader.BuildState(data);
                var outer = BoundaryMap.OuterWalls(s);
                for (int x = 0; x < s.Width; x++)
                {
                    var top = new GridPos(x, 0);
                    if (s.GetTile(top) == TileType.Wall) Assert.IsTrue(outer[x], $"level {data.id}: border wall at {top} must be boundary");
                }
            }
        }
    }
}
