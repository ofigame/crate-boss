using System;
using System.Collections;
using UnityEngine;

namespace CrateBoss.View
{
    /// <summary>
    /// Owner 2026-11-02: a hammer of its own over a breakable crate, striking without the worker: the auto hammer (after three
    /// hits in a row it goes on by itself, faster and faster) and the advanced hammer (a tap from anywhere). Each Strike swings
    /// down onto the lid, calls back when the head lands and swings up again; a new strike cuts the swing back short.
    /// </summary>
    public class AutoHammer : MonoBehaviour
    {
        private Quaternion raised, strike;
        private Coroutine blow;

        /// <summary>Places the hammer (built by the board, its handle end on this transform) beside the crate's lid.</summary>
        public void Setup(Vector3 crate, Vector3 from, bool super)
        {
            var lid = crate + Vector3.up * 0.9f;
            var flat = crate - from;
            flat.y = 0f;
            var toward = flat.sqrMagnitude > 1e-4f ? flat.normalized : Vector3.forward;
            transform.position = lid - toward * 0.62f + Vector3.up * 0.45f;
            strike = Quaternion.LookRotation(lid - transform.position, Vector3.up);
            raised = Quaternion.AngleAxis(-100f, Vector3.Cross(Vector3.up, toward)) * strike;
            transform.rotation = raised;
            if (super) transform.localScale = Vector3.one * 1.25f;
        }

        /// <param name="seconds">The whole blow, down and up.</param>
        public void Strike(float seconds, Action landed)
        {
            if (blow != null) StopCoroutine(blow);
            blow = StartCoroutine(Blow(Mathf.Max(0.03f, seconds), landed));
        }

        private IEnumerator Blow(float seconds, Action landed)
        {
            float down = seconds * 0.55f, up = seconds - down;
            var from = transform.rotation;
            for (float t = 0; t < down; t += Time.deltaTime)
            {
                float k = t / down;
                transform.rotation = Quaternion.Slerp(from, strike, k * k);
                yield return null;
            }
            transform.rotation = strike;
            landed?.Invoke();
            for (float t = 0; t < up; t += Time.deltaTime)
            {
                transform.rotation = Quaternion.Slerp(strike, raised, t / up);
                yield return null;
            }
            transform.rotation = raised;
            blow = null;
        }

        /// <summary>The crate broke or the hammers ran out: the hammer shrinks away.</summary>
        public void Leave() => StartCoroutine(Shrink());

        private IEnumerator Shrink()
        {
            var size = transform.localScale;
            for (float t = 0; t < 0.18f; t += Time.deltaTime)
            {
                transform.localScale = size * (1f - t / 0.18f);
                yield return null;
            }
            Destroy(gameObject);
        }
    }
}
