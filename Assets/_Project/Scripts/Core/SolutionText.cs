using System.Collections.Generic;
using System.Text;

namespace CrateBoss.Core
{
    /// <summary>
    /// The "solution" text of a level file (Ana Belge §6): U, D, L, R for the worker; a car number followed by u, d, l or r
    /// for sending that parked car ("3r" = car 3 to the right).
    /// </summary>
    public static class SolutionText
    {
        public static string Write(IReadOnlyList<PlayerAction> actions)
        {
            var sb = new StringBuilder(actions.Count);
            foreach (var a in actions) sb.Append(a.ToString());
            return sb.ToString();
        }

        /// <summary>Null when the text is empty or has a character that is not part of the format.</summary>
        public static List<PlayerAction> Parse(string text)
        {
            if (string.IsNullOrEmpty(text)) return null;
            var list = new List<PlayerAction>(text.Length);
            int car = -1;
            foreach (char ch in text)
            {
                if (ch >= '0' && ch <= '9') { car = (car < 0 ? 0 : car * 10) + (ch - '0'); continue; }
                if (!TryDirection(char.ToUpperInvariant(ch), out var dir)) return null;
                bool lower = ch >= 'a' && ch <= 'z';
                if (lower != car >= 0) return null; // a car number needs a small letter, the worker a capital
                list.Add(car >= 0 ? PlayerAction.SendCar(car, dir) : PlayerAction.Walk(dir));
                car = -1;
            }
            return car >= 0 ? null : list;
        }

        private static bool TryDirection(char c, out Direction dir)
        {
            switch (c)
            {
                case 'U': dir = Direction.Up; return true;
                case 'D': dir = Direction.Down; return true;
                case 'L': dir = Direction.Left; return true;
                case 'R': dir = Direction.Right; return true;
                default: dir = Direction.Up; return false;
            }
        }
    }
}
