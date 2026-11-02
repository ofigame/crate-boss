# Crate Boss

*Crate Boss: Box Push Puzzle* (Ofigame). Oyuncak bir şehirde geçen, dikey ekranlı, çapraz bakışlı bir 3D kutu itme bulmacası.

Oyuncu baretli ve yelekli bir işçiyle kareli sahalarda kasaları iter. Her bölümde bir görev vardır:
- kasaları hedeflere koymak,
- görev kasasını A'dan B teslim alanına taşımak,
- otoparkı boşaltıp gelen kamyona kasaları yüklemek.

Bulmacayı zorlaştıranlar: kule vinçler (belirli hamlelerde sahaya yük bırakır), boşluk birleştirme, renkli kasa patlatma (aynı renkten 3 kasa yan yana gelince iner), otopark, ağır kasa, buz kasa ve çukurlar.

Her bölümden sonra teslim edilen kasa Boss'un arazisine gider. Oyuncu kasayı **CB Coin** ile açar, eşya seçer ve arazisine yerleştirir. İşçi rütbe atladıkça (Öğrenci → Stajyer → Çırak → Kalfa → Usta) kasaların görünümü değişir.

**Tek kaynak:** [`Docs/CRATE_BOSS_ANA_BELGE.md`](Docs/CRATE_BOSS_ANA_BELGE.md). Bu dosyayla çelişen her şeyde Ana Belge geçerlidir. İşler belgedeki fazlarla (F1–F11) ilerler; güncel durum [`Docs/DURUM_RAPORU.md`](Docs/DURUM_RAPORU.md).

Bu proje, eski test projesi `E:\OFIGAME\OYUNLAR\Sokoban3D` taşınarak 29.10.2026'da kuruldu. Eski projeye dokunulmaz.

## Ayarlar

| | |
|---|---|
| Unity | 6000.6.4f1 + URP |
| Paket kimliği | `com.ofigame.crateboss` |
| Sürüm | 0.1.0 (Android kod 1) |
| Ekran | Sadece dikey |
| Ad alanları | `CrateBoss.Core`, `CrateBoss.Levels`, `CrateBoss.Solver`, `CrateBoss.Meta`, `CrateBoss.Ads`, `CrateBoss.Localization`, `CrateBoss.Game`, `CrateBoss.View`, `CrateBoss.UI`, `CrateBoss.EditorTools`, `CrateBoss.Tests` |
| Diller | tr (kaynak), en (yedek), es, ar, de, it, fr, ja, ko |

## Klasörler

```
Assets/_Project/
  Scripts/Core      kurallar (RulesEngine, GameState, vinçler, patlatma, kamyon); Unity'ye bağımlı değil
  Scripts/Levels    bölüm JSON'u okuma/yazma/doğrulama; Unity'ye bağımlı değil
  Scripts/Solver    çözücü, zorluk ölçümleri, vinç toleransı, kalite filtreleri; Unity'ye bağımlı değil
  Scripts/Meta      CB Coin, kayıt, günlük hediyeler, ödüller
  Scripts/Ads       reklam kuralları (AdPolicy), test reklamları, analitik noktası
  Scripts/Localization  dil tablosu (Loc), Arapça biçimlendirme (ArabicShaper)
  Scripts/Framing   kamera kadrajı
  Scripts/Game      GameController, MetaService, ligler, öğreticiler, ekran görüntüsü turu
  Scripts/View      3D görünüm (saha, işçi, vinç, şehir, efektler, ses)
  Scripts/UI        HUD, bölüm listesi, kazanma paneli, terfi, dil listesi
  Editor            Tools > Crate Boss menüsü: Setup Project, Validate Levels, Level Editor, Normalize Models, Environment Placeholders, Search Hard Level, Generate App Icon, Build Windows (test)
  Resources/LeagueLevels  oyunun 125 lig bölümü (league_001.json …, kayıtlı çözümleriyle)
  Resources/Levels  eski onaylı bölümler; oyun sahibi onaylamadan değişmez
  Resources/PlanLevels, CandidateLevels  eski aday bölümler (F9'da aday havuzu, oyunda kullanılmaz)
  Resources/DemoLevels       test tahtaları
  Resources/Localization     strings.txt (9 dil)
  Tests/EditMode    EditMode testleri
  Art/Icon          yer tutucu uygulama ikonu (kodla üretilir)
Docs/               Ana Belge, durum ve iş raporları, Tasarim/f9_havuz (eski haritalardan F9 adayları)
```

Kod kuralı: her `.cs` dosyasında tek bir tür tanımlanır.

## Açma, derleme, test

Projeyi Unity Hub'dan aç (Unity 6000.6.4f1). `Game` sahnesini `Tools > Crate Boss > Setup Project` yeniden üretir; elle düzenleme. Bütün derlemeler yalnızca E: sürücüsünü kullanır (`E:\OFIGAME\Tools\build-env.sh`).

Windows test derlemesi: `Builds/Windows/CrateBoss.exe`.

```bash
/e/OFIGAME/Tools/build-crateboss.sh
```

Her bölümü en iyi çözümüyle oynatıp ekran görüntüsü alır. Görüntüler `E:\CLAUDE\crateboss_shots` klasörüne yazılır.

```bash
/e/OFIGAME/Tools/build-crateboss.sh shots
```

EditMode testleri:

```bash
/e/OFIGAME/Tools/build-crateboss.sh tests
```

Her bölüm için çözüm, zorluk ve vinç toleransı raporu (Validate Levels):

```bash
/e/OFIGAME/Tools/build-crateboss.sh validate
```

Yerleşim denetimi (Validate Layout, rapor `Docs/YERLESIM_RAPORU.md`):

```bash
/e/OFIGAME/Tools/build-crateboss.sh layout
```

Android test APK'sı: `Builds/Android/CrateBoss.apk` (debug imzalı).

```bash
/e/OFIGAME/Tools/build-crateboss.sh android
```

### Exe argümanları

| Argüman | Ne yapar? |
|---|---|
| `-levels CandidateLevels` | Aday bölümlerle oynatır; liste tamamen açık. `Builds/Windows/Adaylar.bat` bunu açar. |
| `-levels PlanLevels` | Eski plan bölümleri (`Plan100.bat`) |
| `-levels DemoLevels` | Test tahtaları (`Gorev_Demolari.bat`) |
| `-lang ar` | Dili zorlar (tr, en, es, ar, de, it, fr, ja, ko) |
| `-shots <klasör>` | Ekran görüntüsü turu; `-quick` kısa tur, `-langtour` dil turu |
| `-layoutcheck <rapor.md>` | Yerleşim denetimi |
| `-noads` | Reklam yüklenemedi durumunu dener |

### Lig bölümleri üretimi

125 lig bölümü Unity dışında, `E:\CLAUDE\cb_hard` altındaki ters arama üreticisiyle (HardLab, MechLab, Assemble) üretilir ve `Resources/LeagueLevels`'a yazılır. F9'da bu üretici Ana Belge §10'a göre genişletilecek.

## Belgeler (`Docs/`)

| Belge | İçerik |
|---|---|
| `CRATE_BOSS_ANA_BELGE.md` | **Tek kaynak.** Oyunun bütün kuralları, görünüm, 125 bölüm planı, ekonomi, fazlar |
| `DURUM_RAPORU.md` | Ana Belge'ye geçiş durum raporu ve faz raporları |
| `LIG_BOLUMLERI.md` | Üretilmiş 125 lig bölümünün raporu |
| `MODEL_RAPORU.md`, `MODEL_URETIM_LISTESI.md` | Modellerin ölçü/üçgen raporu ve Tripo promptları |
| `YERLESIM_RAPORU.md` | Validate Layout çıktısı |
| `CEVIRI_DURUMU.md` | Hangi dil taslak, hangisi kontrol edildi; sözcük seçimleri |
| `MAGAZA_METINLERI.md` | Mağaza alt başlıkları (9 dil) |
| `YAYIN_KONTROL_LISTESI.md`, `GIZLILIK_POLITIKASI_TASLAK.md` | Yayın öncesi liste, gizlilik metni taslağı |
| `sokoban3d_validate_baseline.txt` | Taşıma öncesi Validate Levels çıktısı (en kısa hamle sayıları) |
| `referans/` | Görsel referans (Parking Jam tarzı) |
