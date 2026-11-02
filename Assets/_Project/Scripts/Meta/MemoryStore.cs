using System.Collections.Generic;

namespace CrateBoss.Meta
{
    /// <summary>A key-value store in memory (tests, and the editor without PlayerPrefs).</summary>
    public sealed class MemoryStore : IKeyValueStore
    {
        public readonly Dictionary<string, string> Values = new Dictionary<string, string>();

        public string Get(string key) => Values.TryGetValue(key, out var v) ? v : null;
        public void Set(string key, string value) => Values[key] = value;
        public void Flush() { }
    }
}
