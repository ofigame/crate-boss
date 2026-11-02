using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CrateBoss.UI
{
    /// <summary>
    /// A small centred card over a dimmed screen: icon, title, a line or two and one to three stacked buttons (login
    /// gift, item card, hint choices, "no ad now"...). The corner X closes it without doing anything.
    /// </summary>
    public class MessageCard : MonoBehaviour
    {
        private const float ButtonHeight = 140f, ButtonGap = 22f, TopPart = 560f;

        private RectTransform card;
        private Text title, body;
        private Image icon;
        private readonly List<GameObject> buttons = new List<GameObject>();

        public bool IsOpen => gameObject.activeSelf;

        public void Build(RectTransform canvas)
        {
            var root = (RectTransform)transform;
            root.SetParent(canvas, false);
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = new Vector2(-200, -400);
            root.offsetMax = new Vector2(200, 400);
            gameObject.AddComponent<Image>().color = new Color(0.05f, 0.06f, 0.1f, 0.55f);
            card = UiFactory.Rect("Card", root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            UiFactory.Place(card, new Vector2(0.5f, 0.5f), new Vector2(860, 760));
            var bg = card.gameObject.AddComponent<Image>();
            bg.sprite = IconFactory.Rounded;
            bg.type = Image.Type.Sliced;
            bg.color = new Color(0.12f, 0.14f, 0.2f, 1f);
            icon = UiFactory.Icon(card, IconKind.Coin, 150, new Color(1f, 0.78f, 0.2f));
            UiFactory.Place(icon.rectTransform, new Vector2(0.5f, 1f), new Vector2(150, 150), new Vector2(0, -120));
            title = UiFactory.Label(UiFactory.Rect("Title", card, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -290), new Vector2(0, -200)), "", 62, TextAnchor.MiddleCenter, true);
            title.color = new Color(1f, 0.84f, 0.32f);
            body = UiFactory.Label(UiFactory.Rect("Body", card, new Vector2(0, 1), new Vector2(1, 1), new Vector2(60, -TopPart + 20), new Vector2(-60, -300)), "", 42);
            body.horizontalOverflow = HorizontalWrapMode.Wrap;
            body.resizeTextForBestFit = true;
            body.resizeTextMinSize = 28;
            body.resizeTextMaxSize = 42;
            body.verticalOverflow = VerticalWrapMode.Truncate;
            var close = UiFactory.Button(card, UiFactory.ButtonColor, Hide, IconKind.Close, null, 46, true);
            UiFactory.Place((RectTransform)close.transform, new Vector2(1, 1), new Vector2(100, 100), new Vector2(-55, -55));
            gameObject.SetActive(false);
        }

        /// <summary>One main button and an optional plain second one (kept for the simple cards).</summary>
        public void Show(IconKind iconKind, string heading, string text, string ok, Action onOk, string cancel = null, Action onCancel = null, bool okEnabled = true)
        {
            var options = new List<CardOption> { new CardOption(ok, onOk, UiFactory.AccentColor, okEnabled) };
            if (cancel != null) options.Add(new CardOption(cancel, onCancel, UiFactory.ButtonColor));
            Show(iconKind, heading, text, options.ToArray());
        }

        public void Show(IconKind iconKind, string heading, string text, params CardOption[] options)
        {
            icon.sprite = IconFactory.Get(iconKind);
            title.text = heading;
            body.text = text;
            foreach (var b in buttons) Destroy(b);
            buttons.Clear();
            float height = TopPart + options.Length * (ButtonHeight + ButtonGap) + 40f;
            card.sizeDelta = new Vector2(860, height);
            for (int i = 0; i < options.Length; i++)
            {
                var o = options[i];
                var b = UiFactory.Button(card, o.Color, () => Close(o.OnClick), o.Icon, o.Label, i == 0 ? 46 : 40);
                UiFactory.Place((RectTransform)b.transform, new Vector2(0.5f, 0f), new Vector2(680, ButtonHeight),
                    new Vector2(0, 40f + ButtonHeight / 2f + (options.Length - 1 - i) * (ButtonHeight + ButtonGap)));
                b.interactable = o.Enabled;
                if (o.Icon.HasValue)
                {
                    ((RectTransform)UiFactory.FaceOf(b).Find("Icon")).anchoredPosition = new Vector2(-280, 0);
                    UiFactory.FaceOf(b).GetComponentInChildren<Text>().rectTransform.offsetMin = new Vector2(70, 0);
                }
                buttons.Add(b.gameObject);
            }
            transform.SetAsLastSibling();
            gameObject.SetActive(true);
        }

        public void Hide() => gameObject.SetActive(false);

        private void Close(Action then)
        {
            gameObject.SetActive(false);
            then?.Invoke();
        }
    }
}
