using System.Collections.Generic;
using System.Text;

namespace CrateBoss.Localization
{
    /// <summary>
    /// Arabic for Unity's plain Text (Ana Belge §18): the letters are joined (Arabic Presentation Forms-B) and each line is
    /// put in visual order, right to left. Runs of Latin letters and digits ("Crate Boss", "+30", "3/5") keep their own
    /// left-to-right order inside the line. Used instead of RTLTMPro because the game's UI is plain Text, not TextMeshPro.
    /// </summary>
    public static class ArabicShaper
    {
        // Isolated, final, initial, medial forms per letter; 0 = the letter never joins forward (right-joining only).
        private static readonly Dictionary<char, char[]> Forms = new Dictionary<char, char[]>
        {
            ['ء'] = new[] { 'ﺀ', 'ﺀ', '\0', '\0' },
            ['آ'] = new[] { 'ﺁ', 'ﺂ', '\0', '\0' },
            ['أ'] = new[] { 'ﺃ', 'ﺄ', '\0', '\0' },
            ['ؤ'] = new[] { 'ﺅ', 'ﺆ', '\0', '\0' },
            ['إ'] = new[] { 'ﺇ', 'ﺈ', '\0', '\0' },
            ['ئ'] = new[] { 'ﺉ', 'ﺊ', 'ﺋ', 'ﺌ' },
            ['ا'] = new[] { 'ﺍ', 'ﺎ', '\0', '\0' },
            ['ب'] = new[] { 'ﺏ', 'ﺐ', 'ﺑ', 'ﺒ' },
            ['ة'] = new[] { 'ﺓ', 'ﺔ', '\0', '\0' },
            ['ت'] = new[] { 'ﺕ', 'ﺖ', 'ﺗ', 'ﺘ' },
            ['ث'] = new[] { 'ﺙ', 'ﺚ', 'ﺛ', 'ﺜ' },
            ['ج'] = new[] { 'ﺝ', 'ﺞ', 'ﺟ', 'ﺠ' },
            ['ح'] = new[] { 'ﺡ', 'ﺢ', 'ﺣ', 'ﺤ' },
            ['خ'] = new[] { 'ﺥ', 'ﺦ', 'ﺧ', 'ﺨ' },
            ['د'] = new[] { 'ﺩ', 'ﺪ', '\0', '\0' },
            ['ذ'] = new[] { 'ﺫ', 'ﺬ', '\0', '\0' },
            ['ر'] = new[] { 'ﺭ', 'ﺮ', '\0', '\0' },
            ['ز'] = new[] { 'ﺯ', 'ﺰ', '\0', '\0' },
            ['س'] = new[] { 'ﺱ', 'ﺲ', 'ﺳ', 'ﺴ' },
            ['ش'] = new[] { 'ﺵ', 'ﺶ', 'ﺷ', 'ﺸ' },
            ['ص'] = new[] { 'ﺹ', 'ﺺ', 'ﺻ', 'ﺼ' },
            ['ض'] = new[] { 'ﺽ', 'ﺾ', 'ﺿ', 'ﻀ' },
            ['ط'] = new[] { 'ﻁ', 'ﻂ', 'ﻃ', 'ﻄ' },
            ['ظ'] = new[] { 'ﻅ', 'ﻆ', 'ﻇ', 'ﻈ' },
            ['ع'] = new[] { 'ﻉ', 'ﻊ', 'ﻋ', 'ﻌ' },
            ['غ'] = new[] { 'ﻍ', 'ﻎ', 'ﻏ', 'ﻐ' },
            ['ف'] = new[] { 'ﻑ', 'ﻒ', 'ﻓ', 'ﻔ' },
            ['ق'] = new[] { 'ﻕ', 'ﻖ', 'ﻗ', 'ﻘ' },
            ['ك'] = new[] { 'ﻙ', 'ﻚ', 'ﻛ', 'ﻜ' },
            ['ل'] = new[] { 'ﻝ', 'ﻞ', 'ﻟ', 'ﻠ' },
            ['م'] = new[] { 'ﻡ', 'ﻢ', 'ﻣ', 'ﻤ' },
            ['ن'] = new[] { 'ﻥ', 'ﻦ', 'ﻧ', 'ﻨ' },
            ['ه'] = new[] { 'ﻩ', 'ﻪ', 'ﻫ', 'ﻬ' },
            ['و'] = new[] { 'ﻭ', 'ﻮ', '\0', '\0' },
            ['ى'] = new[] { 'ﻯ', 'ﻰ', '\0', '\0' },
            ['ي'] = new[] { 'ﻱ', 'ﻲ', 'ﻳ', 'ﻴ' },
        };

        // Lam + alef ligatures: isolated, final.
        private static readonly Dictionary<char, char[]> LamAlef = new Dictionary<char, char[]>
        {
            ['آ'] = new[] { 'ﻵ', 'ﻶ' },
            ['أ'] = new[] { 'ﻷ', 'ﻸ' },
            ['إ'] = new[] { 'ﻹ', 'ﻺ' },
            ['ا'] = new[] { 'ﻻ', 'ﻼ' },
        };

        private const char Lam = 'ل', Tatweel = 'ـ';

        public static bool HasArabic(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            foreach (char c in text) if (c >= '؀' && c <= 'ۿ') return true;
            return false;
        }

        /// <summary>Every line shaped and reversed on its own; text without Arabic letters is returned as it is.</summary>
        public static string ShapeLines(string text)
        {
            if (!HasArabic(text)) return text;
            var lines = text.Split('\n');
            for (int i = 0; i < lines.Length; i++) lines[i] = ShapeLine(lines[i]);
            return string.Join("\n", lines);
        }

        /// <summary>One line: joined letters, then visual order (right to left, Latin and digit runs kept left to right).</summary>
        public static string ShapeLine(string line) => HasArabic(line) ? Visual(Join(line)) : line;

        private static bool IsMark(char c) => c >= 'ً' && c <= 'ٟ' || c == 'ٰ';
        private static bool JoinsForward(char c) => c == Tatweel || Forms.TryGetValue(c, out var f) && f[2] != '\0';
        private static bool JoinsBack(char c) => c == Tatweel || Forms.ContainsKey(c) && c != 'ء';

        private static char Neighbour(string s, int i, int step)
        {
            for (int j = i + step; j >= 0 && j < s.Length; j += step)
                if (!IsMark(s[j])) return s[j];
            return '\0';
        }

        /// <summary>Contextual forms (logical order kept).</summary>
        public static string Join(string s)
        {
            var sb = new StringBuilder(s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (!Forms.TryGetValue(c, out var forms)) { sb.Append(c); continue; }
                char prev = Neighbour(s, i, -1), next = Neighbour(s, i, 1);
                bool back = JoinsForward(prev) && JoinsBack(c);
                if (c == Lam && next != '\0' && LamAlef.TryGetValue(next, out var lig))
                {
                    sb.Append(back ? lig[1] : lig[0]);
                    // Skip the alef (and any marks in between).
                    int j = i + 1;
                    while (j < s.Length && IsMark(s[j])) j++;
                    i = j;
                    continue;
                }
                bool forward = forms[2] != '\0' && JoinsBack(next);
                sb.Append(back && forward ? forms[3] : back ? forms[1] : forward ? forms[2] : forms[0]);
            }
            return sb.ToString();
        }

        private static bool IsLtr(char c) => c < 0x0600 && char.IsLetterOrDigit(c) || c >= '０' && c <= 'ｚ';

        private static char Mirror(char c)
        {
            switch (c)
            {
                case '(': return ')';
                case ')': return '(';
                case '[': return ']';
                case ']': return '[';
                case '<': return '>';
                case '>': return '<';
                case '«': return '»';
                case '»': return '«';
                default: return c;
            }
        }

        /// <summary>
        /// Visual order of a right-to-left line. A left-to-right run starts at a Latin letter or digit (a "+" or "-" right
        /// before a digit joins it) and keeps the neutrals between such characters ("Crate Boss", "3/5", "12.5"); a "%"
        /// right after it stays with it.
        /// </summary>
        public static string Visual(string s)
        {
            var runs = new List<(string text, bool ltr)>();
            int i = 0;
            while (i < s.Length)
            {
                bool sign = (s[i] == '+' || s[i] == '-') && i + 1 < s.Length && char.IsDigit(s[i + 1]);
                if (IsLtr(s[i]) || sign)
                {
                    int end = i + 1;
                    int lastStrong = i;
                    while (end < s.Length)
                    {
                        if (IsLtr(s[end])) { lastStrong = end; end++; continue; }
                        if (s[end] >= '؀' && s[end] <= 'ۿ' || s[end] >= 'ﭐ') break;
                        end++;
                    }
                    int stop = lastStrong + 1;
                    if (stop < s.Length && s[stop] == '%') stop++;
                    runs.Add((s.Substring(i, stop - i), true));
                    i = stop;
                }
                else
                {
                    int end = i + 1;
                    while (end < s.Length && !IsLtr(s[end]) && !((s[end] == '+' || s[end] == '-') && end + 1 < s.Length && char.IsDigit(s[end + 1]))) end++;
                    runs.Add((s.Substring(i, end - i), false));
                    i = end;
                }
            }
            var sb = new StringBuilder(s.Length);
            for (int r = runs.Count - 1; r >= 0; r--)
            {
                var (text, ltr) = runs[r];
                if (ltr) { sb.Append(text); continue; }
                for (int k = text.Length - 1; k >= 0; k--) sb.Append(Mirror(text[k]));
            }
            return sb.ToString();
        }
    }
}
