namespace CrateBoss.Core
{
    /// <summary>One box's move during a turn; the view animates from this.</summary>
    public sealed class BoxMovement
    {
        public int BoxId { get; }
        public BoxType BoxType { get; }
        public GridPos From { get; }
        public GridPos To { get; }
        public FallType FellInto { get; }

        public BoxMovement(int boxId, BoxType boxType, GridPos from, GridPos to, FallType fellInto)
        {
            BoxId = boxId;
            BoxType = boxType;
            From = from;
            To = to;
            FellInto = fellInto;
        }

        public int Distance => From.ManhattanTo(To);
    }
}
