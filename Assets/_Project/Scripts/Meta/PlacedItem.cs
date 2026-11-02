using System;

namespace CrateBoss.Meta
{
    /// <summary>An item standing on a land (Ana Belge §12.6): item id, land, cell of its corner, turn 0–3 (front facing the road, then a quarter each).</summary>
    [Serializable]
    public class PlacedItem
    {
        public string id;
        public string land;
        public int x, y, rot;
    }
}
