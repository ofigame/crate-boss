using System.Collections.Generic;

namespace CrateBoss.Core
{
    /// <summary>
    /// What the truck did at the end of a move (Ana Belge §5.5): a full truck closed its tailgate and left with these crates,
    /// and/or the next truck backed in. <see cref="Remaining"/> trucks still to fill.
    /// </summary>
    public sealed class TruckEvent
    {
        private static readonly int[] NoBoxes = new int[0];

        /// <summary>Crates the leaving truck took (empty when none left this move).</summary>
        public IReadOnlyList<int> BoxIds { get; }
        public int Remaining { get; }
        public bool Left => BoxIds.Count > 0;
        /// <summary>A truck backed into the loading area this move.</summary>
        public bool Arrived { get; }
        /// <summary>Which loading area (index into GameState.Bays).</summary>
        public int Bay { get; }

        public TruckEvent(IReadOnlyList<int> boxIds, int remaining, bool arrived = false, int bay = 0)
        {
            Bay = bay;
            BoxIds = boxIds ?? NoBoxes;
            Remaining = remaining;
            Arrived = arrived;
        }
    }
}
