using System.Collections.Generic;
using System.IO;
using System.Linq;
using CrateBoss.Levels;
using CrateBoss.UI;
using UnityEditor;
using UnityEngine;

namespace CrateBoss.EditorTools
{
    /// <summary>
    /// Tools > Crate Boss > Level Candidates (owner 2026-11-01 brief §6): every level that has hardened candidates, the original
    /// and its candidates side by side as small pictures, with the thinking and labour scores, easy doors, colour-crate share
    /// and the "aha" line from Docs/ADAYLAR.tsv. "Bunu seç" copies a candidate over the game's level file, keeping the original
    /// in LevelBackup/ (outside the build); "Orijinale dön" puts the backup back.
    /// </summary>
    public class LevelCandidatesWindow : EditorWindow
    {
        private const string GameDir = "Assets/_Project/Resources/LeagueLevels";
        private const string Backup = "LevelBackup";

        private Vector2 scroll;
        private Dictionary<string, string[]> table;
        private string[] header;

        private void OnEnable() => Load();

        private void Load()
        {
            table = new Dictionary<string, string[]>();
            if (!File.Exists(HardenLevelsWindow.Table)) return;
            var lines = File.ReadAllLines(HardenLevelsWindow.Table);
            if (lines.Length == 0) return;
            header = lines[0].Split('\t');
            foreach (var l in lines.Skip(1))
            {
                var c = l.Split('\t');
                if (c.Length > 1) table[c[0]] = c;
            }
        }

        private string Cell(string file, string column)
        {
            if (header == null || !table.TryGetValue(file, out var row)) return "-";
            int i = System.Array.IndexOf(header, column);
            return i >= 0 && i < row.Length ? row[i] : "-";
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Yenile")) Load();
            if (!Directory.Exists(HardenLevelsWindow.Folder)) { EditorGUILayout.HelpBox("Henüz aday yok (Tools > Crate Boss > Harden Levels).", MessageType.Info); return; }
            var files = Directory.GetFiles(HardenLevelsWindow.Folder, "league_*_?.json").Select(Path.GetFileName).OrderBy(f => f).ToList();
            scroll = EditorGUILayout.BeginScrollView(scroll);
            foreach (var group in files.GroupBy(f => f.Substring(0, 10)))
            {
                string original = Path.Combine(GameDir, group.Key + ".json");
                EditorGUILayout.LabelField(group.Key, EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                Card("Oyundaki", original, null);
                foreach (var f in group) Card(f, Path.Combine(HardenLevelsWindow.Folder, f), f);
                EditorGUILayout.EndHorizontal();
                if (File.Exists(Path.Combine(Backup, group.Key + ".json")) && GUILayout.Button("Orijinale dön", GUILayout.Width(160)))
                {
                    File.Copy(Path.Combine(Backup, group.Key + ".json"), original, true);
                    AssetDatabase.Refresh();
                }
                EditorGUILayout.Space(12);
            }
            EditorGUILayout.EndScrollView();
        }

        private void Card(string title, string path, string file)
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(250));
            EditorGUILayout.LabelField(title, EditorStyles.miniBoldLabel);
            if (File.Exists(path))
            {
                LevelData data = null;
                try { data = JsonLevel(File.ReadAllText(path)); } catch { }
                if (data != null)
                {
                    var tex = LevelThumbnail.Get(data);
                    float h = Mathf.Min(320f, 230f * tex.height / Mathf.Max(1f, tex.width));
                    GUILayout.Label(tex, GUILayout.Width(230), GUILayout.Height(h));
                }
            }
            if (file != null)
            {
                EditorGUILayout.LabelField($"Mantık {Cell(file, "mantık")} · Zahmet {Cell(file, "zahmet")}");
                EditorGUILayout.LabelField($"Kolay kapı {Cell(file, "kolayKapı")} · Patlatma %{Cell(file, "patlatmaPay")}");
                EditorGUILayout.LabelField(Cell(file, "durum").StartsWith("UYGUN") ? "UYGUN" : "KOLAY", EditorStyles.miniLabel);
                EditorGUILayout.LabelField(Cell(file, "aha"), EditorStyles.wordWrappedMiniLabel);
                if (GUILayout.Button("Bunu seç")) Choose(file);
            }
            EditorGUILayout.EndVertical();
        }

        private static LevelData JsonLevel(string json) => LevelLoader.FromJson(json);

        private static void Choose(string file)
        {
            string key = file.Substring(0, 10);
            string game = Path.Combine(GameDir, key + ".json");
            Directory.CreateDirectory(Backup);
            string backup = Path.Combine(Backup, key + ".json");
            if (!File.Exists(backup) && File.Exists(game)) File.Copy(game, backup); // the original, once
            // The candidate keeps its own name inside the file; the game file keeps the game's file name.
            File.Copy(Path.Combine(HardenLevelsWindow.Folder, file), game, true);
            AssetDatabase.Refresh();
            Debug.Log($"[Level Candidates] {file} → {game} (orijinal: {backup})");
        }
    }
}
