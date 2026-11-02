using CrateBoss.Levels;

namespace CrateBoss.Puzzle
{
    /// <summary>A copy of a level file whose rows can be changed without touching the original.</summary>
    public static class LevelCopy
    {
        public static LevelData Of(LevelData d) => new LevelData
        {
            id = d.id, name = d.name, ground = (string[])d.ground.Clone(), objects = (string[])d.objects.Clone(), crane = d.crane,
            cranes = d.cranes, goal = d.goal, solution = d.solution, league = d.league, trucks = d.trucks,
            deliveries = d.deliveries == null ? null : System.Array.ConvertAll(d.deliveries, x => new DeliveryData { box = x.box == null ? null : (int[])x.box.Clone(), area = x.area, via = x.via, heavy = x.heavy }),
            parking = d.parking, rank = d.rank, taskCard = d.taskCard, design = d.design,
        };
    }
}
