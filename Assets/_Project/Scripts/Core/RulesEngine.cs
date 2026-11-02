using System.Collections.Generic;

namespace CrateBoss.Core
{
    /// <summary>
    /// All movement rules. TryMove changes the state only when the move succeeds; a failed move leaves it untouched and does
    /// not count. New box types plug in through TryPush. A move is processed in this exact order (Ana Belge §5.6), the same
    /// in the game, the solver and the tests:
    /// 1 the worker walks or pushes, or a car is sent (stop when it fails) · 2 move count · 3 delivery / way stop ·
    /// 4 matches · 5 truck leaves / backs in · 6 win check (stop when won) · 7 cranes A, B, C · 8 matches again (a crane's
    /// colour crate) and the delivery check for a crane's delivery crate.
    /// </summary>
    public sealed class RulesEngine
    {
        public MoveResult TryMove(GameState state, Direction dir) => TryMove(state, PlayerAction.Walk(dir));

        public MoveResult TryMove(GameState state, PlayerAction action)
        {
            var result = new MoveResult(action, state.PlayerPos);
            if (action.IsSwap)
            {
                if (!CandySystem.Swap(state, action, result)) return result;                       // 1 (candy levels)
            }
            else if (action.Paint)
            {
                if (!CandySystem.Paint(state, result)) return result;                              // 1 (candy levels)
            }
            else if (action.IsHit)
            {
                if (!HitFromAfar(state, action, result)) return result;                           // 1 (candy levels)
            }
            else if (action.IsCar)
            {
                var car = CarSystem.Send(state, action.Car, action.Direction);                     // 1
                if (car == null) return result;
                result.Car = car;
                if (!car.Left) return result; // a bump changes nothing and is not a move (the session counts it)
            }
            else if (!Walk(state, action.Direction, result)) return result;                         // 1

            result.Success = true;
            state.MoveCount++;                                                                       // 2
            if (action.IsCar || result.BoxMoves.Count > 0) state.PushCount++;
            CandySystem.Age(state);
            if (result.BoxMoves.Count > 0) result.Deliveries = DeliverySystem.Step(state, MovedIds(result.BoxMoves)); // 3
            result.Sunk = MatchSystem.Step(state, out int bonus);                                    // 4
            result.MatchBonus = bonus;
            result.ConveyorMoves = ConveyorSystem.Step(state);                                       // 4b (the cargo belt)
            result.TruckLeft = TruckSystem.Step(state, result);                                              // 5
            result.IsWon = state.IsSolved();                                                         // 6
            if (result.IsWon) return result; // a winning move never runs a machine
            result.CraneDrops = CraneSystem.StepAll(state);                                          // 7
            if (result.CraneDrops.Count > 0)                                                         // 8
            {
                result.CraneSunk = MatchSystem.Step(state, out int more);
                result.MatchBonus += more;
                var dropped = new List<int>(result.CraneDrops.Count);
                foreach (var d in result.CraneDrops) if (d.BoxId >= 0) dropped.Add(d.BoxId);
                result.CraneDeliveries = DeliverySystem.Step(state, dropped);
            }
            return result;
        }

        private static bool Walk(GameState state, Direction dir, MoveResult result)
        {
            var step = dir.ToOffset();
            var to = state.PlayerPos + step;
            if (!GoalRules.PlayerCanEnter(state, to)) return false;
            var box = state.GetBoxAt(to);
            if (box != null && box.Hits > 0)
            {
                // Candy levels (owner 2026-11-01): the worker breaks a fixed crate with the hammer, one hit per move, staying put.
                if (state.Hammers <= 0) { result.NoHammer = true; return false; }
                state.Hammers--;
                box.Hits--;
                result.HitBox = box.Id;
                if (box.Hits == 0) { state.RemoveBox(box); result.Broke = true; }
                return true;
            }
            if (box != null && !TryPush(state, box, step, result)) return false;
            state.PlayerPos = to;
            result.PlayerTo = to;
            return true;
        }

        /// <summary>
        /// Owner 2026-11-02: a hammer hit without walking up to the crate: the auto hammer that goes on hitting a crate by itself
        /// (plain hammers), or a tap with an advanced hammer. The worker stays where he is.
        /// </summary>
        private static bool HitFromAfar(GameState state, PlayerAction action, MoveResult result)
        {
            if (action.Hit >= state.Width * state.Height) return false;
            var box = state.GetBoxAt(state.CellAt(action.Hit));
            if (box == null || box.Hits <= 0) return false;
            if (action.Super ? state.SuperHammers <= 0 : state.Hammers <= 0) { result.NoHammer = true; return false; }
            if (action.Super) state.SuperHammers--; else state.Hammers--;
            box.Hits--;
            result.HitBox = box.Id;
            result.HitFromAfar = true;
            if (box.Hits == 0) { state.RemoveBox(box); result.Broke = true; }
            return true;
        }

        private static List<int> MovedIds(List<BoxMovement> moves)
        {
            var ids = new List<int>(moves.Count);
            foreach (var m in moves) if (m.FellInto == FallType.None) ids.Add(m.BoxId);
            return ids;
        }

        private static bool TryPush(GameState state, Box box, GridPos step, MoveResult result)
        {
            if (box.Locked || box.IsHeavy) return false; // delivered crates stay; heavy ones only move as the back of a chain
            if (state.IsCandyLevel && box.Symbol > 0) return false; // candy levels: colour crates are swapped by the finger
            switch (box.Type)
            {
                case BoxType.Normal:
                case BoxType.Delivery: return TryPushNormal(state, box, step, result);
                case BoxType.Ice: return TryPushIce(state, box, step, result);
                default: return false; // containers never move
            }
        }

        private static bool TryPushNormal(GameState state, Box box, GridPos step, MoveResult result)
        {
            var dest = box.Position + step;
            var blocker = state.GetBoxAt(dest);
            if (blocker != null)
            {
                // The only chain (Ana Belge §4.2): a normal, colour or delivery crate shoves the heavy crate right behind it
                // one cell. The heavy crate's cell is looked at first.
                if (!blocker.IsHeavy || blocker.Locked) return false;
                var heavyDest = dest + step;
                if (!CanEnter(state, heavyDest, out var heavyFall)) return false;
                Apply(state, blocker, heavyDest, heavyFall, result);
                Apply(state, box, dest, FallType.None, result);
                return true;
            }

            if (!CanEnter(state, dest, out var fall)) return false;
            Apply(state, box, dest, fall, result);
            return true;
        }

        private static bool TryPushIce(GameState state, Box box, GridPos step, MoveResult result)
        {
            var cur = box.Position;
            var fall = FallType.None;
            while (true)
            {
                var next = cur + step;
                if (state.GetBoxAt(next) != null || GoalRules.BlocksBox(state, next)) break;
                var tile = state.GetTile(next);
                cur = next;
                if (tile == TileType.Hole) { fall = FallType.Hole; break; }
                if (tile == TileType.Void) { fall = FallType.Void; break; }
            }
            if (cur == box.Position) return false;
            Apply(state, box, cur, fall, result);
            return true;
        }

        /// <summary>Can a single box move into this cell? Holes and void are allowed (it falls in).</summary>
        private static bool CanEnter(GameState state, GridPos cell, out FallType fall)
        {
            fall = FallType.None;
            if (state.GetBoxAt(cell) != null || GoalRules.BlocksBox(state, cell)) return false;
            switch (state.GetTile(cell))
            {
                case TileType.Hole: fall = FallType.Hole; return true;
                case TileType.Void: fall = FallType.Void; return true;
                default: return true;
            }
        }

        private static void Apply(GameState state, Box box, GridPos to, FallType fall, MoveResult result)
        {
            result.BoxMoves.Add(new BoxMovement(box.Id, box.Type, box.Position, to, fall));
            if (fall == FallType.None)
            {
                box.Position = to;
                return;
            }
            state.RemoveBox(box);
            if (fall == FallType.Hole)
            {
                state.SetTile(to, TileType.FilledHole);
                result.FilledHoles.Add(to);
            }
        }
    }
}
