using System.Collections;
using CrateBoss.Core;
using UnityEngine;

namespace CrateBoss.View
{
    /// <summary>What the board needs from the player character, the worker (WorkerView, Ana Belge §19.3).</summary>
    public interface IPlayerView
    {
        Transform transform { get; }
        GameObject gameObject { get; }

        void Build();
        /// <summary>A crane drop is close: the character shows it is worried.</summary>
        bool Worried { get; set; }
        void Face(Direction dir);
        /// <summary>Turns the head toward a point (null = look ahead).</summary>
        void Watch(Vector3? worldTarget);
        IEnumerator Walk(Vector3 target, Direction dir, float duration);
        IEnumerator Push(Vector3 target, Direction dir, float duration, bool heavy);
        IEnumerator Bump(Direction dir, float duration = 0.15f);
        IEnumerator Celebrate();
        IEnumerator Startle();
        /// <summary>A box just landed on a target.</summary>
        void Cheer();
        /// <summary>A hint was used.</summary>
        void Think();
    }
}
