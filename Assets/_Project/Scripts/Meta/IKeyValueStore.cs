namespace CrateBoss.Meta
{
    /// <summary>Where the save lives (PlayerPrefs in the game, memory in tests).</summary>
    public interface IKeyValueStore
    {
        string Get(string key);
        void Set(string key, string value);
        void Flush();
    }
}
