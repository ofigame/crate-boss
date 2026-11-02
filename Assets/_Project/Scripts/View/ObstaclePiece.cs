using UnityEngine;

namespace CrateBoss.View
{
    /// <summary>One inner wall cell's part in a bigger obstacle: its kind and where the whole piece's centre is from this cell.</summary>
    public readonly struct ObstaclePiece
    {
        public static readonly ObstaclePiece Single = new ObstaclePiece(ObstaclePieceKind.Single, Vector3.zero, false);

        public ObstaclePieceKind Kind { get; }
        /// <summary>From this cell's centre to the piece's centre (world units).</summary>
        public Vector3 Offset { get; }
        /// <summary>A car that runs up the screen (two cells in a column) instead of across.</summary>
        public bool Vertical { get; }

        public ObstaclePiece(ObstaclePieceKind kind, Vector3 offset, bool vertical)
        {
            Kind = kind;
            Offset = offset;
            Vertical = vertical;
        }
    }
}
