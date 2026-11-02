using System.Collections.Generic;
using UnityEngine;

namespace CrateBoss.View
{
    /// <summary>
    /// Through roads of the district (owner 2026-10-31: no road loops round the yard; a road runs past and carries on into
    /// side streets, turns are smooth curves, vehicles keep to their lane). A road is a line of corner points with right-angle
    /// turns; the turns are rounded with <see cref="Radius"/>, then drawn as a pavement band, the asphalt and a dashed
    /// centre line, in short pieces so Validate Layout and the placer get tight boxes.
    /// </summary>
    public static class RoadNetwork
    {
        public const float Width = 2.6f, Kerb = 1.1f, Radius = 3.2f;
        private const int ArcSteps = 12;
        private const int QuadsPerPiece = 4;

        /// <summary>The centre line with every corner rounded (corners turn by 90°).</summary>
        public static List<Vector3> Smooth(IList<Vector3> corners, float radius = Radius)
        {
            var path = new List<Vector3> { corners[0] };
            for (int i = 1; i < corners.Count - 1; i++)
            {
                var p = corners[i];
                var a = (p - corners[i - 1]).normalized;
                var b = (corners[i + 1] - p).normalized;
                var centre = p + (b - a) * radius;
                var s = p - a * radius;
                var e = p + b * radius;
                float a0 = Mathf.Atan2(s.z - centre.z, s.x - centre.x), a1 = Mathf.Atan2(e.z - centre.z, e.x - centre.x);
                float delta = Mathf.DeltaAngle(a0 * Mathf.Rad2Deg, a1 * Mathf.Rad2Deg) * Mathf.Deg2Rad;
                for (int k = 0; k <= ArcSteps; k++)
                {
                    float ang = a0 + delta * k / ArcSteps;
                    path.Add(centre + new Vector3(Mathf.Cos(ang), 0, Mathf.Sin(ang)) * radius);
                }
            }
            path.Add(corners[corners.Count - 1]);
            return path;
        }

        /// <summary>Draws the road; the asphalt pieces' boxes go into <paramref name="footprints"/> so nothing is placed on it.</summary>
        public static void Build(Transform t, List<Vector3> centre, WorldTheme theme, List<Bounds> footprints)
        {
            Ribbon(t, centre, Width + 2f * Kerb, -0.052f, MaterialLibrary.Shared(theme.Sidewalk, 0.1f), "Pavement", null);
            Ribbon(t, centre, Width, -0.047f, MaterialLibrary.Shared(theme.Road, 0.1f), "Road", footprints);
            Dashes(t, centre, MaterialLibrary.Shared(theme.RoadLine, 0.2f));
        }

        /// <summary>The line <paramref name="offset"/> to the right of the direction of travel (a driving lane).</summary>
        public static Vector3[] Lane(IList<Vector3> centre, float offset)
        {
            var lane = new Vector3[centre.Count];
            for (int i = 0; i < centre.Count; i++) lane[i] = centre[i] + Right(centre, i) * offset;
            return lane;
        }

        /// <summary>Points every <paramref name="step"/> along the line with the right-hand direction there (lamps, trees).</summary>
        public static IEnumerable<(Vector3 point, Vector3 right)> Along(IList<Vector3> centre, float step, float start)
        {
            float next = start, walked = 0f;
            for (int i = 0; i < centre.Count - 1; i++)
            {
                float len = Vector3.Distance(centre[i], centre[i + 1]);
                if (len < 1e-4f) continue;
                var dir = (centre[i + 1] - centre[i]) / len;
                while (next <= walked + len)
                {
                    yield return (centre[i] + dir * (next - walked), new Vector3(dir.z, 0, -dir.x));
                    next += step;
                }
                walked += len;
            }
        }

        private static Vector3 Right(IList<Vector3> c, int i)
        {
            var d = (c[Mathf.Min(i + 1, c.Count - 1)] - c[Mathf.Max(i - 1, 0)]).normalized;
            return new Vector3(d.z, 0, -d.x);
        }

        private static void Ribbon(Transform t, IList<Vector3> c, float width, float y, Material mat, string name, List<Bounds> footprints)
        {
            for (int start = 0; start < c.Count - 1; start += QuadsPerPiece)
            {
                int end = Mathf.Min(start + QuadsPerPiece, c.Count - 1);
                var verts = new List<Vector3>();
                var tris = new List<int>();
                for (int i = start; i <= end; i++)
                {
                    var r = Right(c, i) * (width / 2f);
                    verts.Add(c[i] - r + Vector3.up * y);
                    verts.Add(c[i] + r + Vector3.up * y);
                }
                for (int q = 0; q < end - start; q++)
                {
                    int a = q * 2;
                    tris.AddRange(new[] { a, a + 2, a + 1, a + 1, a + 2, a + 3 });
                }
                var mesh = new Mesh { name = name };
                mesh.SetVertices(verts);
                mesh.SetTriangles(tris, 0);
                if (Vector3.Cross(verts[2] - verts[0], verts[1] - verts[0]).y < 0f)
                {
                    for (int i = 0; i < tris.Count; i += 3) (tris[i + 1], tris[i + 2]) = (tris[i + 2], tris[i + 1]);
                    mesh.SetTriangles(tris, 0);
                }
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
                var go = new GameObject(name);
                go.transform.SetParent(t, false);
                go.AddComponent<MeshFilter>().sharedMesh = mesh;
                var mr = go.AddComponent<MeshRenderer>();
                mr.sharedMaterial = mat;
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                footprints?.Add(mr.bounds);
            }
        }

        private static void Dashes(Transform t, IList<Vector3> c, Material paint)
        {
            foreach (var (p, right) in Along(c, 1.1f, 0.5f))
            {
                var dir = new Vector3(-right.z, 0, right.x);
                var dash = Shapes.Cube(t, p + Vector3.up * -0.04f, new Vector3(0.09f, 0.012f, 0.55f), paint, "Dash");
                dash.transform.localRotation = Quaternion.LookRotation(dir);
                Shapes.NoShadows(dash);
            }
        }
    }
}
