namespace CrateBoss.Core
{
    /// <summary>
    /// One thing the player does (Ana Belge §5.6 step 1): walk or push in a direction, send a parked car along its axis, or
    /// (candy levels, owner 2026-11-01) swap a colour crate with its neighbour. A plain direction converts to a walk, so code
    /// that passes directions keeps working.
    /// </summary>
    public readonly struct PlayerAction : System.IEquatable<PlayerAction>
    {
        /// <summary>-1 = the worker moves; otherwise the parked car with this index is sent.</summary>
        public readonly int Car;
        public readonly Direction Direction;
        /// <summary>-1, or the cell index of the colour crate swapped with its neighbour in <see cref="Direction"/>.</summary>
        public readonly int Swap;
        /// <summary>A hammer hit on the breakable crate in this cell without walking to it (owner 2026-11-02), or -1.</summary>
        public readonly int Hit;
        /// <summary>The hit uses an advanced hammer (earned by long chains) instead of a plain one.</summary>
        public readonly bool Super;
        /// <summary>The paint gun (owner 2026-11-02): random colour crates take one colour.</summary>
        public readonly bool Paint;

        private PlayerAction(int car, Direction direction, int swap = -1, int hit = -1, bool super = false, bool paint = false)
        {
            Paint = paint;
            Car = car;
            Direction = direction;
            Swap = swap;
            Hit = hit;
            Super = super;
        }

        public bool IsCar => Car >= 0;
        public bool IsSwap => Swap >= 0;
        public bool IsHit => Hit >= 0;

        public static PlayerAction Walk(Direction dir) => new PlayerAction(-1, dir);
        public static PlayerAction SendCar(int car, Direction dir) => new PlayerAction(car, dir);
        public static PlayerAction SwapCandy(int cellIndex, Direction dir) => new PlayerAction(-1, dir, cellIndex);
        /// <summary>A hammer hit from afar: the auto hammer (plain hammers) or a tap with an advanced hammer.</summary>
        public static PlayerAction HitAt(int cellIndex, bool super) => new PlayerAction(-1, Direction.Up, -1, cellIndex, super);
        /// <summary>The paint gun: some colour crates, picked at random, all take one colour.</summary>
        public static PlayerAction UsePaint() => new PlayerAction(-1, Direction.Up, -1, -1, false, true);

        public static implicit operator PlayerAction(Direction dir) => Walk(dir);

        public bool Equals(PlayerAction other) => Car == other.Car && Direction == other.Direction && Swap == other.Swap && Hit == other.Hit && Super == other.Super && Paint == other.Paint;
        public override bool Equals(object obj) => obj is PlayerAction other && Equals(other);
        public override int GetHashCode() => ((Car * 8 + (int)Direction) * 31 + Swap) * 31 + Hit * 2 + (Super ? 1 : 0);
        public static bool operator ==(PlayerAction a, PlayerAction b) => a.Equals(b);
        public static bool operator !=(PlayerAction a, PlayerAction b) => !a.Equals(b);

        /// <summary>Solution text: U, D, L, R for the worker; the car number and u, d, l, r for a car ("3r"); swaps are not written.</summary>
        public override string ToString()
        {
            char c = Direction == Direction.Up ? 'U' : Direction == Direction.Down ? 'D' : Direction == Direction.Left ? 'L' : 'R';
            if (Paint) return "p";
            if (IsHit) return (Super ? "H" : "h") + Hit;
            if (IsSwap) return "s" + Swap + char.ToLowerInvariant(c);
            return IsCar ? Car.ToString() + char.ToLowerInvariant(c) : c.ToString();
        }
    }
}
