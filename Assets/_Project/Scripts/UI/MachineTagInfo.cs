namespace CrateBoss.UI
{
    /// <summary>What a HUD chip or floor badge shows for one machine: its icon and letter, moves left (-1 = none), and whether it is held up.</summary>
    public readonly struct MachineTagInfo
    {
        public readonly string Letter;
        public readonly int Counter;
        public readonly bool Waiting;
        public readonly IconKind Icon;

        public MachineTagInfo(string letter, int counter, bool waiting, IconKind icon = IconKind.Crane)
        {
            Icon = icon;
            Letter = letter;
            Counter = counter;
            Waiting = waiting;
        }
    }
}
