using System.Collections.Generic;

namespace CrateBoss.Core
{
    /// <summary>
    /// Splits the walls of a level for drawing (Ana Belge §2.5): an outer wall can be reached from outside
    /// the map by walking over walls and void only (flood fill from the border); every other wall is an obstacle inside
    /// the yard. The rules never use this: they still see '#'.
    /// </summary>
    public static class BoundaryMap
    {
        /// <summary>True per cell (index y * Width + x) for outer boundary walls.</summary>
        public static bool[] OuterWalls(GameState state)
        {
            int w = state.Width, h = state.Height;
            var outer = new bool[w * h];
            var seen = new bool[w * h];
            var queue = new Queue<GridPos>();
            void Visit(GridPos p)
            {
                if (!state.InBounds(p)) return;
                int i = p.Y * w + p.X;
                if (seen[i]) return;
                var t = state.GetTile(p);
                if (t != TileType.Wall && t != TileType.Void) return;
                seen[i] = true;
                queue.Enqueue(p);
            }
            // Outside the map counts as open ground: start from every border cell.
            for (int x = 0; x < w; x++) { Visit(new GridPos(x, 0)); Visit(new GridPos(x, h - 1)); }
            for (int y = 0; y < h; y++) { Visit(new GridPos(0, y)); Visit(new GridPos(w - 1, y)); }
            while (queue.Count > 0)
            {
                var p = queue.Dequeue();
                if (state.GetTile(p) == TileType.Wall) outer[p.Y * w + p.X] = true;
                Visit(new GridPos(p.X + 1, p.Y));
                Visit(new GridPos(p.X - 1, p.Y));
                Visit(new GridPos(p.X, p.Y + 1));
                Visit(new GridPos(p.X, p.Y - 1));
            }
            return outer;
        }

        /// <summary>Cells the player or a box can ever stand on or fall into (everything but walls and void).</summary>
        public static bool IsYard(TileType t) => t != TileType.Wall && t != TileType.Void;
    }
}
