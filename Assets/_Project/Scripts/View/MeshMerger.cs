using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace CrateBoss.View
{
    /// <summary>
    /// Merges still decoration (the district, the yard barrier) into a few meshes: one per material and shadow mode in each
    /// area of the ground, so what is off screen is still culled. Thousands of small renderers made phones stutter; the
    /// picture stays the same (same meshes, materials and shadows). Moving, see-through and text pieces are left alone.
    /// </summary>
    public static class MeshMerger
    {
        /// <summary>Off for "-layoutcheck": the layout check measures every piece on its own.</summary>
        public static bool Enabled = System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-layoutcheck") < 0;

        private const float Area = 8f;
        private const int MaxVertices = 60000;

        private struct Key
        {
            public Material Material;
            public ShadowCastingMode Shadows;
            public bool Receive;
            public int Layer, Layout, AreaX, AreaZ;
        }

        /// <param name="skip">Pieces (and everything under them) that must stay as they are, e.g. moving traffic.</param>
        public static void Merge(Transform root, System.Func<Transform, bool> skip = null)
        {
            if (!Enabled || root == null) return;
            var toRoot = root.worldToLocalMatrix;
            var groups = new Dictionary<Key, List<(Mesh mesh, int sub, Matrix4x4 m, MeshFilter mf)>>();
            foreach (var mf in root.GetComponentsInChildren<MeshFilter>())
            {
                var mesh = mf.sharedMesh;
                var r = mf.GetComponent<MeshRenderer>();
                if (mesh == null || r == null || !r.enabled || !mesh.isReadable || mf.GetComponent<TextMesh>() != null) continue;
                if (Skipped(mf.transform, root, skip)) continue;
                var mats = r.sharedMaterials;
                if (mats.Length != mesh.subMeshCount) continue;
                bool ok = true;
                foreach (var m in mats) if (m == null || m.renderQueue >= (int)RenderQueue.AlphaTest + 50) ok = false; // see-through: keep its own sort
                if (!ok) continue;
                var matrix = toRoot * mf.transform.localToWorldMatrix;
                if (matrix.determinant <= 0f) continue; // mirrored pieces would turn inside out
                var c = r.bounds.center;
                for (int s = 0; s < mats.Length; s++)
                {
                    var key = new Key
                    {
                        Material = mats[s], Shadows = r.shadowCastingMode, Receive = r.receiveShadows, Layer = mf.gameObject.layer,
                        Layout = Layout(mesh), AreaX = Mathf.FloorToInt(c.x / Area), AreaZ = Mathf.FloorToInt(c.z / Area),
                    };
                    if (!groups.TryGetValue(key, out var list)) groups[key] = list = new List<(Mesh, int, Matrix4x4, MeshFilter)>();
                    list.Add((mesh, s, matrix, mf));
                }
            }

            var merged = new HashSet<MeshFilter>();
            var parts = new List<CombineInstance>();
            foreach (var kv in groups)
            {
                if (kv.Value.Count < 2) continue;
                int start = 0;
                while (start < kv.Value.Count)
                {
                    // Pieces of one renderer go together or not at all, so a renderer is removed only when all of it merged.
                    parts.Clear();
                    int vertices = 0, i = start;
                    for (; i < kv.Value.Count; i++)
                    {
                        var p = kv.Value[i];
                        if (vertices > 0 && vertices + p.mesh.vertexCount > MaxVertices) break;
                        vertices += p.mesh.vertexCount;
                        parts.Add(new CombineInstance { mesh = p.mesh, subMeshIndex = p.sub, transform = p.m });
                    }
                    if (parts.Count >= 2)
                    {
                        var mesh = new Mesh { name = "Merged " + kv.Key.Material.name, indexFormat = vertices > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16 };
                        mesh.CombineMeshes(parts.ToArray(), true, true);
                        mesh.UploadMeshData(true);
                        var go = new GameObject("Merged");
                        go.layer = kv.Key.Layer;
                        go.transform.SetParent(root, false);
                        go.AddComponent<MeshFilter>().sharedMesh = mesh;
                        var mr = go.AddComponent<MeshRenderer>();
                        mr.sharedMaterial = kv.Key.Material;
                        mr.shadowCastingMode = kv.Key.Shadows;
                        mr.receiveShadows = kv.Key.Receive;
                        for (int k = start; k < i; k++) merged.Add(kv.Value[k].mf);
                    }
                    start = i;
                }
            }

            // A renderer whose every sub-mesh merged no longer draws; one with a sub-mesh left over keeps drawing that part.
            var done = new Dictionary<MeshFilter, int>();
            foreach (var kv in groups)
            foreach (var p in kv.Value)
                if (merged.Contains(p.mf)) { done.TryGetValue(p.mf, out int n); done[p.mf] = n + 1; }
            foreach (var kv in done)
            {
                var mf = kv.Key;
                if (kv.Value != mf.sharedMesh.subMeshCount) continue;
                var r = mf.GetComponent<MeshRenderer>();
                Object.Destroy(r);
                Object.Destroy(mf);
            }
        }

        private static bool Skipped(Transform t, Transform root, System.Func<Transform, bool> skip)
        {
            if (skip == null) return false;
            for (; t != null && t != root; t = t.parent) if (skip(t)) return true;
            return false;
        }

        // Meshes with different vertex data (uv, tangents, colours) are merged apart.
        private static int Layout(Mesh mesh)
        {
            int h = mesh.vertexAttributeCount;
            for (int i = 0; i < mesh.vertexAttributeCount; i++) h = h * 31 + (int)mesh.GetVertexAttribute(i).attribute;
            return h;
        }
    }
}
