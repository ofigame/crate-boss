using CrateBoss.Core;
using CrateBoss.Localization;
using CrateBoss.Solver;
using CrateBoss.UI;
using CrateBoss.View;
using UnityEngine;

namespace CrateBoss.Game
{
    /// <summary>
    /// Picks and runs the tutorial of a level (owner 2026-10-31: the first time each puzzle object shows up, a hand, skippable;
    /// 2026-11-01: with an info bubble that says the rule). One per level, each shown once: "push" on the first level played
    /// (the hand swipes every move of the stored solution to the win; a tap shows its second bubble, a second tap ends it), "crane" the first time a crane stands by the yard, then
    /// the first new object of the level (delivery crate, car, cargo, colour crates, hole, ice, heavy) and "fixed" crates.
    /// </summary>
    public sealed class TutorialDirector
    {
        private readonly TutorialView view;
        private readonly BoardView board;
        private string kind;
        private int movesLeft;
        private StoredSolution solution;
        private GameState state;
        private Direction next;
        private GridPos mark;
        /// <summary>Which bubble of the tutorial shows (the push tutorial has two).</summary>
        private int page;

        public TutorialDirector(TutorialView view, BoardView board)
        {
            this.view = view;
            this.board = board;
            view.Tapped += Advance;
        }

        public void Start(GameState start, StoredSolution stored)
        {
            Stop();
            state = start;
            solution = stored;
            if (start.IsCandyLevel && !ProgressStore.TutorialSeen("swap"))
            {
                // Candy levels (owner 2026-11-01): the swap first, then (after the first group) the hammer.
                kind = "swap";
                var first = start.PlayerPos;
                foreach (var b in start.Boxes) if (CandySystem.IsCandy(b)) { first = b.Position; break; }
                view.ShowTap(() => board.CellWorld(first), Loc.T("tut.swap"));
            }
            else if (start.IsCandyLevel && !ProgressStore.TutorialSeen("lock") && start.Bays.Length > 0 && LockWorthy(start))
            {
                // Owner 2026-11-02: a tap on the worker locks him; then short drags slide the view over the yard.
                kind = "lock";
                movesLeft = 6;
                view.ShowTap(() => board.CellWorld(state.PlayerPos), Loc.T("tut.lock"));
            }
            else if (!ProgressStore.TutorialSeen("push") && stored != null && Aim(start))
            {
                kind = "push";
                view.ShowSwipe(() => board.CellWorld(state.PlayerPos), ScreenDir, () => board.CellWorld(mark), Loc.T("tut.push"));
            }
            else if (!ProgressStore.TutorialSeen("crane") && start.Cranes.Count > 0 && start.Cranes[0].Drops.Count > 0)
            {
                kind = "crane";
                var cell = start.Cranes[0].Drops[0].Cell;
                movesLeft = 3;
                view.ShowTap(() => board.CellWorld(cell), Loc.T("tut.crane"));
            }
            else if (FirstNew(start, out var newKind, out var newCell))
            {
                kind = newKind;
                movesLeft = 4;
                view.ShowTap(() => board.CellWorld(newCell), Loc.T("tut." + newKind));
            }
            else if (!ProgressStore.TutorialSeen("fixed") && board.InnerWalls.Count > 0)
            {
                kind = "fixed";
                var cell = Nearest(start.PlayerPos);
                movesLeft = 3;
                view.ShowTap(() => board.CellWorld(cell), Loc.T("tut.fixed"));
            }
        }

        // The first object of this level the player has not been shown yet (colour crates, hole, ice, heavy), and
        // where the first one of it stands.
        private static bool FirstNew(GameState s, out string kind, out GridPos cell)
        {
            foreach (var k in new[] { "deliver", "car", "cargo", "match", "hole", "ice", "heavy" })
            {
                if (ProgressStore.TutorialSeen(k)) continue;
                if (k == "car" && s.Parking != null)
                    for (int i = 0; i < s.Parking.Cars.Count; i++)
                        if (s.CarPresent(i)) { kind = k; cell = s.Parking.Cars[i].A; return true; }
                foreach (var b in s.Boxes)
                    if (k == "match" && b.Symbol > 0 || k == "ice" && b.Type == BoxType.Ice || k == "heavy" && b.Type == BoxType.Heavy
                        || k == "deliver" && b.Delivery >= 0 || k == "cargo" && b.IsCargo)
                    { kind = k; cell = b.Position; return true; }
                for (int y = 0; y < s.Height; y++)
                for (int x = 0; x < s.Width; x++)
                {
                    var t = s.GetTile(new GridPos(x, y));
                    if (k == "hole" && t == TileType.Hole)
                    { kind = k; cell = new GridPos(x, y); return true; }
                }
            }
            kind = null;
            cell = default;
            return false;
        }

        /// <summary>The lock tutorial was tapped away: lock the worker (its bubble asks for that tap).</summary>
        public event System.Action LockRequested;

        /// <summary>The worker was locked or unlocked by a tap: the lock tutorial has done its job.</summary>
        public void OnLockToggled()
        {
            if (kind == "lock") Finish();
        }

        // A candy yard big enough that looking round it helps (the swap tutorial came first).
        private static bool LockWorthy(GameState s) => ProgressStore.TutorialSeen("swap") && s.Width * s.Height >= 120;

        /// <summary>After every successful move.</summary>
        public void OnMoved(GameState now, MoveResult result)
        {
            if (kind == null) return;
            state = now;
            if (kind == "swap")
            {
                if (page == 0 && result.HasCandyWaves) { page = 1; movesLeft = 4; view.SetText(Loc.T("tut.hammer")); }
                else if (page == 1 && --movesLeft <= 0) Finish();
                return;
            }
            if (kind == "push")
            {
                // The whole first level is guided (owner 2026-11-01: "öğretici hiçbir işe yaramıyor"); off the way it ends.
                if (result.IsWon || !Aim(now)) { Finish(); return; }
                if (result.BoxMoves.Count > 0 && page == 0) { page = 1; view.SetText(Loc.T("tut.push2")); }
                return;
            }
            if (--movesLeft <= 0) Finish();
        }

        /// <summary>
        /// A tap on the screen (owner 2026-11-01, instead of the close button): the next bubble when there is one, else the
        /// tutorial closes and counts as seen.
        /// </summary>
        private void Advance()
        {
            if (kind == null) return;
            if (kind == "lock") LockRequested?.Invoke(); // the tap that closes it locks the worker, as the bubble says
            if ((kind == "push" || kind == "swap") && page == 0)
            {
                page = 1;
                movesLeft = 4;
                view.SetText(Loc.T(kind == "push" ? "tut.push2" : "tut.hammer"));
                return;
            }
            Finish();
        }

        public void Stop()
        {
            kind = null;
            page = 0;
            view.Hide();
        }

        private void Finish()
        {
            if (kind != null) ProgressStore.MarkTutorial(kind);
            Stop();
        }

        // The next move of the stored solution from here and the crate its first push moves; false when off its way.
        private bool Aim(GameState s)
        {
            var rest = solution?.From(s.GetKey());
            if (rest == null || rest.Moves.Count == 0) return false;
            if (rest.Moves[0].IsCar) return false;
            next = rest.Moves[0].Direction;
            int push = LevelAnalyzer.FirstPushIndex(s, rest.Moves);
            if (push < 0) return false;
            var sim = s.Clone();
            var rules = new RulesEngine();
            for (int i = 0; i < push; i++) rules.TryMove(sim, rest.Moves[i]);
            mark = sim.PlayerPos + rest.Moves[push].Direction.ToOffset();
            return true;
        }

        private Vector2 ScreenDir()
        {
            var cam = Camera.main;
            if (cam == null) return Vector2.up;
            var a = cam.WorldToScreenPoint(board.CellWorld(state.PlayerPos));
            var b = cam.WorldToScreenPoint(board.CellWorld(state.PlayerPos + next.ToOffset()));
            return new Vector2(b.x - a.x, b.y - a.y);
        }

        private GridPos Nearest(GridPos from)
        {
            var best = board.InnerWalls[0];
            foreach (var c in board.InnerWalls) if (c.ManhattanTo(from) < best.ManhattanTo(from)) best = c;
            return best;
        }
    }
}
