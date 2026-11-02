using System;
using System.Collections.Generic;

namespace CrateBoss.Localization
{
    /// <summary>
    /// The string table (Ana Belge §18): one row per key, one column per language, in Resources/Localization/strings.txt.
    /// First line: "key" then the language codes. "\n" in a cell is a line break; cells are never trimmed (", " keeps its space).
    /// Lines starting with "#" and empty lines are skipped.
    /// </summary>
    public sealed class LocTable
    {
        public IReadOnlyList<string> Languages { get; }
        private readonly Dictionary<string, string[]> rows;

        private LocTable(IReadOnlyList<string> languages, Dictionary<string, string[]> rows)
        {
            Languages = languages;
            this.rows = rows;
        }

        public IEnumerable<string> Keys => rows.Keys;

        public static LocTable Parse(string tsv)
        {
            var lines = (tsv ?? "").Replace("\r\n", "\n").Split('\n');
            string[] header = null;
            var rows = new Dictionary<string, string[]>();
            foreach (var line in lines)
            {
                if (line.Length == 0 || line[0] == '#') continue;
                var cells = line.Split('\t');
                if (header == null) { header = cells; continue; }
                var values = new string[header.Length - 1];
                for (int i = 1; i < header.Length; i++) values[i - 1] = i < cells.Length ? cells[i].Replace("\\n", "\n") : "";
                rows[cells[0]] = values;
            }
            var languages = header == null ? new string[0] : new string[header.Length - 1];
            if (header != null) Array.Copy(header, 1, languages, 0, languages.Length);
            return new LocTable(languages, rows);
        }

        /// <summary>The cell, or null when the key or the language is missing or the cell is empty.</summary>
        public string Get(string key, string language)
        {
            if (key == null || !rows.TryGetValue(key, out var values)) return null;
            for (int i = 0; i < Languages.Count; i++)
                if (Languages[i] == language) return string.IsNullOrEmpty(values[i]) ? null : values[i];
            return null;
        }

        public bool Has(string key) => key != null && rows.ContainsKey(key);
    }
}
