using System.Collections.Generic;

namespace CrateBoss.Core
{
    /// <summary>A level's crane: it drops the listed boxes in order, one every <see cref="Every"/> successful moves. Never changes during play.</summary>
    public sealed class CraneDefinition
    {
        /// <summary>Letter shown on the tower and the floor marks: "A", "B" or "C".</summary>
        public string Id { get; }
        public CraneSide Side { get; }
        public int Every { get; }
        public IReadOnlyList<CraneDrop> Drops { get; }

        public CraneDefinition(int every, IReadOnlyList<CraneDrop> drops) : this("A", CraneSide.Left, every, drops) { }

        public CraneDefinition(string id, CraneSide side, int every, IReadOnlyList<CraneDrop> drops)
        {
            Id = id;
            Side = side;
            Every = every;
            Drops = drops;
        }
    }
}
