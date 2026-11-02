namespace CrateBoss.Meta
{
    /// <summary>
    /// One of the Boss's items (Ana Belge §12.3): id (also its String Table key "item.&lt;id&gt;"), rank set, size class,
    /// footprint in cells, where it may stand, colour of the placeholder model, and whether it can be turned.
    /// </summary>
    public sealed class ItemDef
    {
        public string Id;
        public int Rank;
        /// <summary>small, medium, big, special (the promotion item), decor (can be taken again).</summary>
        public string Size;
        public int W, D;
        /// <summary>building, vehicle, garden, ground (pool, court…), water (boats), shore (piers), path.</summary>
        public string Kind;
        public string Color;
        public bool Rotatable = true;

        public bool Big => Size == "big" || Size == "special";
        public bool Decor => Size == "decor";
        public string NameKey => "item." + Id;
    }
}
