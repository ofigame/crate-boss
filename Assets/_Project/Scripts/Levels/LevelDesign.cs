using System;

namespace CrateBoss.Levels
{
    /// <summary>Design record of a level (Ana Belge §6 "design", §10): the one-sentence idea, its patterns and difficulty target.</summary>
    [Serializable]
    public class LevelDesign
    {
        public string idea;
        /// <summary>Pattern codes from the library (Ana Belge §10.1): "V-renk", "P-düz" …</summary>
        public string[] patterns;
        /// <summary>Difficulty score the slot asks for (Ana Belge §9, §10.5).</summary>
        public int targetDifficulty;
        /// <summary>Crane levels: how many moves may be wasted (Ana Belge §10.3).</summary>
        public int targetTolerance;
        /// <summary>Older records: mechanics used and the tier.</summary>
        public string[] mechanics;
        public string tier;
    }
}
