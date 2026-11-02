using System;

namespace CrateBoss.Core
{
    /// <summary>128-bit fingerprint of a position (see GameState.GetKeyHash); cheap to store and compare.</summary>
    public readonly struct StateHash : IEquatable<StateHash>
    {
        public readonly ulong A, B;

        public StateHash(ulong a, ulong b)
        {
            A = a;
            B = b;
        }

        public bool Equals(StateHash other) => A == other.A && B == other.B;
        public override bool Equals(object obj) => obj is StateHash h && Equals(h);
        public override int GetHashCode() => (int)(A ^ (A >> 32) ^ B);
    }
}
