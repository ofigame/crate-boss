using System;
using NUnit.Framework;
using CrateBoss.Meta;
using UnityEngine;

namespace CrateBoss.Tests
{
    /// <summary>The Boss's land (Ana Belge §12, §21 "Ekonomi" and "Yerleştirme"): crates, the three choices, placing, paths, ranks.</summary>
    public class HomeTests
    {
        private static ItemCatalog Catalog() => new ItemCatalog(
            "house_small\t1\tbig\t3\t3\tbuilding\t#F2A65A\n" +
            "bench\t1\tsmall\t1\t1\tgarden\t#C98A5A\n" +
            "planters\t1\tsmall\t1\t1\tgarden\t#F2558A\n" +
            "swing\t1\tmedium\t2\t1\tgarden\t#9B5DE5\n" +
            "bicycle\t1\tsmall\t1\t1\tvehicle\t#3EC1D3\n" +
            "yacht\t4\tbig\t2\t3\twater\t#F7F7F9\n" +
            "hatchback\t1\tspecial\t1\t2\tvehicle\t#F2558A\n" +
            "decor_tree\t0\tdecor\t1\t1\tgarden\t#4CB86A\n");

        private static SaveData Fresh()
        {
            var d = new SaveData();
            HomeRules.OpenLands(d, 1, true);
            return d;
        }

        [Test]
        public void FirstCrate_IsFree_ThenItCosts30_AndAShortBalanceRefuses()
        {
            var d = Fresh();
            var w = new Wallet(d);
            var cfg = ScriptableObject.CreateInstance<EconomyConfig>();
            d.crates = 2;
            Assert.IsTrue(HomeRules.Open(d, w, cfg, false, false), "the first crate is free (§11.1)");
            Assert.IsFalse(HomeRules.Open(d, w, cfg, false, false), "no CB Coin: refused");
            Assert.AreEqual(1, d.crates, "a crate is never lost");
            w.Add(30);
            Assert.IsTrue(HomeRules.Open(d, w, cfg, false, false));
            Assert.AreEqual(0, w.Balance);
            d.crates = 1;
            Assert.IsTrue(HomeRules.Open(d, w, cfg, false, true), "paid by an ad");
        }

        [Test]
        public void Offer_ThreeDifferent_NotOwned_FromTheRank()
        {
            var cat = Catalog();
            var d = Fresh();
            d.owned.Add("bench");
            var offer = HomeRules.Offer(cat, d, 1, new System.Random(3));
            Assert.AreEqual(3, offer.Count);
            Assert.AreEqual(3, new System.Collections.Generic.HashSet<string>(offer).Count, "no item twice");
            Assert.IsFalse(offer.Contains("bench"), "an owned item never comes again");
            Assert.IsFalse(offer.Contains("hatchback"), "the promotion item only comes in the promotion crate");
        }

        [Test]
        public void PromotionCrate_OffersTheRanksSpecialItem()
        {
            var offer = HomeRules.Offer(Catalog(), Fresh(), 1, new System.Random(1), promotionRank: 1);
            Assert.AreEqual("hatchback", offer[0]);
        }

        [Test]
        public void EveryFifthCrate_HasABigItem()
        {
            var cat = Catalog();
            for (int seed = 0; seed < 20; seed++)
            {
                var d = Fresh();
                d.cratesOpened = 4;
                var offer = HomeRules.Offer(cat, d, 1, new System.Random(seed));
                Assert.IsTrue(offer.Exists(id => cat.Get(id).Big), $"seed {seed}");
            }
        }

        [Test]
        public void Placement_NoOverlap_InsideTheLand_DoorToTheRoadOrAPath()
        {
            var cat = Catalog();
            var d = Fresh();
            var land = LandCatalog.Get("start");
            d.inventory.Add("house_small");
            d.inventory.Add("bench");
            Assert.IsFalse(HomeRules.Place(d, cat, "house_small", land, 2, 2, 0), "the door faces grass, no path");
            Assert.IsTrue(HomeRules.Place(d, cat, "house_small", land, 2, land.D - 3, 0), "front row: the door opens onto the road");
            Assert.IsFalse(HomeRules.Place(d, cat, "bench", land, 3, land.D - 2, 0), "on the house");
            Assert.IsFalse(HomeRules.Place(d, cat, "bench", land, land.W, 0, 0), "outside the land");
            Assert.IsTrue(HomeRules.Place(d, cat, "bench", land, 0, 0, 0));
            d.inventory.Add("house_small");
            Assert.IsTrue(HomeRules.TogglePath(d, cat, land, 6, 4));
            Assert.IsTrue(HomeRules.Place(d, cat, "house_small", land, 6, 1, 0), "the door onto a path");
        }

        [Test]
        public void Vehicles_OnPaths_Boats_OnWater()
        {
            var cat = Catalog();
            var d = Fresh();
            var land = LandCatalog.Get("start");
            Assert.IsFalse(HomeRules.CanPlace(d, cat, land, cat.Get("bicycle"), 4, 4, 0), "a bicycle on the grass");
            HomeRules.TogglePath(d, cat, land, 4, 4);
            Assert.IsTrue(HomeRules.CanPlace(d, cat, land, cat.Get("bicycle"), 4, 4, 0));
            HomeRules.OpenLands(d, 4, true);
            var shore = LandCatalog.Get("shore");
            Assert.IsTrue(HomeRules.CanPlace(d, cat, shore, cat.Get("yacht"), 0, 0, 0), "on the sea");
            Assert.IsFalse(HomeRules.CanPlace(d, cat, land, cat.Get("yacht"), 0, 0, 0), "not on land");
        }

        [Test]
        public void Store_PutsItBackInTheInventory()
        {
            var cat = Catalog();
            var d = Fresh();
            d.inventory.Add("bench");
            HomeRules.Place(d, cat, "bench", LandCatalog.Get("start"), 0, 0, 0);
            HomeRules.Store(d, d.placed[0]);
            Assert.AreEqual(0, d.placed.Count);
            Assert.Contains("bench", d.inventory);
        }

        [Test]
        public void Lands_OpenWithTheRank()
        {
            var d = new SaveData();
            HomeRules.OpenLands(d, 1, false);
            Assert.AreEqual(0, d.lands.Count, "nothing before level 1");
            HomeRules.OpenLands(d, 1, true);
            CollectionAssert.AreEquivalent(new[] { "start" }, d.lands);
            HomeRules.OpenLands(d, 3, true);
            CollectionAssert.AreEquivalent(new[] { "start", "garden", "lake" }, d.lands);
        }

        [Test]
        public void Rank_NeedsTheLastLevelAnd60PercentOfTheStars()
        {
            // 44 of 75 stars: still Öğrenci; 45: Stajyer (Ana Belge §21 "İlerleme").
            Assert.AreEqual(1, RankCatalog.Current(5, l => true, l => 44, l => 45));
            Assert.AreEqual(2, RankCatalog.Current(5, l => l == 1, l => 45, l => 45));
            Assert.AreEqual(1, RankCatalog.Current(5, l => false, l => 75, l => 45), "the exam level must be cleared");
            var ranks = new RankCatalog("1\togrenci\tR1\tstart\n2\tstajyer\tR2\tgarden\n");
            Assert.AreEqual(2, ranks.Ranks.Count);
            Assert.AreEqual("league.2", ranks.Get(2).NameKey);
        }

        [Test]
        public void Wardrobe_BuyThenWear()
        {
            var d = new SaveData();
            var w = new Wallet(d);
            Assert.IsFalse(WardrobeCatalog.BuyOrWear(d, w, "hat_red"), "150 CB Coin needed");
            w.Add(200);
            Assert.IsTrue(WardrobeCatalog.BuyOrWear(d, w, "hat_red"));
            Assert.AreEqual(50, w.Balance);
            Assert.AreEqual("hat_red", WardrobeCatalog.Worn(d, "worker.hat").Id);
        }
    }
}
