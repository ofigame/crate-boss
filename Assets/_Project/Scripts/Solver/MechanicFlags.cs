using System;

namespace CrateBoss.Solver
{
    /// <summary>Which elements a solution actually uses.</summary>
    [Flags]
    public enum MechanicFlags
    {
        None = 0,
        Normal = 1,
        Ice = 2,
        Heavy = 4,
        Hole = 8,
        Void = 16,
        /// <summary>The level has a crane (its drops race or help the solution).</summary>
        Crane = 32,
        // Goal pieces (Ana Belge §5): counted when the level has them.
        Truck = 256,
        /// <summary>Colour crates that sink three in a row (Ana Belge §5.4).</summary>
        Match = 131072,
    }
}
