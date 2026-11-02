using UnityEngine;

namespace CrateBoss.View
{
    /// <summary>
    /// A decorative vehicle driving along a road (Ana Belge §2.5: "yolda giden araba"). Never touches the game.
    /// A closed loop goes round and round; an open route (a through road, owner 2026-10-31) starts again at its first point
    /// once it reaches the last, both ends being off screen.
    /// </summary>
    public class EnvVehicle : MonoBehaviour
    {
        private Vector3[] corners;
        private float speed;
        private int leg;
        private float along;
        private bool open;

        /// <summary>Loop through the corner points (local space of the parent) at <paramref name="unitsPerSecond"/>.</summary>
        public void Drive(Vector3[] loop, float unitsPerSecond, float startFraction) => Drive(loop, unitsPerSecond, startFraction, false);

        /// <param name="openRoute">True: the way back from the last point to the first is a jump, not a road.</param>
        public void Drive(Vector3[] route, float unitsPerSecond, float startFraction, bool openRoute)
        {
            corners = route;
            speed = unitsPerSecond;
            open = openRoute;
            float total = 0f;
            for (int i = 0; i < route.Length; i++) total += Length(i);
            float d = startFraction * total;
            for (leg = 0; leg < route.Length; leg++)
            {
                float len = Length(leg);
                if (d <= len) break;
                d -= len;
            }
            leg %= route.Length;
            along = d;
            Place();
        }

        // The jump back on an open route takes no time.
        private float Length(int i) => open && i == corners.Length - 1 ? 0f : Vector3.Distance(corners[i], corners[(i + 1) % corners.Length]);

        private void Update()
        {
            if (corners == null) return;
            along += speed * Time.deltaTime;
            float len = Length(leg);
            while (along > len)
            {
                along -= len;
                leg = (leg + 1) % corners.Length;
                len = Length(leg);
            }
            Place();
        }

        private void Place()
        {
            if (open && leg == corners.Length - 1) leg = 0;
            var a = corners[leg];
            var b = corners[(leg + 1) % corners.Length];
            var dir = b - a;
            transform.localPosition = a + dir.normalized * along;
            if (dir.sqrMagnitude > 0.001f) transform.localRotation = Quaternion.LookRotation(dir.normalized);
        }
    }
}
