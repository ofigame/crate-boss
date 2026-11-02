
using System.Collections;
using System.Collections.Generic;
using CrateBoss.Ads;
using CrateBoss.Controls;
using CrateBoss.Core;
using CrateBoss.Levels;
using CrateBoss.Localization;
using CrateBoss.Meta;
using CrateBoss.Solver;
using CrateBoss.UI;
using CrateBoss.View;
using UnityEngine;

namespace CrateBoss.Game
{
    /// <summary>
    /// Input -> RulesEngine (through GameSession) -> BoardView. A move that arrives while an animation
    /// is playing is queued (one slot, newest wins) instead of being dropped. A win pays CB Coin (Ana Belge §13). Until the
    /// Boss's land comes (F7, §11) the game opens on the level list.
    /// </summary>
    public class GameController : MonoBehaviour
    {
        private const float StuckDelay = 2f;
        /// <summary>The stuck search waits until the player pauses: a search after every move made fast play stutter (GC, CPU).</summary>
        private const float AnalyseDelay = 0.8f;
        private float analyseAt = -1f;
        /// <summary>Privacy policy page (Ana Belge §15.4). The page itself still has to be published on the site.</summary>
        public const string PrivacyUrl = "https://ofigame.com/crateboss/gizlilik";
        private static readonly Color AdPurple = new Color(0.52f, 0.33f, 0.86f);

        // "-levels DemoLevels" on the command line plays the test boards instead of the game's levels.
        private readonly LevelLoader loader = new LevelLoader(LevelFolderArg());
        private readonly PositionAnalysis analysis = new PositionAnalysis();
        // The best solution is searched off the main thread: big levels took seconds and froze the game on load.
        private readonly PositionAnalysis bestSearch = new PositionAnalysis(LevelSolver.DefaultMaxStates);

        private List<LevelData> levels;
        /// <summary>Owner 2026-10-31: all levels open for testing. Set to false before release (Docs/YAYIN_KONTROL_LISTESI.md).</summary>
        public const bool OpenAllForTesting = true;

        private GameSession session;
        // Match bonus CB Coin of each move still on the board (Ana Belge §5.4): undo takes it back, so undo-redo cannot farm it; paid on the win.
        private readonly Stack<int> matchBonuses = new Stack<int>();
        private BoardView board;
        private CameraFramer framer;
        private WorldEnvironment environment;
        private PostEffects post;
        private GameFeedback feedback;
        private MusicPlayer music;
        private SwipeInput input;
        private HudView hud;
        private LevelSelectView levelSelect;
        private WinView winView;
        private PromotionView promotion;
        private HomeController home;
        private RectTransform canvas;
        // A win to bring home: a first clear brings its crate, a promotion its moment (Ana Belge §11.3, §11.4).
        private bool pendingDelivery;
        private int pendingPromotion;
        private LanguagePanel languagePanel;
        private SettingsPanel settings;
        private MessageCard message;
        private MetaService meta;
        private readonly AdSessionState adSession = new AdSessionState();
        private AdConfig adConfig;
        private GameObject transition;
        private int lastWinCoins;
        private bool autoPlaying;

        private static double Now => Time.realtimeSinceStartupAsDouble;
        private Camera cam;
        private WorldTheme theme;

        private bool busy;
        private bool won;
        private PlayerAction? pending;
        private bool stuckShown;
        private string startKey;
        private StoredSolution stored;
        private TutorialDirector tutorials;
        private Vector2Int lastScreen;

        public int CurrentLevel { get; private set; }
        public int LevelCount => levels?.Count ?? 0;
        /// <summary>Shortest solution from the level start; null while it is still being searched.</summary>
        public SolveResult Best { get; private set; }
        public bool IsIdle => !busy && pending == null && !board.IsIntroPlaying && !board.IsAnimating;
        public bool IsWon => won;
        public GameState State => session?.State;
        public BoardView Board => board;
        public MetaService Meta => meta;

        private static string LevelFolderArg()
        {
            var args = System.Environment.GetCommandLineArgs();
            int i = System.Array.IndexOf(args, "-levels");
            return i >= 0 && i < args.Length - 1 ? args[i + 1] : LevelLoader.GameFolder;
        }

        private void Start()
        {
            QualityManager.ApplySaved();
            BuildScene();
            levels = loader.LoadAll();
            if (levels.Count == 0)
            {
                Debug.LogError("[CrateBoss] No level files in Resources/" + LevelFolderArg() + ".");
                return;
            }
            meta = new MetaService(levels, ScreenshotTour.Requested);
            ImportOldProgress();
            home.Init(meta, cam, framer, canvas, message, promotion, adSession, NextLevelName, () => board.Effects);
            home.PlayRequested += () => LoadLevel(meta.NextLevel);
            home.LevelsRequested += ShowLevels;
            if (meta.ClearedCount == 0 && !ScreenshotTour.Requested) LoadLevel(0); // Ana Belge §11.1: a new player starts in level 1
            else ShowHome();
            if (!ScreenshotTour.Requested) ShowLoginGift();
        }

        // Players of the earlier builds keep what they cleared (ProgressStore counted levels by index).
        private void ImportOldProgress()
        {
            if (meta.Data.levels.Count > 0 || ScreenshotTour.Requested) return;
            int done = Mathf.Min(ProgressStore.Unlocked - 1, levels.Count);
            if (done <= 0) return;
            for (int i = 0; i < done; i++)
            {
                var rec = meta.Data.Level(MetaService.Key(levels[i]));
                rec.cleared = true;
                rec.stars = ProgressStore.GetStars(i);
            }
            meta.Save();
        }

        // Login streak (Ana Belge §13.1): the first visit of the day pays the streak day's CB Coin.
        private void ShowLoginGift()
        {
            int gift = DailyRules.ClaimLogin(meta.Data, meta.Config, System.DateTime.Now);
            if (gift <= 0) return;
            meta.Wallet.Add(gift);
            meta.Save();
            int last = meta.Config.loginStreak[meta.Config.loginStreak.Length - 1];
            var options = new List<CardOption> { new CardOption(Loc.T("gift.take"), null, UiFactory.AccentColor) };
            if (DailyRules.GiftAdAvailable(meta.Data, System.DateTime.Now))
                options.Add(new CardOption(Loc.F("gift.watch", gift), () => WatchGiftAd(gift), AdPurple, true, IconKind.Ad));
            message.Show(IconKind.Calendar, Loc.F("gift.title", meta.Data.streakDay), Loc.F("gift.body", gift, last), options.ToArray());
        }

        private void WatchGiftAd(int gift)
        {
            RewardedAds.Show(AdServiceLocator.Rewarded, AdPlacement.DailyGift, adSession, () => Now, () =>
            {
                DailyRules.UseGiftAd(meta.Data, System.DateTime.Now);
                meta.Wallet.Add(gift);
                meta.Save();
                message.Show(IconKind.Coin, Loc.T("gift.second.title"), Loc.F("gift.second.body", gift), Loc.T("ui.ok"), null);
            }, NoAd);
        }

        // Main Ana Belge §18: the choice is saved; every screen is built again in the new language (texts are set when built).
        private void ChangeLanguage(string code)
        {
            Loc.Set(code);
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }

        // Ana Belge §15.1: the ad could not load. Nothing is given, no error is shown.
        private void NoAd() => message.Show(IconKind.Ad, Loc.T("ad.none.title"), Loc.T("ad.none.body"), Loc.T("ui.ok"), null);

        private readonly List<MachineTagInfo> craneTags = new List<MachineTagInfo>();
        private readonly List<Vector2?> craneBadgePoints = new List<Vector2?>();

        private void LateUpdate()
        {
            // One chip per active crane in the top bar, and a badge next to each drop cell.
            craneTags.Clear();
            craneBadgePoints.Clear();
            bool showBadges = !won && board != null && !levelSelect.IsOpen && !board.IsIntroPlaying;
            if (board != null && !won)
                foreach (var crane in board.Cranes)
                {
                    if (!crane.Active) continue;
                    craneTags.Add(new MachineTagInfo(crane.Id, crane.Counter, crane.Waiting));
                    craneBadgePoints.Add(showBadges ? (Vector2?)cam.WorldToScreenPoint(crane.BadgeAnchor) : null);
                }
            AddGoalChips(craneTags);
            hud.SetCraneChips(craneTags);
            hud.SetCraneBadges(craneTags, craneBadgePoints);
        }

        // Goal chips (Ana Belge §17.1): trucks still to fill, deliveries done, car bumps.
        private void AddGoalChips(List<MachineTagInfo> tags)
        {
            var s = session?.State;
            if (s == null || won) return;
            if (s.Truck != null) tags.Add(new MachineTagInfo(s.TrucksLeft.ToString(), -1, false, IconKind.Truck));
            if (s.Deliveries.Count > 0)
            {
                int done = 0;
                foreach (var b in s.Boxes) if (b.Locked) done++;
                tags.Add(new MachineTagInfo(done + "/" + s.Deliveries.Count, -1, false, IconKind.Check));
            }
            if (s.Parking != null) tags.Add(new MachineTagInfo(session.Collisions.ToString(), -1, false, IconKind.Exclaim));
        }

        /// <summary>One line for the goal card shown when the level starts (Ana Belge §17.1).</summary>
        private static string TaskText(GameState s)
        {
            var parts = new List<string>();
            if ((s.Goal & GoalType.Trucks) != 0) parts.Add(s.TrucksLeft > 1 ? Loc.F("goal.trucks", s.TrucksLeft) : Loc.T("goal.truck"));
            if ((s.Goal & GoalType.Targets) != 0 && s.Truck == null) parts.Add(Loc.T("goal.targets"));
            if ((s.Goal & GoalType.Deliver) != 0) parts.Add(Loc.T("goal.deliver"));
            if (s.HasSymbols) parts.Add(Loc.T("goal.match"));
            if (parts.Count == 0) return null;
            return Loc.Capitalize(string.Join(Loc.T("goal.sep") + (Loc.Code == "ja" ? "" : " "), parts));
        }

        private void Update()
        {
            // Menus do not need 60 FPS (battery, heat).
            Application.targetFrameRate = levelSelect != null && (levelSelect.IsOpen) ? 30 : 60;
            if (lastScreen.x != Screen.width || lastScreen.y != Screen.height)
            {
                lastScreen = new Vector2Int(Screen.width, Screen.height);
                UpdateCameraInsets();
            }
            if (Best == null && bestSearch.IsDone)
            {
                Best = bestSearch.Result;
                if (session != null && !won) hud.SetMoves(session.State.PushCount, BestMoves);
            }
            if (analyseAt >= 0f && !busy && session != null && Time.time >= analyseAt)
            {
                analyseAt = -1f;
                analysis.Start(session.State, Time.time);
            }
            // Stuck for 2 s with no way back to a win: gently point at Undo.
            if (!stuckShown && !won && !busy && analysis.IsStuck && Time.time - analysis.StartedAt >= StuckDelay)
            {
                stuckShown = true;
                hud.GlowUndo(true);
            }
        }

        public void LoadLevel(int index)
        {
            CurrentLevel = index;
            Play(levels[index]);
        }

        private void Play(LevelData data)
        {
            StopMoves();
            home?.Hide();
            hud.Show(true);
            board.gameObject.SetActive(true);
            VisualCatalog.RankSuffix = data.league > 0 ? meta.Ranks.Get(data.league).ModelSuffix : ""; // Ana Belge §7: the rank's crates
            promotion.Hide();
            message.Hide();
            GameState start;
            try { start = loader.BuildState(data); }
            catch (LevelFormatException e)
            {
                Debug.LogError($"[CrateBoss] {e.Message}");
                return;
            }
            // Owner 2026-11-02: every try of a candy level gets other crane drops (the board itself is the level's own).
            if (start.IsCandyLevel && !ScreenshotTour.Requested) start.CandyRng = (uint)Random.Range(1, int.MaxValue);
            session = new GameSession(start);
            levelStart = start.Clone();
            StopAutoHammers();
            hitCrate = -1; hitsInRow = 0;
            board.WorkerLocked = false;
            input.PanFirst = false;
            adHints = 0;
            matchBonuses.Clear();
            Best = null;
            startKey = start.GetKey();
            // Candy levels (owner 2026-11-01): random refills, so no stored way, no search and no hints.
            stored = start.IsCandyLevel ? null : StoredSolution.From(data.solution, start);
            if (stored != null) Best = stored.Result;
            else if (!start.IsCandyLevel) bestSearch.Start(start, Time.time);
            hud.SetCandy(start.IsCandyLevel, 0, start.CandyDropsLeft);
            won = false;
            winView.Hide();
            levelSelect.Hide();
            BackdropBlur.Clear();
            StartCoroutine(post.BlurBackground(false, 0.01f));

            theme = WorldThemes.Current;
            DistrictBuilder.CraneMasts.Clear();
            foreach (var c in start.Cranes) DistrictBuilder.CraneMasts.Add(CraneView.MastAt(start, c.Side));
            DistrictBuilder.BackRoad = start.IsCandyLevel;
            DistrictBuilder.LeftRoad = DistrictBuilder.FrontRoad = false;
            if (start.IsCandyLevel)
                foreach (var bay in start.Bays)
                {
                    var outward = TruckView.Outward(bay, c => new Vector3(c.X, 0f, start.Height - 1 - c.Y), start.Width, start.Height);
                    if (outward == Vector3.left) DistrictBuilder.LeftRoad = true;
                    if (outward == Vector3.back) DistrictBuilder.FrontRoad = true;
                }
            environment.Apply(theme, start.Width, start.Height, cam, QualityManager.ParticleScale, data.id);
            DistrictBuilder.BackRoad = DistrictBuilder.LeftRoad = DistrictBuilder.FrontRoad = false;
            post.ApplyTheme(theme);
            music.PlayWorld(theme.Id);

            board.ClearHistory();
            board.Seed = data.id;
            Outfit.Set(meta.Data); // wardrobe pieces (Ana Belge §13.2)
            board.Build(session.State, theme);
            hud.ShowTaskCard(TaskText(start));
            UpdateCameraInsets();
            framer.minCellWidth = PlayCell; // a close-up of the last level ends here
            framer.Peek(false);
            FrameYard(start, 0f, true);
            UpdateCameraInsets();
            Debug.Log($"[Frame] {data.name} cell={framer.CellPixels(Screen.width):F1}px");
            music.SetIntensity(board.FilledTargets);
            hud.SetLevel(CurrentLevel + 1, Loc.Level(data.id, data.name), data.league > 0 ? Loc.T("league." + data.league) : "", theme.SkyTop.grayscale < 0.4f);
            hud.SetMoves(0, BestMoves);
            hud.SetHints(meta.Data.hints);
            board.StartCoroutine(board.PlayIntro());
            tutorials.Start(start, stored);
            Analyse();
        }

        // The camera frames the yard cells only; the pavement ring and the district spill past the screen edges.
        /// <param name="seconds">0 = at once; otherwise the camera eases to the new area.</param>
        /// <param name="flyIn">Level start: begin from a wide view of the yard, the cranes and the city round it, then glide in close
        /// (owner 2026-10-31: "ilk bölümler gibi yakından olsun, bölüme girerken bu açıdan yakına gelsin").</param>
        private void FrameYard(GameState s, float seconds = 0f, bool flyIn = false)
        {
            int minX = s.Width, maxX = -1, minY = s.Height, maxY = -1;
            for (int y = 0; y < s.Height; y++)
            for (int x = 0; x < s.Width; x++)
            {
                var cell = new GridPos(x, y);
                if (!BoundaryMap.IsYard(s.GetTile(cell))) continue;
                minX = Mathf.Min(minX, x); maxX = Mathf.Max(maxX, x);
                minY = Mathf.Min(minY, y); maxY = Mathf.Max(maxY, y);
            }
            if (maxX < 0) { framer.Frame(s.Width, s.Height); return; }
            // Grid row y sits at world z = Height - 1 - y; a little extra round the edge for the barrier.
            float x0 = minX - 0.45f, x1 = maxX + 0.45f, z0 = s.Height - 1 - maxY - 0.45f, z1 = s.Height - 1 - minY + 0.45f;
            if (flyIn)
            {
                // Owner 2026-11-02: the level opens high over the whole city, looking straight down, and the camera glides down
                // into the play view (the yard whole on the screen).
                framer.Frame(x0, x1, z0, z1);
                framer.FlyIn(2f);
                return;
            }
            framer.Frame(x0, x1, z0, z1, seconds);
        }

        /// <summary>Each crane (tower and jib) as a box, so the camera keeps all of them on screen.</summary>
        private Bounds[] CraneBounds()
        {
            var list = new List<Bounds>();
            foreach (var crane in board.Cranes)
            {
                if (crane == null) continue;
                var renderers = crane.GetComponentsInChildren<Renderer>();
                if (renderers.Length == 0) continue;
                var b = renderers[0].bounds;
                foreach (var r in renderers) b.Encapsulate(r.bounds);
                list.Add(b);
            }
            return list.ToArray();
        }

        public void RequestMove(Direction dir) => RequestAction(dir);

        /// <summary>A touch on a parked car makes it wobble: that is the car a swipe from here sends.</summary>
        private void OnPressed(Vector2 pos)
        {
            var s = session?.State;
            if (s?.Parking == null || won) return;
            var cell = board.CellAtScreen(cam, pos);
            int car = cell.HasValue ? s.CarAt(cell.Value) : -1;
            if (car >= 0) board.PulseCar(car);
        }

        /// <summary>
        /// A tap: on the worker it locks / unlocks him (owner 2026-11-02: locked, drags slide the view); on a numbered
        /// (breakable) crate right next to him it hits it as walking into it would; on one further off it hits it with an
        /// advanced hammer when there is one.
        /// </summary>
        private void OnTapped(Vector2 pos)
        {
            var s = session?.State;
            if (s == null || won) return;
            foreach (var height in new[] { 0.8f, 0.45f, 0f })
            {
                var c = board.CellAtScreen(cam, pos, height);
                if (!c.HasValue) continue;
                if (c.Value == s.PlayerPos) { ToggleLock(); return; }
                var box = s.GetBoxAt(c.Value);
                if (box != null && s.IsCandyLevel && box.Hits <= 0)
                {
                    // Owner 2026-11-02: a tap on a crate near the screen's edge brings that side of the yard into view (with the
                    // worker locked, any tap centres the view on the spot).
                    var world = board.CellWorld(c.Value);
                    var v = cam.WorldToViewportPoint(world);
                    if (board.WorkerLocked || framer.Follows && (v.x < 0.22f || v.x > 0.78f || v.y < 0.25f || v.y > 0.72f)) framer.LookAt(world);
                    return;
                }
                if (box == null || box.Hits <= 0) continue;
                var d = c.Value - s.PlayerPos;
                if (System.Math.Abs(d.X) + System.Math.Abs(d.Y) == 1)
                {
                    RequestMove(d.X > 0 ? Direction.Right : d.X < 0 ? Direction.Left : d.Y > 0 ? Direction.Down : Direction.Up);
                    return;
                }
                if (s.SuperHammers > 0) SuperHit(c.Value);
                else if (board.WorkerLocked) framer.LookAt(board.CellWorld(c.Value));
                return;
            }
            // Owner 2026-11-02: with the worker locked, a tap anywhere brings that spot to the middle of the view.
            if (board.WorkerLocked && framer.GroundAt(pos) is Vector3 ground) framer.LookAt(ground);
        }

        // An advanced hammer on a crate anywhere (owner 2026-11-02): one hit, no walking; an undo step of its own.
        private void SuperHit(GridPos cell)
        {
            if (busy) return;
            var result = session.Move(PlayerAction.HitAt(session.State.Index(cell), true));
            if (!result.Success) return;
            board.PlayRemoteHit(result, session.State, 0.16f, true);
            RefreshCandy();
        }

        // Owner 2026-11-02: the worker locked by a tap; a lock bobs over him and drags slide the view instead of moving him.
        private void ToggleLock()
        {
            board.WorkerLocked = !board.WorkerLocked;
            input.PanFirst = board.WorkerLocked;
            feedback.Pop();
            tutorials.OnLockToggled();
        }

        // Where a touch acts even with the worker locked: on him, a colour crate or a numbered crate.
        private bool ActsAt(Vector2 pos)
        {
            var s = session?.State;
            if (s == null) return false;
            foreach (var height in new[] { 0.8f, 0.45f, 0f })
            {
                var c = board.CellAtScreen(cam, pos, height);
                if (!c.HasValue) continue;
                if (c.Value == s.PlayerPos) return true;
                var box = s.GetBoxAt(c.Value);
                if (box != null && (CandySystem.IsCandy(box) || box.Hits > 0)) return true;
            }
            return false;
        }

        /// <summary>A flick: started on a parked car and along its axis it sends the car, otherwise the worker moves.</summary>
        private void OnSwipe(Vector2 start, Direction dir)
        {
            var s = session?.State;
            if (s != null && s.IsCandyLevel)
            {
                // Candy levels: a flick that starts on a colour crate swaps it with its neighbour that way. The finger lands on
                // the crate's lid or front, not on the floor: look from the lid down (owner 2026-11-01: a crane over a crate must
                // not get in the way).
                GridPos? at = null;
                foreach (var height in new[] { 0.8f, 0.45f, 0f })
                {
                    var c = board.CellAtScreen(cam, start, height);
                    if (c.HasValue && CandySystem.IsCandy(s.GetBoxAt(c.Value))) { at = c; break; }
                }
                if (at.HasValue)
                {
                    RequestAction(PlayerAction.SwapCandy(s.Index(at.Value), dir));
                    return;
                }
            }
            if (s?.Parking != null)
            {
                var cell = board.CellAtScreen(cam, start);
                int car = cell.HasValue ? s.CarAt(cell.Value) : -1;
                if (car >= 0)
                {
                    if (s.Parking.Cars[car].AlongAxis(dir)) RequestAction(PlayerAction.SendCar(car, dir));
                    return; // across the car: nothing happens (Ana Belge §5.5)
                }
            }
            RequestMove(dir);
        }

        /// <summary>A walk / push, or sending a parked car (Ana Belge §5.5).</summary>
        public void RequestAction(PlayerAction dir)
        {
            if (session == null || won || levelSelect.IsOpen || autoPlaying || message.IsOpen) return;
            // A look round the yard ends with the next walk; sending a car or swapping candy keeps the view (owner 2026-11-01).
            if (!dir.IsCar && !dir.IsSwap) framer.ReturnPan();
            if (board.IsIntroPlaying) board.SkipIntro(session.State);
            if (busy)
            {
                pending = dir;
                return;
            }
            StartCoroutine(DoMove(dir));
        }

        private static int CraneDropsDone(GameState state)
        {
            int n = 0;
            foreach (var c in state.CraneStates) n += c.Index;
            return n;
        }

        public void Undo()
        {
            if (session == null || won || autoPlaying || session.UndoCount == 0) return;
            StopMoves();
            StopAutoHammers();
            int dropsBefore = CraneDropsDone(session.State);
            session.Undo();
            if (matchBonuses.Count > 0) matchBonuses.Pop();
            if (CraneDropsDone(session.State) < dropsBefore) feedback.CraneRewind();
            board.Build(session.State, theme);
            hud.SetMoves(session.State.PushCount, BestMoves);
            music.SetIntensity(board.FilledTargets);
            Analyse();
            RefreshCandy();
        }

        /// <summary>Restart builds the level again from its file data.</summary>
        public void Restart()
        {
            if (levels == null || levels.Count == 0 || session == null) return;
            LoadLevel(CurrentLevel);
        }

        /// <summary>Hint: show the next box push of the shortest solution from here (free hints per day).</summary>
        public void RequestHint()
        {
            if (session == null || won || busy || autoPlaying) return;
            if (session.State.IsCandyLevel) { RequestCandyHint(); return; }
            if (meta.Data.hints > 0) { StartCoroutine(ShowHint(0, true)); return; }
            // Free hints used up (Ana Belge §14; owner 2026-11-02): in a level the first hint comes with a rewarded ad, the ones
            // after it cost CB Coin; watching the whole solution costs CB Coin and an ad.
            int price = meta.Config.hint, solution = meta.Config.watchSolution, balance = meta.Wallet.Balance;
            var hint = adHints == 0
                ? new CardOption(Loc.T("hint.ad"), WatchHintAd, AdPurple, true, IconKind.Ad)
                : new CardOption(Loc.F("hint.buy", price), () => StartCoroutine(ShowHint(price, false)), UiFactory.AccentColor, balance >= price, IconKind.Coin);
            message.Show(IconKind.Hint, Loc.T("hint.none.title"), Loc.F("hint.none.body", balance), hint,
                new CardOption(Loc.F("hint.solution.ad", solution), WatchSolutionAd, UiFactory.ButtonColor, balance >= solution, IconKind.Ad));
        }

        // Owner 2026-11-02: on a candy level the hint shows a swap that pays the most hammers now; it costs what a hint costs
        // (a free one, else the first by an ad and the next ones by CB Coin).
        private void RequestCandyHint()
        {
            var s = session.State;
            if (s.CandyDone || !CandySystem.BestSwap(s).HasValue) { hud.ShowTaskCard(Loc.T("candy.nohint")); return; }
            if (meta.Data.hints > 0) { meta.Data.hints--; meta.Save(); hud.SetHints(meta.Data.hints); ShowCandyHint(); return; }
            int price = meta.Config.hint, balance = meta.Wallet.Balance;
            var option = adHints == 0
                ? new CardOption(Loc.T("hint.ad"), () => RewardedAds.Show(AdServiceLocator.Rewarded, AdPlacement.HintRewarded, adSession, () => Now, () => { adHints++; ShowCandyHint(); }, NoAd), AdPurple, true, IconKind.Ad)
                : new CardOption(Loc.F("hint.buy", price), () => { if (meta.Wallet.TrySpend(price)) { meta.Save(); ShowCandyHint(); } }, UiFactory.AccentColor, balance >= price, IconKind.Coin);
            message.Show(IconKind.Hint, Loc.T("hint.none.title"), Loc.F("hint.none.body", balance), option);
        }

        private void ShowCandyHint()
        {
            var swap = CandySystem.BestSwap(session.State);
            if (!swap.HasValue) return;
            board.ShowSwapHint(session.State.CellAt(swap.Value.Swap), swap.Value.Direction);
            hud.ShowTaskCard(Loc.T("candy.hintswap"));
        }

        // Hints this level paid for with an ad (the first one; then CB Coin).
        private int adHints;

        private void WatchHintAd() =>
            RewardedAds.Show(AdServiceLocator.Rewarded, AdPlacement.HintRewarded, adSession, () => Now, () => { adHints++; StartCoroutine(ShowHint(0, false)); }, NoAd);

        // The whole solution: the ad first, then the CB Coin (spent only when the solution plays).
        private void WatchSolutionAd() =>
            RewardedAds.Show(AdServiceLocator.Rewarded, AdPlacement.HintRewarded, adSession, () => Now, () => StartCoroutine(PlaySolution()), NoAd);

        /// <summary>The shortest solution from the current position (the level start's one is searched on load).</summary>
        private IEnumerator FindSolution(System.Action<SolveResult> done)
        {
            // A stored solution answers from any position on its way (hard levels: the device search would give up).
            var rest = stored?.From(session.State.GetKey());
            if (rest != null) { done(rest); yield break; }
            // Off its way on a stored-solution level the device search is too small to help: undo back onto the way.
            if (stored != null) { done(null); yield break; }
            if (session.State.GetKey() == startKey)
            {
                while (Best == null) yield return null;
                done(Best);
                yield break;
            }
            if (analysis.Position == null || !analysis.Position.SameAs(session.State)) AnalyseNow();
            while (!analysis.IsDone) yield return null;
            done(analysis.Result);
        }

        // "Çözümü izle" (Ana Belge §14): plays the whole solution from here, the last resort for a stuck player.
        private IEnumerator PlaySolution()
        {
            SolveResult result = null;
            yield return FindSolution(r => result = r);
            if (result == null || !result.IsSolved)
            {
                hud.GlowUndo(true);
                message.Show(IconKind.Undo, Loc.T("nosolution.title"), Loc.T("nosolution.paid"), Loc.T("ui.ok"), null);
                yield break;
            }
            if (!meta.Wallet.TrySpend(meta.Config.watchSolution)) yield break;
            meta.Save();
            autoPlaying = true;
            foreach (var dir in result.Moves)
            {
                while (busy || board.IsAnimating || board.IsIntroPlaying) yield return null;
                if (won) break;
                yield return new WaitForSeconds(0.12f);
                StartCoroutine(DoMove(dir));
                yield return null;
            }
            autoPlaying = false;
        }

        public void ShowLevels()
        {
            tutorials?.Stop();
            winView.Hide();
            promotion.Hide();
            message.Hide();
            var worlds = new List<string>();
            bool leagues = LeagueRules.IsLeagueSet(levels);
            for (int i = 0; i < levels.Count; i++)
                worlds.Add(leagues ? LeagueRules.LeagueOf(levels, i).ToString() : ""); // one group per rank; test folders have none
            AdServiceLocator.Banner.Hide(); // the banner is for the home screen; the list closes back to it
            // Test folders (-levels CandidateLevels / DemoLevels) open every level: they are for trying boards, not progress.
            // OpenAllForTesting (owner 2026-10-31): every yard open in the main game too while the owner tests.
            int open = OpenAllForTesting || LevelFolderArg() != LevelLoader.GameFolder ? levels.Count : meta.Unlocked;
            levelSelect.Show(worlds, open, i => meta.Stars(levels[i]), CurrentLevel, i => LevelThumbnail.Get(levels[i]), i => LevelLabel(i),
                leagues ? key => LeagueHeader(int.Parse(key), open) : ((System.Func<string, (int, string, int, int, int, bool)>)null), i => LevelThumbnail.Size(levels[i]));
        }

        // A league band in the level list: its name, the stars won of all, and (closed) the stars it needs from the one before.
        private (int, string, int, int, int, bool) LeagueHeader(int league, int open)
        {
            int first = 0;
            while (first < levels.Count && LeagueRules.LeagueOf(levels, first) != league) first++;
            return (league, Loc.T("league." + league), LeagueRules.StarsIn(levels, league, l => meta.Stars(l)), 3 * LeagueRules.LevelsIn(levels, league),
                LeagueRules.StarsNeeded(levels, league), first >= open);
        }

        // Owner 2026-11-01: only pushes (and cars sent) are moves, so the goal is the pushes of the best known solution.
        private SolveResult countedBest;
        private int bestPushes = -1;
        private GameState levelStart;

        private int BestMoves
        {
            get
            {
                if (Best == null || !Best.IsSolved || levelStart == null) return -1;
                if (countedBest != Best)
                {
                    countedBest = Best;
                    var s = levelStart.Clone();
                    var rules = new RulesEngine();
                    foreach (var m in Best.Moves) rules.TryMove(s, m);
                    bestPushes = s.PushCount;
                }
                return bestPushes;
            }
        }

        // ---------- candy levels (owner 2026-11-01) ----------


        private void RefreshCandy()
        {
            var s = session?.State;
            if (s == null || !s.IsCandyLevel) return;
            hud.SetCandy(true, s.Hammers, s.CandyDropsLeft);
            hud.SetSuper(s.SuperHammers);
            hud.SetPaint(s.PaintGuns);
        }

        // The paint gun (owner 2026-11-02): paint flies from the worker onto random colour crates; what lines up sinks.
        private void UsePaint()
        {
            var s = session?.State;
            if (s == null || won || s.PaintGuns <= 0) return;
            RequestAction(PlayerAction.UsePaint());
        }

        /// <summary>Hammers fly from the sunk group to the counter on the right and count up as they land.</summary>
        private void OnHammersEarned(int hammers, Vector3 at)
        {
            var s = session?.State;
            if (s == null || !s.IsCandyLevel) return;
            hud.FlyHammers(cam.WorldToScreenPoint(board.CellWorldPoint(at)), hammers);
        }

        /// <summary>
        /// The worker hit a breakable crate with no hammer: while swaps are left, a reminder; with none left the level is lost
        /// (owner 2026-11-01): a rewarded ad gives five hammers, or play it again.
        /// </summary>
        /// <summary>Hammers a rewarded ad gives when they ran out (owner 2026-11-02: 15, as a hit now comes for every sunk crate).</summary>
        public const int AdHammers = 15;

        private void NoHammer()
        {
            var s = session.State;
            if (CandySystem.HasSwap(s)) { hud.ShowTaskCard(Loc.T("candy.needhammer")); return; }
            message.Show(IconKind.Hammer, Loc.T("candy.out.title"), Loc.T("candy.out.body"),
                new CardOption(Loc.T("candy.out.ad"), () => RewardedAds.Show(AdServiceLocator.Rewarded, AdPlacement.HintRewarded, adSession, () => Now,
                    () => { session.State.Hammers += AdHammers; RefreshCandy(); }, NoAd), AdPurple, true, IconKind.Ad),
                new CardOption(Loc.T("candy.out.retry"), Restart, UiFactory.ButtonColor, true, IconKind.Restart));
        }

        /// <summary>The play view's smallest cell (share of the screen width), and the close-up after the candy (owner 2026-11-02).</summary>
        private const float PlayCell = 0.075f, CloseCell = 0.15f;

        // Owner 2026-11-02: hits on the same breakable crate in a row; from the third on a hammer of its own goes on hitting it.
        private int hitCrate = -1, hitsInRow;
        private const int AutoHitAfter = 3;
        private readonly Dictionary<int, Coroutine> autoHammers = new Dictionary<int, Coroutine>();

        private void StartAutoHammer(int boxId)
        {
            if (autoHammers.ContainsKey(boxId)) return;
            hitCrate = -1;
            hitsInRow = 0;
            autoHammers[boxId] = StartCoroutine(AutoHammer(boxId));
        }

        // Blow after blow until the crate breaks or the hammers run out; the more hits the crate needs, the faster (the whole
        // crate in about one and a half seconds, owner 2026-11-02 "çok hızlı").
        private IEnumerator AutoHammer(int boxId)
        {
            var first = session.State.GetBoxById(boxId);
            float gap = first == null ? 0.1f : Mathf.Clamp(1.5f / first.Hits, 0.035f, 0.16f);
            while (!won)
            {
                yield return new WaitForSeconds(gap);
                var s = session.State;
                var box = s.GetBoxById(boxId);
                if (box == null || box.Hits <= 0) break;
                var result = session.Move(PlayerAction.HitAt(s.Index(box.Position), false), false);
                if (!result.Success) { if (result.NoHammer) NoHammer(); break; }
                board.PlayRemoteHit(result, session.State, Mathf.Min(gap * 0.95f, 0.14f), false);
                RefreshCandy();
                if (result.Broke) break;
                gap = Mathf.Max(0.03f, gap * 0.97f);
            }
            board.StopAutoHammer(boxId);
            autoHammers.Remove(boxId);
        }

        /// <summary>Undo, restart, a new level: every auto hammer stops (the crates they hit are back as they were).</summary>
        private void StopAutoHammers()
        {
            foreach (var kv in autoHammers) { if (kv.Value != null) StopCoroutine(kv.Value); board.StopAutoHammer(kv.Key); }
            autoHammers.Clear();
        }

        private IEnumerator DoMove(PlayerAction dir)
        {
            busy = true;
            ClearStuck();
            while (true)
            {
                var result = session.Move(dir);
                if (result.Success)
                {
                    if (result.HitBox >= 0 && result.HitBox == hitCrate) hitsInRow++;
                    else { hitCrate = result.HitBox; hitsInRow = result.HitBox >= 0 ? 1 : 0; }
                    // The third hit in a row hands the crate to its own hammer; the worker is free to go on (owner 2026-11-02).
                    if (hitsInRow >= AutoHitAfter && !result.Broke && session.State.Hammers > 0) StartAutoHammer(result.HitBox);
                    matchBonuses.Push(result.MatchBonus);
                    if (result.MatchBonus > 0) hud.ShowTaskCard(Loc.F("match.bonus", result.MatchBonus));
                    hud.SetMoves(session.State.PushCount, BestMoves);
                    // Owner 2026-11-02: combos, long chains and the paint gun lift the view to the whole yard for a moment.
                    int waves = 0; bool blast = false;
                    foreach (var w in result.CandyWaves) { if (w.Sunk.Count > 0) waves++; if (w.Blasts.Count > 0) blast = true; }
                    bool peek = blast || waves >= 3 || result.Painted.Count > 0;
                    if (peek) framer.Peek(true);
                    yield return board.Guard(board.PlayMove(result, session.State)); // a failed animation never locks the worker
                    if (peek) framer.Peek(false);
                    RefreshCandy();
                    // Owner 2026-11-02: when the candy is over the camera comes close to the worker and follows him.
                    if (result.CandyFinished) framer.SetCellWidth(CloseCell, 1.2f);
                    if (result.PaintEarned) hud.ShowTaskCard(Loc.T("candy.paintgun"));
                    foreach (var w in result.CandyWaves) if (w.SuperHammers > 0) { hud.ShowTaskCard(Loc.T("candy.super")); break; }
                    if (result.CandyFinished) hud.ShowTaskCard(Loc.T("candy.done"));
                    tutorials.OnMoved(session.State, result);
                    music.SetIntensity(board.FilledTargets);
                    if (result.IsWon)
                    {
                        pending = null;
                        busy = false;
                        StartCoroutine(WinSequence());
                        yield break;
                    }
                }
                else if (result.Car != null)
                {
                    hud.SetMoves(session.State.PushCount, BestMoves);
                    yield return board.PlayCarBump(result.Car);
                }
                else if (dir.IsSwap)
                {
                    var from = session.State.CellAt(dir.Swap);
                    yield return board.PlaySwap(from, from + dir.Direction.ToOffset(), true);
                }
                else if (!dir.IsCar)
                {
                    yield return board.PlayBump(dir.Direction);
                    if (result.NoHammer) NoHammer();
                }

                if (pending == null) break;
                dir = pending.Value;
                pending = null;
            }
            busy = false;
            Analyse();
        }

        /// <param name="cost">CB Coin to pay (0 = none).</param>
        /// <param name="useFree">Takes one of the free hints; neither = earned by a rewarded ad.</param>
        private IEnumerator ShowHint(int cost, bool useFree)
        {
            SolveResult result = null;
            yield return FindSolution(r => result = r);
            if (result == null || !result.IsSolved || result.MoveCount == 0)
            {
                hud.GlowUndo(true);
                if (cost == 0 && !useFree)
                {
                    // The ad was watched but there is nothing to show from here: keep the hint for later.
                    meta.Data.hints++;
                    meta.Save();
                    hud.SetHints(meta.Data.hints);
                }
                int back = UndoNeeded();
                message.Show(IconKind.Undo, Loc.T("nosolution.title"), back > 0 ? Loc.F("hint.undo", back) : Loc.T("nosolution.hint"), Loc.T("ui.ok"), null);
                yield break;
            }
            int push = LevelAnalyzer.FirstPushIndex(session.State, result.Moves);
            if (push < 0) yield break;
            var rules = new RulesEngine();
            var s = session.State.Clone();
            for (int i = 0; i < push; i++) rules.TryMove(s, result.Moves[i]);
            var next = result.Moves[push];
            if (cost > 0 ? !meta.Wallet.TrySpend(cost) : useFree && meta.Data.hints <= 0) yield break;
            if (useFree) meta.Data.hints--;
            meta.Save();
            hud.SetHints(meta.Data.hints);
            if (next.IsCar) board.ShowCarHint(next.Car, next.Direction);
            else board.ShowHint(s.PlayerPos + next.Direction.ToOffset(), next.Direction);
        }

        /// <summary>
        /// Ana Belge §14: how many moves back the position can be won again (the undo states are tried from the newest), or 0
        /// when it cannot be worked out quickly. Never a wrong number: only positions proven winnable count.
        /// </summary>
        private int UndoNeeded()
        {
            int n = 0;
            foreach (var past in session.History)
            {
                n++;
                if (n > 30) break;
                if (stored != null) { if (stored.From(past.GetKey()) != null) return n; continue; }
                var r = new LevelSolver(30000) { UseAStar = true, MaxSeconds = 0.3 }.Solve(past);
                if (r.IsSolved) return n;
            }
            return 0;
        }

        private IEnumerator WinSequence()
        {
            won = true;
            tutorials.Stop();
            ClearStuck();
            // Candy levels: stars for the hammer hits left over (owner 2026-11-01).
            int stars = session.State.IsCandyLevel ? (session.State.Hammers >= 3 ? 3 : session.State.Hammers >= 1 ? 2 : 1)
                : StarRating.Stars(session.State.PushCount, BestMoves, session.Collisions, session.State.Parking != null, stored == null);
                ProgressStore.Unlock(CurrentLevel + 2);
                ProgressStore.SetStars(CurrentLevel, stars);
                var reward = RewardRules.Win(meta.Config, meta.Data.Level(MetaService.Key(levels[CurrentLevel])), stars);
                int matchCoins = 0;
                foreach (var b in matchBonuses) matchCoins += b;
                meta.Wallet.Add(reward.Total + matchCoins);
                winView.SetReward(reward.Clear + matchCoins, reward.Better);
                lastWinCoins = reward.Total + matchCoins;
                if (reward.FirstClear) { meta.Data.crates++; pendingDelivery = true; } // Ana Belge §12.1: a delivery crate for every first clear
                int promoted = meta.CheckPromotion();
                if (promoted > 0) pendingPromotion = promoted;
                AdPolicy.LevelFinished(adSession);
            meta.Save();
            feedback.Win();
            StartCoroutine(framer.ZoomIn(0.88f, 0.6f));
            yield return board.PlayWin();
            yield return post.BlurBackground(true);
            winView.Show(session.State.PushCount, BestMoves, stars, CurrentLevel >= levels.Count - 1);
        }

        // Owner 2026-11-01: "Sonraki" plays the next level (the delivery crate waits on the land). A promotion, the last level or a
        // rank gate still go home. The interstitial (when due) plays on the way.
        private void OnNext()
        {
            int next = CurrentLevel + 1;
            if (pendingPromotion > 0 || next >= levels.Count || next > meta.NextLevel) next = -1;
            if (AdPolicy.CanShowInterstitial(adConfig, adSession, meta.ClearedCount, Now) && AdServiceLocator.Interstitial.IsReady)
                StartCoroutine(InterstitialThenLoad(next));
            else if (next < 0) ShowHome();
            else LoadLevel(next);
        }

        // Ana Belge §15.2: only here, after "Sonraki" on the win card; a short "next yard" screen first so a tap meant for a
        // button cannot land on the ad.
        private IEnumerator InterstitialThenLoad(int next)
        {
            winView.Hide();
            promotion.Hide();
            transition.SetActive(true);
            transition.transform.SetAsLastSibling();
            yield return new WaitForSecondsRealtime(adConfig.transitionSeconds);
            AdPolicy.InterstitialShown(adSession, Now);
            AnalyticsHub.Log(AdPlacement.LevelInterstitial, AdResult.Shown);
            bool closed = false;
            AdServiceLocator.Interstitial.Show(() => closed = true);
            while (!closed) yield return null;
            AnalyticsHub.Log(AdPlacement.LevelInterstitial, AdResult.Completed);
            AdServiceLocator.Interstitial.Load();
            transition.SetActive(false);
            if (next < 0) ShowHome(); else LoadLevel(next);
        }

        // "Reklam izle, ×2" (Ana Belge §15.1): once per win, the win's CB Coin again.
        private void OnDouble()
        {
            int extra = lastWinCoins;
            if (extra <= 0) return;
            RewardedAds.Show(AdServiceLocator.Rewarded, AdPlacement.DoubleReward, adSession, () => Now, () =>
            {
                lastWinCoins = 0;
                meta.Wallet.Add(extra);
                meta.Save();
                winView.ApplyDouble();
            }, NoAd);
        }

        /// <summary>The home screen. Until the Boss's land comes (F7, Ana Belge §11) it is the level list.</summary>
        public void ShowHome()
        {
            tutorials?.Stop();
            StopMoves();
            won = false;
            session = null;
            winView.Hide();
            levelSelect.Hide();
            promotion.Hide();
            message.Hide();
            hud.Show(false);
            board.gameObject.SetActive(false);
            BackdropBlur.Clear();
            StartCoroutine(post.BlurBackground(false, 0.01f));
            theme = WorldThemes.Current;
            DistrictBuilder.CraneMasts.Clear();
            // Owner 2026-11-01: while the Boss's city is hidden, a full car park in the city stands blurred behind the crate.
            DistrictBuilder.FullCarPark = !HomeView.CityShown;
            int size = HomeView.CityShown ? 1 : HomeView.StageSize;
            environment.Apply(theme, size, size, cam, QualityManager.ParticleScale, 7, !HomeView.CityShown);
            DistrictBuilder.FullCarPark = false;
            post.ApplyTheme(theme);
            music.PlayWorld(theme.Id);
            music.SetIntensity(0);
            UpdateCameraInsets();
            bool deliver = pendingDelivery;
            int promo = pendingPromotion;
            pendingDelivery = false;
            pendingPromotion = 0;
            home.Show(deliver, promo);
            if (!HomeView.CityShown) StartCoroutine(BlurHomeBackdrop());
        }

        // Once the home camera stands: the city behind the crate and the worker becomes a soft picture, the real one goes away.
        private IEnumerator BlurHomeBackdrop()
        {
            yield return null;
            yield return null;
            if (!home.IsOpen) yield break;
            BackdropBlur.Show(cam, home.Stage, framer.Distance + 1.2f);
            environment.HideNear();
            home.HideGround();
        }

        /// <summary>The next level's number and name for the home screen.</summary>
        private string NextLevelName()
        {
            int next = meta.NextLevel;
            return next >= 0 && next < levels.Count ? LevelLabel(next) : "";
        }

        /// <summary>"12. Kayan Şehir": owner 2026-11-01 wants every level's number in front of its name.</summary>
        private string LevelLabel(int index) => (index + 1) + ". " + Loc.Level(levels[index].id, levels[index].name);

        /// <summary>Screenshot tour: the win card's buttons and closing a card.</summary>
        public void NextFromWin() => OnNext();

        public void DoubleFromWin() => OnDouble();

        public void CloseCards() => message.Hide();

        public void CloseTutorial() => tutorials?.Stop();

        public void ShowLanguages() => languagePanel.Show();

        private void OnPlay()
        {
            if (message.IsOpen) return;
            LoadLevel(meta.NextLevel);
        }

        private void Analyse()
        {
            ClearStuck();
            analysis.Clear();
            analyseAt = -1f;
            if (session == null) return;
            if (session.State.IsCandyLevel) return;
            // On the stored solution's way the level can still be won: nothing to search.
            if (stored != null && stored.IsOnWay(session.State.GetKey())) return;
            analyseAt = Time.time + AnalyseDelay;
        }

        private void AnalyseNow()
        {
            ClearStuck();
            analyseAt = -1f;
            if (session != null) analysis.Start(session.State, Time.time);
        }

        private void ClearStuck()
        {
            stuckShown = false;
            hud.GlowUndo(false);
        }

        private void StopMoves()
        {
            StopAllCoroutines();
            busy = false;
            pending = null;
            autoPlaying = false;
            analysis.Clear();
            analyseAt = -1f;
            ClearStuck();
        }

        private void UpdateCameraInsets()
        {
            if (hud == null || framer == null) return;
            var insets = hud.ScreenInsets();
            framer.SetInsets(insets.x, insets.y);
            post.SetFocus(framer.Distance);
            if (board != null) QualityManager.SetShadowDistance(framer.Distance + Mathf.Max(board.Width, board.Height) + 4f);
        }

        private void ToggleSound()
        {
            bool on = !ProgressStore.SoundOn;
            ProgressStore.SoundOn = on;
            feedback.SoundOn = on;
            music.Muted = !on;
            settings.SetSound(on);
        }

        private void CycleQuality()
        {
            string label = QualityManager.Cycle();
            settings.SetQuality(label);
            post.Refresh();
            board.Effects.CountScale = QualityManager.ParticleScale;
        }

        private void BuildScene()
        {
            cam = Camera.main;
            if (cam == null) cam = new GameObject("Main Camera") { tag = "MainCamera" }.AddComponent<Camera>();
            framer = cam.GetComponent<CameraFramer>();
            if (framer == null) framer = cam.gameObject.AddComponent<CameraFramer>();
            // "-camyaw 45": try another diagonal angle without a rebuild (owner compares 25° and 45°).
            var args = System.Environment.GetCommandLineArgs();
            int yawArg = System.Array.IndexOf(args, "-camyaw");
            if (yawArg >= 0 && yawArg < args.Length - 1 && float.TryParse(args[yawArg + 1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float yaw))
                framer.yaw = yaw;

            environment = new GameObject("World").AddComponent<WorldEnvironment>();
            post = new GameObject("PostEffects").AddComponent<PostEffects>();
            post.Build(cam);

            feedback = gameObject.AddComponent<GameFeedback>();
            feedback.SoundOn = ProgressStore.SoundOn;
            music = new GameObject("Music").AddComponent<MusicPlayer>();
            music.Muted = !ProgressStore.SoundOn;
            PressableButton.Pressed += feedback.Click;

            board = new GameObject("BoardView").AddComponent<BoardView>();
            board.Feedback = feedback;
            board.Framer = framer;
            board.HammersEarned += OnHammersEarned;
            // Owner 2026-11-02 "biraz daha aksiyon": big waves get a cheer.
            board.Cheered += (blasts, groups, chain) =>
            {
                int score = blasts * 2 + groups + chain;
                hud.ShowTaskCard(Loc.T(score >= 6 ? "combo.3" : score >= 3 ? "combo.2" : "combo.1"));
            };

            input = gameObject.AddComponent<SwipeInput>();
            input.DirectionPressed += RequestMove;
            input.Swiped += OnSwipe;
            input.Pressed += OnPressed;
            input.Tapped += OnTapped;
            input.ActsAt = ActsAt;
            input.UndoPressed += Undo;
            input.RestartPressed += Restart;

            canvas = UiFactory.CreateCanvas("UI");
            home = gameObject.AddComponent<HomeController>();
            hud = new GameObject("Hud", typeof(RectTransform)).AddComponent<HudView>();
            hud.transform.SetParent(canvas, false);
            hud.Build(canvas);
            hud.UndoClicked += Undo;
            hud.RestartClicked += Restart;
            hud.PaintClicked += UsePaint;
            hud.HintClicked += RequestHint;
            hud.LevelsClicked += ShowHome;

            levelSelect = new GameObject("LevelSelect", typeof(RectTransform)).AddComponent<LevelSelectView>();
            levelSelect.Build(canvas);
            var tutorialView = new GameObject("Tutorial", typeof(RectTransform)).AddComponent<TutorialView>();
            tutorialView.Build(canvas);
            tutorials = new TutorialDirector(tutorialView, board);
            tutorials.LockRequested += () => { if (!board.WorkerLocked) ToggleLock(); };
            levelSelect.LevelChosen += LoadLevel;
            levelSelect.Closed += () => { if (session == null || won) ShowHome(); };

            winView = new GameObject("Win", typeof(RectTransform)).AddComponent<WinView>();
            winView.Build(canvas);
            winView.NextClicked += OnNext;
            winView.ReplayClicked += Restart;
            winView.LevelsClicked += ShowLevels;
            winView.StarShown += feedback.Star;
            winView.DepotClicked += ShowHome;
            winView.DoubleClicked += OnDouble;
            adConfig = AdConfig.Current;
            AdServiceLocator.Rewarded.Load();
            AdServiceLocator.Interstitial.Load();

            promotion = new GameObject("Promotion", typeof(RectTransform)).AddComponent<PromotionView>();
            promotion.Build(canvas);
            languagePanel = new GameObject("Language", typeof(RectTransform)).AddComponent<LanguagePanel>();
            languagePanel.Build(canvas);
            languagePanel.Picked += ChangeLanguage;
            // Owner 2026-11-01: the settings open from the top right of the home screen.
            settings = new GameObject("Settings", typeof(RectTransform)).AddComponent<SettingsPanel>();
            settings.Build(canvas);
            settings.SoundClicked += ToggleSound;
            settings.QualityClicked += CycleQuality;
            settings.LanguageClicked += languagePanel.Show;
            settings.PrivacyClicked += () => Application.OpenURL(PrivacyUrl);
            home.SettingsRequested += () => settings.Show(ProgressStore.SoundOn, QualityManager.Label);

            message = new GameObject("Message", typeof(RectTransform)).AddComponent<MessageCard>();
            message.Build(canvas);

            // "Sonraki saha hazırlanıyor…" before an interstitial (Ana Belge §15.2).
            var cover = UiFactory.Rect("Transition", canvas, Vector2.zero, Vector2.one, new Vector2(-200, -400), new Vector2(200, 400));
            cover.gameObject.AddComponent<UnityEngine.UI.Image>().color = new Color(0.1f, 0.12f, 0.18f, 1f);
            UiFactory.Label(cover, Loc.T("transition"), 54, TextAnchor.MiddleCenter, true);
            transition = cover.gameObject;
            transition.SetActive(false);
        }

        private void OnDestroy() => PressableButton.Pressed -= feedback.Click;
    }
}
