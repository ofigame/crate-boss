using System.Collections;
using UnityEngine;

namespace CrateBoss.View
{
    /// <summary>
    /// Floor hatch under a sinking symbol crate (Ana Belge §5.4): a yellow-black frame whose two flaps swing
    /// down, wait while the crate goes through, then spring shut with a little overshoot and fade away.
    /// </summary>
    public class HatchView : MonoBehaviour
    {
        private Transform left, right;

        public static HatchView Create(Transform parent, Vector3 cell)
        {
            var h = new GameObject("Hatch").AddComponent<HatchView>();
            h.transform.SetParent(parent, false);
            h.transform.position = cell + Vector3.up * 0.012f;
            h.Build();
            return h;
        }

        private void Build()
        {
            var stripe = MaterialLibrary.Shared(new Color(0.98f, 0.78f, 0.15f), 0.4f);
            var dark = MaterialLibrary.Shared(new Color(0.16f, 0.16f, 0.18f), 0.3f);
            for (int i = 0; i < 4; i++)
            {
                var rot = Quaternion.Euler(0, 90 * i, 0);
                var bar = Shapes.Rounded(transform, rot * new Vector3(0, 0, 0.46f), new Vector3(0.98f, 0.03f, 0.08f), i % 2 == 0 ? stripe : dark, 0.01f, "Frame");
                bar.transform.localRotation = rot;
                Shapes.NoShadows(bar);
            }
            var hole = Shapes.Quad(transform, new Vector3(0, 0.002f, 0), new Vector3(0.86f, 0.86f, 1f), Quaternion.Euler(90, 0, 0),
                MaterialLibrary.Unlit(new Color(0.05f, 0.05f, 0.07f)), "Hole");
            Shapes.NoShadows(hole);
            left = Flap(-1, stripe);
            right = Flap(1, stripe);
        }

        // A flap hinged on the frame edge; its pivot is the hinge so it can swing down.
        private Transform Flap(int side, Material mat)
        {
            var hinge = new GameObject(side < 0 ? "HingeL" : "HingeR").transform;
            hinge.SetParent(transform, false);
            hinge.localPosition = new Vector3(0.43f * side, 0.012f, 0);
            var plate = Shapes.Rounded(hinge, new Vector3(-0.215f * side, 0, 0), new Vector3(0.43f, 0.025f, 0.86f), mat, 0.01f, "Flap");
            Shapes.NoShadows(plate);
            return hinge;
        }

        public IEnumerator Open(float duration = 0.12f)
        {
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                SetAngle(Mathf.SmoothStep(0, 1, t / duration) * 95f);
                yield return null;
            }
            SetAngle(95f);
        }

        /// <summary>Springs shut (small overshoot), then fades into the floor.</summary>
        public IEnumerator CloseAndVanish(float duration = 0.22f)
        {
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                float k = t / duration;
                // 95° -> -12° -> 0°: a springy snap.
                float angle = k < 0.7f ? Mathf.Lerp(95f, -12f, k / 0.7f) : Mathf.Lerp(-12f, 0f, (k - 0.7f) / 0.3f);
                SetAngle(angle);
                yield return null;
            }
            SetAngle(0);
            for (float t = 0; t < 0.3f; t += Time.deltaTime) yield return null;
            Destroy(gameObject);
        }

        private void SetAngle(float a)
        {
            // Positive angles swing both flaps down into the pit.
            left.localRotation = Quaternion.Euler(0, 0, -a);
            right.localRotation = Quaternion.Euler(0, 0, a);
        }
    }
}
