using System.Collections.Generic;

namespace CrateBoss.Levels
{
    /// <summary>The "parking" block (Ana Belge §6): parked cars, exits to the road, the truck route, its loading area and how many trucks.</summary>
    public class ParkingData
    {
        public CarData[] cars;
        public int[][] exits;
        public int[][] truckRoute;
        public int[][] bay;
        public int trucks = 1;
        /// <summary>More loading areas (owner 2026-11-02), each with its own route and trucks; null when none.</summary>
        public DockData[] docks;

        public static ParkingData FromJson(object node)
        {
            if (!(node is Dictionary<string, object> obj)) throw new LevelFormatException("'parking' must be an object.");
            var p = new ParkingData();
            if (obj.TryGetValue("cars", out var cars) && cars != null)
            {
                if (!(cars is List<object> list)) throw new LevelFormatException("parking.cars must be a list.");
                p.cars = new CarData[list.Count];
                for (int i = 0; i < list.Count; i++) p.cars[i] = CarData.FromJson(list[i], i);
            }
            if (obj.TryGetValue("exits", out var exits) && exits != null) p.exits = JsonCells.List(exits, "parking.exits");
            if (obj.TryGetValue("truckRoute", out var route) && route != null) p.truckRoute = JsonCells.List(route, "parking.truckRoute");
            if (obj.TryGetValue("bay", out var bay) && bay != null) p.bay = JsonCells.List(bay, "parking.bay");
            if (obj.TryGetValue("trucks", out var t) && t is double d) p.trucks = (int)d;
            if (obj.TryGetValue("docks", out var docks) && docks != null)
            {
                if (!(docks is List<object> list)) throw new LevelFormatException("parking.docks must be a list.");
                p.docks = new DockData[list.Count];
                for (int i = 0; i < list.Count; i++) p.docks[i] = DockData.FromJson(list[i], i);
            }
            return p;
        }
    }
}
