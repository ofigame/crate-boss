namespace CrateBoss.Meta
{
    /// <summary>
    /// Stars for a win (Ana Belge §5.8): ★★★ up to the best solution (a known best that is not proven shortest: +10 %),
    /// ★★ up to the best + 25 %, else ★. In car parks ★★★ also needs no bump and ★★ at most two. There is no losing.
    /// </summary>
    public static class StarRating
    {
        /// <summary>Most moves that still give three stars.</summary>
        public static int Three(int best, bool exact = true) => exact ? best : best + (best + 9) / 10;

        /// <summary>Most moves that still give two stars.</summary>
        public static int Two(int best) => best + (best + 3) / 4;

        /// <param name="best">Moves of the best known solution; 0 or less = not known yet (moves alone then never cost a star).</param>
        /// <param name="exact">The best is the solver's proven shortest (false: a stored, known-best solution).</param>
        public static int Stars(int moves, int best, int collisions = 0, bool parking = false, bool exact = true)
        {
            int byMoves = best <= 0 || moves <= Three(best, exact) ? 3 : moves <= Two(best) ? 2 : 1;
            int byBumps = !parking || collisions == 0 ? 3 : collisions <= 2 ? 2 : 1;
            return System.Math.Min(byMoves, byBumps);
        }
    }
}
