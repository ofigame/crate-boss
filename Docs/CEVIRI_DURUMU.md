# Çeviri durumu

Ana Belge §18. Tablo: `Assets/_Project/Resources/Localization/strings.txt` (sekmeyle ayrılmış: anahtar, tr, en, es, ar, de, it, fr, ja, ko). Toplam **163 anahtar**.

| Dil | Kod | Durum | Kim yazdı |
|---|---|---|---|
| Türkçe | tr | **Kaynak**, kesin | Claude (oyun sahibi onaylar) |
| İngilizce | en | **Taslak** (yedek dil) | Claude |
| İspanyolca | es | **Taslak** | Claude |
| Arapça | ar | **Taslak** | Claude |
| Almanca | de | **Taslak** | Claude |
| İtalyanca | it | **Taslak** | Claude |
| Fransızca | fr | **Taslak** | Claude |
| Japonca | ja | **Taslak** | Claude |
| Korece | ko | **Taslak** | Claude |

**Her anahtar tr dışındaki 8 dilde taslaktır.** Mağazaya çıkmadan önce ana dili o dil olan biri bütün sütunu kontrol etmeli. Kontrol edilen dilin satırını burada "Kontrol edildi (ad, tarih)" diye işaretleyin.

## Gruplar

| Grup | Anahtar öneki | Not |
|---|---|---|
| Genel butonlar | `ui.*` | En fazla 12 karakter (§L.5) |
| Oyun ekranı | `hud.*`, `transition` | |
| Görev kartı | `goal.*` | Parçalar `goal.sep` ve bir boşlukla birleşir (ja'da boşluk yok); ilk harf o dilin kuralıyla büyür |
| Giriş hediyesi, reklam | `gift.*`, `ad.*`, `freeprim.*` | |
| İpucu | `hint.*`, `nosolution.*` | |
| Kazanma, haklar | `win.*`, `lost.*`, `life.*`, `nolives.*`, `promo.*`, `firstprim.*` | |
| Depo, dükkân, harita | `depot.*`, `shop.*`, `map.*`, `daily.*`, `quality.*`, `lives.*`, `prim.*` | Eşya adları `shop.<id>` |
| Unvanlar | `title.0`–`title.5` | "Crate Boss" hiçbir dilde çevrilmez |
| Semtler | `district.*` (harita, unvan), `world.*` (oyun ekranı) | |
| Bölüm adları | `level.<id>.name` | tr sütunu bölüm dosyasındaki adla aynı olmalı (test). Tabloda olmayan bölüm, dosyadaki adla görünür |

## Sözcük seçimleri (taslak)

- **Prim** (para birimi): en Bonus, es Prima, ar مكافأة, de Prämie, it Premio, fr Prime, ja ボーナス, ko 보너스.
- **Saha** (bölüm): en yard, es patio, ar ساحة, de Hof, it piazzale, fr cour, ja 現場, ko 작업장.
- **Hak** (can): en life, es vida, ar محاولة, de Leben, it vita, fr vie, ja ライフ, ko 하트.

## Teknik notlar

- Unity Localization paketi yerine kendi tablomuz (`Loc`, `LocTable`) kullanıldı: arayüz koddan kurulan düz `Text` bileşenleri; anahtar düzeni §L.2 ile aynı (`ui.undo` biçimi). Paket gerekirse tablo ona aktarılabilir.
- Arapça: RTLTMPro TextMeshPro içindir, oyun TextMeshPro kullanmıyor. Onun yerine `ArabicShaper` (harf bağlama, Arapça Sunum Biçimleri-B; satır sağdan sola, Latin ve rakam parçaları kendi sırasıyla) ve `GameText` (sola hizalı metni sağa alır, satır kırmayı kendisi yapar) yazıldı.
- Japonca: Noto Sans KR kana ve sık kanjiyi de taşıyor; ek olarak cihaz fontları (Yu Gothic, Hiragino, Noto Sans CJK JP) yedek listesinde.
- Sayılar her dilde Batı rakamı (0–9).
- Dil: ilk açılışta cihaz dili (desteklenmiyorsa İngilizce), sonra haritadaki **Dil** düğmesiyle seçilen. Exe/apk için `-lang ar` gibi bir argüman ekran görüntüsü turunda dili zorlar.

## 01.11.2026 eklemeleri (Ana Belge F7–F10)

- Yeni satırlar: ana sayfa (`home.*`), teslimat kasası (`crate.*`), düzenleme modu (`edit.*`), gardırop (`wardrobe.*`, `wear.*`), boyut ve araziler (`size.*`, `land.*`), 63 eşya adı (`item.*`), ipucu "N hamle geri al" (`hint.undo`), 125 lig bölümünün adı (`level.1001.name` … `level.1125.name`).
- Prim geçen bütün satırlar **CB Coin** oldu; "Crate Boss" ve "CB Coin" hiçbir dilde çevrilmez.
- Türkçe dışındaki bütün yeni satırlar **taslaktır**; yayından önce ana dili konuşan biri kontrol etmeli.
