using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CrateBoss.Levels;
using CrateBoss.Solver;
using UnityEditor;
using UnityEngine;

namespace CrateBoss.EditorTools
{
    /// <summary>
    /// Tools > Crate Boss > Level Editor: paint ground and objects on a grid, see the solver's verdict
    /// and difficulty score right away, save as JSON into Resources/Levels.
    /// </summary>
    public class LevelEditorWindow : EditorWindow
    {
        private const string LevelsDir = "Assets/_Project/Resources/Levels";
        private static readonly char[] GroundBrushes = { '#', '.', 'T', 'o', '_' };
        private static readonly char[] ObjectBrushes = { 'P', 'B', 'I', 'H', '.' };

        private int width = 7, height = 9;
        private char[,] ground;
        private char[,] objects;
        private char brush = '#';
        private bool objectBrush;
        private bool autoSolve = true;
        private string levelName = "Yeni Bölüm";
        private string idea = "";
        private CraneData crane; // kept from the loaded file (crane drops are edited in the JSON for now)
        private CraneData[] cranes;
        private string fileName = "";
        private string result = "";
        private Vector2 scroll;

        [MenuItem("Tools/Crate Boss/Level Editor")]
        public static void Open() => GetWindow<LevelEditorWindow>("Bölüm Düzenleyici");

        private void OnEnable()
        {
            if (ground == null) Resize(width, height);
        }

        private void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            DrawFileRow();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            int w = EditorGUILayout.IntSlider("Genişlik", width, 3, 13);
            int h = EditorGUILayout.IntSlider("Yükseklik", height, 3, 15);
            EditorGUILayout.EndHorizontal();
            if (w != width || h != height) { Resize(w, h); Changed(); }

            levelName = EditorGUILayout.TextField("Ad", levelName);
            idea = EditorGUILayout.TextField("Ana fikir", idea);

            DrawBrushes();
            DrawGrid();

            EditorGUILayout.BeginHorizontal();
            autoSolve = EditorGUILayout.ToggleLeft("Her değişiklikte çöz", autoSolve, GUILayout.Width(170));
            if (GUILayout.Button("Çöz", GUILayout.Width(80))) Solve();
            if (GUILayout.Button("Kenarları duvar yap", GUILayout.Width(150))) { FrameWithWalls(); Changed(); }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.HelpBox(string.IsNullOrEmpty(result) ? "Henüz çözülmedi." : result, MessageType.None);
            EditorGUILayout.EndScrollView();
        }

        private void DrawFileRow()
        {
            EditorGUILayout.BeginHorizontal();
            var files = Directory.Exists(LevelsDir)
                ? Directory.GetFiles(LevelsDir, "level_*.json").Select(Path.GetFileNameWithoutExtension).OrderBy(n => n).ToArray()
                : new string[0];
            int current = Array.IndexOf(files, fileName);
            int picked = EditorGUILayout.Popup("Dosya", current, files);
            if (picked != current && picked >= 0) Load(files[picked]);
            if (GUILayout.Button("Yeni", GUILayout.Width(50))) { fileName = ""; levelName = "Yeni Bölüm"; idea = ""; Resize(7, 9); FrameWithWalls(); Changed(); }
            if (GUILayout.Button("Kaydet", GUILayout.Width(60))) Save(files);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawBrushes()
        {
            EditorGUILayout.LabelField("Zemin:  # duvar  . zemin  T hedef  o çukur  _ boşluk     Nesne:  P oyuncu  B kutu  I buz  H ağır  . sil");
            EditorGUILayout.BeginHorizontal();
            foreach (var c in GroundBrushes) BrushButton(c, false);
            GUILayout.Space(20);
            foreach (var c in ObjectBrushes) BrushButton(c, true);
            EditorGUILayout.EndHorizontal();
        }

        private void BrushButton(char c, bool isObject)
        {
            bool on = brush == c && objectBrush == isObject;
            var label = isObject && c == '.' ? "sil" : c.ToString();
            if (GUILayout.Toggle(on, label, "Button", GUILayout.Width(36)) && !on)
            {
                brush = c;
                objectBrush = isObject;
            }
        }

        private void DrawGrid()
        {
            const float cell = 30f;
            var area = GUILayoutUtility.GetRect(width * cell, height * cell, GUILayout.ExpandWidth(false));
            var e = Event.current;
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                var r = new Rect(area.x + x * cell, area.y + y * cell, cell - 2, cell - 2);
                EditorGUI.DrawRect(r, GroundColor(ground[x, y]));
                if (objects[x, y] != '.')
                {
                    var inner = new Rect(r.x + 5, r.y + 5, r.width - 10, r.height - 10);
                    EditorGUI.DrawRect(inner, ObjectColor(objects[x, y]));
                    GUI.Label(inner, objects[x, y].ToString(), EditorStyles.centeredGreyMiniLabel);
                }
                if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && e.button == 0 && r.Contains(e.mousePosition))
                {
                    Paint(x, y);
                    e.Use();
                }
            }
        }

        private void Paint(int x, int y)
        {
            if (objectBrush)
            {
                if (brush == 'P')
                    for (int yy = 0; yy < height; yy++)
                    for (int xx = 0; xx < width; xx++)
                        if (objects[xx, yy] == 'P') objects[xx, yy] = '.';
                objects[x, y] = brush;
            }
            else
            {
                ground[x, y] = brush;
                if (brush != '.' && brush != 'T') objects[x, y] = '.';
            }
            Changed();
        }

        private void Changed()
        {
            result = "";
            if (autoSolve) Solve();
            Repaint();
        }

        private void Solve()
        {
            try
            {
                var state = new LevelParser().Parse(ToData());
                var m = new LevelAnalyzer(LevelSolver.DefaultMaxStates, 150000).Analyze(state);
                var r = m.Solution;
                if (!r.IsSolved)
                {
                    result = r.HitLimit ? $"Çözücü limiti aşıldı ({r.ExploredStates} durum)." : $"ÇÖZÜLEMEZ ({r.ExploredStates} durum incelendi).";
                    return;
                }
                var path = new StringBuilder();
                foreach (var d in r.Moves) path.Append(d.ToString()[0]);
                string trap = m.TrapRatio >= 0 ? m.TrapRatio.ToString("0.00", CultureInfo.InvariantCulture) : "?";
                result = $"Çözüldü: {m.Moves} hamle, {m.Pushes} itme, {m.States} durum, tuzak {trap}, fazla kutu {m.ExtraBoxes}, mekanik {m.MechanicCount} ({m.Mechanics})\n" +
                         $"Puan {m.Score.ToString("0.0", CultureInfo.InvariantCulture)} → {LevelValidator.TierName(m.Tier)}\nYol: {path}";
            }
            catch (LevelFormatException e)
            {
                result = "Hata: " + e.Message;
            }
        }

        private LevelData ToData()
        {
            var g = new string[height];
            var o = new string[height];
            for (int y = 0; y < height; y++)
            {
                var gs = new StringBuilder();
                var os = new StringBuilder();
                for (int x = 0; x < width; x++) { gs.Append(ground[x, y]); os.Append(objects[x, y]); }
                g[y] = gs.ToString();
                o[y] = os.ToString();
            }
            return new LevelData
            {
                id = IdFromFile(fileName),
                name = levelName,
                ground = g,
                objects = o,
                crane = crane,
                cranes = cranes,
                design = new LevelDesign { idea = idea, mechanics = new string[0], tier = "" },
            };
        }

        private void Load(string file)
        {
            var data = new LevelLoader().Load(file);
            fileName = file;
            levelName = data.name;
            idea = data.design?.idea ?? "";
            crane = data.HasOldCrane ? data.crane : null;
            cranes = data.HasCraneList ? data.cranes : null;
            Resize(data.ground[0].Length, data.ground.Length);
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                ground[x, y] = data.ground[y][x];
                objects[x, y] = data.objects[y][x];
            }
            Changed();
        }

        private void Save(string[] existing)
        {
            if (string.IsNullOrEmpty(fileName)) fileName = $"level_{existing.Length + 1:00}";
            var data = ToData();
            // Fill mechanics and tier from the solver so the record matches what the level really needs.
            var m = new LevelAnalyzer().Analyze(new LevelParser().Parse(data), measureTraps: false);
            if (m.IsSolved)
            {
                data.design.tier = LevelValidator.TierId(m.Tier);
                data.design.mechanics = Enum.GetValues(typeof(MechanicFlags)).Cast<MechanicFlags>()
                    .Where(f => f != MechanicFlags.None && m.Mechanics.HasFlag(f))
                    .Select(f => f.ToString().ToLowerInvariant()).ToArray();
            }
            string path = $"{LevelsDir}/{fileName}.json";
            File.WriteAllText(path, LevelJsonWriter.ToJson(data), new UTF8Encoding(false));
            AssetDatabase.ImportAsset(path);
            Debug.Log($"[Level Editor] saved {path}");
        }

        private void Resize(int w, int h)
        {
            var g = new char[w, h];
            var o = new char[w, h];
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                bool keep = ground != null && x < width && y < height;
                g[x, y] = keep ? ground[x, y] : '.';
                o[x, y] = keep ? objects[x, y] : '.';
            }
            width = w;
            height = h;
            ground = g;
            objects = o;
        }

        private void FrameWithWalls()
        {
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                if (x == 0 || y == 0 || x == width - 1 || y == height - 1) { ground[x, y] = '#'; objects[x, y] = '.'; }
        }

        private static int IdFromFile(string file) =>
            file != null && file.StartsWith("level_") && int.TryParse(file.Substring(6), out int n) ? n : 0;

        private static Color GroundColor(char c)
        {
            switch (c)
            {
                case '#': return new Color(0.25f, 0.3f, 0.4f);
                case 'T': return new Color(0.3f, 0.75f, 0.45f);
                case 'o': return new Color(0.08f, 0.08f, 0.1f);
                case '_': return new Color(0.55f, 0.7f, 0.85f);
                default: return new Color(0.92f, 0.88f, 0.8f);
            }
        }

        private static Color ObjectColor(char c)
        {
            switch (c)
            {
                case 'P': return new Color(0.95f, 0.3f, 0.55f);
                case 'I': return new Color(0.3f, 0.85f, 0.9f);
                case 'H': return new Color(0.2f, 0.2f, 0.22f);
                default: return new Color(0.86f, 0.53f, 0.25f);
            }
        }
    }
}
