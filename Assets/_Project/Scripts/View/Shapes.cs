using UnityEngine;

namespace CrateBoss.View
{
    /// <summary>Unity primitives and rounded boxes without colliders (the game never uses physics).</summary>
    public static class Shapes
    {
        public static GameObject Make(PrimitiveType type, Transform parent, Vector3 localPos, Vector3 scale, Material material, string name = null)
        {
            var go = GameObject.CreatePrimitive(type);
            if (name != null) go.name = name;
            var col = go.GetComponent<Collider>();
            if (col != null) Object.Destroy(col);
            Place(go.transform, parent, localPos, scale);
            go.GetComponent<Renderer>().sharedMaterial = material;
            return go;
        }

        public static GameObject Cube(Transform parent, Vector3 localPos, Vector3 scale, Material material, string name = null) =>
            Make(PrimitiveType.Cube, parent, localPos, scale, material, name);

        /// <summary>A box with bevelled edges. Radius is in world units (it is divided by the smallest side).</summary>
        public static GameObject Rounded(Transform parent, Vector3 localPos, Vector3 scale, Material material, float radius, string name = "Rounded")
        {
            var go = new GameObject(name);
            float smallest = Mathf.Min(scale.x, Mathf.Min(scale.y, scale.z));
            go.AddComponent<MeshFilter>().sharedMesh = RoundedBoxMesh.Get(radius / Mathf.Max(0.001f, smallest));
            go.AddComponent<MeshRenderer>().sharedMaterial = material;
            Place(go.transform, parent, localPos, scale);
            return go;
        }

        /// <summary>A sphere for small details (post caps): a few pixels on screen, so 168 triangles instead of Unity's 768.</summary>
        public static GameObject SmallSphere(Transform parent, Vector3 localPos, Vector3 scale, Material material, string name = "Sphere")
        {
            var go = new GameObject(name);
            go.AddComponent<MeshFilter>().sharedMesh = smallSphere ??= BuildSphere(12, 8);
            go.AddComponent<MeshRenderer>().sharedMaterial = material;
            Place(go.transform, parent, localPos, scale);
            return go;
        }

        private static Mesh smallSphere;

        private static Mesh BuildSphere(int segments, int rings)
        {
            var vertices = new System.Collections.Generic.List<Vector3>();
            var uvs = new System.Collections.Generic.List<Vector2>();
            var triangles = new System.Collections.Generic.List<int>();
            for (int r = 0; r <= rings; r++)
            {
                float v = r / (float)rings, phi = v * Mathf.PI;
                for (int s = 0; s <= segments; s++)
                {
                    float u = s / (float)segments, theta = u * Mathf.PI * 2f;
                    vertices.Add(new Vector3(Mathf.Sin(phi) * Mathf.Cos(theta), Mathf.Cos(phi), Mathf.Sin(phi) * Mathf.Sin(theta)) * 0.5f);
                    uvs.Add(new Vector2(u, 1f - v));
                }
            }
            for (int r = 0; r < rings; r++)
            for (int s = 0; s < segments; s++)
            {
                int a = r * (segments + 1) + s, b = a + segments + 1;
                if (r > 0) { triangles.Add(a); triangles.Add(a + 1); triangles.Add(b); }
                if (r < rings - 1) { triangles.Add(a + 1); triangles.Add(b + 1); triangles.Add(b); }
            }
            var mesh = new Mesh { name = "SmallSphere" };
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            var normals = new System.Collections.Generic.List<Vector3>();
            foreach (var p in vertices) normals.Add(p * 2f); // no seam on a sphere: the normal is the direction
            mesh.SetNormals(normals);
            mesh.RecalculateBounds();
            return mesh;
        }

        public static GameObject Quad(Transform parent, Vector3 localPos, Vector3 scale, Quaternion rotation, Material material, string name = "Quad")
        {
            var go = Make(PrimitiveType.Quad, parent, localPos, scale, material, name);
            go.transform.localRotation = rotation;
            return go;
        }

        public static void NoShadows(GameObject go)
        {
            foreach (var r in go.GetComponentsInChildren<Renderer>())
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        private static void Place(Transform t, Transform parent, Vector3 localPos, Vector3 scale)
        {
            t.SetParent(parent, false);
            t.localPosition = localPos;
            t.localScale = scale;
        }
    }
}
