# Crate Boss — Oyunun tanıtımı

*Crate Boss: Box Push Puzzle* · Ofigame · Android ve iOS, dikey ekran · 13+ · 9 dil. Kurallar ve tasarımın tek kaynağı: [`CRATE_BOSS_ANA_BELGE.md`](CRATE_BOSS_ANA_BELGE.md). Bu sayfa oyunun bugünkü hâlini anlatır (01.11.2026).

## Oyun

Oyuncak bir şehirde, baretli ve yelekli bir işçiyle kareli sahalarda kasa itersin. Her bölümün tek bir görevi var:

- **Hedef:** Kasaları yuvalara koy.
- **Görev kasası (A → B):** Görev kasasını A işaretinden B teslim alanına götür. B'ye girince kilitlenir. Bazı bölümlerde önce ara durağa (M) uğraması gerekir; birden çok görev kasası olabilir; ağır görev kasası yalnız önüne konan bir kasayla itilir.
- **Kamyona yükle:** Otoparktaki arabaları doğru sırayla gönder; kamyonun yolu açılınca geri geri yanaşır, kasaları yükle.

Bulmacayı zorlaştıranlar:

| Ne | Kural |
|---|---|
| Boşluk birleştirme | Kasalarla tıkalı bir bantta dağınık boşlukları kaydırıp görev kasasına koridor açarsın. |
| Kule vinç | Belli hamlelerde sahaya yük bırakır: sabit konteyner, kasa, renkli kasa, görev kasası. Bırakma karesinde biri durursa bekler (kilitleme). |
| Kasa patlatma | Aynı renkten 3 kasa yatay ya da dikey yan yana gelince iner; kare boşalır. 4'lü +5, 5'li +10 CB Coin. |
| Otopark | Arabalar yalnızca uzun ekseni boyunca gider; yol açıksa çıkar, değilse çarpıp geri döner (çarpışma yıldızdan götürür). |
| Ağır kasa | Tek başına itilmez; önüne konan kasayla birlikte bir kare gider. |
| Çukur | İçine düşen kasa çukuru doldurur, kare yürünür olur. |
| Buz kasa | İtilince bir engele kadar kayar. |

Geri Al sınırsız ve ücretsiz, Yeniden Başlat her zaman ücretsiz. Kaybetmek yok.

**Yıldızlar:** en iyi çözüm kadar hamle ★★★, %25 fazlasına kadar ★★, gerisi ★. Otoparkta ★★★ için hiç çarpışma olmamalı.

**İpucu:** Bulunduğun konumdan bir sonraki hamleyi gösterir (hangi kasa hangi yöne ya da hangi araba). Takıldıysan kaç hamle geri alman gerektiğini söyler. Günde bir ücretsiz ipucu (en fazla 3); bitince 50 CB Coin ya da reklam. Çözümün tamamı 300 CB Coin.

## Kontroller

- Ekranda kaydır: işçi o yönde yürür ya da önündeki kasayı iter. Kaydırma, çapraz kamera açısına göre en yakın ızgara yönüne çevrilir.
- Bir arabanın üstünden başlayarak onun ekseni boyunca kaydır: araba gönderilir.
- Parmağı basılı tutup sürükle: görüntü kayar (büyük sahalar).

## Rütbeler

Her 25 bölümde bir terfi: Öğrenci → Stajyer → Çırak → Kalfa → Usta (sonra Şef, Mühendis, Kıdemli Mühendis). Bir sonraki rütbe, o rütbenin son bölümü bitip yıldızların %60'ı (75'te 45) toplanınca açılır. Terfide kutlama, ücretsiz terfi kasası ve yeni bir arazi gelir. Bölüm numarası hiçbir yerde görünmez.

## Boss'un arazisi (ana sayfa)

Her bölümü ilk bitirişinde bir kamyon teslimat kasasını arazine getirir. Kasayı 30 CB Coin ile (ilki ücretsiz, ya da reklamla) açarsın; içinden çıkan 3 eşyadan birini seçer, arazine yerleştirirsin.

- **Eşya türleri:** ev, araç, havuz, kort, ağaç, bank ve benzerleri.
- **Yerleştirme kuralları:** Binanın kapısı yola ya da bir patikaya bakmalı, araçlar patikada durur, tekneler suda.
- **Patikalar:** ücretsiz ve sınırsız.

Rütbe atladıkça yeni araziler açılır: başlangıç arsası, bahçe, göl kenarı, sahil, şehir merkezi.

## CB Coin

Oyunun tek parası; gerçek parayla satın alınmaz.

- **Kazanma:**
  - Bölümü ilk bitirişte ★ 10, ★★ 18, ★★★ 30.
  - Tekrar oynayıp daha iyi yıldız alırsan yalnız fark.
  - Bölüm sonunda reklamla ×2.
  - Ana sayfada reklamla +30 (günde 3).
  - Günlük giriş serisi: 10, 15, 20, 25, 30, 40, 60.
- **Harcama:**
  - Kasa açma 30.
  - İpucu 50.
  - Çözümü izle 300.
  - Gardırop 100–500.

## Reklam

İlk 8 bölüm reklamsız. Geçiş reklamı yalnız bölüm bitince "Devam"dan sonra; en az 3 bölümde bir ve 120 saniye arayla, oturumda en fazla 6; ödüllü reklamdan sonraki 5 dakika gösterilmez. Banner yok, satın alma yok. Şimdilik test reklamları.

## Görünüm

"Parking Jam" tarzı oyuncak şehir: açık gri zemin, kıvrımlı koyu yollar, beyaz çizgili otoparklar, şeker renkli arabalar. Saha sarı oyuncak bariyerle çevrili; iç duvarlar sabit kasa. Kasalar oyun sahibinin modelleri (1. seviye kasalar; diğer rütbelerin kasaları geldikçe değişecek).
