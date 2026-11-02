using System.Collections;
using System.IO;
using CrateBoss.Core;
using UnityEngine;

namespace CrateBoss.Game
{
    /// <summary>
    /// Test helper: run the player with "-shots &lt;folder&gt;" and it plays every level's best solution,
    /// saving a picture at the start, mid-way and on the win card, plus a hint and the level map, then quits.
    /// Add "-quick" to only take the start pictures.
    /// </summary>
    public class ScreenshotTour : MonoBehaviour
    {
        /// <summary>The game was started with -shots: it plays on a save kept in memory.</summary>
        public static bool Requested => System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-shots") >= 0 || System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-perf") >= 0;

        private string folder;
        private bool quick;
        private bool languageTour;
        private bool candyTour;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void StartIfAsked()
        {
            var args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] != "-shots") continue;
                var tour = new GameObject("ScreenshotTour").AddComponent<ScreenshotTour>();
                tour.folder = args[i + 1];
                tour.quick = System.Array.IndexOf(args, "-quick") >= 0;
                tour.languageTour = System.Array.IndexOf(args, "-langtour") >= 0;
                tour.candyTour = System.Array.IndexOf(args, "-candytour") >= 0;
                return;
            }
        }

        private IEnumerator Start()
        {
            Directory.CreateDirectory(folder);
            var game = FindAnyObjectByType<GameController>();
            yield return new WaitForSeconds(1.2f);
            if (candyTour)
            {
                yield return CandyShots(game);
                Application.Quit();
                yield break;
            }
            if (languageTour)
            {
                yield return LanguageShots(game);
                Application.Quit();
                yield break;
            }
            yield return HomeShots(game);
            for (int level = 0; level < game.LevelCount; level++)
            {
                game.LoadLevel(level);
                yield return new WaitForSeconds(2.2f);
                yield return Shot($"level{level + 1:00}_a_start");
                if (quick) continue;

                if (level == 10)
                {
                    game.RequestHint();
                    yield return new WaitForSeconds(1.0f);
                    yield return Shot($"level{level + 1:00}_hint");
                    // Free hints used up: the choice card (ad / CB Coin / whole solution, Ana Belge §14).
                    game.Meta.Data.hints = 0;
                    game.RequestHint();
                    yield return new WaitForSeconds(0.5f);
                    yield return Shot("hint_card");
                    game.CloseCards();
                    game.LoadLevel(level);
                    yield return new WaitForSeconds(1.2f);
                }

                while (game.Best == null) yield return null;
                var moves = game.Best.Moves;
                bool tenseShot = false, undoShot = false, waitShot = false;
                for (int m = 0; m < moves.Count; m++)
                {
                    while (!game.IsIdle) yield return null;
                    int drops = game.State.CraneIndex;
                    game.RequestAction(moves[m]);
                    if (m == moves.Count / 2)
                    {
                        yield return new WaitForSeconds(0.07f);
                        yield return Shot($"level{level + 1:00}_b_mid");
                    }
                    if (game.State.Crane == null) continue;
                    // Crane levels: the warning at one move left, every drop, and one undo of a drop.
                    while (!game.IsIdle) yield return null;
                    if (!tenseShot && game.State.HasActiveCrane && game.State.CraneCounter == 1)
                    {
                        tenseShot = true;
                        yield return new WaitForSeconds(0.3f);
                        yield return Shot($"level{level + 1:00}_crane_1left");
                    }
                    if (game.State.CraneIndex > drops)
                    {
                        yield return new WaitForSeconds(0.7f);
                        yield return Shot($"level{level + 1:00}_crane_drop{game.State.CraneIndex}");
                        if (!undoShot)
                        {
                            undoShot = true;
                            game.Undo();
                            yield return new WaitForSeconds(0.3f);
                            yield return Shot($"level{level + 1:00}_crane_undo");
                            game.RequestAction(moves[m]);
                        }
                    }
                    if (!waitShot && CraneWaiting(game))
                    {
                        waitShot = true;
                        yield return new WaitForSeconds(0.3f);
                        yield return Shot($"level{level + 1:00}_crane_wait");
                    }
                }
                yield return new WaitForSeconds(4.5f);
                yield return Shot($"level{level + 1:00}_c_win");
                Debug.Log($"[Tour] level {level + 1}: won={game.IsWon} moves={moves.Count}");
                if (level == 11) yield return InterstitialShots(game);
                if (level == 12) yield return DoubleShots(game);
            }
            game.ShowLevels();
            yield return new WaitForSeconds(0.8f);
            yield return Shot("map");
            Application.Quit();
        }

        // Ana Belge §18: one short pass per language ("-langtour -lang xx"): the home list, the language list, a crane level
        // with its chips and task card, the hint choices, the win panel and the test rewarded ad.
        private IEnumerator LanguageShots(GameController game)
        {
            game.ShowLevels();
            yield return new WaitForSeconds(0.6f);
            yield return Shot("lang_map");
            game.ShowLanguages();
            yield return new WaitForSeconds(0.4f);
            yield return Shot("lang_list");
            game.CloseCards();
            int machine = 0;
            for (int i = 0; i < game.LevelCount; i++)
            {
                game.LoadLevel(i);
                if (game.State.Cranes.Count > 0) { machine = i; break; }
            }
            game.LoadLevel(machine);
            yield return new WaitForSeconds(0.5f);
            yield return Shot("lang_game_card");
            yield return new WaitForSeconds(2.5f);
            yield return Shot("lang_game");
            game.Meta.Data.hints = 0;
            game.RequestHint();
            yield return new WaitForSeconds(0.5f);
            yield return Shot("lang_hint");
            game.CloseCards();
            game.LoadLevel(0);
            yield return new WaitForSeconds(1.2f);
            while (game.Best == null) yield return null;
            foreach (var move in game.Best.Moves)
            {
                while (!game.IsIdle) yield return null;
                game.RequestAction(move);
            }
            yield return new WaitForSeconds(4.5f);
            yield return Shot("lang_win");
            game.DoubleFromWin();
            yield return new WaitForSeconds(1.5f);
            yield return Shot("lang_ad");
            yield return new WaitForSeconds(4.5f);
            Debug.Log($"[Tour] language {Localization.Loc.Code} done");
        }

        // The home screen (the level list until the Boss's land comes in F7).
        private IEnumerator HomeShots(GameController game)
        {
            // A small lived-in plot for the picture (the tour plays on a save in memory).
            var d = game.Meta.Data;
            Meta.HomeRules.OpenLands(d, 2, true);
            d.crates = 2;
            var start = Meta.LandCatalog.Get("start");
            for (int x = 0; x < start.W; x++) Meta.HomeRules.TogglePath(d, game.Meta.Items, start, x, 6);
            foreach (var (id, x, y) in new[] { ("house_small", 1, 3), ("small_tree", 6, 2), ("bench", 5, 8), ("planters", 8, 8), ("bicycle", 4, 6) })
            {
                d.inventory.Add(id);
                Meta.HomeRules.Place(d, game.Meta.Items, id, start, x, y, 0);
            }
            game.ShowHome();
            yield return new WaitForSeconds(1.2f);
            yield return Shot("home_plot");
            yield return new WaitForSeconds(0.6f);
            yield return Shot("home_start");
        }

        // Ana Belge §15.2: "Sonraki" after a cleared yard → the short cover, then the test interstitial.
        private IEnumerator InterstitialShots(GameController game)
        {
            game.NextFromWin();
            yield return new WaitForSeconds(0.12f);
            yield return Shot("ad_transition");
            yield return new WaitForSeconds(1.2f);
            yield return Shot("ad_interstitial");
            yield return new WaitForSeconds(3.5f);
        }

        // Ana Belge §15.1: "Reklam izle, ×2" → the test rewarded ad, then the doubled line.
        private IEnumerator DoubleShots(GameController game)
        {
            game.DoubleFromWin();
            yield return new WaitForSeconds(1.5f);
            yield return Shot("ad_rewarded");
            yield return new WaitForSeconds(4.5f);
            yield return Shot("win_doubled");
        }

        private static bool CraneWaiting(GameController game)
        {
            for (int i = 0; i < game.State.Cranes.Count; i++)
                if (Core.CraneSystem.IsWaiting(game.State, i)) return true;
            return false;
        }

        // Candy levels (owner 2026-11-01): the first candy level, a few best-paying swaps, then hammer hits on a crate.
        private IEnumerator CandyShots(GameController game)
        {
            // "-candylevel 4": the tour plays that level (1-based) instead of the first.
            var args = System.Environment.GetCommandLineArgs();
            int pick = System.Array.IndexOf(args, "-candylevel");
            int levelIndex = pick >= 0 && pick < args.Length - 1 && int.TryParse(args[pick + 1], out var n1) ? n1 - 1 : 0;
            game.LoadLevel(levelIndex);
            yield return new WaitForSeconds(4f);
            yield return Shot("candy_a_start");
            // The combo marks on a copy of the yard (the game itself is not touched).
            {
                var look = game.State.Clone();
                look.CandyDone = true; // the cargo arrows show on the copy
                var kinds = new[] { CandySpecial.StripeRow, CandySpecial.StripeColumn, CandySpecial.Bomb, CandySpecial.Rainbow };
                int k = 0;
                foreach (var b in look.Boxes)
                    if (k < kinds.Length && CandySystem.IsCandy(b) && b.Position.X >= 4 && b.Position.Y >= 6) b.Special = kinds[k++];
                game.Board.Build(look, null);
                yield return new WaitForSeconds(0.6f);
                yield return Shot("candy_combos");
                game.Board.Build(game.State, null);
                yield return new WaitForSeconds(0.3f);
            }
            game.CloseTutorial();
            var rules = new RulesEngine();
            for (int n = 0; n < 10 && CandySystem.CandyLeft(game.State) > 0; n++)
            {
                while (!game.IsIdle) yield return null;
                PlayerAction? best = null;
                int pay = -1;
                foreach (var box in game.State.Boxes.ToArray())
                {
                    if (!CandySystem.IsCandy(box)) continue;
                    foreach (var dir in new[] { Direction.Right, Direction.Down })
                    {
                        var a = PlayerAction.SwapCandy(game.State.Index(box.Position), dir);
                        var t = game.State.Clone();
                        if (rules.TryMove(t, a).Success && t.Hammers - game.State.Hammers > pay) { pay = t.Hammers - game.State.Hammers; best = a; }
                    }
                }
                if (best == null) break;
                game.RequestAction(best.Value);
                yield return new WaitForSeconds(0.45f);
                yield return Shot($"candy_b_swap{n + 1}");
                yield return new WaitForSeconds(0.45f);
                yield return Shot($"candy_b_crane{n + 1}");
                yield return new WaitForSeconds(0.25f);
                yield return Shot($"candy_b_crane{n + 1}b");
                while (!game.IsIdle) yield return null;
                yield return Shot($"candy_c_after{n + 1}");
            }
            // Walk up to the crate under the cargo and hit it.
            while (!game.IsIdle) yield return null;
            game.RequestAction(Direction.Up);
            while (!game.IsIdle) yield return null;
            game.RequestAction(Direction.Up);
            yield return new WaitForSeconds(0.2f);
            yield return Shot("candy_d_swing");
            while (!game.IsIdle) yield return null;
            yield return Shot("candy_d_hit");
            game.RequestAction(Direction.Up);
            while (!game.IsIdle) yield return null;
            yield return Shot("candy_e_broken");
            Debug.Log($"[Tour] candy hammers={game.State.Hammers} dropsLeft={game.State.CandyDropsLeft} candy={CandySystem.CandyLeft(game.State)}");
        }

        private IEnumerator Shot(string name)
        {
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(folder, name + ".png"));
            yield return null;
        }
    }
}
