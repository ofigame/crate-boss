using System.IO;
using CrateBoss.Core;
using System.Linq;
using System.Text;
using CrateBoss.Levels;
using CrateBoss.Puzzle;
using UnityEditor;
using UnityEngine;

namespace CrateBoss.EditorTools
{
    /// <summary>
    /// Tools > Crate Boss > Harden Levels (brief §5): hardens the easy levels of a range one by one, with a progress bar and
    /// Cancel; a level that already has candidates is skipped, so a second run goes on where the first stopped. Candidates go
    /// to Resources/LeagueLevels_Hard (league_XXX_a.json …) and their measures to Docs/ADAYLAR.tsv; the game's files stay.
    /// </summary>
    public class HardenLevelsWindow : EditorWindow
    {
        public const string Folder = "Assets/_Project/Resources/LeagueLevels_Hard";
        public const string Table = "Docs/ADAYLAR.tsv";

        private int from = 6, to = 25;
        private float minutes = 10f;

        private void OnGUI()
        {
            from = EditorGUILayout.IntField("İlk bölüm", Mathf.Max(DifficultyLimits.Tutorials + 1, from));
            to = EditorGUILayout.IntField("Son bölüm", Mathf.Clamp(to, from, 125));
            minutes = EditorGUILayout.FloatField("Bölüm başı dakika", Mathf.Max(0.5f, minutes));
            if (GUILayout.Button("Zorlaştır")) Run();
        }

        private void Run()
        {
            Directory.CreateDirectory(Folder);
            var levels = new LevelLoader(LevelLoader.GameFolder).LoadAll();
            bool cancel = false;
            try
            {
                for (int n = from; n <= to && n <= levels.Count && !cancel; n++)
                {
                    if (File.Exists(Path.Combine(Folder, $"league_{n:000}_a.json"))) continue; // done before
                    var d = levels[n - 1];
                    var before = DifficultyMeter.Measure(d, n, SolutionText.Parse(d.solution));
                    if (!before.Easy) continue;
                    var clock = System.Diagnostics.Stopwatch.StartNew();
                    var options = new HardenOptions { Seconds = minutes * 60 };
                    var cands = LevelHardener.Harden(d, n, options, () =>
                        cancel = cancel || EditorUtility.DisplayCancelableProgressBar("Harden Levels", $"{n}. {d.name} ({clock.Elapsed.TotalSeconds:0} sn)", (n - from) / (float)(to - from + 1)));
                    var table = new StringBuilder();
                    if (!File.Exists(Table)) table.Append("dosya\t" + DifficultyTable.TsvHeader + "\taha\n");
                    for (int i = 0; i < cands.Count && i < 3; i++)
                    {
                        string name = $"league_{n:000}_{(char)('a' + i)}.json";
                        File.WriteAllText(Path.Combine(Folder, name), LevelJsonWriter.ToJson(cands[i].Data), new UTF8Encoding(false));
                        table.Append(name + "\t" + DifficultyTable.TsvRow(cands[i].Report) + "\t" + cands[i].Aha + "\n");
                    }
                    File.AppendAllText(Table, table.ToString(), new UTF8Encoding(false));
                }
            }
            finally { EditorUtility.ClearProgressBar(); }
            AssetDatabase.Refresh();
        }
    }
}
