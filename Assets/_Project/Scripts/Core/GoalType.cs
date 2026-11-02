using System;

namespace CrateBoss.Core
{
    /// <summary>What a level asks for (Ana Belge §5.7); several can be combined.</summary>
    [Flags]
    public enum GoalType
    {
        None = 0,
        /// <summary>Every target holds a crate (the classic goal, the default).</summary>
        Targets = 1,
        /// <summary>Every truck has been filled and has left.</summary>
        Trucks = 4,
        /// <summary>Every delivery area holds a delivery crate.</summary>
        Deliver = 32,
    }
}
