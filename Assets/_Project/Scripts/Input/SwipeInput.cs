using System;
using System.Collections.Generic;
using CrateBoss.Core;
using CrateBoss.View;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace CrateBoss.Controls
{
    /// <summary>
    /// Swipe (touch or mouse drag) plus keyboard: arrows/WASD move, Z/U/Backspace undo, R restart.
    /// A quick flick fires a move as soon as it passes the threshold, along the grid edge closest to it on screen; a finger
    /// held down and dragged slides the view round a big yard and the city (owner 2026-10-31).
    /// </summary>
    public class SwipeInput : MonoBehaviour
    {
        /// <summary>Owner 2026-11-01 "dokunup ittiğim anda algılamalı": about 3 mm of finger travel is a move (pixels from the screen's dpi).</summary>
        public float minSwipeMillimetres = 3f;
        private float MinSwipePixels => Mathf.Max(18f, (Screen.dpi > 0 ? Screen.dpi : 160f) * minSwipeMillimetres / 25.4f);

        public event Action<Direction> DirectionPressed;
        /// <summary>A flick on the screen: where it started (screen pixels) and its grid direction. A flick that starts on a parked car sends that car (Ana Belge §5.5).</summary>
        public event Action<Vector2, Direction> Swiped;
        /// <summary>A finger (or the mouse) went down off the buttons: where, in screen pixels.</summary>
        public event Action<Vector2> Pressed;
        /// <summary>A short touch that hardly moved (not a flick, not held): where, in screen pixels.</summary>
        public event Action<Vector2> Tapped;
        public event Action UndoPressed;
        public event Action RestartPressed;

        /// <summary>A touch held this long without a flick slides the view instead of moving.</summary>
        public float holdSeconds = 0.3f;

        /// <summary>
        /// Owner 2026-11-02: the worker is locked (a tap on him): a touch that does not start on something to act on (see
        /// <see cref="ActsAt"/>) slides the view at once, no holding needed.
        /// </summary>
        public bool PanFirst { get; set; }
        /// <summary>True when a touch at this screen point starts an action (a candy crate, a breakable crate, the worker).</summary>
        public Func<Vector2, bool> ActsAt { get; set; }

        private bool tracking, panning;
        private bool lockedPan; // a locked worker's drag: released without moving it is a tap after all
        private Vector2 pressPos;
        private float pressedAt;
        private Vector2 start;
        private readonly List<RaycastResult> uiHits = new List<RaycastResult>();

        private void Update()
        {
            ReadKeyboard();
            if (ReadPinch()) return;
            ReadPointer();
        }

        /// <summary>
        /// Owner 2026-11-02: two fingers spread apart (true) or pinched together (false), one step per gesture: the view
        /// comes close to the worker or goes back to the play view. The mouse wheel does the same on a computer.
        /// </summary>
        public event Action<bool> Pinched;
        private float pinchStart;
        private bool pinchDone;

        // True while two fingers are down (no swipe or drag then).
        private bool ReadPinch()
        {
            var ts = Touchscreen.current;
            if (ts != null)
            {
                int down = 0;
                Vector2 a = default, b = default;
                foreach (var t in ts.touches)
                {
                    if (!t.press.isPressed) continue;
                    if (down == 0) a = t.position.ReadValue(); else if (down == 1) b = t.position.ReadValue();
                    down++;
                }
                if (down >= 2)
                {
                    float d = (a - b).magnitude;
                    if (pinchStart <= 0f) { pinchStart = Mathf.Max(1f, d); pinchDone = false; }
                    else if (!pinchDone)
                    {
                        float ratio = d / pinchStart;
                        if (ratio > 1.25f) { pinchDone = true; Pinched?.Invoke(true); }
                        else if (ratio < 0.8f) { pinchDone = true; Pinched?.Invoke(false); }
                    }
                    tracking = false;
                    panning = false;
                    lockedPan = false;
                    return true;
                }
                pinchStart = 0f;
            }
            var mouse = Mouse.current;
            if (mouse != null)
            {
                float wheel = mouse.scroll.ReadValue().y;
                if (wheel > 0.5f) Pinched?.Invoke(true);
                else if (wheel < -0.5f) Pinched?.Invoke(false);
            }
            return false;
        }

        private void ReadKeyboard()
        {
            var kb = Keyboard.current;
            if (kb == null) return;
            if (kb.upArrowKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame) DirectionPressed?.Invoke(FromScreen(Vector2.up));
            if (kb.downArrowKey.wasPressedThisFrame || kb.sKey.wasPressedThisFrame) DirectionPressed?.Invoke(FromScreen(Vector2.down));
            if (kb.leftArrowKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame) DirectionPressed?.Invoke(FromScreen(Vector2.left));
            if (kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame) DirectionPressed?.Invoke(FromScreen(Vector2.right));
            if (kb.zKey.wasPressedThisFrame || kb.uKey.wasPressedThisFrame || kb.backspaceKey.wasPressedThisFrame) UndoPressed?.Invoke();
            if (kb.rKey.wasPressedThisFrame) RestartPressed?.Invoke();
        }

        private void ReadPointer()
        {
            var pointer = Pointer.current;
            if (pointer == null) return;
            var pos = pointer.position.ReadValue();

            if (pointer.press.wasPressedThisFrame)
            {
                // Owner 2026-10-31: a quick flick (finger not held) moves the player; a finger held down and dragged slides
                // the view to see the rest of a big yard and the city. Anywhere off the buttons.
                panning = false;
                tracking = !IsOverUi(pos);
                start = pos;
                pressedAt = Time.unscaledTime;
                if (tracking) Pressed?.Invoke(pos);
                lockedPan = false;
                pressPos = pos;
                if (tracking && PanFirst && !(ActsAt?.Invoke(pos) ?? false)) { tracking = false; panning = true; lockedPan = true; }
            }
            if (!pointer.press.isPressed)
            {
                if (pointer.press.wasReleasedThisFrame && tracking && (pos - start).magnitude < MinSwipePixels) Tapped?.Invoke(pos);
                // Owner 2026-11-02: with the worker locked a touch that did not drag is a tap (the view comes to centre on it).
                else if (pointer.press.wasReleasedThisFrame && lockedPan && (pos - pressPos).magnitude < MinSwipePixels) Tapped?.Invoke(pos);
                lockedPan = false;
                tracking = false;
                panning = false;
                return;
            }
            if (panning)
            {
                var framer = Camera.main != null ? Camera.main.GetComponent<CameraFramer>() : null;
                if (framer != null && pos != start) framer.Drag(start, pos);
                start = pos;
                return;
            }
            if (!tracking) return;

            var delta = pos - start;
            if (Time.unscaledTime - pressedAt > holdSeconds && delta.magnitude < MinSwipePixels)
            {
                // Held down without a flick (owner 2026-11-01: a slow start is a drag of the view, not a walk): from now on
                // this touch slides the view.
                tracking = false;
                panning = true;
                start = pos;
                return;
            }
            if (delta.magnitude < MinSwipePixels) return;
            tracking = false; // one move per swipe; holding still first slides the view
            Swiped?.Invoke(start, FromScreen(delta));
        }

        /// <summary>
        /// The grid direction whose on-screen direction is closest to a screen-space vector. With the yard turned towards
        /// the camera (diagonal view, owner 2026-10-31) "up on screen" is the grid edge that points most upwards.
        /// </summary>
        public static Direction FromScreen(Vector2 delta)
        {
            var cam = Camera.main;
            if (cam == null) return Dominant(delta);
            // Ground point under the middle of the screen; project one cell along each grid axis from there.
            var ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Mathf.Abs(ray.direction.y) < 1e-4f) return Dominant(delta);
            var centre = ray.origin + ray.direction * (-ray.origin.y / ray.direction.y);
            Vector2 Screen(Vector3 world) => (Vector2)cam.WorldToScreenPoint(centre + world) - (Vector2)cam.WorldToScreenPoint(centre);
            // Board space: grid x grows to world +x; grid "up" (y - 1) is world +z (BoardView.ToWorld).
            var right = Screen(Vector3.right).normalized;
            var up = Screen(Vector3.forward).normalized;
            float r = Vector2.Dot(delta, right), u = Vector2.Dot(delta, up);
            return Mathf.Abs(r) > Mathf.Abs(u) ? (r > 0 ? Direction.Right : Direction.Left) : (u > 0 ? Direction.Up : Direction.Down);
        }

        private static Direction Dominant(Vector2 delta) =>
            Mathf.Abs(delta.x) > Mathf.Abs(delta.y)
                ? (delta.x > 0 ? Direction.Right : Direction.Left)
                : (delta.y > 0 ? Direction.Up : Direction.Down);

        private bool IsOverUi(Vector2 screenPos)
        {
            var es = EventSystem.current;
            if (es == null) return false;
            uiHits.Clear();
            es.RaycastAll(new PointerEventData(es) { position = screenPos }, uiHits);
            return uiHits.Count > 0;
        }
    }
}
