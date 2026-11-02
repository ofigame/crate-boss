using CrateBoss.Meta;
using UnityEngine;

namespace CrateBoss.Game
{
    /// <summary>The device save (PlayerPrefs) behind <see cref="SaveStore"/>.</summary>
    public sealed class PlayerPrefsStore : IKeyValueStore
    {
        public string Get(string key) => PlayerPrefs.HasKey(key) ? PlayerPrefs.GetString(key) : null;
        public void Set(string key, string value) => PlayerPrefs.SetString(key, value);
        public void Flush() => PlayerPrefs.Save();
    }
}
