namespace CrateBoss.Meta
{
    /// <summary>
    /// One of the Boss's lands (Ana Belge §12.5): its size in cells, where it lies in the home city (cell offset), the rank
    /// that opens it, and its water (lake, sea). Row 0 is the far side; the front row (D − 1) runs along the city road.
    /// </summary>
    public sealed class LandDef
    {
        public string Id;
        public int W, D;
        public int OffsetX, OffsetY;
        public int Rank;
        /// <summary>Water cells: a pond in the middle (lake) or the back rows (shore).</summary>
        public string Water = "none";

        public bool IsWater(int x, int y)
        {
            switch (Water)
            {
                case "pond": return x >= W / 2 - 2 && x < W / 2 + 2 && y >= D / 2 - 2 && y < D / 2 + 2;
                case "sea": return y < 3;
                default: return false;
            }
        }

        public bool InBounds(int x, int y) => x >= 0 && y >= 0 && x < W && y < D;
        public string NameKey => "land." + Id;
    }
}
