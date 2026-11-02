using System.Collections.Generic;

namespace CrateBoss.Core
{
    /// <summary>
    /// Candy levels (owner 2026-11-01). The worker never pushes a colour crate; the player swaps two side by side, and only a
    /// swap that lines up three or more of one colour counts. Every run sinks and earns a hammer hit for every crate that
    /// sinks (owner 2026-11-02: 3 → 3, 10 → 10, blasts included). The four cranes drop a random colour crate into every emptied cell of the candy
    /// area while the level's drops last, and those can make runs again. With no swap left that makes a run while drops
    /// remain, the crates are shuffled. With the cranes empty, the crates beyond a hole slide toward the worker; with no
    /// swap left either, the rest stay as breakable crates (one hit per move they stood). Plain C#, deterministic (the random draw
    /// lives in the state).
    /// </summary>
    public static class CandySystem
    {
        private const int MinRun = 3;
        private const int MaxWaves = 20;
        private const int RedrawTries = 2;

        /// <summary>Moves on the ground per hammer hit of a left-over colour crate (1 = one hit for every move; adjustable).</summary>
        public static int MovesPerHit = 1;

        /// <summary>Hammer hits for <paramref name="length"/> sunk crates (owner 2026-11-02: one hit for every crate that sinks, 3 → 3, 10 → 10).</summary>
        public static int HammersFor(int length, bool fromDrops) => length;

        /// <summary>The colour of a rainbow crate: it lines up with nothing.</summary>
        public const int RainbowSymbol = 7;
        /// <summary>Hits a breakable crate loses for each colour crate that sinks right next to it (owner 2026-11-02).</summary>
        public const int ChipNextTo = 1;
        /// <summary>Hits a breakable crate loses when a combo's blast covers it.</summary>
        public const int ChipBlast = 2;

        /// <summary>
        /// A candy swap: two colour crates side by side change places, or (owner 2026-11-01) one slides into an empty cell of
        /// the candy area next to it. A rainbow crate swapped with a colour crate clears that colour (owner 2026-11-02). False
        /// (nothing changes) when neither fits or no run comes of it.
        /// </summary>
        public static bool Swap(GameState s, PlayerAction action, MoveResult result)
        {
            if (!s.IsCandyLevel || s.CandyDone || action.Swap >= s.Width * s.Height) return false;
            var a = s.CellAt(action.Swap);
            var b = a + action.Direction.ToOffset();
            var boxA = s.GetBoxAt(a);
            var boxB = s.InBounds(b) ? s.GetBoxAt(b) : null;
            if (!IsCandy(boxA)) return false;
            bool rainbow = boxB != null && IsCandy(boxB) && (boxA.Special == CandySpecial.Rainbow || boxB.Special == CandySpecial.Rainbow);
            // Owner 2026-11-02: two combo crates swapped both go off, whatever their colours.
            bool pair = !rainbow && boxB != null && IsCandy(boxB) && boxA.Special != CandySpecial.None && boxB.Special != CandySpecial.None;
            if (!rainbow && !pair && (boxB == null ? !FreeCandyCell(s, b) : !IsCandy(boxB) || Colour(boxA) == Colour(boxB))) return false;
            boxA.Position = b;
            if (boxB != null) boxB.Position = a;
            if (!rainbow && !pair && Runs(s).Count == 0)
            {
                boxA.Position = a;
                if (boxB != null) boxB.Position = b;
                return false;
            }
            result.SwapFrom = a;
            result.SwapTo = b;
            HashSet<int> first = null;
            if (rainbow)
            {
                // The rainbow takes every crate of the other's colour with it (two rainbows: every colour crate).
                var bow = boxA.Special == CandySpecial.Rainbow ? boxA : boxB;
                var other = bow == boxA ? boxB : boxA;
                first = new HashSet<int> { s.Index(bow.Position) };
                foreach (var c in s.Boxes)
                    if (IsCandy(c) && (other.Special == CandySpecial.Rainbow || Colour(c) == Colour(other))) first.Add(s.Index(c.Position));
                bow.Special = CandySpecial.None; // spent: it does not go off again as a caught rainbow
            }
            else if (pair) first = new HashSet<int> { s.Index(boxA.Position), s.Index(boxB.Position) };
            Resolve(s, result, first);
            // Owner 2026-11-02: in an easy level, two groups sinking one after another (a chain) give a paint gun.
            if (s.CandyPaint)
            {
                int groups = 0, waves = 0;
                foreach (var w in result.CandyWaves) if (w.Groups > 0) { groups += w.Groups; waves++; }
                if (groups >= 2 && waves >= 2) { s.PaintGuns++; result.PaintEarned = true; }
            }
            return true;
        }

        /// <summary>How many colour crates the paint gun recolours.</summary>
        public const int PaintCrates = 6;

        /// <summary>
        /// The paint gun (owner 2026-11-02): one colour of the level, drawn at random, goes onto six colour crates picked at random
        /// (plain ones of another colour); what lines up sinks as after a swap.
        /// </summary>
        public static bool Paint(GameState s, MoveResult result)
        {
            if (!s.IsCandyLevel || s.CandyDone || s.PaintGuns <= 0) return false;
            int colour = Draw(s);
            var pool = new List<Box>();
            foreach (var b in s.Boxes) if (IsCandy(b) && b.Special == CandySpecial.None && b.Symbol != colour && b.Symbol != RainbowSymbol) pool.Add(b);
            if (pool.Count == 0) return false;
            s.PaintGuns--;
            result.PaintColour = colour;
            for (int k = 0; k < PaintCrates && pool.Count > 0; k++)
            {
                int i = (int)(Next(s) % (uint)pool.Count);
                var box = pool[i];
                pool.RemoveAt(i);
                box.Symbol = colour;
                result.Painted.Add(box.Id);
            }
            Resolve(s, result);
            return true;
        }

        /// <summary>
        /// Sinks runs, pays hammers, refills from the cranes, again until nothing more sinks. Then, with no swap left: while the
        /// cranes still hold crates they drop them into the empty candy cells (owner 2026-11-01 "vinçte kasa kaldıysa 0 olana
        /// kadar indirsin"), stopping as soon as a swap is there again; a full area with no swap is shuffled. Only when that
        /// leaves no swap either is the candy part over.
        /// </summary>
        public static void Resolve(GameState s, MoveResult result, HashSet<int> first = null)
        {
            int self = 0; // waves in a row this move that sank by themselves
            Waves(s, result, false, ref self, first);
            for (int round = 0; round < MaxWaves; round++)
            {
                if (HasSwap(s)) return;
                if (s.CandyDropsLeft <= 0) break;
                var fill = new CandyWave();
                Slide(s, fill);
                Fill(s, fill);
                if (fill.Drops.Count == 0)
                {
                    // Nowhere to drop: deal the crates out again (a deal that lines some up sinks them, and on it goes).
                    result.Shuffled = true;
                    if (Shuffle(s)) return;
                    if (Runs(s).Count > 0) { Waves(s, result, true, ref self); continue; }
                    break;
                }
                result.CandyWaves.Add(fill);
                Waves(s, result, true, ref self);
            }
            if (!HasSwap(s)) Finish(s, result);
        }

        // Each wave: runs make combo crates (4 → stripe, L / T → bomb, 5 → rainbow) or sink; combo crates caught in what sinks
        // go off (row, column, 3 × 3, a colour) and set off others; hammers are paid; breakable crates next to the holes wear
        // down (and break at 0); the crates above come down and the cranes fill the top. Again until nothing sinks.
        private static void Waves(GameState s, MoveResult result, bool chained, ref int self, HashSet<int> first = null)
        {
            for (int wave = 0; wave < MaxWaves; wave++)
            {
                var runs = Runs(s);
                bool opening = wave == 0 && first != null;
                if (runs.Count == 0 && !opening) break;
                var w = new CandyWave { Groups = runs.Count };
                var keep = Combos(s, runs, wave == 0 && !chained ? result : null);
                var sink = new HashSet<int>();
                foreach (var run in runs)
                    foreach (var i in run)
                        if (!keep.ContainsKey(i)) sink.Add(i);
                if (opening) foreach (var i in first) sink.Add(i);
                var blasted = new HashSet<int>();
                GoOff(s, sink, blasted, w);
                var cells = new List<int>(sink);
                cells.Sort(); // reading order
                foreach (var i in cells)
                {
                    var cell = s.CellAt(i);
                    var box = s.GetBoxAt(cell);
                    if (!IsCandy(box)) continue;
                    w.Sunk.Add(new SunkCrate(box.Id, cell, Colour(box) > 0 ? Colour(box) : box.Symbol));
                    s.RemoveBox(box);
                }
                // Every crate that sank pays a hit; once more waves than CandySuperAfter have sunk by themselves in a row, the next
                // ones pay advanced hammers instead (owner 2026-11-02).
                bool byItself = chained || wave > 0;
                if (byItself) self++;
                if (byItself && self > s.CandySuperAfter) w.SuperHammers += w.Sunk.Count;
                else w.Hammers += HammersFor(w.Sunk.Count, byItself);
                Wear(s, cells, blasted, w);
                foreach (var made in keep) MakeCombo(s, s.CellAt(made.Key), made.Value, w);
                s.Hammers += w.Hammers;
                s.SuperHammers += w.SuperHammers;
                // Owner 2026-11-02: the crates above the holes always come down (toward the worker); what they line up sinks
                // first (the next wave), and only when nothing more sinks do the cranes fill the cells left empty at the top.
                Slide(s, w);
                if (Runs(s).Count == 0) Fill(s, w);
                result.CandyWaves.Add(w);
            }
        }

        // Which run crates become combo crates: where an L / T crosses a bomb; a straight 5 a rainbow; a 4 a stripe along it.
        // The crate the player moved is the one that changes when it is in the run.
        private static Dictionary<int, CandySpecial> Combos(GameState s, List<List<int>> runs, MoveResult moved)
        {
            var keep = new Dictionary<int, CandySpecial>();
            var count = new Dictionary<int, int>();
            foreach (var run in runs) foreach (var i in run) count[i] = count.TryGetValue(i, out var n) ? n + 1 : 1;
            var used = new HashSet<List<int>>();
            foreach (var run in runs)
                foreach (var i in run)
                    if (count[i] > 1 && !keep.ContainsKey(i) && Plain(s, i)) { keep[i] = CandySpecial.Bomb; used.Add(run); }
            foreach (var run in runs)
            {
                if (used.Contains(run) || run.Count < 4) continue;
                bool row = s.CellAt(run[0]).Y == s.CellAt(run[run.Count - 1]).Y;
                int at = run[run.Count / 2];
                if (moved != null)
                    foreach (var i in run)
                        if (s.CellAt(i) == moved.SwapTo || s.CellAt(i) == moved.SwapFrom) at = i;
                if (keep.ContainsKey(at) || !Plain(s, at)) continue;
                keep[at] = run.Count >= 5 ? CandySpecial.Rainbow : row ? CandySpecial.StripeRow : CandySpecial.StripeColumn;
            }
            return keep;
        }

        // A colour crate with no combo of its own (one already a combo goes off instead of changing).
        private static bool Plain(GameState s, int cell)
        {
            var box = s.GetBoxAt(s.CellAt(cell));
            return IsCandy(box) && box.Special == CandySpecial.None;
        }

        // Combo crates among the cells that sink go off: their row, column, 3 × 3 or colour sinks too, setting off others.
        private static void GoOff(GameState s, HashSet<int> sink, HashSet<int> blasted, CandyWave w)
        {
            var queue = new Queue<int>(sink);
            var done = new HashSet<int>();
            while (queue.Count > 0)
            {
                int i = queue.Dequeue();
                var cell = s.CellAt(i);
                var box = s.GetBoxAt(cell);
                if (!IsCandy(box) || box.Special == CandySpecial.None || !done.Add(i)) continue;
                w.Blasts.Add(new CandyBlast(cell, box.Special));
                foreach (var p in Reach(s, cell, box))
                {
                    int j = s.Index(p);
                    var hit = s.GetBoxAt(p);
                    if (IsCandy(hit)) { if (sink.Add(j)) queue.Enqueue(j); }
                    else if (hit != null && hit.Hits > 0 && hit.Type == BoxType.Container) blasted.Add(j);
                }
            }
        }

        private static List<GridPos> Reach(GameState s, GridPos at, Box combo)
        {
            var cells = new List<GridPos>();
            switch (combo.Special)
            {
                case CandySpecial.StripeRow:
                    for (int x = 0; x < s.Width; x++) cells.Add(new GridPos(x, at.Y));
                    break;
                case CandySpecial.StripeColumn:
                    for (int y = 0; y < s.Height; y++) cells.Add(new GridPos(at.X, y));
                    break;
                case CandySpecial.Bomb:
                    for (int dy = -1; dy <= 1; dy++)
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            var p = new GridPos(at.X + dx, at.Y + dy);
                            if (s.InBounds(p)) cells.Add(p);
                        }
                    break;
                case CandySpecial.Rainbow:
                {
                    // Caught by another blast: it takes the commonest colour.
                    var tally = new Dictionary<int, int>();
                    foreach (var b in s.Boxes)
                        if (IsCandy(b) && Colour(b) > 0) tally[Colour(b)] = tally.TryGetValue(Colour(b), out var n) ? n + 1 : 1;
                    int best = 0, most = -1;
                    foreach (var kv in tally) if (kv.Value > most) { most = kv.Value; best = kv.Key; }
                    foreach (var b in s.Boxes) if (IsCandy(b) && Colour(b) == best) cells.Add(b.Position);
                    break;
                }
            }
            return cells;
        }

        // Owner 2026-11-02: breakable crates lose a hit for every colour crate that sank right next to them, two when a blast
        // covered them; at 0 they break.
        private static void Wear(GameState s, List<int> sunk, HashSet<int> blasted, CandyWave w)
        {
            var loss = new Dictionary<Box, int>();
            void Add(Box b, int n)
            {
                if (b == null || b.Hits <= 0 || b.Type != BoxType.Container) return;
                loss[b] = (loss.TryGetValue(b, out var old) ? old : 0) + n;
            }
            foreach (var i in sunk)
            {
                var c = s.CellAt(i);
                foreach (var d in new[] { new GridPos(1, 0), new GridPos(-1, 0), new GridPos(0, 1), new GridPos(0, -1) })
                    if (s.InBounds(c + d)) Add(s.GetBoxAt(c + d), ChipNextTo);
            }
            foreach (var i in blasted) Add(s.GetBoxAt(s.CellAt(i)), ChipBlast);
            foreach (var kv in loss)
            {
                var box = kv.Key;
                box.Hits = System.Math.Max(0, box.Hits - kv.Value);
                w.Chips.Add(new CrateChip(box.Id, box.Position, box.Hits));
                if (box.Hits == 0) s.RemoveBox(box);
            }
        }

        // A run crate becomes a combo crate where it stands (a rainbow is a new crate of no colour).
        private static void MakeCombo(GameState s, GridPos cell, CandySpecial kind, CandyWave w)
        {
            var box = s.GetBoxAt(cell);
            if (!IsCandy(box)) return;
            if (kind != CandySpecial.Rainbow)
            {
                box.Special = kind;
                w.Made.Add(new CandyMade(box.Id, cell, kind, -1));
                return;
            }
            s.RemoveBox(box);
            var bow = s.AddBox(BoxType.Normal, cell, RainbowSymbol);
            bow.Special = CandySpecial.Rainbow;
            bow.Age = box.Age;
            // Owner 2026-11-02 (later): a colour of its own, unlike every other crate (ComboDecor draws it).
            w.Made.Add(new CandyMade(bow.Id, cell, kind, box.Id));
        }

        // The cranes drop a crate into every empty cell of the candy area while they hold crates.
        private static void Fill(GameState s, CandyWave w)
        {
            for (int i = 0; i < s.CandyArea.Length && s.CandyDropsLeft > 0; i++)
            {
                var cell = s.CellAt(i);
                if (FreeCandyCell(s, cell)) Drop(s, cell, w);
            }
        }

        // One crane drop into a cell. Mostly a colour that does not finish a run on landing (two tries more), so chains
        // happen now and then and the player's swaps decide the hammers, not luck.
        private static void Drop(GameState s, GridPos cell, CandyWave w)
        {
            int symbol = Draw(s);
            for (int tries = 0; tries < RedrawTries && MakesRun(s, cell, symbol); tries++) symbol = Draw(s);
            var box = s.AddBox(BoxType.Normal, cell, symbol);
            s.CandyDropsLeft--;
            w.Drops.Add(new CandyDrop(box.Id, cell, symbol, CraneFor(s, cell)));
        }

        /// <summary>
        /// The cranes are done and no swap is left (owner 2026-11-01): the colour crates stay where they are as breakable
        /// crates, each needing one hammer hit for every move it stood on the ground; their numbers show only now.
        /// </summary>
        public static void Finish(GameState s, MoveResult result)
        {
            s.CandyDone = true;
            foreach (var box in s.Boxes)
            {
                if (!IsCandy(box)) continue;
                int hits = System.Math.Max(1, (box.Age + MovesPerHit - 1) / MovesPerHit);
                box.Hits = s.CandyHitCap > 0 ? System.Math.Min(hits, s.CandyHitCap) : hits;
            }
            result.CandyFinished = true;
        }

        /// <summary>After every move of the candy part: each colour crate on the ground grows one move older.</summary>
        public static void Age(GameState s)
        {
            if (!s.IsCandyLevel || s.CandyDone) return;
            foreach (var box in s.Boxes)
                if (IsCandy(box)) box.Age++;
        }

        /// <summary>
        /// Closes the holes of the candy area: each colour crate moves down (owner 2026-11-02, later: toward the bottom of the phone) into the free candy
        /// cell ahead, again and again until none can, never past another kind of crate or a cell outside the area.
        /// </summary>
        public static void Slide(GameState s, CandyWave wave)
        {
            var fall = Down(s);
            var order = FallOrder(s, fall);
            Dictionary<int, GridPos> from = null;
            for (bool moved = true; moved;)
            {
                moved = false;
                foreach (int i in order)
                {
                    var cell = s.CellAt(i);
                    var box = s.GetBoxAt(cell);
                    if (!IsCandy(box)) continue;
                    var to = cell + ((Direction)fall[i]).ToOffset();
                    if (!FreeCandyCell(s, to)) continue;
                    from ??= new Dictionary<int, GridPos>();
                    if (!from.ContainsKey(box.Id)) from[box.Id] = cell;
                    box.Position = to;
                    moved = true;
                }
            }
            if (from == null) return;
            foreach (var kv in from)
            {
                var box = s.GetBoxById(kv.Key);
                wave.Falls.Add(new BoxMovement(box.Id, box.Type, kv.Value, box.Position, FallType.None));
            }
        }

        /// <summary>
        /// Owner 2026-11-02 (later): the crates always come down toward the bottom of the phone (grid down, the camera looks
        /// straight on), in every band; the inward field (<see cref="Fall"/>) still tells the belts where the bands end.
        /// </summary>
        private static sbyte[] Down(GameState s)
        {
            if (s.CandyDown != null) return s.CandyDown;
            var down = new sbyte[s.Width * s.Height];
            for (int i = 0; i < down.Length; i++) down[i] = s.CandyArea[i] ? (sbyte)Direction.Down : (sbyte)-1;
            s.CandyDown = down;
            return down;
        }

        /// <summary>
        /// The fall way of every cell (found once per level, kept in the state): the worker's open floor (floor outside the
        /// candy area he can reach without crossing it, crates ignored) is the bottom; every other floor cell falls toward it
        /// by the shortest way, preferring the main direction toward the worker, then down, left, right, up.
        /// </summary>
        public static sbyte[] Fall(GameState s)
        {
            if (s.CandyFall != null) return s.CandyFall;
            int n = s.Width * s.Height;
            var dist = new int[n];
            for (int i = 0; i < n; i++) dist[i] = int.MaxValue;
            var queue = new Queue<int>();
            bool Floor(GridPos p) => s.InBounds(p) && GoalRules.IsWalkableTile(s.GetTile(p));
            // The worker's open floor.
            if (Floor(s.PlayerPos) && !s.CandyArea[s.Index(s.PlayerPos)])
            {
                int start = s.Index(s.PlayerPos);
                dist[start] = 0;
                queue.Enqueue(start);
                var flood = new Queue<int>();
                flood.Enqueue(start);
                while (flood.Count > 0)
                {
                    var c = s.CellAt(flood.Dequeue());
                    for (int d = 0; d < 4; d++)
                    {
                        var p = c + ((Direction)d).ToOffset();
                        if (!Floor(p)) continue;
                        int j = s.Index(p);
                        if (dist[j] == 0 || s.CandyArea[j]) continue;
                        dist[j] = 0;
                        queue.Enqueue(j);
                        flood.Enqueue(j);
                    }
                }
            }
            while (queue.Count > 0)
            {
                int i = queue.Dequeue();
                var c = s.CellAt(i);
                for (int d = 0; d < 4; d++)
                {
                    var p = c + ((Direction)d).ToOffset();
                    if (!Floor(p)) continue;
                    int j = s.Index(p);
                    if (dist[j] != int.MaxValue) continue;
                    dist[j] = dist[i] + 1;
                    queue.Enqueue(j);
                }
            }
            var main = TowardWorker(s);
            var prefer = new[] { main, Direction.Down, Direction.Left, Direction.Right, Direction.Up };
            var fall = new sbyte[n];
            for (int i = 0; i < n; i++)
            {
                fall[i] = -1;
                if (!s.CandyArea[i] || dist[i] == int.MaxValue || dist[i] == 0) continue;
                var c = s.CellAt(i);
                foreach (var d in prefer)
                {
                    var p = c + d.ToOffset();
                    if (Floor(p) && dist[s.Index(p)] == dist[i] - 1) { fall[i] = (sbyte)d; break; }
                }
            }
            s.CandyFall = fall;
            return fall;
        }

        // Candy cells nearest the bottom first, so a whole column closes up in one pass.
        private static List<int> FallOrder(GameState s, sbyte[] fall)
        {
            var depth = new Dictionary<int, int>();
            int Depth(int i)
            {
                if (depth.TryGetValue(i, out int k)) return k;
                depth[i] = 0; // guards against a loop (there is none: every way goes down)
                int next = fall[i] < 0 ? -1 : s.Index(s.CellAt(i) + ((Direction)fall[i]).ToOffset());
                k = next < 0 || !s.CandyArea[next] || fall[next] < 0 ? 0 : Depth(next) + 1;
                depth[i] = k;
                return k;
            }
            var order = new List<int>();
            for (int i = 0; i < fall.Length; i++) if (fall[i] >= 0) { Depth(i); order.Add(i); }
            order.Sort((a, b) => depth[a] != depth[b] ? depth[a].CompareTo(depth[b]) : a.CompareTo(b));
            return order;
        }

        /// <summary>The grid direction from the middle of the candy area toward the worker (down when he is right on it).</summary>
        public static Direction TowardWorker(GameState s)
        {
            float sx = 0, sy = 0;
            int n = 0;
            for (int i = 0; i < s.CandyArea.Length; i++)
                if (s.CandyArea[i]) { var c = s.CellAt(i); sx += c.X; sy += c.Y; n++; }
            if (n == 0) return Direction.Down;
            float dx = s.PlayerPos.X - sx / n, dy = s.PlayerPos.Y - sy / n;
            if (System.Math.Abs(dx) > System.Math.Abs(dy)) return dx > 0 ? Direction.Right : Direction.Left;
            return dy < 0 ? Direction.Up : Direction.Down;
        }

        /// <summary>Colour crates still on the board.</summary>
        public static int CandyLeft(GameState s)
        {
            int n = 0;
            foreach (var b in s.Boxes) if (IsCandy(b)) n++;
            return n;
        }

        /// <summary>Some swap of two colour crates, or slide of one into an empty candy cell, would make a run.</summary>
        public static bool HasSwap(GameState s)
        {
            if (!s.IsCandyLevel || s.CandyDone) return false;
            foreach (var box in s.Boxes.ToArray())
            {
                if (!IsCandy(box)) continue;
                foreach (var dir in new[] { Direction.Up, Direction.Down, Direction.Left, Direction.Right })
                {
                    var to = box.Position + dir.ToOffset();
                    if (FreeCandyCell(s, to))
                    {
                        var from = box.Position;
                        box.Position = to;
                        bool made = Runs(s).Count > 0;
                        box.Position = from;
                        if (made) return true;
                        continue;
                    }
                    if (dir == Direction.Up || dir == Direction.Left) continue; // pairs are tried once, from the left / top crate
                    var other = s.GetBoxAt(box.Position + dir.ToOffset());
                    if (!IsCandy(other)) continue;
                    if (box.Special == CandySpecial.Rainbow || other.Special == CandySpecial.Rainbow) return true;
                    if (box.Special != CandySpecial.None && other.Special != CandySpecial.None) return true;
                    if (Colour(other) == Colour(box)) continue;
                    var a = box.Position;
                    box.Position = other.Position;
                    other.Position = a;
                    bool run = Runs(s).Count > 0;
                    other.Position = box.Position;
                    box.Position = a;
                    if (run) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Owner 2026-11-02: the candy hint, the swap that pays the most hammers right now (tried on a copy), or null.
        /// </summary>
        public static PlayerAction? BestSwap(GameState s)
        {
            if (!s.IsCandyLevel || s.CandyDone) return null;
            var rules = new RulesEngine();
            PlayerAction? best = null;
            int most = -1;
            foreach (var box in s.Boxes)
            {
                if (!IsCandy(box)) continue;
                foreach (var dir in new[] { Direction.Right, Direction.Down, Direction.Left, Direction.Up })
                {
                    var a = PlayerAction.SwapCandy(s.Index(box.Position), dir);
                    var t = s.Clone();
                    if (!rules.TryMove(t, a).Success) continue;
                    int pay = t.Hammers + t.SuperHammers * 2 - s.Hammers - s.SuperHammers * 2;
                    if (pay > most) { most = pay; best = a; }
                }
            }
            return best;
        }

        /// <summary>Deals the colour crates' places out again until a swap works and nothing sinks by itself (gives up after a while).</summary>
        public static bool Shuffle(GameState s)
        {
            var candy = new List<Box>();
            foreach (var b in s.Boxes) if (IsCandy(b)) candy.Add(b);
            if (candy.Count < MinRun) return false;
            var places = new List<GridPos>();
            foreach (var b in candy) places.Add(b.Position);
            for (int attempt = 0; attempt < 60; attempt++)
            {
                for (int i = places.Count - 1; i > 0; i--)
                {
                    int j = (int)(Next(s) % (uint)(i + 1));
                    (places[i], places[j]) = (places[j], places[i]);
                }
                for (int i = 0; i < candy.Count; i++) candy[i].Position = places[i];
                if (Runs(s).Count == 0 && HasSwap(s)) return true;
            }
            return false; // gave up: the crates stay as dealt last
        }

        /// <summary>An empty cell of the candy area the worker does not stand on: a colour crate may slide into it.</summary>
        public static bool FreeCandyCell(GameState s, GridPos p) =>
            s.InBounds(p) && s.CandyArea[s.Index(p)] && s.GetBoxAt(p) == null && s.PlayerPos != p;

        public static bool IsCandy(Box b) => b != null && b.Symbol > 0 && b.Type == BoxType.Normal;

        /// <summary>
        /// The colour a crate lines up with: its own; none (0) for a rainbow (owner 2026-11-02, later: the rainbow is a colour of its
        /// own, unlike every other crate; it works only by a swap, or caught in a blast).
        /// </summary>
        public static int Colour(Box b) => b.Symbol == RainbowSymbol ? 0 : b.Symbol;

        /// <summary>Every horizontal or vertical run of three or more of one colour, as lists of cell indices.</summary>
        public static List<List<int>> Runs(GameState s)
        {
            int w = s.Width, h = s.Height;
            var symbol = new int[w * h];
            foreach (var b in s.Boxes)
                if (IsCandy(b)) symbol[s.Index(b.Position)] = Colour(b);
            var runs = new List<List<int>>();
            void Scan(int lines, int length, System.Func<int, int, int> cell)
            {
                for (int line = 0; line < lines; line++)
                {
                    int start = 0;
                    for (int i = 1; i <= length; i++)
                    {
                        int prev = symbol[cell(line, i - 1)];
                        if (i < length && symbol[cell(line, i)] == prev) continue;
                        if (prev > 0 && i - start >= MinRun)
                        {
                            var run = new List<int>(i - start);
                            for (int k = start; k < i; k++) run.Add(cell(line, k));
                            runs.Add(run);
                        }
                        start = i;
                    }
                }
            }
            Scan(h, w, (y, x) => y * w + x);
            Scan(w, h, (x, y) => y * w + x);
            return runs;
        }

        private static int Draw(GameState s) => s.CandyColors[(int)(Next(s) % (uint)s.CandyColors.Length)];

        /// <summary>A crate of this colour landing on the cell would finish a run of three with crates already there.</summary>
        private static bool MakesRun(GameState s, GridPos cell, int symbol)
        {
            int Count(GridPos step)
            {
                int n = 0;
                for (var p = cell + step; s.InBounds(p); p += step)
                {
                    var b = s.GetBoxAt(p);
                    if (!IsCandy(b) || Colour(b) != symbol) break;
                    n++;
                }
                return n;
            }
            return Count(new GridPos(1, 0)) + Count(new GridPos(-1, 0)) >= 2 || Count(new GridPos(0, 1)) + Count(new GridPos(0, -1)) >= 2;
        }

        // xorshift32
        private static uint Next(GameState s)
        {
            uint x = s.CandyRng;
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;
            s.CandyRng = x;
            return x;
        }

        // The nearest crane: 0 / 1 at the back corners, 2 / 3 on the left / right side (BoardView places them so).
        private static int CraneFor(GameState s, GridPos cell) => (cell.X * 2 >= s.Width ? 1 : 0) + (cell.Y * 3 >= s.Height ? 2 : 0);
    }
}
