namespace CrateBoss.Core
{
    public static class TileRules
    {
        /// <summary>Plain ground the player can walk on and a box can rest on.</summary>
        public static bool IsSolidGround(TileType tile) =>
            tile == TileType.Floor || tile == TileType.Target || tile == TileType.FilledHole;
    }
}
