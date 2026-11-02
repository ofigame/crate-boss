using System;
using System.Collections.Generic;

namespace CrateBoss.Meta
{
    /// <summary>
    /// Everything kept on the device (Ana Belge §12.6): CB Coin, owned items, level records, hints, login streak and ad
    /// counters. Written as JSON with a backup copy.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public int version = 2;
        /// <summary>CB Coin balance (Ana Belge §13).</summary>
        public int coins;
        /// <summary>Old saves (version 1) kept the balance here as Prim; moved to <see cref="coins"/> on load.</summary>
        public int prim;
        /// <summary>Ids of owned items (Boss items, wardrobe pieces).</summary>
        public List<string> owned = new List<string>();
        /// <summary>Item ids per slot the player wears / uses now ("worker.hat" → "hat_red").</summary>
        public List<string> equipped = new List<string>();
        /// <summary>Items the player has already seen (the rest show "YENİ").</summary>
        public List<string> seen = new List<string>();
        public List<LevelRecord> levels = new List<LevelRecord>();
        public int hints = -1;          // -1 = not given yet (a new player gets the start hints)
        public string hintGiftDay = "";
        public int streakDay;           // 1..7, 0 = none yet
        public string lastLoginDay = "";
        public string adCoinDay = "";   // the home screen's "+30 CB Coin" ad (Ana Belge §15.1)
        public int adCoinCount;
        public string giftAdDay = "";   // the login gift's second box (rewarded ad) opened on this day
        public bool firstCoinCardShown;
        // ---------- the Boss's land (Ana Belge §11, §12) ----------
        /// <summary>Delivery crates waiting on the pile (never lost).</summary>
        public int crates;
        /// <summary>Promotion crates waiting (their rank); opened free.</summary>
        public List<int> promoCrates = new List<int>();
        public int cratesOpened;
        public bool firstCrateOpened;
        /// <summary>Items chosen but not placed yet.</summary>
        public List<string> inventory = new List<string>();
        public List<PlacedItem> placed = new List<PlacedItem>();
        /// <summary>Path tiles, "land:x:y".</summary>
        public List<string> paths = new List<string>();
        public List<string> lands = new List<string>();
        /// <summary>Crates opened through an ad today (Ana Belge §15.1: 5 a day).</summary>
        public string crateAdDay = "";
        public int crateAdCount;

        /// <summary>Highest rank already celebrated (Ana Belge §7, §11.4); 1 = Öğrenci.</summary>
        public int rank = 1;

        public LevelRecord Level(string key)
        {
            foreach (var r in levels) if (r.key == key) return r;
            var rec = new LevelRecord { key = key };
            levels.Add(rec);
            return rec;
        }

        public bool Owns(string id) => owned.Contains(id);
    }
}
