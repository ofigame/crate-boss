using System;
using System.IO;
using System.Text.RegularExpressions;
using CrateBoss.Ads;
using CrateBoss.Meta;
using NUnit.Framework;
using UnityEngine;

namespace CrateBoss.Tests
{
    /// <summary>Ad rules, rewarded ads and the hidden level number (Ana Belge §15, §U, §10 "Ekonomi ve reklam").</summary>
    public class AdPolicyTests
    {
        private static AdConfig Config() => ScriptableObject.CreateInstance<AdConfig>();

        // A session that already won three levels, long after any earlier ad.
        private static AdSessionState Ready()
        {
            var s = new AdSessionState();
            for (int i = 0; i < 3; i++) AdPolicy.LevelFinished(s);
            return s;
        }

        [Test]
        public void FirstEightLevels_NeverShowAnInterstitial()
        {
            var cfg = Config();
            var s = Ready();
            for (int cleared = 1; cleared <= 8; cleared++)
                Assert.IsFalse(AdPolicy.CanShowInterstitial(cfg, s, cleared, 10000), $"cleared {cleared}");
            Assert.IsTrue(AdPolicy.CanShowInterstitial(cfg, s, 9, 10000));
        }

        [Test]
        public void Interstitial_NeedsThreeLevelsAndTwoMinutesBetween()
        {
            var cfg = Config();
            var s = Ready();
            AdPolicy.InterstitialShown(s, 1000);
            AdPolicy.LevelFinished(s);
            AdPolicy.LevelFinished(s);
            Assert.IsFalse(AdPolicy.CanShowInterstitial(cfg, s, 20, 2000), "only two levels since the last ad");
            AdPolicy.LevelFinished(s);
            Assert.IsFalse(AdPolicy.CanShowInterstitial(cfg, s, 20, 1100), "less than 120 s since the last ad");
            Assert.IsTrue(AdPolicy.CanShowInterstitial(cfg, s, 20, 1121));
        }

        [Test]
        public void Interstitial_AtMostSixPerSession()
        {
            var cfg = Config();
            var s = new AdSessionState();
            double t = 0;
            for (int i = 0; i < 6; i++)
            {
                for (int k = 0; k < 3; k++) AdPolicy.LevelFinished(s);
                t += 200;
                Assert.IsTrue(AdPolicy.CanShowInterstitial(cfg, s, 30, t), $"ad {i + 1}");
                AdPolicy.InterstitialShown(s, t);
            }
            for (int k = 0; k < 3; k++) AdPolicy.LevelFinished(s);
            Assert.IsFalse(AdPolicy.CanShowInterstitial(cfg, s, 30, t + 1000), "seventh ad in one session");
        }

        [Test]
        public void Interstitial_QuietForFiveMinutesAfterARewardedAd_AndNeverAfterTheDailyPuzzle()
        {
            var cfg = Config();
            var s = Ready();
            AdPolicy.RewardedCompleted(s, 500);
            Assert.IsFalse(AdPolicy.CanShowInterstitial(cfg, s, 20, 700));
            Assert.IsTrue(AdPolicy.CanShowInterstitial(cfg, s, 20, 801));
        }

        [Test]
        public void Interstitial_MidLevel_NeverCountsAsAWin()
        {
            // Only a won yard moves the counter: restarts and moves in a level do not open the way to an ad.
            var s = new AdSessionState();
            Assert.IsFalse(AdPolicy.CanShowInterstitial(Config(), s, 30, 10000));
        }

        private sealed class FakeRewarded : IRewardedAdService
        {
            public bool Ready = true, Complete;
            public bool IsReady => Ready;
            public void Load() { }
            public void Show(Action<bool> onFinished) => onFinished(Complete);
        }

        [Test]
        public void RewardedAd_GivesTheRewardOnlyWhenWatchedToTheEnd()
        {
            var ad = new FakeRewarded { Complete = false };
            var s = new AdSessionState();
            int rewards = 0, skipped = 0, missing = 0;
            RewardedAds.Show(ad, AdPlacement.HomeCoins, s, () => 50, () => rewards++, () => missing++, () => skipped++);
            Assert.AreEqual(0, rewards, "closed early");
            Assert.AreEqual(1, skipped);
            Assert.IsTrue(double.IsNegativeInfinity(s.LastRewardedAt));

            ad.Complete = true;
            RewardedAds.Show(ad, AdPlacement.HomeCoins, s, () => 60, () => rewards++, () => missing++);
            Assert.AreEqual(1, rewards);
            Assert.AreEqual(60, s.LastRewardedAt);

            ad.Ready = false;
            RewardedAds.Show(ad, AdPlacement.HomeCoins, s, () => 70, () => rewards++, () => missing++);
            Assert.AreEqual(1, rewards, "no ad loaded: nothing given");
            Assert.AreEqual(1, missing);
        }

        [Test]
        public void GiftAd_OncePerDay_AfterTheLoginGift()
        {
            var cfg = ScriptableObject.CreateInstance<EconomyConfig>();
            var d = new SaveData();
            var day = new DateTime(2026, 11, 2, 9, 0, 0);
            Assert.IsFalse(DailyRules.GiftAdAvailable(d, day), "no login gift yet");
            DailyRules.ClaimLogin(d, cfg, day);
            Assert.IsTrue(DailyRules.GiftAdAvailable(d, day));
            DailyRules.UseGiftAd(d, day);
            Assert.IsFalse(DailyRules.GiftAdAvailable(d, day.AddHours(5)));
            DailyRules.ClaimLogin(d, cfg, day.AddDays(1));
            Assert.IsTrue(DailyRules.GiftAdAvailable(d, day.AddDays(1)));
        }

        // Ana Belge §7 / §10: no screen may show a level number. Looks for the usual ways one would be printed.
        [Test]
        public void NoUiText_ShowsALevelNumber()
        {
            var root = Path.Combine(Application.dataPath, "_Project", "Scripts");
            var bad = new Regex(@"\(index \+ 1\)\.ToString\(\)|\{number\}|\{index \+ 1\}|\{CurrentLevel \+ 1\}|\.id\}|Bölüm \{");
            foreach (var folder in new[] { "UI", "View" })
                foreach (var file in Directory.GetFiles(Path.Combine(root, folder), "*.cs"))
                {
                    var lines = File.ReadAllLines(file);
                    for (int i = 0; i < lines.Length; i++)
                        Assert.IsFalse(bad.IsMatch(lines[i]), $"{Path.GetFileName(file)}:{i + 1}: {lines[i].Trim()}");
                }
            var game = File.ReadAllLines(Path.Combine(root, "Game", "GameController.cs"));
            for (int i = 0; i < game.Length; i++)
                Assert.IsFalse(bad.IsMatch(game[i]), $"GameController.cs:{i + 1}: {game[i].Trim()}");
        }
    }
}
