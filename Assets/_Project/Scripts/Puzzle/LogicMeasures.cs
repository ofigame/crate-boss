namespace CrateBoss.Puzzle
{
    /// <summary>How much thinking a level asks for (owner 2026-11-01 brief §2, "MANTIK ÖLÇÜLERİ"; higher = harder).</summary>
    public sealed class LogicMeasures
    {
        /// <summary>Pushes that take a crate away from its goal, or put an in-the-way crate back on a cell it left.</summary>
        public int ReversePushes;
        /// <summary>Moments a crate has to be moved aside for another and then moved again (or the goal crate waits for it).</summary>
        public int Preparations;
        /// <summary>Pushes on the way that look possible but leave the level unsolvable (checked from the first positions).</summary>
        public int TrapPushes;
        /// <summary>Average pushes a trapped player can still make before nothing helps (0 = seen at once).</summary>
        public double TrapDelay;
        /// <summary>A crate the solution moves later can be moved now in a way that loses: the order matters.</summary>
        public bool OrderMatters;
        /// <summary>Different push solutions found (about; 1 = only one way).</summary>
        public int Solutions;
        /// <summary>A player who only pushes goal crates closer to their goal can still win.</summary>
        public bool HastySolves;
        /// <summary>Mechanics of the level the solution does not need (crane, colour crates): decoration.</summary>
        public string DecorativeMechanics = "";
        /// <summary>A gap the right crate goes into while another crate could go there and lose.</summary>
        public int WrongGapChoices;
        /// <summary>Colour triples whose last crate needs another crate moved first or comes the long way.</summary>
        public int IndirectTriples;
        /// <summary>Colour crates pushed in a losing order (the order of the triples matters).</summary>
        public bool TripleOrderMatters;
        /// <summary>Longest chain of cars that must leave before another can.</summary>
        public int CarChain;
        /// <summary>Cars a crate shuts in, or crates a car shuts in.</summary>
        public int CarCrateLocks;
        /// <summary>Positions of the solution the trap search looked at (the rest was too far in to check).</summary>
        public int CheckedSteps;
    }
}
