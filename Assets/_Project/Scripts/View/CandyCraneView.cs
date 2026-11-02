using System.Collections;
using UnityEngine;

namespace CrateBoss.View
{
    /// <summary>
    /// One of the four cranes round a candy level's yard (owner 2026-11-01): a tower crane at a corner outside the fence that
    /// swings its jib in toward an emptied cell when it drops a new colour crate there, and back out to its corner after
    /// (owner 2026-11-02: no jib hangs over the yard). Only a picture; CandySystem decides.
    /// </summary>
    public class CandyCraneView : MonoBehaviour
    {
        public const float JibHeight = 4.6f;
        private const float SwingTime = 0.14f;

        private Transform pivot;
        private Vector3 home; // a point away from the yard: the jib rests pointing out, over the corner

        public void Build(Vector3 mast, Vector3 lookAt)
        {
            transform.position = mast;
            pivot = new GameObject("Slewing").transform;
            pivot.SetParent(transform, false);
            pivot.localPosition = Vector3.up * JibHeight;
            // Owner 2026-11-01: thin and see-through, so they frame the yard without hiding anything.
            var glass = Glass;
            var model = VisualCatalog.TryInstantiate("CraneBody", pivot);
            if (model != null) model.transform.localScale = Vector3.Scale(model.transform.localScale, new Vector3(0.55f, 1f, 1f));
            else
            {
                // No model: a mast and a jib.
                Shapes.Cube(transform, Vector3.up * JibHeight / 2f, new Vector3(0.18f, JibHeight, 0.18f), glass, "Mast");
                Shapes.Cube(pivot, new Vector3(0f, 0.1f, 1.6f), new Vector3(0.16f, 0.2f, 3.6f), glass, "Jib");
            }
            foreach (var r in GetComponentsInChildren<Renderer>())
            {
                var mats = r.sharedMaterials;
                for (int i = 0; i < mats.Length; i++) mats[i] = glass;
                r.sharedMaterials = mats;
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            var away = mast - lookAt;
            away.y = 0f;
            home = mast + (away.sqrMagnitude > 1e-4f ? away.normalized : Vector3.back) * 3f;
            Face(home);
        }

        /// <summary>Owner 2026-11-02: back to the corner, out of the yard, after dropping (a new drop turns it in again).</summary>
        public void GoHome()
        {
            if (homing != null) StopCoroutine(homing);
            homing = StartCoroutine(Swing(home));
        }

        private Coroutine homing;

        private static Material glass;
        private static Material Glass => glass != null ? glass : glass = MaterialLibrary.Create(new Color(Palette.CraneYellow.r, Palette.CraneYellow.g, Palette.CraneYellow.b, 0.45f), 0.4f);

        /// <summary>Turns the jib toward a world point and waits until it points there.</summary>
        public IEnumerator SwingTo(Vector3 world)
        {
            if (homing != null) { StopCoroutine(homing); homing = null; }
            yield return Swing(world);
        }

        private IEnumerator Swing(Vector3 world)
        {
            var from = pivot.rotation;
            var to = Aim(world);
            for (float t = 0; t < SwingTime; t += Time.deltaTime)
            {
                pivot.rotation = Quaternion.Slerp(from, to, Mathf.SmoothStep(0f, 1f, t / SwingTime));
                yield return null;
            }
            pivot.rotation = to;
        }

        public void Face(Vector3 world) => pivot.rotation = Aim(world);

        private const float LowerTime = 0.17f, RetractTime = 0.1f, CrateTop = 0.86f;

        /// <summary>
        /// Owner 2026-11-01 "vinçlerin kasayı bıraktıkları görünsün, hızlı ama görünsün": the crate hangs on a rope and hook
        /// under the jib over its cell, is lowered quickly onto the floor, let go, and the rope and hook go back up.
        /// </summary>
        public IEnumerator Lower(Transform crate, Vector3 rest)
        {
            float top = transform.position.y + JibHeight - 0.15f;
            var rope = new GameObject("Rope").transform;
            rope.SetParent(transform.parent, false);
            rope.position = new Vector3(rest.x, top, rest.z);
            var line = Shapes.Cube(rope, new Vector3(0f, -0.5f, 0f), new Vector3(0.035f, 1f, 0.035f), RopeMaterial, "Line");
            Shapes.NoShadows(line);
            var hook = new GameObject("Hook").transform;
            hook.SetParent(transform.parent, false);
            if (VisualCatalog.TryInstantiate("CraneHook", hook) == null)
                Shapes.Cube(hook, new Vector3(0f, 0.1f, 0f), new Vector3(0.16f, 0.2f, 0.16f), RopeMaterial, "HookBlock");
            float start = top - 1.2f - CrateTop; // hanging a little under the jib
            void Place(float y)
            {
                crate.position = new Vector3(rest.x, y, rest.z);
                float hookY = y + CrateTop;
                hook.position = new Vector3(rest.x, hookY, rest.z);
                rope.localScale = new Vector3(1f, Mathf.Max(0.01f, top - hookY), 1f);
            }
            for (float t = 0; t < LowerTime; t += Time.deltaTime)
            {
                float k = t / LowerTime;
                Place(Mathf.Lerp(start, rest.y, k * k));
                yield return null;
            }
            Place(rest.y);
            // Let go: the empty hook goes back up.
            for (float t = 0; t < RetractTime; t += Time.deltaTime)
            {
                float hookY = Mathf.Lerp(rest.y + CrateTop, top, t / RetractTime);
                hook.position = new Vector3(rest.x, hookY, rest.z);
                rope.localScale = new Vector3(1f, Mathf.Max(0.01f, top - hookY), 1f);
                yield return null;
            }
            Destroy(rope.gameObject);
            Destroy(hook.gameObject);
        }

        private static Material ropeMaterial;
        private static Material RopeMaterial => ropeMaterial != null ? ropeMaterial : ropeMaterial = MaterialLibrary.Create(new Color(0.22f, 0.22f, 0.25f), 0.3f);

        private Quaternion Aim(Vector3 world)
        {
            var d = world - transform.position;
            d.y = 0f;
            return d.sqrMagnitude < 1e-4f ? pivot.rotation : Quaternion.LookRotation(d.normalized, Vector3.up);
        }
    }
}
