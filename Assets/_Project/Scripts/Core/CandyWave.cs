using System.Collections.Generic;

namespace CrateBoss.Core
{
    /// <summary>
    /// One round of a candy move: the groups that sank, the hammer hits they earned, and the crates the cranes dropped into
    /// the holes (or, with the cranes empty, the crates that slid in). The first wave comes from the swap; every later one
    /// from dropped or slid crates that made a group by themselves.
    /// </summary>
    public sealed class CandyWave
    {
        public List<SunkCrate> Sunk { get; } = new List<SunkCrate>();
        public List<CandyDrop> Drops { get; } = new List<CandyDrop>();
        /// <summary>Once the cranes are empty: colour crates that slid toward the worker to close the holes.</summary>
        public List<BoxMovement> Falls { get; } = new List<BoxMovement>();
        /// <summary>Combo crates made by this wave's runs (they stay instead of sinking).</summary>
        public List<CandyMade> Made { get; } = new List<CandyMade>();
        /// <summary>Combo crates that went off in this wave.</summary>
        public List<CandyBlast> Blasts { get; } = new List<CandyBlast>();
        /// <summary>Breakable crates worn down (or broken) by this wave.</summary>
        public List<CrateChip> Chips { get; } = new List<CrateChip>();
        /// <summary>Groups (runs of three or more) in this wave.</summary>
        public int Groups { get; internal set; }
        public int Hammers { get; internal set; }
        /// <summary>Advanced hammers this wave paid (owner 2026-11-02: a long chain of crates sinking by themselves).</summary>
        public int SuperHammers { get; internal set; }
    }
}
