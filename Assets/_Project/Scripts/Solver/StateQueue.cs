using System.Collections.Generic;
using CrateBoss.Core;

namespace CrateBoss.Solver
{
    /// <summary>
    /// The solver's open list: a plain FIFO queue for breadth-first search, or a binary min-heap on f = moves + bound for A*
    /// (ties go to the older entry, so equal-cost states come out in the order they were found).
    /// </summary>
    public sealed class StateQueue
    {
        private readonly bool ordered;
        private readonly Queue<(GameState, int)> fifo = new Queue<(GameState, int)>();
        private readonly List<(int f, long order, GameState state, int node)> heap = new List<(int, long, GameState, int)>();
        private long counter;

        public StateQueue(bool ordered) => this.ordered = ordered;

        public int Count => ordered ? heap.Count : fifo.Count;

        public void Push(GameState state, int node, int f)
        {
            if (!ordered) { fifo.Enqueue((state, node)); return; }
            heap.Add((f, counter++, state, node));
            int i = heap.Count - 1;
            while (i > 0)
            {
                int p = (i - 1) / 2;
                if (!Less(heap[i], heap[p])) break;
                (heap[i], heap[p]) = (heap[p], heap[i]);
                i = p;
            }
        }

        public (GameState state, int node) Pop()
        {
            if (!ordered) return fifo.Dequeue();
            var top = heap[0];
            var last = heap[heap.Count - 1];
            heap.RemoveAt(heap.Count - 1);
            if (heap.Count > 0)
            {
                heap[0] = last;
                int i = 0;
                while (true)
                {
                    int l = 2 * i + 1, r = l + 1, m = i;
                    if (l < heap.Count && Less(heap[l], heap[m])) m = l;
                    if (r < heap.Count && Less(heap[r], heap[m])) m = r;
                    if (m == i) break;
                    (heap[i], heap[m]) = (heap[m], heap[i]);
                    i = m;
                }
            }
            return (top.state, top.node);
        }

        private static bool Less((int f, long order, GameState, int) a, (int f, long order, GameState, int) b) =>
            a.f < b.f || a.f == b.f && a.order < b.order;
    }
}
