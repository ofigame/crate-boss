using NUnit.Framework;
using CrateBoss.Core;
using CrateBoss.Levels;

namespace CrateBoss.Tests
{
    /// <summary>Candy levels (owner 2026-11-01): finger swaps, crane refills, hammer hits on breakable crates.</summary>
    public class CandyTests
    {
        // Row 1: 1 1 3 1 → swapping the 3 with the 1 below it lines up 1 1 1.
        private const string Json = @"{
  ""id"": 9001, ""name"": ""Şeker deneme"",
  ""ground"": [
    ""#######"",
    ""#.....#"",
    ""#.....#"",
    ""#.....#"",
    ""#.....#"",
    ""#######""
  ],
  ""objects"": [
    ""......."",
    "".113..."",
    "".531..."",
    ""......."",
    "".PF...."",
    "".......""
  ],
  ""goal"": [""targets""],
  ""candy"": { ""drops"": DROPS, ""colors"": [1, 3, 5], ""seed"": 7, ""hits"": [[2, 4, 2]] }
}";

        private static GameState Level(int drops = 10) =>
            new LevelParser().Parse(LevelLoader.FromJson(Json.Replace("DROPS", drops.ToString())));

        private static int Cell(GameState s, int x, int y) => s.Index(new GridPos(x, y));

        [Test]
        public void SwapThatLinesUpThree_SinksThem_EarnsAHammer_CranesRefill()
        {
            var s = Level();
            var r = new RulesEngine().TryMove(s, PlayerAction.SwapCandy(Cell(s, 3, 1), Direction.Down));
            Assert.IsTrue(r.Success);
            Assert.IsTrue(r.HasCandyWaves);
            Assert.AreEqual(3, r.CandyWaves[0].Sunk.Count);
            Assert.GreaterOrEqual(s.Hammers, 1);
            Assert.AreEqual(3, r.CandyWaves[0].Drops.Count, "every emptied candy cell gets a new crate");
            Assert.Less(s.CandyDropsLeft, 10);
        }

        [Test]
        public void SwapWithoutARun_IsRefused_AndChangesNothing()
        {
            var s = Level();
            string before = s.GetKey();
            var r = new RulesEngine().TryMove(s, PlayerAction.SwapCandy(Cell(s, 1, 1), Direction.Down));
            Assert.IsFalse(r.Success);
            Assert.AreEqual(before, s.GetKey());
        }

        [Test]
        public void ACrate_SlidesIntoAnEmptyCandyCell_WhenThatMakesARun()
        {
            var s = Level(0);
            // Row 1 is 1 1 3: take the 3 away and move the 1 under it (row 2: 5 3 1) up into the hole: 1 1 1.
            s.RemoveBox(s.GetBoxAt(new GridPos(3, 1)));
            var rules = new RulesEngine();
            Assert.IsFalse(rules.TryMove(s, PlayerAction.SwapCandy(Cell(s, 2, 2), Direction.Right)).Success, "a swap that makes no run: refused");
            var r = rules.TryMove(s, PlayerAction.SwapCandy(Cell(s, 3, 2), Direction.Up));
            Assert.IsTrue(r.Success);
            Assert.AreEqual(3, r.CandyWaves[0].Sunk.Count);
            Assert.IsFalse(new RulesEngine().TryMove(Level(0), PlayerAction.SwapCandy(Cell(s, 1, 1), Direction.Up)).Success, "out of the candy area: refused");
        }

        [Test]
        public void NoSwapLeft_CranesDropWhatTheyHold_BeforeTheCandyPartEnds()
        {
            var s = Level(3);
            foreach (var b in s.Boxes.ToArray()) if (CandySystem.IsCandy(b)) s.RemoveBox(b);
            var r = new MoveResult(Direction.Up, s.PlayerPos);
            CandySystem.Resolve(s, r);
            Assert.AreEqual(0, s.CandyDropsLeft, "the cranes emptied themselves into the holes");
            Assert.IsTrue(r.HasCandyWaves);
            Assert.AreEqual(3, r.CandyWaves[0].Drops.Count);
            Assert.IsTrue(s.CandyDone || CandySystem.HasSwap(s), "then either a swap is there or the part is over");
        }

        // Owner 2026-11-02: a breakable crate loses a hit for every colour crate that sinks right next to it, and breaks at 0.
        [Test]
        public void SinkingNextToABreakableCrate_WearsItDown_AndBreaksItAtZero()
        {
            var s = Level(0);
            var crate = s.AddBox(BoxType.Container, new GridPos(4, 1));
            crate.Hits = 5;
            var r = new RulesEngine().TryMove(s, PlayerAction.SwapCandy(Cell(s, 3, 1), Direction.Down));
            Assert.IsTrue(r.Success);
            Assert.AreEqual(4, crate.Hits, "one crate sank next to it");
            Assert.AreEqual(1, r.CandyWaves[0].Chips.Count);

            var t = Level(0);
            var weak = t.AddBox(BoxType.Container, new GridPos(4, 1));
            weak.Hits = 1;
            new RulesEngine().TryMove(t, PlayerAction.SwapCandy(Cell(t, 3, 1), Direction.Down));
            Assert.IsNull(t.GetBoxAt(new GridPos(4, 1)), "worn to 0: broken");
        }

        private static GameState Empty()
        {
            var s = Level(0);
            foreach (var b in s.Boxes.ToArray()) if (CandySystem.IsCandy(b)) s.RemoveBox(b);
            return s;
        }

        [Test]
        public void RunOfFour_LeavesAStripedCrate()
        {
            var s = Empty();
            s.AddBox(BoxType.Normal, new GridPos(1, 1), 1);
            s.AddBox(BoxType.Normal, new GridPos(2, 1), 1);
            s.AddBox(BoxType.Normal, new GridPos(4, 1), 1);
            s.AddBox(BoxType.Normal, new GridPos(3, 2), 1);
            var r = new RulesEngine().TryMove(s, PlayerAction.SwapCandy(Cell(s, 3, 2), Direction.Up));
            Assert.IsTrue(r.Success);
            Assert.AreEqual(1, r.CandyWaves[0].Made.Count);
            Assert.AreEqual(CandySpecial.StripeRow, r.CandyWaves[0].Made[0].Kind);
            Assert.AreEqual(3, r.CandyWaves[0].Sunk.Count, "the other three sink, the striped one stays");
        }

        [Test]
        public void StripedCrateInARun_ClearsItsRow()
        {
            var s = Empty();
            s.AddBox(BoxType.Normal, new GridPos(1, 1), 1).Special = CandySpecial.StripeRow;
            s.AddBox(BoxType.Normal, new GridPos(2, 1), 1);
            s.AddBox(BoxType.Normal, new GridPos(5, 1), 5);
            s.AddBox(BoxType.Normal, new GridPos(3, 2), 1);
            var r = new RulesEngine().TryMove(s, PlayerAction.SwapCandy(Cell(s, 3, 2), Direction.Up));
            Assert.IsTrue(r.Success);
            Assert.AreEqual(1, r.CandyWaves[0].Blasts.Count);
            Assert.IsNull(s.GetBoxAt(new GridPos(5, 1)), "the far crate of another colour went with the row");
        }

        [Test]
        public void RainbowSwappedWithACrate_ClearsThatColour()
        {
            var s = Empty();
            s.AddBox(BoxType.Normal, new GridPos(1, 1), CandySystem.RainbowSymbol).Special = CandySpecial.Rainbow;
            s.AddBox(BoxType.Normal, new GridPos(2, 1), 4);
            s.AddBox(BoxType.Normal, new GridPos(5, 2), 4);
            s.AddBox(BoxType.Normal, new GridPos(3, 2), 1);
            var r = new RulesEngine().TryMove(s, PlayerAction.SwapCandy(Cell(s, 1, 1), Direction.Right));
            Assert.IsTrue(r.Success, "a rainbow swap needs no run");
            Assert.IsNull(s.GetBoxAt(new GridPos(5, 2)), "every 4 is gone");
            Assert.AreEqual(1, CandySystem.CandyLeft(s), "only the 1 stays");
        }

        [Test]
        public void TheWorker_CannotPushAColourCrate()
        {
            var s = Level();
            s.PlayerPos = new GridPos(1, 3);
            var r = new RulesEngine().TryMove(s, Direction.Up);
            Assert.IsFalse(r.Success);
        }

        [Test]
        public void BreakableCrate_TakesItsHits_ThenIsGone_AndNeedsHammers()
        {
            var s = Level();
            var rules = new RulesEngine();
            var r = rules.TryMove(s, Direction.Right);
            Assert.IsFalse(r.Success);
            Assert.IsTrue(r.NoHammer, "no hammer yet");
            s.Hammers = 2;
            r = rules.TryMove(s, Direction.Right);
            Assert.IsTrue(r.Success);
            Assert.AreEqual(new GridPos(1, 4), s.PlayerPos, "the worker stays while hitting");
            Assert.IsFalse(r.Broke);
            r = rules.TryMove(s, Direction.Right);
            Assert.IsTrue(r.Broke);
            Assert.AreEqual(0, s.Hammers);
            Assert.IsNull(s.GetBoxAt(new GridPos(2, 4)));
        }

        [Test]
        public void NoDropsLeft_TheHolesStayEmpty()
        {
            var s = Level(0);
            var r = new RulesEngine().TryMove(s, PlayerAction.SwapCandy(Cell(s, 3, 1), Direction.Down));
            Assert.IsTrue(r.Success);
            Assert.AreEqual(0, r.CandyWaves[0].Drops.Count);
            Assert.IsNull(s.GetBoxAt(new GridPos(1, 1)));
            // No swap is left: the candy part is over and the crates left stay, now breakable (at least one hit each).
            Assert.IsTrue(s.CandyDone);
            Assert.IsTrue(r.CandyFinished);
            Assert.AreEqual(3, CandySystem.CandyLeft(s));
            foreach (var b in s.Boxes) if (CandySystem.IsCandy(b)) Assert.GreaterOrEqual(b.Hits, 1);
        }

        [Test]
        public void LeftOverCrate_NeedsOneHitPerMoveItStood()
        {
            var s = Level(10);
            var rules = new RulesEngine();
            rules.TryMove(s, Direction.Up); // the worker walks: every colour crate is one move older
            rules.TryMove(s, Direction.Down);
            foreach (var b in s.Boxes) if (CandySystem.IsCandy(b)) Assert.AreEqual(2, b.Age);
            CandySystem.Finish(s, new MoveResult(Direction.Up, s.PlayerPos));
            foreach (var b in s.Boxes) if (CandySystem.IsCandy(b)) Assert.AreEqual(2, b.Hits);
            Assert.IsFalse(rules.TryMove(s, PlayerAction.SwapCandy(Cell(s, 3, 1), Direction.Down)).Success, "no swaps once it is over");
        }

        [Test]
        public void CranesEmpty_CratesSlideTowardTheWorker()
        {
            var s = Level(0);
            s.PlayerPos = new GridPos(2, 4);
            var wave = new CandyWave();
            s.RemoveBox(s.GetBoxAt(new GridPos(1, 2))); // a hole under the 1 at (1, 1)
            CandySystem.Slide(s, wave);
            Assert.IsNotNull(s.GetBoxAt(new GridPos(1, 2)), "the crate above slid down into the hole");
            Assert.IsNull(s.GetBoxAt(new GridPos(1, 1)));
            Assert.AreEqual(1, wave.Falls.Count);
        }

        [Test]
        public void SameStart_SameSwap_SameDrops()
        {
            var a = Level();
            var b = a.Clone();
            var rules = new RulesEngine();
            rules.TryMove(a, PlayerAction.SwapCandy(Cell(a, 3, 1), Direction.Down));
            rules.TryMove(b, PlayerAction.SwapCandy(Cell(b, 3, 1), Direction.Down));
            Assert.AreEqual(a.GetKey(), b.GetKey());
            Assert.AreEqual(a.Hammers, b.Hammers);
        }

        [Test]
        public void HammersFor_OneHitPerSunkCrate()
        {
            // Owner 2026-11-02: 3 crates sink → 3 hits, 4 → 4, 10 → 10.
            Assert.AreEqual(3, CandySystem.HammersFor(3, false));
            Assert.AreEqual(4, CandySystem.HammersFor(4, false));
            Assert.AreEqual(10, CandySystem.HammersFor(10, true));
        }

        [Test]
        public void AHitFromAfar_UsesAHammer_TheWorkerStays()
        {
            // Owner 2026-11-02: the auto hammer goes on by itself while the worker walks off; an advanced hammer hits by a tap.
            var s = Level();
            var crate = s.GetBoxAt(new GridPos(2, 4));
            s.Hammers = 1;
            var at = s.PlayerPos;
            var rules = new RulesEngine();
            var r = rules.TryMove(s, PlayerAction.HitAt(Cell(s, 2, 4), false));
            Assert.IsTrue(r.Success);
            Assert.IsTrue(r.HitFromAfar);
            Assert.AreEqual(1, crate.Hits);
            Assert.AreEqual(0, s.Hammers);
            Assert.AreEqual(at, s.PlayerPos, "nobody walked");
            Assert.IsFalse(rules.TryMove(s, PlayerAction.HitAt(Cell(s, 2, 4), false)).Success, "no hammer left");
            s.SuperHammers = 1;
            r = rules.TryMove(s, PlayerAction.HitAt(Cell(s, 2, 4), true));
            Assert.IsTrue(r.Broke, "the advanced hammer finished it");
            Assert.AreEqual(0, s.SuperHammers);
        }

        [Test]
        public void ThePaintGun_GivesCratesOneColour()
        {
            // Owner 2026-11-02: random colour crates take one colour; what lines up sinks.
            var s = Level(0);
            var rules = new RulesEngine();
            Assert.IsFalse(rules.TryMove(s, PlayerAction.UsePaint()).Success, "no paint gun yet");
            s.PaintGuns = 1;
            var r = rules.TryMove(s, PlayerAction.UsePaint());
            Assert.IsTrue(r.Success);
            Assert.AreEqual(0, s.PaintGuns);
            Assert.Greater(r.Painted.Count, 0);
            Assert.LessOrEqual(r.Painted.Count, CandySystem.PaintCrates);
            foreach (var id in r.Painted)
            {
                var b = s.GetBoxById(id);
                if (b != null) Assert.AreEqual(r.PaintColour, b.Symbol, "painted crates that did not sink wear the colour");
            }
        }

        [Test]
        public void TwoComboCrates_Swapped_BothGoOff()
        {
            var s = Empty();
            s.AddBox(BoxType.Normal, new GridPos(1, 1), 1).Special = CandySpecial.StripeRow;
            s.AddBox(BoxType.Normal, new GridPos(1, 2), 3).Special = CandySpecial.StripeColumn;
            s.AddBox(BoxType.Normal, new GridPos(4, 2), 4); // in the row the row stripe ends up in
            s.AddBox(BoxType.Normal, new GridPos(1, 3), 5); // in the column stripe's column
            var r = new RulesEngine().TryMove(s, PlayerAction.SwapCandy(Cell(s, 1, 1), Direction.Down));
            Assert.IsTrue(r.Success, "two combos swap without a run");
            Assert.AreEqual(0, CandySystem.CandyLeft(s), "the row and the column went");
            Assert.AreEqual(8, s.Hammers, "two hits for every crate a combo cleared (owner 2026-11-02)");
        }
    }
}
