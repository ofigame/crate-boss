namespace CrateBoss.Core
{
    /// <summary>A combo crate made by a run (candy levels); a rainbow crate replaces the crate that stood there.</summary>
    public sealed class CandyMade
    {
        public int BoxId { get; }
        public GridPos Cell { get; }
        public CandySpecial Kind { get; }
        /// <summary>The crate it replaced (a rainbow is a new crate), or -1.</summary>
        public int ReplacedId { get; }

        public CandyMade(int boxId, GridPos cell, CandySpecial kind, int replacedId)
        {
            BoxId = boxId;
            Cell = cell;
            Kind = kind;
            ReplacedId = replacedId;
        }
    }
}
