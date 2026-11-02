using System.Collections.Generic;
using System.Text;

namespace CrateBoss.Core
{
    /// <summary>The whole board: ground tiles, player, boxes and the move count. Small enough to copy for undo.</summary>
    public sealed class GameState
    {
        // Shared between a state and its clones until one of them changes a tile (copy on write: the solver clones a lot).
        private TileType[] tiles;
        private bool tilesShared;
        private int filledHoles;
        // Cells of the tiles the rules look at every move, found once and shared by clones (reset when such a tile changes).
        private int[] targetCells, arrivalCells;
        private int nextBoxId;

        public int Width { get; }
        public int Height { get; }
        public GridPos PlayerPos { get; set; }
        public List<Box> Boxes { get; } = new List<Box>();
        public int MoveCount { get; set; }
        /// <summary>
        /// Moves the player is scored on (owner 2026-11-01 "boş alanda hareketi hamle sayma, kasa itmeler sayılsın"): pushes
        /// and cars sent; plain walks do not count. <see cref="MoveCount"/> keeps every move for the machines.
        /// </summary>
        public int PushCount { get; set; }

        private static readonly CraneDefinition[] NoCranes = new CraneDefinition[0];
        private static readonly CraneState[] NoCraneStates = new CraneState[0];

        /// <summary>The level's cranes in step order (shared, never changed); empty when none.</summary>
        public IReadOnlyList<CraneDefinition> Cranes { get; private set; } = NoCranes;
        /// <summary>Each crane's place during play, same order as <see cref="Cranes"/>.</summary>
        public CraneState[] CraneStates { get; private set; } = NoCraneStates;

        /// <summary>The first crane (shared, never changed), or null.</summary>
        public CraneDefinition Crane => Cranes.Count > 0 ? Cranes[0] : null;
        /// <summary>Index of the next drop in the first crane's list.</summary>
        public int CraneIndex
        {
            get => CraneStates.Length > 0 ? CraneStates[0].Index : 0;
            set { if (CraneStates.Length > 0) CraneStates[0].Index = value; }
        }
        /// <summary>Successful moves left until the first crane's next drop (0 = due, waiting for a free cell).</summary>
        public int CraneCounter
        {
            get => CraneStates.Length > 0 ? CraneStates[0].Counter : 0;
            set { if (CraneStates.Length > 0) CraneStates[0].Counter = value; }
        }

        public bool HasActiveCrane => IsCraneActive(0);
        public CraneDrop NextDrop => NextDropOf(0);

        /// <summary>True while crane <paramref name="i"/> still has drops left.</summary>
        public bool IsCraneActive(int i) => i < Cranes.Count && CraneStates[i].Index < Cranes[i].Drops.Count;
        public CraneDrop NextDropOf(int i) => IsCraneActive(i) ? Cranes[i].Drops[CraneStates[i].Index] : null;

        /// <summary>True while any crane has drops left (time still matters).</summary>
        public bool HasActiveMachine => HasAnyActiveCrane;

        /// <summary>True while any crane still has drops left.</summary>
        public bool HasAnyActiveCrane
        {
            get
            {
                for (int i = 0; i < Cranes.Count; i++)
                    if (IsCraneActive(i)) return true;
                return false;
            }
        }

        public GameState(int width, int height)
        {
            Width = width;
            Height = height;
            tiles = new TileType[width * height];
            for (int i = 0; i < tiles.Length; i++) tiles[i] = TileType.Void;
        }

        // Clone: the tiles come from the original.
        private GameState(int width, int height, TileType[] shared)
        {
            Width = width;
            Height = height;
            tiles = shared;
            tilesShared = true;
        }

        public bool InBounds(GridPos p) => p.X >= 0 && p.Y >= 0 && p.X < Width && p.Y < Height;

        /// <summary>Outside the grid counts as the edge of the world (void).</summary>
        public TileType GetTile(GridPos p) => InBounds(p) ? tiles[p.Y * Width + p.X] : TileType.Void;

        public void SetTile(GridPos p, TileType tile)
        {
            int i = p.Y * Width + p.X;
            if (tiles[i] == tile) return;
            if (tilesShared)
            {
                tiles = (TileType[])tiles.Clone();
                tilesShared = false;
            }
            if (IsListed(tiles[i]) || IsListed(tile)) targetCells = arrivalCells = null;
            if (tiles[i] == TileType.FilledHole) filledHoles--;
            if (tile == TileType.FilledHole) filledHoles++;
            tiles[i] = tile;
        }

        /// <summary>Grid indices of every target tile, in reading order.</summary>
        public int[] TargetCells => targetCells ??= Collect(TileType.Target);

        /// <summary>Grid indices of every delivery area tile, in reading order.</summary>
        public int[] ArrivalCells => arrivalCells ??= Collect(TileType.Arrival);

        private static bool IsListed(TileType t) => t == TileType.Target || t == TileType.Arrival;

        private int[] Collect(TileType match)
        {
            int n = 0;
            for (int i = 0; i < tiles.Length; i++) if (tiles[i] == match) n++;
            var cells = new int[n];
            n = 0;
            for (int i = 0; i < tiles.Length; i++) if (tiles[i] == match) cells[n++] = i;
            return cells;
        }

        public GridPos CellAt(int index) => new GridPos(index % Width, index / Width);

        /// <summary>Attaches a crane and resets it to its start (counter = every).</summary>
        public void SetCrane(CraneDefinition crane) => SetCranes(crane == null ? null : new[] { crane });

        /// <summary>Attaches the cranes (null or empty = none) and resets each to its start (counter = every).</summary>
        public void SetCranes(IReadOnlyList<CraneDefinition> cranes)
        {
            if (cranes == null || cranes.Count == 0)
            {
                Cranes = NoCranes;
                CraneStates = NoCraneStates;
                return;
            }
            Cranes = cranes;
            CraneStates = new CraneState[cranes.Count];
            for (int i = 0; i < cranes.Count; i++) CraneStates[i] = new CraneState(0, cranes[i].Every);
        }

        public Box AddBox(BoxType type, GridPos pos) => AddBox(type, pos, 0);

        /// <param name="delivery">Delivery crates: index into <see cref="Deliveries"/> (its "heavy" flag comes from there).</param>
        public Box AddBox(BoxType type, GridPos pos, int symbol, int delivery = -1)
        {
            bool heavy = type == BoxType.Delivery && delivery >= 0 && delivery < Deliveries.Count && Deliveries[delivery].Heavy;
            var box = new Box(nextBoxId++, type, pos, symbol, delivery, heavy);
            Boxes.Add(box);
            if (symbol > 0) HasSymbols = true;
            if (box.IsCargo) HasCargo = true;
            return box;
        }

        // ---------- candy levels (owner 2026-11-01) ----------

        /// <summary>Cells the cranes refill (where the colour crates stood at the start); null = not a candy level.</summary>
        public bool[] CandyArea { get; private set; }
        /// <summary>Colour crates the cranes may still drop this level.</summary>
        public int CandyDropsLeft { get; set; }
        /// <summary>The colours the cranes drop.</summary>
        public int[] CandyColors { get; private set; }
        /// <summary>The random draw of the next dropped colour (kept in the state, so undo and replays drop the same).</summary>
        public uint CandyRng { get; set; }
        /// <summary>Hammer hits earned by sunk groups, spent on breakable crates.</summary>
        public int Hammers { get; set; }
        /// <summary>
        /// Advanced hammers (owner 2026-11-02): earned by long chains of crates sinking by themselves; one hits a breakable crate
        /// anywhere with a tap, no walking.
        /// </summary>
        public int SuperHammers { get; set; }
        /// <summary>Paint guns in hand (owner 2026-11-02: two groups sinking one after another in an easy level give one).</summary>
        public int PaintGuns { get; set; }
        /// <summary>The level hands out paint guns (the easy candy levels).</summary>
        public bool CandyPaint { get; set; }
        /// <summary>Chain waves that sink by themselves before the next ones pay advanced hammers (3, later 4, 5, 6).</summary>
        public int CandySuperAfter { get; set; } = 3;
        /// <summary>The candy part is over (no crane drops, no swap): the colour crates left stand as breakable crates.</summary>
        public bool CandyDone { get; set; }
        /// <summary>Most hits a left-over colour crate can need (0 = no limit): big late levels keep their ages in bounds.</summary>
        public int CandyHitCap { get; set; }
        /// <summary>Colour crates are swapped by the finger, never pushed; cranes refill; groups earn hammers.</summary>
        public bool IsCandyLevel => CandyArea != null;
        /// <summary>
        /// Per cell, the way a colour crate falls (0 up, 1 down, 2 left, 3 right, -1 none): toward the worker's open floor by the
        /// shortest way (owner 2026-11-02: candy may frame the yard and falls inward). Found once (CandySystem.Fall), shared.
        /// </summary>
        public sbyte[] CandyFall { get; set; }
        /// <summary>Per cell the way a colour crate falls (owner 2026-11-02, later: always down); found once, shared.</summary>
        public sbyte[] CandyDown { get; set; }

        public void SetCandy(bool[] area, int drops, int[] colors, uint seed)
        {
            CandyArea = area;
            CandyDropsLeft = drops;
            CandyColors = colors;
            CandyRng = seed == 0 ? 2463534242u : seed;
        }

        /// <summary>True once the level has had a match crate (the match rule only runs then).</summary>
        public bool HasSymbols { get; private set; }

        /// <summary>The level has cargo crates (owner 2026-11-01): then only they fill the truck, other crates are in the way.</summary>
        public bool HasCargo { get; private set; }

        // ---------- goals (Ana Belge §5.7) ----------

        /// <summary>What the level asks for; Targets by default.</summary>
        public GoalType Goal { get; set; } = GoalType.Targets;
        /// <summary>
        /// Every truck loading area with its route (owner 2026-11-02: trucks may come from up to four sides), shared and never
        /// changed; each has its own trucks still to come and its own parked flag.
        /// </summary>
        public TruckBay[] Bays { get; private set; } = NoBays;
        private static readonly TruckBay[] NoBays = new TruckBay[0];
        private int[] bayTrucks = NoInts;
        private bool[] bayParked = NoFlags;
        private static readonly int[] NoInts = new int[0];
        private static readonly bool[] NoFlags = new bool[0];
        /// <summary>The first truck loading area and route, or null.</summary>
        public TruckBay Truck => Bays.Length > 0 ? Bays[0] : null;
        /// <summary>The cargo belt's cells in riding order (owner 2026-11-02), empty when the level has none. Shared, never changed.</summary>
        public IReadOnlyList<GridPos> Conveyor { get; private set; } = NoCells;
        private static readonly GridPos[] NoCells = new GridPos[0];
        public void SetConveyor(IReadOnlyList<GridPos> cells) => Conveyor = cells ?? NoCells;
        /// <summary>Belts lie by themselves along every open way from under the candy to the truck (owner 2026-11-02).</summary>
        public bool AutoBelts { get; set; }
        /// <summary>Trucks still to fill, all loading areas together.</summary>
        public int TrucksLeft
        {
            get
            {
                int n = 0;
                foreach (var t in bayTrucks) n += t;
                return n;
            }
        }
        /// <summary>A truck stands in the first loading area now (its route cells are walls, its cargo cells take crates).</summary>
        public bool TruckParked => bayParked.Length > 0 && bayParked[0];

        /// <summary>Trucks still to fill at loading area <paramref name="bay"/>.</summary>
        public int TrucksAt(int bay) => bayTrucks[bay];
        /// <summary>A truck stands in loading area <paramref name="bay"/> now.</summary>
        public bool ParkedAt(int bay) => bayParked[bay];
        public void SetBay(int bay, int trucksLeft, bool parked)
        {
            bayTrucks[bay] = trucksLeft;
            bayParked[bay] = parked;
        }

        public int Index(GridPos p) => p.Y * Width + p.X;

        public void SetTruck(TruckBay bay) => SetTrucks(bay == null ? NoBays : new[] { bay });

        public void SetTrucks(TruckBay[] bays)
        {
            Bays = bays ?? NoBays;
            bayTrucks = new int[Bays.Length];
            bayParked = new bool[Bays.Length];
            for (int i = 0; i < Bays.Length; i++)
            {
                bayTrucks[i] = Bays[i].Count;
                bayParked[i] = !Bays[i].HasRoute; // a truck without a route is there from the start
            }
        }

        /// <summary>The loading area this cell belongs to, or -1.</summary>
        public int BayAt(GridPos p)
        {
            for (int i = 0; i < Bays.Length; i++) if (Bays[i].Contains(p)) return i;
            return -1;
        }

        /// <summary>A parked truck stands on this cell (a route cell, not its open cargo area).</summary>
        public bool IsTruckWall(GridPos p)
        {
            for (int i = 0; i < Bays.Length; i++)
                if (bayParked[i] && Bays[i].OnRoute(p) && !Bays[i].Contains(p)) return true;
            return false;
        }

        /// <summary>The cell is a truck's way in (route, not loading area) of any loading area.</summary>
        public bool OnTruckRoute(GridPos p)
        {
            foreach (var b in Bays) if (b.OnRoute(p) && !b.Contains(p)) return true;
            return false;
        }

        // ---------- deliveries (Ana Belge §5.1) ----------

        private static readonly DeliveryDefinition[] NoDeliveries = new DeliveryDefinition[0];

        /// <summary>The level's deliveries (shared, never changed); empty when none.</summary>
        public IReadOnlyList<DeliveryDefinition> Deliveries { get; private set; } = NoDeliveries;
        /// <summary>Per cell: the area number (1–3) of a delivery area cell, 0 elsewhere (shared); null when the level has none.</summary>
        public int[] AreaOf { get; private set; }

        public void SetDeliveries(IReadOnlyList<DeliveryDefinition> deliveries, int[] areaOf)
        {
            Deliveries = deliveries ?? NoDeliveries;
            AreaOf = areaOf;
        }

        public int AreaAt(GridPos p) => AreaOf != null && InBounds(p) ? AreaOf[Index(p)] : 0;

        // ---------- car park (Ana Belge §5.5) ----------

        /// <summary>The car park (shared, never changed), or null.</summary>
        public ParkingLot Parking { get; private set; }
        /// <summary>Bit i set: car i has left the park.</summary>
        public ulong CarsGone { get; set; }

        public void SetParking(ParkingLot parking)
        {
            Parking = parking;
            CarsGone = 0;
        }

        public bool CarPresent(int i) => Parking != null && i < Parking.Cars.Count && (CarsGone & (1UL << i)) == 0;

        /// <summary>Index of the parked car on this cell, or -1.</summary>
        public int CarAt(GridPos p)
        {
            if (Parking == null) return -1;
            for (int i = 0; i < Parking.Cars.Count; i++)
                if ((CarsGone & (1UL << i)) == 0 && Parking.Cars[i].Covers(p)) return i;
            return -1;
        }

        public void RemoveBox(Box box) => Boxes.Remove(box);

        public Box GetBoxAt(GridPos p)
        {
            foreach (var b in Boxes)
                if (b.Position == p) return b;
            return null;
        }

        public Box GetBoxById(int id)
        {
            foreach (var b in Boxes)
                if (b.Id == id) return b;
            return null;
        }

        /// <summary>
        /// Won when the level's goals are done (see <see cref="WinChecker"/>). The classic goal: every target has a
        /// box on it (any kind but a crane container or a match crate); a level with no targets and no other goal is never won.
        /// </summary>
        public bool IsSolved() => WinChecker.IsWon(this);

        public GameState Clone()
        {
            tilesShared = true;
            var copy = new GameState(Width, Height, tiles)
            {
                filledHoles = filledHoles,
                targetCells = targetCells,
                arrivalCells = arrivalCells,
                PlayerPos = PlayerPos,
                MoveCount = MoveCount,
                PushCount = PushCount,
                nextBoxId = nextBoxId,
                Cranes = Cranes,
                CraneStates = CloneCraneStates(),
                Goal = Goal,
                Bays = Bays,
                bayTrucks = Bays.Length > 0 ? (int[])bayTrucks.Clone() : bayTrucks,
                bayParked = Bays.Length > 0 ? (bool[])bayParked.Clone() : bayParked,
                Conveyor = Conveyor,
                AutoBelts = AutoBelts,
                HasSymbols = HasSymbols,
                HasCargo = HasCargo,
                Deliveries = Deliveries,
                AreaOf = AreaOf,
                Parking = Parking,
                CarsGone = CarsGone,
                CandyArea = CandyArea,
                CandyFall = CandyFall,
                CandyDown = CandyDown,
                CandyDropsLeft = CandyDropsLeft,
                CandyColors = CandyColors,
                CandyRng = CandyRng,
                Hammers = Hammers,
                SuperHammers = SuperHammers,
                PaintGuns = PaintGuns,
                CandyPaint = CandyPaint,
                CandySuperAfter = CandySuperAfter,
                CandyDone = CandyDone,
                CandyHitCap = CandyHitCap,
            };
            foreach (var b in Boxes) copy.Boxes.Add(b.Clone());
            return copy;
        }

        /// <summary>
        /// Position key without the move count: player, boxes (sorted, with type), filled holes and the cranes' places in their lists.
        /// Two states with the same key play exactly the same from here on.
        /// </summary>
        public string GetKey()
        {
            var cells = new List<int>(Boxes.Count);
            foreach (var b in Boxes) cells.Add(BoxCode(b));
            cells.Sort();
            var sb = new StringBuilder(32);
            sb.Append(PlayerPos.Y * Width + PlayerPos.X).Append('|');
            foreach (var c in cells) sb.Append(c).Append(',');
            sb.Append('|');
            for (int i = 0; i < tiles.Length; i++)
                if (tiles[i] == TileType.FilledHole) sb.Append(i).Append(',');
            foreach (var c in CraneStates) sb.Append('|').Append(c.Index).Append(':').Append(c.Counter);
            for (int i = 0; i < Bays.Length; i++) sb.Append("|t").Append(bayTrucks[i]).Append(bayParked[i] ? 'p' : '-');
            if (Parking != null) sb.Append("|c").Append(CarsGone);
            return sb.ToString();
        }

        /// <summary>
        /// The same position as <see cref="GetKey"/> folded into 128 bits without building a string (the solver's
        /// "seen" set). Two hash streams make a collision vanishingly unlikely for the few hundred thousand states searched.
        /// </summary>
        public StateHash GetKeyHash()
        {
            ulong a = 14695981039346656037UL, b = 0x9E3779B97F4A7C15UL;
            void Mix(int v)
            {
                unchecked
                {
                    a = (a ^ (uint)v) * 1099511628211UL;
                    b = (b ^ (ulong)(v + 0x632BE5AB)) * 0xBF58476D1CE4E5B9UL;
                    b ^= b >> 31;
                }
            }
            Mix(PlayerPos.Y * Width + PlayerPos.X);
            Mix(-1);
            // Boxes in a fixed order (sorted by cell), as the string key does.
            int n = Boxes.Count;
            System.Span<int> cells = n <= 32 ? stackalloc int[n] : new int[n];
            for (int i = 0; i < n; i++)
            {
                // Insertion sort: a handful of boxes, no allocation.
                int code = BoxCode(Boxes[i]), j = i - 1;
                while (j >= 0 && cells[j] > code) { cells[j + 1] = cells[j]; j--; }
                cells[j + 1] = code;
            }
            for (int i = 0; i < n; i++) Mix(cells[i]);
            Mix(-2);
            if (filledHoles > 0)
                for (int i = 0; i < tiles.Length; i++)
                    if (tiles[i] == TileType.FilledHole) Mix(i);
            foreach (var c in CraneStates) { Mix(-3); Mix(c.Index); Mix(c.Counter); }
            for (int i = 0; i < Bays.Length; i++) { Mix(-6); Mix(bayTrucks[i] * 2 + (bayParked[i] ? 1 : 0)); }
            if (Parking != null) { Mix(-7); Mix((int)CarsGone); Mix((int)(CarsGone >> 32)); }
            return new StateHash(a, b);
        }

        // Cell, type (3 bits), colour (3 bits), delivery (2 bits), locked and way stop passed of a box, as one number for the keys.
        private int BoxCode(Box b) =>
            (((((((Index(b.Position) << 3) | (int)b.Type) << 3) | b.Symbol) << 2) | (b.Delivery + 1)) << 2) | (b.Locked ? 2 : 0) | (b.ViaDone ? 1 : 0);

        private CraneState[] CloneCraneStates()
        {
            if (CraneStates.Length == 0) return NoCraneStates;
            var copy = new CraneState[CraneStates.Length];
            for (int i = 0; i < copy.Length; i++) copy[i] = CraneStates[i].Clone();
            return copy;
        }

        /// <summary>Exact equality including box ids, move count and cranes (used to check undo).</summary>
        public bool SameAs(GameState other)
        {
            if (other == null || other.Width != Width || other.Height != Height) return false;
            if (other.PlayerPos != PlayerPos || other.MoveCount != MoveCount || other.Boxes.Count != Boxes.Count) return false;
            if (other.CraneStates.Length != CraneStates.Length) return false;
            for (int i = 0; i < CraneStates.Length; i++)
                if (other.CraneStates[i].Index != CraneStates[i].Index || other.CraneStates[i].Counter != CraneStates[i].Counter) return false;
            for (int i = 0; i < tiles.Length; i++)
                if (tiles[i] != other.tiles[i]) return false;
            foreach (var b in Boxes)
            {
                var o = other.GetBoxById(b.Id);
                if (o == null || o.Type != b.Type || o.Position != b.Position || o.Symbol != b.Symbol || o.Delivery != b.Delivery
                    || o.Locked != b.Locked || o.ViaDone != b.ViaDone) return false;
            }
            if (other.Bays.Length != Bays.Length) return false;
            for (int i = 0; i < Bays.Length; i++)
                if (other.bayTrucks[i] != bayTrucks[i] || other.bayParked[i] != bayParked[i]) return false;
            return other.CarsGone == CarsGone;
        }
    }
}
