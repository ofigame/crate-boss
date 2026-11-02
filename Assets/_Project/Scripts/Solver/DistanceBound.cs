using System;
using System.Collections.Generic;
using CrateBoss.Core;

namespace CrateBoss.Solver
{
    /// <summary>
    /// A* lower bound on the moves still needed (Ana Belge §10.3). Every push moves one crate one cell, so for each empty
    /// goal target the distance from the nearest usable crate, plus for each undelivered delivery crate the distance to its
    /// area, can never be more than the moves left. Turned off (0) where one move can carry a crate further or move two
    /// (ice, heavy chains) or where a machine can bring new crates (crane drops other than containers).
    /// Returns -1 when a goal target can never be filled (no usable crate is left or coming).
    /// </summary>
    public sealed class DistanceBound
    {
        private readonly List<GridPos> targets = new List<GridPos>();
        private readonly List<GridPos>[] areas = new List<GridPos>[4];
        private readonly bool deliver;

        private DistanceBound(GameState s)
        {
            if ((s.Goal & GoalType.Targets) != 0)
                foreach (int i in s.TargetCells)
                    if (GoalRules.IsGoalTarget(s, s.CellAt(i))) targets.Add(s.CellAt(i));
            deliver = (s.Goal & GoalType.Deliver) != 0;
            for (int a = 1; a <= 3; a++) areas[a] = new List<GridPos>();
            if (s.AreaOf != null)
                for (int i = 0; i < s.AreaOf.Length; i++)
                    if (s.AreaOf[i] > 0) areas[s.AreaOf[i]].Add(s.CellAt(i));
        }

        /// <summary>Null when no useful bound exists for this level (plain breadth-first is used then).</summary>
        public static DistanceBound For(GameState s)
        {
            foreach (var b in s.Boxes)
                if (b.Type == BoxType.Ice || b.IsHeavy) return null;
            foreach (var c in s.Cranes)
                foreach (var d in c.Drops)
                    if (d.Type != BoxType.Container) return null;
            var bound = new DistanceBound(s);
            return bound.targets.Count == 0 && !bound.deliver ? null : bound;
        }

        public int Estimate(GameState s)
        {
            int h = 0;
            foreach (var t in targets)
            {
                if (GoalRules.IsCovered(s, t)) continue;
                int best = int.MaxValue;
                foreach (var b in s.Boxes)
                {
                    if (b.Type == BoxType.Container || b.Symbol > 0 || b.Locked || (deliver && b.Delivery >= 0)) continue;
                    if (GoalRules.IsGoalTarget(s, b.Position)) continue; // already filling a target
                    best = Math.Min(best, b.Position.ManhattanTo(t));
                }
                if (best == int.MaxValue) return -1;
                h += best;
            }
            if (deliver)
                foreach (var b in s.Boxes)
                {
                    if (b.Delivery < 0 || b.Locked) continue;
                    var cells = areas[s.Deliveries[b.Delivery].Area];
                    int best = int.MaxValue;
                    foreach (var c in cells) best = Math.Min(best, b.Position.ManhattanTo(c));
                    if (best != int.MaxValue) h += best;
                }
            return h;
        }
    }
}
