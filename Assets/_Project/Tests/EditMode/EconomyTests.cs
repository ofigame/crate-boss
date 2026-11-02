using System;
using NUnit.Framework;
using CrateBoss.Meta;
using UnityEngine;

namespace CrateBoss.Tests
{
    /// <summary>CB Coin, rewards, save, daily gifts (Ana Belge §13 "Ekonomi").</summary>
    public class EconomyTests
    {
        private static EconomyConfig Config() => ScriptableObject.CreateInstance<EconomyConfig>();

        [Test]
        public void Wallet_AddsAndSpends_AndRefusesWhenShort()
        {
            var data = new SaveData();
            var w = new Wallet(data);
            w.Add(120);
            Assert.IsTrue(w.TrySpend(50));
            Assert.AreEqual(70, w.Balance);
            Assert.IsFalse(w.TrySpend(100), "not enough CB Coin");
            Assert.AreEqual(70, w.Balance, "a refused purchase changes nothing");
        }

        [Test]
        public void Save_ThenLoad_GivesTheSameData_AndABrokenSaveFallsBackToTheBackup()
        {
            var store = new MemoryStore();
            var saves = new SaveStore(store);
            var data = new SaveData { coins = 345 };
            data.owned.Add("racks");
            data.Level("12").stars = 3;
            saves.Save(data);
            var again = saves.Load();
            Assert.AreEqual(JsonUtility.ToJson(data), JsonUtility.ToJson(again));

            data.coins = 400;
            saves.Save(data);                    // the 345 save becomes the backup
            store.Set(SaveStore.MainKey, "{broken");
            var recovered = saves.Load();
            Assert.IsTrue(saves.Recovered);
            Assert.AreEqual(345, recovered.coins);
            store.Set(SaveStore.BackupKey, "also broken");
            Assert.AreEqual(0, saves.Load().coins, "nothing usable: a fresh game, never a crash");
        }

        [Test]
        public void Rewards_PayOnce_ReplayOnlyTheDifference()
        {
            var cfg = Config();
            var rec = new LevelRecord { key = "5" };
            var first = RewardRules.Win(cfg, rec, 2);
            Assert.AreEqual(18, first.Total, "two stars on the first clear: 18 (Ana Belge §13.1)");
            Assert.IsTrue(first.FirstClear);
            var again = RewardRules.Win(cfg, rec, 2);
            Assert.AreEqual(0, again.Total, "replaying the same result pays nothing");
            var better = RewardRules.Win(cfg, rec, 3);
            Assert.AreEqual(12, better.Better, "two stars to three: 30 - 18");
            Assert.AreEqual(0, better.Clear);
            Assert.AreEqual(0, RewardRules.Win(cfg, rec, 3).Total);
        }

        [Test]
        public void LoginStreak_CountsUp_AndRestartsWhenBroken()
        {
            var cfg = Config();
            var d = new SaveData();
            var day = new DateTime(2026, 10, 1, 9, 0, 0);
            Assert.AreEqual(10, DailyRules.ClaimLogin(d, cfg, day));
            Assert.AreEqual(0, DailyRules.ClaimLogin(d, cfg, day.AddHours(5)), "once a day");
            Assert.AreEqual(15, DailyRules.ClaimLogin(d, cfg, day.AddDays(1)));
            Assert.AreEqual(20, DailyRules.ClaimLogin(d, cfg, day.AddDays(2)));
            Assert.AreEqual(10, DailyRules.ClaimLogin(d, cfg, day.AddDays(4)), "a missed day starts again at day 1");
            for (int i = 5; i <= 10; i++) DailyRules.ClaimLogin(d, cfg, day.AddDays(i));
            Assert.AreEqual(7, d.streakDay);
            Assert.AreEqual(10, DailyRules.ClaimLogin(d, cfg, day.AddDays(11)), "after day 7 the week starts over");
        }

        [Test]
        public void Hints_ThreeAtStart_PlusOneADay_AtMostThree()
        {
            var cfg = Config();
            var d = new SaveData();
            var day = new DateTime(2026, 10, 1);
            DailyRules.GiftHints(d, cfg, day);
            Assert.AreEqual(3, d.hints);
            d.hints = 0;
            DailyRules.GiftHints(d, cfg, day);
            Assert.AreEqual(0, d.hints, "no second gift the same day");
            DailyRules.GiftHints(d, cfg, day.AddDays(1));
            Assert.AreEqual(1, d.hints);
            d.hints = 3;
            DailyRules.GiftHints(d, cfg, day.AddDays(2));
            Assert.AreEqual(3, d.hints, "never above three");
        }

        [Test]
        public void AdCoins_ThreeADay()
        {
            var cfg = Config();
            var d = new SaveData();
            var day = new DateTime(2026, 10, 1);
            for (int i = 0; i < 3; i++) DailyRules.UseAdCoins(d, day);
            Assert.AreEqual(0, DailyRules.AdCoinsLeft(d, cfg, day));
            Assert.AreEqual(3, DailyRules.AdCoinsLeft(d, cfg, day.AddDays(1)));
        }

        [Test]
        public void OldSave_PrimBecomesCoins()
        {
            var store = new MemoryStore();
            store.Set(SaveStore.MainKey, "{\"version\":1,\"prim\":120}");
            var d = new SaveStore(store).Load();
            Assert.AreEqual(120, d.coins);
            Assert.AreEqual(0, d.prim);
        }

    }
}
