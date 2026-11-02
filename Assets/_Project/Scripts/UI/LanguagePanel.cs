using System;
using CrateBoss.Localization;
using UnityEngine;
using UnityEngine.UI;

namespace CrateBoss.UI
{
    /// <summary>
    /// Language list of the settings (Ana Belge §18): the nine languages, each written in its own name, three per row.
    /// The current one is orange. Picking one fires <see cref="Picked"/>; the game reloads its screens in that language.
    /// </summary>
    public class LanguagePanel : MonoBehaviour
    {
        public event Action<string> Picked;

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

            var card = UiFactory.Rect("Card", root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            UiFactory.Place(card, new Vector2(0.5f, 0.5f), new Vector2(900, 820));
            var bg = card.gameObject.AddComponent<Image>();
            bg.sprite = IconFactory.Rounded;
            bg.type = Image.Type.Sliced;
            bg.color = new Color(0.12f, 0.14f, 0.2f, 1f);
            UiFactory.Label(UiFactory.Rect("Title", card, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -150), new Vector2(0, -40)),
                Loc.T("ui.language"), 66, TextAnchor.MiddleCenter, true);
            var close = UiFactory.Button(card, UiFactory.ButtonColor, Hide, IconKind.Close, null, 40, true);
            UiFactory.Place((RectTransform)close.transform, new Vector2(1, 1), new Vector2(110, 110), new Vector2(-70, -70));

            var languages = Loc.Languages;
            for (int i = 0; i < languages.Length; i++)
            {
                var language = languages[i];
                bool current = language.Code == Loc.Code;
                var b = UiFactory.Button(card, current ? UiFactory.AccentColor : UiFactory.ButtonColor, () => Pick(language.Code), null, language.NativeName, 40);
                // Native names are shown as they are: Arabic shaped even when the game is in another language.
                var label = UiFactory.FaceOf(b).GetComponentInChildren<Text>();
                label.text = ArabicShaper.ShapeLines(language.NativeName);
                int row = i / 3, column = i % 3;
                UiFactory.Place((RectTransform)b.transform, new Vector2((column + 0.5f) / 3f, 1), new Vector2(260, 130), new Vector2(0, -250 - row * 170));
            }
            gameObject.SetActive(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
        }

        public void Hide() => gameObject.SetActive(false);

        private void Pick(string code)
        {
            Hide();
            if (code != Loc.Code) Picked?.Invoke(code);
        }
    }
}
