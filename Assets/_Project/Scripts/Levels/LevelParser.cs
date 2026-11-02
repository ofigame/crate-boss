using System.Collections.Generic;
using CrateBoss.Core;

namespace CrateBoss.Levels
{
    /// <summary>
    /// Turns a level file (Ana Belge §6) into a GameState and checks it. Plain C#, no Unity calls. Ground letters (§4.3):
    /// # . T o _, B (delivery area 1) or 1–3 (delivery areas), A (start mark), M (way stop). Objects (§4.2): P B I H D Y 1–6,
    /// F (breakable fixed crate, candy levels).
    /// </summary>
    public sealed class LevelParser
    {
        public GameState Parse(LevelData data)
        {
            if (data == null) throw new LevelFormatException("Level data is empty.");
            string label = $"Level {data.id} '{data.name}'";
            var ground = data.ground;
            var objects = data.objects;
            if (ground == null || ground.Length == 0) throw new LevelFormatException($"{label}: 'ground' is missing or empty.");
            if (objects == null || objects.Length != ground.Length)
                throw new LevelFormatException($"{label}: 'objects' has {objects?.Length ?? 0} rows, 'ground' has {ground.Length}.");

            int height = ground.Length;
            int width = ground[0].Length;
            if (width == 0) throw new LevelFormatException($"{label}: row 0 is empty.");
            for (int y = 0; y < height; y++)
            {
                if (ground[y] == null || ground[y].Length != width)
                    throw new LevelFormatException($"{label}: ground row {y} has length {ground[y]?.Length ?? 0}, expected {width}.");
                if (objects[y] == null || objects[y].Length != width)
                    throw new LevelFormatException($"{label}: objects row {y} has length {objects[y]?.Length ?? 0}, expected {width}.");
            }

            var state = new GameState(width, height);
            int[] areaOf = null;
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                var p = new GridPos(x, y);
                char c = ground[y][x];
                state.SetTile(p, ParseTile(c, label, x, y));
                int area = c == 'B' ? 1 : c >= '1' && c <= '3' ? c - '0' : 0;
                if (area > 0) (areaOf ??= new int[width * height])[y * width + x] = area;
            }

            // Delivery crates first: the boxes need their delivery (and its "heavy" flag) when they are made.
            var deliveryAt = ParseDeliveries(data, state, areaOf, label);

            int players = 0;
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                var p = new GridPos(x, y);
                char c = objects[y][x];
                if (c == '.') continue;
                var under = state.GetTile(p);
                if (!GoalRules.IsWalkableTile(under))
                    throw new LevelFormatException($"{label}: object '{c}' at column {x}, row {y} is not on floor.");
                switch (c)
                {
                    case 'P': players++; state.PlayerPos = p; break;
                    case 'B': state.AddBox(BoxType.Normal, p); break;
                    case 'I': state.AddBox(BoxType.Ice, p); break;
                    case 'H': state.AddBox(BoxType.Heavy, p); break;
                    case 'D': state.AddBox(BoxType.Delivery, p, 0, deliveryAt[p]); break;
                    case 'Y': state.AddBox(BoxType.Delivery, p, 0, -1); break; // truck cargo
                    case 'F': state.AddBox(BoxType.Container, p).Hits = data.candy?.HitsAt(x, y) ?? 1; break; // breakable (candy levels)
                    case '1': case '2': case '3': case '4': case '5': case '6':
                        state.AddBox(BoxType.Normal, p, c - '0');
                        break;
                    case 'K': throw new LevelFormatException($"{label}: 'K' at column {x}, row {y}: containers only come from the crane, use a wall.");
                    default: throw new LevelFormatException($"{label}: unknown object '{c}' at column {x}, row {y}.");
                }
            }
            if (players != 1) throw new LevelFormatException($"{label}: needs exactly 1 player 'P', found {players}.");
            if (data.candy != null) SetCandy(data.candy, state, label);
            if (data.conveyor != null) SetConveyor(data.conveyor, state, label);
            state.AutoBelts = data.belts;
            if (data.HasCrane) state.SetCranes(ParseCranes(data, state, label));
            CheckDeliverySources(state, label);
            ParseParking(data, state, label);
            ParseGoals(data, state, label);
            return state;
        }

        /// <summary>The cargo belt: floor cells, each next to the one before (owner 2026-11-02).</summary>
        private static void SetConveyor(int[][] cells, GameState state, string label)
        {
            var belt = new List<GridPos>(cells.Length);
            foreach (var c in cells)
            {
                var p = new GridPos(c[0], c[1]);
                if (!state.InBounds(p) || !GoalRules.IsWalkableTile(state.GetTile(p))) throw new LevelFormatException($"{label}: conveyor cell {p} is not floor.");
                if (belt.Count > 0 && belt[belt.Count - 1].ManhattanTo(p) != 1) throw new LevelFormatException($"{label}: conveyor cell {p} is not next to the one before.");
                belt.Add(p);
            }
            state.SetConveyor(belt);
        }

        // ---------- candy levels (owner 2026-11-01) ----------

        /// <summary>The candy area is where the colour crates stand at the start; colours default to the ones on the board.</summary>
        private static void SetCandy(CandyData candy, GameState state, string label)
        {
            var area = new bool[state.Width * state.Height];
            var seen = new List<int>();
            foreach (var b in state.Boxes)
            {
                if (b.Symbol <= 0) continue;
                area[state.Index(b.Position)] = true;
                if (!seen.Contains(b.Symbol)) seen.Add(b.Symbol);
            }
            var colors = candy.colors != null && candy.colors.Length > 0 ? candy.colors : seen.ToArray();
            if (colors.Length == 0) throw new LevelFormatException($"{label}: a candy level needs colour crates.");
            foreach (var c in colors)
                if (c < 1 || c > 6) throw new LevelFormatException($"{label}: candy colour {c} must be 1–6.");
            state.SetCandy(area, System.Math.Max(0, candy.drops), colors, (uint)candy.seed);
            state.CandyHitCap = System.Math.Max(0, candy.hitCap);
            state.CandySuperAfter = System.Math.Max(1, candy.superAfter);
            state.CandyPaint = candy.paint;
            state.PuzzlePar = System.Math.Max(0, candy.puzzlePar);
            state.PuzzleReward = System.Math.Max(0, candy.puzzleReward);
            state.GatesOpen = state.PuzzlePar == 0; // the barriers wait for the puzzle
        }

        // ---------- deliveries (Ana Belge §5.1, §6) ----------

        /// <summary>Makes the delivery list; returns the delivery index of every D crate in the objects layer.</summary>
        private static Dictionary<GridPos, int> ParseDeliveries(LevelData data, GameState state, int[] areaOf, string label)
        {
            var crates = new List<GridPos>();
            for (int y = 0; y < state.Height; y++)
            for (int x = 0; x < state.Width; x++)
                if (data.objects[y][x] == 'D') crates.Add(new GridPos(x, y));
            var at = new Dictionary<GridPos, int>();
            var defs = new List<DeliveryDefinition>();
            bool HasArea(int a)
            {
                if (areaOf == null) return false;
                foreach (int v in areaOf) if (v == a) return true;
                return false;
            }
            bool hasVia = false;
            for (int y = 0; y < state.Height && !hasVia; y++)
                for (int x = 0; x < state.Width; x++)
                    if (state.GetTile(new GridPos(x, y)) == TileType.Via) { hasVia = true; break; }

            if (data.deliveries != null && data.deliveries.Length > 0)
            {
                if (data.deliveries.Length > 3) throw new LevelFormatException($"{label}: at most 3 deliveries, found {data.deliveries.Length}.");
                for (int i = 0; i < data.deliveries.Length; i++)
                {
                    var d = data.deliveries[i];
                    int area = ParseArea(d.area, label, i);
                    if (!HasArea(area)) throw new LevelFormatException($"{label}: delivery {i} goes to area {d.area}, which is not on the ground.");
                    bool via = !string.IsNullOrEmpty(d.via);
                    if (via && (d.via != "M" || !hasVia)) throw new LevelFormatException($"{label}: delivery {i} way stop '{d.via}' needs 'M' cells on the ground.");
                    defs.Add(new DeliveryDefinition(area, via, d.heavy));
                    if (d.box == null) continue; // brought by a crane
                    var cell = new GridPos(d.box[0], d.box[1]);
                    if (!crates.Contains(cell)) throw new LevelFormatException($"{label}: delivery {i} box {cell} is not a 'D' in objects.");
                    if (at.ContainsKey(cell)) throw new LevelFormatException($"{label}: two deliveries name the box at {cell}.");
                    at[cell] = i;
                }
                foreach (var c in crates)
                    if (!at.ContainsKey(c)) throw new LevelFormatException($"{label}: the 'D' at {c} is in no entry of 'deliveries'.");
            }
            else
            {
                // Without a list: one delivery per D in reading order; with numbered areas crate k goes to area k.
                bool numbered = HasArea(2);
                for (int i = 0; i < crates.Count; i++)
                {
                    int area = numbered ? i + 1 : 1;
                    if (!HasArea(area)) throw new LevelFormatException($"{label}: delivery crate {i + 1} has no area {area} on the ground.");
                    defs.Add(new DeliveryDefinition(area, false, false));
                    at[crates[i]] = i;
                }
            }
            state.SetDeliveries(defs, areaOf);
            return at;
        }

        private static int ParseArea(string area, string label, int index)
        {
            switch (area)
            {
                case null: case "": case "B": case "B1": case "1": return 1;
                case "B2": case "2": return 2;
                case "B3": case "3": return 3;
                default: throw new LevelFormatException($"{label}: delivery {index} has unknown area '{area}' (B, 1, 2 or 3).");
            }
        }

        /// <summary>Every delivery has exactly one crate: on the board, or dropped by a crane.</summary>
        private static void CheckDeliverySources(GameState state, string label)
        {
            var count = new int[state.Deliveries.Count];
            foreach (var b in state.Boxes) if (b.Delivery >= 0) count[b.Delivery]++;
            foreach (var c in state.Cranes)
                foreach (var d in c.Drops)
                    if (d.Type == BoxType.Delivery)
                    {
                        if (d.Delivery < 0 || d.Delivery >= count.Length)
                            throw new LevelFormatException($"{label}: crane {c.Id} drops delivery {d.Delivery}, which is not in 'deliveries'.");
                        count[d.Delivery]++;
                    }
            for (int i = 0; i < count.Length; i++)
                if (count[i] != 1) throw new LevelFormatException($"{label}: delivery {i} has {count[i]} crates, needs exactly 1.");
        }

        // ---------- car park and truck (Ana Belge §5.5, §6) ----------

        private const int MaxCars = 60;

        private static void ParseParking(LevelData data, GameState state, string label)
        {
            if (data.trucks != null && data.parking != null) throw new LevelFormatException($"{label}: has both 'trucks' and 'parking'; use 'parking'.");
            if (data.trucks != null)
            {
                // Older block: a parked truck from the start, its loading cells are targets.
                if (data.trucks.count < 1) throw new LevelFormatException($"{label}: trucks.count must be at least 1.");
                if (data.trucks.bay == null || data.trucks.bay.Length == 0) throw new LevelFormatException($"{label}: trucks.bay has no cells.");
                var cells = new List<GridPos>();
                foreach (var c in data.trucks.bay)
                {
                    var p = new GridPos(c[0], c[1]);
                    if (!state.InBounds(p) || state.GetTile(p) != TileType.Target)
                        throw new LevelFormatException($"{label}: truck bay cell {p} must be a target 'T'.");
                    cells.Add(p);
                }
                state.SetTruck(new TruckBay(cells, data.trucks.count));
                return;
            }
            var pk = data.parking;
            if (pk == null) return;

            GridPos Cell(int[] c, string what)
            {
                var p = new GridPos(c[0], c[1]);
                if (!state.InBounds(p)) throw new LevelFormatException($"{label}: {what} {p} is outside the map.");
                var t = state.GetTile(p);
                if (t == TileType.Wall || t == TileType.Void || t == TileType.Hole) throw new LevelFormatException($"{label}: {what} {p} is on a {t}.");
                return p;
            }

            var cars = new List<ParkedCar>();
            var taken = new HashSet<GridPos>();
            if (pk.cars != null)
            {
                if (pk.cars.Length > MaxCars) throw new LevelFormatException($"{label}: {pk.cars.Length} cars, at most {MaxCars}.");
                for (int i = 0; i < pk.cars.Length; i++)
                {
                    var car = pk.cars[i];
                    if (car?.cells == null || car.cells.Length != 2) throw new LevelFormatException($"{label}: car {i} needs exactly 2 cells.");
                    var a = Cell(car.cells[0], $"car {i} cell");
                    var b = Cell(car.cells[1], $"car {i} cell");
                    if (a.ManhattanTo(b) != 1) throw new LevelFormatException($"{label}: car {i} cells {a} and {b} are not side by side.");
                    foreach (var p in new[] { a, b })
                    {
                        if (!taken.Add(p)) throw new LevelFormatException($"{label}: car {i} overlaps another car at {p}.");
                        if (state.GetBoxAt(p) != null || state.PlayerPos == p) throw new LevelFormatException($"{label}: car {i} stands on a crate or the worker at {p}.");
                    }
                    cars.Add(new ParkedCar(a, b, car.color ?? ""));
                }
            }
            var exits = new List<GridPos>();
            if (pk.exits != null) foreach (var e in pk.exits) exits.Add(Cell(e, "parking exit"));
            if (cars.Count > 0 && exits.Count == 0) throw new LevelFormatException($"{label}: parking has cars but no exits.");
            if (cars.Count > 0) state.SetParking(new ParkingLot(cars, exits));

            // The first loading area ("bay"), then any more ("docks", owner 2026-11-02: trucks from up to four sides).
            var bays = new List<TruckBay>();
            TruckBay Bay(int[][] cells, int[][] way, int trucks, string what)
            {
                if (trucks < 1) throw new LevelFormatException($"{label}: {what} trucks must be at least 1.");
                var bay = new List<GridPos>();
                foreach (var c in cells)
                {
                    var p = Cell(c, "truck bay cell");
                    if (taken.Contains(p)) throw new LevelFormatException($"{label}: truck bay cell {p} is under a car.");
                    bay.Add(p);
                }
                var route = new List<GridPos>();
                if (way != null) foreach (var c in way) route.Add(Cell(c, "truck route cell"));
                return new TruckBay(bay, route, trucks);
            }
            if (pk.bay != null && pk.bay.Length > 0) bays.Add(Bay(pk.bay, pk.truckRoute, pk.trucks, "parking"));
            if (pk.docks != null)
                for (int i = 0; i < pk.docks.Length; i++)
                {
                    var d = pk.docks[i];
                    if (d.bay == null || d.bay.Length == 0) throw new LevelFormatException($"{label}: dock {i} has no bay cells.");
                    bays.Add(Bay(d.bay, d.truckRoute, d.trucks, $"dock {i}"));
                }
            if (bays.Count == 0) return; // a car park without a truck
            state.SetTrucks(bays.ToArray());
        }

        // ---------- goals (Ana Belge §5.7) ----------

        private static void ParseGoals(LevelData data, GameState state, string label)
        {
            var goal = GoalType.None;
            if (data.goal == null || data.goal.Length == 0) goal = GoalType.Targets;
            else
                foreach (var name in data.goal)
                {
                    if (!LevelGoal.TryParse(name, out var g)) throw new LevelFormatException($"{label}: unknown goal '{name}' (targets, deliver, truck).");
                    goal |= g;
                }
            if ((goal & GoalType.Trucks) != 0 && state.Truck == null) throw new LevelFormatException($"{label}: goal 'truck' needs a truck (parking.bay).");
            if ((goal & GoalType.Deliver) != 0 && state.Deliveries.Count == 0) throw new LevelFormatException($"{label}: goal 'deliver' needs a delivery area 'B' and a delivery crate 'D'.");
            state.Goal = goal;
        }

        // ---------- cranes (Ana Belge §5.3) ----------

        private static readonly string[] CraneIds = { "A", "B", "C" };
        private static readonly CraneSide[] DefaultSides = { CraneSide.Left, CraneSide.Right, CraneSide.Top };

        /// <summary>The old "crane" block or the "cranes" list (not both), at most 3 cranes, ids and sides not repeated, and every crane valid on its own.</summary>
        private static List<CraneDefinition> ParseCranes(LevelData data, GameState state, string label)
        {
            if (data.HasOldCrane && data.HasCraneList)
                throw new LevelFormatException($"{label}: has both 'crane' and 'cranes'; use only 'cranes'.");
            var list = data.AllCranes;
            if (list.Length > CraneIds.Length)
                throw new LevelFormatException($"{label}: has {list.Length} cranes, at most {CraneIds.Length} allowed.");
            var result = new List<CraneDefinition>(list.Length);
            for (int i = 0; i < list.Length; i++)
            {
                var c = list[i];
                if (c == null || !c.HasDrops) throw new LevelFormatException($"{label}: crane {i} has no drops.");
                string id = string.IsNullOrEmpty(c.id) ? CraneIds[i] : c.id;
                if (System.Array.IndexOf(CraneIds, id) < 0)
                    throw new LevelFormatException($"{label}: crane {i} has id '{id}', must be A, B or C.");
                var side = string.IsNullOrEmpty(c.side) ? DefaultSides[i] : ParseSide(c.side, label, id);
                foreach (var other in result)
                {
                    if (other.Id == id) throw new LevelFormatException($"{label}: crane id '{id}' is used twice.");
                    if (other.Side == side) throw new LevelFormatException($"{label}: cranes {other.Id} and {id} both stand on the {side} side.");
                }
                result.Add(ParseCrane(c, id, side, state, label));
            }
            return result;
        }

        private static CraneSide ParseSide(string side, string label, string id)
        {
            switch (side)
            {
                case "left": return CraneSide.Left;
                case "right": return CraneSide.Right;
                case "top": return CraneSide.Top;
                case "bottom": return CraneSide.Bottom;
                default: throw new LevelFormatException($"{label}: crane {id} has unknown side '{side}' (left, right, top or bottom).");
            }
        }

        /// <summary>every ≥ 2; each drop inside the map, not on a wall or target; type K, B, I, H, 1–6 or D.</summary>
        private static CraneDefinition ParseCrane(CraneData crane, string id, CraneSide side, GameState state, string label)
        {
            if (crane.every < 2) throw new LevelFormatException($"{label}: crane {id} 'every' is {crane.every}, must be at least 2.");
            var drops = new List<CraneDrop>(crane.drops.Length);
            for (int i = 0; i < crane.drops.Length; i++)
            {
                var d = crane.drops[i];
                if (d == null) throw new LevelFormatException($"{label}: crane {id} drop {i} is empty.");
                var cell = new GridPos(d.x, d.y);
                if (!state.InBounds(cell)) throw new LevelFormatException($"{label}: crane {id} drop {i} at {cell} is outside the map.");
                var tile = state.GetTile(cell);
                if (tile == TileType.Wall || tile == TileType.Target)
                    throw new LevelFormatException($"{label}: crane {id} drop {i} at {cell} is on a {(tile == TileType.Wall ? "wall" : "target")}.");
                drops.Add(ParseDrop(d, cell, label, i));
            }
            return new CraneDefinition(id, side, crane.every, drops);
        }

        private static CraneDrop ParseDrop(CraneDropData d, GridPos cell, string label, int index)
        {
            switch (d.type)
            {
                case "B": return new CraneDrop(cell, BoxType.Normal);
                case "I": return new CraneDrop(cell, BoxType.Ice);
                case "H": return new CraneDrop(cell, BoxType.Heavy);
                case "K": return new CraneDrop(cell, BoxType.Container);
                case "D": return new CraneDrop(cell, BoxType.Delivery, 0, d.deliveryIndex);
                case "1": case "2": case "3": case "4": case "5": case "6": return new CraneDrop(cell, BoxType.Normal, d.type[0] - '0');
                default: throw new LevelFormatException($"{label}: crane drop {index} has unknown type '{d.type}' (K, B, I, H, 1–6 or D).");
            }
        }

        private static TileType ParseTile(char c, string label, int x, int y)
        {
            switch (c)
            {
                case '#': return TileType.Wall;
                case '.': return TileType.Floor;
                case 'T': return TileType.Target;
                case 'o': return TileType.Hole;
                case '_': return TileType.Void;
                case 'B': case '1': case '2': case '3': return TileType.Arrival;
                case 'A': return TileType.StartMark;
                case 'M': return TileType.Via;
                case 'G': return TileType.Gate;
                default: throw new LevelFormatException($"{label}: unknown ground '{c}' at column {x}, row {y}.");
            }
        }
    }
}
