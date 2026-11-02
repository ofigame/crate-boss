using System;
using System.Collections;
using System.Collections.Generic;
using CrateBoss.Meta;
using UnityEngine;
using UnityEngine.UI;
using CrateBoss.Localization;

namespace CrateBoss.UI
{
    /// <summary>
    /// Top: level name, world and "Hamle 12 / 18" (moves / best). Bottom 20% (thumb zone):
    /// Undo, Restart, Hint, Levels as raised icon buttons.
    /// </summary>
    public class HudView : MonoBehaviour
    {
        private const float TopHeight = 250f;
        private const float BottomHeight = 260f;

        public event Action UndoClicked;
        public event Action RestartClicked;
        public event Action HintClicked;
        public event Action LevelsClicked;

        private RectTransform top, bottom;
        private Text title, world, moves, hintBadge;
        private readonly Image[] stars = new Image[3];
        private Image hintAd;
        private Button undoButton;
        private RectTransform movesPill;
        private readonly MachineTag[] chips = new MachineTag[MaxChips];
        private readonly MachineTag[] badges = new MachineTag[MaxChips];
        private Coroutine undoGlow;
        private Image undoGlowImage;

        public void Build(RectTransform safeArea)
        {
            top = UiFactory.Rect("Top", safeArea, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -TopHeight), Vector2.zero);
            var worldRt = UiFactory.Rect("World", top, new Vector2(0, 0.72f), new Vector2(1, 0.97f));
            world = UiFactory.Label(worldRt, "", 36);
            world.color = new Color(UiFactory.InkColor.r, UiFactory.InkColor.g, UiFactory.InkColor.b, 0.7f);
            // Long names shrink instead of running over the edge.
            var titleRt = UiFactory.Rect("Title", top, new Vector2(0.21f, 0.36f), new Vector2(0.79f, 0.76f));
            title = UiFactory.Label(titleRt, "", 74, TextAnchor.MiddleCenter, true);
            title.color = UiFactory.InkColor;
            title.horizontalOverflow = HorizontalWrapMode.Wrap; // best fit only shrinks text that has to stay inside its box
            title.verticalOverflow = VerticalWrapMode.Truncate;
            title.resizeTextForBestFit = true;
            title.resizeTextMinSize = 34;
            title.resizeTextMaxSize = 74;
            var movesRt = UiFactory.Rect("Moves", top, new Vector2(0.5f, 0.05f), new Vector2(0.5f, 0.05f));
            UiFactory.Place(movesRt, new Vector2(0.5f, 0.17f), new Vector2(420, 72));
            var pill = movesRt.gameObject.AddComponent<Image>();
            pill.sprite = IconFactory.Rounded;
            pill.type = Image.Type.Sliced;
            pill.color = new Color(1f, 1f, 1f, 0.55f);
            pill.raycastTarget = false;
            moves = UiFactory.Label(movesRt, "", 40, TextAnchor.MiddleLeft, true);
            moves.rectTransform.offsetMin = new Vector2(26, 0);
            moves.color = UiFactory.InkColor;
            // Stars still within reach for the moves made so far (Ana Belge §5.8).
            for (int i = 0; i < 3; i++)
            {
                stars[i] = UiFactory.Icon(movesRt, IconKind.Star, 40, StarOn);
                stars[i].rectTransform.anchoredPosition = new Vector2(PillWidth / 2f - 134 + i * 42, 0);
            }

            movesPill = movesRt;
            // Candy levels (owner 2026-11-01): the colour crates the cranes still hold, in a small pill; owner 2026-11-02: top left,
            // level with the hammers on the right.
            candyPill = UiFactory.Rect("Candy", top, new Vector2(0f, 0f), new Vector2(0f, 0f));
            UiFactory.Place(candyPill, new Vector2(0f, -0.2f), new Vector2(210, 84), new Vector2(115, 0)); // owner 2026-11-02: right under the moves row
            UiFactory.Panel("Rim", candyPill, Color.white, true).raycastTarget = false;
            var candyFill = UiFactory.Panel("Fill", candyPill, UiFactory.PillColor, true);
            candyFill.color = new Color(0.08f, 0.2f, 0.32f, 1f);
            candyFill.raycastTarget = false;
            candyFill.rectTransform.offsetMin = new Vector2(5, 5); candyFill.rectTransform.offsetMax = new Vector2(-5, -5);
            UiFactory.Icon(candyPill, IconKind.Crane, 62, new Color(1f, 0.8f, 0.25f)).rectTransform.anchoredPosition = new Vector2(-62, 0);
            dropsText = UiFactory.Label(UiFactory.Rect("Drops", candyPill, new Vector2(0.48f, 0f), new Vector2(0.95f, 1f)), "0", 48, TextAnchor.MiddleLeft, true);
            candyPill.gameObject.SetActive(false);
            // The hammers (owner 2026-11-01: a toy hammer with its count in a frosted ring; owner 2026-11-02: top right, level with
            // the cranes' count on the left).
            hammerBox = UiFactory.Rect("Hammers", top, new Vector2(1f, 0f), new Vector2(1f, 0f));
            UiFactory.Place(hammerBox, new Vector2(1f, -0.2f), new Vector2(220, 110), new Vector2(-118, 0));
            var hammer = UiFactory.Rect("Hammer", hammerBox, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            UiFactory.Place(hammer, new Vector2(0.5f, 0.5f), new Vector2(104, 104), new Vector2(-52, 0));
            hammerIcon = hammer.gameObject.AddComponent<Image>();
            hammerIcon.sprite = IconFactory.ToyHammer; // the model's picture once a level (and its light) stands: SetCandy
            hammerIcon.raycastTarget = false;
            var drop = hammerIcon.gameObject.AddComponent<Shadow>();
            drop.effectColor = new Color(0f, 0f, 0f, 0.3f);
            drop.effectDistance = new Vector2(0f, -6f);
            hammerRing = UiFactory.Rect("Ring", hammerBox, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            UiFactory.Place(hammerRing, new Vector2(0.5f, 0.5f), new Vector2(100, 100), new Vector2(54, 0));
            var rim = UiFactory.Panel("Rim", hammerRing, new Color(1f, 1f, 1f, 0.9f));
            rim.sprite = IconFactory.Circle;
            rim.raycastTarget = false;
            var frost = UiFactory.Panel("Frost", hammerRing, new Color(0.78f, 0.9f, 1f, 0.55f));
            frost.sprite = IconFactory.Circle;
            frost.raycastTarget = false;
            frost.rectTransform.offsetMin = new Vector2(7, 7); frost.rectTransform.offsetMax = new Vector2(-7, -7);
            var shine = UiFactory.Rect("Shine", hammerRing, new Vector2(0.22f, 0.58f), new Vector2(0.62f, 0.86f));
            var shineImage = shine.gameObject.AddComponent<Image>();
            shineImage.sprite = IconFactory.Circle;
            shineImage.color = new Color(1f, 1f, 1f, 0.35f);
            shineImage.raycastTarget = false;
            hammerText = UiFactory.Ink(UiFactory.Label(hammerRing, "0", 58, TextAnchor.MiddleCenter, true), new Color(0.1f, 0.25f, 0.45f));
            hammerBox.gameObject.SetActive(false);
            // Advanced hammers (owner 2026-11-02): a golden hammer with its count under the plain ones, only while there are some.
            superBox = UiFactory.Rect("Advanced", top, new Vector2(1f, 0f), new Vector2(1f, 0f));
            UiFactory.Place(superBox, new Vector2(1f, -0.2f), new Vector2(200, 84), new Vector2(-118, -108));
            UiFactory.Panel("Rim", superBox, Color.white, true).raycastTarget = false;
            var superFill = UiFactory.Panel("Fill", superBox, new Color(0.95f, 0.66f, 0.12f), true);
            superFill.raycastTarget = false;
            superFill.rectTransform.offsetMin = new Vector2(5, 5); superFill.rectTransform.offsetMax = new Vector2(-5, -5);
            var gold = UiFactory.Rect("Hammer", superBox, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            UiFactory.Place(gold, new Vector2(0.5f, 0.5f), new Vector2(84, 84), new Vector2(-55, 0));
            superIcon = gold.gameObject.AddComponent<Image>();
            superIcon.sprite = IconFactory.ToyHammer;
            superIcon.color = new Color(1f, 0.93f, 0.55f);
            superIcon.raycastTarget = false;
            superText = UiFactory.Label(UiFactory.Rect("Count", superBox, new Vector2(0.48f, 0f), new Vector2(0.95f, 1f)), "0", 48, TextAnchor.MiddleLeft, true);
            superBox.gameObject.SetActive(false);
            // The paint gun (owner 2026-11-02): under the hammers, swelling and shrinking while there is one to use.
            paintButton = UiFactory.Button(top, new Color(0.93f, 0.33f, 0.62f), () => PaintClicked?.Invoke(), IconKind.Paint);
            paintRoot = (RectTransform)paintButton.transform;
            UiFactory.Place(paintRoot, new Vector2(1f, -0.2f), new Vector2(124, 124), new Vector2(-118, -225));
            paintCount = UiFactory.Ink(UiFactory.Label(UiFactory.Rect("Count", paintRoot, new Vector2(0.55f, 0f), new Vector2(1.1f, 0.4f)), "", 40, TextAnchor.MiddleCenter, true), Color.white);
            paintRoot.gameObject.SetActive(false);
            for (int i = 0; i < MaxChips; i++)
            {
                chips[i] = BuildTag("CraneChip", top, true);
                badges[i] = BuildTag("CraneBadge", safeArea, false);
                badges[i].Root.SetAsFirstSibling(); // under the HUD bars
            }

            bottom = UiFactory.Rect("Bottom", safeArea, new Vector2(0, 0), new Vector2(1, 0), new Vector2(50, 40), new Vector2(-50, BottomHeight - 40));
            var layout = bottom.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 34;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            undoButton = UiFactory.Button(bottom, UiFactory.ButtonColor, () => UndoClicked?.Invoke(), IconKind.Undo);
            UiFactory.Button(bottom, UiFactory.ButtonColor, () => RestartClicked?.Invoke(), IconKind.Restart);
            var hint = UiFactory.Button(bottom, UiFactory.WarmColor, () => HintClicked?.Invoke(), IconKind.Hint);
            UiFactory.Button(bottom, UiFactory.ButtonColor, () => LevelsClicked?.Invoke(), IconKind.Levels);

            var badge = UiFactory.Rect("Badge", UiFactory.FaceOf(hint), new Vector2(1, 1), new Vector2(1, 1));
            UiFactory.Place(badge, new Vector2(0.92f, 0.88f), new Vector2(64, 64));
            var badgeImage = badge.gameObject.AddComponent<Image>();
            badgeImage.sprite = IconFactory.Circle;
            badgeImage.color = new Color(0.9f, 0.25f, 0.3f);
            badgeImage.raycastTarget = false;
            hintBadge = UiFactory.Label(badge, "3", 36, TextAnchor.MiddleCenter, true);
            hintAd = UiFactory.Icon(badge, IconKind.Ad, 40, Color.white); // free hints used up: a rewarded ad (Ana Belge §14)
            hintAd.enabled = false;

            undoGlowImage = UiFactory.Panel("Glow", UiFactory.FaceOf(undoButton), new Color(1f, 0.85f, 0.3f, 0f), true);
            undoGlowImage.raycastTarget = false;
            undoGlowImage.transform.SetAsFirstSibling();
        }

        /// <param name="darkBackground">Light text over dark worlds (mine), dark text over light ones.</param>
        public void SetLevel(int number, string name, string worldName, bool darkBackground)
        {
            title.text = number > 0 ? number + ". " + name : name; // owner 2026-11-01: the level number in front
            world.text = worldName.ToUpper(new System.Globalization.CultureInfo("tr-TR"));
            var ink = darkBackground ? new Color(0.98f, 0.95f, 0.9f) : UiFactory.InkColor;
            title.color = ink;
            world.color = new Color(ink.r, ink.g, ink.b, 0.7f);
        }

        /// <summary>The level HUD lives straight under the safe area (top bar, bottom bar, badges): hide them all off the level.</summary>
        public void Show(bool on)
        {
            top.gameObject.SetActive(on);
            bottom.gameObject.SetActive(on);
            foreach (var b in badges) if (b != null) b.Root.gameObject.SetActive(on && b.Root.gameObject.activeSelf);
            gameObject.SetActive(on);
        }

        public const int MaxChips = 4;
        private const float PillWidth = 420f, ChipWidth = 170f, ChipGap = 14f;
        private static readonly Color CraneOrange = new Color(1f, 0.416f, 0.239f);
        private static readonly Color CraneRed = new Color(0.9f, 0.16f, 0.16f);

        // One machine tag: a chip in the top bar (icon + "A 3") or a badge on the floor next to the drop cell.
        // Waiting shows the letter and a pause sign, so the state never relies on colour alone (Ana Belge §5.3).
        private MachineTag BuildTag(string name, RectTransform parent, bool chip)
        {
            var root = chip ? UiFactory.Rect(name, parent, new Vector2(0.5f, 0.17f), new Vector2(0.5f, 0.17f))
                            : UiFactory.Rect(name, parent, Vector2.zero, Vector2.zero);
            root.pivot = new Vector2(0.5f, 0.5f);
            root.sizeDelta = chip ? new Vector2(ChipWidth, 72) : new Vector2(130, 58);
            var image = root.gameObject.AddComponent<Image>();
            image.sprite = IconFactory.Rounded;
            image.type = Image.Type.Sliced;
            image.color = CraneOrange;
            image.raycastTarget = false;
            float left = chip ? -ChipWidth / 2f : -65f;
            Image icon = null;
            if (chip)
            {
                icon = UiFactory.Icon(root, IconKind.Crane, 50, Color.white);
                ((RectTransform)icon.transform).anchoredPosition = new Vector2(left + 36, 2);
                left += 64;
            }
            else left += 20;
            var text = UiFactory.Label(root, "", chip ? 42 : 36, TextAnchor.MiddleLeft, true);
            text.rectTransform.offsetMin = new Vector2(left + root.sizeDelta.x / 2f, 0);
            var pause = UiFactory.Icon(root, IconKind.Pause, chip ? 44 : 38, Color.white);
            ((RectTransform)pause.transform).anchoredPosition = new Vector2(left + (chip ? 52 : 46), 0);
            root.gameObject.SetActive(false);
            return new MachineTag(root, image, text, pause, icon);
        }

        /// <summary>
        /// Top-bar chips, one per active machine (at most <see cref="MaxChips"/>, three per row); the move pill moves
        /// left to make room. Pass an empty list to hide them all.
        /// </summary>
        public void SetCraneChips(IReadOnlyList<MachineTagInfo> infos)
        {
            int n = Mathf.Min(infos.Count, MaxChips);
            int firstRow = Mathf.Min(n, 3);
            float total = PillWidth + firstRow * (ChipWidth + ChipGap);
            movesPill.anchoredPosition = new Vector2(n == 0 ? 0 : -total / 2f + PillWidth / 2f, 0);
            for (int i = 0; i < MaxChips; i++)
            {
                var tag = chips[i];
                tag.Root.gameObject.SetActive(i < n);
                if (i >= n) continue;
                int row = i / 3, col = i % 3;
                float x = row == 0 ? -total / 2f + PillWidth + ChipGap + col * (ChipWidth + ChipGap) + ChipWidth / 2f
                                   : -total / 2f + PillWidth + ChipGap + ChipWidth / 2f + col * (ChipWidth + ChipGap);
                tag.Root.anchoredPosition = new Vector2(x, -row * 82f);
                tag.Show(infos[i], CraneOrange, CraneRed);
            }
        }

        /// <summary>Floor badges ("A 3" / "A ⏸") that follow screen points; a null point hides that badge.</summary>
        public void SetCraneBadges(IReadOnlyList<MachineTagInfo> infos, IReadOnlyList<Vector2?> screenPoints)
        {
            for (int i = 0; i < MaxChips; i++)
            {
                var tag = badges[i];
                bool on = i < infos.Count && i < screenPoints.Count && screenPoints[i].HasValue;
                tag.Root.gameObject.SetActive(on);
                if (!on) continue;
                var parent = (RectTransform)tag.Root.parent;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenPoints[i].Value, null, out var local))
                    tag.Root.anchoredPosition = local + parent.rect.size * 0.5f; // anchors sit at the parent's corner
                int counter = infos[i].Counter;
                float pulse = infos[i].Waiting ? 1f : counter == 2 ? 1.1f + 0.06f * Mathf.Sin(Time.time * 7f) : counter <= 1 ? 1.15f : 1f;
                tag.Root.localScale = Vector3.one * pulse;
                tag.Show(infos[i], CraneOrange, CraneRed);
            }
        }

        /// <summary>"Hamle 12 / 18": the solver's best is the goal to beat.</summary>
        private static readonly Color StarOn = new Color(1f, 0.74f, 0.1f), StarOff = new Color(0.3f, 0.3f, 0.35f, 0.35f);

        /// <summary>Moves made and the best solution, and the stars still possible (best unknown = three stars).</summary>
        public void SetMoves(int count, int best)
        {
            moves.text = best > 0 ? Loc.F("hud.moves", count, best) : Loc.F("hud.moves1", count);
            int lit = Meta.StarRating.Stars(count, best);
            for (int i = 0; i < 3; i++) stars[i].color = i < lit ? StarOn : StarOff;
            moves.color = best > 0 && count > Meta.StarRating.Two(best) ? new Color(0.8f, 0.2f, 0.15f) : UiFactory.InkColor;
        }

        public void SetHints(int left)
        {
            hintBadge.text = left > 0 ? left.ToString() : "";
            hintAd.enabled = left <= 0;
        }

        // ---------- goal card (Ana Belge §5) ----------

        private RectTransform taskCard;
        private Text taskText;
        private CanvasGroup taskGroup;
        private Coroutine taskRoutine;

        /// <summary>Short goal card under the top bar for ~2.5 s when a level starts (null hides it).</summary>
        private RectTransform candyPill, hammerBox, hammerRing;
        private Image hammerIcon;
        private Text hammerText, dropsText, superText;
        private Button paintButton;
        private RectTransform paintRoot;
        private Text paintCount;

        /// <summary>The paint gun was tapped (owner 2026-11-02).</summary>
        public event Action PaintClicked;

        /// <summary>Paint guns in hand: the button shows (and pulses) while there is one.</summary>
        public void SetPaint(int count)
        {
            if (paintRoot == null) return;
            paintRoot.gameObject.SetActive(count > 0 && hammerBox.gameObject.activeSelf);
            paintCount.text = count > 1 ? "×" + count : "";
        }

        private void Update()
        {
            if (paintRoot != null && paintRoot.gameObject.activeSelf) paintRoot.localScale = Vector3.one * (1f + 0.12f * Mathf.Sin(Time.time * 5f));
        }
        private RectTransform superBox;
        private Image superIcon;

        /// <summary>Advanced hammers (owner 2026-11-02): shown under the plain ones while there are some; a pop when they grow.</summary>
        public void SetSuper(int count)
        {
            if (superBox == null) return;
            bool grew = count > shownSuper;
            shownSuper = count;
            superBox.gameObject.SetActive(count > 0 && hammerBox.gameObject.activeSelf);
            superText.text = "×" + count;
            if (superIcon.sprite == IconFactory.ToyHammer && hammerIcon.sprite != IconFactory.ToyHammer) superIcon.sprite = hammerIcon.sprite;
            if (grew && isActiveAndEnabled) StartCoroutine(Pop(superBox));
        }
        private int shownSuper;
        private int hammersTarget, hammersFlying;

        /// <summary>Candy levels: hammer hits in hand and colour crates left in the cranes (hidden on other levels).</summary>
        public void SetCandy(bool on, int hammers, int drops)
        {
            candyPill.gameObject.SetActive(on);
            hammerBox.gameObject.SetActive(on);
            if (!on) { hammersFlying = 0; shownSuper = 0; superBox.gameObject.SetActive(false); paintRoot.gameObject.SetActive(false); return; }
            if (hammerIcon.sprite == IconFactory.ToyHammer) hammerIcon.sprite = View.ModelIcon.Hammer;
            hammersTarget = hammers;
            dropsText.text = drops.ToString();
            ShowHammers();
        }

        // The count shows what has landed: the hammers still flying are added when they arrive.
        private void ShowHammers() => hammerText.text = Mathf.Max(0, hammersTarget - hammersFlying).ToString();

        /// <summary>
        /// Hammers earned by a sunk group fly from where it sank (screen pixels) to the counter, one after another; each one
        /// that lands counts up and makes the ring pop (owner 2026-11-01).
        /// </summary>
        public void FlyHammers(Vector2 screenFrom, int count)
        {
            if (!isActiveAndEnabled || !hammerBox.gameObject.activeSelf) return;
            hammersFlying += count;
            hammersTarget += count;
            ShowHammers();
            // Owner 2026-11-02: a hit for every sunk crate, so a big wave earns many: at most eight fly, sharing the count.
            int icons = Mathf.Min(count, 8);
            for (int i = 0; i < icons; i++) StartCoroutine(Fly(screenFrom, i * 0.07f, count / icons + (i < count % icons ? 1 : 0)));
        }

        private System.Collections.IEnumerator Fly(Vector2 screenFrom, float delay, int carries)
        {
            var root = (RectTransform)hammerBox.parent;
            var canvas = root.GetComponentInParent<Canvas>();
            var cam = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(root, screenFrom, cam, out var from);
            var to = (Vector2)root.InverseTransformPoint(hammerRing.position);
            var rt = UiFactory.Rect("FlyingHammer", root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(110, 110);
            var image = rt.gameObject.AddComponent<Image>();
            image.sprite = View.ModelIcon.Hammer;
            image.raycastTarget = false;
            rt.anchoredPosition = from - root.rect.size * (new Vector2(0.5f, 0.5f) - root.pivot);
            rt.localScale = Vector3.zero;
            if (delay > 0f) yield return new WaitForSeconds(delay);
            var start = rt.anchoredPosition;
            var end = to - root.rect.size * (new Vector2(0.5f, 0.5f) - root.pivot);
            var lift = start + (end - start) * 0.35f + new Vector2(0f, 260f); // an arc up and over
            const float time = 0.55f;
            for (float t = 0; t < time; t += Time.deltaTime)
            {
                float k = t / time;
                float e = k * k * (3f - 2f * k);
                var a = Vector2.Lerp(start, lift, e);
                var b = Vector2.Lerp(lift, end, e);
                rt.anchoredPosition = Vector2.Lerp(a, b, e);
                rt.localScale = Vector3.one * (k < 0.15f ? k / 0.15f * 1.2f : Mathf.Lerp(1.2f, 0.7f, k));
                rt.localRotation = Quaternion.Euler(0f, 0f, 360f * e);
                yield return null;
            }
            Destroy(rt.gameObject);
            hammersFlying = Mathf.Max(0, hammersFlying - carries);
            ShowHammers();
            StartCoroutine(Pop(hammerRing));
            StartCoroutine(Pop(hammerIcon.rectTransform));
        }

        private static System.Collections.IEnumerator Pop(RectTransform rt)
        {
            for (float t = 0; t < 0.3f; t += Time.deltaTime)
            {
                rt.localScale = Vector3.one * (1f + 0.45f * Mathf.Sin(t / 0.3f * Mathf.PI));
                yield return null;
            }
            rt.localScale = Vector3.one;
        }

        public void ShowTaskCard(string text)
        {
            if (taskCard == null)
            {
                taskCard = UiFactory.Rect("TaskCard", top, new Vector2(0.5f, 0), new Vector2(0.5f, 0));
                UiFactory.Place(taskCard, new Vector2(0.5f, 0), new Vector2(900, 150), new Vector2(0, -100));
                var bg = taskCard.gameObject.AddComponent<Image>();
                bg.sprite = IconFactory.Rounded;
                bg.type = Image.Type.Sliced;
                bg.color = new Color(0.12f, 0.14f, 0.2f, 0.88f);
                bg.raycastTarget = false;
                taskText = UiFactory.Label(taskCard, "", 40, TextAnchor.MiddleCenter, true);
                // Several goals make a long line: wrap and shrink inside the card instead of running off the screen.
                taskText.rectTransform.offsetMin = new Vector2(24, 8);
                taskText.rectTransform.offsetMax = new Vector2(-24, -8);
                taskText.horizontalOverflow = HorizontalWrapMode.Wrap;
                taskText.verticalOverflow = VerticalWrapMode.Truncate;
                taskText.resizeTextForBestFit = true;
                taskText.resizeTextMinSize = 24;
                taskText.resizeTextMaxSize = 40;
                taskText.color = new Color(1f, 0.86f, 0.4f);
                taskGroup = taskCard.gameObject.AddComponent<CanvasGroup>();
                taskGroup.blocksRaycasts = false;
            }
            if (taskRoutine != null) StopCoroutine(taskRoutine);
            taskCard.gameObject.SetActive(!string.IsNullOrEmpty(text));
            if (string.IsNullOrEmpty(text)) return;
            taskText.text = text;
            taskRoutine = StartCoroutine(TaskCardLife());
        }

        private IEnumerator TaskCardLife()
        {
            for (float t = 0; t < 0.2f; t += Time.deltaTime) { taskGroup.alpha = t / 0.2f; yield return null; }
            taskGroup.alpha = 1f;
            yield return new WaitForSeconds(2.5f);
            for (float t = 0; t < 0.4f; t += Time.deltaTime) { taskGroup.alpha = 1f - t / 0.4f; yield return null; }
            taskCard.gameObject.SetActive(false);
            taskRoutine = null;
        }

        /// <summary>Stuck: gently pulse the Undo button.</summary>
        public void GlowUndo(bool on)
        {
            if (undoGlow != null) StopCoroutine(undoGlow);
            undoGlow = null;
            undoGlowImage.color = new Color(1f, 0.85f, 0.3f, 0f);
            if (on) undoGlow = StartCoroutine(Pulse());
        }

        private IEnumerator Pulse()
        {
            while (true)
            {
                float k = (Mathf.Sin(Time.time * 4f) + 1f) / 2f;
                undoGlowImage.color = new Color(1f, 0.85f, 0.3f, 0.25f + 0.55f * k);
                yield return null;
            }
        }

        /// <summary>Screen fractions covered at the top and bottom (for the camera's safe area).</summary>
        public Vector2 ScreenInsets()
        {
            Canvas.ForceUpdateCanvases();
            float h = Screen.height;
            if (h <= 0) return new Vector2(0.15f, 0.2f);
            var corners = new Vector3[4];
            top.GetWorldCorners(corners);
            float topInset = 1f - corners[0].y / h;
            if (candyPill.gameObject.activeSelf)
            {
                candyPill.GetWorldCorners(corners);
                topInset = Mathf.Max(topInset, 1f - corners[0].y / h);
            }
            bottom.GetWorldCorners(corners);
            float bottomInset = corners[1].y / h;
            return new Vector2(Mathf.Clamp(topInset + 0.01f, 0.05f, 0.4f), Mathf.Clamp(bottomInset + 0.02f, 0.05f, 0.4f));
        }
    }
}
