using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace CrateBoss.EditorTools
{
    /// <summary>
    /// Placeholder app icon (Ana Belge §0): an orange wooden crate with dark "CB" letters on a
    /// yellow-black hazard-stripe background. Drawn in code, written as a PNG and set as the default icon.
    /// </summary>
    public static class AppIconGenerator
    {
        public const string IconPath = "Assets/_Project/Art/Icon/app_icon.png";
        private const int Size = 512;

        private static readonly string[] LetterC = { ".###.", "#...#", "#....", "#....", "#....", "#...#", ".###." };
        private static readonly string[] LetterB = { "####.", "#...#", "#...#", "####.", "#...#", "#...#", "####." };

        [MenuItem("Tools/Crate Boss/Generate App Icon")]
        public static void Generate()
        {
            var px = new Color32[Size * Size];
            var yellow = new Color32(255, 200, 40, 255);
            var black = new Color32(34, 30, 28, 255);
            var wood = new Color32(232, 132, 48, 255);
            var woodDark = new Color32(168, 84, 30, 255);
            var woodLight = new Color32(250, 168, 82, 255);
            var ink = new Color32(70, 36, 16, 255);

            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                    px[y * Size + x] = ((x + y) / 48) % 2 == 0 ? yellow : black;

            // The crate: a frame of darker planks, horizontal boards inside, a light top edge.
            const int c0 = 76, c1 = Size - 76, frame = 34;
            for (int y = c0; y < c1; y++)
                for (int x = c0; x < c1; x++)
                {
                    bool edge = x < c0 + frame || x >= c1 - frame || y < c0 + frame || y >= c1 - frame;
                    bool seam = !edge && ((y - c0 - frame) % 58) < 5;
                    var col = edge ? woodDark : seam ? woodDark : wood;
                    if (!edge && !seam && ((y - c0 - frame) % 58) > 50) col = woodLight;
                    px[y * Size + x] = col;
                }

            // "CB" from a 5x7 bitmap font, centred on the crate. Rows are stored top-down; textures go bottom-up.
            const int cell = 22, gap = 22;
            int textW = 2 * 5 * cell + gap, textH = 7 * cell;
            int ox = (Size - textW) / 2, oy = (Size - textH) / 2;
            void Letter(string[] rows, int left)
            {
                for (int r = 0; r < 7; r++)
                    for (int c = 0; c < 5; c++)
                    {
                        if (rows[r][c] != '#') continue;
                        for (int dy = 0; dy < cell; dy++)
                            for (int dx = 0; dx < cell; dx++)
                                px[(oy + (6 - r) * cell + dy) * Size + left + c * cell + dx] = ink;
                    }
            }
            Letter(LetterC, ox);
            Letter(LetterB, ox + 5 * cell + gap);

            var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
            tex.SetPixels32(px);
            tex.Apply();
            Directory.CreateDirectory(Path.GetDirectoryName(IconPath));
            File.WriteAllBytes(IconPath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(IconPath, ImportAssetOptions.ForceUpdate);

            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
            PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { icon }, UnityEditor.IconKind.Any);
        }
    }
}
