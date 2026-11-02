using System;
using System.Collections;
using CrateBoss.Core;
using UnityEngine;

namespace CrateBoss.View
{
    /// <summary>
    /// A parked toy car of the car park (Ana Belge §5.5, §16): the same chunky code-drawn car as the city's, spanning its two
    /// cells. Sent off it leans forward, rolls out through the exit and away; bumping it squashes on the obstacle, a few
    /// stars pop and it hops back onto its bay.
    /// </summary>
    public class CarView : MonoBehaviour
    {
        private Transform body;
        private Vector3 home;
        private Func<GridPos, Vector3> toWorld;
        private ParkedCar car;

        /// <summary>Candy colours by the names used in level files (Ana Belge §2.3).</summary>
        public static Color ColorOf(string name, int fallback)
        {
            switch (name)
            {
                case "pink": return Hex("#F2558A");
                case "purple": return Hex("#9B5DE5");
                case "teal": return Hex("#3EC1D3");
                case "yellow": return Hex("#FFD23F");
                case "orange": return Hex("#FF8C42");
                case "green": return Hex("#3DCC6B");
                case "red": return Hex("#EF4444");
                case "blue": return Hex("#5AA9F0");
                case "white": return Hex("#F7F7F9");
                default:
                    string[] all = { "#F2558A", "#9B5DE5", "#3EC1D3", "#FFD23F", "#FF8C42", "#3DCC6B", "#EF4444", "#5AA9F0" };
                    return Hex(all[Mathf.Abs(fallback) % all.Length]);
            }
        }

        private static Color Hex(string hex) => ColorUtility.TryParseHtmlString(hex, out var c) ? c : Color.magenta;

        public void Build(ParkedCar car, int index, Func<GridPos, Vector3> toWorld)
        {
            this.toWorld = toWorld;
            this.car = car;
            home = (toWorld(car.A) + toWorld(car.B)) / 2f;
            transform.localPosition = home;
            body = new GameObject("Body").transform;
            body.SetParent(transform, false);
            bool alongZ = car.A.X == car.B.X; // grid rows run along world z
            ObstacleBuilder.ToyCar(body, Vector3.zero, alongZ, MaterialLibrary.Shared(ColorOf(car.Color, index), 0.5f), index * 37 + car.A.X * 11 + car.A.Y);
            body.localScale = new Vector3(0.94f, 0.94f, 0.94f);
        }

        /// <summary>Drives until its front is on the exit, then away down the road, and hides.</summary>
        public IEnumerator DriveOff(GridPos exit, Direction dir, EffectsView effects)
        {
            var step = toWorld(exit + dir.ToOffset()) - toWorld(exit);
            var end = home + toWorld(exit) - toWorld(car.Front(dir));
            effects?.ToyClouds(transform.position - step * 0.9f, 2, 0.16f, -step * 0.4f);
            body.localRotation = Quaternion.AngleAxis(4f, Vector3.Cross(Vector3.up, step)); // leans into the start
            yield return Move(home, end, 0.1f + 0.06f * Vector3.Distance(home, end), true);
            yield return Move(end, end + step * 9f, 0.45f, false);
            gameObject.SetActive(false);
        }

        /// <summary>Rolls up to the last free cell, squashes against what is there, and hops back.</summary>
        public IEnumerator Bump(GridPos reached, Direction dir, EffectsView effects)
        {
            var step = toWorld(reached + dir.ToOffset()) - toWorld(reached);
            var hit = home + toWorld(reached) - toWorld(car.Front(dir)) + step * 0.22f;
            yield return Move(home, hit, 0.08f + 0.05f * Vector3.Distance(home, hit), true);
            effects?.Stars(transform.position + step * 1.1f + Vector3.up * 0.6f, 3);
            for (float t = 0; t < 0.12f; t += Time.deltaTime)
            {
                float k = Mathf.Sin(t / 0.12f * Mathf.PI);
                body.localScale = new Vector3(0.94f + 0.08f * k, 0.94f - 0.12f * k, 0.94f + 0.08f * k);
                yield return null;
            }
            body.localScale = Vector3.one * 0.94f;
            yield return Move(hit, home, 0.22f, false, 0.25f);
        }

        private Coroutine pulse;

        /// <summary>Touched (owner 2026-11-01): a cartoon squash and stretch, so the player sees which car the swipe will send.</summary>
        public void Pulse()
        {
            if (!isActiveAndEnabled) return;
            if (pulse != null) StopCoroutine(pulse);
            pulse = StartCoroutine(PulseRoutine());
        }

        private IEnumerator PulseRoutine()
        {
            const float time = 0.32f;
            for (float t = 0; t < time; t += Time.deltaTime)
            {
                float k = t / time;
                // Squash down, stretch up, settle: a damped wobble.
                float w = Mathf.Sin(k * Mathf.PI * 3f) * (1f - k);
                body.localScale = new Vector3(0.94f * (1f + 0.14f * w), 0.94f * (1f - 0.2f * w), 0.94f * (1f + 0.14f * w));
                yield return null;
            }
            body.localScale = Vector3.one * 0.94f;
            pulse = null;
        }

        private IEnumerator Move(Vector3 from, Vector3 to, float time, bool easeIn, float hop = 0f)
        {
            for (float t = 0; t < time; t += Time.deltaTime)
            {
                float k = Mathf.Clamp01(t / time);
                float e = easeIn ? k * k : 1f - (1f - k) * (1f - k);
                transform.localPosition = Vector3.Lerp(from, to, e) + Vector3.up * (hop * Mathf.Sin(k * Mathf.PI));
                yield return null;
            }
            transform.localPosition = to;
            body.localRotation = Quaternion.identity;
        }
    }
}
