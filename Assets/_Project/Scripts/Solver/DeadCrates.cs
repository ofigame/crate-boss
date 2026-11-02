using CrateBoss.Core;

namespace CrateBoss.Solver
{
    /// <summary>
    /// Search pruning for the classic target goal: a crate pushed into a corner made by two walls can never move again
    /// (the player would have to stand inside a wall to push it out). When the crates that can still reach a target, plus
    /// the crates still to come from machines, are fewer than the targets, the position cannot be won and the solver drops
    /// it. Only the solver's speed changes: a dropped state never leads to a win, so the shortest solutions stay the same.
    /// Switched off for goals where a stuck crate could still be fine (trucks, colours, order, stages, road) and on belts.
    /// </summary>
    public sealed class DeadCrates
    {
        private readonly bool[] corner;
        private readonly int targets;
        public bool Enabled { get; }

        private DeadCrates(bool[] corner, int targets, bool enabled)
        {
            this.corner = corner;
            this.targets = targets;
            Enabled = enabled;
        }

        public static DeadCrates For(GameState s)
        {
            var g = s.Goal;
            bool enabled = (g & GoalType.Targets) != 0 && (g & GoalType.Trucks) == 0 && s.Truck == null;
            var corner = new bool[s.Width * s.Height];
            int targets = 0;
            for (int y = 0; y < s.Height && enabled; y++)
            for (int x = 0; x < s.Width; x++)
            {
                var p = new GridPos(x, y);
                var t = s.GetTile(p);
                if (t == TileType.Target) { targets++; continue; }
                if (t == TileType.Hole || t == TileType.Void || t == TileType.Wall) continue;
                bool Wall(int dx, int dy) => s.GetTile(new GridPos(x + dx, y + dy)) == TileType.Wall;
                corner[s.Index(p)] = (Wall(-1, 0) || Wall(1, 0)) && (Wall(0, -1) || Wall(0, 1));
            }
            return new DeadCrates(corner, targets, enabled && targets > 0);
        }

        /// <summary>True when the targets can no longer all be covered.</summary>
        public bool IsDead(GameState s)
        {
            if (!Enabled) return false;
            int usable = 0;
            foreach (var b in s.Boxes)
            {
                if (b.Type == BoxType.Container || b.Symbol > 0) continue; // symbol crates never fill a target
                var t = s.GetTile(b.Position);
                if (t == TileType.Target || !corner[s.Index(b.Position)]) usable++;
            }
            if (usable >= targets) return false;
            // Crates still to come: crane loads that are not containers.
            for (int i = 0; i < s.Cranes.Count; i++)
            {
                var drops = s.Cranes[i].Drops;
                for (int d = s.CraneStates[i].Index; d < drops.Count; d++)
                    if (drops[d].Type != BoxType.Container) usable++;
            }
            return usable < targets;
        }
    }
}
