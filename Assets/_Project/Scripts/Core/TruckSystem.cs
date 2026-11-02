using System.Collections.Generic;

namespace CrateBoss.Core
{
    /// <summary>
    /// The trucks (Ana Belge §5.5, step 5 of a move), one loading area after another (owner 2026-11-02: up to four sides).
    /// A parked truck whose loading area is full leaves with those crates (they are gone from the yard), one truck less is
    /// needed there. When no truck is parked at an area, trucks are still to come for it and its route and loading area are
    /// all free (no crate, car or worker), the next one backs in. A blocked route keeps it away.
    /// </summary>
    public static class TruckSystem
    {
        /// <summary>Runs every loading area; returns the first event (null when none) and adds all of them to the move result.</summary>
        public static TruckEvent Step(GameState s, MoveResult result = null)
        {
            TruckEvent first = null;
            for (int i = 0; i < s.Bays.Length; i++)
            {
                var e = StepBay(s, i);
                if (e == null) continue;
                first ??= e;
                result?.TruckEvents.Add(e);
            }
            return first;
        }

        private static TruckEvent StepBay(GameState s, int index)
        {
            var bay = s.Bays[index];
            int left = s.TrucksAt(index);
            bool parked = s.ParkedAt(index);
            if (left <= 0) return null;
            List<int> ids = null;
            if (parked)
            {
                // Checked first without building lists: this runs on every move of every searched state.
                bool full = true;
                foreach (var c in bay.Cells)
                {
                    var box = s.GetBoxAt(c);
                    if (box == null || box.Type == BoxType.Container || (s.HasCargo && !box.IsCargo)) { full = false; break; }
                }
                if (!full) return null;
                ids = new List<int>(bay.Cells.Count);
                foreach (var c in bay.Cells)
                {
                    var box = s.GetBoxAt(c);
                    ids.Add(box.Id);
                    s.RemoveBox(box);
                }
                left--;
                parked = false;
            }
            bool arrived = false;
            if (!parked && left > 0 && RouteClear(s, bay))
            {
                parked = true;
                arrived = true;
            }
            if (ids == null && !arrived) return null;
            s.SetBay(index, left, parked);
            return new TruckEvent(ids, left, arrived, index);
        }

        /// <summary>Nothing stands on the first area's route or loading area.</summary>
        public static bool RouteClear(GameState s) => RouteClear(s, s.Truck);

        /// <summary>Nothing stands on this route or loading area.</summary>
        public static bool RouteClear(GameState s, TruckBay bay)
        {
            foreach (var c in bay.Route) if (Taken(s, c)) return false;
            foreach (var c in bay.Cells) if (Taken(s, c)) return false;
            return true;
        }

        private static bool Taken(GameState s, GridPos c) => s.PlayerPos == c || s.GetBoxAt(c) != null || s.CarAt(c) >= 0;
    }
}
