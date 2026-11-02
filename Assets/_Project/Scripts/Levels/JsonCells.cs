using System;
using System.Collections.Generic;

namespace CrateBoss.Levels
{
    /// <summary>Reads [x, y] cells and lists of cells from JsonLite trees.</summary>
    public static class JsonCells
    {
        public static int[] Pair(object node, string what)
        {
            if (node is List<object> list && list.Count == 2 && list[0] is double a && list[1] is double b && a == Math.Floor(a) && b == Math.Floor(b))
                return new[] { (int)a, (int)b };
            throw new LevelFormatException($"{what} must be [x, y].");
        }

        public static int[][] List(object node, string what)
        {
            if (!(node is List<object> list)) throw new LevelFormatException($"{what} must be a list of [x, y].");
            var cells = new int[list.Count][];
            for (int i = 0; i < cells.Length; i++) cells[i] = Pair(list[i], $"{what} {i}");
            return cells;
        }

        /// <summary>"goal" may be one name or a list of names.</summary>
        public static string[] Names(object node)
        {
            if (node is string one) return new[] { one };
            if (node is List<object> list)
            {
                var names = new string[list.Count];
                for (int i = 0; i < names.Length; i++) names[i] = list[i] as string ?? throw new LevelFormatException("'goal' entries must be names.");
                return names;
            }
            throw new LevelFormatException("'goal' must be a name or a list of names.");
        }
    }
}
