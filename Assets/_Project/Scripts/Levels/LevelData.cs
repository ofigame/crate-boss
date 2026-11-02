using System;

namespace CrateBoss.Levels
{
    /// <summary>A level file as written in JSON (field names match the file).</summary>
    [Serializable]
    public class LevelData
    {
        public int id;
        public string name;
        public string[] ground;
        public string[] objects;
        /// <summary>Old single-crane block (read as cranes[0]); no drops = no crane.</summary>
        public CraneData crane;
        /// <summary>Up to 3 independent cranes, stepped in list order.</summary>
        public CraneData[] cranes;

        // Goals (Ana Belge §5.7). goal / trucks are read by LevelLoader with JsonLite (string-or-list, nested arrays).
        /// <summary>Goal names ("targets", "trucks", "deliver"); null = ["targets"].</summary>
        [NonSerialized] public string[] goal;
        /// <summary>
        /// Stored solution (U/D/L/R per move) for levels too hard for the on-device search (the hard puzzle lab): it sets the
        /// best move count and gives the hints while the player is on its way. Empty = searched on the device.
        /// </summary>
        public string solution;
        /// <summary>League of the level (1 = Öğrenci ...); 0 = not in a league (old sets).</summary>
        public int league;
        [NonSerialized] public TruckData trucks;
        /// <summary>Delivery crates and their areas (Ana Belge §6); null = one per D crate in reading order.</summary>
        [NonSerialized] public DeliveryData[] deliveries;
        /// <summary>Car park, its exits and the truck (Ana Belge §6); null = none.</summary>
        [NonSerialized] public ParkingData parking;
        /// <summary>Candy level (owner 2026-11-01): colour crates swapped by the finger, four cranes refill, hammers break 'F' crates; null = none.</summary>
        [NonSerialized] public CandyData candy;
        /// <summary>The cargo belt, cells in riding order [[x, y], ...] (owner 2026-11-02); null = none.</summary>
        [NonSerialized] public int[][] conveyor;
        /// <summary>Candy levels: belts appear by themselves along every open way to the truck (owner 2026-11-02).</summary>
        public bool belts;
        /// <summary>Rank name ("ogrenci", "stajyer" …), optional; the league number drives the game.</summary>
        public string rank;
        /// <summary>String Table key of the goal card shown at the start ("goal.deliver").</summary>
        public string taskCard;
        public LevelDesign design;

        public bool HasOldCrane => crane != null && crane.HasDrops;
        public bool HasCraneList => cranes != null && cranes.Length > 0;

        /// <summary>True when any crane is defined.</summary>
        public bool HasCrane => HasOldCrane || HasCraneList;

        /// <summary>The cranes in step order: the list, or the old single block as a one-item list (empty when none).</summary>
        public CraneData[] AllCranes => HasCraneList ? cranes : HasOldCrane ? new[] { crane } : new CraneData[0];
    }
}
