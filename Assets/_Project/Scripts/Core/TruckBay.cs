using System.Collections.Generic;

namespace CrateBoss.Core
{
    /// <summary>
    /// The truck of a level (Ana Belge §5.5): its loading area (the cargo cells that take crates like targets), the route it
    /// backs in along, and how many trucks come. A truck without a route is parked from the start (older "trucks" block).
    /// Never changes.
    /// </summary>
    public sealed class TruckBay
    {
        public IReadOnlyList<GridPos> Cells { get; }
        /// <summary>Cells the truck backs along and stands on (walls while it is parked); empty = no route.</summary>
        public IReadOnlyList<GridPos> Route { get; }
        public int Count { get; }

        public TruckBay(IReadOnlyList<GridPos> cells, int count) : this(cells, new GridPos[0], count) { }

        public TruckBay(IReadOnlyList<GridPos> cells, IReadOnlyList<GridPos> route, int count)
        {
            Cells = cells;
            Route = route;
            Count = count;
        }

        public bool HasRoute => Route.Count > 0;

        public bool Contains(GridPos p)
        {
            foreach (var c in Cells) if (c == p) return true;
            return false;
        }

        public bool OnRoute(GridPos p)
        {
            foreach (var c in Route) if (c == p) return true;
            return false;
        }
    }
}
