using System.Collections.Generic;
using System.Text;

namespace CrateBoss.Puzzle
{
    /// <summary>Docs/ZORLUK_TEST_RAPORU.md (owner 2026-11-01 brief §7): one row per level, before → after.</summary>
    public static class DifficultyTable
    {
        public sealed class Row
        {
            public DifficultyReport Before, After;
            public string Aha = "";
        }

        public static string Markdown(IEnumerable<Row> rows, string intro)
        {
            var sb = new StringBuilder();
            sb.Append(intro).Append("\n\n");
            sb.Append("| Bölüm | Alan | Mantık önce → sonra | Zahmet önce → sonra | Kolay kapı önce → sonra | Patlatma yoğunluğu önce → sonra | Durum | Aha anı |\n");
            sb.Append("|---|---|---|---|---|---|---|---|\n");
            foreach (var r in rows)
            {
                var b = r.Before;
                var a = r.After ?? r.Before;
                string Candy(DifficultyReport x) => x.Candy == null || x.Candy.Candy == 0 ? "-" : $"%{x.Candy.Share * 100:0}";
                string status = a.Easy ? "KOLAY (" + string.Join("; ", a.Problems) + ")" : "UYGUN";
                sb.Append($"| {b.Number} {Escape(b.Name)} | {b.Width - 2}×{b.Height - 2} | {b.LogicScore:0.0} → {a.LogicScore:0.0} | {b.EffortScore:0.0} → {a.EffortScore:0.0} | ");
                sb.Append($"{b.Doors.EasyDoors} → {a.Doors.EasyDoors} | {Candy(b)} → {Candy(a)} | {Escape(status)} | {Escape(r.Aha)} |\n");
            }
            return sb.ToString();
        }

        /// <summary>Columns of Docs/ADAYLAR.tsv after the file name (the last one is the "aha" line).</summary>
        public const string TsvHeader = "no\tad\ten×boy\tçözülür\titme\tters\thazırlık\ttuzak\tgecikme\tsıra\tçözüm\taceleci\tsüs\tboşlukYanlış\tdolaylıÜçlü\tarabaZincir\tkapı\tkolayKapı\tilk3\tyürüme\tdüz\titme/kasa\tgidipGelme\tpatlatmaPay\tyalnız\tküme\tmantık\tzahmet\tdurum";

        public static string TsvRow(DifficultyReport r)
        {
            var l = r.Logic; var e = r.Effort; var d = r.Doors; var c = r.Candy;
            return string.Join("	", r.Number, r.Name, $"{r.Width - 2}×{r.Height - 2}", r.Solvable ? "evet" : "HAYIR", r.Pushes, l.ReversePushes, l.Preparations,
                l.TrapPushes, l.TrapDelay.ToString("0.0"), l.OrderMatters ? "var" : "yok", l.Solutions, l.HastySolves ? "çözer" : "çözemez", l.DecorativeMechanics,
                l.WrongGapChoices, l.IndirectTriples, l.CarChain, d.Doors, d.EasyDoors, (d.OpenedByFirstThree * 100).ToString("0"),
                (e.WalkRatio * 100).ToString("0"), e.StraightRun, e.PushesPerCrate.ToString("0.0"), e.EndToEndTrips,
                c == null ? "-" : (c.Share * 100).ToString("0"), c == null ? "-" : c.Lonely.ToString(), c == null ? "-" : c.Clusters.ToString(),
                r.LogicScore.ToString("0.0"), r.EffortScore.ToString("0.0"), r.Easy ? "KOLAY: " + string.Join("; ", r.Problems) : "UYGUN");
        }

        private static string Escape(string s) => (s ?? "").Replace("|", "/");
    }
}
