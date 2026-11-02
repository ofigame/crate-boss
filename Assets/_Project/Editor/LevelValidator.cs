using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using CrateBoss.Core;
using CrateBoss.Levels;
using CrateBoss.Solver;
using UnityEditor;
using UnityEngine;

namespace CrateBoss.EditorTools
{
    /// <summary>
    /// Tools > Crate Boss > Validate Levels: solves every level, prints the difficulty measures (Ana Belge §10)
    /// and warns about broken design rules (size, jumps between neighbours, idea sentence...).
    /// </summary>
    public static class LevelValidator
    {
        private const int MaxMoves = 150;
        private const double MaxNeighbourJump = 15;
        private const int MaxSlack = 20;

        [MenuItem("Tools/Crate Boss/Validate Levels")]
        public static bool Validate() => Validate(LevelLoader.ResourceFolder);

        /// <param name="folder">Resources folder: "Levels" (the game) or "CandidateLevels" (generated candidates).</param>
        public static bool Validate(string folder)
        {
            var loader = new LevelLoader(folder);
            var analyzer = new LevelAnalyzer();
            var solver = new LevelSolver();
            bool allGood = true;
            var report = new StringBuilder($"[Validate Levels] Resources/{folder}\n");
            var warnings = new StringBuilder();
            var ideas = new Dictionary<string, int>();
            double? previousScore = null;
            try
            {
                foreach (var data in loader.LoadAll())
                {
                    GameState start;
                    try { start = loader.BuildState(data); }
                    catch (LevelFormatException e)
                    {
                        report.AppendLine($"Bölüm {data.id}: HATA {e.Message}");
                        allGood = false;
                        continue;
                    }

                    var m = analyzer.Analyze(start);
                    var r = m.Solution;
                    string status = r.IsSolved ? "çözüldü" : r.HitLimit ? "LİMİT AŞILDI" : "ÇÖZÜLEMEDİ";
                    report.AppendLine($"Bölüm {data.id} '{data.name}': {status} / en kısa {(r.IsSolved ? r.MoveCount.ToString() : "-")} hamle / {r.ExploredStates} durum incelendi");
                    allGood &= r.IsSolved;
                    if (!r.IsSolved) continue;

                    string trap = m.TrapRatio >= 0 ? m.TrapRatio.ToString("0.00", CultureInfo.InvariantCulture) : "?";
                    report.AppendLine($"    M={m.Moves} P={m.Pushes} S={m.States} D={trap} E={m.ExtraBoxes} K={m.MechanicCount} ({m.Mechanics})" +
                                      $" → puan {F(m.Score)} [{TierName(m.Tier)}], tuzakla {F(m.ScoreWithTraps)}");
                    report.AppendLine($"    yol: {Path(r)}");
                    int slack = CraneTolerance.Measure(start, solver);
                    if (slack != CraneTolerance.NoCrane)
                    {
                        string slackText = slack >= MaxSlack ? $"≥{MaxSlack}" : slack.ToString();
                        report.AppendLine($"    makine (vinç ×{start.Cranes.Count}): en kısa {r.MoveCount} / {r.ExploredStates} durum / tolerans {slackText} boş hamle");
                    }

                    // Quality filters (Ana Belge §10.5); the first five levels are the tutorials.
                    bool tutorial = data.league == 1 && data.id <= 1005;
                    var q = QualityCheck.Evaluate(start, data.design?.idea, tutorial);
                    string tol = q.Tolerance == CraneTolerance.NoCrane ? "-" : q.Tolerance >= MaxSlack ? $"≥{MaxSlack}" : q.Tolerance.ToString();
                    string without = !q.HasMachines ? "-" : q.MovesWithoutMachines < 0 ? "çözülmez" : q.MovesWithoutMachines + " hamle";
                    report.AppendLine($"    kalite{(tutorial ? " (öğretici)" : "")}: " +
                                      $"puan {F(q.Score)} / tolerans {tol} / monoton {(q.MonotoneSolvable ? "ÇÖZÜLÜYOR" : "çözülmüyor")}" +
                                      $" / makinesiz {without} / makine önemli {(q.MachinesMatter ? "evet" : "HAYIR")} / kilit {(q.UsesLock ? "var" : "yok")} / alan {q.YardWidth}×{q.YardHeight}");
                    report.AppendLine(q.Passed ? "    filtreler: GEÇTİ" : "    filtreler: KALDI — " + string.Join("; ", q.Failures));

                    string label = $"Bölüm {data.id} '{data.name}'";
                    CheckSize(label, start, m.Tier, warnings);
                    if (m.Moves > MaxMoves) warnings.AppendLine($"{label}: {m.Moves} hamle > {MaxMoves}.");
                    var design = data.design;
                    if (design == null || string.IsNullOrWhiteSpace(design.idea)) warnings.AppendLine($"{label}: ana fikir (design.idea) yazılmamış.");
                    else if (ideas.TryGetValue(design.idea.Trim(), out int other)) warnings.AppendLine($"{label}: ana fikir {other}. bölümle aynı.");
                    else ideas[design.idea.Trim()] = data.id;
                    if (design != null && !string.IsNullOrEmpty(design.tier) && design.tier != TierId(m.Tier))
                        warnings.AppendLine($"{label}: tasarım kademesi '{design.tier}', ölçülen '{TierId(m.Tier)}'.");
                    if (previousScore.HasValue && m.Score - previousScore.Value > MaxNeighbourJump)
                        warnings.AppendLine($"{label}: önceki bölümden {F(m.Score - previousScore.Value)} puan zor (sınır +{MaxNeighbourJump}).");
                    previousScore = m.Score;
                }
            }
            catch (Exception e)
            {
                report.AppendLine($"HATA {e.Message}");
                allGood = false;
            }

            if (warnings.Length > 0) report.Append("\nUyarılar (tasarım kuralları):\n").Append(warnings);
            if (allGood) Debug.Log(report.ToString());
            else Debug.LogError(report.ToString());
            return allGood;
        }

        public static void ValidateBatch() => EditorApplication.Exit(Validate() ? 0 : 1);

        /// <summary>Doc §4.6: portrait-friendly sizes and an interior budget per tier.</summary>
        private static void CheckSize(string label, GameState s, DifficultyTier tier, StringBuilder warnings)
        {
            int w = s.Width, h = s.Height;
            bool widthOk = w >= 5 && w <= 9;
            bool heightOk = h >= w && h <= w * 1.6 || (w == 9 && h == 6);
            if (!widthOk || !heightOk) warnings.AppendLine($"{label}: boyut {w}×{h} dikey ekran kuralı dışında (genişlik 5–9, yükseklik genişlik×1.0–1.6).");
            int inner = 0;
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                var t = s.GetTile(new GridPos(x, y));
                if (t != TileType.Wall && t != TileType.Void) inner++;
            }
            int budget = tier == DifficultyTier.Easy ? 30 : tier == DifficultyTier.Medium ? 45 : tier == DifficultyTier.Hard ? 60 : 75;
            if (inner > budget) warnings.AppendLine($"{label}: iç alan {inner} kare > {budget} ({TierName(tier)}).");
        }

        public static string TierId(DifficultyTier t) =>
            t == DifficultyTier.Easy ? "easy" : t == DifficultyTier.Medium ? "medium" : t == DifficultyTier.Hard ? "hard" : "very_hard";

        public static string TierName(DifficultyTier t) =>
            t == DifficultyTier.Easy ? "Kolay" : t == DifficultyTier.Medium ? "Orta" : t == DifficultyTier.Hard ? "Zor" : "Çok zor";

        private static string F(double v) => v.ToString("0.0", CultureInfo.InvariantCulture);

        private static string Path(SolveResult result)
        {
            var sb = new StringBuilder();
            foreach (var d in result.Moves) sb.Append(d.ToString()[0]);
            return sb.ToString();
        }
    }
}
