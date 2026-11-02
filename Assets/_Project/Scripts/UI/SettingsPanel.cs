using System;
using CrateBoss.Localization;
using UnityEngine;
using UnityEngine.UI;

namespace CrateBoss.UI
{
    /// <summary>
    /// Settings card opened from the top right of the home screen (owner 2026-11-01: the sound, quality, language and privacy
    /// buttons left the bottom of the level list, where the banner covered them). One big button each, one under the other.
    /// </summary>
    public class SettingsPanel : MonoBehaviour
    {
        public event Action SoundClicked, QualityClicked, LanguageClicked, PrivacyClicked;

        private Image soundIcon;
        private Text soundLabel, qualityLabel;

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
            UiFactory.Place(card, new Vector2(0.5f, 0.5f), new Vector2(820, 940));
            var bg = card.gameObject.AddComponent<Image>();
            bg.sprite = IconFactory.Rounded;
            bg.type = Image.Type.Sliced;
            bg.color = new Color(0.12f, 0.14f, 0.2f, 1f);
            UiFactory.Label(UiFactory.Rect("Title", card, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -150), new Vector2(0, -40)),
                Loc.T("ui.settings"), 66, TextAnchor.MiddleCenter, true);
            var close = UiFactory.Button(card, UiFactory.ButtonColor, Hide, IconKind.Close, null, 40, true);
            UiFactory.Place((RectTransform)close.transform, new Vector2(1, 1), new Vector2(110, 110), new Vector2(-70, -70));

            var sound = Row(card, 0, IconKind.Sound, Loc.T("ui.sound"), () => SoundClicked?.Invoke());
            soundIcon = UiFactory.FaceOf(sound).Find("Icon")?.GetComponent<Image>();
            soundLabel = UiFactory.FaceOf(sound).GetComponentInChildren<Text>();
            qualityLabel = UiFactory.FaceOf(Row(card, 1, IconKind.Quality, Loc.T("ui.quality"), () => QualityClicked?.Invoke())).GetComponentInChildren<Text>();
            Row(card, 2, null, Loc.T("ui.language"), () => { Hide(); LanguageClicked?.Invoke(); });
            Row(card, 3, null, Loc.T("ui.privacy"), () => PrivacyClicked?.Invoke());
            gameObject.SetActive(false);
        }

        private static Button Row(RectTransform card, int index, IconKind? icon, string text, Action click)
        {
            var b = UiFactory.Button(card, UiFactory.ButtonColor, click, icon, text, 40);
            UiFactory.Place((RectTransform)b.transform, new Vector2(0.5f, 1), new Vector2(640, 140), new Vector2(0, -260 - index * 170));
            return b;
        }

        public void Show(bool soundOn, string quality)
        {
            SetSound(soundOn);
            SetQuality(quality);
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
        }

        public void SetSound(bool on)
        {
            if (soundIcon != null) soundIcon.sprite = IconFactory.Get(on ? IconKind.Sound : IconKind.SoundOff);
            if (soundLabel != null) soundLabel.text = Loc.T("ui.sound");
        }

        public void SetQuality(string label)
        {
            if (qualityLabel != null) qualityLabel.text = label;
        }

        public void Hide() => gameObject.SetActive(false);
    }
}
