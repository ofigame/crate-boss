using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using CrateBoss.Localization;

namespace CrateBoss.UI
{
    /// <summary>
    /// "Terfi!" (Ana Belge §7): a rank is done, the worker climbs to the next rank. Big card with the new rank, the
    /// gift and paper confetti falling over the screen. The full promotion moment comes in F6.
    /// </summary>
    public class PromotionView : MonoBehaviour
    {
        private static readonly Color[] ConfettiColors =
        {
            new Color(1f, 0.55f, 0.16f), new Color(0.2f, 0.72f, 0.45f), new Color(0.3f, 0.55f, 0.95f),
            new Color(1f, 0.82f, 0.25f), new Color(0.9f, 0.3f, 0.45f),
        };

        public event Action Closed;

        private RectTransform card, confetti;
        private Text title, gift;

        public bool IsOpen => gameObject.activeSelf;

        public void Build(RectTransform canvas)
        {
            var root = (RectTransform)transform;
            root.SetParent(canvas, false);
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = new Vector2(-200, -400);
            root.offsetMax = new Vector2(200, 400);
            gameObject.AddComponent<Image>().color = new Color(0.05f, 0.06f, 0.1f, 0.6f);
            confetti = UiFactory.Stretch("Confetti", root);

            card = UiFactory.Rect("Card", root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            UiFactory.Place(card, new Vector2(0.5f, 0.5f), new Vector2(860, 900));
            var bg = card.gameObject.AddComponent<Image>();
            bg.sprite = IconFactory.Rounded;
            bg.type = Image.Type.Sliced;
            bg.color = new Color(0.12f, 0.14f, 0.2f, 1f);
            var heading = UiFactory.Label(UiFactory.Rect("Heading", card, new Vector2(0, 0.8f), new Vector2(1, 0.95f)), Loc.T("promo.title"), 110, TextAnchor.MiddleCenter, true);
            heading.color = new Color(1f, 0.58f, 0.18f);
            var outline = heading.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.36f, 0.19f, 0.07f);
            outline.effectDistance = new Vector2(4f, -4f);
            var star = UiFactory.Icon(card, IconKind.Star, 210, new Color(1f, 0.82f, 0.25f));
            star.rectTransform.anchorMin = star.rectTransform.anchorMax = new Vector2(0.5f, 0.64f);
            var label = UiFactory.Label(UiFactory.Rect("Label", card, new Vector2(0, 0.44f), new Vector2(1, 0.51f)), Loc.T("promo.label"), 38);
            label.color = new Color(1f, 1f, 1f, 0.65f);
            title = UiFactory.Label(UiFactory.Rect("Title", card, new Vector2(0, 0.33f), new Vector2(1, 0.45f)), "", 74, TextAnchor.MiddleCenter, true);
            gift = UiFactory.Label(UiFactory.Rect("Gift", card, new Vector2(0, 0.24f), new Vector2(1, 0.33f)), "", 50, TextAnchor.MiddleCenter, true);
            gift.color = new Color(1f, 0.86f, 0.4f);
            var ok = UiFactory.Button(card, UiFactory.AccentColor, Hide, null, Loc.T("promo.ok"), 56);
            UiFactory.Place((RectTransform)ok.transform, new Vector2(0.5f, 0.11f), new Vector2(560, 150));
            gameObject.SetActive(false);
        }

        public void Show(string newTitle, int coins) => Show(newTitle, Loc.F("coin.plus", coins));

        /// <summary>The new rank and what it brings (Ana Belge §11.4: a promotion crate, a new land, the new crates).</summary>
        public void Show(string newTitle, string giftLine)
        {
            title.text = newTitle;
            gift.text = giftLine;
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            StopAllCoroutines();
            StartCoroutine(Pop());
            StartCoroutine(Confetti());
        }

        public void Hide()
        {
            if (!gameObject.activeSelf) return;
            StopAllCoroutines();
            for (int i = confetti.childCount - 1; i >= 0; i--) Destroy(confetti.GetChild(i).gameObject);
            gameObject.SetActive(false);
            Closed?.Invoke();
        }

        private IEnumerator Pop()
        {
            for (float t = 0; t < 0.35f; t += Time.deltaTime)
            {
                float k = t / 0.35f;
                card.localScale = Vector3.one * (k < 0.7f ? Mathf.Lerp(0.4f, 1.12f, k / 0.7f) : Mathf.Lerp(1.12f, 1f, (k - 0.7f) / 0.3f));
                yield return null;
            }
            card.localScale = Vector3.one;
        }

        private IEnumerator Confetti()
        {
            var rng = new System.Random(5);
            var bits = new RectTransform[70];
            var speed = new Vector3[bits.Length];
            for (int i = 0; i < bits.Length; i++)
            {
                bits[i] = UiFactory.Rect("Bit", confetti, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
                UiFactory.Place(bits[i], new Vector2((float)rng.NextDouble(), 1f), new Vector2(26, 16), new Vector2(0, 60 + (float)rng.NextDouble() * 900));
                var img = bits[i].gameObject.AddComponent<Image>();
                img.color = ConfettiColors[i % ConfettiColors.Length];
                img.raycastTarget = false;
                speed[i] = new Vector3(-60 + (float)rng.NextDouble() * 120, 380 + (float)rng.NextDouble() * 320, -300 + (float)rng.NextDouble() * 600);
            }
            for (float t = 0; t < 6f; t += Time.deltaTime)
            {
                for (int i = 0; i < bits.Length; i++)
                {
                    var p = bits[i].anchoredPosition;
                    p += new Vector2(speed[i].x + Mathf.Sin(t * 3f + i) * 80f, -speed[i].y) * Time.deltaTime;
                    if (p.y < -2400f) p.y += 2500f;
                    bits[i].anchoredPosition = p;
                    bits[i].localRotation = Quaternion.Euler(0, 0, speed[i].z * t);
                }
                yield return null;
            }
        }
    }
}
