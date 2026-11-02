using System;

namespace CrateBoss.Levels
{
    /// <summary>One crane of a level file: one drop every <c>every</c> successful moves.</summary>
    [Serializable]
    public class CraneData
    {
        /// <summary>"A", "B" or "C"; empty = by list position.</summary>
        public string id;
        /// <summary>"left", "right", "top" or "bottom" (view only); empty = by list position.</summary>
        public string side;
        public int every;
        public CraneDropData[] drops;

        public bool HasDrops => drops != null && drops.Length > 0;
    }
}
