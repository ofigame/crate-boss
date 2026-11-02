namespace CrateBoss.Core
{
    /// <summary>Combo colour crates (owner 2026-11-02).</summary>
    public enum CandySpecial
    {
        None,
        /// <summary>From a run of 4 along a row: clears its row when it sinks.</summary>
        StripeRow,
        /// <summary>From a run of 4 down a column: clears its column when it sinks.</summary>
        StripeColumn,
        /// <summary>From an L or T (a row run and a column run sharing a crate): clears the 3 × 3 round it.</summary>
        Bomb,
        /// <summary>From a straight run of 5: swapped with a colour crate it clears every crate of that colour.</summary>
        Rainbow,
    }
}
