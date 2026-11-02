using UnityEngine;

namespace CrateBoss.View
{
    /// <summary>Hint (Ana Belge §10): a bobbing arrow over the box to push, pointing the push direction, and a ring where to stand.</summary>
    public class HintMarker : MonoBehaviour
    {
        private Transform arrow;

        public void Build(Vector3 boxPos, Vector3 pushDir, Vector3 standPos)
        {
            var mat = MaterialLibrary.Unlit(new Color(1f, 0.85f, 0.2f));
            arrow = new GameObject("Arrow").transform;
            arrow.SetParent(transform, false);
            arrow.position = boxPos + Vector3.up * 1.35f;
            arrow.rotation = Quaternion.LookRotation(pushDir, Vector3.up);
            Shapes.Cube(arrow, new Vector3(0, 0, -0.12f), new Vector3(0.12f, 0.06f, 0.4f), mat, "Shaft");
            var head1 = Shapes.Cube(arrow, new Vector3(0.08f, 0, 0.12f), new Vector3(0.08f, 0.06f, 0.3f), mat, "Head");
            head1.transform.localRotation = Quaternion.Euler(0, -40, 0);
            var head2 = Shapes.Cube(arrow, new Vector3(-0.08f, 0, 0.12f), new Vector3(0.08f, 0.06f, 0.3f), mat, "Head");
            head2.transform.localRotation = Quaternion.Euler(0, 40, 0);
            Shapes.NoShadows(arrow.gameObject);

            var ring = new GameObject("Stand").transform;
            ring.SetParent(transform, false);
            ring.position = standPos + Vector3.up * 0.02f;
            const float s = 0.7f, b = 0.07f;
            Shapes.Cube(ring, new Vector3(0, 0, s / 2), new Vector3(s, 0.02f, b), mat);
            Shapes.Cube(ring, new Vector3(0, 0, -s / 2), new Vector3(s, 0.02f, b), mat);
            Shapes.Cube(ring, new Vector3(s / 2, 0, 0), new Vector3(b, 0.02f, s), mat);
            Shapes.Cube(ring, new Vector3(-s / 2, 0, 0), new Vector3(b, 0.02f, s), mat);
            Shapes.NoShadows(ring.gameObject);
        }

        private void Update()
        {
            if (arrow == null) return;
            var p = arrow.position;
            p.y = 1.35f + Mathf.Sin(Time.time * 6f) * 0.08f;
            arrow.position = p;
        }
    }
}
