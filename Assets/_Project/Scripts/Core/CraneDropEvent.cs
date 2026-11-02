namespace CrateBoss.Core
{
    /// <summary>A crane drop that happened at the end of a move; the view animates from this.</summary>
    public sealed class CraneDropEvent
    {
        /// <summary>Id of the new box, or -1 when it fell into a hole or the void.</summary>
        public int BoxId { get; }
        public BoxType Type { get; }
        public GridPos Cell { get; }
        public FallType FellInto { get; }
        /// <summary>Which crane dropped it (index into <see cref="GameState.Cranes"/>).</summary>
        public int Crane { get; }
        /// <summary>Colour of a colour crate (1–6), 0 otherwise.</summary>
        public int Symbol { get; }

        public CraneDropEvent(int boxId, BoxType type, GridPos cell, FallType fellInto, int crane = 0, int symbol = 0)
        {
            BoxId = boxId;
            Type = type;
            Cell = cell;
            FellInto = fellInto;
            Crane = crane;
            Symbol = symbol;
        }
    }
}
