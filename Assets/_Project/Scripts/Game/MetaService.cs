using System;
using System.Collections.Generic;
using CrateBoss.Levels;
using CrateBoss.Meta;

namespace CrateBoss.Game
{
    /// <summary>
    /// The player's meta progress in one place (Ana Belge §7): the save, CB Coin, which levels are done and the rank
    /// gates. The screenshot tour plays on a save in memory, so it never touches the real one.
    /// </summary>
    public sealed class MetaService
    {
        public SaveData Data { get; }
        public Wallet Wallet { get; }
        public EconomyConfig Config { get; }
        /// <summary>The ranks (Resources/Data/Ranks.txt) and the Boss's items (Resources/Data/Items.txt).</summary>
        public RankCatalog Ranks { get; }
        public ItemCatalog Items { get; }

        private readonly SaveStore store;
        private readonly IReadOnlyList<LevelData> levels;

        public MetaService(IReadOnlyList<LevelData> levels, bool inMemory)
        {
            this.levels = levels;
            Config = EconomyConfig.Current;
            store = new SaveStore(inMemory ? (IKeyValueStore)new MemoryStore() : new PlayerPrefsStore());
            Data = store.Load();
            Wallet = new Wallet(Data);
            Ranks = new RankCatalog(UnityEngine.Resources.Load<UnityEngine.TextAsset>("Data/Ranks")?.text);
            Items = new ItemCatalog(UnityEngine.Resources.Load<UnityEngine.TextAsset>("Data/Items")?.text);
            DailyRules.GiftHints(Data, Config, DateTime.Now);
        }

        public void Save() => store.Save(Data);

        public static string Key(LevelData level) => level.id.ToString();

        public bool Cleared(int levelId)
        {
            foreach (var r in Data.levels) if (r.key == levelId.ToString()) return r.cleared;
            return false;
        }

        public bool Cleared(LevelData level) => Cleared(level.id);

        public int Stars(LevelData level)
        {
            foreach (var r in Data.levels) if (r.key == Key(level)) return r.stars;
            return 0;
        }

        /// <summary>
        /// Levels open to play: everything up to the one after the furthest cleared level (owner 2026-11-02: a level skipped
        /// or left from an older set must not lock the ones after it, "Sonraki" sent the player home).
        /// </summary>
        public int Unlocked
        {
            get
            {
                bool leagues = LeagueRules.IsLeagueSet(levels);
                int open = 1;
                for (int i = 0; i < levels.Count; i++)
                {
                    // A new league opens only with 60% of the stars of the one before (owner 2026-10-31).
                    if (leagues && i > 0 && LeagueRules.LeagueOf(levels, i) != LeagueRules.LeagueOf(levels, i - 1))
                    {
                        int league = LeagueRules.LeagueOf(levels, i);
                        if (LeagueRules.StarsIn(levels, league - 1, Stars) < LeagueRules.StarsNeeded(levels, league)) return Math.Min(open, i);
                    }
                    if (Cleared(levels[i])) open = Math.Max(open, i + 2);
                }
                return Math.Min(open, levels.Count);
            }
        }

        /// <summary>Main levels cleared so far (the ad rules' "first levels free").</summary>
        public int ClearedCount
        {
            get
            {
                int n = 0;
                foreach (var l in levels) if (Cleared(l)) n++;
                return n;
            }
        }

        /// <summary>Index of the level OYNA starts: the first one not cleared (or the last).</summary>
        public int NextLevel => Math.Min(Unlocked, levels.Count) - 1;

        // ---------- ranks (Ana Belge §7) ----------

        private int Leagues => LeagueRules.IsLeagueSet(levels) ? LeagueRules.LeagueOf(levels, levels.Count - 1) : 1;

        private bool LastCleared(int league)
        {
            for (int i = levels.Count - 1; i >= 0; i--)
                if (LeagueRules.LeagueOf(levels, i) == league) return Cleared(levels[i]);
            return false;
        }

        /// <summary>The player's rank now: the next one opens when a rank's last level is cleared with 60 % of its stars.</summary>
        public int Rank => !LeagueRules.IsLeagueSet(levels) ? 1 :
            RankCatalog.Current(Leagues, LastCleared, l => LeagueRules.StarsIn(levels, l, Stars), l => LeagueRules.StarsNeeded(levels, l));

        /// <summary>Share of the current rank's levels cleared (the bar on the home screen, no numbers).</summary>
        public float RankProgress
        {
            get
            {
                if (!LeagueRules.IsLeagueSet(levels)) return 0f;
                int rank = Math.Min(Rank, Leagues), all = 0, done = 0;
                for (int i = 0; i < levels.Count; i++)
                {
                    if (LeagueRules.LeagueOf(levels, i) != rank) continue;
                    all++;
                    if (Cleared(levels[i])) done++;
                }
                return all == 0 ? 0f : (float)done / all;
            }
        }

        /// <summary>After a win: a higher rank than saved is a promotion (a promotion crate, new lands). Returns the new rank, or 0.</summary>
        public int CheckPromotion()
        {
            int rank = Rank;
            if (rank <= Data.rank) return 0;
            for (int r = Data.rank + 1; r <= rank; r++) Data.promoCrates.Add(Math.Min(r, 5));
            Data.rank = rank;
            HomeRules.OpenLands(Data, rank, true);
            return rank;
        }
    }
}
