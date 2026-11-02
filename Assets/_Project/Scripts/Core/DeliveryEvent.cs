namespace CrateBoss.Core
{
    /// <summary>A delivery crate reached an area this move: locked in, or turned away (wrong area or the way stop missed).</summary>
    public sealed class DeliveryEvent
    {
        public int BoxId { get; }
        public GridPos Cell { get; }
        /// <summary>True: delivered and locked. False: the area flashes red, the crate stays pushable.</summary>
        public bool Accepted { get; }

        public DeliveryEvent(int boxId, GridPos cell, bool accepted)
        {
            BoxId = boxId;
            Cell = cell;
            Accepted = accepted;
        }
    }
}
