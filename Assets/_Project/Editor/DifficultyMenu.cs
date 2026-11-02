using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CrateBoss.Core;
using CrateBoss.Levels;
using CrateBoss.Puzzle;
using UnityEditor;
using UnityEngine;

namespace CrateBoss.EditorTools
{
    /// <summary>
    /// Owner 2026-11-01 brief §1: Tools > Crate Boss > Validate Solutions (replays every stored solution) and Difficulty Check
    /// (measures every level and writes Docs/ZORLUK_TEST_RAPORU.md). Harden Levels opens its own window.
    /// </summary>
    public static class DifficultyMenu
    {
        public const string Report = "Docs/ZORLUK_TEST_RAPORU.md";

        [MenuItem("Tools/Crate Boss/Validate Solutions")]
        public static void ValidateSolutions()
        {
            var levels = new LevelLoader(LevelLoader.GameFolder).LoadAll();
            var broken = new List<string>();
            var rules = new RulesEngine();
            for (int i = 0; i < levels.Count; i++)
            {
                var d = levels[i];
                var moves = SolutionText.Parse(d.solution);
                var s = new LevelParser().Parse(d);
                string why = moves == null ? "çözüm yok" : null;
                for (int m = 0; why == null && m < moves.Count; m++)
                {
                    var r = rules.TryMove(s, moves[m]);
                    if (!r.Success) why = $"{m + 1}. hamle geçersiz";
                    else if (r.IsWon != (m == moves.Count - 1)) why = $"{m + 1}. hamlede kazanıldı ({moves.Count} hamleden)";
                }
                if (why != null) broken.Add($"{i + 1}. {d.name}: {why}");
            }
            string text = broken.Count == 0 ? $"{levels.Count} bölümün hepsinin kayıtlı çözümü kazanıyor." : $"{broken.Count} bölüm bozuk:\n" + string.Join("\n", broken);
            Debug.Log("[Validate Solutions] " + text);
            EditorUtility.DisplayDialog("Validate Solutions", text, "Tamam");
        }

        [MenuItem("Tools/Crate Boss/Difficulty Check")]
        public static void DifficultyCheck()
        {
            var levels = new LevelLoader(LevelLoader.GameFolder).LoadAll();
            var rows = new List<DifficultyTable.Row>();
            try
            {
                for (int i = 0; i < levels.Count; i++)
                {
                    if (EditorUtility.DisplayCancelableProgressBar("Difficulty Check", $"{i + 1}. {levels[i].name}", i / (float)levels.Count)) break;
                    var r = DifficultyMeter.Measure(levels[i], i + 1, SolutionText.Parse(levels[i].solution));
                    rows.Add(new DifficultyTable.Row { Before = r, Aha = AhaText.For(r) });
                }
            }
            finally { EditorUtility.ClearProgressBar(); }
            int easy = rows.Count(r => r.Before.Easy);
            File.WriteAllText(Report, DifficultyTable.Markdown(rows, $"# Zorluk test raporu\n\n{rows.Count} bölüm ölçüldü; {easy} bölüm KOLAY. Mantık ve zahmet puanının formülü: Scripts/Puzzle/DifficultyLimits.cs."), new UTF8Encoding(false));
            Debug.Log($"[Difficulty Check] {rows.Count} bölüm, {easy} kolay. {Report}");
            EditorUtility.DisplayDialog("Difficulty Check", $"{rows.Count} bölüm ölçüldü, {easy} tanesi KOLAY.\n{Report}", "Tamam");
        }

        [MenuItem("Tools/Crate Boss/Harden Levels")]
        public static void Harden() => EditorWindow.GetWindow<HardenLevelsWindow>("Harden Levels");

        [MenuItem("Tools/Crate Boss/Level Candidates")]
        public static void Candidates() => EditorWindow.GetWindow<LevelCandidatesWindow>("Bölüm adayları");
    }
}
