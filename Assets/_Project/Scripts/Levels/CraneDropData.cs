using System;

namespace CrateBoss.Levels
{
    /// <summary>
    /// One crane drop in a level file (Ana Belge §5.3, §6): x = column, y = row (row 0 at the top), type = K (container), B, I, H,
    /// 1–6 (colour crate) or D (delivery crate, with <see cref="deliveryIndex"/>).
    /// </summary>
    [Serializable]
    public class CraneDropData
    {
        public int x;
        public int y;
        public string type;
        /// <summary>Type D: which entry of "deliveries" the crate belongs to (0-based).</summary>
        public int deliveryIndex;
    }
}
