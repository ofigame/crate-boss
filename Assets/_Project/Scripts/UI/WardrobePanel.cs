using System;
using System.Collections.Generic;
using CrateBoss.Localization;
using UnityEngine;
using UnityEngine.UI;

namespace CrateBoss.UI
{
    /// <summary>The wardrobe (Ana Belge §13.2): hard-hat, vest and glove colours for the worker, bought with CB Coin (100–500) and worn.</summary>
    public class WardrobePanel : MonoBehaviour
    {
        public event Action<string> BuyOrWear;

        private RectTransform list;
        private Text balance;

        public bool IsOpen => gameObject.activeSelf;

        public void Build(RectTransform canvas)
        {
            var root = (RectTransform)transform;
            root.SetParent(canvas, false);
            root.anchorMin = Vector2.zero; root.anchorMax = Vector2.one;
            root.offsetMin = new Vector2(-200, -400); root.offsetMax = new Vector2(200, 400);
            gameObject.AddComponent<Image>().color = new Color(0.05f, 0.06f, 0.1f, 0.6f);
            var card = UiFactory.Panel("Card", root, UiFactory.PanelColor, true).rectTransform;
            UiFactory.Place(card, new Vector2(0.5f, 0.5f), new Vector2(940, 1400));
            UiFactory.Label(UiFactory.Rect("Title", card, new Vector2(0, 0.9f), new Vector2(1, 0.98f)), Loc.T("home.wardrobe"), 64, TextAnchor.MiddleCenter, true);
            balance = UiFactory.Label(UiFactory.Rect("Balance", card, new Vector2(0, 0.85f), new Vector2(1, 0.9f)), "", 38, TextAnchor.MiddleCenter, true);
            balance.color = new Color(1f, 0.86f, 0.4f);
            list = UiFactory.Rect("List", card, new Vector2(0.04f, 0.04f), new Vector2(0.96f, 0.84f));
            var close = UiFactory.Button(card, UiFactory.ButtonColor, Hide, IconKind.Close, null, 40, true);
            UiFactory.Place((RectTransform)close.transform, new Vector2(1, 1), new Vector2(110, 110), new Vector2(-40, -40));
            gameObject.SetActive(false);
        }

        /// <param name="items">id, name, colour, price, state: 0 buy, 1 owned, 2 worn.</param>
        public void Show(int coins, IReadOnlyList<(string id, string name, Color color, int price, int state)> items)
        {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            balance.text = Loc.F("coin.balance", coins);
            for (int i = list.childCount - 1; i >= 0; i--) Destroy(list.GetChild(i).gameObject);
            int cols = 3, rows = Mathf.CeilToInt(items.Count / (float)cols);
            for (int i = 0; i < items.Count; i++)
            {
                var it = items[i];
                int cx = i % cols, cy = i / cols;
                var cell = UiFactory.Panel("Item", list, new Color(1f, 1f, 1f, 0.95f), true).rectTransform;
                cell.anchorMin = new Vector2(cx / (float)cols + 0.01f, 1f - (cy + 1) / (float)rows + 0.01f);
                cell.anchorMax = new Vector2((cx + 1) / (float)cols - 0.01f, 1f - cy / (float)rows - 0.01f);
                cell.offsetMin = cell.offsetMax = Vector2.zero;
                var sw = UiFactory.Panel("Swatch", cell, it.color, true).rectTransform;
                sw.anchorMin = new Vector2(0.2f, 0.5f); sw.anchorMax = new Vector2(0.8f, 0.92f); sw.offsetMin = sw.offsetMax = Vector2.zero;
                var n = UiFactory.Label(UiFactory.Rect("Name", cell, new Vector2(0, 0.3f), new Vector2(1, 0.5f)), it.name, 28, TextAnchor.MiddleCenter, true);
                n.color = UiFactory.InkColor;
                string label = it.state == 2 ? Loc.T("wardrobe.worn") : it.state == 1 ? Loc.T("wardrobe.wear") : it.price.ToString();
                string id = it.id;
                var b = UiFactory.Button(cell, it.state == 0 ? UiFactory.WarmColor : UiFactory.AccentColor, () => BuyOrWear?.Invoke(id), it.state == 0 ? IconKind.Coin : (IconKind?)null, label, 28);
                UiFactory.Place((RectTransform)b.transform, new Vector2(0.5f, 0.14f), new Vector2(240, 90));
                b.interactable = it.state != 2 && (it.state == 1 || coins >= it.price);
            }
        }

        public void Hide() => gameObject.SetActive(false);
    }
}
