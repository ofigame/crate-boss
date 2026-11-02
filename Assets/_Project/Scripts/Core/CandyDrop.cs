namespace CrateBoss.Core
{
    /// <summary>A colour crate a crane dropped into an emptied cell (candy levels); the view animates from this.</summary>
    public sealed class CandyDrop
    {
        public int BoxId { get; }
        public GridPos Cell { get; }
        public int Symbol { get; }
        /// <summary>Which of the four cranes round the yard dropped it (0 / 1 back left / right, 2 / 3 left / right side).</summary>
        public int Crane { get; }

        public CandyDrop(int boxId, GridPos cell, int symbol, int crane)
        {
            BoxId = boxId;
            Cell = cell;
            Symbol = symbol;
            Crane = crane;
        }
    }
}
