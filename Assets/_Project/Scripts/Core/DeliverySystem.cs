using System.Collections.Generic;

namespace CrateBoss.Core
{
    /// <summary>
    /// Delivery check (Ana Belge §5.1, step 3 of a move): a delivery crate on a way stop (M) marks it passed; on its own area
    /// with the way stop done (when it needs one) it is delivered and locked for good; on another area, or on its own without
    /// the way stop, it is turned away and stays pushable.
    /// </summary>
    public static class DeliverySystem
    {
        private static readonly DeliveryEvent[] None = new DeliveryEvent[0];

        public static IReadOnlyList<DeliveryEvent> Step(GameState s, IReadOnlyList<int> movedIds)
        {
            if (s.Deliveries.Count == 0 || movedIds.Count == 0) return None;
            List<DeliveryEvent> events = null;
            foreach (int id in movedIds)
            {
                var box = s.GetBoxById(id);
                if (box == null || box.Delivery < 0 || box.Locked) continue;
                var e = Check(s, box);
                if (e != null) (events ??= new List<DeliveryEvent>(1)).Add(e);
            }
            return (IReadOnlyList<DeliveryEvent>)events ?? None;
        }

        private static DeliveryEvent Check(GameState s, Box box)
        {
            var tile = s.GetTile(box.Position);
            if (tile == TileType.Via) { box.ViaDone = true; return null; }
            if (tile != TileType.Arrival) return null;
            var def = s.Deliveries[box.Delivery];
            bool accepted = s.AreaAt(box.Position) == def.Area && (!def.NeedsVia || box.ViaDone);
            if (accepted) box.Locked = true;
            return new DeliveryEvent(box.Id, box.Position, accepted);
        }

        /// <summary>Every delivery of the level has a locked crate (a crate still to come from a crane does not count yet).</summary>
        public static bool AllDelivered(GameState s)
        {
            if (s.Deliveries.Count == 0) return false;
            for (int d = 0; d < s.Deliveries.Count; d++)
            {
                bool done = false;
                foreach (var b in s.Boxes)
                    if (b.Delivery == d && b.Locked) { done = true; break; }
                if (!done) return false;
            }
            return true;
        }
    }
}
