using System.Collections;
using System.Collections.Generic;
using CrateBoss.Core;
using UnityEngine;

namespace CrateBoss.View
{
    /// <summary>
    /// Draws a GameState as a little diorama and plays MoveResults. Applies no rules itself:
    /// it only moves pictures to where the logic says things went. Every event gets
    /// a picture, a sound and a vibration (Ana Belge §2).
    /// </summary>
    public class BoardView : MonoBehaviour
    {
        // Owner 2026-11-01: quick, but holding the finger walks on without stopping, so not so quick that it runs away.
        public const float WalkTime = 0.095f;
        public const float PushTime = 0.105f;
        public const float HeavyPushTime = 0.15f;
        public const float IceCellTime = 0.06f;
        public const float FrontWallHeight = 0.5f;

        private readonly Dictionary<GridPos, TileView> tiles = new Dictionary<GridPos, TileView>();
        /// <summary>The fixed crates of the yard (inner walls), for the wordless tutorial.</summary>
        public readonly List<GridPos> InnerWalls = new List<GridPos>();
        private readonly Dictionary<int, BoxView> boxes = new Dictionary<int, BoxView>();
        // Kind of box that filled each hole, so a rebuild (undo) keeps the look.
        private readonly Dictionary<GridPos, BoxType> filledBy = new Dictionary<GridPos, BoxType>();
        private Transform content;
        // The yard's still decoration (site markings, barrier): kept through undo while the layout is the same.
        private Transform staticPart;
        private string staticKey;
        private IPlayerView robot;
        private HintMarker hint;
        private readonly List<CraneView> cranes = new List<CraneView>();
        private WorldTheme theme = WorldThemes.Current;
        private int running;
        private bool introPlaying;

        public GameFeedback Feedback { get; set; }
        public CameraFramer Framer { get; set; }
        public EffectsView Effects { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public bool IsAnimating => running > 0;
        public bool IsIntroPlaying => introPlaying;
        public int FilledTargets { get; private set; }
        /// <summary>Level id: keeps the obstacle kinds the same each time the level opens.</summary>
        public int Seed { get; set; }
        /// <summary>The level's first crane, or null.</summary>
        public CraneView Crane => cranes.Count > 0 ? cranes[0] : null;
        /// <summary>All cranes, in the same order as GameState.Cranes.</summary>
        public IReadOnlyList<CraneView> Cranes => cranes;

        private bool AnyCraneTense()
        {
            foreach (var c in cranes)
                if (c.IsTense) return true;
            return false;
        }

        // Worried while a drop is close, looking at the nearest such drop cell (Ana Belge §19).
        private void UpdateWorry()
        {
            bool tense = AnyCraneTense();
            robot.Worried = tense;
            CraneView nearest = null;
            float best = float.MaxValue;
            foreach (var c in cranes)
            {
                if (!c.IsTense) continue;
                float d = (c.BadgeAnchor - robot.transform.position).sqrMagnitude;
                if (d < best) { best = d; nearest = c; }
            }
            robot.Watch(nearest != null ? (Vector3?)nearest.BadgeAnchor : null);
        }

        public Vector3 ToWorld(GridPos p) => new Vector3(p.X, 0f, Height - 1 - p.Y);

        /// <summary>A cell's centre in world space (the board may be moved as a whole).</summary>
        public Vector3 CellWorld(GridPos p) => content != null ? content.TransformPoint(ToWorld(p)) : ToWorld(p);

        /// <summary>A board-space point (from <see cref="ToWorld"/>) in the world.</summary>
        public Vector3 CellWorldPoint(Vector3 local) => content != null ? content.TransformPoint(local) : local;

        /// <summary>New level: forget hole fillings from the previous one.</summary>
        public void ClearHistory() => filledBy.Clear();

        private string LayoutKey(GameState state)
        {
            var sb = new System.Text.StringBuilder(theme.Id).Append('|').Append(state.Width).Append('x').Append(state.Height).Append('|');
            for (int y = 0; y < state.Height; y++)
            for (int x = 0; x < state.Width; x++) sb.Append((int)state.GetTile(new GridPos(x, y))).Append(',');
            foreach (var bay in state.Bays) foreach (var c in bay.Cells) sb.Append('T').Append(c.X).Append(',').Append(c.Y);
            return sb.ToString();
        }

        /// <summary>Rebuilds everything to match the state instantly (level start, undo).</summary>
        public void Build(GameState state, WorldTheme worldTheme)
        {
            StopAllCoroutines();
            truckTrips.Clear();
            truckDriving.Clear();
            autoHammers.Clear(); // they lived under the content, rebuilt below
            running = 0;
            introPlaying = false;
            theme = worldTheme ?? WorldThemes.Current;
            string layout = LayoutKey(state);
            if (staticPart != null && layout == staticKey) staticPart.SetParent(transform, false);
            else if (staticPart != null) { Destroy(staticPart.gameObject); staticPart = null; }
            if (content != null) Destroy(content.gameObject);
            tiles.Clear();
            InnerWalls.Clear();
            boxes.Clear();
            hint = null;

            Width = state.Width;
            Height = state.Height;
            lastState = state;
            content = new GameObject("Board").transform;
            content.SetParent(transform, false);

            if (Effects == null)
            {
                Effects = new GameObject("Effects").AddComponent<EffectsView>();
                Effects.transform.SetParent(transform, false);
                Effects.Build();
            }
            Effects.CountScale = QualityManager.ParticleScale;

            var outer = BoundaryMap.OuterWalls(state);
            if (staticPart == null)
            {
                staticPart = new GameObject("Static").transform;
                staticPart.SetParent(content, false);
                CitySiteBuilder.Build(state, staticPart, theme, ToWorld);
                BarrierBuilder.Build(state, outer, staticPart, theme);
                staticKey = layout;
            }
            else staticPart.SetParent(content, false);
            var pieces = ObstacleLayout.For(state, outer);
            for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
            {
                var cell = new GridPos(x, y);
                var type = state.GetTile(cell);
                if (type == TileType.Void) continue;
                var tile = new GameObject($"Tile {x},{y}").AddComponent<TileView>();
                tile.transform.SetParent(content, false);
                tile.transform.position = ToWorld(cell);
                bool isOuter = outer[y * Width + x];
                if (type == TileType.Wall && !isOuter) InnerWalls.Add(cell);
                bool front = type == TileType.Wall && !isOuter && IsFrontWall(state, cell);
                tile.Build(type, cell, (x + y) % 2 == 1, theme, filledBy.TryGetValue(cell, out var b) ? b : BoxType.Normal,
                    front ? FrontWallHeight : 1f, front, isOuter, Seed, pieces[y * Width + x]);
                if (type == TileType.Gate) tile.SetGate(state.GatesOpen);
                tiles[cell] = tile;
            }

            foreach (var box in state.Boxes) AddBox(box);
            hitBadges.Clear();
            foreach (var box in state.Boxes)
                if (box.Hits > 0) hitBadges[box.Id] = HitBadge.Create(boxes[box.Id].transform, ToWorld(box.Position) + Vector3.up * BadgeHeight(box), box.Hits);

            // The worker (Ana Belge §19.3).
            robot = new GameObject("Player").AddComponent<WorkerView>();
            robot.transform.SetParent(content, false);
            robot.transform.position = ToWorld(state.PlayerPos);
            robot.Build();
            robot.Face(Direction.Down);
            if (Framer != null) Framer.Follow = robot.transform;
            workerLock = WorkerLock.Create(content, robot.transform);
            workerLock.Show(WorkerLocked);

            cranes.Clear();
            for (int i = 0; i < state.Cranes.Count; i++)
            {
                var crane = new GameObject("Crane " + state.Cranes[i].Id).AddComponent<CraneView>();
                crane.transform.SetParent(content, false);
                crane.Build(state, i, ToWorld, Feedback);
                cranes.Add(crane);
            }
            beltRoot = null;
            beltKey = null;
            RefreshBelts(state, false);
            candyCranes.Clear();
            if (state.IsCandyLevel)
            {
                // Owner 2026-11-01: four cranes refill the candy area: two at the back corners, two on the sides level with the
                // top of the yard (none in front, where they would hide it). They stand on the pavement just outside the fence,
                // never on the road.
                var middle = ToWorld(new GridPos(Width / 2, Height / 2));
                float left = -1.1f, right = Width + 0.1f, back = Height + 0.1f, side = Height - 1 - Height / 3;
                foreach (var mast in new[] { new Vector3(left, 0f, back), new Vector3(right, 0f, back), new Vector3(left, 0f, side), new Vector3(right, 0f, side) })
                {
                    var crane = new GameObject("CandyCrane").AddComponent<CandyCraneView>();
                    crane.transform.SetParent(content, false);
                    crane.Build(mast, middle);
                    candyCranes.Add(crane);
                }
            }
            UpdateWorry();

            // Goal pieces (Ana Belge §5): the truck and the parked cars.
            // Owner 2026-11-02: one truck for every loading area (up to four sides).
            trucks.Clear();
            for (int b = 0; b < state.Bays.Length; b++)
            {
                var bay = state.Bays[b];
                var truck = new GameObject("Truck " + b).AddComponent<TruckView>();
                truck.transform.SetParent(content, false);
                truck.Build(bay, ToWorld, Width, Height, Feedback, state.ParkedAt(b));
                // Candy levels: the road runs right behind the yard (DistrictBuilder.BackRoad); the truck uses it.
                if (state.IsCandyLevel)
                {
                    // Back and right always have a road; left and front get one when a truck comes that way.
                    var outward = TruckView.Outward(bay, ToWorld, Width, Height);
                    var mid = Vector3.zero;
                    foreach (var c in bay.Cells) mid += ToWorld(c);
                    mid /= bay.Cells.Count;
                    float off = DistrictBuilder.BackRoadOffset;
                    if (outward == Vector3.forward) truck.RoadDistance = Height - 0.5f + off - mid.z;
                    else if (outward == Vector3.right) truck.RoadDistance = Width - 0.5f + off - mid.x;
                    else if (outward == Vector3.left) truck.RoadDistance = mid.x + 0.5f + off;
                    else if (outward == Vector3.back) truck.RoadDistance = mid.z + 0.5f + off;
                }
                trucks.Add(truck);
            }
            cars.Clear();
            if (state.Parking != null)
                for (int i = 0; i < state.Parking.Cars.Count; i++)
                {
                    var car = new GameObject("Car " + i).AddComponent<CarView>();
                    car.transform.SetParent(content, false);
                    car.Build(state.Parking.Cars[i], i, ToWorld);
                    car.gameObject.SetActive(state.CarPresent(i));
                    cars.Add(car);
                }
            RefreshDeliveryAreas(state);
            RefreshTargets(state, false);
            RefreshOcclusion(state);
        }

        private readonly List<CandyCraneView> candyCranes = new List<CandyCraneView>();
        private readonly Dictionary<int, HitBadge> hitBadges = new Dictionary<int, HitBadge>();

        /// <summary>Candy levels: hammer hits earned by a sunk wave, and where (the HUD counts them up).</summary>
        public event System.Action<int, Vector3> HammersEarned;

        private readonly List<TruckView> trucks = new List<TruckView>();
        private readonly List<CarView> cars = new List<CarView>();
        private GameState lastState;

        /// <summary>Delivery areas glow green once their crate is locked in (Ana Belge §5.1).</summary>
        private void RefreshDeliveryAreas(GameState s)
        {
            if (s.Deliveries.Count == 0) return;
            var done = new bool[4];
            foreach (var b in s.Boxes)
                if (b.Locked) done[s.Deliveries[b.Delivery].Area] = true;
            foreach (var tile in tiles.Values)
                if (tile.Type == TileType.Arrival) tile.SetAreaDone(done[s.AreaAt(tile.Cell)]);
        }

        /// <summary>Step 3 as a picture: a delivered crate hops and settles, its area turns green; a refused one flashes red.</summary>
        private void PlayDeliveries(IReadOnlyList<DeliveryEvent> events, GameState after)
        {
            foreach (var e in events)
            {
                var pos = ToWorld(e.Cell);
                if (e.Accepted)
                {
                    if (boxes.TryGetValue(e.BoxId, out var view)) Run(view.Hops(0.18f, 0.06f));
                    Effects.Ring(pos, Palette.TargetDone);
                    Effects.Stars(pos + Vector3.up * 0.6f, 5);
                    Feedback?.TargetLocked(4);
                }
                else if (tiles.TryGetValue(e.Cell, out var tile)) Run(tile.FlashRefused());
            }
            RefreshDeliveryAreas(after);
        }

        /// <summary>A sent car that hit something rolls up, squashes and hops back (Ana Belge §16 "Araba çarpar").</summary>
        public IEnumerator PlayCarBump(CarEvent e)
        {
            if (e.Car < cars.Count)
            {
                Feedback?.Boing();
                yield return cars[e.Car].Bump(e.ReachedCell, e.Direction, Effects);
            }
        }

        /// <summary>Hint for a car: the arrow over the car, pointing the way it should be sent.</summary>
        /// <summary>The touched car wobbles (owner 2026-11-01).</summary>
        public void PulseCar(int car)
        {
            if (car >= 0 && car < cars.Count && cars[car] != null) cars[car].Pulse();
        }

        public void ShowCarHint(int car, Direction dir)
        {
            ClearHint();
            if (car >= cars.Count) return;
            var center = cars[car].transform.localPosition;
            var pushDir = ToWorld(dir.ToOffset()) - ToWorld(new GridPos(0, 0));
            hint = new GameObject("Hint").AddComponent<HintMarker>();
            hint.transform.SetParent(content, false);
            hint.Build(center, pushDir, center - pushDir);
            robot.Think();
        }

        /// <summary>The yard cell under a screen point (on the ground plane), or null.</summary>
        public GridPos? CellAtScreen(Camera cam, Vector2 screen) => CellAtScreen(cam, screen, 0f);

        /// <summary>The cell under a screen point on a level <paramref name="height"/> above the floor (a crate's lid, say).</summary>
        public GridPos? CellAtScreen(Camera cam, Vector2 screen, float height)
        {
            if (cam == null || content == null) return null;
            var ray = cam.ScreenPointToRay(screen);
            if (Mathf.Abs(ray.direction.y) < 1e-4f) return null;
            var hit = ray.origin + ray.direction * ((content.position.y + height - ray.origin.y) / ray.direction.y);
            var local = content.InverseTransformPoint(hit);
            int x = Mathf.RoundToInt(local.x), y = Height - 1 - Mathf.RoundToInt(local.z);
            return x >= 0 && y >= 0 && x < Width && y < Height ? new GridPos(x, y) : (GridPos?)null;
        }

        // Step 5 of a move (Ana Belge §5.6) as a picture: a full truck leaves, the next one backs in.
        // Owner 2026-11-02: the truck comes and goes while the player plays on; its trips queue up and run on their own.
        private IEnumerator PlayGoalEvents(MoveResult result, GameState after)
        {
            if (result.TruckLeft == null) yield break;
            foreach (var e in result.TruckEvents)
            {
                if (e.Bay >= trucks.Count) continue;
                var truck = trucks[e.Bay];
                if (!truckTrips.TryGetValue(e.Bay, out var trips)) truckTrips[e.Bay] = trips = new Queue<IEnumerator>();
                if (e.Left)
                {
                    var crates = new List<Transform>();
                    foreach (var id in e.BoxIds)
                        if (boxes.TryGetValue(id, out var view)) { crates.Add(view.transform); boxes.Remove(id); }
                    trips.Enqueue(truck.Leave(crates));
                }
                if (e.Arrived) trips.Enqueue(truck.Arrive());
                if (!truckDriving.Contains(e.Bay)) StartCoroutine(DriveTruck(e.Bay));
            }
            yield break;
        }

        // Each loading area's trips run in turn; the areas drive at the same time.
        private readonly Dictionary<int, Queue<IEnumerator>> truckTrips = new Dictionary<int, Queue<IEnumerator>>();
        private readonly HashSet<int> truckDriving = new HashSet<int>();

        private IEnumerator DriveTruck(int bay)
        {
            truckDriving.Add(bay);
            var trips = truckTrips[bay];
            while (trips.Count > 0) yield return trips.Dequeue();
            truckDriving.Remove(bay);
        }

        // ---------- level intro ----------

        /// <summary>The island rises, then boxes and the worker drop in one after another (≤ 0.8 s).</summary>
        public IEnumerator PlayIntro()
        {
            introPlaying = true;
            var drops = new List<Transform>();
            foreach (var b in boxes.Values) drops.Add(b.transform);
            drops.Add(robot.transform);
            var rest = new Dictionary<Transform, Vector3>();
            foreach (var d in drops) { rest[d] = d.position; d.position += Vector3.up * 3f; d.gameObject.SetActive(false); }

            const float rise = 0.3f;
            for (float t = 0; t < rise; t += Time.deltaTime)
            {
                float k = 1f - Mathf.Pow(1f - t / rise, 3f);
                content.localPosition = Vector3.down * (2.5f * (1f - k));
                yield return null;
            }
            content.localPosition = Vector3.zero;

            float stagger = Mathf.Min(0.06f, 0.3f / Mathf.Max(1, drops.Count));
            for (int i = 0; i < drops.Count; i++)
            {
                drops[i].gameObject.SetActive(true);
                Run(Drop(drops[i], rest[drops[i]], 0.18f));
                yield return new WaitForSeconds(stagger);
            }
            while (running > 0) yield return null;
            introPlaying = false;
        }

        /// <summary>A tap skips the intro: everything snaps into place.</summary>
        public void SkipIntro(GameState state)
        {
            if (!introPlaying) return;
            Build(state, theme);
        }

        private IEnumerator Drop(Transform t, Vector3 rest, float duration)
        {
            var start = rest + Vector3.up * 3f;
            for (float time = 0; time < duration; time += Time.deltaTime)
            {
                float k = time / duration;
                t.position = Vector3.Lerp(start, rest, k * k);
                yield return null;
            }
            t.position = rest;
            Feedback?.Settle();
        }

        // ---------- moves ----------

        /// <summary>Animates one successful move; finishes when every part has landed.</summary>
        public IEnumerator PlayMove(MoveResult result, GameState after)
        {
            lastState = after;
            ClearHint();
            RefreshOcclusion(after);
            bool heavy = false;
            foreach (var m in result.BoxMoves) heavy |= m.BoxType == BoxType.Heavy;
            float duration = heavy ? HeavyPushTime : result.BoxMoves.Count > 0 ? PushTime : WalkTime;
            var target = ToWorld(result.PlayerTo);

            if (result.Car != null)
            {
                // A car was sent and drove off (Ana Belge §16 "Araba gider"); the worker points it on its way.
                Feedback?.Horn();
                robot.Think();
                if (result.Car.Car < cars.Count) Run(cars[result.Car.Car].DriveOff(result.Car.ReachedCell, result.Car.Direction, Effects));
            }
            else if (result.Action.IsSwap) Run(PlaySwap(result.SwapFrom, result.SwapTo, false));
            else if (result.Painted.Count > 0) Run(PlayPaint(result));
            else if (result.HitBox >= 0) Run(PlayHit(result, after));
            else if (result.BoxMoves.Count == 0)
            {
                Run(robot.Walk(target, result.Direction, WalkTime));
                Feedback?.Step();
            }
            else
            {
                Run(robot.Push(target, result.Direction, duration, heavy));
                if (heavy) { Feedback?.HeavyPush(); Framer?.Shake(0.05f, 0.025f); } // Ana Belge §16: a heavy chain shakes the camera a little
                else if (result.BoxMoves[0].BoxType == BoxType.Ice) Feedback?.IceSlide();
                else Feedback?.WoodPush();
            }
            foreach (var move in result.BoxMoves) Run(AnimateBox(move, duration));
            while (running > 0) yield return null;
            PlayDeliveries(result.Deliveries, after);
            yield return PlaySunk(result.Sunk);
            if (result.HasCandyWaves || result.Shuffled) yield return PlayCandyWaves(result, after);
            RefreshBelts(after, true);
            if (result.ConveyorMoves != null && result.ConveyorMoves.Count > 0) yield return PlayBelt(result.ConveyorMoves);
            if (result.GatesOpened)
            {
                // Owner 2026-11-02: the candy yard's puzzle is solved: hammers fly in and the barriers sink.
                Feedback?.Win();
                Effects.Stars(robot.transform.position + Vector3.up * 1.6f, 16);
                if (result.PuzzleReward > 0) HammersEarned?.Invoke(result.PuzzleReward, content.InverseTransformPoint(robot.transform.position));
                foreach (var t in tiles.Values) if (t.Type == TileType.Gate) Run(t.OpenGate());
            }
            if (result.CandyFinished) ShowNewBadges(after);
            if (result.CandyFinished) foreach (var v in boxes.Values) v.GetComponent<CargoArrow>()?.Show(true); // the candy part is over
            yield return PlayGoalEvents(result, after);
            // Cranes that dropped play one after another (A, B, C); the others just count down.
            for (int i = 0; i < cranes.Count; i++)
            {
                CraneDropEvent drop = null;
                foreach (var d in result.CraneDrops)
                    if (d.Crane == i) drop = d;
                if (drop != null) yield return PlayCraneDrop(drop, after);
                else cranes[i].Refresh(after);
            }
            UpdateWorry();
            yield return PlaySunk(result.CraneSunk);
            PlayDeliveries(result.CraneDeliveries, after);
            RefreshTargets(after, true);
        }

        // ---------- candy levels (owner 2026-11-01) ----------

        private BoxView ViewAt(GridPos cell)
        {
            var pos = ToWorld(cell);
            foreach (var view in boxes.Values)
            {
                var d = view.transform.position - pos;
                if (d.x * d.x + d.z * d.z < 0.04f) return view;
            }
            return null;
        }

        /// <summary>Two colour crates slide past each other (or one into an empty cell); a refused swap goes half way and back.</summary>
        public IEnumerator PlaySwap(GridPos a, GridPos b, bool refused)
        {
            var va = ViewAt(a);
            var vb = ViewAt(b);
            if (va == null) yield break;
            Vector3 pa = ToWorld(a), pb = ToWorld(b);
            Feedback?.WoodPush();
            if (vb == null)
            {
                // Into an empty cell (owner 2026-11-01): one crate slides, or goes half way and back.
                if (refused)
                {
                    yield return va.MoveTo(Vector3.Lerp(pa, pb, 0.35f), 0.08f, false, false);
                    Feedback?.Bump();
                    yield return va.MoveTo(pa, 0.1f, false, true);
                }
                else yield return va.MoveTo(pb, 0.15f, false, true);
                yield break;
            }
            if (refused)
            {
                Run(va.MoveTo(Vector3.Lerp(pa, pb, 0.35f), 0.08f, false, false));
                yield return vb.MoveTo(Vector3.Lerp(pb, pa, 0.35f), 0.08f, false, false);
                Feedback?.Bump();
                Run(va.MoveTo(pa, 0.1f, false, true));
                yield return vb.MoveTo(pb, 0.1f, false, true);
                yield break;
            }
            Run(va.MoveTo(pb, 0.15f, false, true));
            yield return vb.MoveTo(pa, 0.15f, false, true);
        }

        /// <summary>Each wave: the groups sink, the hammers they earned fly to the counter, the cranes drop new crates in.</summary>
        private IEnumerator PlayCandyWaves(MoveResult result, GameState after)
        {
            foreach (var wave in result.CandyWaves)
            {
                // Combo crates going off: a flash along the row / column, a burst for a bomb, sparkles for a rainbow.
                foreach (var blast in wave.Blasts) PlayBlast(blast);
                if (wave.Sunk.Count > 0 && (wave.Blasts.Count > 0 || wave.Groups >= 2 || wave != result.CandyWaves[0])) Cheered?.Invoke(wave.Blasts.Count, wave.Groups, result.CandyWaves.IndexOf(wave));
                // A rainbow at work (swapped, or caught in a blast): its web goes out to every crate it takes first.
                GridPos? bow = null;
                foreach (var sk in wave.Sunk) if (sk.Symbol == CandySystem.RainbowSymbol) { bow = sk.Cell; break; }
                if (bow == null) foreach (var bl in wave.Blasts) if (bl.Kind == CandySpecial.Rainbow) { bow = bl.Cell; break; }
                if (bow.HasValue)
                {
                    var reach = new List<SunkCrate>();
                    foreach (var sk in wave.Sunk) if (sk.Cell != bow.Value) reach.Add(sk);
                    if (reach.Count > 0) yield return PlayWeb(bow.Value, reach);
                }
                yield return PlaySunk(wave.Sunk);
                PlayChips(wave.Chips, after);
                foreach (var made in wave.Made) PlayMade(made, after);
                if (wave.Hammers > 0 && wave.Sunk.Count > 0) HammersEarned?.Invoke(wave.Hammers, ToWorld(wave.Sunk[wave.Sunk.Count / 2].Cell));
                // The crates above the holes come down first (toward the worker)...
                foreach (var fall in wave.Falls)
                    if (boxes.TryGetValue(fall.BoxId, out var slid)) Run(slid.MoveTo(ToWorld(fall.To), 0.1f + 0.04f * fall.Distance, true, true));
                if (wave.Falls.Count > 0) Feedback?.WoodPush();
                while (running > 0) yield return null;
                // ...then each crane lowers new ones into the cells left empty, one after another; the four work side by side.
                var queued = new int[4];
                foreach (var drop in wave.Drops)
                {
                    int crane = Mathf.Clamp(drop.Crane, 0, 3);
                    Run(DropCandy(drop, queued[crane] * DropStep + crane * 0.03f));
                    queued[crane]++;
                }
                while (running > 0) yield return null;
                // Owner 2026-11-02: the jibs do not hang over the yard; back to their corners once they have dropped.
                foreach (var c in candyCranes) c.GoHome();
            }
            if (result.Shuffled)
            {
                // No swap was left: every colour crate glides to its new place.
                foreach (var box in after.Boxes)
                    if (box.Symbol > 0 && boxes.TryGetValue(box.Id, out var view)) Run(view.MoveTo(ToWorld(box.Position), 0.35f, false, true));
                while (running > 0) yield return null;
            }
            RefreshOcclusion(after);
        }

        // Owner 2026-11-02: a combo crate's mark (stick, bomb, ball) stands on its lid; its number floats above the mark.
        private static float BadgeHeight(Box box) => box.Special != CandySpecial.None ? 1.62f : 1.02f;

        /// <summary>The candy part is over: every crate that became breakable shows how many hits it needs.</summary>
        private void ShowNewBadges(GameState after)
        {
            foreach (var box in after.Boxes)
            {
                if (box.Hits <= 0 || hitBadges.ContainsKey(box.Id)) continue;
                var pos = ToWorld(box.Position);
                hitBadges[box.Id] = HitBadge.Create(boxes.TryGetValue(box.Id, out var owner) ? owner.transform : content, pos + Vector3.up * BadgeHeight(box), box.Hits);
                hitBadges[box.Id].Set(box.Hits);
                Effects.Sparkle(pos + Vector3.up * 0.7f, Color.white, 4);
            }
        }

        /// <summary>One crane's turn: swing, lower, let go (CandyCraneView).</summary>
        private const float DropStep = 0.3f; // owner 2026-11-02: more action

        /// <summary>Candy levels: a wave worth cheering (blasts, groups, its place in the chain); the HUD shows "Süper!" and such.</summary>
        public event System.Action<int, int, int> Cheered;

        private void PlayBlast(CandyBlast blast)
        {
            var at = ToWorld(blast.Cell) + Vector3.up * 0.5f;
            Framer?.Shake(0.08f, 0.05f);
            switch (blast.Kind)
            {
                case CandySpecial.StripeRow:
                case CandySpecial.StripeColumn:
                    var step = blast.Kind == CandySpecial.StripeRow ? Vector3.right : Vector3.forward;
                    int span = blast.Kind == CandySpecial.StripeRow ? Width : Height;
                    for (int i = -span; i <= span; i += 2) Effects.Sparkle(at + step * i, Color.white, 3);
                    break;
                case CandySpecial.Bomb:
                    Effects.ToyClouds(at, 10, 0.4f);
                    Effects.Confetti(at, 40);
                    break;
                default:
                    Effects.Stars(at + Vector3.up * 0.4f, 12);
                    Effects.Confetti(at, 50);
                    break;
            }
            Feedback?.Boing();
        }

        // Breakable crates worn by the sinking next to them: their number counts down; at 0 they burst.
        private void PlayChips(List<CrateChip> chips, GameState after)
        {
            foreach (var chip in chips)
            {
                if (!boxes.TryGetValue(chip.BoxId, out var view)) continue;
                var pos = view.transform.position;
                if (!chip.Broke)
                {
                    if (hitBadges.TryGetValue(chip.BoxId, out var badge)) badge.Set(chip.HitsLeft);
                    Run(view.Wobble(0.15f));
                    continue;
                }
                Effects.ToyClouds(pos, 8, 0.35f);
                Effects.Confetti(pos + Vector3.up * 0.5f, 24);
                boxes.Remove(chip.BoxId);
                Destroy(view.gameObject);
                hitBadges.Remove(chip.BoxId);
            }
            RefreshOcclusion(after);
        }

        // A combo crate is made: it gets its marks with a pop (a rainbow replaces the crate that stood there).
        private void PlayMade(CandyMade made, GameState after)
        {
            if (made.ReplacedId >= 0 && boxes.TryGetValue(made.ReplacedId, out var old)) { boxes.Remove(made.ReplacedId); Destroy(old.gameObject); }
            if (!boxes.TryGetValue(made.BoxId, out var view))
            {
                var box = after.GetBoxById(made.BoxId);
                if (box == null) return;
                AddBox(box);
                view = boxes[made.BoxId];
                view.transform.position = ToWorld(made.Cell); // where it was made; later falls move it on
            }
            else ComboDecor.Apply(view.transform, made.Kind);
            Effects.Stars(view.transform.position + Vector3.up * 0.8f, 6);
            Run(Sequence(view.Squash(0.06f, 0.25f), view.Hops(0.2f)));
        }

        // ---------- the cargo belt (owner 2026-11-02) ----------

        private Transform beltRoot;
        private string beltKey;

        /// <summary>
        /// The belts lying now (owner 2026-11-02: they appear along every open way to the truck): drawn again whenever they
        /// change, new ones growing out of the floor.
        /// </summary>
        private void RefreshBelts(GameState state, bool grow)
        {
            var belts = ConveyorSystem.Belts(state);
            var key = new System.Text.StringBuilder();
            foreach (var b in belts) { foreach (var c in b) key.Append(c.X).Append(',').Append(c.Y).Append(';'); key.Append('|'); }
            if (key.ToString() == beltKey) return;
            beltKey = key.ToString();
            if (beltRoot != null) Destroy(beltRoot.gameObject);
            beltRoot = new GameObject("Belts").transform;
            beltRoot.SetParent(content, false);
            foreach (var b in belts) BuildBelt(b, grow);
        }

        // A dark strip on the floor with rails and yellow chevrons pointing the way it carries.
        private void BuildBelt(List<GridPos> belt, bool grow)
        {
            if (belt.Count < 2) return;
            var strip = MaterialLibrary.Shared(new Color(0.22f, 0.24f, 0.28f), 0.4f);
            var rail = MaterialLibrary.Shared(new Color(0.62f, 0.65f, 0.7f), 0.6f, 0.4f);
            var arrow = MaterialLibrary.Shared(new Color(1f, 0.8f, 0.2f), 0.5f);
            var root = new GameObject("Belt").transform;
            root.SetParent(beltRoot, false);
            for (int i = 0; i < belt.Count; i++)
            {
                var to = i + 1 < belt.Count ? belt[i + 1] : belt[i] + (belt[i] - belt[i - 1]);
                var d = ToWorld(to) - ToWorld(belt[i]);
                var cell = new GameObject("BeltCell").transform;
                cell.SetParent(root, false);
                cell.position = ToWorld(belt[i]);
                cell.rotation = Quaternion.LookRotation(d.normalized, Vector3.up);
                if (grow) { cell.localScale = Vector3.zero; StartCoroutine(Grow(cell, i * 0.035f)); }
                Shapes.NoShadows(Shapes.Cube(cell, new Vector3(0f, 0.012f, 0f), new Vector3(0.78f, 0.024f, 1f), strip, "Strip"));
                foreach (var x in new[] { -0.42f, 0.42f })
                    Shapes.NoShadows(Shapes.Cube(cell, new Vector3(x, 0.03f, 0f), new Vector3(0.06f, 0.06f, 1f), rail, "Rail"));
                foreach (var side in new[] { -1f, 1f })
                {
                    var bar = Shapes.Cube(cell, new Vector3(side * 0.11f, 0.03f, 0.02f), new Vector3(0.05f, 0.012f, 0.3f), arrow, "Chevron");
                    bar.transform.localRotation = Quaternion.Euler(0f, -side * 40f, 0f);
                    Shapes.NoShadows(bar);
                }
            }
        }

        private static IEnumerator Grow(Transform cell, float delay)
        {
            yield return new WaitForSeconds(delay);
            for (float t = 0; t < 0.18f && cell != null; t += Time.deltaTime)
            {
                cell.localScale = Vector3.one * Mathf.SmoothStep(0f, 1f, t / 0.18f);
                yield return null;
            }
            if (cell != null) cell.localScale = Vector3.one;
        }

        // Cargo crates ride the belt one cell at a time, in the order the rules moved them.
        private IEnumerator PlayBelt(List<BoxMovement> rides)
        {
            foreach (var ride in rides)
            {
                if (!boxes.TryGetValue(ride.BoxId, out var view)) continue;
                yield return view.MoveTo(ToWorld(ride.To), 0.09f, true, false);
            }
            Feedback?.Settle();
        }

        private IEnumerator DropCandy(CandyDrop drop, float delay)
        {
            if (delay > 0f) yield return new WaitForSeconds(delay);
            var pos = ToWorld(drop.Cell);
            if (drop.Crane < candyCranes.Count) yield return candyCranes[drop.Crane].SwingTo(pos);
            var view = new GameObject($"Box {drop.BoxId} candy").AddComponent<BoxView>();
            view.transform.SetParent(content, false);
            view.transform.position = pos + Vector3.up * CandyCraneView.JibHeight;
            view.BuildSymbol(drop.BoxId, drop.Symbol);
            boxes[drop.BoxId] = view;
            if (drop.Crane < candyCranes.Count) yield return candyCranes[drop.Crane].Lower(view.transform, pos);
            else view.transform.position = pos;
            Effects.ToyClouds(pos, 3, 0.2f);
            Feedback?.Settle();
            yield return Sequence(view.Squash(0.06f, 0.2f), view.Hops(0.12f));
        }

        /// <summary>The worker swings the hammer: the crate shakes, its number counts down; at 0 it bursts.</summary>
        /// <summary>Owner 2026-11-02: the worker is locked (a lock bobs over his head; drags slide the view).</summary>
        public bool WorkerLocked
        {
            get => workerLocked;
            set { workerLocked = value; if (workerLock != null) workerLock.Show(value); }
        }
        private bool workerLocked;
        private WorkerLock workerLock;

        /// <summary>Owner 2026-11-02: the third hit on the same crate in a row goes on by itself, blow after blow, quicker.</summary>
        public bool RapidHits { get; set; }

        private IEnumerator PlayHit(MoveResult result, GameState after)
        {
            var dir = result.Direction;
            robot.Face(dir);
            Run(robot.Push(ToWorld(result.PlayerTo), dir, 0.12f, true));
            if (!boxes.TryGetValue(result.HitBox, out var view)) yield break;
            var pos = view.transform.position;
            // Owner 2026-11-01: the hammer comes down on the crate's lid and the crate hops.
            yield return HammerBlow(pos, ToWorld(result.PlayerTo));
            Feedback?.HeavyPush();
            Framer?.Shake(0.06f, 0.04f);
            Effects.Sparkle(pos + Vector3.up * 0.9f, new Color(1f, 0.85f, 0.3f), 8);
            Effects.ToyClouds(pos + Vector3.up * 0.85f, 3, 0.18f);
            if (!result.Broke)
            {
                if (hitBadges.TryGetValue(result.HitBox, out var badge)) badge.Set(after.GetBoxById(result.HitBox)?.Hits ?? 0);
                if (RapidHits) { Run(Sequence(view.Squash(0.03f, 0.18f), view.Hops(0.1f))); yield return new WaitForSeconds(0.04f); }
                else yield return Sequence(view.Squash(0.05f, 0.22f), view.Hops(0.16f));
                yield break;
            }
            // Broken: a burst of dust and confetti, the crate and its number are gone.
            Effects.ToyClouds(pos, 8, 0.35f);
            Effects.Confetti(pos + Vector3.up * 0.5f, 24);
            Feedback?.Boing();
            boxes.Remove(result.HitBox);
            Destroy(view.gameObject);
            if (hitBadges.TryGetValue(result.HitBox, out var gone)) { hitBadges.Remove(result.HitBox); Destroy(gone.gameObject); }
            RefreshOcclusion(after);
        }

        /// <summary>
        /// The hammer (3dModeller/crateboss/aletler) appears over the crate on the worker's side, swings down onto the lid and
        /// shrinks away. Its handle end is the pivot; the head lands on the lid.
        /// </summary>
        private readonly Dictionary<int, AutoHammer> autoHammers = new Dictionary<int, AutoHammer>();

        /// <summary>
        /// Owner 2026-11-02: a hit from afar, the auto hammer going on by itself or a tap with an advanced hammer. A hammer of
        /// its own stands over the crate and strikes in <paramref name="seconds"/>; it plays alongside whatever the worker does.
        /// </summary>
        public void PlayRemoteHit(MoveResult result, GameState after, float seconds, bool super)
        {
            if (!boxes.TryGetValue(result.HitBox, out var view)) return;
            int id = result.HitBox;
            var pos = view.transform.position;
            if (!autoHammers.TryGetValue(id, out var hammer) || hammer == null)
            {
                var root = BuildHammer(super ? "AdvancedHammer" : "AutoHammer");
                if (root == null) return;
                hammer = root.gameObject.AddComponent<AutoHammer>();
                var cam = Camera.main;
                // It comes from the side facing the camera's right, so it never hides the crate's number.
                var from = pos - (cam != null ? cam.transform.right : Vector3.right) * 2f;
                hammer.Setup(pos, from, super);
                autoHammers[id] = hammer;
            }
            bool broke = result.Broke;
            hammer.Strike(seconds, () =>
            {
                if (view == null) return;
                Effects.Sparkle(pos + Vector3.up * 0.9f, super ? new Color(1f, 0.8f, 0.15f) : new Color(1f, 0.85f, 0.3f), super ? 10 : 5);
                if (!broke)
                {
                    Feedback?.WoodPush();
                    if (hitBadges.TryGetValue(id, out var badge)) badge.Set(after.GetBoxById(id)?.Hits ?? 0);
                    StartCoroutine(Sequence(view.Squash(0.02f, 0.14f), view.Hops(0.06f))); // not waited for: the worker plays on
                    return;
                }
                Feedback?.Boing();
                Effects.ToyClouds(pos, 8, 0.35f);
                Effects.Confetti(pos + Vector3.up * 0.5f, 24);
                Framer?.Shake(0.06f, 0.04f);
                boxes.Remove(id);
                Destroy(view.gameObject);
                if (hitBadges.TryGetValue(id, out var gone)) { hitBadges.Remove(id); Destroy(gone.gameObject); }
                StopAutoHammer(id);
                RefreshOcclusion(after);
            });
        }

        /// <summary>The auto hammer on this crate is done (broken, out of hammers, undo): it shrinks away.</summary>
        public void StopAutoHammer(int boxId)
        {
            if (autoHammers.TryGetValue(boxId, out var hammer) && hammer != null) hammer.Leave();
            autoHammers.Remove(boxId);
        }

        // The hammer model with its handle end on the returned root (its head along the root's +z), or null without a model.
        private Transform BuildHammer(string name)
        {
            var root = new GameObject(name).transform;
            root.SetParent(content, false);
            var aligner = new GameObject("Model").transform;
            aligner.SetParent(root, false);
            aligner.localRotation = Quaternion.Euler(0f, 90f, 0f); // the model's head end (-x) points along the root's +z
            var model = VisualCatalog.TryInstantiate("Hammer", aligner);
            if (model == null) { Destroy(root.gameObject); return null; }
            model.transform.localScale *= 1.25f;
            // Put the handle's end on the root.
            var renderers = model.GetComponentsInChildren<Renderer>();
            var b = new Bounds(root.InverseTransformPoint(renderers[0].bounds.center), Vector3.zero);
            foreach (var r in renderers)
            {
                var rb = r.bounds;
                b.Encapsulate(root.InverseTransformPoint(rb.min));
                b.Encapsulate(root.InverseTransformPoint(rb.max));
            }
            aligner.localPosition = new Vector3(-b.center.x, -b.center.y, -b.min.z);
            foreach (var r in renderers) r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return root;
        }

        private IEnumerator HammerBlow(Vector3 crate, Vector3 worker)
        {
            var root = BuildHammer("HammerBlow");
            if (root == null) yield break;

            var flat = crate - worker;
            flat.y = 0f;
            var toward = flat.sqrMagnitude > 1e-4f ? flat.normalized : Vector3.forward;
            var lid = crate + Vector3.up * 0.9f;
            root.position = lid - toward * 0.62f + Vector3.up * 0.45f;
            var strike = Quaternion.LookRotation(lid - root.position, Vector3.up);
            var side = Vector3.Cross(Vector3.up, toward);
            var raised = Quaternion.AngleAxis(-115f, side) * strike;
            root.rotation = raised;
            root.localScale = Vector3.zero;
            float grow = RapidHits ? 0.02f : 0.05f;
            for (float t = 0; t < grow; t += Time.deltaTime) { root.localScale = Vector3.one * (t / grow); yield return null; }
            root.localScale = Vector3.one;
            float swing = RapidHits ? 0.06f : 0.085f; // owner 2026-11-02: snappier blows, a quick series when it hits on its own
            for (float t = 0; t < swing; t += Time.deltaTime)
            {
                float k = t / swing;
                root.rotation = Quaternion.Slerp(raised, strike, k * k);
                yield return null;
            }
            root.rotation = strike;
            Run(Linger(root));
        }

        // After the blow the hammer bounces up a little and shrinks away.
        private IEnumerator Linger(Transform root)
        {
            var strike = root.rotation;
            var side = root.right;
            for (float t = 0; t < 0.14f; t += Time.deltaTime)
            {
                float k = t / 0.14f;
                root.rotation = Quaternion.AngleAxis(-25f * Mathf.Sin(k * Mathf.PI * 0.5f), side) * strike;
                root.localScale = Vector3.one * (1f - k * k);
                yield return null;
            }
            Destroy(root.gameObject);
        }

        // ---------- colour crates (Ana Belge §5.4) ----------

        /// <summary>Three in a row: hatches open under the crates, they glow and drop through one after another, hatches spring shut.</summary>
        /// <summary>
        /// Owner 2026-11-02: the paint gun, blobs of one colour fly from the worker to the crates it picked, one after another;
        /// each crate turns that colour as its blob lands (then whatever lines up sinks, as after a swap).
        /// </summary>
        private IEnumerator PlayPaint(MoveResult result)
        {
            var color = SymbolArt.ColorOf(result.PaintColour);
            var paint = MaterialLibrary.Create(color, 0.3f);
            var from = robot.transform.position + Vector3.up * 1.1f;
            robot.Think();
            Feedback?.Pop();
            Effects.Sparkle(from, color, 12);
            foreach (int id in result.Painted)
            {
                if (!boxes.TryGetValue(id, out var view)) continue;
                Run(PaintBlob(from, view, id, result.PaintColour, paint, color));
                yield return new WaitForSeconds(0.13f);
            }
            // Owner 2026-11-02 "efekt daha belirgin olsun": a moment to see the painted crates before anything sinks.
            yield return new WaitForSeconds(0.55f);
        }

        // One blob: a big drop of paint with a trail arcs from the worker onto the crate, splashes there and the crate takes
        // the colour, hopping a few times (owner 2026-11-02: "zıplayarak boyansın sonra dursun").
        private IEnumerator PaintBlob(Vector3 from, BoxView view, int id, int symbol, Material paint, Color color)
        {
            var blobGo = Shapes.Make(PrimitiveType.Sphere, content, Vector3.zero, Vector3.one * 0.46f, paint, "Paint");
            Shapes.NoShadows(blobGo);
            var blob = blobGo.transform;
            var trail = blobGo.AddComponent<TrailRenderer>();
            trail.time = 0.25f;
            trail.startWidth = 0.32f;
            trail.endWidth = 0.02f;
            trail.sharedMaterial = paint;
            trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            var to = view.transform.position + Vector3.up * 0.95f;
            blob.position = from;
            const float time = 0.5f;
            for (float t = 0; t < time; t += Time.deltaTime)
            {
                float k = t / time;
                blob.position = Vector3.Lerp(from, to, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 2.2f;
                blob.localScale = Vector3.one * (0.46f + 0.12f * Mathf.Sin(k * Mathf.PI * 4f));
                yield return null;
            }
            Destroy(blob.gameObject);
            Effects.Sparkle(to, color, 18);
            Effects.Confetti(to, 14);
            Feedback?.Pop();
            var painted = Recolour(id, symbol);
            if (painted != null) yield return Sequence(painted.Squash(0.06f, 0.3f), painted.Hops(0.45f, 0.25f, 0.12f));
        }

        // A colour crate takes another colour where it stands (the paint gun): its view is built again in that colour.
        private BoxView Recolour(int id, int symbol)
        {
            if (!boxes.TryGetValue(id, out var old) || old == null) return null;
            var view = new GameObject($"Box {id} Normal").AddComponent<BoxView>();
            view.transform.SetParent(content, false);
            view.transform.position = old.transform.position;
            view.BuildSymbol(id, symbol);
            Destroy(old.gameObject);
            boxes[id] = view;
            return view;
        }

        private static Material webMaterial;

        /// <summary>
        /// Owner 2026-11-02: the rainbow casts a web, a quick line from it to every crate it takes, one after another; each
        /// crate it reaches lights up and swells, and only then do they all sink.
        /// </summary>
        private IEnumerator PlayWeb(GridPos origin, List<SunkCrate> targets)
        {
            webMaterial ??= MaterialLibrary.Unlit(new Color(1f, 1f, 1f, 0.9f));
            var from = ToWorld(origin) + Vector3.up * 1.05f;
            targets.Sort((a, b) => (ToWorld(a.Cell) - from).sqrMagnitude.CompareTo((ToWorld(b.Cell) - from).sqrMagnitude));
            var lines = new List<GameObject>();
            float step = Mathf.Clamp(0.5f / Mathf.Max(1, targets.Count), 0.02f, 0.06f);
            foreach (var t in targets)
            {
                var to = ToWorld(t.Cell) + Vector3.up * 0.9f;
                var go = new GameObject("Web");
                go.transform.SetParent(content, false);
                var line = go.AddComponent<LineRenderer>();
                line.useWorldSpace = true;
                line.positionCount = 2;
                line.SetPosition(0, from);
                line.SetPosition(1, to);
                line.startWidth = 0.07f;
                line.endWidth = 0.03f;
                line.sharedMaterial = webMaterial;
                line.startColor = Color.white;
                line.endColor = SymbolArt.ColorOf(t.Symbol);
                line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                lines.Add(go);
                if (boxes.TryGetValue(t.BoxId, out var view))
                {
                    StartCoroutine(Glow(view.transform));
                    Effects.Sparkle(to, Color.white, 3);
                }
                Feedback?.Pop();
                yield return new WaitForSeconds(step);
            }
            yield return new WaitForSeconds(0.18f);
            foreach (var go in lines) Destroy(go);
        }

        // A crate the web reached: it swells and stays a little bigger until it sinks.
        private static IEnumerator Glow(Transform crate)
        {
            var size = crate.localScale;
            for (float t = 0; t < 0.12f; t += Time.deltaTime)
            {
                if (crate == null) yield break;
                crate.localScale = size * (1f + 0.18f * Mathf.Sin(t / 0.12f * Mathf.PI * 0.5f));
                yield return null;
            }
        }

        private IEnumerator PlaySunk(IReadOnlyList<SunkCrate> sunk)
        {
            if (sunk.Count == 0) yield break;
            var hatches = new List<HatchView>();
            for (int i = 0; i < sunk.Count; i++)
            {
                var pos = ToWorld(sunk[i].Cell);
                var hatch = HatchView.Create(content, pos);
                hatches.Add(hatch);
                Run(hatch.Open());
                if (boxes.TryGetValue(sunk[i].BoxId, out var view)) Run(view.SinkThroughHatch(0.05f * i));
                Effects.Sparkle(pos + Vector3.up * 0.6f, SymbolArt.ColorOf(sunk[i].Symbol));
            }
            // Ana Belge §16: three rising notes and a pop; confetti, more of it for a long row.
            if (Feedback != null) Run(Feedback.MatchNotes(sunk.Count));
            var middle = ToWorld(sunk[sunk.Count / 2].Cell);
            Effects.Confetti(middle, sunk.Count >= 4 ? 60 : 26);
            Effects.Stars(middle + Vector3.up * 0.5f, sunk.Count + 2);
            Framer?.Shake(0.05f, 0.03f);
            while (running > 0) yield return null;
            foreach (var s in sunk)
                if (boxes.TryGetValue(s.BoxId, out var view)) { boxes.Remove(s.BoxId); Destroy(view.gameObject); }
            foreach (var h in hatches) Run(h.CloseAndVanish());
            Feedback?.Settle();
        }

        /// <summary>Doc §4.6 "Bırakma": the box falls, squashes, dust ring, short shake, metal boom or wood knock.</summary>
        private IEnumerator PlayCraneDrop(CraneDropEvent drop, GameState after)
        {
            var crane = cranes[drop.Crane];
            yield return crane.Drop();
            yield return PlayLanding(drop.Type, drop.Cell, drop.BoxId, drop.FellInto, after, true);
            crane.Reload(after);
        }

        /// <summary>A crane load arrives on its cell: a box appears (squash, dust), or it fills a hole / sinks.</summary>
        private IEnumerator PlayLanding(BoxType type, GridPos cell, int boxId, FallType fellInto, GameState after, bool fromAbove)
        {
            var pos = ToWorld(cell);
            if (fromAbove) Framer?.Shake(0.06f, 0.05f);
            Feedback?.CraneLanded(type);
            Effects.Ring(pos, new Color(0.8f, 0.76f, 0.7f, 0.85f));
            if (fromAbove && after.PlayerPos.ManhattanTo(cell) <= 1) Run(robot.Startle());
            var drop = (Type: type, Cell: cell, FellInto: fellInto);

            var box = fellInto == FallType.None ? after.GetBoxById(boxId) : null;
            if (box != null)
            {
                AddBox(box);
                // Ana Belge §16: squash, then two shrinking hops (25 % then 8 %), cloud ring and stars.
                var landed = boxes[box.Id];
                Run(Sequence(landed.Squash(0.1f, 0.3f), landed.Hops(0.25f, 0.08f)));
                Effects.ToyCloudRing(pos);
                Effects.Stars(pos + Vector3.up * 0.6f, 4);
                Feedback?.Boing();
                RefreshOcclusion(after);
            }
            else
            {
                // Straight into a hole (fills it) or into the sea.
                var view = new GameObject($"Dropped {drop.Type}").AddComponent<BoxView>();
                view.transform.SetParent(content, false);
                view.transform.position = pos;
                view.Build(-1, drop.Type);
                bool intoHole = drop.FellInto == FallType.Hole;
                Feedback?.Fall();
                yield return view.Fall(intoHole);
                if (intoHole)
                {
                    view.SinkStatic();
                    filledBy[drop.Cell] = drop.Type;
                    Feedback?.Settle();
                    if (tiles.TryGetValue(drop.Cell, out var tile)) yield return tile.PlayFill();
                }
                else Destroy(view.gameObject);
            }
        }

        public IEnumerator PlayBump(Direction dir)
        {
            Feedback?.Bump();
            Run(robot.Bump(dir));
            while (running > 0) yield return null;
        }

        /// <summary>Win: the worker cheers, confetti flies, every target glows in turn.</summary>
        public IEnumerator PlayWin()
        {
            Run(robot.Celebrate());
            Effects.Confetti(robot.transform.position);
            foreach (var tile in tiles.Values)
            {
                if (tile.Type != TileType.Target) continue;
                Run(tile.Pulse(0.4f));
                Effects.Sparkle(tile.transform.position + Vector3.up * 0.3f, Palette.TargetDone);
                yield return new WaitForSeconds(0.12f);
            }
            while (running > 0) yield return null;
        }

        private IEnumerator AnimateBox(BoxMovement move, float pushTime)
        {
            if (!boxes.TryGetValue(move.BoxId, out var view)) yield break;
            var from = ToWorld(move.From);
            var to = ToWorld(move.To);
            var pushDir = (to - from).normalized;
            bool ice = move.BoxType == BoxType.Ice;
            bool heavy = move.BoxType == BoxType.Heavy;
            if (!ice) Effects.ToyClouds(from - pushDir * 0.45f, heavy ? 4 : 3, heavy ? 0.3f : 0.24f, -pushDir * 0.5f); // Ana Belge §16 cartoon clouds

            if (ice)
            {
                robot.Watch(view.transform.position);
                float duration = move.Distance * IceCellTime;
                var slide = view.MoveTo(to, duration, true, move.FellInto == FallType.None);
                float trail = 0f;
                while (slide.MoveNext())
                {
                    robot.Watch(view.transform.position);
                    trail += Time.deltaTime;
                    if (trail > 0.03f) { trail = 0f; Effects.Frost(view.transform.position); }
                    yield return slide.Current;
                }
                robot.Watch(null);
                if (move.FellInto == FallType.None)
                {
                    Feedback?.IceStop();
                    Effects.Sparkle(to + Vector3.up * 0.5f, new Color(0.8f, 0.97f, 1f));
                    yield return view.Recoil(pushDir); // two small bounces as it stops
                }
            }
            else
            {
                yield return view.MoveTo(to, pushTime, false, move.FellInto == FallType.None);
                if (move.FellInto == FallType.None) Run(view.Wobble(0.16f));
                if (heavy && move.FellInto == FallType.None)
                {
                    Framer?.Shake();
                    Effects.Ring(to, new Color(0.7f, 0.65f, 0.6f, 0.8f));
                }
            }
            if (move.FellInto == FallType.None) yield break;

            Feedback?.Fall();
            bool intoHole = move.FellInto == FallType.Hole;
            if (intoHole) Effects.ToyClouds(to, 5, 0.28f);
            else Effects.Dust(to, pushDir, new Color(0.9f, 0.9f, 0.9f, 0.7f), 10);
            yield return view.Fall(intoHole);
            if (!intoHole)
            {
                boxes.Remove(move.BoxId);
                Destroy(view.gameObject);
                yield break;
            }
            // The box stays down in the hole, half visible, so a filled hole never looks like plain floor.
            boxes.Remove(move.BoxId);
            view.SinkStatic();
            filledBy[move.To] = move.BoxType;
            Feedback?.Settle();
            if (tiles.TryGetValue(move.To, out var tile)) yield return tile.PlayFill();
        }

        // ---------- hints ----------

        public void ShowHint(GridPos boxCell, Direction dir)
        {
            ClearHint();
            var offset = dir.ToOffset();
            var pushDir = ToWorld(boxCell + offset) - ToWorld(boxCell);
            hint = new GameObject("Hint").AddComponent<HintMarker>();
            hint.transform.SetParent(content, false);
            hint.Build(ToWorld(boxCell), pushDir, ToWorld(boxCell - offset));
            robot.Think();
        }

        /// <summary>Owner 2026-11-02: the candy hint, an arrow from one colour crate to its neighbour; both hop a few times.</summary>
        public void ShowSwapHint(GridPos cell, Direction dir)
        {
            ClearHint();
            var other = cell + dir.ToOffset();
            hint = new GameObject("Hint").AddComponent<HintMarker>();
            hint.transform.SetParent(content, false);
            hint.Build(ToWorld(cell), ToWorld(other) - ToWorld(cell), ToWorld(cell));
            foreach (var b in lastState.Boxes)
                if ((b.Position == cell || b.Position == other) && boxes.TryGetValue(b.Id, out var view))
                    StartCoroutine(Sequence(view.Hops(0.25f), view.Hops(0.18f), view.Hops(0.12f)));
        }

        public void ClearHint()
        {
            if (hint != null) Destroy(hint.gameObject);
            hint = null;
        }

        // ---------- state-driven looks ----------

        private void AddBox(Box box)
        {
            var view = new GameObject($"Box {box.Id} {box.Type}").AddComponent<BoxView>();
            view.transform.SetParent(content, false);
            view.transform.position = ToWorld(box.Position);
            if (box.Symbol > 0) view.BuildSymbol(box.Id, ComboDecor.ModelSymbol(box));
            else view.Build(box.Id, box.Type);
            if (box.Special != CandySpecial.None) ComboDecor.Apply(view.transform, box.Special);
            if (box.IsCargo) CargoArrow.Add(view.transform).Show(!lastState.IsCandyLevel || lastState.CandyDone);
            boxes[box.Id] = view;
        }

        private void RefreshTargets(GameState state, bool celebrateNew)
        {
            int filled = 0;
            foreach (var tile in tiles.Values)
            {
                if (tile.Type != TileType.Target) continue;
                var box = state.GetBoxAt(tile.Cell);
                bool occupied = box != null;
                if (celebrateNew && occupied && !tile.Occupied)
                {
                    Feedback?.TargetLocked(filled);
                    robot.Cheer();
                    Run(tile.PlayLock());
                    Run(tile.Pulse());
                    if (boxes.TryGetValue(box.Id, out var view)) { Run(view.Flash()); Run(view.Hop()); }
                    // Ana Belge §16: a ring of light spreading on the floor and a few stars.
                    Effects.LightWave(tile.transform.position, Palette.TargetDone);
                    Effects.Stars(tile.transform.position + Vector3.up * 0.5f, 5);
                }
                tile.SetOccupied(occupied);
                if (occupied) filled++;
            }
            FilledTargets = filled;
        }

        /// <summary>Front walls fade while the player or a box stands right behind them.</summary>
        private void RefreshOcclusion(GameState state)
        {
            foreach (var tile in tiles.Values)
            {
                if (!tile.IsFrontWall) continue;
                var behind = new GridPos(tile.Cell.X, tile.Cell.Y - 1);
                tile.SetOccluding(state.PlayerPos == behind || state.GetBoxAt(behind) != null);
            }
        }

        // A wall right in front of (camera side of) a playable cell would hide it.
        private static bool IsFrontWall(GameState state, GridPos cell)
        {
            var behind = state.GetTile(new GridPos(cell.X, cell.Y - 1));
            return behind != TileType.Wall && behind != TileType.Void;
        }

        private static IEnumerator Sequence(params IEnumerator[] steps)
        {
            foreach (var s in steps) yield return s;
        }

        // Every animation runs on this component so Build() can stop them all at once.
        private void Run(IEnumerator routine) => StartCoroutine(Track(routine));

        private IEnumerator Track(IEnumerator routine)
        {
            running++;
            yield return Guard(routine);
            running--;
        }

        /// <summary>
        /// Runs an animation (and the ones it waits for) step by step, so one that fails (owner 2026-11-02: a crate broken and
        /// gone while it still hopped) only ends itself: before, the error stopped the count of running animations and the
        /// worker could not move again until an undo rebuilt the board.
        /// </summary>
        public IEnumerator Guard(IEnumerator routine)
        {
            var stack = new Stack<IEnumerator>();
            stack.Push(routine);
            while (stack.Count > 0)
            {
                var top = stack.Peek();
                bool more;
                try { more = top.MoveNext(); }
                catch (System.Exception e)
                {
                    Debug.LogWarning("[CrateBoss] animation stopped: " + e.Message);
                    break;
                }
                if (!more) { stack.Pop(); continue; }
                if (top.Current is IEnumerator nested) { stack.Push(nested); continue; }
                yield return top.Current;
            }
        }
    }
}
