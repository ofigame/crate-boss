using System.IO;
using CrateBoss.View;
using UnityEditor;
using UnityEngine;

namespace CrateBoss.EditorTools
{
    /// <summary>
    /// Tools > Crate Boss > Environment Placeholders (Ana Belge §2.5): one placeholder prefab per district
    /// piece at Art/Environment/&lt;district&gt;/Resources/Environment/&lt;District&gt;/&lt;Piece&gt;.prefab. Each only holds an
    /// EnvPlaceholder that builds simple shapes; overwrite the prefab with a real model (pivot bottom centre, front -Z,
    /// 1 unit = 1 cell) and the game uses it without code changes. Existing prefabs are never overwritten.
    /// </summary>
    public static class EnvironmentPrefabs
    {
        private const string Root = "Assets/_Project/Art/Environment";

        private static readonly (DistrictKind district, string folder, EnvPiece[] pieces)[] Sets =
        {
            (DistrictKind.City, "sehir_deposu", new[] { EnvPiece.Building, EnvPiece.Shop, EnvPiece.Tree, EnvPiece.StreetLamp, EnvPiece.Car }),
        };

        [MenuItem("Tools/Crate Boss/Environment Placeholders")]
        public static void Generate()
        {
            foreach (var (district, folder, pieces) in Sets)
            {
                string dir = $"{Root}/{folder}/Resources/{Path.GetDirectoryName(EnvironmentPieces.ResourcePath(district, pieces[0]))}".Replace('\\', '/');
                Directory.CreateDirectory(dir);
                foreach (var piece in pieces)
                {
                    string path = $"{dir}/{piece}.prefab";
                    if (File.Exists(path)) continue;
                    var go = new GameObject(piece.ToString());
                    var p = go.AddComponent<EnvPlaceholder>();
                    p.district = district;
                    p.piece = piece;
                    PrefabUtility.SaveAsPrefabAsset(go, path);
                    Object.DestroyImmediate(go);
                }
            }
            AssetDatabase.SaveAssets();
        }
    }
}
