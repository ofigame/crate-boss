using CrateBoss.UI;
using UnityEngine;
using UnityEngine.UI;

namespace CrateBoss.View
{
    /// <summary>
    /// Owner 2026-11-02: a yellow arrow bobs up and down over each cargo crate (the ones the truck takes), always facing the
    /// camera; on candy levels only once the candy part is over. It rides along with the crate.
    /// </summary>
    public class CargoArrow : MonoBehaviour
    {
        private const float Scale = 0.0095f, Low = 1.6f, Swing = 0.28f;

        private RectTransform arrow;
        private float phase;

        public static CargoArrow Add(Transform crate)
        {
            var self = crate.gameObject.AddComponent<CargoArrow>();
            var go = new GameObject("CargoArrow", typeof(RectTransform), typeof(Canvas));
            go.transform.SetParent(crate, false);
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 6;
            self.arrow = (RectTransform)go.transform;
            self.arrow.sizeDelta = new Vector2(100f, 100f);
            self.arrow.localScale = Vector3.one * Scale;
            self.arrow.localPosition = Vector3.up * Low;
            var image = UiFactory.Panel("Arrow", self.arrow, Color.white);
            image.sprite = IconFactory.DownArrow;
            image.raycastTarget = false;
            self.phase = Random.value * 6f;
            go.SetActive(false);
            return self;
        }

        public void Show(bool on)
        {
            if (arrow != null) arrow.gameObject.SetActive(on);
        }

        private void LateUpdate()
        {
            if (arrow == null || !arrow.gameObject.activeSelf) return;
            arrow.localPosition = Vector3.up * (Low + Swing * (0.5f + 0.5f * Mathf.Sin(Time.time * 4f + phase)));
            var cam = Camera.main;
            if (cam != null) arrow.rotation = cam.transform.rotation;
        }
    }
}
