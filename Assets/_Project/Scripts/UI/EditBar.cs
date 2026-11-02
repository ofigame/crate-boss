using System;
using System.Collections.Generic;
using CrateBoss.Localization;
using UnityEngine;
using UnityEngine.UI;

namespace CrateBoss.UI
{
    /// <summary>
    /// Edit mode (Ana Belge §12.4): the inventory strip at the bottom (items chosen but not placed), and the tools: turn, place,
    /// back to the inventory, path tiles, done. Tapping a cell of a land moves the ghost there.
    /// </summary>
    public class EditBar : MonoBehaviour
    {
        public event Action<string> InventoryPicked;
        public event Action RotateClicked, PlaceClicked, StoreClicked, PathClicked, DoneClicked;

        private RectTransform strip;
        private Text hint;
        private Image pathFace;

        public void Build(RectTransform canvas)
        {
            var root = (RectTransform)transform;
            root.SetParent(canvas, false);
            root.anchorMin = Vector2.zero; root.anchorMax = Vector2.one; root.offsetMin = root.offsetMax = Vector2.zero;
            var bar = UiFactory.Panel("Bar", root, new Color(1f, 1f, 1f, 0.9f), true).rectTransform;
            bar.anchorMin = new Vector2(0.02f, 0.01f); bar.anchorMax = new Vector2(0.98f, 0.2f); bar.offsetMin = bar.offsetMax = Vector2.zero;
            strip = UiFactory.Rect("Inventory", bar, new Vector2(0.02f, 0.5f), new Vector2(0.98f, 0.97f));
            hint = UiFactory.Label(UiFactory.Rect("Hint", root, new Vector2(0, 0.86f), new Vector2(1, 0.92f)), "", 38, TextAnchor.MiddleCenter, true);
            hint.color = UiFactory.InkColor;
            var tools = new (IconKind, Action, string)[]
            {
                (IconKind.Restart, () => RotateClicked?.Invoke(), "edit.rotate"),
                (IconKind.Check, () => PlaceClicked?.Invoke(), "edit.place"),
                (IconKind.Undo, () => StoreClicked?.Invoke(), "edit.store"),
                (IconKind.Map, () => PathClicked?.Invoke(), "edit.path"),
                (IconKind.Close, () => DoneClicked?.Invoke(), "edit.done"),
            };
            for (int i = 0; i < tools.Length; i++)
            {
                var (icon, act, key) = tools[i];
                var b = UiFactory.Button(bar, i == 1 ? UiFactory.AccentColor : UiFactory.ButtonColor, act, icon, null, 30, true);
                UiFactory.Place((RectTransform)b.transform, new Vector2(0.1f + i * 0.2f, 0.27f), new Vector2(120, 120));
                if (key == "edit.path") pathFace = UiFactory.FaceOf(b).GetComponent<Image>();
                var l = UiFactory.Label(UiFactory.Rect("Caption", (RectTransform)b.transform, new Vector2(-0.6f, -0.38f), new Vector2(1.6f, 0)), Loc.T(key), 24, TextAnchor.MiddleCenter, true);
                l.color = UiFactory.InkColor;
            }
            gameObject.SetActive(false);
        }

        public void Show(bool on) => gameObject.SetActive(on);

        public void SetHint(string text) => hint.text = text;

        public void SetPathMode(bool on) { if (pathFace != null) pathFace.color = on ? UiFactory.WarmColor : UiFactory.ButtonColor; }

        public void SetInventory(IReadOnlyList<(string id, string name, Color color)> items)
        {
            for (int i = strip.childCount - 1; i >= 0; i--) Destroy(strip.GetChild(i).gameObject);
            for (int i = 0; i < items.Count && i < 8; i++)
            {
                var (id, name, color) = items[i];
                var b = UiFactory.Button(strip, color, () => InventoryPicked?.Invoke(id), null, name, 22);
                var rt = (RectTransform)b.transform;
                rt.anchorMin = new Vector2(i / 8f + 0.005f, 0); rt.anchorMax = new Vector2((i + 1) / 8f - 0.005f, 1); rt.offsetMin = rt.offsetMax = Vector2.zero;
            }
        }
    }
}
