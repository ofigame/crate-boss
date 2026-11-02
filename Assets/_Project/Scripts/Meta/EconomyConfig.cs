using UnityEngine;

namespace CrateBoss.Meta
{
    /// <summary>
    /// Every CB Coin value in one place (Ana Belge §13), first draft, so balancing later is one file.
    /// The game loads Resources/EconomyConfig; without the asset the defaults below are used.
    /// </summary>
    [CreateAssetMenu(menuName = "Crate Boss/Economy Config", fileName = "EconomyConfig")]
    public class EconomyConfig : ScriptableObject
    {
        [Header("Earning (§13.1)")]
        /// <summary>A level's first clear by stars (★, ★★, ★★★); replaying pays only the difference to a better result.</summary>
        public int[] starCoins = { 10, 18, 30 };
        public int[] loginStreak = { 10, 15, 20, 25, 30, 40, 60 };
        public int adCoins = 30;
        public int adCoinsPerDay = 3;
        public int crateAdsPerDay = 5;
        public int match4 = 5, match5 = 10;

        [Header("Spending (§13.2)")]
        public int hint = 50;
        public int watchSolution = 300;
        public int crateOpen = 30;
        public int recolor = 40;
        /// <summary>Wardrobe prices (Ana Belge §13.2: 100–500).</summary>
        public int wardrobeMin = 100, wardrobeMax = 500;

        [Header("Hints (§14)")]
        public int startHints = 3;
        public int dailyHintGift = 1;
        public int maxStoredHints = 3;

        private static EconomyConfig loaded;

        /// <summary>The project asset, or an in-memory copy with the default values.</summary>
        public static EconomyConfig Current
        {
            get
            {
                if (loaded != null) return loaded;
                loaded = Resources.Load<EconomyConfig>("EconomyConfig");
                if (loaded == null) loaded = CreateInstance<EconomyConfig>();
                return loaded;
            }
        }
    }
}
