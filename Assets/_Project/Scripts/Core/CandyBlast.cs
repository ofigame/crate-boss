namespace CrateBoss.Core
{
    /// <summary>A combo crate that went off (candy levels): where, and which kind; the view plays its blast from this.</summary>
    public sealed class CandyBlast
    {
        public GridPos Cell { get; }
        public CandySpecial Kind { get; }

        public CandyBlast(GridPos cell, CandySpecial kind)
        {
            Cell = cell;
            Kind = kind;
        }
    }
}
