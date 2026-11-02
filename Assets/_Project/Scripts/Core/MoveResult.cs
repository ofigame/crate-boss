using System.Collections.Generic;

namespace CrateBoss.Core
{
    /// <summary>What happened on one move attempt. The view layer only reads this.</summary>
    public sealed class MoveResult
    {
        private static readonly CraneDropEvent[] NoDrops = new CraneDropEvent[0];
        private static readonly SunkCrate[] NoSunk = new SunkCrate[0];
        private static readonly DeliveryEvent[] NoDeliveries = new DeliveryEvent[0];

        public bool Success { get; internal set; }
        /// <summary>What the player did: a walk / push, or sending a car.</summary>
        public PlayerAction Action { get; }
        public Direction Direction => Action.Direction;
        /// <summary>The car that was sent (left, or bumped and came back), or null.</summary>
        public CarEvent Car { get; internal set; }
        /// <summary>Delivery crates that reached an area this move (step 3).</summary>
        public IReadOnlyList<DeliveryEvent> Deliveries { get; internal set; } = NoDeliveries;
        /// <summary>A crane's delivery crate that landed on an area (step 8).</summary>
        public IReadOnlyList<DeliveryEvent> CraneDeliveries { get; internal set; } = NoDeliveries;
        public GridPos PlayerFrom { get; }
        public GridPos PlayerTo { get; internal set; }
        public List<BoxMovement> BoxMoves { get; } = new List<BoxMovement>();
        // Rarely used: made on first use (the solver makes a MoveResult for every searched move).
        public List<GridPos> FilledHoles => filledHoles ??= new List<GridPos>();
        private List<GridPos> filledHoles;
        public bool IsWon { get; internal set; }
        /// <summary>Every crane drop at the end of this move, in crane order (empty when none).</summary>
        public IReadOnlyList<CraneDropEvent> CraneDrops { get; internal set; } = NoDrops;
        /// <summary>The first crane drop at the end of this move, or null.</summary>
        public CraneDropEvent CraneDrop => CraneDrops.Count > 0 ? CraneDrops[0] : null;
        /// <summary>The truck left full and/or the next one backed in (step 5), or null.</summary>
        public TruckEvent TruckLeft { get; internal set; }
        /// <summary>Every truck event this move, one per loading area that changed (rarely more than one).</summary>
        public List<TruckEvent> TruckEvents => truckEvents ??= new List<TruckEvent>();
        private List<TruckEvent> truckEvents;
        /// <summary>Match crates that sank after the push, in reading order.</summary>
        public IReadOnlyList<SunkCrate> Sunk { get; internal set; } = NoSunk;
        /// <summary>Match crates that sank because a crane's colour crate finished a run (step 8).</summary>
        public IReadOnlyList<SunkCrate> CraneSunk { get; internal set; } = NoSunk;
        /// <summary>CB Coin bonus for runs of 4 or 5+ this move (Ana Belge §5.4).</summary>
        public int MatchBonus { get; internal set; }

        // ---------- candy levels (owner 2026-11-01) ----------

        /// <summary>The swapped cells (a candy swap).</summary>
        public GridPos SwapFrom { get; internal set; }
        public GridPos SwapTo { get; internal set; }
        /// <summary>The rounds of a candy swap: sunk groups, hammers earned, crates dropped.</summary>
        public List<CandyWave> CandyWaves => candyWaves ??= new List<CandyWave>();
        private List<CandyWave> candyWaves;
        public bool HasCandyWaves => candyWaves != null && candyWaves.Count > 0;
        /// <summary>No swap was left: the colour crates were dealt out again.</summary>
        public bool Shuffled { get; internal set; }
        /// <summary>The worker hit this breakable crate with the hammer (its id), or -1.</summary>
        public int HitBox { get; internal set; } = -1;
        /// <summary>The paint gun was used: the colour, and the crates it went onto (owner 2026-11-02).</summary>
        public int PaintColour { get; internal set; }
        public List<int> Painted => painted ??= new List<int>();
        private List<int> painted;
        /// <summary>This move earned a paint gun.</summary>
        public bool PaintEarned { get; internal set; }
        /// <summary>The hit came from afar (auto hammer or advanced hammer): the worker did not move.</summary>
        public bool HitFromAfar { get; internal set; }
        /// <summary>The hit broke it (it is gone).</summary>
        public bool Broke { get; internal set; }
        /// <summary>The worker walked into a breakable crate with no hammer hits left.</summary>
        public bool NoHammer { get; internal set; }
        /// <summary>The candy part ended this move: the colour crates left became breakable crates.</summary>
        public bool CandyFinished { get; internal set; }
        /// <summary>One-cell rides of cargo crates on the belt this move, in order.</summary>
        public List<BoxMovement> ConveyorMoves { get; internal set; }

        public MoveResult(PlayerAction action, GridPos playerFrom)
        {
            Action = action;
            PlayerFrom = playerFrom;
            PlayerTo = playerFrom;
        }
    }
}
