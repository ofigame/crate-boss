namespace CrateBoss.Core
{
    /// <summary>One delivery of a level (Ana Belge §5.1, §6 "deliveries"): its area and whether it must pass the way stop first. Never changes.</summary>
    public sealed class DeliveryDefinition
    {
        /// <summary>Area number 1–3 (ground B = 1, or the digits 1, 2, 3).</summary>
        public int Area { get; }
        /// <summary>The crate must enter an M cell before its area counts.</summary>
        public bool NeedsVia { get; }
        /// <summary>A heavy delivery crate ("Dh").</summary>
        public bool Heavy { get; }

        public DeliveryDefinition(int area, bool needsVia, bool heavy)
        {
            Area = area;
            NeedsVia = needsVia;
            Heavy = heavy;
        }
    }
}
