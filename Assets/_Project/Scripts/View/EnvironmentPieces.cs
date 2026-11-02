using System.Collections.Generic;
using UnityEngine;

namespace CrateBoss.View
{
    /// <summary>Spawns surroundings pieces from their prefabs (real model or placeholder), or straight from EnvShapes when a prefab is missing.</summary>
    public static class EnvironmentPieces
    {
        private static readonly Dictionary<(DistrictKind, EnvPiece), GameObject> cache = new Dictionary<(DistrictKind, EnvPiece), GameObject>();

        public static string ResourcePath(DistrictKind district, EnvPiece piece) => $"Environment/{district}/{piece}";

        public static Transform Spawn(DistrictKind district, EnvPiece piece, Transform parent, Vector3 position, float yaw, int variant)
        {
            if (!cache.TryGetValue((district, piece), out var prefab))
                cache[(district, piece)] = prefab = Resources.Load<GameObject>(ResourcePath(district, piece));
            Transform t;
            if (prefab != null)
            {
                t = Object.Instantiate(prefab, parent, false).transform;
                var placeholder = t.GetComponent<EnvPlaceholder>();
                if (placeholder != null) placeholder.Build(variant);
            }
            else
            {
                t = new GameObject(piece.ToString()).transform;
                t.SetParent(parent, false);
                EnvShapes.Build(district, piece, t, variant);
            }
            t.name = piece.ToString();
            t.localPosition = position;
            t.localRotation = Quaternion.Euler(0f, yaw, 0f);
            return t;
        }
    }
}
