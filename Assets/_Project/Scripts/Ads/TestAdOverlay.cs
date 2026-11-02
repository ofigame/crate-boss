using System;
using System.Collections;
using CrateBoss.Localization;
using UnityEngine;
using UnityEngine.UI;

namespace CrateBoss.Ads
{
    /// <summary>
    /// The full-screen test ad (Ana Belge §15.4): a dark panel, the title, a countdown, then "Kapat". The close button of a
    /// rewarded ad is there from the start; closing before the countdown ends earns nothing.
    /// </summary>
    public class TestAdOverlay : MonoBehaviour
    {
        private Action<bool> done;
        private Text count, closeText;
        private Button close;
        private float seconds;
        private bool rewarded, finished, autoClose;

        public static void Open(string title, float seconds, bool rewarded, bool autoClose, Action<bool> done)
        {
            var go = new GameObject("TestAd", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            go.AddComponent<TestAdOverlay>().Build(title, seconds, rewarded, autoClose, done);
        }

        private void Build(string title, float s, bool isRewarded, bool auto, Action<bool> onDone)
        {
            seconds = s;
            rewarded = isRewarded;
            autoClose = auto;
            done = onDone;
            var bg = Rect("Back", transform, Vector2.zero, Vector2.one).gameObject.AddComponent<Image>();
            bg.color = new Color(0.04f, 0.05f, 0.08f, 1f);
            var stripe = Rect("Stripe", transform, new Vector2(0, 0.42f), new Vector2(1, 0.62f)).gameObject.AddComponent<Image>();
            stripe.color = new Color(0.52f, 0.33f, 0.86f, 0.35f);
            stripe.raycastTarget = false;
            Label(Rect("Title", transform, new Vector2(0, 0.52f), new Vector2(1, 0.62f)), title, 84);
            count = Label(Rect("Count", transform, new Vector2(0, 0.42f), new Vector2(1, 0.52f)), "", 54);
            count.color = new Color(1f, 1f, 1f, 0.7f);

            var closeRt = Rect("Close", transform, new Vector2(1, 1), new Vector2(1, 1));
            closeRt.sizeDelta = new Vector2(rewarded ? 340 : 260, 110);
            closeRt.anchoredPosition = new Vector2(-210, -120);
            var img = closeRt.gameObject.AddComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0.15f);
            close = closeRt.gameObject.AddComponent<Button>();
            close.targetGraphic = img;
            close.onClick.AddListener(Close);
            closeText = Label(closeRt, "", 40);
            close.gameObject.SetActive(rewarded);
            StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            for (float t = 0; t < seconds; t += Time.unscaledDeltaTime)
            {
                int left = Mathf.CeilToInt(seconds - t);
                count.text = rewarded ? Loc.Display(Loc.F("ad.countdown", left)) : left.ToString();
                if (rewarded) closeText.text = Loc.Display(Loc.T("ad.close.noreward"));
                yield return null;
            }
            finished = true;
            count.text = rewarded ? Loc.Display(Loc.T("ad.rewarded")) : "";
            closeText.text = Loc.Display(Loc.T("ui.close"));
            close.gameObject.SetActive(true);
            if (!autoClose) yield break;
            yield return new WaitForSecondsRealtime(0.4f);
            Close();
        }

        private void Close()
        {
            var callback = done;
            done = null;
            Destroy(gameObject);
            callback?.Invoke(finished);
        }

        private static RectTransform Rect(string name, Transform parent, Vector2 min, Vector2 max)
        {
            var rt = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            return rt;
        }

        private static Text Label(RectTransform parent, string text, int size)
        {
            var label = Rect("Label", parent, Vector2.zero, Vector2.one).gameObject.AddComponent<Text>();
            var font = Resources.Load<Font>("Fonts/Nunito-ExtraBold");
            label.font = font != null ? font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.text = Loc.Display(text);
            label.fontSize = size;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            label.raycastTarget = false;
            return label;
        }
    }
}
