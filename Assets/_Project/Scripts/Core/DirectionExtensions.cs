namespace CrateBoss.Core
{
    public static class DirectionExtensions
    {
        public static readonly Direction[] All = { Direction.Up, Direction.Down, Direction.Left, Direction.Right };

        /// <summary>Grid offset; "up" is y - 1 (the first JSON row is the farthest one).</summary>
        public static GridPos ToOffset(this Direction dir)
        {
            switch (dir)
            {
                case Direction.Up: return new GridPos(0, -1);
                case Direction.Down: return new GridPos(0, 1);
                case Direction.Left: return new GridPos(-1, 0);
                default: return new GridPos(1, 0);
            }
        }
    }
}
