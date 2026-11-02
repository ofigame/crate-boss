using System.Linq;
using System.Text.RegularExpressions;
using CrateBoss.Localization;
using CrateBoss.Meta;
using NUnit.Framework;
using UnityEngine;

namespace CrateBoss.Tests
{
    /// <summary>The string table and Arabic shaping (Ana Belge §18, Ana Belge §17 language note and §10 hidden level count).</summary>
    public class LocalizationTests
    {
        private static LocTable Table => LocTable.Parse(Resources.Load<TextAsset>("Localization/strings").text);

        [Test]
        public void TableHasTheNineLanguagesInOrder()
        {
            CollectionAssert.AreEqual(Loc.Languages.Select(l => l.Code).ToArray(), Table.Languages.ToArray());
        }

        [Test]
        public void EveryKeyIsWrittenInEveryLanguage()
        {
            var table = Table;
            var missing = table.Keys.SelectMany(k => table.Languages.Where(l => table.Get(k, l) == null).Select(l => $"{k}/{l}")).ToList();
            Assert.IsEmpty(missing, string.Join(", ", missing));
        }

        [Test]
        public void PlaceholdersMatchTheTurkishSource()
        {
            var table = Table;
            foreach (var key in table.Keys)
            {
                var source = Placeholders(table.Get(key, Loc.Source));
                foreach (var language in table.Languages)
                    CollectionAssert.AreEquivalent(source, Placeholders(table.Get(key, language)), $"{key}/{language}");
            }
        }

        [Test]
        public void NoTextShowsALevelNumber()
        {
            // Ana Belge §21: no screen text names a level number ("Bölüm 12", "Level {0}").
            var table = Table;
            var pattern = new Regex(@"(Bölüm|Level|Nivel|Livello|Niveau|Stufe|レベル|레벨)\s*(\d|\{)", RegexOptions.IgnoreCase);
            foreach (var key in table.Keys)
                foreach (var language in table.Languages)
                    Assert.IsFalse(pattern.IsMatch(table.Get(key, language) ?? ""), $"{key}/{language}");
        }

        [Test]
        public void LevelNamesMatchTheLevelFiles()
        {
            var table = Table;
            foreach (var asset in Resources.LoadAll<TextAsset>("Levels"))
            {
                var data = JsonUtility.FromJson<CrateBoss.Levels.LevelData>(asset.text);
                string key = $"level.{data.id}.name";
                if (table.Has(key)) Assert.AreEqual(data.name, table.Get(key, "tr"), key);
            }
        }

        [Test]
        public void MissingCellsFallBackToEnglish()
        {
            var table = LocTable.Parse("key\ttr\ten\tde\nhi\tMerhaba\tHello\t\n");
            Assert.AreEqual("Hello", table.Get("hi", "en"));
            Assert.IsNull(table.Get("hi", "de"));
            Assert.IsNull(table.Get("nope", "en"));
        }

        [Test]
        public void ArabicLettersJoin()
        {
            // "بيت" (house): initial beh, medial yeh, final teh.
            Assert.AreEqual("ﺑﻴﺖ", ArabicShaper.Join("بيت"));
            // Alef never joins forward: "ابا" = isolated alef, initial beh, final alef.
            Assert.AreEqual("ﺍﺑﺎ", ArabicShaper.Join("ابا"));
        }

        [Test]
        public void LamAlefBecomesOneLigature()
        {
            Assert.AreEqual("ﻻ", ArabicShaper.Join("لا"));
            // After a joining letter the final form: "بلا".
            Assert.AreEqual("ﺑﻼ", ArabicShaper.Join("بلا"));
        }

        [Test]
        public void ArabicLineIsReversedButNumbersAndLatinKeepTheirOrder()
        {
            string line = ArabicShaper.ShapeLine("بت +30 Crate Boss");
            StringAssert.Contains("+30", line);
            StringAssert.Contains("Crate Boss", line);
            // The Arabic word comes last in visual order (it is first in reading order, on the right).
            Assert.AreEqual('ﺖ', line[line.Length - 2]);
            Assert.AreEqual('ﺑ', line[line.Length - 1]);
        }

        [Test]
        public void TextWithoutArabicIsUntouched()
        {
            Assert.AreEqual("Hamle 3/5", ArabicShaper.ShapeLines("Hamle 3/5"));
            Assert.AreEqual("日本語", ArabicShaper.ShapeLines("日本語"));
        }

        [Test]
        public void SystemLanguageMapsToASupportedOneOrEnglish()
        {
            Assert.AreEqual("tr", Loc.FromSystem(SystemLanguage.Turkish));
            Assert.AreEqual("ja", Loc.FromSystem(SystemLanguage.Japanese));
            Assert.AreEqual("en", Loc.FromSystem(SystemLanguage.Russian));
        }

        private static string[] Placeholders(string text) =>
            Regex.Matches(text ?? "", @"\{\d+\}").Cast<Match>().Select(m => m.Value).OrderBy(v => v).ToArray();
    }
}
