using System.Collections.Generic;

namespace CrateBoss.Core
{
    /// <summary>
    /// The crane step run after every successful move that did not win (Ana Belge §5.3):
    /// count down, and at zero try to drop. A box, a car or the player on the drop cell makes the crane wait
    /// (counter stays at 0, it tries again after the next move). Several cranes run independently,
    /// in list order (A, B, C), so a box crane A just dropped makes crane B wait on the same cell.
    /// </summary>
    public static class CraneSystem
    {
        private static readonly List<CraneDropEvent> None = new List<CraneDropEvent>();

        /// <summary>Steps every crane in order; returns the drops that happened (an empty list when none).</summary>
        public static IReadOnlyList<CraneDropEvent> StepAll(GameState state)
        {
            List<CraneDropEvent> drops = null;
            for (int i = 0; i < state.Cranes.Count; i++)
            {
                var e = Step(state, i);
                if (e == null) continue;
                (drops ??= new List<CraneDropEvent>(2)).Add(e);
            }
            return drops ?? None;
        }

        /// <summary>Steps the first crane only. Returns the drop that happened, or null.</summary>
        public static CraneDropEvent Step(GameState state) => Step(state, 0);

        /// <summary>Steps crane <paramref name="i"/>. Returns the drop that happened, or null.</summary>
        public static CraneDropEvent Step(GameState state, int i)
        {
            if (i >= state.Cranes.Count) return null;
            var crane = state.Cranes[i];
            var cs = state.CraneStates[i];
            if (cs.Index >= crane.Drops.Count) return null;
            if (cs.Counter > 0) cs.Counter--;
            if (cs.Counter > 0) return null;

            var drop = crane.Drops[cs.Index];
            if (IsBlocked(state, drop.Cell)) return null;

            cs.Index++;
            cs.Counter = crane.Every;
            switch (state.GetTile(drop.Cell))
            {
                case TileType.Hole:
                    state.SetTile(drop.Cell, TileType.FilledHole);
                    return new CraneDropEvent(-1, drop.Type, drop.Cell, FallType.Hole, i, drop.Symbol);
                case TileType.Void:
                    return new CraneDropEvent(-1, drop.Type, drop.Cell, FallType.Void, i, drop.Symbol);
                default:
                    var box = state.AddBox(drop.Type, drop.Cell, drop.Symbol, drop.Delivery);
                    return new CraneDropEvent(box.Id, drop.Type, drop.Cell, FallType.None, i, drop.Symbol);
            }
        }

        /// <summary>True while the first crane's next drop is due but its cell is taken.</summary>
        public static bool IsWaiting(GameState state) => IsWaiting(state, 0);

        /// <summary>True while crane <paramref name="i"/>'s next drop is due but its cell is taken.</summary>
        public static bool IsWaiting(GameState state, int i)
        {
            if (i >= state.Cranes.Count) return false;
            var crane = state.Cranes[i];
            var cs = state.CraneStates[i];
            return cs.Index < crane.Drops.Count && cs.Counter == 0 && IsBlocked(state, crane.Drops[cs.Index].Cell);
        }

        // The player, a box or a parked car on the cell holds the crane up (Ana Belge §5.3).
        private static bool IsBlocked(GameState state, GridPos cell) =>
            state.PlayerPos == cell || state.GetBoxAt(cell) != null || state.CarAt(cell) >= 0;
    }
}
