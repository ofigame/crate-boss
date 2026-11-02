using System.Collections.Generic;

namespace CrateBoss.Solver
{
    /// <summary>Everything the quality filters measured for one level (Ana Belge §10.5).</summary>
    public sealed class QualityResult
    {
        public LevelMetrics Metrics;
        /// <summary>Crane tolerance in wasted moves; CraneTolerance.NoCrane without machines.</summary>
        public int Tolerance = CraneTolerance.NoCrane;
        /// <summary>ScoreWithTraps plus the tolerance term 4·max(0, 5 − tolerance) on machine levels.</summary>
        public double Score;
        /// <summary>True when the level can be won without ever pushing a box away from the targets (bad).</summary>
        public bool MonotoneSolvable;
        /// <summary>Without its machines: solved, and in how many moves (-1 = not solved).</summary>
        public int MovesWithoutMachines = -1;
        /// <summary>False when the machines change nothing (same shortest solution without them and tolerance ≥ 8).</summary>
        public bool MachinesMatter = true;
        public bool HasMachines;
        public int YardWidth, YardHeight;
        /// <summary>The shortest solution holds a crane up with a box at least once ("kilitleme").</summary>
        public bool UsesLock;
        /// <summary>A crane load fills a hole during the shortest solution (the machine helps).</summary>
        public bool MachineFillsHole;
        /// <summary>Readable reasons, empty when every filter passed.</summary>
        public readonly List<string> Failures = new List<string>();

        public bool Passed => Failures.Count == 0;
    }
}
