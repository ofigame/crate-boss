using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace CrateBoss.UI
{
    /// <summary>Builds the uGUI pieces from code: rounded raised buttons, icons, cards, labels.</summary>
    public static class UiFactory
    {
        public static readonly Color ButtonColor = new Color(0.22f, 0.56f, 0.96f); // candy blue (owner 2026-11-01)
        public static readonly Color AccentColor = new Color(0.20f, 0.72f, 0.45f);
        public static readonly Color WarmColor = new Color(1.00f, 0.62f, 0.25f);
        public static readonly Color DisabledColor = new Color(0.55f, 0.57f, 0.62f);
        public static readonly Color PanelColor = new Color(0.12f, 0.14f, 0.20f, 0.94f);
        public static readonly Color InkColor = new Color(0.14f, 0.16f, 0.22f);

        /// <summary>Canvas plus a safe-area child; build everything under the returned transform.</summary>
        public static RectTransform CreateCanvas(string name)
        {
            var canvas = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)).GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            // Narrow phones (9:21) follow width, tablets (3:4) follow height.
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            if (UnityEngine.Object.FindAnyObjectByType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            var safe = Stretch("SafeArea", canvas.transform);
            safe.gameObject.AddComponent<SafeAreaFitter>();
            return safe;
        }

        public static RectTransform Rect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin = default, Vector2 offsetMax = default)
        {
            var rt = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
            return rt;
        }

        public static RectTransform Stretch(string name, Transform parent) => Rect(name, parent, Vector2.zero, Vector2.one);

        public static Image Panel(string name, Transform parent, Color color, bool rounded = false)
        {
            var image = Stretch(name, parent).gameObject.AddComponent<Image>();
            image.color = color;
            if (rounded) { image.sprite = IconFactory.Rounded; image.type = Image.Type.Sliced; }
            return image;
        }

        public static Image Icon(Transform parent, IconKind kind, float size, Color color)
        {
            var rt = Rect("Icon", parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            rt.sizeDelta = new Vector2(size, size);
            var image = rt.gameObject.AddComponent<Image>();
            image.sprite = IconFactory.Get(kind);
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        public static Text Label(Transform parent, string text, int size, TextAnchor anchor = TextAnchor.MiddleCenter, bool title = false)
        {
            var label = Stretch("Label", parent).gameObject.AddComponent<GameText>();
            label.font = title ? UiFonts.Title : UiFonts.Body;
            label.fontSize = size;
            label.alignment = anchor; // before the text: GameText right-aligns Arabic
            label.text = text;
            label.color = Color.white;
            label.raycastTarget = false;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            return label;
        }

        /// <summary>
        /// Raised rounded button: a darker shadow below, the coloured face on top. Holds an icon and/or text.
        /// Returns the Button; its face transform is the first child.
        /// </summary>
        public static Button Button(Transform parent, Color color, Action onClick, IconKind? icon = null, string text = null,
            int fontSize = 46, bool round = false)
        {
            var rt = new GameObject($"Button {text ?? icon?.ToString()}", typeof(RectTransform), typeof(Image), typeof(Button)).GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            var hit = rt.GetComponent<Image>();
            hit.color = new Color(0, 0, 0, 0); // invisible hit area covering face + shadow

            // Candy look (owner 2026-11-01, "butonlar … bu tarzda olmalı"): a deep-coloured base under the button, a white rim,
            // the face shaded light at the top and deeper below, a glossy band over its upper half, outlined white text.
            var sprite = round ? IconFactory.Circle : IconFactory.Rounded;
            var type = round ? Image.Type.Simple : Image.Type.Sliced;
            var dark = new Color(color.r * 0.45f, color.g * 0.45f, color.b * 0.5f, 1f);
            var shadow = Stretch("Shadow", rt).gameObject.AddComponent<Image>();
            shadow.sprite = sprite;
            shadow.type = type;
            shadow.color = dark;
            shadow.raycastTarget = false;

            var rim = Stretch("Rim", rt);
            rim.offsetMin = new Vector2(0, 10);
            var rimImage = rim.gameObject.AddComponent<Image>();
            rimImage.sprite = sprite;
            rimImage.type = type;
            rimImage.color = Color.white;
            rimImage.raycastTarget = false;

            const float edge = 7f;
            var face = Stretch("Face", rt);
            face.offsetMin = new Vector2(edge, 10 + edge);
            face.offsetMax = new Vector2(-edge, -edge);
            var faceImage = face.gameObject.AddComponent<Image>();
            faceImage.sprite = sprite;
            faceImage.type = type;
            faceImage.color = color;
            faceImage.raycastTarget = false;
            face.gameObject.AddComponent<UiGradient>();
            face.SetAsLastSibling();

            var gloss = Rect("Gloss", face, new Vector2(0.07f, 0.5f), new Vector2(0.93f, 0.95f));
            var glossImage = gloss.gameObject.AddComponent<Image>();
            glossImage.sprite = sprite;
            glossImage.type = type;
            glossImage.pixelsPerUnitMultiplier = 1.6f; // smaller corners on the thin band
            glossImage.color = new Color(1f, 1f, 1f, 0.22f);
            glossImage.raycastTarget = false;

            if (icon.HasValue && text == null) Lift(Icon(face, icon.Value, 92, Color.white));
            else if (icon.HasValue)
            {
                var ic = Lift(Icon(face, icon.Value, 70, Color.white));
                ((RectTransform)ic.transform).anchoredPosition = new Vector2(-150, 0);
                Ink(Label(face, text, fontSize, TextAnchor.MiddleCenter, true), dark).rectTransform.offsetMin = new Vector2(60, 0);
            }
            else if (text != null) Ink(Label(face, text, fontSize, TextAnchor.MiddleCenter, true), dark);

            var button = rt.GetComponent<Button>();
            button.targetGraphic = faceImage;
            var colors = button.colors;
            colors.pressedColor = new Color(0.9f, 0.9f, 0.9f);
            colors.disabledColor = new Color(0.7f, 0.7f, 0.7f, 0.6f);
            button.colors = colors;
            button.onClick.AddListener(() => onClick());
            var press = rt.gameObject.AddComponent<PressableButton>();
            press.face = face;
            return button;
        }

        /// <summary>White button text with a dark rim and a drop, so it reads on any candy colour.</summary>
        public static Text Ink(Text label, Color rim)
        {
            var outline = label.gameObject.AddComponent<Outline>();
            outline.effectColor = rim;
            outline.effectDistance = new Vector2(3f, -3f);
            var drop = label.gameObject.AddComponent<Shadow>();
            drop.effectColor = new Color(0f, 0f, 0f, 0.3f);
            drop.effectDistance = new Vector2(0f, -5f);
            return label;
        }

        private static Image Lift(Image icon)
        {
            var drop = icon.gameObject.AddComponent<Shadow>();
            drop.effectColor = new Color(0f, 0f, 0f, 0.28f);
            drop.effectDistance = new Vector2(0f, -5f);
            return icon;
        }

        /// <summary>Dark glassy pill (top counters, captions under round buttons) in the candy style.</summary>
        public static readonly Color PillColor = new Color(0.08f, 0.2f, 0.32f, 0.82f);

        public static void Place(RectTransform rt, Vector2 anchor, Vector2 size, Vector2 offset = default)
        {
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = offset;
        }

        public static RectTransform FaceOf(Button b) => (RectTransform)b.transform.Find("Face");
    }
}
