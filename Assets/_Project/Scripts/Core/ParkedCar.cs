namespace CrateBoss.Core
{
    /// <summary>A parked toy car (Ana Belge §5.5): two cells side by side; it only drives along that line. Never changes.</summary>
    public sealed class ParkedCar
    {
        public GridPos A { get; }
        public GridPos B { get; }
        /// <summary>Colour name from the level file ("pink", "teal" …), only for the picture.</summary>
        public string Color { get; }

        public ParkedCar(GridPos a, GridPos b, string color)
        {
            A = a;
            B = b;
            Color = color;
        }

        public bool Horizontal => A.Y == B.Y;
        public bool Covers(GridPos p) => p == A || p == B;

        /// <summary>The car can be sent this way (along its long axis).</summary>
        public bool AlongAxis(Direction dir) =>
            Horizontal ? dir == Direction.Left || dir == Direction.Right : dir == Direction.Up || dir == Direction.Down;

        /// <summary>The cell of the car that leads when it drives this way.</summary>
        public GridPos Front(Direction dir)
        {
            var o = dir.ToOffset();
            return (A.X - B.X) * o.X + (A.Y - B.Y) * o.Y > 0 ? A : B;
        }
    }
}
