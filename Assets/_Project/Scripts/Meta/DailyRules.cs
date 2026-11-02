using System;

namespace CrateBoss.Meta
{
    /// <summary>
    /// Day-based gifts (Ana Belge §13.1): the login streak (day 1→7, broken streak starts again at day 1), the daily
    /// free hint (+1, at most 3 stored) and the "+30 CB Coin" ad limit. Dates are local calendar days.
    /// </summary>
    public static class DailyRules
    {
        public static string Day(DateTime t) => t.ToString("yyyyMMdd");

        /// <summary>First visit of the day: advances the streak and returns its CB Coin (0 when already claimed today).</summary>
        public static int ClaimLogin(SaveData d, EconomyConfig cfg, DateTime now)
        {
            string today = Day(now);
            if (d.lastLoginDay == today) return 0;
            bool continues = d.lastLoginDay == Day(now.AddDays(-1));
            d.streakDay = continues ? d.streakDay % cfg.loginStreak.Length + 1 : 1;
            d.lastLoginDay = today;
            return cfg.loginStreak[d.streakDay - 1];
        }

        /// <summary>New players get the start hints; every new day adds one, never above the cap.</summary>
        public static void GiftHints(SaveData d, EconomyConfig cfg, DateTime now)
        {
            string today = Day(now);
            if (d.hints < 0)
            {
                d.hints = cfg.startHints;
                d.hintGiftDay = today;
                return;
            }
            if (d.hintGiftDay == today) return;
            d.hintGiftDay = today;
            d.hints = Math.Min(cfg.maxStoredHints, d.hints + cfg.dailyHintGift);
        }

        /// <summary>How many "+30 CB Coin" ads are left today.</summary>
        public static int AdCoinsLeft(SaveData d, EconomyConfig cfg, DateTime now) =>
            d.adCoinDay == Day(now) ? Math.Max(0, cfg.adCoinsPerDay - d.adCoinCount) : cfg.adCoinsPerDay;

        public static void UseAdCoins(SaveData d, DateTime now)
        {
            string today = Day(now);
            if (d.adCoinDay != today) { d.adCoinDay = today; d.adCoinCount = 0; }
            d.adCoinCount++;
        }

        /// <summary>The login gift's second box (Ana Belge §13.1): once a day, through a rewarded ad.</summary>
        public static bool GiftAdAvailable(SaveData d, DateTime now) => d.lastLoginDay == Day(now) && d.giftAdDay != Day(now);

        public static void UseGiftAd(SaveData d, DateTime now) => d.giftAdDay = Day(now);
    }
}
