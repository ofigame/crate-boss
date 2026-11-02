using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CrateBoss.UI
{
    /// <summary>
    /// Tutorial: a pointing hand that swipes the way to play from a point on the board, or taps a point, with a pulsing ring
    /// round what is new, and an info bubble that says the rule in one line (owner 2026-11-01: "öğreticide bilgi bulutları da
    /// olsun"). The points follow the camera every frame. A tap anywhere goes to the next bubble or closes the last one (owner
    /// 2026-11-01, instead of a close button). Nothing here blocks the swipes.
    /// </summary>
    public class TutorialView : MonoBehaviour
    {
        private RectTransform root, hand, ring, shadow;
        private Image handImage, shadowImage, ringImage;
        private RectTransform bubble, tail;
        private Text bubbleText;
        private Func<Vector3> handAt, ringAt;
        private Func<Vector2> swipeDir;
        private Coroutine loop;

        /// <summary>A tap on the screen (not a swipe, not on a button) while it shows.</summary>
        public event Action Tapped;

        public bool IsShowing => gameObject.activeSelf;

        public void Build(RectTransform canvas)
        {
            root = (RectTransform)transform;
            root.SetParent(canvas, false);
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = root.offsetMax = Vector2.zero;

            ring = UiFactory.Rect("Ring", root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            ring.sizeDelta = new Vector2(220, 220);
            ringImage = ring.gameObject.AddComponent<Image>();
            ringImage.sprite = IconFactory.Circle;
            ringImage.raycastTarget = false;

            shadow = UiFactory.Rect("HandShadow", root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            shadow.sizeDelta = new Vector2(196, 196);
            shadow.pivot = new Vector2(0.42f, 0.9f); // the fingertip is the point
            shadowImage = shadow.gameObject.AddComponent<Image>();
            shadowImage.sprite = IconFactory.Get(IconKind.Hand);
            shadowImage.color = new Color(0.12f, 0.14f, 0.2f, 0.85f);
            shadowImage.raycastTarget = false;

            hand = UiFactory.Rect("Hand", root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            hand.sizeDelta = new Vector2(180, 180);
            hand.pivot = new Vector2(0.42f, 0.9f);
            handImage = hand.gameObject.AddComponent<Image>();
            handImage.sprite = IconFactory.Get(IconKind.Hand);
            handImage.raycastTarget = false;

            // Info bubble: a white rounded card with a little tail, above the marked point (kept on screen).
            bubble = UiFactory.Panel("Bubble", root, Color.white, true).rectTransform;
            bubble.anchorMin = bubble.anchorMax = new Vector2(0.5f, 0.5f);
            bubble.sizeDelta = new Vector2(760, 170);
            bubble.GetComponent<Image>().raycastTarget = false;
            tail = UiFactory.Rect("Tail", bubble, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            tail.sizeDelta = new Vector2(46, 46);
            tail.anchoredPosition = new Vector2(0, -12);
            tail.localRotation = Quaternion.Euler(0, 0, 45);
            var tailImage = tail.gameObject.AddComponent<Image>();
            tailImage.color = Color.white;
            tailImage.raycastTarget = false;
            tail.SetAsFirstSibling();
            var textRt = UiFactory.Rect("Text", bubble, Vector2.zero, Vector2.one, new Vector2(34, 18), new Vector2(-34, -18));
            bubbleText = UiFactory.Label(textRt, "", 42, TextAnchor.MiddleCenter, true);
            bubbleText.horizontalOverflow = HorizontalWrapMode.Wrap; // owner 2026-11-01: the text ran out of the bubble
            bubbleText.color = UiFactory.InkColor;
            bubbleText.raycastTarget = false;
            bubble.gameObject.SetActive(false);

            gameObject.SetActive(false);
        }

        /// <summary>The hand swipes from a board point along a screen direction, again and again; the ring marks <paramref name="mark"/>.</summary>
        public void ShowSwipe(Func<Vector3> from, Func<Vector2> direction, Func<Vector3> mark, string text = null)
        {
            Begin(from, mark);
            SetText(text);
            swipeDir = direction;
            loop = StartCoroutine(SwipeLoop());
        }

        /// <summary>The hand taps a board point again and again; the ring pulses round it.</summary>
        public void ShowTap(Func<Vector3> at, string text = null)
        {
            Begin(at, at);
            SetText(text);
            swipeDir = null;
            loop = StartCoroutine(TapLoop());
        }

        /// <summary>The bubble's line (null or empty hides it); may change while the tutorial runs.</summary>
        public void SetText(string text)
        {
            bool on = !string.IsNullOrEmpty(text);
            bubble.gameObject.SetActive(on);
            if (!on) return;
            bubbleText.text = text;
            // The bubble grows with the text: wrapped lines at the bubble's width, at least the one-line height.
            float width = bubble.sizeDelta.x - 68f;
            var settings = bubbleText.GetGenerationSettings(new Vector2(width, 0f));
            float lines = bubbleText.cachedTextGeneratorForLayout.GetPreferredHeight(bubbleText.text, settings) / bubbleText.pixelsPerUnit;
            bubble.sizeDelta = new Vector2(bubble.sizeDelta.x, Mathf.Max(150f, lines + 56f));
        }

        private Vector2 pressAt;
        private float pressTime;
        private bool pressOnUi;

        // A short touch that hardly moved, off the buttons, is a tap; swipes and held fingers are not.
        private void Update()
        {
            var pointer = UnityEngine.InputSystem.Pointer.current;
            if (pointer == null) return;
            var pos = pointer.position.ReadValue();
            if (pointer.press.wasPressedThisFrame)
            {
                pressAt = pos;
                pressTime = Time.unscaledTime;
                var es = UnityEngine.EventSystems.EventSystem.current;
                pressOnUi = es != null && es.IsPointerOverGameObject();
            }
            if (pointer.press.wasReleasedThisFrame && !pressOnUi && (pos - pressAt).magnitude < 24f && Time.unscaledTime - pressTime < 0.35f)
                Tapped?.Invoke();
        }

        public void Hide()
        {
            if (loop != null) StopCoroutine(loop);
            loop = null;
            gameObject.SetActive(false);
        }

        private void Begin(Func<Vector3> handPoint, Func<Vector3> ringPoint)
        {
            Hide();
            handAt = handPoint;
            ringAt = ringPoint;
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
        }

        private Vector2 Local(Vector3 world)
        {
            var cam = Camera.main;
            if (cam == null) return Vector2.zero;
            var screen = cam.WorldToScreenPoint(world);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(root, screen, null, out var local);
            return local + root.rect.size * (root.pivot - new Vector2(0.5f, 0.5f));
        }

        private void LateUpdate()
        {
            if (ringAt == null) return;
            ring.anchoredPosition = Local(ringAt());
            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * 5f);
            ring.localScale = Vector3.one * (0.9f + 0.25f * pulse);
            ringImage.color = new Color(1f, 0.85f, 0.25f, 0.18f + 0.22f * (1f - pulse));
            if (bubble.gameObject.activeSelf)
            {
                // Above the point (below it when the point is high on the screen), inside the screen.
                var p = ring.anchoredPosition;
                var half = root.rect.size / 2f;
                float lift = 175f + bubble.sizeDelta.y / 2f; // clear of the ring and the hand
                bool below = p.y + lift + bubble.sizeDelta.y / 2f > half.y - 180f;
                float x = Mathf.Clamp(p.x, -half.x + bubble.sizeDelta.x / 2f + 20f, half.x - bubble.sizeDelta.x / 2f - 20f);
                bubble.anchoredPosition = new Vector2(x, p.y + (below ? -lift : lift));
                tail.anchorMin = tail.anchorMax = new Vector2(Mathf.Clamp01(0.5f + (p.x - x) / bubble.sizeDelta.x), below ? 1f : 0f);
                tail.anchoredPosition = new Vector2(0, below ? 12 : -12);
            }
        }

        private void PlaceHand(Vector2 p, float scale, float alpha)
        {
            hand.anchoredPosition = p;
            shadow.anchoredPosition = p + new Vector2(6f, -8f);
            hand.localScale = shadow.localScale = Vector3.one * scale;
            handImage.color = new Color(1f, 1f, 1f, alpha);
            shadowImage.color = new Color(0.12f, 0.14f, 0.2f, 0.85f * alpha);
        }

        private IEnumerator SwipeLoop()
        {
            while (true)
            {
                for (float t = 0; t < 1.5f; t += Time.deltaTime)
                {
                    var start = Local(handAt());
                    var dir = swipeDir().normalized;
                    float move = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((t - 0.25f) / 0.6f));
                    float alpha = t < 0.2f ? t / 0.2f : t > 1.15f ? Mathf.Clamp01((1.5f - t) / 0.35f) : 1f;
                    float press = t < 0.25f ? Mathf.Lerp(1.1f, 0.92f, t / 0.25f) : 0.92f;
                    PlaceHand(start + dir * 190f * move, press, alpha);
                    yield return null;
                }
                yield return new WaitForSeconds(0.25f);
            }
        }

        private IEnumerator TapLoop()
        {
            while (true)
            {
                for (float t = 0; t < 1.1f; t += Time.deltaTime)
                {
                    float press = t < 0.35f ? Mathf.Lerp(1.15f, 0.85f, t / 0.35f) : t < 0.55f ? Mathf.Lerp(0.85f, 1.15f, (t - 0.35f) / 0.2f) : 1.15f;
                    PlaceHand(Local(handAt()) + new Vector2(10f, -10f), press, 1f);
                    yield return null;
                }
                yield return new WaitForSeconds(0.3f);
            }
        }
    }
}
