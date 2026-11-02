using System;
using System.Collections.Generic;
using CrateBoss.View;
using UnityEngine;
using UnityEngine.UI;
using CrateBoss.Localization;

namespace CrateBoss.UI
{
    /// <summary>
    /// The yard list (owner 2026-10-31, replaces the city map): every level as a big bird's-eye picture of its yard, grouped
    /// by district, with its name and stars. Locked yards are veiled with a padlock.
    /// </summary>
    public class LevelSelectView : MonoBehaviour
    {
        private const float HeaderHeight = 200f;

        public event Action<int> LevelChosen;
        /// <summary>The close button (not a yard being chosen).</summary>
        public event Action Closed;

        private ScrollRect scroll;
        private RectTransform content;

        public bool IsOpen => gameObject.activeSelf;

        public void Build(RectTransform canvas)
        {
            var root = (RectTransform)transform;
            root.SetParent(canvas, false);
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = new Vector2(-200, -400);
            root.offsetMax = new Vector2(200, 400);
            gameObject.AddComponent<Image>().color = new Color(0.10f, 0.12f, 0.18f, 0.97f);
            var inner = UiFactory.Rect("Inner", root, Vector2.zero, Vector2.one, new Vector2(200, 400), new Vector2(-200, -400));

            // Placeholder logo: the game name in orange with a brown outline, "Şehir Haritası" under it.
            var logoRt = UiFactory.Rect("Logo", inner, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -135), new Vector2(0, -25));
            var logo = UiFactory.Label(logoRt, "Crate Boss", 88, TextAnchor.MiddleCenter, true);
            logo.color = new Color(1f, 0.58f, 0.18f);
            var outline = logo.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.36f, 0.19f, 0.07f);
            outline.effectDistance = new Vector2(4f, -4f);
            var titleRt = UiFactory.Rect("Title", inner, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -190), new Vector2(0, -130));
            UiFactory.Label(titleRt, Loc.T("map.title"), 46, TextAnchor.MiddleCenter, true).color = new Color(0.85f, 0.88f, 0.95f);

            var close = UiFactory.Button(inner, UiFactory.ButtonColor, () => { Hide(); Closed?.Invoke(); }, IconKind.Close, null, 46, true);
            UiFactory.Place((RectTransform)close.transform, new Vector2(1, 1), new Vector2(130, 130), new Vector2(-95, -100));
            // Owner 2026-11-01: sound, quality, language and privacy moved to the settings card on the home screen.

            var viewport = UiFactory.Rect("Viewport", inner, Vector2.zero, Vector2.one, new Vector2(0, 40), new Vector2(0, -190));
            viewport.gameObject.AddComponent<RectMask2D>();
            content = UiFactory.Rect("Content", viewport, new Vector2(0, 1), new Vector2(1, 1));
            content.pivot = new Vector2(0.5f, 1f);
            scroll = viewport.gameObject.AddComponent<ScrollRect>();
            scroll.content = content;
            scroll.horizontal = false;
            scroll.viewport = viewport;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            viewport.gameObject.AddComponent<Image>().color = new Color(0, 0, 0, 0); // catches drags
            Hide();
        }

        /// <param name="worlds">Group key per level (index = level index): the rank number, or "" for test folders.</param>
        /// <param name="unlocked">How many levels are open (1-based count).</param>
        /// <param name="thumbOf">Bird's-eye picture of a level's yard (LevelThumbnail).</param>
        /// <param name="nameOf">The level's name in the player's language (never a number).</param>
        /// <param name="leagueOf">Leagues (owner 2026-10-31): the group key is then the league, with its name, its stars and,
        /// while closed, the stars it needs. Null = district headers.</param>
        public void Show(IReadOnlyList<string> worlds, int unlocked, Func<int, int> starsOf, int current, Func<int, Texture2D> thumbOf, Func<int, string> nameOf,
            Func<string, (int number, string name, int stars, int max, int need, bool closed)> leagueOf = null, Func<int, Vector2Int> sizeOf = null)
        {
            // Owner 2026-10-31: instead of the map, a list of big bird's-eye pictures of every yard. Locked yards lie under a
            // dark veil with a padlock in the middle; the next one to open has a lighter veil.
            for (int i = content.childCount - 1; i >= 0; i--) Destroy(content.GetChild(i).gameObject);
            pictures.Clear();
            float y = -30f;
            string world = null;
            float currentY = 0f;
            for (int i = 0; i < worlds.Count; i++)
            {
                if (worlds[i] != world)
                {
                    world = worlds[i];
                    // A band per rank (Ana Belge §7); test folders have no ranks and no bands.
                    if (leagueOf != null)
                    {
                        AddLeagueHeader(leagueOf(world), y);
                        y -= HeaderHeight;
                    }
                }
                // Owner 2026-11-01 "bölümlere tıklayınca çok geç geliyor": the cards come at once, the pictures a few per frame.
                var size = sizeOf != null ? sizeOf(i) : SizeOf(thumbOf(i));
                float cardHeight = AddCard(i, y, size, nameOf(i), i < unlocked, i == unlocked - 1, i == unlocked, starsOf(i));
                if (i == current || (current < 0 && i == unlocked - 1)) currentY = -y + cardHeight / 2f;
                y -= cardHeight + CardGap;
            }
            content.sizeDelta = new Vector2(0, -y + 60f);
            gameObject.SetActive(true);
            if (filling != null) StopCoroutine(filling);
            int focus = current >= 0 ? current : unlocked - 1;
            pictures.Sort((a, b) => Mathf.Abs(a.index - focus).CompareTo(Mathf.Abs(b.index - focus)));
            filling = StartCoroutine(FillPictures(thumbOf));
            transform.SetAsLastSibling();
            Canvas.ForceUpdateCanvases();
            float viewHeight = ((RectTransform)scroll.viewport).rect.height;
            float scrollable = Mathf.Max(1f, content.sizeDelta.y - viewHeight);
            scroll.verticalNormalizedPosition = 1f - Mathf.Clamp01((currentY - viewHeight / 2f) / scrollable);
        }

        private const float CardWidth = 820f, CardGap = 36f, NameHeight = 96f;

        // One yard: the picture (aspect kept), its name and stars under it; veil and padlock while locked. Returns its height.
        private readonly List<(RawImage raw, int index)> pictures = new List<(RawImage, int)>();
        private Coroutine filling;

        private static Vector2Int SizeOf(Texture2D t) => new Vector2Int(t.width, t.height);

        private System.Collections.IEnumerator FillPictures(Func<int, Texture2D> thumbOf)
        {
            for (int i = 0; i < pictures.Count; i++)
            {
                if (pictures[i].raw != null) { pictures[i].raw.texture = thumbOf(pictures[i].index); pictures[i].raw.color = Color.white; }
                if (i % 3 == 2) yield return null;
            }
            filling = null;
        }

        private float AddCard(int index, float y, Vector2Int thumb, string name, bool open, bool newest, bool nextToOpen, int stars)
        {
            float pictureH = Mathf.Min(CardWidth * thumb.y / Mathf.Max(1f, thumb.x), 900f);
            float pictureW = pictureH * thumb.x / Mathf.Max(1f, thumb.y);
            float height = pictureH + NameHeight + 24f;
            var card = UiFactory.Button(content, newest ? UiFactory.AccentColor : open ? UiFactory.ButtonColor : UiFactory.DisabledColor,
                () => { if (open) LevelChosen?.Invoke(index); }, null, null, 30, true);
            card.interactable = open;
            var rt = (RectTransform)card.transform;
            UiFactory.Place(rt, new Vector2(0.5f, 1), new Vector2(CardWidth + 40f, height), new Vector2(0, y - height / 2f));
            var face = UiFactory.FaceOf(card);

            var pic = UiFactory.Rect("Yard", face, new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            UiFactory.Place(pic, new Vector2(0.5f, 1), new Vector2(pictureW, pictureH), new Vector2(0, -12f - pictureH / 2f));
            var raw = pic.gameObject.AddComponent<RawImage>();
            raw.color = new Color(1f, 1f, 1f, 0.15f); // until its picture is drawn
            pictures.Add((raw, index));
            raw.raycastTarget = false;

            var nameRt = UiFactory.Rect("Name", face, new Vector2(0, 0), new Vector2(1, 0), new Vector2(30, 8), new Vector2(open ? -230 : -30, NameHeight));
            var label = UiFactory.Label(nameRt, name, 44, TextAnchor.MiddleLeft, true);
            label.color = open ? Color.white : new Color(1f, 1f, 1f, 0.55f);
            if (open && !newest)
                for (int s = 0; s < 3; s++)
                {
                    var star = UiFactory.Rect("Star", face, new Vector2(1, 0), new Vector2(1, 0));
                    UiFactory.Place(star, new Vector2(1, 0), new Vector2(60, 60), new Vector2(-190 + s * 64, NameHeight / 2f + 6));
                    var img = star.gameObject.AddComponent<Image>();
                    img.sprite = IconFactory.Get(s < stars ? IconKind.Star : IconKind.StarEmpty);
                    img.color = s < stars ? new Color(1f, 0.82f, 0.25f) : new Color(1f, 1f, 1f, 0.35f);
                    img.raycastTarget = false;
                }
            if (newest)
            {
                // The yard to play now: a play badge where the stars go.
                var play = UiFactory.Rect("Play", face, new Vector2(1, 0), new Vector2(1, 0));
                UiFactory.Place(play, new Vector2(1, 0), new Vector2(84, 84), new Vector2(-70, NameHeight / 2f + 6));
                var img = play.gameObject.AddComponent<Image>();
                img.sprite = IconFactory.Get(IconKind.Next);
                img.raycastTarget = false;
            }
            if (!open)
            {
                var veil = UiFactory.Rect("Veil", pic, Vector2.zero, Vector2.one);
                var v = veil.gameObject.AddComponent<Image>();
                v.color = new Color(0f, 0f, 0f, nextToOpen ? 0.5f : 0.86f); // barely visible below; the next one a little more
                v.raycastTarget = false;
                var lockRt = UiFactory.Rect("Lock", pic, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
                UiFactory.Place(lockRt, new Vector2(0.5f, 0.5f), new Vector2(150, 150), Vector2.zero);
                var li = lockRt.gameObject.AddComponent<Image>();
                li.sprite = IconFactory.Get(IconKind.Lock);
                li.color = new Color(1f, 1f, 1f, 0.92f);
                li.raycastTarget = false;
            }
            return height;
        }

        public void Hide() => gameObject.SetActive(false);

        private static readonly Color[] LeagueColors =
        {
            new Color(0.25f, 0.62f, 0.95f), new Color(0.2f, 0.72f, 0.5f), new Color(0.98f, 0.62f, 0.2f), new Color(0.85f, 0.32f, 0.45f), new Color(0.55f, 0.38f, 0.85f),
        };

        // League band: the name, then a star with the stars won of all (or, while closed, a padlock and the stars it needs).
        private void AddLeagueHeader((int number, string name, int stars, int max, int need, bool closed) league, float y)
        {
            var rt = UiFactory.Rect("League " + league.name, content, new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            UiFactory.Place(rt, new Vector2(0.5f, 1), new Vector2(820, 130), new Vector2(0, y - 75));
            var bg = rt.gameObject.AddComponent<Image>();
            bg.sprite = IconFactory.Rounded;
            bg.type = Image.Type.Sliced;
            bg.color = league.closed ? new Color(0.36f, 0.39f, 0.46f) : LeagueColors[Mathf.Max(0, league.number - 1) % LeagueColors.Length];
            bg.raycastTarget = false;
            var nameRt = UiFactory.Rect("Name", rt, new Vector2(0, 0), new Vector2(1, 1), new Vector2(36, 0), new Vector2(-280, 0));
            UiFactory.Label(nameRt, league.name, 60, TextAnchor.MiddleLeft, true).color = league.closed ? new Color(1f, 1f, 1f, 0.6f) : Color.white;
            float x = -36f;
            var countRt = UiFactory.Rect("Stars", rt, new Vector2(1, 0.5f), new Vector2(1, 0.5f));
            UiFactory.Place(countRt, new Vector2(1, 0.5f), new Vector2(170, 90), new Vector2(x - 85f, 0));
            UiFactory.Label(countRt, league.closed ? league.need.ToString() : league.stars + "/" + league.max, 46, TextAnchor.MiddleRight, true);
            x -= 180f;
            Icon(rt, IconKind.Star, new Color(1f, 0.82f, 0.25f), x);
            if (league.closed) Icon(rt, IconKind.Lock, Color.white, x - 70f);
        }

        private static void Icon(RectTransform parent, IconKind kind, Color color, float x)
        {
            var ir = UiFactory.Rect(kind.ToString(), parent, new Vector2(1, 0.5f), new Vector2(1, 0.5f));
            UiFactory.Place(ir, new Vector2(1, 0.5f), new Vector2(64, 64), new Vector2(x - 32f, 0));
            var img = ir.gameObject.AddComponent<Image>();
            img.sprite = IconFactory.Get(kind);
            img.color = color;
            img.raycastTarget = false;
        }
    }
}
