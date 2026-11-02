using UnityEngine;
using UnityEngine.UI;

namespace CrateBoss.UI
{
    /// <summary>The pieces of one chip or badge: "A 3" while counting, "A" + pause sign while held up.</summary>
    public sealed class MachineTag
    {
        public RectTransform Root { get; }
        private readonly Image background;
        private readonly Text text;
        private readonly Image pause;
        private readonly Image icon;
        private IconKind shownIcon = IconKind.Crane;
        private readonly float pauseX; // pause sign position after a one-letter id

        /// <param name="icon">The machine icon on a chip; null on floor badges.</param>
        public MachineTag(RectTransform root, Image background, Text text, Image pause, Image icon = null)
        {
            this.icon = icon;
            Root = root;
            this.background = background;
            this.text = text;
            this.pause = pause;
            pauseX = ((RectTransform)pause.transform).anchoredPosition.x;
        }

        public void Show(MachineTagInfo info, Color normal, Color urgent)
        {
            string value = info.Waiting || info.Counter < 0 ? info.Letter : $"{info.Letter} {info.Counter}";
            if (text.text != value) text.text = value;
            pause.gameObject.SetActive(info.Waiting);
            if (info.Waiting)
            {
                // Longer ids ("F1") push the pause sign right so they never overlap.
                var rt = (RectTransform)pause.transform;
                rt.anchoredPosition = new Vector2(pauseX + (info.Letter.Length - 1) * text.fontSize * 0.62f, rt.anchoredPosition.y);
            }
            background.color = !info.Waiting && info.Counter >= 0 && info.Counter <= 1 ? urgent : normal;
            if (icon != null && shownIcon != info.Icon)
            {
                shownIcon = info.Icon;
                icon.sprite = IconFactory.Get(info.Icon);
            }
        }
    }
}
