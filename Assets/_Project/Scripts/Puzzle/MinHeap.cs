using System.Collections.Generic;

namespace CrateBoss.Puzzle
{
    /// <summary>A small binary min-heap by integer priority (Unity's .NET has no PriorityQueue); equal priorities come out first-in first-out.</summary>
    public sealed class MinHeap<T>
    {
        private readonly List<(int priority, long order, T item)> items = new List<(int, long, T)>();
        private long counter;

        public int Count => items.Count;

        public void Push(T item, int priority)
        {
            items.Add((priority, counter++, item));
            int i = items.Count - 1;
            while (i > 0)
            {
                int p = (i - 1) / 2;
                if (!Less(items[i], items[p])) break;
                (items[i], items[p]) = (items[p], items[i]);
                i = p;
            }
        }

        public T Pop()
        {
            var top = items[0].item;
            int last = items.Count - 1;
            items[0] = items[last];
            items.RemoveAt(last);
            int i = 0;
            while (true)
            {
                int l = 2 * i + 1, r = l + 1, m = i;
                if (l < items.Count && Less(items[l], items[m])) m = l;
                if (r < items.Count && Less(items[r], items[m])) m = r;
                if (m == i) break;
                (items[i], items[m]) = (items[m], items[i]);
                i = m;
            }
            return top;
        }

        private static bool Less((int priority, long order, T item) a, (int priority, long order, T item) b) =>
            a.priority < b.priority || a.priority == b.priority && a.order < b.order;
    }
}
