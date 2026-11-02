using NUnit.Framework;
using CrateBoss.Levels;
using CrateBoss.Solver;

namespace CrateBoss.Tests
{
    /// <summary>Every test board (Resources/DemoLevels: cranes, the truck) loads and can be won.</summary>
    public class DemoLevelTests
    {
        [Test]
        public void AllDemoBoards_LoadAndSolve()
        {
            var loader = new LevelLoader("DemoLevels");
            var solver = new LevelSolver();
            var all = loader.LoadAll();
            Assert.GreaterOrEqual(all.Count, 2); // the goal-type demos went with the removed mechanics (Ana Belge §20)
            foreach (var data in all)
            {
                var start = loader.BuildState(data);
                var r = solver.Solve(start);
                TestContext.WriteLine($"{data.id} {data.name}: {r}");
                Assert.IsTrue(r.IsSolved, $"{data.name}: {r}");
            }
        }
    }
}
