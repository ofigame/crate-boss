using CrateBoss.UI;
using UnityEngine;
using UnityEngine.UI;

namespace CrateBoss.View
{
    /// <summary>
    /// The number over a breakable crate (owner 2026-11-01 candy levels): how many hammer hits it still needs, in a small
    /// frosted cloud, always turned to the camera. Small and low over the lid, so a yard full of them stays readable.
    /// </summary>
    public class HitBadge : MonoBehaviour
    {
        private const float Scale = 0.0042f;

        private Text label;

        public static HitBadge Create(Transform parent, Vector3 world, int hits)
        {
            var go = new GameObject("HitBadge", typeof(RectTransform), typeof(Canvas));
            go.transform.SetParent(parent, false);
            go.transform.position = world;
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 5;
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(150f, 120f);
            rt.localScale = Vector3.one * Scale;
            var cloud = UiFactory.Panel("Cloud", rt, Color.white);
            cloud.sprite = IconFactory.FrostCloud;
            cloud.raycastTarget = false;
            var text = UiFactory.Rect("Count", rt, new Vector2(0.1f, 0.12f), new Vector2(0.9f, 0.82f));
            var badge = go.AddComponent<HitBadge>();
            badge.label = UiFactory.Label(text, hits.ToString(), 70, TextAnchor.MiddleCenter, true);
            badge.label.color = new Color(0.1f, 0.24f, 0.46f);
            var rim = badge.label.gameObject.AddComponent<Outline>();
            rim.effectColor = new Color(1f, 1f, 1f, 0.9f);
            rim.effectDistance = new Vector2(3f, -3f);
            return badge;
        }

        public void Set(int hits)
        {
            label.text = hits.ToString();
            transform.localScale = Vector3.one * Scale * 1.35f; // a little pop; LateUpdate eases it back
        }

        private void LateUpdate()
        {
            var cam = Camera.main;
            if (cam != null) transform.rotation = cam.transform.rotation;
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * Scale, 1f - Mathf.Exp(-10f * Time.deltaTime));
        }
    }
}
