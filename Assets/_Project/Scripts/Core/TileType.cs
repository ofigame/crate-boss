namespace CrateBoss.Core
{
    public enum TileType
    {
        Wall,
        Floor,
        Target,
        Hole,
        FilledHole,
        Void,
        /// <summary>B (or 1–3): a delivery area (Ana Belge §4.3, §5.1); which delivery it belongs to is in <see cref="GameState.AreaOf"/>.</summary>
        Arrival,
        /// <summary>A: start mark of the delivery crate. Only a picture: plain floor for the rules.</summary>
        StartMark,
        /// <summary>M: way stop; a delivery that asks for it must pass here before its area counts (Ana Belge §5.1).</summary>
        Via,
    }
}
