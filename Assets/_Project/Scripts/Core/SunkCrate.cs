namespace CrateBoss.Core
{
    /// <summary>A symbol crate that sank through the floor hatch in a match; the view animates from this.</summary>
    public sealed class SunkCrate
    {
        public int BoxId { get; }
        public GridPos Cell { get; }
        public int Symbol { get; }

        public SunkCrate(int boxId, GridPos cell, int symbol)
        {
            BoxId = boxId;
            Cell = cell;
            Symbol = symbol;
        }
    }
}
