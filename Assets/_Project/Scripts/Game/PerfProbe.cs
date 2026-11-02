using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.Profiling;
using UnityEngine;

namespace CrateBoss.Game
{
    /// <summary>
    /// Test helper: run the player with "-perf &lt;file&gt;" and it opens the home screen and a spread of levels, sits idle,
    /// then plays part of each stored solution, writing frame times, hitches, GC runs and render counts to the file, then quits.
    /// </summary>
    public class PerfProbe : MonoBehaviour
    {
        private static readonly int[] Levels = { 0, 9, 24, 39, 59, 74, 89, 109, 124 };

        private string file;
        private readonly StringBuilder log = new StringBuilder();
        private ProfilerRecorder mainThread, batches, setPass, triangles, shadowCasters, gcAlloc;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void StartIfAsked()
        {
            var args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
                if (args[i] == "-perf")
                {
                    new GameObject("PerfProbe").AddComponent<PerfProbe>().file = args[i + 1];
                    return;
                }
        }

        private void OnEnable()
        {
            mainThread = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 15);
            batches = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count");
            setPass = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count");
            triangles = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Triangles Count");
            shadowCasters = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Shadow Casters Count");
            gcAlloc = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
        }

        private void OnDisable()
        {
            mainThread.Dispose(); batches.Dispose(); setPass.Dispose(); triangles.Dispose(); shadowCasters.Dispose(); gcAlloc.Dispose();
        }

        private IEnumerator Start()
        {
            var game = FindAnyObjectByType<GameController>();
            yield return new WaitForSeconds(2f);
            log.AppendLine("phase\tframes\tavgMs\tp95Ms\tmaxMs\tover33\tgcRuns\tgcKB/f\tbatches\tsetpass\ttrisK\tshadowCasters\trenderers\tmaterials\tgameObjects");
            game.ShowHome();
            yield return new WaitForSeconds(1.5f);
            yield return Measure("home", 3f, null);
            foreach (int level in Levels)
            {
                if (level >= game.LevelCount) continue;
                game.LoadLevel(level);
                yield return new WaitForSeconds(2.5f);
                yield return Measure($"L{level + 1}_idle", 3f, null);
                ScreenCapture.CaptureScreenshot(Path.Combine(Path.GetDirectoryName(file), $"perf_level{level + 1:00}.png"));
                yield return null;
                while (game.Best == null) yield return null;
                yield return Measure($"L{level + 1}_play", 0f, Play(game));
                yield return Measure($"L{level + 1}_undo", 0f, Undos(game));
            }
            File.WriteAllText(file, log.ToString());
            Application.Quit();
        }

        // Idle for a while, or (with a game) play up to 14 moves of the stored solution at a quick player's pace.
        private IEnumerator Measure(string phase, float seconds, IEnumerator player)
        {
            var times = new List<float>();
            long gcBytes = 0;
            int gcStart = System.GC.CollectionCount(0);
            float end = Time.realtimeSinceStartup + seconds;
            while (player != null ? player.MoveNext() : Time.realtimeSinceStartup < end)
            {
                yield return null;
                times.Add(Time.unscaledDeltaTime * 1000f);
                if (gcAlloc.Valid) gcBytes += gcAlloc.LastValue;
            }
            if (times.Count == 0) yield break;
            times.Sort();
            float sum = 0; int over = 0;
            foreach (var t in times) { sum += t; if (t > 33.4f) over++; }
            var renderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            var mats = new HashSet<Material>();
            int visible = 0;
            foreach (var r in renderers)
            {
                if (!r.enabled || !r.gameObject.activeInHierarchy) continue;
                visible++;
                foreach (var m in r.sharedMaterials) if (m != null) mats.Add(m);
            }
            log.AppendLine(string.Join("\t", phase, times.Count, (sum / times.Count).ToString("0.0"), times[(int)(times.Count * 0.95f)].ToString("0.0"),
                times[times.Count - 1].ToString("0.0"), over, System.GC.CollectionCount(0) - gcStart, (gcBytes / 1024f / times.Count).ToString("0.0"),
                Val(batches), Val(setPass), triangles.Valid ? (triangles.LastValue / 1000).ToString() : "-", Val(shadowCasters), visible, mats.Count,
                FindObjectsByType<Transform>(FindObjectsSortMode.None).Length));
            if (phase.EndsWith("_idle")) TopMeshes();
        }

        private static long Tris(Mesh m)
        {
            long n = 0;
            for (int i = 0; i < m.subMeshCount; i++) n += m.GetIndexCount(i) / 3;
            return n;
        }

        // Which meshes carry the triangles (instances × triangles), and how many renderers each top-level group holds.
        private void TopMeshes()
        {
            var tris = new Dictionary<string, (int count, long tris, bool readable)>();
            var groups = new Dictionary<string, int>();
            foreach (var mf in FindObjectsByType<MeshFilter>(FindObjectsSortMode.None))
            {
                var r = mf.GetComponent<Renderer>();
                if (mf.sharedMesh == null || r == null || !r.enabled || !mf.gameObject.activeInHierarchy) continue;
                string key = mf.sharedMesh.name;
                tris.TryGetValue(key, out var v);
                tris[key] = (v.count + 1, v.tris + Tris(mf.sharedMesh), mf.sharedMesh.isReadable);
                var g = mf.transform;
                while (g.parent != null && g.parent.parent != null && g.parent.parent.parent != null) g = g.parent;
                string group = g.root.name + "/" + (g.parent != null ? g.parent.name + "/" : "") + g.name;
                groups.TryGetValue(group, out int n);
                groups[group] = n + 1;
            }
            var list = new List<KeyValuePair<string, (int count, long tris, bool readable)>>(tris);
            list.Sort((a, b) => b.Value.tris.CompareTo(a.Value.tris));
            for (int i = 0; i < Mathf.Min(14, list.Count); i++)
                log.AppendLine($"  mesh {list[i].Key}\tx{list[i].Value.count}\t{list[i].Value.tris / 1000}K\treadable={list[i].Value.readable}");
            var gl = new List<KeyValuePair<string, int>>(groups);
            gl.Sort((a, b) => b.Value.CompareTo(a.Value));
            for (int i = 0; i < Mathf.Min(12, gl.Count); i++) log.AppendLine($"  group {gl[i].Key}\t{gl[i].Value}");
        }

        private static string Val(ProfilerRecorder r) => r.Valid ? r.LastValue.ToString() : "-";

        private static IEnumerator Undos(GameController game)
        {
            for (int i = 0; i < 4; i++)
            {
                while (!game.IsIdle) yield return null;
                game.Undo();
                float wait = Time.realtimeSinceStartup + 0.3f;
                while (Time.realtimeSinceStartup < wait) yield return null;
            }
        }

        private static IEnumerator Play(GameController game)
        {
            var moves = game.Best.Moves;
            int n = Mathf.Min(14, moves.Count - 1);
            for (int m = 0; m < n; m++)
            {
                while (!game.IsIdle) yield return null;
                float wait = Time.realtimeSinceStartup + 0.12f;
                while (Time.realtimeSinceStartup < wait) yield return null;
                game.RequestAction(moves[m]);
                yield return null;
            }
            float settle = Time.realtimeSinceStartup + 1.5f;
            while (Time.realtimeSinceStartup < settle) yield return null;
        }
    }
}
