namespace CrateBoss.Core
{
    /// <summary>Which cells the player and boxes may use, and what counts as a filled target. Shared by the push rules and the machines.</summary>
    public static class GoalRules
    {
        /// <summary>Static ground a player can stand on (not counting what is on it).</summary>
        public static bool IsWalkableTile(TileType t) =>
            t == TileType.Floor || t == TileType.Target || t == TileType.FilledHole || t == TileType.Arrival
            || t == TileType.StartMark || t == TileType.Via;

        /// <summary>The player may step here (ignoring boxes): walkable ground, no parked car, not under the parked truck.</summary>
        public static bool PlayerCanEnter(GameState s, GridPos p) =>
            IsWalkableTile(s.GetTile(p)) && s.CarAt(p) < 0 && !s.IsTruckWall(p);

        /// <summary>Something that stops a box entering a cell, apart from other boxes: a wall, a parked car, the parked truck.</summary>
        public static bool BlocksBox(GameState s, GridPos p) =>
            s.GetTile(p) == TileType.Wall || s.CarAt(p) >= 0 || s.IsTruckWall(p);

        /// <summary>A target counts as filled: a box that is not a crane container nor a match crate.</summary>
        public static bool IsCovered(GameState s, GridPos p)
        {
            var box = s.GetBoxAt(p);
            return box != null && box.Type != BoxType.Container && box.Symbol == 0;
        }

        /// <summary>Targets that belong to the classic goal (truck cargo cells are the truck's).</summary>
        public static bool IsGoalTarget(GameState s, GridPos p) => s.GetTile(p) == TileType.Target && s.BayAt(p) < 0;
    }
}
