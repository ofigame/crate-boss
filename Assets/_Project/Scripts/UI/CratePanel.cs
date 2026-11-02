using System;
using System.Collections;
using System.Collections.Generic;
using CrateBoss.Localization;
using UnityEngine;
using UnityEngine.UI;

namespace CrateBoss.UI
{
    /// <summary>
    /// Opening a delivery crate (Ana Belge §12.2): the crate comes to the middle and wobbles; "Aç (30)" or "Reklam izle ve aç";
    /// the lid pops (confetti) and three item cards come up; one is chosen ("Yerleştir" now or "Sonra"), the others go.
    /// </summary>
    public class CratePanel : MonoBehaviour
    {
        public event Action OpenClicked, AdClicked;
        public event Action<string, bool> Chosen; // item id, place now
        public event Action Closed;

        private RectTransform card, crate, cards;
        private Button open, ad;
        private Text heading;
        private readonly List<GameObject> choices = new List<GameObject>();

        public bool IsOpen => gameObject.activeSelf;

        public void Build(RectTransform canvas)
        {
            var root = (RectTransform)transform;
            root.SetParent(canvas, false);
            root.anchorMin = Vector2.zero; root.anchorMax = Vector2.one;
            root.offsetMin = new Vector2(-200, -400); root.offsetMax = new Vector2(200, 400);
            gameObject.AddComponent<Image>().color = new Color(0.05f, 0.06f, 0.1f, 0.65f);
            card = UiFactory.Rect("Card", root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            UiFactory.Place(card, new Vector2(0.5f, 0.5f), new Vector2(940, 1200));
            var bg = card.gameObject.AddComponent<Image>();
            bg.sprite = IconFactory.Rounded; bg.type = Image.Type.Sliced; bg.color = UiFactory.PanelColor;
            heading = UiFactory.Label(UiFactory.Rect("Heading", card, new Vector2(0, 0.86f), new Vector2(1, 0.97f)), "", 64, TextAnchor.MiddleCenter, true);
            crate = UiFactory.Panel("Crate", card, new Color(0.94f, 0.27f, 0.27f), true).rectTransform;
            UiFactory.Place(crate, new Vector2(0.5f, 0.6f), new Vector2(360, 340));
            UiFactory.Icon(crate, IconKind.Next, 180, Color.white).rectTransform.localRotation = Quaternion.Euler(0, 0, 90);
            cards = UiFactory.Rect("Cards", card, new Vector2(0, 0.2f), new Vector2(1, 0.84f));
            open = UiFactory.Button(card, UiFactory.AccentColor, () => OpenClicked?.Invoke(), IconKind.Coin, "", 50);
            UiFactory.Place((RectTransform)open.transform, new Vector2(0.5f, 0.24f), new Vector2(640, 150));
            ad = UiFactory.Button(card, new Color(0.52f, 0.33f, 0.86f), () => AdClicked?.Invoke(), IconKind.Ad, Loc.T("crate.ad"), 44);
            UiFactory.Place((RectTransform)ad.transform, new Vector2(0.5f, 0.11f), new Vector2(640, 140));
            var close = UiFactory.Button(card, UiFactory.ButtonColor, () => { Hide(); Closed?.Invoke(); }, IconKind.Close, null, 40, true);
            UiFactory.Place((RectTransform)close.transform, new Vector2(1, 1), new Vector2(110, 110), new Vector2(-40, -40));
            gameObject.SetActive(false);
        }

        /// <param name="price">CB Coin to open (0 = free: the first crate, or a promotion crate).</param>
        public void ShowClosed(string title, int price, bool canPay, bool adAvailable)
        {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            ClearChoices();
            heading.text = title;
            crate.gameObject.SetActive(true);
            UiFactory.FaceOf(open).GetComponentInChildren<Text>().text = price > 0 ? Loc.F("crate.open", price) : Loc.T("crate.openfree");
            open.gameObject.SetActive(true);
            open.interactable = price == 0 || canPay;
            ad.gameObject.SetActive(price > 0 && !canPay && adAvailable);
            StopAllCoroutines();
            StartCoroutine(Wobble());
        }

        /// <summary>The lid pops and the three cards come up.</summary>
        public void ShowChoices(IReadOnlyList<(string id, string name, string size, Color color)> items)
        {
            StopAllCoroutines();
            open.gameObject.SetActive(false);
            ad.gameObject.SetActive(false);
            crate.gameObject.SetActive(false);
            heading.text = Loc.T("crate.choose");
            ClearChoices();
            for (int i = 0; i < items.Count; i++)
            {
                var it = items[i];
                var c = UiFactory.Panel("Choice", cards, new Color(1f, 1f, 1f, 0.95f), true).rectTransform;
                c.anchorMin = new Vector2(0.04f + i * 0.32f, 0.08f); c.anchorMax = new Vector2(0.32f + i * 0.32f, 0.95f);
                c.offsetMin = c.offsetMax = Vector2.zero;
                var swatch = UiFactory.Panel("Swatch", c, it.color, true).rectTransform;
                swatch.anchorMin = new Vector2(0.15f, 0.52f); swatch.anchorMax = new Vector2(0.85f, 0.9f); swatch.offsetMin = swatch.offsetMax = Vector2.zero;
                var n = UiFactory.Label(UiFactory.Rect("Name", c, new Vector2(0.04f, 0.3f), new Vector2(0.96f, 0.5f)), it.name, 34, TextAnchor.MiddleCenter, true);
                n.color = UiFactory.InkColor;
                var size = UiFactory.Label(UiFactory.Rect("Size", c, new Vector2(0, 0.22f), new Vector2(1, 0.31f)), Loc.T("size." + it.size), 26);
                size.color = new Color(0.3f, 0.32f, 0.4f);
                string id = it.id;
                var place = UiFactory.Button(c, UiFactory.AccentColor, () => { Hide(); Chosen?.Invoke(id, true); }, null, Loc.T("crate.place"), 30);
                UiFactory.Place((RectTransform)place.transform, new Vector2(0.5f, 0.12f), new Vector2(230, 90));
                var later = UiFactory.Button(c, UiFactory.ButtonColor, () => { Hide(); Chosen?.Invoke(id, false); }, null, Loc.T("crate.later"), 26);
                UiFactory.Place((RectTransform)later.transform, new Vector2(0.5f, 0.0f), new Vector2(200, 70), new Vector2(0, -20));
                choices.Add(c.gameObject);
                StartCoroutine(Pop(c, i * 0.12f));
            }
        }

        void ClearChoices()
        {
            foreach (var c in choices) Destroy(c);
            choices.Clear();
        }

        public void Hide() => gameObject.SetActive(false);

        IEnumerator Wobble()
        {
            for (float t = 0; ; t += Time.deltaTime)
            {
                crate.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(t * 9f) * 6f * (0.5f + 0.5f * Mathf.Sin(t * 1.7f)));
                yield return null;
            }
        }

        static IEnumerator Pop(RectTransform t, float delay)
        {
            t.localScale = Vector3.zero;
            yield return new WaitForSeconds(delay);
            for (float k = 0; k < 0.3f; k += Time.deltaTime)
            {
                float x = k / 0.3f;
                t.localScale = Vector3.one * (x < 0.7f ? Mathf.Lerp(0f, 1.1f, x / 0.7f) : Mathf.Lerp(1.1f, 1f, (x - 0.7f) / 0.3f));
                yield return null;
            }
            t.localScale = Vector3.one;
        }
    }
}
