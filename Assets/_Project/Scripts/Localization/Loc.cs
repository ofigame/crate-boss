using System;
using System.Globalization;
using UnityEngine;

namespace CrateBoss.Localization
{
    /// <summary>
    /// The game's texts in nine languages (Ana Belge §18, Ana Belge §17 language note). Turkish is the source, English the fallback.
    /// Every screen text comes from the table by key; level names use "level.&lt;id&gt;.name" with the JSON name as the source.
    /// Choice: device language on first start (English when not supported), then the player's pick, saved on the device.
    /// "-lang xx" on the command line overrides both (screenshots per language).
    /// Numbers stay Western digits in every language (§L.2): formatting uses the invariant culture.
    /// </summary>
    public static class Loc
    {
        public const string Source = "tr", Fallback = "en";
        private const string PrefKey = "crateboss.lang";

        public static readonly GameLanguage[] Languages =
        {
            new GameLanguage { Code = "tr", NativeName = "Türkçe", Culture = "tr-TR" },
            new GameLanguage { Code = "en", NativeName = "English", Culture = "en-US" },
            new GameLanguage { Code = "es", NativeName = "Español", Culture = "es-ES" },
            new GameLanguage { Code = "ar", NativeName = "العربية", Culture = "ar-SA", RightToLeft = true },
            new GameLanguage { Code = "de", NativeName = "Deutsch", Culture = "de-DE" },
            new GameLanguage { Code = "it", NativeName = "Italiano", Culture = "it-IT" },
            new GameLanguage { Code = "fr", NativeName = "Français", Culture = "fr-FR" },
            new GameLanguage { Code = "ja", NativeName = "日本語", Culture = "ja-JP" },
            new GameLanguage { Code = "ko", NativeName = "한국어", Culture = "ko-KR" },
        };

        private static LocTable table;
        private static GameLanguage current;
        private static CultureInfo culture;

        public static event Action Changed;

        public static LocTable Table => table ??= LocTable.Parse(Resources.Load<TextAsset>("Localization/strings")?.text);

        public static GameLanguage Current
        {
            get
            {
                if (current == null) Set(Initial(), false);
                return current;
            }
        }

        public static string Code => Current.Code;
        public static bool IsRtl => Current.RightToLeft;

        /// <summary>Upper case and similar rules of the current language (Turkish: i → İ).</summary>
        public static CultureInfo Culture => culture ??= MakeCulture(Current.Culture);

        public static GameLanguage Find(string code)
        {
            foreach (var l in Languages) if (l.Code == code) return l;
            return null;
        }

        public static void Set(string code, bool save = true)
        {
            var language = Find(code) ?? Find(Fallback);
            bool changed = current != null && current != language;
            current = language;
            culture = null;
            if (save) { PlayerPrefs.SetString(PrefKey, language.Code); PlayerPrefs.Save(); }
            if (changed) Changed?.Invoke();
        }

        /// <summary>The text of <paramref name="key"/>: current language, else English, else Turkish, else the fallback (or the key).</summary>
        public static string T(string key, string fallback = null)
        {
            var t = Table;
            return t.Get(key, Code) ?? t.Get(key, Fallback) ?? t.Get(key, Source) ?? fallback ?? key;
        }

        public static string F(string key, params object[] args) => string.Format(CultureInfo.InvariantCulture, T(key), args);

        /// <summary>A level's name: table "level.&lt;id&gt;.name", else the name written in the level file (the Turkish source).</summary>
        public static string Level(int id, string source)
        {
            if (Code == Source) return source;
            var t = Table;
            string key = $"level.{id}.name";
            return t.Has(key) && t.Get(key, Source) == source ? T(key, source) : source;
        }

        /// <summary>First letter upper case by the current language's rule.</summary>
        public static string Capitalize(string text) =>
            string.IsNullOrEmpty(text) ? text : char.ToUpper(text[0], Culture) + text.Substring(1);

        /// <summary>Text ready for a plain Text component that is not a GameText: Arabic shaped and in visual order.</summary>
        public static string Display(string text) => IsRtl ? ArabicShaper.ShapeLines(text) : text;

        private static string Initial()
        {
            var args = Environment.GetCommandLineArgs();
            int i = Array.IndexOf(args, "-lang");
            if (i >= 0 && i < args.Length - 1 && Find(args[i + 1]) != null) return args[i + 1];
            string saved = PlayerPrefs.GetString(PrefKey, "");
            if (Find(saved) != null) return saved;
            return FromSystem(Application.systemLanguage);
        }

        public static string FromSystem(SystemLanguage language)
        {
            switch (language)
            {
                case SystemLanguage.Turkish: return "tr";
                case SystemLanguage.Spanish: return "es";
                case SystemLanguage.Arabic: return "ar";
                case SystemLanguage.German: return "de";
                case SystemLanguage.Italian: return "it";
                case SystemLanguage.French: return "fr";
                case SystemLanguage.Japanese: return "ja";
                case SystemLanguage.Korean: return "ko";
                default: return Fallback;
            }
        }

        private static CultureInfo MakeCulture(string name)
        {
            try { return new CultureInfo(name); }
            catch (CultureNotFoundException) { return CultureInfo.InvariantCulture; }
        }
    }
}
