using System;

namespace CrateBoss.Meta
{
    /// <summary>What the player has earned on one level (key = level id): best stars, cleared.</summary>
    [Serializable]
    public class LevelRecord
    {
        public string key;
        public int stars;
        public bool cleared;
    }
}
