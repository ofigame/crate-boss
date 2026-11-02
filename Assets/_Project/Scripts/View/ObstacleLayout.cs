using CrateBoss.Core;
using UnityEngine;

namespace CrateBoss.View
{
    /// <summary>
    /// Ana Belge §3: inner walls turn into obstacles by their shape, so nothing looks squeezed into one cell. Cells are taken in
    /// reading order: a free 2x2 square becomes a container stack, two cells side by side (across first, then down) a parked
    /// toy car, anything left a one-cell obstacle. The same board always gives the same layout.
    /// </summary>
    public static class ObstacleLayout
    {
        /// <summary>True: every inner wall cell is one piece (a fixed crate).</summary>
        public static bool OneCellOnly = true;

        /// <summary>The piece of every cell (index y * width + x); outer walls and non-walls stay Single.</summary>
        public static ObstaclePiece[] For(GameState s, bool[] outer)
        {
            int w = s.Width, h = s.Height;
            var pieces = new ObstaclePiece[w * h];
            var taken = new bool[w * h];
            bool Inner(int x, int y) => x >= 0 && y >= 0 && x < w && y < h && s.GetTile(new GridPos(x, y)) == TileType.Wall && !outer[y * w + x] && !taken[y * w + x];
            for (int i = 0; i < pieces.Length; i++) pieces[i] = ObstaclePiece.Single;
            // Owner 2026-10-31: no cars or container stacks in the yard any more; every inner wall is its own fixed crate.
            if (OneCellOnly) return pieces;
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                if (!Inner(x, y)) continue;
                int i = y * w + x;
                if (Inner(x + 1, y) && Inner(x, y + 1) && Inner(x + 1, y + 1))
                {
                    // World z grows up the screen while grid y grows down it, hence -0.5 on z.
                    var c = new Vector3(0.5f, 0f, -0.5f);
                    pieces[i] = new ObstaclePiece(ObstaclePieceKind.BigHead, c, false);
                    Take(i + 1, c - Vector3.right); Take(i + w, c + Vector3.forward); Take(i + w + 1, c - Vector3.right + Vector3.forward);
                    taken[i] = true;
                }
                else if (Inner(x + 1, y))
                {
                    pieces[i] = new ObstaclePiece(ObstaclePieceKind.CarHead, new Vector3(0.5f, 0f, 0f), false);
                    pieces[i + 1] = new ObstaclePiece(ObstaclePieceKind.CarTail, new Vector3(-0.5f, 0f, 0f), false);
                    taken[i] = taken[i + 1] = true;
                }
                else if (Inner(x, y + 1))
                {
                    pieces[i] = new ObstaclePiece(ObstaclePieceKind.CarHead, new Vector3(0f, 0f, -0.5f), true);
                    pieces[i + w] = new ObstaclePiece(ObstaclePieceKind.CarTail, new Vector3(0f, 0f, 0.5f), true);
                    taken[i] = taken[i + w] = true;
                }
                else taken[i] = true;
            }
            return pieces;

            void Take(int index, Vector3 offset)
            {
                pieces[index] = new ObstaclePiece(ObstaclePieceKind.BigPart, offset, false);
                taken[index] = true;
            }
        }
    }
}
