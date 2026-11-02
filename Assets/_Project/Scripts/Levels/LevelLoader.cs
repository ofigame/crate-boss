using System;
using System.Collections.Generic;
using CrateBoss.Core;
using UnityEngine;

namespace CrateBoss.Levels
{
    /// <summary>Reads Resources/Levels/*.json (or another Resources folder, sorted by file name) and builds game states.</summary>
    public sealed class LevelLoader
    {
        public const string ResourceFolder = "Levels";
        /// <summary>The levels the game plays (owner 2026-10-31: 5 leagues of 25 hard puzzles); "Levels" stays for the tools.</summary>
        public const string GameFolder = "LeagueLevels";

        private readonly LevelParser parser = new LevelParser();
        private readonly string folder;

        /// <param name="resourceFolder">Folder under Resources; "DemoLevels" holds test boards that are not in the game.</param>
        public LevelLoader(string resourceFolder = ResourceFolder) => folder = resourceFolder;

        public List<LevelData> LoadAll()
        {
            var assets = Resources.LoadAll<TextAsset>(folder);
            Array.Sort(assets, (a, b) => string.CompareOrdinal(a.name, b.name));
            var levels = new List<LevelData>(assets.Length);
            foreach (var asset in assets) levels.Add(Parse(asset));
            return levels;
        }

        public LevelData Load(string fileName)
        {
            var asset = Resources.Load<TextAsset>($"{folder}/{fileName}");
            if (asset == null) throw new LevelFormatException($"Level file '{fileName}' not found in Resources/{folder}.");
            return Parse(asset);
        }

        /// <summary>Validates the layers; throws LevelFormatException with a readable message.</summary>
        public GameState BuildState(LevelData data) => parser.Parse(data);

        /// <summary>A level from its JSON text (editor tools read candidate files straight from disk).</summary>
        public static LevelData FromJson(string json)
        {
            var data = JsonUtility.FromJson<LevelData>(json);
            if (data == null) throw new LevelFormatException("empty level file");
            ReadExtras(json, data);
            return data;
        }

        private static LevelData Parse(TextAsset asset)
        {
            LevelData data;
            try { data = JsonUtility.FromJson<LevelData>(asset.text); }
            catch (Exception e) { throw new LevelFormatException($"{asset.name}.json is not valid JSON: {e.Message}"); }
            if (data == null) throw new LevelFormatException($"{asset.name}.json is empty.");
            try { ReadExtras(asset.text, data); }
            catch (LevelFormatException e) { throw new LevelFormatException($"{asset.name}.json: {e.Message}"); }
            return data;
        }

        /// <summary>
        /// The parts JsonUtility cannot read (nested arrays, a name-or-list, nulls): goal, trucks, deliveries, parking.
        /// Only parsed again when the text has one of them.
        /// </summary>
        public static void ReadExtras(string json, LevelData data)
        {
            if (!json.Contains("\"goal\"") && !json.Contains("\"trucks\"") && !json.Contains("\"deliveries\"") && !json.Contains("\"parking\"") && !json.Contains("\"candy\"") && !json.Contains("\"conveyor\"")) return;
            if (!(JsonLite.Parse(json) is Dictionary<string, object> root)) return;
            if (root.TryGetValue("goal", out var goal) && goal != null) data.goal = JsonCells.Names(goal);
            if (root.TryGetValue("trucks", out var trucks) && trucks != null) data.trucks = TruckData.FromJson(trucks);
            if (root.TryGetValue("parking", out var parking) && parking != null) data.parking = ParkingData.FromJson(parking);
            if (root.TryGetValue("candy", out var candy) && candy != null) data.candy = CandyData.FromJson(candy);
            if (root.TryGetValue("conveyor", out var belt) && belt != null) data.conveyor = JsonCells.List(belt, "conveyor");
            if (root.TryGetValue("deliveries", out var deliveries) && deliveries is List<object> list)
            {
                data.deliveries = new DeliveryData[list.Count];
                for (int i = 0; i < list.Count; i++) data.deliveries[i] = DeliveryData.FromJson(list[i], i);
            }
        }
    }
}
