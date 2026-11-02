using System.Collections.Generic;

namespace CrateBoss.Levels
{
    /// <summary>One more truck loading area of a car park (owner 2026-11-02: trucks may come from up to four sides).</summary>
    public class DockData
    {
        public int[][] truckRoute;
        public int[][] bay;
        public int trucks = 1;

        public static DockData FromJson(object node, int index)
        {
            if (!(node is Dictionary<string, object> obj)) throw new LevelFormatException($"parking.docks[{index}] must be an object.");
            var d = new DockData();
            if (obj.TryGetValue("truckRoute", out var route) && route != null) d.truckRoute = JsonCells.List(route, "dock truckRoute");
            if (obj.TryGetValue("bay", out var bay) && bay != null) d.bay = JsonCells.List(bay, "dock bay");
            if (obj.TryGetValue("trucks", out var t) && t is double n) d.trucks = (int)n;
            return d;
        }
    }
}
