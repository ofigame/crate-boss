using CrateBoss.UI;
using UnityEngine;
using UnityEngine.UI;

namespace CrateBoss.View
{
    /// <summary>
    /// Owner 2026-11-02: a tap on the worker locks him (short drags then slide the view instead of moving him); a golden lock
    /// bobs up and down over his head while he is locked, always facing the camera.
    /// </summary>
    public class WorkerLock : MonoBehaviour
    {
        private const float Scale = 0.0085f, Low = 2.25f, Swing = 0.22f;

        private Transform follow;
        private RectTransform icon;

        public static WorkerLock Create(Transform parent, Transform worker)
        {
            var go = new GameObject("WorkerLock", typeof(RectTransform), typeof(Canvas));
            go.transform.SetParent(parent, false);
            var self = go.AddComponent<WorkerLock>();
            self.follow = worker;
            go.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            go.GetComponent<Canvas>().sortingOrder = 7;
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(100f, 100f);
            rt.localScale = Vector3.one * Scale;
            var ring = UiFactory.Panel("Ring", rt, new Color(1f, 1f, 1f, 0.9f));
            ring.sprite = IconFactory.Circle;
            ring.raycastTarget = false;
            var image = UiFactory.Panel("Lock", rt, new Color(0.95f, 0.66f, 0.12f));
            image.sprite = IconFactory.Get(IconKind.Lock);
            image.raycastTarget = false;
            image.rectTransform.offsetMin = new Vector2(14, 14);
            image.rectTransform.offsetMax = new Vector2(-14, -14);
            self.icon = rt;
            go.SetActive(false);
            return self;
        }

        public void Show(bool on) => gameObject.SetActive(on);

        private void LateUpdate()
        {
            if (follow == null) return;
            transform.position = follow.position + Vector3.up * (Low + Swing * (0.5f + 0.5f * Mathf.Sin(Time.time * 4f)));
            var cam = Camera.main;
            if (cam != null) icon.rotation = cam.transform.rotation;
        }
    }
}
