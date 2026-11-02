using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using CrateBoss.Localization;

namespace CrateBoss.UI
{
    /// <summary>
    /// Win card (Ana Belge §2): rises from the bottom over the blurred scene, then the stars fill one by one.
    /// </summary>
    public class WinView : MonoBehaviour
    {
        public event Action NextClicked;
        public event Action ReplayClicked;
        public event Action LevelsClicked;
        public event Action DepotClicked;
        /// <summary>"Reklam izle, ×2" (Ana Belge §15.1), once per win.</summary>
        public event Action DoubleClicked;
        /// <summary>Fired as each star lights (index 0..2), for the "tık" sound.</summary>
        public event Action<int> StarShown;

        private RectTransform card;
        private Image dim;
        private Text moves, prim, primLines;
        private int primAmount;
        private string primDetail = "";
        private readonly Image[] stars = new Image[3];
        private Button next, doubleButton;
        private Text doubleText;
        private int earned;

        public bool IsOpen => gameObject.activeSelf;

        public void Build(RectTransform canvas)
        {
            var root = (RectTransform)transform;
            root.SetParent(canvas, false);
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = new Vector2(-200, -400);
            root.offsetMax = new Vector2(200, 400);
            dim = gameObject.AddComponent<Image>();
            dim.color = new Color(0.05f, 0.06f, 0.1f, 0.25f);

            card = UiFactory.Rect("Card", root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            UiFactory.Place(card, new Vector2(0.5f, 0.5f), new Vector2(880, 1120));
            var bg = card.gameObject.AddComponent<Image>();
            bg.sprite = IconFactory.Rounded;
            bg.type = Image.Type.Sliced;
            bg.color = UiFactory.PanelColor;

            var title = UiFactory.Rect("Title", card, new Vector2(0, 0.83f), new Vector2(1, 0.96f));
            UiFactory.Label(title, Loc.T("win.title"), 92, TextAnchor.MiddleCenter, true).color = new Color(1f, 0.84f, 0.32f);

            var starRow = UiFactory.Rect("Stars", card, new Vector2(0, 0.65f), new Vector2(1, 0.83f));
            for (int i = 0; i < 3; i++)
            {
                var slot = UiFactory.Rect($"Star{i}", starRow, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
                float size = i == 1 ? 190 : 150;
                UiFactory.Place(slot, new Vector2(0.5f, 0.5f), new Vector2(size, size), new Vector2((i - 1) * 200, i == 1 ? 18 : 0));
                var empty = slot.gameObject.AddComponent<Image>();
                empty.sprite = IconFactory.Get(IconKind.StarEmpty);
                empty.color = new Color(1f, 1f, 1f, 0.25f);
                var fill = UiFactory.Icon(slot, IconKind.Star, size, new Color(1f, 0.82f, 0.25f));
                stars[i] = fill;
            }

            moves = UiFactory.Label(UiFactory.Rect("Moves", card, new Vector2(0, 0.585f), new Vector2(1, 0.65f)), "", 46);
            moves.color = new Color(1f, 1f, 1f, 0.75f);
            // CB Coin earned (Ana Belge §17.2), counting up, with its lines (clear · three stars) under it.
            var primRow = UiFactory.Rect("Prim", card, new Vector2(0, 0.51f), new Vector2(1, 0.59f));
            var coin = UiFactory.Icon(primRow, IconKind.Coin, 80, new Color(1f, 0.78f, 0.2f));
            coin.rectTransform.anchoredPosition = new Vector2(-150, 0);
            prim = UiFactory.Label(primRow, "", 64, TextAnchor.MiddleCenter, true);
            prim.rectTransform.offsetMin = new Vector2(60, 0);
            prim.color = new Color(1f, 0.86f, 0.4f);
            primLines = UiFactory.Label(UiFactory.Rect("PrimLines", card, new Vector2(0, 0.465f), new Vector2(1, 0.515f)), "", 32);
            primLines.color = new Color(1f, 1f, 1f, 0.6f);

            doubleButton = UiFactory.Button(card, new Color(0.52f, 0.33f, 0.86f), () => DoubleClicked?.Invoke(), IconKind.Ad, "", 40);
            UiFactory.Place((RectTransform)doubleButton.transform, new Vector2(0.5f, 0.375f), new Vector2(740, 130));
            ((RectTransform)UiFactory.FaceOf(doubleButton).Find("Icon")).anchoredPosition = new Vector2(-300, 0);
            doubleText = UiFactory.FaceOf(doubleButton).GetComponentInChildren<Text>();
            doubleText.rectTransform.offsetMin = new Vector2(70, 0);
            next = UiFactory.Button(card, UiFactory.AccentColor, () => NextClicked?.Invoke(), IconKind.Next, Loc.T("win.next"), 58);
            UiFactory.Place((RectTransform)next.transform, new Vector2(0.5f, 0.235f), new Vector2(640, 150));
            var replay = UiFactory.Button(card, UiFactory.ButtonColor, () => ReplayClicked?.Invoke(), IconKind.Restart);
            UiFactory.Place((RectTransform)replay.transform, new Vector2(0.17f, 0.08f), new Vector2(150, 130));
            var levels = UiFactory.Button(card, UiFactory.ButtonColor, () => LevelsClicked?.Invoke(), IconKind.Map);
            UiFactory.Place((RectTransform)levels.transform, new Vector2(0.37f, 0.08f), new Vector2(150, 130));
            var depot = UiFactory.Button(card, UiFactory.WarmColor, () => DepotClicked?.Invoke(), null, Loc.T("ui.home"), 42);
            UiFactory.Place((RectTransform)depot.transform, new Vector2(0.72f, 0.08f), new Vector2(390, 130));
            gameObject.SetActive(false);
        }

        /// <summary>The CB Coin this win paid (call before Show); all zero hides the line.</summary>
        public void SetReward(int clear, int threeStars, string clearLabel = null)
        {
            primAmount = clear + threeStars;
            var parts = new System.Collections.Generic.List<string>();
            if (clear > 0) parts.Add($"{clearLabel ?? Loc.T("win.yard")} +{clear}");
            if (threeStars > 0) parts.Add(Loc.F("win.stars", threeStars));
            primDetail = string.Join("  ·  ", parts);
            earned = primAmount;
        }

        /// <summary>The rewarded ad was watched: the line doubles and the button goes away.</summary>
        public void ApplyDouble()
        {
            primDetail += "  ·  " + Loc.F("win.adx2", earned);
            earned *= 2;
            prim.text = Loc.F("coin.plus", earned);
            primLines.text = primDetail;
            doubleButton.gameObject.SetActive(false);
        }

        /// <param name="bestMoves">Shortest solution, or -1 when unknown.</param>
        public void Show(int moveCount, int bestMoves, int starCount, bool isLastLevel)
        {
            prim.transform.parent.gameObject.SetActive(primAmount > 0);
            prim.text = "+0";
            primLines.text = primAmount > 0 ? primDetail : Loc.T("win.claimed");
            moves.text = bestMoves >= 0 ? Loc.F("win.moves", moveCount, bestMoves) : Loc.F("hud.moves1", moveCount);
            next.gameObject.SetActive(!isLastLevel);
            doubleButton.gameObject.SetActive(earned > 0);
            doubleText.text = Loc.F("win.double", earned);
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            StartCoroutine(Reveal(starCount));
        }

        public void Hide()
        {
            StopAllCoroutines();
            gameObject.SetActive(false);
        }

        private IEnumerator Reveal(int starCount)
        {
            foreach (var s in stars) s.transform.localScale = Vector3.zero;
            const float rise = 0.35f;
            for (float t = 0; t < rise; t += Time.deltaTime)
            {
                float k = t / rise;
                float back = 1f + 2.2f * Mathf.Pow(k - 1f, 3f) + 1.2f * Mathf.Pow(k - 1f, 2f); // ease-out with a small overshoot
                card.anchoredPosition = new Vector2(0, Mathf.LerpUnclamped(-1400f, 0f, back));
                dim.color = new Color(0.05f, 0.06f, 0.1f, 0.25f + 0.25f * k);
                yield return null;
            }
            card.anchoredPosition = Vector2.zero;
            for (int i = 0; i < starCount; i++)
            {
                yield return new WaitForSeconds(0.18f);
                StarShown?.Invoke(i);
                for (float t = 0; t < 0.2f; t += Time.deltaTime)
                {
                    float k = t / 0.2f;
                    stars[i].transform.localScale = Vector3.one * (k < 0.7f ? k / 0.7f * 1.25f : Mathf.Lerp(1.25f, 1f, (k - 0.7f) / 0.3f));
                    yield return null;
                }
                stars[i].transform.localScale = Vector3.one;
            }
            for (float t = 0; t < 0.6f && primAmount > 0; t += Time.deltaTime)
            {
                prim.text = Loc.F("coin.plus", Mathf.RoundToInt(primAmount * t / 0.6f));
                yield return null;
            }
            prim.text = Loc.F("coin.plus", primAmount);
            primAmount = 0;
        }
    }
}
