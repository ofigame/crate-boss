using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using CrateBoss.Core;
using CrateBoss.Levels;
using CrateBoss.Solver;
using UnityEditor;
using Debug = UnityEngine.Debug;
using Random = System.Random;

namespace CrateBoss.EditorTools
{
    /// <summary>
    /// Tools > Crate Boss > Search Hard Level: keeps a hand-made room fixed, places the boxes and the player
    /// at random many times, solves each try and keeps the one with the longest shortest solution
    /// that really uses ice, heavy box and hole. Writes it as the next level file.
    /// </summary>
    public static class HardLevelSearch
    {
        private const string LevelsDir = "Assets/_Project/Resources/Levels";
        private const int SearchStateLimit = 150000;

        // Rooms only: '#', '.', 'T', 'o', '_'. The boxes listed after each room get random places.
        private static readonly string[][] Rooms =
        {
            new[]
            {
                "##########",
                "#...#....#",
                "#.T....o.#",
                "#..##.#..#",
                "#.T...#..#",
                "##..#...T#",
                "#_.....#.#",
                "##########",
            },
            new[]
            {
                "#########",
                "#..#....#",
                "#.T..#..#",
                "#..o...T#",
                "##.#.#..#",
                "#.T.....#",
                "#..#._..#",
                "#########",
            },
        };

        private static readonly BoxType[] Boxes = { BoxType.Normal, BoxType.Normal, BoxType.Ice, BoxType.Heavy };

        [MenuItem("Tools/Crate Boss/Search Hard Level")]
        public static void SearchMenu() => Search(120, 9, "Usta Sınavı");

        public static void SearchBatch()
        {
            try { EditorApplication.Exit(Search(300, 9, "Usta Sınavı") ? 0 : 1); }
            catch (Exception e) { Debug.LogException(e); EditorApplication.Exit(1); }
        }

        public static bool Search(int seconds, int id, string name)
        {
            var rng = new Random(Environment.TickCount);
            var solver = new LevelSolver(SearchStateLimit);
            var parser = new LevelParser();
            var watch = Stopwatch.StartNew();
            LevelData best = null;
            int bestMoves = 0, tries = 0, solved = 0;

            while (watch.Elapsed.TotalSeconds < seconds)
            {
                var room = Rooms[tries % Rooms.Length];
                var data = RandomLayout(room, rng, id, name);
                tries++;
                var start = parser.Parse(data);
                if (start.IsSolved()) continue;
                var result = solver.Solve(start);
                if (!result.IsSolved) continue;
                solved++;
                if (result.MoveCount <= bestMoves || !UsesEverything(start, result)) continue;
                bestMoves = result.MoveCount;
                best = data;
                Debug.Log($"[Search] new best {bestMoves} moves after {tries} tries ({result.ExploredStates} states)");
            }

            Debug.Log($"[Search] {tries} tries, {solved} solvable, best {bestMoves} moves");
            if (best == null) return false;
            string path = $"{LevelsDir}/level_{id:00}.json";
            File.WriteAllText(path, LevelJsonWriter.ToJson(best), new UTF8Encoding(false));
            AssetDatabase.ImportAsset(path);
            Debug.Log($"[Search] wrote {path}\n{LevelJsonWriter.ToJson(best)}");
            return true;
        }

        private static LevelData RandomLayout(string[] room, Random rng, int id, string name)
        {
            var free = new List<(int x, int y)>();
            for (int y = 0; y < room.Length; y++)
            for (int x = 0; x < room[y].Length; x++)
                if (room[y][x] == '.' && !IsCorner(room, x, y)) free.Add((x, y));

            var objects = new char[room.Length][];
            for (int y = 0; y < room.Length; y++) objects[y] = new string('.', room[y].Length).ToCharArray();

            var picks = new List<char> { 'P' };
            foreach (var b in Boxes) picks.Add(b == BoxType.Ice ? 'I' : b == BoxType.Heavy ? 'H' : 'B');
            foreach (var c in picks)
            {
                int i = rng.Next(free.Count);
                var (x, y) = free[i];
                free.RemoveAt(i);
                objects[y][x] = c;
            }

            var rows = new string[room.Length];
            for (int y = 0; y < room.Length; y++) rows[y] = new string(objects[y]);
            return new LevelData { id = id, name = name, ground = room, objects = rows };
        }

        // A box in a wall corner can never move again; skip those cells.
        private static bool IsCorner(string[] room, int x, int y)
        {
            bool Wall(int cx, int cy) => room[cy][cx] == '#';
            bool vertical = Wall(x, y - 1) || Wall(x, y + 1);
            bool horizontal = Wall(x - 1, y) || Wall(x + 1, y);
            return vertical && horizontal;
        }

        /// <summary>The solution must slide the ice box, move the heavy box and fill a hole.</summary>
        private static bool UsesEverything(GameState start, SolveResult result)
        {
            var session = new GameSession(start.Clone());
            bool ice = false, heavy = false, hole = false;
            foreach (var dir in result.Moves)
            {
                foreach (var move in session.Move(dir).BoxMoves)
                {
                    ice |= move.BoxType == BoxType.Ice;
                    heavy |= move.BoxType == BoxType.Heavy;
                    hole |= move.FellInto == FallType.Hole;
                }
            }
            return ice && heavy && hole;
        }
    }
}
