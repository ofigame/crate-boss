using System.Collections.Generic;

namespace CrateBoss.Core
{
    /// <summary>The car park of a level (Ana Belge §5.5, §6 "parking"): the cars and the exits to the road. Never changes.</summary>
    public sealed class ParkingLot
    {
        public IReadOnlyList<ParkedCar> Cars { get; }
        /// <summary>Cells where a car leaves the park for the city road.</summary>
        public IReadOnlyList<GridPos> Exits { get; }

        public ParkingLot(IReadOnlyList<ParkedCar> cars, IReadOnlyList<GridPos> exits)
        {
            Cars = cars;
            Exits = exits;
        }

        public bool IsExit(GridPos p)
        {
            foreach (var e in Exits) if (e == p) return true;
            return false;
        }
    }
}
