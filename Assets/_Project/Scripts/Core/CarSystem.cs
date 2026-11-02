namespace CrateBoss.Core
{
    /// <summary>
    /// Sending a parked car (Ana Belge §5.5): only along its long axis. It rolls cell by cell; if every cell up to an exit is
    /// free (no car, crate, wall, worker or parked truck) it drives out and its bay is free. Otherwise it bumps into the first
    /// thing in its way and rolls back: nothing changes on the board (only the bump counter, kept by the session).
    /// </summary>
    public static class CarSystem
    {
        /// <summary>Null when the car cannot be sent this way at all (no such car, gone, or across its axis): not a move.</summary>
        public static CarEvent Send(GameState s, int car, Direction dir)
        {
            if (!s.CarPresent(car)) return null;
            var c = s.Parking.Cars[car];
            if (!c.AlongAxis(dir)) return null;
            var step = dir.ToOffset();
            var cell = c.Front(dir);
            var reached = cell;
            if (s.Parking.IsExit(cell)) return Leave(s, car, dir, cell);
            while (true)
            {
                cell = cell + step;
                if (!Free(s, cell)) return new CarEvent(car, dir, false, reached);
                reached = cell;
                if (s.Parking.IsExit(cell)) return Leave(s, car, dir, cell);
            }
        }

        private static CarEvent Leave(GameState s, int car, Direction dir, GridPos exit)
        {
            s.CarsGone |= 1UL << car;
            return new CarEvent(car, dir, true, exit);
        }

        private static bool Free(GameState s, GridPos p)
        {
            if (!s.InBounds(p)) return false;
            var t = s.GetTile(p);
            if (t == TileType.Wall || t == TileType.Void || t == TileType.Hole) return false;
            return s.PlayerPos != p && s.GetBoxAt(p) == null && s.CarAt(p) < 0 && !s.IsTruckWall(p)
                   && !(s.BayAt(p) is int b && b >= 0 && s.ParkedAt(b));
        }
    }
}
