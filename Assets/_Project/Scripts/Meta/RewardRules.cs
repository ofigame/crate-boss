namespace CrateBoss.Meta
{
    /// <summary>
    /// Level rewards (Ana Belge §13.1): the first clear pays by stars (★ 10, ★★ 18, ★★★ 30 CB Coin); replaying pays only the
    /// difference when the result is better (★ → ★★★: +20). A first clear also brings a delivery crate (§12.1).
    /// </summary>
    public static class RewardRules
    {
        public static int ValueOf(EconomyConfig cfg, int stars) => stars <= 0 ? 0 : cfg.starCoins[System.Math.Min(stars, cfg.starCoins.Length) - 1];

        /// <summary>Works out the reward and updates the record.</summary>
        public static RewardBreakdown Win(EconomyConfig cfg, LevelRecord record, int stars)
        {
            bool first = !record.cleared;
            int before = record.cleared ? ValueOf(cfg, record.stars) : 0;
            int now = ValueOf(cfg, stars);
            int pay = System.Math.Max(0, now - before);
            record.cleared = true;
            if (stars > record.stars) record.stars = stars;
            return new RewardBreakdown(first ? pay : 0, first ? 0 : pay, first);
        }
    }
}
