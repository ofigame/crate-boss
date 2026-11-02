namespace CrateBoss.Core
{
    /// <summary>
    /// One entry of a crane's drop list: where it drops and what (Ana Belge §5.3 "drops[].type"): a fixed container (K),
    /// a normal crate (B), a colour crate (1–6), a delivery crate (D, with its delivery), a heavy crate (H) or an ice crate (I).
    /// </summary>
    public sealed class CraneDrop
    {
        public GridPos Cell { get; }
        public BoxType Type { get; }
        /// <summary>Colour 1–6 of a colour crate; 0 otherwise.</summary>
        public int Symbol { get; }
        /// <summary>Delivery index of a delivery crate; -1 otherwise.</summary>
        public int Delivery { get; }

        public CraneDrop(GridPos cell, BoxType type, int symbol = 0, int delivery = -1)
        {
            Cell = cell;
            Type = type;
            Symbol = symbol;
            Delivery = type == BoxType.Delivery ? delivery : -1;
        }
    }
}
