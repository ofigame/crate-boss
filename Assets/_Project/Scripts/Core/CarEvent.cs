namespace CrateBoss.Core
{
    /// <summary>A car was sent: it drove off through an exit, or ran into something and bounced back (Ana Belge §5.5).</summary>
    public sealed class CarEvent
    {
        public int Car { get; }
        public Direction Direction { get; }
        /// <summary>True: it left the park. False: it bumped and is back on its bay (only the bump counter changed).</summary>
        public bool Left { get; }
        /// <summary>Last free cell in front of it: the exit it left by, or the cell before what it hit (its own front cell when blocked at once).</summary>
        public GridPos ReachedCell { get; }

        public CarEvent(int car, Direction direction, bool left, GridPos reachedCell)
        {
            Car = car;
            Direction = direction;
            Left = left;
            ReachedCell = reachedCell;
        }
    }
}
