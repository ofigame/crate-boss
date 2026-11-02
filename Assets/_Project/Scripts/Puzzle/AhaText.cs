using System.Collections.Generic;

namespace CrateBoss.Puzzle
{
    /// <summary>The level's "aha" moment in one or two Turkish sentences, from what its measure says needs thinking.</summary>
    public static class AhaText
    {
        public static string For(DifficultyReport r)
        {
            var l = r.Logic;
            var parts = new List<string>();
            if (l.Preparations > 0) parts.Add($"Yoldaki kasalardan {(l.Preparations > 1 ? "bazılarını" : "birini")} önce geçici olarak kenara çekmen gerekiyor; sonra geri dönüp asıl kasayı geçireceksin.");
            if (l.ReversePushes > 0) parts.Add($"{l.ReversePushes} itme yanlış yöne gibi görünüyor: kasayı hedefinden uzaklaştırmadan yol açılmıyor.");
            if (l.WrongGapChoices > 0) parts.Add("Aynı boşluğa birden fazla kasa sığıyor ama yalnızca biri doğru; yanlışı seçersen yol kilitleniyor.");
            if (l.TripleOrderMatters || l.IndirectTriples > 0) parts.Add("Şeker üçlülerinin sırası önemli: eksik kasa ancak dolambaçlı yoldan geliyor, acele edersen renkler kilitleniyor.");
            if (l.CarChain >= 2) parts.Add($"Arabalar zincirli: kamyonun yolundaki araba ancak {l.CarChain - 1} araba önce çıkınca gidebiliyor.");
            if (l.OrderMatters && parts.Count == 0) parts.Add("Kasaların sırası önemli: sonradan itilmesi gereken kasayı erken itersen bölüm kilitleniyor.");
            if (l.TrapPushes > 0 && parts.Count < 2) parts.Add($"Doğru görünen {l.TrapPushes} itme bölümü kilitliyor; hamle yapmadan önce sonrasını düşün.");
            if (parts.Count == 0) parts.Add("Bu dizilimde belirgin bir aha anı yok.");
            return parts.Count > 1 ? parts[0] + " " + parts[1] : parts[0];
        }
    }
}
