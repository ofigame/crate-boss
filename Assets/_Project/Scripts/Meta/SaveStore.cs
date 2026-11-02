using System;
using UnityEngine;

namespace CrateBoss.Meta
{
    /// <summary>
    /// Loads and saves <see cref="SaveData"/> (Ana Belge §12.6). Each save keeps the previous good copy as a backup; a broken
    /// main save falls back to the backup, and a broken backup to a fresh game. The game never crashes on a bad save.
    /// </summary>
    public sealed class SaveStore
    {
        public const string MainKey = "crateboss.save";
        public const string BackupKey = "crateboss.save.bak";

        private readonly IKeyValueStore store;

        public SaveStore(IKeyValueStore store) => this.store = store;

        /// <summary>True when the last Load had to use the backup or start fresh.</summary>
        public bool Recovered { get; private set; }

        public SaveData Load()
        {
            Recovered = false;
            var data = TryParse(store.Get(MainKey));
            if (data == null)
            {
                Recovered = store.Get(MainKey) != null;
                data = TryParse(store.Get(BackupKey)) ?? new SaveData();
            }
            Migrate(data);
            return data;
        }

        public void Save(SaveData data)
        {
            var current = store.Get(MainKey);
            if (TryParse(current) != null) store.Set(BackupKey, current);
            store.Set(MainKey, JsonUtility.ToJson(data));
            store.Flush();
        }

        /// <summary>Version 1 kept the balance as Prim (removed, Ana Belge §20); it becomes CB Coin.</summary>
        private static void Migrate(SaveData data)
        {
            if (data.prim > 0) { data.coins += data.prim; data.prim = 0; }
            if (data.version < 2) data.version = 2;
        }

        private static SaveData TryParse(string json)
        {
            if (string.IsNullOrEmpty(json)) return null;
            try
            {
                var d = JsonUtility.FromJson<SaveData>(json);
                return d != null && d.version >= 1 ? d : null;
            }
            catch (Exception) { return null; }
        }
    }
}
