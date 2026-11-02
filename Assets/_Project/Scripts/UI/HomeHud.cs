using System;
using System.Collections.Generic;
using CrateBoss.View;
using CrateBoss.Localization;
using UnityEngine;
using UnityEngine.UI;

namespace CrateBoss.UI
{
    /// <summary>
    /// The home screen's overlay (Ana Belge §11.2, 390×844 reference; candy style 2026-11-01): top left the rank, the logo and
    /// a progress bar
    /// to the next rank (no numbers); top right the CB Coin balance; right edge "+30 CB Coin · Reklam izle · 2/3"; left edge
    /// Wardrobe, Edit, Levels / Settings; the crate pile button with its count; bottom the big orange OYNA and the next
    /// level's name. Edit mode swaps it for the placement bar.
    /// </summary>
    public class HomeHud : MonoBehaviour
    {
        public event Action PlayClicked, CratesClicked, AdCoinsClicked, WardrobeClicked, EditClicked, LevelsClicked, SettingsClicked;

        private Text rankName, coins, nextName, adLabel, crateCount;
        private Image rankFill;
        private GameObject crateBadge;
        private RectTransform root, play, nextPill;

        public void Build(RectTransform canvas)
        {
            root = (RectTransform)transform;
            root.SetParent(canvas, false);
            root.anchorMin = Vector2.zero; root.anchorMax = Vector2.one; root.offsetMin = root.offsetMax = Vector2.zero;

            // Candy style (owner 2026-11-01, after a reference picture): dark glassy pills on top with their icon sticking out,
            // the game logo under them, the rank bar under the logo, round buttons with a caption pill below.
            var top = UiFactory.Rect("Top", root, new Vector2(0, 1), new Vector2(1, 1));
            // Owner 2026-11-01 "üst kısım ekrana sığmıyor": the bar hangs from the top of the safe area.
            top.pivot = new Vector2(0.5f, 1f);
            top.sizeDelta = new Vector2(0, 130);
            top.anchoredPosition = new Vector2(0, -24);
            var rank = Pill(top, new Vector2(0, 0.5f), new Vector2(430, 96), new Vector2(285, 0));
            UiFactory.Icon(rank, IconKind.Star, 120, new Color(1f, 0.78f, 0.15f)).rectTransform.anchoredPosition = new Vector2(-205, 4);
            rankName = UiFactory.Label(UiFactory.Rect("Name", rank, new Vector2(0.2f, 0f), new Vector2(0.97f, 1f)), "", 44, TextAnchor.MiddleCenter, true);
            var coinRt = Pill(top, new Vector2(1, 0.5f), new Vector2(300, 96), new Vector2(-385, 0));
            UiFactory.Icon(coinRt, IconKind.Coin, 104, new Color(1f, 0.8f, 0.2f)).rectTransform.anchoredPosition = new Vector2(150, 4);
            coins = UiFactory.Label(UiFactory.Rect("Count", coinRt, new Vector2(0.04f, 0), new Vector2(0.8f, 1)), "0", 52, TextAnchor.MiddleCenter, true);
            // Owner 2026-11-01: the settings (sound, quality, language, privacy) open from the top right of the home screen.
            var settings = UiFactory.Button(top, UiFactory.ButtonColor, () => SettingsClicked?.Invoke(), IconKind.Settings, null, 40, true);
            UiFactory.Place((RectTransform)settings.transform, new Vector2(1, 0.5f), new Vector2(124, 124), new Vector2(-86, 0));

            // The logo, and the way to the next rank as a bar under it (no numbers).
            var logo = GameLogo.Build(root, 0.82f);
            logo.anchorMin = logo.anchorMax = new Vector2(0.5f, 1f);
            logo.anchoredPosition = new Vector2(0, -330);
            var bar = UiFactory.Rect("RankBar", root, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            UiFactory.Place(bar, new Vector2(0.5f, 1f), new Vector2(560, 60), new Vector2(0, -520));
            UiFactory.Panel("Rim", bar, Color.white, true);
            var track = UiFactory.Panel("Track", bar, new Color(0.1f, 0.2f, 0.36f), true).rectTransform;
            track.offsetMin = new Vector2(7, 7); track.offsetMax = new Vector2(-7, -7);
            rankFill = UiFactory.Panel("Fill", track, new Color(0.36f, 0.86f, 0.25f), true);
            rankFill.gameObject.AddComponent<UiGradient>();
            rankFill.rectTransform.anchorMin = Vector2.zero; rankFill.rectTransform.anchorMax = new Vector2(0.3f, 1f);
            rankFill.rectTransform.offsetMin = rankFill.rectTransform.offsetMax = Vector2.zero;
            UiFactory.Icon(bar, IconKind.Star, 100, new Color(1f, 0.78f, 0.15f)).rectTransform.anchoredPosition = new Vector2(285, 0);

            // Right edge: +30 CB Coin through an ad.
            var ad = UiFactory.Button(root, new Color(0.62f, 0.36f, 0.95f), () => AdCoinsClicked?.Invoke(), IconKind.Ad, null, 40, true);
            UiFactory.Place((RectTransform)ad.transform, new Vector2(1, 0.6f), new Vector2(150, 150), new Vector2(-110, 0));
            adLabel = Caption((RectTransform)ad.transform, "");

            // Left edge: edit (only while the Boss's city is shown) and levels. Owner 2026-11-02: no wardrobe, the worker is
            // always the same.
            float y = 0.6f;
            var left = new List<(IconKind, Action, string)>();
            if (HomeView.CityShown) left.Add((IconKind.Map, () => EditClicked?.Invoke(), "home.edit"));
            left.Add((IconKind.Levels, () => LevelsClicked?.Invoke(), "home.levels"));
            foreach (var (icon, act, key) in left)
            {
                var b = UiFactory.Button(root, UiFactory.ButtonColor, act, icon, null, 40, true);
                UiFactory.Place((RectTransform)b.transform, new Vector2(0, y), new Vector2(150, 150), new Vector2(110, 0));
                Caption((RectTransform)b.transform, Loc.T(key));
                y -= 0.13f;
            }

            // Crate pile button (the pile itself stands in the scene).
            var crates = UiFactory.Button(root, new Color(0.94f, 0.27f, 0.27f), () => CratesClicked?.Invoke(), IconKind.Next, Loc.T("home.crates"), 40);
            UiFactory.Place((RectTransform)crates.transform, new Vector2(1, 0.3f), new Vector2(360, 140), new Vector2(-200, 0));
            crateBadge = UiFactory.Panel("Badge", crates.transform, new Color(1f, 0.82f, 0.25f), true).gameObject;
            UiFactory.Place((RectTransform)crateBadge.transform, new Vector2(1, 1), new Vector2(84, 84), new Vector2(-10, -10));
            crateCount = UiFactory.Label(crateBadge.transform, "0", 44, TextAnchor.MiddleCenter, true);
            crateCount.color = UiFactory.InkColor;
            crates.gameObject.SetActive(HomeView.CityShown); // the crates furnish the city

            // Bottom: OYNA and the next level's name in a pill.
            var playButton = UiFactory.Button(root, new Color(0.3f, 0.82f, 0.28f), () => PlayClicked?.Invoke(), null, Loc.T("ui.playbig"), 84);
            play = (RectTransform)playButton.transform;
            UiFactory.Place(play, new Vector2(0.5f, 0), new Vector2(640, 200), new Vector2(0, 200));
            var next = Pill(root, new Vector2(0.5f, 0), new Vector2(620, 70), new Vector2(0, 80));
            nextPill = next;
            nextName = UiFactory.Label(next, "", 38, TextAnchor.MiddleCenter, true);
            gameObject.SetActive(false);
        }

        /// <param name="rankProgress">Share of the current rank's levels cleared (0–1), drawn as a bar without numbers.</param>
        public void Set(string rank, float rankProgress, int balance, int crates, int adsLeft, int adsPerDay, string next)
        {
            rankName.text = rank;
            rankFill.rectTransform.anchorMax = new Vector2(Mathf.Clamp01(rankProgress), 1f);
            coins.text = balance.ToString();
            crateCount.text = crates.ToString();
            crateBadge.SetActive(crates > 0);
            adLabel.text = $"+30 · {adsLeft}/{adsPerDay}";
            nextName.text = next;
        }

        /// <summary>Room kept free at the bottom (the banner, reference units): OYNA and the next level's name sit above it.</summary>
        public void SetBottomInset(float height)
        {
            play.anchoredPosition = new Vector2(0, 200 + height);
            nextPill.anchoredPosition = new Vector2(0, 80 + height);
        }

        public void Show(bool on) => gameObject.SetActive(on);

        /// <summary>A dark glassy pill with a thin white edge (the candy style's counters and captions).</summary>
        private static RectTransform Pill(Transform parent, Vector2 anchor, Vector2 size, Vector2 offset)
        {
            var edge = UiFactory.Panel("Pill", parent, new Color(1f, 1f, 1f, 0.9f), true).rectTransform;
            UiFactory.Place(edge, anchor, size, offset);
            var fill = UiFactory.Panel("Fill", edge, UiFactory.PillColor, true).rectTransform;
            fill.offsetMin = new Vector2(4, 4); fill.offsetMax = new Vector2(-4, -4);
            fill.GetComponent<Image>().color = new Color(0.08f, 0.2f, 0.32f, 1f);
            return edge;
        }

        /// <summary>The caption pill under a round button.</summary>
        private static Text Caption(RectTransform button, string text)
        {
            var pill = Pill(button, new Vector2(0.5f, 0f), new Vector2(190, 52), new Vector2(0, -10));
            return UiFactory.Label(pill, text, 28, TextAnchor.MiddleCenter, true);
        }
    }
}
