using System.Collections.Generic;
using UnityEngine;

namespace CrateBoss.View
{
    /// <summary>
    /// Real models (Ana Belge §19): if a prefab exists at Resources/Models/&lt;key&gt; (made by
    /// Tools > Crate Boss > Normalize Models, e.g. "BoxNormal", "Target", "CraneBody", "Worker") it is used
    /// instead of the built-in shapes. Pivot at the bottom centre, 1 unit = 1 cell, already at its game size.
    /// </summary>
    public static class VisualCatalog
    {
        /// <summary>Yard floor: depot concrete with dashed cell lines (default) or the earlier checker slabs.</summary>
        public static FloorStyle Floor = FloorStyle.Concrete;

        private static readonly Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();
        private static readonly Dictionary<string, Bounds> bounds = new Dictionary<string, Bounds>();

        /// <summary>The player's rank model set (Ana Belge §7): "R2" looks for "BoxNormal_R2" first; "" = only the base models.</summary>
        public static string RankSuffix = "";

        public static bool Has(string key) => Load(key) != null;

        /// <summary>The rank's own model of a crate or fixed piece when there is one, else the base model, else null.</summary>
        public static GameObject TryInstantiateRanked(string key, Transform parent) =>
            (RankSuffix.Length > 0 && Has(key + "_" + RankSuffix) ? TryInstantiate(key + "_" + RankSuffix, parent) : null) ?? TryInstantiate(key, parent);

        public static GameObject TryInstantiate(string key, Transform parent)
        {
            var prefab = Load(key);
            if (prefab == null) return null;
            var go = Object.Instantiate(prefab, parent, false);
            go.name = key;
            MaterialLibrary.MakeToy(go);
            if (key.StartsWith("BoxFixed")) CrateStains.Apply(go); // owner 2026-11-01: fixed crates must not look like pushable ones
            return go;
        }

        /// <summary>The model's box around its pivot (from the meshes, no instance needed); false when there is no model.</summary>
        public static bool TryGetBounds(string key, out Bounds b)
        {
            if (bounds.TryGetValue(key, out b)) return true;
            var prefab = Load(key);
            if (prefab == null) return false;
            var toRoot = prefab.transform.worldToLocalMatrix;
            bool first = true;
            foreach (var mf in prefab.GetComponentsInChildren<MeshFilter>(true))
            {
                if (mf.sharedMesh == null) continue;
                var m = toRoot * mf.transform.localToWorldMatrix;
                var mb = mf.sharedMesh.bounds;
                for (int i = 0; i < 8; i++)
                {
                    var corner = mb.center + Vector3.Scale(mb.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                    var p = m.MultiplyPoint3x4(corner);
                    if (first) { b = new Bounds(p, Vector3.zero); first = false; }
                    else b.Encapsulate(p);
                }
            }
            if (first) return false;
            bounds[key] = b;
            return true;
        }

        private static GameObject Load(string key)
        {
            if (prefabs.TryGetValue(key, out var p)) return p;
            p = Resources.Load<GameObject>($"Models/{key}");
            prefabs[key] = p;
            return p;
        }
    }
}
