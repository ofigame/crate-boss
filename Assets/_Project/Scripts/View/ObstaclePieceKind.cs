namespace CrateBoss.View
{
    /// <summary>What an inner wall cell becomes (Ana Belge §3).</summary>
    public enum ObstaclePieceKind
    {
        /// <summary>A one-cell obstacle of the district (pallets, concrete block, cones, bin...).</summary>
        Single,
        /// <summary>The cell that draws a parked toy car covering it and its neighbour.</summary>
        CarHead,
        /// <summary>The car's second cell (drawn by the head).</summary>
        CarTail,
        /// <summary>The cell that draws a 2x2 container stack.</summary>
        BigHead,
        /// <summary>The other three cells of the stack.</summary>
        BigPart,
    }
}
