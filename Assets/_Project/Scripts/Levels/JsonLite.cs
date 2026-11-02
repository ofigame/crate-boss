using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace CrateBoss.Levels
{
    /// <summary>
    /// Minimal JSON reader for the parts JsonUtility cannot read (nested arrays such as a truck bay's
    /// "path": [[1,6],[2,6]] and null values). Objects become Dictionary&lt;string, object&gt;, arrays List&lt;object&gt;,
    /// numbers double, plus string, bool and null.
    /// </summary>
    public sealed class JsonLite
    {
        private readonly string text;
        private int pos;

        private JsonLite(string text) => this.text = text;

        /// <exception cref="LevelFormatException">The text is not valid JSON.</exception>
        public static object Parse(string text)
        {
            var reader = new JsonLite(text);
            var value = reader.ReadValue();
            reader.SkipSpace();
            if (reader.pos != text.Length) throw reader.Error("unexpected text after the end");
            return value;
        }

        private object ReadValue()
        {
            SkipSpace();
            if (pos >= text.Length) throw Error("unexpected end");
            char c = text[pos];
            switch (c)
            {
                case '{': return ReadObject();
                case '[': return ReadArray();
                case '"': return ReadString();
                case 't': Expect("true"); return true;
                case 'f': Expect("false"); return false;
                case 'n': Expect("null"); return null;
                default:
                    if (c == '-' || char.IsDigit(c)) return ReadNumber();
                    throw Error($"unexpected '{c}'");
            }
        }

        private Dictionary<string, object> ReadObject()
        {
            var obj = new Dictionary<string, object>();
            pos++; // {
            SkipSpace();
            if (Peek() == '}') { pos++; return obj; }
            while (true)
            {
                SkipSpace();
                if (Peek() != '"') throw Error("expected a key");
                string key = ReadString();
                SkipSpace();
                if (Peek() != ':') throw Error("expected ':'");
                pos++;
                obj[key] = ReadValue();
                SkipSpace();
                char c = Peek();
                pos++;
                if (c == '}') return obj;
                if (c != ',') throw Error("expected ',' or '}'");
            }
        }

        private List<object> ReadArray()
        {
            var list = new List<object>();
            pos++; // [
            SkipSpace();
            if (Peek() == ']') { pos++; return list; }
            while (true)
            {
                list.Add(ReadValue());
                SkipSpace();
                char c = Peek();
                pos++;
                if (c == ']') return list;
                if (c != ',') throw Error("expected ',' or ']'");
            }
        }

        private string ReadString()
        {
            var sb = new StringBuilder();
            pos++; // opening quote
            while (pos < text.Length)
            {
                char c = text[pos++];
                if (c == '"') return sb.ToString();
                if (c != '\\') { sb.Append(c); continue; }
                if (pos >= text.Length) break;
                char e = text[pos++];
                switch (e)
                {
                    case 'n': sb.Append('\n'); break;
                    case 't': sb.Append('\t'); break;
                    case 'r': sb.Append('\r'); break;
                    case 'b': sb.Append('\b'); break;
                    case 'f': sb.Append('\f'); break;
                    case 'u':
                        if (pos + 4 > text.Length) throw Error("bad \\u escape");
                        sb.Append((char)int.Parse(text.Substring(pos, 4), NumberStyles.HexNumber));
                        pos += 4;
                        break;
                    default: sb.Append(e); break; // \" \\ \/
                }
            }
            throw Error("unterminated string");
        }

        private double ReadNumber()
        {
            int start = pos;
            while (pos < text.Length && "+-0123456789.eE".IndexOf(text[pos]) >= 0) pos++;
            if (!double.TryParse(text.Substring(start, pos - start), NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
                throw Error("bad number");
            return d;
        }

        private void Expect(string word)
        {
            if (string.CompareOrdinal(text, pos, word, 0, word.Length) != 0) throw Error($"expected '{word}'");
            pos += word.Length;
        }

        private char Peek() => pos < text.Length ? text[pos] : '\0';

        private void SkipSpace()
        {
            while (pos < text.Length && char.IsWhiteSpace(text[pos])) pos++;
        }

        private LevelFormatException Error(string what) => new LevelFormatException($"JSON error at character {pos}: {what}.");
    }
}
