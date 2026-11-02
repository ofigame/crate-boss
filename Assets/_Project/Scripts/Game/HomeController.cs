using System;
using System.Collections;
using System.Collections.Generic;
using CrateBoss.Ads;
using CrateBoss.Localization;
using CrateBoss.Meta;
using CrateBoss.UI;
using CrateBoss.View;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CrateBoss.Game
{
    /// <summary>
    /// The home screen, the Boss's land (Ana Belge §11–§13): shows the lands and items, brings the delivery crate after a
    /// win, opens crates (30 CB Coin, the first one free, or an ad), offers three items, places them in edit mode, the
    /// wardrobe, "+30 CB Coin" by an ad, and the promotion moment.
    /// </summary>
    public class HomeController : MonoBehaviour
    {
        public event Action PlayRequested, LevelsRequested, SettingsRequested;

        private MetaService meta;
        private Camera cam;
        private CameraFramer framer;
        private MessageCard message;
        private PromotionView promotion;
        private AdSessionState adSession;
        private Func<string> nextLevelName;
        private Func<EffectsView> effects;

        private HomeView view;
        private HomeHud hud;
        private CratePanel cratePanel;
        private EditBar editBar;
        private WardrobePanel wardrobe;
        private readonly System.Random rng = new System.Random();

        private bool editing, pathMode;
        private string selected;
        private (LandDef land, int x, int y)? ghostAt;
        private int rot;
        private bool pendingPromotionCrate;
        private List<string> offer;

        private static double Now => Time.realtimeSinceStartupAsDouble;
        public bool IsOpen => view != null && view.gameObject.activeSelf;
        public GameObject Stage => view.Stage;
        public void HideGround() => view.HideGround();

        public void Init(MetaService meta, Camera cam, CameraFramer framer, RectTransform canvas, MessageCard message, PromotionView promotion,
            AdSessionState adSession, Func<string> nextLevelName, Func<EffectsView> effects)
        {
            this.meta = meta; this.cam = cam; this.framer = framer; this.message = message; this.promotion = promotion;
            this.adSession = adSession; this.nextLevelName = nextLevelName; this.effects = effects;
            view = new GameObject("HomeView").AddComponent<HomeView>();
            view.gameObject.SetActive(false);
            hud = new GameObject("HomeHud", typeof(RectTransform)).AddComponent<HomeHud>();
            hud.Build(canvas);
            hud.PlayClicked += () => { if (!Busy) PlayRequested?.Invoke(); };
            hud.LevelsClicked += () => { if (!Busy) LevelsRequested?.Invoke(); };
            hud.SettingsClicked += () => { if (!Busy) SettingsRequested?.Invoke(); };
            hud.CratesClicked += OpenNextCrate;
            hud.AdCoinsClicked += AdCoins;
            hud.WardrobeClicked += ShowWardrobe;
            hud.EditClicked += () => SetEditing(true);
            cratePanel = new GameObject("CratePanel", typeof(RectTransform)).AddComponent<CratePanel>();
            cratePanel.Build(canvas);
            cratePanel.OpenClicked += () => OpenCrate(false);
            cratePanel.AdClicked += () => RewardedAds.Show(AdServiceLocator.Rewarded, AdPlacement.OpenCrate, adSession, () => Now, () =>
            {
                UseCrateAd();
                OpenCrate(true);
            }, () => message.Show(IconKind.Ad, Loc.T("ad.none.title"), Loc.T("ad.none.body"), Loc.T("ui.ok"), null));
            cratePanel.Chosen += Choose;
            cratePanel.Closed += Refresh;
            editBar = new GameObject("EditBar", typeof(RectTransform)).AddComponent<EditBar>();
            editBar.Build(canvas);
            editBar.InventoryPicked += id => { selected = id; rot = 0; pathMode = false; editBar.SetPathMode(false); UpdateGhost(); };
            editBar.RotateClicked += () => { rot = (rot + 1) % 4; UpdateGhost(); };
            editBar.PlaceClicked += PlaceSelected;
            editBar.StoreClicked += () => { selected = null; view.HideGhost(); RefreshEdit(); };
            editBar.PathClicked += () => { pathMode = !pathMode; selected = null; view.HideGhost(); editBar.SetPathMode(pathMode); RefreshEdit(); };
            editBar.DoneClicked += () => SetEditing(false);
            wardrobe = new GameObject("Wardrobe", typeof(RectTransform)).AddComponent<WardrobePanel>();
            wardrobe.Build(canvas);
            wardrobe.BuyOrWear += id =>
            {
                if (WardrobeCatalog.BuyOrWear(meta.Data, meta.Wallet, id)) { meta.Save(); Outfit.Set(meta.Data); }
                ShowWardrobe();
                Refresh();
            };
        }

        bool Busy => cratePanel.IsOpen || wardrobe.IsOpen || message.IsOpen || promotion.IsOpen || editing;

        /// <param name="deliver">A level was cleared for the first time: a truck brings its crate (Ana Belge §11.3).</param>
        /// <param name="promotedRank">The rank just reached (Ana Belge §11.4), or 0.</param>
        public void Show(bool deliver, int promotedRank)
        {
            HomeRules.OpenLands(meta.Data, meta.Rank, meta.ClearedCount > 0);
            meta.Save();
            view.gameObject.SetActive(true);
            view.Build(meta.Data, meta.Items);
            hud.Show(true);
            // Owner 2026-11-01: a banner on the home screen only; the buttons at the bottom move up above it.
            AdServiceLocator.Banner.Show();
            hud.SetBottomInset(AdServiceLocator.Banner.Height);
            Outfit.Set(meta.Data);
            // The whole plot in view: no worker to follow and no close-up limit (that is for big yards).
            if (savedMinCell < 0) saved = (framer.minCellWidth, framer.pitch, framer.minFrameWidth, framer.minFrameDepth);
            savedMinCell = framer.minCellWidth;
            framer.minCellWidth = 0f;
            framer.Follow = null;
            var a = view.OpenArea;
            if (HomeView.CityShown) framer.Frame(a.min.x, a.max.x, a.min.z, a.max.z);
            else
            {
                // Owner 2026-11-01: only the crate and the worker sitting on it, seen from low in front, filling the middle.
                framer.pitch = StagePitch;
                framer.minFrameWidth = framer.minFrameDepth = 0f;
                framer.SetInsets(0.3f, 0.25f); // below the logo and the rank bar, above OYNA
                var c = view.Figure.center;
                framer.Frame(c.x - 0.5f, c.x + 0.5f, c.z - 0.5f, c.z + 0.5f, 0f, new[] { view.Figure });
            }
            Refresh();
            StopAllCoroutines();
            StartCoroutine(Arrive(deliver, promotedRank));
        }

        IEnumerator Arrive(bool deliver, int promotedRank)
        {
            if (deliver) yield return view.DeliverCrate(effects?.Invoke());
            if (promotedRank > 1)
            {
                var r = meta.Ranks.Get(promotedRank);
                promotion.Show(Loc.T(r.NameKey), Loc.T("promo.gift"));
            }
            else if (HomeView.CityShown && !meta.Data.firstCrateOpened && meta.Data.crates > 0)
                message.Show(IconKind.Coin, Loc.T("crate.first.title"), Loc.T("crate.first.body"), Loc.T("crate.first.open"), OpenNextCrate);
        }

        private float savedMinCell = -1f;
        private (float minCell, float pitch, float width, float depth) saved;
        private const float StagePitch = 16f;

        public void Hide()
        {
            StopAllCoroutines();
            if (savedMinCell >= 0)
            {
                (framer.minCellWidth, framer.pitch, framer.minFrameWidth, framer.minFrameDepth) = saved;
                savedMinCell = -1f;
            }
            SetEditing(false);
            view.HideGhost();
            view.gameObject.SetActive(false);
            hud.Show(false);
            AdServiceLocator.Banner.Hide();
            cratePanel.Hide();
            wardrobe.Hide();
        }

        void Refresh()
        {
            if (!IsOpen) return;
            int rank = meta.Rank;
            var r = meta.Ranks.Get(Mathf.Min(rank, meta.Ranks.Ranks.Count));
            hud.Set(Loc.T(r.NameKey), meta.RankProgress, meta.Wallet.Balance, meta.Data.crates + meta.Data.promoCrates.Count,
                DailyRules.AdCoinsLeft(meta.Data, meta.Config, DateTime.Now), meta.Config.adCoinsPerDay, nextLevelName());
            view.RefreshPile();
        }

        // ---------- crates (§12.1, §12.2) ----------

        void OpenNextCrate()
        {
            if (Busy && !message.IsOpen) return;
            message.Hide();
            if (!HomeRules.HasCrate(meta.Data))
            {
                message.Show(IconKind.Coin, Loc.T("crate.none.title"), Loc.T("crate.none.body"), Loc.T("ui.ok"), null);
                return;
            }
            pendingPromotionCrate = meta.Data.promoCrates.Count > 0;
            int price = pendingPromotionCrate || !meta.Data.firstCrateOpened ? 0 : meta.Config.crateOpen;
            bool adLeft = (meta.Data.crateAdDay != DailyRules.Day(DateTime.Now) ? 0 : meta.Data.crateAdCount) < meta.Config.crateAdsPerDay;
            cratePanel.ShowClosed(Loc.T(pendingPromotionCrate ? "crate.promo" : "crate.title"), price, meta.Wallet.Balance >= price, adLeft);
        }

        void UseCrateAd()
        {
            string today = DailyRules.Day(DateTime.Now);
            if (meta.Data.crateAdDay != today) { meta.Data.crateAdDay = today; meta.Data.crateAdCount = 0; }
            meta.Data.crateAdCount++;
        }

        void OpenCrate(bool free)
        {
            int promoRank = pendingPromotionCrate && meta.Data.promoCrates.Count > 0 ? meta.Data.promoCrates[0] : 0;
            if (!HomeRules.Open(meta.Data, meta.Wallet, meta.Config, pendingPromotionCrate, free)) return;
            offer = HomeRules.Offer(meta.Items, meta.Data, Mathf.Clamp(meta.Rank, 1, 5), rng, promoRank);
            meta.Save();
            var cards = new List<(string, string, string, Color)>();
            foreach (var id in offer)
            {
                var item = meta.Items.Get(id);
                ColorUtility.TryParseHtmlString(item?.Color ?? "#FFFFFF", out var c);
                cards.Add((id, Loc.T("item." + id), item?.Size ?? "small", c));
            }
            effects?.Invoke()?.Confetti(cam.transform.position + cam.transform.forward * 8f, 40);
            cratePanel.ShowChoices(cards);
            Refresh();
        }

        void Choose(string id, bool placeNow)
        {
            HomeRules.Choose(meta.Data, id);
            meta.Save();
            Refresh();
            if (placeNow) { SetEditing(true); selected = id; UpdateGhost(); }
        }

        // ---------- edit mode (§12.4) ----------

        void SetEditing(bool on)
        {
            if (editing == on) return;
            editing = on;
            hud.Show(!on && IsOpen);
            editBar.Show(on);
            view.ShowGrid(on);
            if (!on) { selected = null; ghostAt = null; pathMode = false; view.HideGhost(); meta.Save(); Refresh(); }
            else RefreshEdit();
        }

        void RefreshEdit()
        {
            var items = new List<(string, string, Color)>();
            foreach (var id in meta.Data.inventory)
            {
                var item = meta.Items.Get(id);
                ColorUtility.TryParseHtmlString(item?.Color ?? "#FFFFFF", out var c);
                items.Add((id, Loc.T("item." + id), c));
            }
            editBar.SetInventory(items);
            editBar.SetHint(pathMode ? Loc.T("edit.hint.path") : selected != null ? Loc.T("edit.hint.place") : Loc.T("edit.hint.pick"));
        }

        void UpdateGhost()
        {
            RefreshEdit();
            var item = meta.Items.Get(selected);
            if (item == null) { view.HideGhost(); return; }
            if (ghostAt == null)
            {
                var start = LandCatalog.Get("start");
                ghostAt = (start, start.W / 2 - 1, start.D / 2 - 1);
            }
            var (land, x, y) = ghostAt.Value;
            view.ShowGhost(item, land, x, y, rot, HomeRules.CanPlace(meta.Data, meta.Items, land, item, x, y, rot));
        }

        void PlaceSelected()
        {
            if (selected == null || ghostAt == null) return;
            var (land, x, y) = ghostAt.Value;
            if (!HomeRules.Place(meta.Data, meta.Items, selected, land, x, y, rot))
            {
                editBar.SetHint(Loc.T("edit.hint.blocked"));
                return;
            }
            meta.Save();
            view.HideGhost();
            view.RefreshItems();
            effects?.Invoke()?.ToyClouds(HomeView.CellWorld(land, x, y), 3, 0.25f);
            selected = null;
            RefreshEdit();
        }

        private Vector2 pressAt;
        private float pressTime;

        void Update()
        {
            if (!IsOpen) return;
            var pointer = Pointer.current;
            if (pointer == null) return;
            if (pointer.press.wasPressedThisFrame) { pressAt = pointer.position.ReadValue(); pressTime = Time.unscaledTime; }
            if (!pointer.press.wasReleasedThisFrame) return;
            var pos = pointer.position.ReadValue();
            if ((pos - pressAt).magnitude > 30f || Time.unscaledTime - pressTime > 0.4f) return; // a drag pans the view
            if (UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;
            if (!editing)
            {
                if (!Busy && view.PileAtScreen(cam, pos)) OpenNextCrate();
                return;
            }
            var cell = view.CellAtScreen(cam, pos);
            if (cell == null) return;
            var (land, x, y) = cell.Value;
            if (pathMode)
            {
                if (HomeRules.TogglePath(meta.Data, meta.Items, land, x, y)) { view.RefreshItems(); meta.Save(); }
                return;
            }
            if (selected == null)
            {
                // Tap a placed item to pick it up again (free, Ana Belge §12.4).
                var placed = HomeRules.OccupiedBy(meta.Data, meta.Items, land.Id, x, y);
                if (placed == null) return;
                HomeRules.Store(meta.Data, placed);
                selected = placed.id; rot = placed.rot; ghostAt = (land, placed.x, placed.y);
                view.RefreshItems();
                UpdateGhost();
                return;
            }
            ghostAt = (land, x, y);
            UpdateGhost();
        }

        // ---------- coins and wardrobe ----------

        void AdCoins()
        {
            if (Busy) return;
            if (DailyRules.AdCoinsLeft(meta.Data, meta.Config, DateTime.Now) <= 0)
            {
                message.Show(IconKind.Ad, Loc.T("adcoins.limit.title"), Loc.T("adcoins.limit.body"), Loc.T("ui.ok"), null);
                return;
            }
            RewardedAds.Show(AdServiceLocator.Rewarded, AdPlacement.HomeCoins, adSession, () => Now, () =>
            {
                DailyRules.UseAdCoins(meta.Data, DateTime.Now);
                meta.Wallet.Add(meta.Config.adCoins);
                meta.Save();
                Refresh();
            }, () => message.Show(IconKind.Ad, Loc.T("ad.none.title"), Loc.T("ad.none.body"), Loc.T("ui.ok"), null));
        }

        void ShowWardrobe()
        {
            var list = new List<(string, string, Color, int, int)>();
            foreach (var p in WardrobeCatalog.Pieces)
            {
                ColorUtility.TryParseHtmlString(p.Color, out var c);
                var worn = WardrobeCatalog.Worn(meta.Data, p.Slot);
                int state = worn != null && worn.Id == p.Id ? 2 : meta.Data.owned.Contains(p.Id) ? 1 : 0;
                list.Add((p.Id, Loc.T(p.NameKey), c, p.Price, state));
            }
            wardrobe.Show(meta.Wallet.Balance, list);
        }
    }
}
