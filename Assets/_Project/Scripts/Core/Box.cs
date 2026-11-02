namespace CrateBoss.Core
{
    public sealed class Box
    {
        public int Id { get; }
        public BoxType Type { get; }
        public GridPos Position { get; set; }
        /// <summary>Match colour 1-6 (Ana Belge §5.4: three in a row sink); 0 = a plain crate.</summary>
        public int Symbol { get; internal set; } // the paint gun recolours (owner 2026-11-02)
        /// <summary>Delivery crates: index into <see cref="GameState.Deliveries"/>; -1 for every other box.</summary>
        public int Delivery { get; }
        /// <summary>A heavy delivery crate (Ana Belge §4.2 "Dh"): heavy rules plus delivery.</summary>
        public bool HeavyDelivery { get; }
        /// <summary>Delivered: sits on its own area for good and can no longer be pushed (Ana Belge §5.1).</summary>
        public bool Locked { get; set; }
        /// <summary>The delivery crate has passed its way stop (M).</summary>
        public bool ViaDone { get; set; }
        /// <summary>
        /// A fixed crate the worker breaks with hammer hits (owner 2026-11-01 candy levels): hits still needed; 0 = not breakable.
        /// </summary>
        public int Hits { get; set; }
        /// <summary>Candy levels: moves this colour crate has stood on the ground (its hits once the cranes are done).</summary>
        public int Age { get; set; }
        /// <summary>Candy levels: a combo crate (owner 2026-11-02), or None.</summary>
        public CandySpecial Special { get; set; }
        /// <summary>The colour a crate is drawn in when it differs from <see cref="Symbol"/> (a rainbow keeps the colour it came from); 0 = its symbol.</summary>
        public int Look { get; set; }

        /// <summary>A truck's cargo crate (object 'Y'): looks like a delivery crate, has no area, only it fills the truck.</summary>
        public bool IsCargo => Type == BoxType.Delivery && Delivery < 0;

        /// <summary>Moves only as the back of a chain, never pushed alone (Ana Belge §4.2).</summary>
        public bool IsHeavy => Type == BoxType.Heavy || HeavyDelivery;

        public Box(int id, BoxType type, GridPos position) : this(id, type, position, 0) { }

        public Box(int id, BoxType type, GridPos position, int symbol, int delivery = -1, bool heavyDelivery = false)
        {
            Id = id;
            Type = type;
            Position = position;
            Symbol = symbol;
            Delivery = type == BoxType.Delivery ? delivery : -1;
            HeavyDelivery = type == BoxType.Delivery && heavyDelivery;
        }

        public Box Clone() => new Box(Id, Type, Position, Symbol, Delivery, HeavyDelivery) { Locked = Locked, ViaDone = ViaDone, Hits = Hits, Age = Age, Special = Special, Look = Look };
    }
}
