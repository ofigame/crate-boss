namespace CrateBoss.Puzzle
{
    /// <summary>The "kapı" checks (brief §4 "KOLAY ENGEL YASAĞI").</summary>
    public sealed class DoorMeasures
    {
        /// <summary>Moments the worker's reachable area grows by 4 cells or more.</summary>
        public int Doors;
        /// <summary>Doors one push of one crate opens from the position after the door before.</summary>
        public int EasyDoors;
        /// <summary>Share of all the area opened during the solution that the first three pushes open.</summary>
        public double OpenedByFirstThree;
    }
}
