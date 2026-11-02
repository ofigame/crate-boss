using System;
using System.Collections;
using System.Collections.Generic;
using CrateBoss.Core;
using UnityEngine;

namespace CrateBoss.View
{
    /// <summary>
    /// Placeholder truck (Ana Belge §5.5, §16): its open cargo bed is the loading area, the cab on the route side (or past the
    /// nearest yard edge without a route). It backs in beeping when its route is clear (≈1.5 s), and drives off with the
    /// crates when the bed is full; the next one backs in the same way.
    /// A prefab at Resources/Models/Truck (cab only, front +Z) replaces the built-in cab.
    /// </summary>
    public class TruckView : MonoBehaviour
    {
        private Vector3 outward;
        private Transform rig;
        private GameFeedback feedback;
        private Vector3 home;

        public void Build(TruckBay bay, Func<GridPos, Vector3> toWorld, int width, int height, GameFeedback sounds) => Build(bay, toWorld, width, height, sounds, true);

        /// <param name="parked">False: no truck stands there now (it backs in later with <see cref="Arrive"/>).</param>
        public void Build(TruckBay bay, Func<GridPos, Vector3> toWorld, int width, int height, GameFeedback sounds, bool parked)
        {
            feedback = sounds;
            var min = new Vector2(float.MaxValue, float.MaxValue);
            var max = new Vector2(float.MinValue, float.MinValue);
            foreach (var c in bay.Cells)
            {
                var p = toWorld(c);
                min = Vector2.Min(min, new Vector2(p.x, p.z));
                max = Vector2.Max(max, new Vector2(p.x, p.z));
            }
            outward = Outward(bay, toWorld, width, height);

            rig = new GameObject("TruckRig").transform;
            rig.SetParent(transform, false);
            var center = new Vector3((min.x + max.x) / 2f, 0f, (min.y + max.y) / 2f);
            rig.localPosition = center;
            home = center;
            var size = new Vector3(max.x - min.x + 1f, 0f, max.y - min.y + 1f);
            var steel = MaterialLibrary.Shared(new Color(0.85f, 0.32f, 0.22f), 0.35f, 0.3f);
            var dark = MaterialLibrary.Shared(new Color(0.18f, 0.19f, 0.21f), 0.2f);
            // Low bed sides round the loading area (the side toward the yard stays open: the tailgate is down).
            Shapes.Cube(rig, new Vector3(0, -0.06f, 0), new Vector3(size.x + 0.1f, 0.06f, size.z + 0.1f), dark, "Bed");
            var along = new Vector3(Mathf.Abs(outward.x), 0, Mathf.Abs(outward.z));
            var side = new Vector3(along.z, 0, along.x);
            float halfAlong = Vector3.Dot(size, along) / 2f, halfAcross = Vector3.Dot(size, side) / 2f;
            foreach (var s in new[] { -1f, 1f })
                Shapes.Cube(rig, side * s * (halfAcross + 0.04f) + Vector3.up * 0.18f, side * 0.06f + along * (halfAlong * 2f) + Vector3.up * 0.36f, steel, "BedSide");
            Shapes.Cube(rig, outward * (halfAlong + 0.04f) + Vector3.up * 0.25f, side * (halfAcross * 2f + 0.1f) + along * 0.08f + Vector3.up * 0.5f, steel, "Headboard");
            var cab = new GameObject("Cab").transform;
            cab.SetParent(rig, false);
            cab.localPosition = outward * (halfAlong + 0.75f);
            cab.localRotation = Quaternion.LookRotation(outward);
            if (VisualCatalog.TryInstantiate("Truck", cab) == null)
            {
                Shapes.Rounded(cab, new Vector3(0, 0.55f, 0), new Vector3(1.1f, 1f, 1.1f), steel, 0.1f, "CabBody");
                Shapes.Cube(cab, new Vector3(0, 0.8f, 0.56f), new Vector3(0.9f, 0.36f, 0.04f), MaterialLibrary.Shared(new Color(0.3f, 0.45f, 0.6f), 0.8f), "Windscreen");
                foreach (var x in new[] { -0.5f, 0.5f })
                    Shapes.Make(PrimitiveType.Cylinder, cab, new Vector3(x, 0.18f, 0f), new Vector3(0.36f, 0.08f, 0.36f), dark, "Wheel").transform.localRotation = Quaternion.Euler(0, 0, 90);
            }
            if (!parked) { rig.localPosition = home + outward * 12f; rig.gameObject.SetActive(false); }
        }

        /// <summary>The side the truck backs in from: along its route, or the nearest edge of the map (world direction).</summary>
        public static Vector3 Outward(TruckBay bay, Func<GridPos, Vector3> toWorld, int width, int height)
        {
            if (bay.HasRoute)
            {
                Vector3 b = Vector3.zero, r = Vector3.zero;
                foreach (var c in bay.Cells) b += toWorld(c);
                foreach (var c in bay.Route) r += toWorld(c);
                var d = r / bay.Route.Count - b / bay.Cells.Count;
                if (d.sqrMagnitude > 0.01f) return Mathf.Abs(d.x) > Mathf.Abs(d.z) ? new Vector3(Mathf.Sign(d.x), 0, 0) : new Vector3(0, 0, Mathf.Sign(d.z));
            }
            var min = new Vector2(float.MaxValue, float.MaxValue);
            var max = new Vector2(float.MinValue, float.MinValue);
            foreach (var c in bay.Cells)
            {
                var p = toWorld(c);
                min = Vector2.Min(min, new Vector2(p.x, p.z));
                max = Vector2.Max(max, new Vector2(p.x, p.z));
            }
            float left = min.x, right = width - 1 - max.x, near = min.y, far = height - 1 - max.y;
            float best = Mathf.Min(Mathf.Min(left, right), Mathf.Min(near, far));
            return best == far ? Vector3.forward : best == left ? Vector3.left : best == right ? Vector3.right : Vector3.back;
        }

        /// <summary>
        /// Candy levels (owner 2026-11-01 "kamyon rastgele gitmesin, yoldan gelsin yoldan gitsin"): how far from the loading
        /// area the road crosses the truck's way out (0 = no road: it slides straight out and back in).
        /// </summary>
        public float RoadDistance { get; set; }

        private const float RoadRun = 16f;

        /// <summary>The full truck closes its tailgate and drives off with these crates.</summary>
        public IEnumerator Leave(List<Transform> crates)
        {
            foreach (var c in crates) c.SetParent(rig, true);
            feedback?.Horn();
            if (RoadDistance > 0f)
            {
                // Out to the road, turn and drive off along it, back the way the trucks come (the road's straight part).
                var junction = home + outward * RoadDistance;
                var along = -new Vector3(outward.z, 0f, -outward.x);
                yield return Slide(home, junction, 0.5f);
                yield return Turn(Quaternion.identity, Quaternion.FromToRotation(outward, along), 0.25f);
                yield return Slide(junction, junction + along * RoadRun, 0.9f);
                rig.localRotation = Quaternion.identity;
                foreach (var c in crates) Destroy(c.gameObject);
                rig.gameObject.SetActive(false);
                yield break;
            }
            yield return Slide(home, home + outward * 12f, 0.55f);
            foreach (var c in crates) Destroy(c.gameObject);
            rig.gameObject.SetActive(false);
        }

        /// <summary>The next truck backs in along its route, beeping ("bip bip").</summary>
        public IEnumerator Arrive()
        {
            rig.gameObject.SetActive(true);
            if (RoadDistance > 0f)
            {
                // Along the road from the left, a turn at the way in, then backing in.
                var junction = home + outward * RoadDistance;
                var along = new Vector3(outward.z, 0f, -outward.x);
                rig.localRotation = Quaternion.FromToRotation(outward, along);
                yield return Slide(junction - along * RoadRun, junction, 0.9f);
                yield return Turn(rig.localRotation, Quaternion.identity, 0.25f);
                feedback?.CraneWarn(); // reversing beep
                yield return Slide(junction, home, 0.8f);
                feedback?.CraneWarn();
                yield break;
            }
            feedback?.CraneWarn(); // reversing beep
            yield return Slide(home + outward * 12f, home, 1.2f);
            feedback?.CraneWarn();
        }

        private IEnumerator Turn(Quaternion from, Quaternion to, float time)
        {
            for (float t = 0; t < time; t += Time.deltaTime)
            {
                rig.localRotation = Quaternion.Slerp(from, to, Mathf.SmoothStep(0, 1, t / time));
                yield return null;
            }
            rig.localRotation = to;
        }

        private IEnumerator Slide(Vector3 from, Vector3 to, float time)
        {
            for (float t = 0; t < time; t += Time.deltaTime)
            {
                rig.localPosition = Vector3.Lerp(from, to, Mathf.SmoothStep(0, 1, t / time));
                yield return null;
            }
            rig.localPosition = to;
        }
    }
}
