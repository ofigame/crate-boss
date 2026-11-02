namespace CrateBoss.Core
{
    /// <summary>
    /// A breakable crate that lost hits because colour crates sank next to it or a combo caught it (owner 2026-11-02): its
    /// hits left, and whether that broke it.
    /// </summary>
    public sealed class CrateChip
    {
        public int BoxId { get; }
        public GridPos Cell { get; }
        public int HitsLeft { get; }
        public bool Broke => HitsLeft <= 0;

        public CrateChip(int boxId, GridPos cell, int hitsLeft)
        {
            BoxId = boxId;
            Cell = cell;
            HitsLeft = hitsLeft;
        }
    }
}
