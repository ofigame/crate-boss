using CrateBoss.Localization;
using UnityEngine;
using UnityEngine.UI;

namespace CrateBoss.Ads
{
    /// <summary>The test banner: a purple strip "TEST ŞERİT REKLAM" along the bottom edge, over everything but the ads.</summary>
    public sealed class TestBannerAdService : IBannerAdService
    {
        private GameObject strip;

        public float Height => 150f;

        public void Show()
        {
            if (strip == null) Build();
            strip.SetActive(true);
        }

        public void Hide()
        {
            if (strip != null) strip.SetActive(false);
        }

        private void Build()
        {
            strip = new GameObject("TestBanner", typeof(Canvas), typeof(CanvasScaler));
            Object.DontDestroyOnLoad(strip);
            var canvas = strip.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 900;
            var scaler = strip.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            var back = new GameObject("Back", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
            back.SetParent(strip.transform, false);
            back.anchorMin = new Vector2(0, 0);
            back.anchorMax = new Vector2(1, 0);
            back.pivot = new Vector2(0.5f, 0);
            back.sizeDelta = new Vector2(0, Height);
            back.anchoredPosition = Vector2.zero;
            back.GetComponent<Image>().color = new Color(0.36f, 0.24f, 0.62f, 1f);
            var label = new GameObject("Label", typeof(RectTransform), typeof(Text)).GetComponent<Text>();
            label.rectTransform.SetParent(back, false);
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = label.rectTransform.offsetMax = Vector2.zero;
            var font = Resources.Load<Font>("Fonts/Nunito-ExtraBold");
            label.font = font != null ? font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 44;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            label.raycastTarget = false;
            label.text = Loc.T("ad.test.banner");
        }
    }
}
