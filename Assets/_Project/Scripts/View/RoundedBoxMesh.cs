using System.Collections.Generic;
using UnityEngine;

namespace CrateBoss.View
{
    /// <summary>
    /// Unit box (-0.5..0.5) with rounded (bevelled) edges and smooth normals.
    /// Sharp 90° edges look cheap; every block in the game uses this instead of Unity's cube.
    /// </summary>
    public static class RoundedBoxMesh
    {
        private const int BevelSegments = 3;
        private static readonly Dictionary<int, Mesh> cache = new Dictionary<int, Mesh>();

        /// <param name="radius">Bevel radius relative to the unit box (0..0.5).</param>
        public static Mesh Get(float radius)
        {
            radius = Mathf.Clamp(radius, 0.001f, 0.49f);
            int key = Mathf.RoundToInt(radius * 1000f);
            if (cache.TryGetValue(key, out var mesh) && mesh != null) return mesh;
            mesh = Build(radius);
            cache[key] = mesh;
            return mesh;
        }

        private static Mesh Build(float r)
        {
            // Sample positions along one axis: dense inside the bevel, one span across the flat middle.
            var samples = new List<float>();
            for (int i = 0; i <= BevelSegments; i++) samples.Add(-0.5f + r * i / BevelSegments);
            for (int i = BevelSegments; i >= 0; i--) samples.Add(0.5f - r * i / BevelSegments);

            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();
            float inner = 0.5f - r;
            int n = samples.Count;

            // Six faces: axis index of the face normal and its sign.
            for (int face = 0; face < 6; face++)
            {
                int axis = face / 2;
                float sign = face % 2 == 0 ? 1f : -1f;
                int u = (axis + 1) % 3, v = (axis + 2) % 3;
                int start = vertices.Count;
                for (int j = 0; j < n; j++)
                for (int i = 0; i < n; i++)
                {
                    var p = Vector3.zero;
                    p[axis] = 0.5f * sign;
                    p[u] = samples[i];
                    p[v] = samples[j];
                    var core = new Vector3(Mathf.Clamp(p.x, -inner, inner), Mathf.Clamp(p.y, -inner, inner), Mathf.Clamp(p.z, -inner, inner));
                    var normal = (p - core).normalized;
                    vertices.Add(core + normal * r);
                    normals.Add(normal);
                    uvs.Add(new Vector2(samples[i] + 0.5f, samples[j] + 0.5f));
                }
                for (int j = 0; j < n - 1; j++)
                for (int i = 0; i < n - 1; i++)
                {
                    int a = start + j * n + i, b = a + 1, c = a + n, d = c + 1;
                    // Unity's front faces wind clockwise: normal = cross(v1 - v0, v2 - v0).
                    if (sign > 0) { triangles.Add(a); triangles.Add(b); triangles.Add(c); triangles.Add(b); triangles.Add(d); triangles.Add(c); }
                    else { triangles.Add(a); triangles.Add(c); triangles.Add(b); triangles.Add(b); triangles.Add(c); triangles.Add(d); }
                }
            }

            var mesh = new Mesh { name = $"RoundedBox_{r:0.000}" };
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            return mesh;
        }
    }
}
