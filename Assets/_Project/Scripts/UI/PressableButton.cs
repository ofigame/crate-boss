using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CrateBoss.UI
{
    /// <summary>Raised button look: the face sits above its shadow and sinks into it while pressed.</summary>
    public class PressableButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        /// <summary>Any button press (for the click sound).</summary>
        public static event Action Pressed;

        public RectTransform face;
        public float depth = 10f;

        private bool down;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (face == null || down) return;
            down = true;
            face.anchoredPosition = new Vector2(face.anchoredPosition.x, face.anchoredPosition.y - depth);
            Pressed?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData) => Release();
        public void OnPointerExit(PointerEventData eventData) => Release();

        private void OnDisable() => Release();

        private void Release()
        {
            if (!down || face == null) return;
            down = false;
            face.anchoredPosition = new Vector2(face.anchoredPosition.x, face.anchoredPosition.y + depth);
        }
    }
}
