# Crate Boss — Ana Belge

> Sürüm 1.0 · 01.11.2026 · Okuyucu: **Claude Code** (ve oyun sahibi)
>
> **Bu belge tek kaynaktır.** Önceki bütün görev ve tasarım belgeleri (`CRATE_BOSS_GOREV.md`, `CRATE_BOSS_EK1.md`, `CRATE_BOSS_EK2.md`, `GOREV_V3_SAHA_VINC_FORKLIFT.md`, `GOREV_LIMAN_VINC.md`, `TASARIM_VIZYONU.md`, `VINC_VE_LOJISTIK_DUNYALAR.md`, `OYUN_TANITIMI.md`, `OYUN_TANITIMI_SOKOBAN3D.md`, `sokoban3d-prototip-gorevi.md`, `TASLAK_125_BOLUM.md`, `EK2_SAHIP_NOTLARI.md`, `BOLUM_PLANI_100.md`, `LIG_TASARIM.md`, `YOLU_AC_TASARIM.md`, `Tasarim/GUNLUK_HAVUZ.md`, `Tasarim/PLAN100_URETIM.md`) **silindi** ve geçersizdir. İçlerindeki oyun sahibi kararlarından geçerli olanlar (çapraz kamera, sadece renkli patlatma kasaları, %60 yıldız kuralı, kodla çizilen arabalar, model prompt stili…) bu belgeye işlendi. Kodda, yorumlarda ya da raporlarda onlara atıf varsa bu belgeye çevir.
>
> **Silinmeyen (rapor ve çalışma listeleri; geçerli, bu belgeyle birlikte kullan):** `MODEL_RAPORU.md`, `YERLESIM_RAPORU.md`, `CEVIRI_DURUMU.md`, `MAGAZA_METINLERI.md`, `GIZLILIK_POLITIKASI_TASLAK.md`, `YAYIN_KONTROL_LISTESI.md`, `MODEL_URETIM_LISTESI.md` (model promptları), `LIG_BOLUMLERI.md` (üretilmiş 125 bölümün raporu), `sokoban3d_validate_baseline.txt` ve `Tasarim/` altındaki veri klasörleri. Bu belgeyle çelişen bir satır varsa **bu belge geçerlidir**; ilgili raporu güncelle.
>
> Konum: `E:\OFIGAME\OYUNLAR\CrateBoss\Docs\CRATE_BOSS_ANA_BELGE.md` (aynı kopya `E:\OFIGAME\OYUNLAR\` altında).
>
> **Nasıl çalışılır:** Önce belgenin tamamını oku. Sonra §22'deki **durum raporunu** yaz (mevcut kodda ne var, ne bu belgeyle uyumlu, ne kaldırılacak). Ardından §22'deki fazlarla ilerle. Her faz sonunda testleri çalıştır, `Validate Levels` ve `Validate Layout` çalıştır, ekran görüntüsü al, kısa rapor ver.

---

## İçindekiler

0. Proje bilgisi ve mevcut durum
1. Oyunun özeti
2. Görsel yön: oyuncak şehir
3. Kamera, ölçek ve mantıklı yerleşim
4. Temel kurallar
5. Mekanikler (kesin kurallar)
6. Bölüm dosyası biçimi
7. Rütbeler ve kasa görünümleri
8. Alan büyüklüğü ve gezinme
9. 125 bölüm planı
10. Bölüm üretimi ve doğrulama
11. Ana sayfa: Boss'un arazisi
12. Teslimat kasaları, eşyalar ve yerleştirme
13. CB Coin ekonomisi
14. İpucu
15. Reklam
16. Oyuncak efektler
17. Arayüz
18. Dil desteği
19. Modeller, Tripo, karakter ve animasyonlar
20. Kaldırılanlar
21. Testler
22. Fazlar
23. Rapor

---

## 0. Proje Bilgisi ve Mevcut Durum

| Konu | Değer |
|---|---|
| Oyunun adı | **Crate Boss** (mağazada: *Crate Boss: Box Push Puzzle*) |
| Proje klasörü | `E:\OFIGAME\OYUNLAR\CrateBoss` (kurulu) |
| Eski test projesi | `E:\OFIGAME\OYUNLAR\Sokoban3D` (dokunulmaz, arşiv) |
| Ham modeller | `E:\OFIGAME\OYUNLAR\3D_Modeller` (oyuncak sürümleri: `3D_Modeller\oyuncak\`) |
| Motor | Unity 6 (6000.6.4f1) + URP |
| Stüdyo / paket kimliği | Ofigame · `com.ofigame.crateboss` |
| Ad alanı | `CrateBoss.*` |
| Editör menüsü | `Tools > Crate Boss > …` |
| Platform | Android ve iOS, sadece dikey ekran |
| Hedef kitle | 13+ |
| Diller | Türkçe (kaynak), İngilizce (yedek), İspanyolca, Arapça, Almanca, İtalyanca, Fransızca, Japonca, Korece |

**Kod kuralları (değişmez):**
- Her `.cs` dosyasında **tek tür** (class, enum, struct, record, interface).
- Mevcut public API'ler (dışarıdan kullanılan imzalar) gereksiz yere bozulmaz. Kaldırılan sistemler (§20) bunun istisnasıdır: tamamen silinir.
- Oyun kuralları (`Core`) Unity'den bağımsız sade C#; görüntü katmanı sadece çizer.
- Çözücü, oyunla **aynı** kural kodunu kullanır; ayrı kural yazılmaz.

**Mevcut kodda korunanlar:** itme kuralları, geri alma, çoklu vinç (`CraneSystem`, `CraneState`, `CraneDefinition`…), sınır haritası (`BoundaryMap`), model içe aktarma ve normalleştirme araçları, çözücü altyapısı, bölüm doğrulayıcı, karakter ve animasyon kurulumu, dil altyapısı (varsa).

**Daha önce yapılmış ve korunacak işler (oyun sahibinin onayladığı):**

| Var olan | Not |
|---|---|
| Lig sistemi: `Resources/LeagueLevels/league_001..125.json` | HardLab ters-BFS üreticisiyle (`E:\CLAUDE\cb_hard`) üretildi; her dosyada kayıtlı `solution` var, ipucu bu çözümden gelir. `LevelLoader.GameFolder = "LeagueLevels"`. Bu 125 bölüm, §9 planına F9'da **eşlenir** (bkz. F9): uyanlar kalır, uymayanlar yeniden üretilir. Yeni üretilenler aynı klasöre aynı adla yazılır. |
| `LeagueRules` + `MetaService.Unlocked` | Rütbe kilidi: %60 yıldız kuralı (§7). Korunur. |
| `OpenAllForTesting = true` | Test için her şey açık. **Yayından önce mutlaka `false`** (YAYIN_KONTROL_LISTESI'ne yaz). |
| `TutorialDirector` / `TutorialView` | Sözsüz öğreticiler; 1–5 ve her yeni mekaniğin ilk bölümünde kullanılır. |
| İç duvarların "sabit kasa" görünümü | Lacivert-gri, kayışlı. §3.4'teki rütbe engelleriyle birlikte kullanılabilir. |
| Patlatma kasaları `objects` katmanında `1`–`6` rakamları | §5.4. Modeller: `Resources/Models/BoxMatch_1..5` (plastik market kasası). |
| Kodla çizilen park ve trafik arabaları | Oyun sahibi beğendi; Tripo araba modeli **kullanılmaz** (§19.2). |
| `PlanLevels/plan_001..100` | Eski plan bölümleri; F9'da aday havuzu olarak taranabilir, oyunda kullanılmaz. |
| Kamera (`CameraFramer`, `SwipeInput.FromScreen`) | §3.1'deki çapraz kamera. Korunur. |

---

## 1. Oyunun Özeti

**Crate Boss**, oyuncak bir şehirde geçen modern bir kutu itme bulmacasıdır. Oyuncu, baretli ve yelekli bir işçiyle kareli sahalarda kasaları iter. Her bölümde bir **görev** vardır: kasaları hedeflere koymak, **görev kasasını** A'dan B'ye taşımak ya da otoparkı boşaltıp gelen **kamyona** kasaları yüklemek. Bunu zorlaştıran ve zevkli kılan şeyler: **kule vinçler** (belirli hamlelerde sahaya yük bırakır), **boşluk birleştirme** (sıkışık kasaların arasındaki boşlukları toplayarak yol açmak), **kasa patlatma** (aynı renkten 3 kasayı yan yana getirince kasalar yere iner) ve **otopark** (park etmiş arabaları doğru sırayla göndermek).

Her bölüm bitince teslim edilen kasa **Boss'un arazisine** gider. Oyuncu kasayı **CB Coin** ile açar, içinden çıkan 3 eşyadan birini seçer (ev, araba, havuz, şirket binası…) ve şehirdeki arazisine yerleştirir. İşçi **rütbe** atladıkça (Öğrenci → Stajyer → Çırak → Kalfa → Usta) kasalar değişir, arazi büyür, Boss'un hayatı kurulur.

**Tasarım ilkeleri:**
1. **Her bölüm bir fikir:** Her bölümde tek cümleyle anlatılabilen bir "aha" anı vardır. Fikirsiz bölüm oyuna girmez.
2. **Adil zorluk:** Her şey önceden görünür. Vinç ne zaman nereye bırakacağını gösterir. Sürpriz ceza yok.
3. **Stres atma:** Saniye sayacı yok. Geri Al sınırsız ve ücretsiz. Kaybetme yok, sadece takılma (ipucu var).
4. **Tekrara düşmeme:** Birkaç bölümde bir yenilik gelir; alanlar büyür; rütbeyle görünüm değişir.
5. **Görsel tatmin:** Her şey oyuncak gibi, canlı, temiz; her hareket sevimli bir efektle.

---

## 2. Görsel Yön: Oyuncak Şehir

### 2.1 Referans

"Parking Jam" türü oyunlar: açık gri, temiz bir şehir; aralardan kıvrılan koyu gri yollar; beyaz çizgili otoparklar; şeker renkli, tombul oyuncak arabalar; düşük poligonlu yuvarlak ağaçlar, banklar, binalar.

### 2.2 Oyuncak tarzı (her nesne için)

Oyundaki **her nesne** (sahanın içinde ve dışında: kasalar, sabit engeller, bariyerler, vinç, arabalar, kamyon, binalar, ağaçlar, Boss'un eşyaları) gerçek bir nesne olarak tanınır ama **oyuncak tarzındadır**.

| Özellik | İstemiyoruz | İstiyoruz |
|---|---|---|
| Şekil | Keskin kenar, ince parça | **Tombul**, kalın yuvarlatılmış kenarlar, hafif abartılı oranlar |
| Yüzey | Ahşap damarı, pas, kir, çizik | **Pürüzsüz kalıp plastik** hissi |
| Detay | Vida, küçük yazı, kablo | Sadece büyük, okunaklı detaylar |
| Renk | Soluk, doğal | **Temiz şeker tonları**, düz renk ağırlıklı |
| Parlaklık | Metalik ya da tamamen mat | Hafif parlak plastik |
| Işık | Sert gölge | Bol, yumuşak ışık; düşük kontrast |

### 2.3 Renk paleti

| Kullanım | Renk |
|---|---|
| Şehir zemini, kaldırım | `#E4E6EA`, `#D6D9DE`; kaldırım kenarı `#C9CDD3` |
| Saha (park alanı) zemini | `#ECEEF1` + beyaz çizgiler `#FFFFFF` |
| Yol | asfalt `#4E5462`, kenar `#5A6070`, beyaz kesikli şerit |
| Şeker renkleri | pembe `#F2558A`, mor `#9B5DE5`, turkuaz `#3EC1D3`, sarı `#FFD23F`, turuncu `#FF8C42`, yeşil `#3DCC6B`, kırmızı `#EF4444`, açık mavi `#5AA9F0`, beyaz `#F7F7F9` |
| Ağaç | `#4CB86A` / `#2E8F4E`, gövde `#8A5A3B` |
| Sarı bariyer, vinç | `#FFC93C` / `#F2B632` |
| Gökyüzü | `#DDEBF5` → `#F5F8FA` |

### 2.4 Malzeme ve ışık (URP)

- URP Lit; **Metallic 0**; **Smoothness 0,45–0,55**; normal map yoğunluğu 0,3–0,5.
- Doku boyutu: 512 (küçük nesneler), 1024 (vinç, binalar, karakter).
- Tek ana ışık sol-üstten (≈40°), sıcak beyaz, yumuşak gölge (güç 0,6–0,7); gradient ortam ışığı (üst açık mavi, alt açık gri); hafif SSAO (köşelerde yumuşak kararma); çok hafif bloom; doygunluk +10.
- **Okunabilirlik:** İtilebilen her şey sıcak ve doygun; itilemeyen her şey (duvarlar, sabit engeller) soğuk ve mat.

### 2.5 Saha görünümü

- Saha, şehrin zemininde, aynı düzlemde bir park alanıdır. **Kaide, toprak kesiti ya da havada duran platform yok.**
- Sahanın dış sınırı **sarı oyuncak bariyer** (dış sınır `#` kareleri blok olarak çizilmez; sınır kenarına bariyer parçaları dizilir; köşelerde köşe parçası, açıklıklarda uç direk; mevcut `BoundaryMap` mantığı).
- Bariyerin dışında kaldırım, yol, binalar, ağaçlar. Ekranda boş arka plan kalmaz.
- Saha içindeki duvarlar (`#`, sınıra bağlı olmayanlar) gerçek oyuncak nesnelerdir (§3.4; rütbeye göre değişir, §7).

### 2.6 Şehir kontrol listesi (her ekran görüntüsünde)

1. Kaide, toprak kesiti, havada duran platform **yok**.
2. Saha sınırı **sarı bariyer**; gri blok duvar **yok**.
3. Bariyerin dışında **kaldırım, yol, bina**; arka plan boş değil.
4. Karakter **işçi** (baretli, yelekli).
5. Her nesne **oyuncak tarzında** (§2.2).
6. İtilen kasalar sıcak, engeller soğuk (okunabilirlik).

Geçmeyen ekran görüntüsü varsa faz bitmiş sayılmaz.

---

## 3. Kamera, Ölçek ve Mantıklı Yerleşim

### 3.1 Oyun kamerası

- **Çapraz bakış (mevcut, oyun sahibi kararı, korunur):** yatay dönüş **yaw 25°**, eğim **pitch 55°**, **perspektif FOV 30°** (`CameraFramer.yaw`).
- **Kaydırma eşlemesi:** `SwipeInput.FromScreen` ekrandaki kaydırmayı kameranın açısına göre **en yakın ızgara yönüne** çevirir. Oyuncu "kasanın gittiği yöne" kaydırır; çapraz açı kontrolü bozmaz. Bu eşleme için test var/olmalı (§21).
- **Oyun alanı dışında sürükleme** kamerayı kaydırır (en fazla 3 birim). Oyun alanı içinde tek parmak kaydırma her zaman karakteri yürütür.
- **Okunabilirlik sınırı:** en büyük alanda kare **≥ 24 px** (oyun sahibi kabul etti).
- Bölüm açılışında kamera şehrin üstünden süzülüp oyun açısına oturur (≈1,2 sn). Ana sayfa (Boss'un arazisi) da aynı çapraz dilde, biraz daha yüksekten bakar (§11.2).

### 3.2 Tek ölçek (1 birim = 1 kare)

| Nesne | Boyut |
|---|---|
| Karakter | boy ≈1,05 |
| Kasalar | taban 0,82–0,92 (rütbeye göre model değişse de aynı oran) |
| Park etmiş araba | **1×2 kare**, yükseklik ≈0,8 |
| Kamyon | kabin 2×1,2; yükleme alanı bölüme göre |
| Kule vinç | direk taban 1×1, direk boyu 7–9, bom sahayı kaplayacak kadar (modüler parçalarla) |
| Ev | 3×3 – 5×4, yükseklik 2,5–4 |
| Ağaç | taban 0,8–1,2, boy 1,5–2,5 |
| Sokak lambası | boy ≈2,2 |
| Yol şeridi | genişlik 1,4 (iki şerit 2,8) |

Nesneler ekrana sığsın diye ayrı ayrı büyütülüp küçültülmez.

### 3.3 Mantıklı yerleşim kuralları (oyunun her yerinde)

1. Her nesne zemine ya da üstünde durabileceği bir yüzeye oturur; havada duran yok (vinç kancasındaki yük hariç).
2. İki nesne iç içe geçmez.
3. Yolun üstünde sadece yolda olması gerekenler olur. Bina, ağaç, lamba, bank, vinç direği yola taşmaz.
4. **Kule vinç** direği sahanın dışında, kendi beton temelinde, boş arazide durur; yola, kaldırıma, binaya, bariyere değmez. Bom havada sahanın üstüne uzanabilir; arka kısmı binaların içinden geçmez. Kanca bırakma karesinin tam üstünden dikey iner.
5. Seyreden araçlar şeridin içinde, şerit yönünde. Park etmiş araçlar park yerinin içinde.
6. Binaların kapısı yola ya da kaldırıma bakar.
7. Sarı bariyer sınır çizgisinin tam üstünde, boşluksuz ve üst üste binmeden.
8. Saha içi sabit nesneler kendi karelerinde, ortalı; komşu kareye taşmaz.
9. Oyun kamerasından bakınca hiçbir süs nesnesi sahanın önünü kapatmaz. Sahanın kameraya yakın kenarının önünde sadece alçak nesneler (çim, kaldırım, alçak çit, bank).
10. Bütün nesneler aynı kamera ve ışıkla çizilir; ekrana yapıştırılmış 2D resim yok (arayüz hariç).

### 3.4 Saha içi sabit engellerin seçimi

İç duvarlar (`#`) şekillerine göre nesneye dönüşür (nesne seti rütbeye göre, §7):
- Yan yana **2 kare** → park etmiş oyuncak araba (uzun kenar boyunca).
- **Tek kare** → rütbe setinden tek kare engel.
- **2×2 ve üstü** → rütbe setinden büyük engel (konteyner yığını, kulübe…).
- Bölüm dosyasında istenirse `obstacleHints` ile belirli bir grup için nesne seçilebilir.
- Seçim, bölüm kimliğine bağlı sabit tohumla yapılır (her açılışta aynı görünüm).

### 3.5 Yerleşim doğrulayıcı (zorunlu)

`Tools > Crate Boss > Validate Layout`. Her bölüm ve ana sayfa için:

| Kontrol | Yöntem |
|---|---|
| Havada nesne | Tabandan aşağı ışın; > 0,02 boşluk → hata |
| İç içe geçme | Sınır kutuları kesişiyor → hata (izinli istisnalar listesi hariç) |
| Yolda yasak nesne | Yol karolarıyla kesişen bina/ağaç/direk/lamba/bank → hata |
| Vinç direği | Temeli yol, kaldırım, bina ya da bariyerle kesişiyor → hata |
| Sahanın önünü kapatma | Kameradan her saha karesine ışın; saha dışı nesneye çarpıyor → hata |
| Ölçek | §3.2'den sapma > %15 → uyarı |

Sonuç: `Docs/YERLESIM_RAPORU.md`. Hatalı bölüm onaya sunulmaz.

---

## 4. Temel Kurallar

### 4.1 Hareket ve itme

- Oyuncu dört yöne (yukarı, aşağı, sol, sağ) tek kare yürür. Tek parmak kaydırma (≥ ~50 px) ya da klavye (editör).
- Önündeki kareye kasa varsa kasayı bir kare iter; kasanın gideceği kare boş zemin, hedef, B karesi ya da dolu çukur olmalı.
- Aynı anda **bir kasa** itilir. **Tek istisna:** normal kasa (ya da patlatma kasası, ya da görev kasası) + arkasındaki **ağır kasa** zinciri (§4.2).
- Başarısız hamle (duvara, itilemeyen kasaya, iki kasaya) sayılmaz; oyuncu küçük bir sarsılma yapar.
- **Geri Al sınırsız** ve ücretsiz; vinç, patlatma, otopark ve kamyon dahil her şeyi geri alır.
- **Yeniden Başlat** her zaman ücretsiz.

### 4.2 Kasa türleri

| Simge | Tür | Kural |
|---|---|---|
| `B` | Normal kasa | Bir kare itilir. Görünüm rütbeye göre (§7). |
| `H` | Ağır kasa | Tek başına itilmez. Oyuncu bir normal/patlatma/görev kasasını iterken onun hemen arkasındaysa ikisi birlikte bir kare ilerler (önce ağır kasanın gideceği kareye bakılır). Patlatmaya girmez. |
| `D` | Görev kasası | Kırmızı, beyaz yukarı oklu. Normal kasa kuralları. Teslim alanına (`B` zemin) götürülür (§5.1). Patlatmaya girmez. |
| `Dh` | Ağır görev kasası | Ağır kasa kuralları + görev kasası. (Bölüm 91 ve sonrası) |
| `1`–`6` | Patlatma kasası | Normal kasa kuralları + renk (1 kırmızı, 2 mavi, 3 yeşil, 4 sarı, 5 mor, 6 pembe); 3'lü eşleşmede iner (§5.4). |
| `K` | Sabit konteyner | Sadece vinçten gelir. İtilemez, zincire girmez, hedefi doldurmuş sayılmaz; duvar gibidir. |

> Not: Nesne katmanında normal kasa simgesi mevcut kodla uyumlu olarak `B`'dir; zemin katmanındaki teslim alanı da `B` harfini kullanır. **Katmanlar ayrı** olduğu için çakışma yoktur. Karışıklık olursa zemin teslim alanı için `Z` kullan ve raporla.

### 4.3 Zemin türleri

| Simge | Tür | Kural |
|---|---|---|
| `#` | Duvar / engel | Geçilmez. Dış sınırdaysa sarı bariyer, içerideyse oyuncak engel olarak çizilir. |
| `.` | Zemin | |
| `T` | Hedef | Üstünde kasa (`K` hariç) olmalı (klasik görev). |
| `B` | Teslim alanı | Görev kasasının gideceği bölge (birkaç kare). Zemine boyalı, parlayan çerçeve. |
| `M` | Ara durak | Görev kasasının önce uğraması gereken bölge (§5.1). |
| `A` | Başlangıç işareti | Sadece görsel (görev kasasının çıkış bölgesi); kural yok. |
| `o` | Çukur | Oyuncu giremez. İçine düşen kasa çukuru doldurur, kare zemin olur. |
| `_` | Boşluk | Haritanın dışı; kullanılmaz (şehir zemini çizilir). |

---

## 5. Mekanikler (Kesin Kurallar)

### 5.1 Görev kasası ve teslim

- Bölüm 10'dan itibaren ana görev: görev kasasını (`D`) **A** bölgesinden **B** teslim alanına götürmek.
- Görev kasası herhangi bir `B` karesine girdiği anda **teslim edilir**: kasa yerine "kilitlenir" (artık itilemez, hafif zıplayıp oturur, B alanı yeşile döner).
- Birden fazla görev kasası olabilir (2 ya da 3); her birinin kendi B alanı vardır (`B1`, `B2`, `B3` – bölüm dosyasında `deliveries` listesiyle). Görev kasası sadece **kendi** B alanında teslim sayılır; kasaların üstünde numara rozeti.
- **Ara durak** (`M`): Bölüm dosyasında `via` tanımlıysa görev kasası önce M bölgesine girmeli; girince M yeşile döner, sonra B'ye gidebilir. M'ye uğramadan B'ye giren kasa teslim **sayılmaz** (B alanı kırmızı yanıp söner, kasa itilebilir kalır).
- Görev kasasını vinç de getirebilir (§5.3).

### 5.2 Boşluk birleştirme

- Ayrı bir kural yok; bir **bölüm tasarımı biçimi**dir: A ile B arasında çok sayıda itilebilir kasa ve aralarında dağınık boşluklar. Kasalar iterek boşluklar birleştirilir ve görev kasasına yol açılır.
- Tasarımda dikkat: karakterin görev kasasının **arkasına geçebileceği** yol da açık olmalı.

### 5.3 Kule vinç

**Temel (mevcut kod, korunur):**
- Bir bölümde **0–3 vinç** (A, B, C). Her vincin kendi `every` (kaç başarılı hamlede bir), kendi sıralı bırakma listesi, kendi sayacı vardır. Vinçler **birbirinden bağımsızdır**.
- Sayaç sadece **başarılı** hamlede azalır.
- Sayaç sıfırdayken bırakma karesinde **oyuncu ya da kasa** (ya da araba) varsa vinç **bekler** (kilitleme). Kare boşalınca sonraki başarılı hamlede bırakır.
- Kazanılan hamlede vinç çalışmaz (önce kazanma kontrolü).
- Bırakma karesi duvar ya da hedef olamaz (doğrulama).

**Bırakma türleri (`drops[].type`):**

| Tür | Ne bırakır |
|---|---|
| `K` | Sabit konteyner (engel) |
| `B` | İtilebilir normal kasa (yardımcı) |
| `1`–`6` | Renkli patlatma kasası (o renk, §5.4) |
| `D` | Görev kasası (`deliveryIndex` ile hangi teslimata ait) |
| `H` | Ağır kasa |

- Bırakma karesi çukursa kasa çukura düşer ve doldurur (köprü).
- Bırakılan renkli kasa düştüğü anda bir 3'lü oluşturursa o hamlenin sonunda iner (§5.6 sırası).

**Görünüm:** Her vincin kulesinde harf levhası; yerde turuncu kesikli işaret + **harf ve kalan hamle** rozeti (`A 3`); kilitliyse `A ⏸`. Rozet ekran dışındaysa ekran kenarında gösterilir.

### 5.4 Kasa patlatma (3'lü eşleşme)

- Patlatma kasaları **sadece renkle** ayrılır (simge/sembol yok; oyun sahibi kararı). Bölüm dosyasında `objects` katmanında rakamla yazılır: `1` kırmızı · `2` mavi · `3` yeşil · `4` sarı · `5` mor · `6` pembe. Renkler birbirinden **ton ve parlaklıkla** da belirgin ayrılmalı (kırmızı/yeşil aynı parlaklıkta olmasın); ekran görüntüsünü gri tonlamada kontrol et.
- Model: rütbeden bağımsız **plastik market kasası** (`Resources/Models/BoxMatch_1..5`; 6. renk için `BoxMatch_6` üret ya da malzemeyle renklendir). Rütbe hissi için kasanın kenar şeridi/etiketi rütbe malzemesine uyabilir; renk her zaman baskın kalır.
- Bir hamle sonunda aynı renkli kasalardan **yatayda ya da dikeyde art arda 3 veya daha fazla** varsa o sıra **iner**. **Çapraz sayılmaz.**
- Aynı anda birden fazla sıra oluştuysa hepsi iner; kesişen sıralar birlikte iner.
- İnen kasaların kareleri **zemin** olur. **Zincirleme düşme yok**: başka hiçbir şey hareket etmez.
- Normal kasa (`B`), görev kasası, ağır kasa, `K` eşleşmeye girmez.
- 4'lü: +5 CB Coin, 5'li ve üstü: +10 CB Coin (bonus; bulmaca 3'lerle çözülebilecek şekilde tasarlanır).
- Kapaklar: inen her kasanın karesinde zemin kapağı açılır, kasa döner-küçülür iner, kapak zıplayarak kapanır (§16).

### 5.5 Otopark

**Düzen:** Otopark bölümlerinde sahanın yanında (aynı alanın bir parçası olarak) beyaz çizgili bir otopark, otoparktan şehrin yoluna çıkışlar ve kamyonun sahaya geleceği bir **kamyon rotası** vardır.

**Araba kuralları:**
- Arabalar **1×2 kare**; bölüm dosyasında kareleri ve yönleriyle tanımlı (`parking.cars`).
- Oyuncu bir arabanın üstünde, arabanın **uzun ekseni boyunca** (burun ya da arka yönü) kaydırır. Diğer yönlere kaydırma etkisizdir.
- Araba o yönde kare kare ilerler. Önündeki bütün kareler otopark çıkışına (`parking.exits`) kadar **boşsa** araba çıkışa ulaşır, yola çıkar, şehirde uzaklaşıp kaybolur (yeri boşalır).
- Yolda bir **araba, kasa, engel, oyuncu ya da kamyon** varsa araba ona kadar gider, **çarpar, zıplayıp eski yerine döner**. Hiçbir şey değişmez; sadece **çarpışma sayacı** artar (yıldız için).
- Araba gönderme bir **hamle** sayılır (geri alınabilir; vinç sayaçlarını azaltır).
- Arabalar başka bir şeyi itmez.

**Kamyon:**
- Kamyon rotası (`parking.truckRoute`) zemine boyalı soluk bir çizgi ile gösterilir. Rotanın **tüm kareleri boşalınca** (o hamlenin sonunda) kamyon şehirden gelir, rota boyunca **geri geri** yanaşır ve yükleme alanında (`parking.bay`) durur (≈1,5 sn animasyon; girişler sıraya alınır).
- Yükleme alanı kamyon kasasıdır: hedef kareleri gibi davranır. Bütün kareleri kasayla dolunca kamyonun kapağı kapanır, kamyon kasalarla birlikte gider; varsa sonraki kamyon aynı rotadan gelir (`parking.trucks` sayısı kadar).
- Kamyon rotası tekrar tıkanırsa (örneğin oyuncu rotaya kasa iterse) sonraki kamyon **gelmez**; rota boşalınca gelir.
- Kamyon yükleme alanında dururken onun kareleri duvar gibidir (yükleme alanı hariç).

**İç içe durumlar** (bölüm 35'ten itibaren): Bir arabanın yolunu bir kasa kapatabilir (önce kasa itilir); bir kasanın yolunu bir araba kapatabilir (önce araba gönderilir); vinç bir arabanın önüne engel bırakabilir (o araba önceden gönderilmeli).

### 5.6 Bir hamlenin işlenme sırası (kesin)

```
1. Oyuncu hamlesi: yürüme/itme (§4.1) YA DA araba gönderme (§5.5). Başarısızsa → DUR.
2. MoveCount += 1
3. Görev kasası teslim / ara durak kontrolü (§5.1)
4. Patlatma kontrolü (§5.4)
5. Kamyon kontrolü: rota boşaldıysa kamyon gelir; yükleme alanı dolduysa kamyon gider (§5.5)
6. Kazanma kontrolü (§5.7). Kazanıldıysa → DUR.
7. Vinçler (A, B, C sırayla) (§5.3)
8. Vinç bırakması bir 3'lü oluşturduysa: patlatma kontrolü tekrar (§5.4)
```

Bu sıra kodda, çözücüde ve testlerde **birebir aynı** olmalı.

### 5.7 Kazanma koşulları (`goal`)

| `goal` | Koşul |
|---|---|
| `targets` | Her hedefin (`T`) üstünde bir kasa (`K` hariç) |
| `deliver` | Bütün görev kasaları kendi B alanlarında teslim edildi |
| `truck` | Bütün kamyonlar dolup gitti |

Birden fazla `goal` verilebilir (hepsi sağlanmalı); bölüm 100 ve 125 bunun örneği.

### 5.8 Yıldızlar

| Yıldız | Koşul |
|---|---|
| ★ | Bölümü bitirmek |
| ★★ | Hamle sayısı ≤ en iyi çözüm + %25 (otoparkta çarpışma ≤ 2) |
| ★★★ | Hamle sayısı ≤ en iyi çözüm (büyük alanlarda: bilinen en iyi + %10); otoparkta **hiç çarpışma yok** |

"En iyi çözüm", çözücünün bulduğu en kısa (ya da bilinen en iyi) çözümdür.

---

## 6. Bölüm Dosyası Biçimi

`Resources/LeagueLevels/league_###.json` (mevcut klasör ve ad; üç haneli numara; numara oyuncuya gösterilmez). Aşağıdaki alanlar mevcut biçime **eklenir**; var olan alanlar (`solution` dahil) korunur.

```json
{
  "id": 42,
  "name": "Renkli Teslimat",
  "rank": "stajyer",
  "goal": ["deliver"],
  "ground":  ["#########", "#..BBB..#", "..."],
  "objects": [".........", "....1....", "..."],
  "deliveries": [ { "box": [4, 12], "area": "B", "via": null } ],
  "cranes": [
    { "id": "A", "side": "left", "every": 7,
      "drops": [ { "x": 5, "y": 6, "type": "1" }, { "x": 3, "y": 4, "type": "K" } ] }
  ],
  "parking": null,
  "obstacleHints": [],
  "design": {
    "idea": "Eksik üçüncü kasayı vinç getirecek; düşeceği karenin yanına ikiliyi hazırla.",
    "patterns": ["V-renk", "P-düz"],
    "targetDifficulty": 68,
    "targetTolerance": 3
  },
  "solution": "UURRDL…"
}
```

Otopark bölümünde:

```json
"parking": {
  "cars": [ { "cells": [[2,1],[2,2]], "color": "pink" }, { "cells": [[3,1],[4,1]], "color": "teal" } ],
  "exits": [[0,1],[0,2]],
  "truckRoute": [[9,4],[9,5],[9,6],[8,6]],
  "bay": [[7,6],[7,7],[8,7]],
  "trucks": 1
}
```

- Birden fazla görev kasasında `B1`, `B2` zemin harfleri yerine `deliveries[].area` ile bölge kimliği kullanılır; zemin katmanında bölge harfleri `1`, `2`, `3` olabilir. Hangisini seçtiğini raporla ve tutarlı kullan.
- `LevelLoader` bütün alanları doğrular; hatada anlaşılır mesaj verir.
- Eski bölüm dosyaları (tek `crane`, eski alanlar) okunabilir kalsın ama yeni bölümler bu biçimle yazılır.

---

## 7. Rütbeler ve Kasa Görünümleri

İlerlemeyi rütbeler gösterir: **her 25 bölümde bir terfi.** Yeni bölümler eklendikçe yeni rütbeler eklenir (yapı buna hazır olmalı: rütbe listesi veri dosyasında).

| Rütbe | Bölümler | İtilen kasalar | Saha içi sabit engeller | Patlatma kasaları |
|---|---|---|---|---|
| **Öğrenci** | 1–25 | Karton koliler: açık kahve, bantlı, tombul | Trafik konisi grupları, beton bloklar, park etmiş oyuncak arabalar | Renkli plastik market kasaları (her rütbede aynı model, sadece renk) |
| **Stajyer** | 26–50 | Ahşap kasalar: bal rengi, kalın tahtalı | Palet yığınları, çöp konteynerleri, oyuncak arabalar | 〃 |
| **Çırak** | 51–75 | Plastik kasalar: canlı renkli, delikli yanlı | Varil grupları, plastik bariyer blokları, oyuncak arabalar | 〃 |
| **Kalfa** | 76–100 | Metal kasalar: oyuncak sac, büyük yuvarlak cıvatalı | İstif konteynerler (1 kat), jeneratör kutuları, oyuncak arabalar | 〃 |
| **Usta** | 101–125 | Premium kasalar: krem gövde, altın şerit köşe, kurdele | Şık çiçeklikler, heykel kaideleri, cam vitrin kutuları, oyuncak arabalar | 〃 |

- **Görev kasası** her rütbede kırmızı + beyaz yukarı oklu (malzemesi rütbeye uyar).
- **Ağır kasa** her rütbede koyu gri + sarı-siyah şerit.
- Rütbe değişince **kurallar değişmez**, sadece görünüm.
- **Terfi şartı (%60 kuralı, mevcut `LeagueRules`/`MetaService`):** Bir rütbenin son bölümü (25, 50, 75, 100) bitince sonraki rütbenin bölümleri, **o rütbede toplanan yıldız ≥ en fazla yıldızın %60'ı** ise açılır (25 bölüm × 3 = 75 yıldız → **45 yıldız**). Yetmiyorsa terfi kutlaması bekler; rütbe ekranında "x yıldız daha" yazar (yıldız sayısı gösterilebilir, bölüm numarası gösterilmez) ve oyuncu o rütbenin bölümlerini tekrar oynayıp yıldız toplar. Yıldız şartı sağlandığı an terfi anı oynar.
- **Terfi anı:** 25, 50, 75, 100, 125. bölümlerin sonunda (yıldız şartı sağlanmışsa) tam ekran kutlama, rütbe rozeti, yeni kasa görünümünün tanıtımı, ücretsiz **terfi kasası**, yeni arazi (§12.5).
- Ekranda bölüm numarası **hiçbir yerde** görünmez; bölüm adı ve rütbe görünür. Rütbe çubuğu ilerlemeyi sayı yazmadan gösterir.

---

## 8. Alan Büyüklüğü ve Gezinme

| Bölümler | Alan (iç) | |
|---|---|---|
| 1–9 | 5×5 → 7×8 | Ekrana sığar |
| 10–19 | 8×12 → 8×14 | Sığar (hafif uzaklaştırma) |
| 20–35 | 8×14 → 9×18 | Gerekirse kaydırma |
| 36–125 | 9×18 → 9×30 | **Kaydırmalı** |

- **Genişlik en fazla 9 kare**; büyüme boyuna.
- **Gezinme:** iki parmakla sürükleme (asıl), uzun basıp sürükleme (yedek). Tek parmak kaydırma her zaman karakteri yürütür (ya da arabaya kaydırma arabayı gönderir).
- **Otomatik takip:** karakter ekranın üst/alt %25'ine girince kamera yumuşakça kayar.
- **Yakınlaştırma:** iki parmakla; kare 24–70 px arası (varsayılan: alan genişliği ekrana sığacak şekilde).
- **Genel bakış butonu:** tüm alanı bir an için sığdırır; tekrar basınca karaktere döner.
- A, B ve vinç rozetleri ekran dışındaysa kenarda okla gösterilir.

---

## 9. 125 Bölüm Planı

### 9.1 Tablo kısaltmaları

**Mekanik kodları:** `S` klasik · `G` görev kasası · `BB` boşluk birleştirme · `V` vinç · `P` kasa patlatma · `O` otopark · `Ç` çukur · `H` ağır kasa.

**Sayılar:** `K14` itilebilir normal kasa · `G1` görev kasası sayısı · `B5` başlangıçtaki boşluk sayısı (BB bölümlerinde) · `P12` patlatma kasası · `V1(2E)` 1 vinç, 2 bırakma, ikisi de engel · `A8` araba · `H2` ağır kasa · `Ç2` çukur · `T3` hedef ya da kamyona yüklenecek kasa sayısı.
**Vinç bırakma harfleri (tabloda):** `E` sabit engel (`K`) · `k` itilebilir kasa · `r` renkli patlatma kasası · `ç` çukura düşüp köprü olan kasa · `g` görev kasası.
**Alan:** `en × boy` (iç, oynanabilir). `+O`: sahanın yanında otopark ve kamyon yolu.
**Zorluk:** §10.5 zorluk puanı hedefi; vinçli bölümlerde ayrıca tolerans (boşa harcanabilecek en fazla hamle).

### 9.2 Zorluk eğrisi ve ritim

Her rütbe 25 bölüm. İçinde 5'erli bloklar:

| Blok | Bölümler (rütbe içinde) | Rol |
|---|---|---|
| 1 | 1–5 | Isınma: yeni kasa görünümüyle bilinen fikirler, sonra yenilik tanıtımı |
| 2 | 6–10 | Yeniliği pekiştir |
| 3 | 11–15 | Yeniliği eski mekaniklerle karıştır (5'te bir otopark) |
| 4 | 16–20 | Zorlaşma + bir "nefes" bölümü |
| 5 | 21–25 | Zirve; 25. bölüm = **terfi sınavı** |

- Komşu iki bölüm arasında zorluk artışı en fazla **+12** (sınavlar hariç).
- Her rütbede en az 2 "nefes" bölümü: kısa ama fikir gerektiren, zorluk bandının alt ucunda.
- Aynı ana mekanik art arda en fazla 3 bölüm baskın olur.
- Otopark bölümleri: **15, 25, 35, 45, 55, 65, 75, 85, 95, 105, 115, 125** (15'te tanıtım, sonra her 10 bölümde bir; her biri öncekinden karmaşık).

### 9.3 Öğrenci (1–25) · Karton koliler

| # | Ad | Alan | Mekanik | Sayılar | Görev | Aha fikri | Zorluk |
|---|---|---|---|---|---|---|---|
| 1 | İlk Koli | 5×5 | S | K1 · T1 | Hedef | Koliyi düz it; hedefe oturunca parlar. (Öğretici) | 5 |
| 2 | İki Koli | 5×6 | S | K2 · T2 | Hedef | Birini yerleştirirken diğerinin yolunu kapatma. (Öğretici) | 10 |
| 3 | Köşeyi Dön | 6×6 | S | K2 · T2 | Hedef | Koliyi köşeden döndürmek için önce yanına dolaş. (Öğretici) | 15 |
| 4 | Önce Arkadaki | 6×6 | S | K3 · T3 | Hedef | Kapıya en yakın hedef en son dolar; yoksa diğerlerine yol kalmaz. (Öğretici) | 22 |
| 5 | Geçici Park | 6×7 | S | K3 · T3 | Hedef | Bir koliyi boş köşeye çek, yolu aç, sonra geri getir. (Öğretici) | 28 |
| 6 | Dar Koridor | 6×7 | S | K3 · T3 | Hedef | Tek kişilik koridorda koliler sırayla girer; yanlış sıra koridoru tıkar. | 34 |
| 7 | Ters Yön | 7×7 | S | K4 · T4 | Hedef | Bir koliyi önce hedeften **uzaklaştırman** gerekir. | 40 |
| 8 | Fazla Koli | 7×8 | S | K5 · T4 | Hedef | Bir koli fazla; hangisinin dışarıda kalacağını seçmek bulmacanın kendisi. | 46 |
| 9 | Çember | 7×8 | S | K5 · T5 | Hedef | Kolilerin etrafında dolaşacak boşluğu son hamleye kadar koru. | 52 |
| 10 | Görev Kolisi | 8×12 | **G + BB** tanıtım | G1 · K14 · B4 | A → B | A ile B arasında dağınık 4 boşluk var; kolileri kaydırıp görev kolisine tek bir koridor aç. | 40 |
| 11 | Boşluk Yürüt | 8×12 | G + BB | G1 · K16 · B4 | A → B | Boşluğu bir uçtan diğerine kare kare "yürütmek" gerekir. | 48 |
| 12 | Kanca Geliyor | 8×12 | **V** tanıtım | G1 · K10 · B5 · V1(1E) | A → B | Vinç 9 hamle sonra koridora engel bırakacak; koridoru ondan önce kullan. Tolerans ≥ 5. | 45 |
| 13 | Vinci Durdur | 8×12 | V (kilitleme tanıtım) | G1 · K12 · B5 · V1(1E) | A → B | Bir koliyi düşme karesine park et; vinç bekler, sen işini bitirirsin. | 52 |
| 14 | Kapanan Kapı | 8×13 | G + BB + V | G1 · K16 · B5 · V1(2E) | A → B | İkinci bırakma B'nin girişini kapatacak; görev kolisi ondan önce B'de olmalı. Tolerans 3. | 58 |
| 15 | Otopark Tıkandı | 8×10 +O | **O** tanıtım | A6 · K3 · T3 | Kamyona yükle | Arabaları doğru sırayla gönder; kamyon yanaşınca 3 koliyi yükle. İki aşama ayrı. | 45 |
| 16 | Dolu Boşluk | 8×13 | G + BB + V | G1 · K16 · B6 · V1(1k) | A → B | Vinç bir **koli** getirir ve boşluklardan birini doldurur; o boşluğa güvenme. | 55 |
| 17 | Uzun Koridor | 8×14 | G + BB | G1 · K20 · B6 | A → B | Görev kolisini uzun koridorda itebilmek için arkasına geçecek yolu açık tut. | 60 |
| 18 | Üç Bırakma | 8×14 | V | G1 · K14 · B6 · V1(2E+1k) | A → B | Vinç üç kez bırakır: ikincisini kilitle, üçüncüsünün getirdiği koliyi kullan. | 62 |
| 19 | Uzak Boşluk | 8×14 | G + BB | G1 · K22 · B6 | A → B | En yakın boşluk tuzak; uzaktaki boşluğu getirmek gerekiyor. (Nefes: kısa çözüm) | 54 |
| 20 | Üçle ve İndir | 8×14 | **P** tanıtım | G1 · P9 (3 renk) · K4 | A → B | Üç üçlüyü indir, B'nin yolu açılsın. 9 renkli kolinin hepsi gerekli. | 45 |
| 21 | Renk Seçimi | 8×15 | P | G1 · P11 (3 renk) · K4 | A → B | İki koli artık kalacak; hangisi? Yanlış seçim yolu tıkar. | 55 |
| 22 | Dik Üçlü | 8×15 | P | G1 · P12 (4 renk) · K6 | A → B | Dikey üçlü kurmak için koliyi yukarıdan aşağı sıraya sok. | 58 |
| 23 | Kesişim | 8×15 | P | G1 · P14 (3 renk) · K6 | A → B | Tek itmede yatay ve dikey üçlü birlikte iner (5 koli); başka yolu yok. | 62 |
| 24 | Kanca ve Renk | 8×16 | P + V | G1 · P12 · K6 · V1(2E) | A → B | Vinç yol üstüne engel bırakmadan iki üçlüyü indir. Tolerans 3. | 64 |
| 25 | Öğrenci Sınavı | 8×16 +O | O + P + V | A8 · P12 · K6 · V1(2E) · T4 | Kamyona yükle | Arabaları boşalt, kamyon gelsin; sahada üçlülerle yol aç, 4 koliyi yükle. → **Terfi: Stajyer** | 70 |

### 9.4 Stajyer (26–50) · Ahşap kasalar

| # | Ad | Alan | Mekanik | Sayılar | Görev | Aha fikri | Zorluk |
|---|---|---|---|---|---|---|---|
| 26 | Yeni Kasalar | 8×16 | P + BB | G1 · P9 · K14 · B4 | A → B | Patlatmayla açılan yer, boşluk birleştirmede ilk hamleyi verir. (Isınma) | 55 |
| 27 | Karşı Uçtan | 8×16 | P | G1 · P15 (4 renk) · K6 | A → B | Üçlünün son kasası alanın öbür ucunda; onu getirmek için yol planla. | 60 |
| 28 | Köprü Kurulur | 8×16 | V(ç) + Ç tanıtım | G1 · K14 · B5 · Ç1 · V1(1ç+1E) | A → B | Vinç çukura kasa bırakıp köprü kurar; köprü gelene kadar görev kasasını hazırla. | 58 |
| 29 | Tek Pencere | 8×17 | V(ç) + BB | G1 · K18 · B5 · Ç1 · V1(1ç+1E) | A → B | Köprüyü bekle ama ikinci bırakma geçidi kapatacak; arada tek bir pencere var. Tolerans 2. | 66 |
| 30 | İkisi Bir Arada | 9×16 | **P + BB birleşik** tanıtım | G1 · P12 · K16 · B5 | A → B | Önce boşluk birleştir ki üçlü kurulabilsin; sonra indir ki yol açılsın. | 62 |
| 31 | Kaydır ve Patlat | 9×16 | P + BB | G1 · P15 · K14 · B6 | A → B | Bir kasayı boşluğa kaydırmak, uzaktaki üçlüyü tamamlar. | 64 |
| 32 | Yanlış Üçlü | 9×17 | P + BB | G1 · P15 · K16 · B5 | A → B | Hazır görünen üçlü tuzak: indirirsen görev kasasının arkasına geçemezsin. | 68 |
| 33 | Çift Kanca | 9×17 | **V×2** tanıtım | G1 · K18 · B6 · V2(1E+1E) | A → B | Tek kilit kasası var; hangi vinci durduracaksın? | 66 |
| 34 | Vinç Yarışı | 9×17 | V×2 + P | G1 · P12 · K8 · V2(2E+1E) | A → B | İki vinç farklı hızda; hızlı olanın karesini önce kullan. Tolerans 2. | 70 |
| 35 | Kasa Arabayı Kapatıyor | 9×14 +O | **O iç içe** | A8 · K8 · T4 | Kamyona yükle | Bir arabanın yolunu kasa kapatıyor: önce kasayı it, sonra arabayı gönder. | 62 |
| 36 | Uzun Yol | 9×18 | P + BB | G1 · P15 · K20 · B6 | A → B | **İlk kaydırmalı alan.** B çok uzakta; önce genel bakışla planla. | 64 |
| 37 | Dört Renk | 9×18 | P | G1 · P18 (4 renk) · K6 | A → B | İki renk birbirinin önünü kesiyor; hangisini önce indireceğini bul. | 68 |
| 38 | Kapat Beni | 9×18 | V + BB | G1 · K20 · B6 · V1(2E) | A → B | Görev kasası yerine girerken vinç seni kapatacak; sorun değil, doğru sıra bu. | 70 |
| 39 | Dörtlü | 9×18 | P | G1 · P16 · K6 | A → B | Dörtlü kurmak bonus verir; ikisini birden yapan tek bir sıra var. (Nefes) | 62 |
| 40 | Ağır Yük | 9×18 | **H** tanıtım | G1 · K16 · H2 · B5 | A → B | Ağır kasa tek itilmez; önüne kasa koyup birlikte it. | 64 |
| 41 | Ağır Köprü | 9×18 | H + Ç + V | G1 · K16 · H2 · Ç1 · V1(1E) | A → B | Ağır kasayı çukura feda et; vincin getireceği kasayı başka iş için sakla. | 70 |
| 42 | Renkli Teslimat | 9×18 | **V(r)** tanıtım | G1 · P11 · K8 · V1(1r+1E) | A → B | Eksik üçüncü kasayı vinç getirecek; düşeceği karenin yanına ikiliyi hazırla. | 68 |
| 43 | Doğru Yere Düş | 9×19 | V(r) + BB | G1 · P11 · K14 · B5 · V1(2r) | A → B | Vinç kasası yanlış yere düşerse üçlü kurulamaz; kilitle ve zamanla. | 72 |
| 44 | Kesişen Koridorlar | 9×19 | P + BB | G1 · P18 · K18 · B6 | A → B | Görev kasası iki boşluk koridorunun kesişiminden geçer; ikisini aynı anda açma. | 74 |
| 45 | İki Kamyon | 9×16 +O | O + V | A10 · K8 · V1(1E) · T3+T3 | İki kamyon | İlk kamyon gidince ikincisi yanaşır; ilk kamyona hangi kasaları vereceğin önemli. | 72 |
| 46 | Uzak B | 9×20 | P + BB | G1 · P18 · K20 · B7 | A → B | B'ye giden iki yoldan sadece biri açılabilir; yanlışına başlarsan kasalar yetmez. | 76 |
| 47 | Ağır Renk | 9×20 | H + P | G1 · P15 · H2 · K6 | A → B | Ağır kasa üçlünün tam ortasında; onu zincirle kaydırıp yer aç. | 76 |
| 48 | Saat Gibi | 9×20 | V×2 + BB | G1 · K22 · B6 · V2(2E+2E) | A → B | İki vinç, dört bırakma; kilit sırası saat gibi işlemeli. Tolerans 1. | 80 |
| 49 | Nefes | 9×18 | P + BB | G1 · P12 · K16 · B5 | A → B | Kısa ama zarif: tek bir kaydırma iki üçlüyü birden kurar. (Nefes) | 70 |
| 50 | Stajyer Sınavı | 9×22 +O | O + P + BB + V×2 | A10 · P18 · K20 · B6 · V2(2E+1r) · T4 | Kamyona yükle | Arabalar, renkli vinç, boşluklar: hepsini doğru sırada çöz. → **Terfi: Çırak** | 85 |

### 9.5 Çırak (51–75) · Plastik kasalar

| # | Ad | Alan | Mekanik | Sayılar | Görev | Aha fikri | Zorluk |
|---|---|---|---|---|---|---|---|
| 51 | Plastik Kasalar | 9×20 | P + BB | G1 · P15 · K18 · B6 | A → B | Yeni kasalar, bilinen dersler. (Isınma) | 70 |
| 52 | Üç Kanca | 9×20 | **V×3** tanıtım | G1 · K20 · B6 · V3(1E+1E+1E) | A → B | Üç vinç, iki kilit kasası: üçüncüsüyle yarışacaksın. | 76 |
| 53 | Yükü Kullan | 9×20 | V(k) + BB | G1 · K18 · B5 · V2(1k+1E) | A → B | Vincin getirdiği kasa olmadan son boşluk kapanmaz; onu beklemek zorundasın. | 76 |
| 54 | Beşli | 9×20 | P | G1 · P20 (4 renk) · K8 | A → B | Yolun açılmasının tek yolu bir beşli kurmak. | 78 |
| 55 | Otopark Labirenti | 9×16 +O | O | A14 · K8 · T4 | Kamyona yükle | Arabalar zincirleme birbirini kilitliyor; kamyon rotası uzun ve kıvrımlı. | 76 |
| 56 | Çift Görev | 9×22 | **G×2** tanıtım | G2 · P9 · K22 · B7 | İki teslim | İki görev kasası iki ayrı B alanına; birinin yolu diğerinin geçişiyle açılır. | 80 |
| 57 | Ayrılan Yollar | 9×22 | G×2 + V | G2 · K22 · B6 · V1(2E) | İki teslim | Vinç yollardan birini kapatacak; hangi görev kasası önce gitmeli? | 82 |
| 58 | Dönüş Yok | 9×22 | P + BB | G1 · P18 · K22 · B6 | A → B | Görev kasasının arkasına geçebilmek için en son boşluğu sona sakla. | 82 |
| 59 | Tuzak Renk | 9×22 | P (5 renk) | G1 · P20 · K8 | A → B | Beş renkten biri hiç inmeyecek; onu yoldan çekmek gerekir. | 80 |
| 60 | Ağır Duvar | 9×22 | H + BB | G1 · H4 · K18 · B6 | A → B | Ağır kasalardan bir duvar; sadece zincir itmeyle delinir. (Nefes) | 76 |
| 61 | Köprü Zinciri | 9×22 | V(ç)×2 + Ç | G1 · K18 · B5 · Ç2 · V2(1ç+1ç) | A → B | İki çukur, iki vinç; köprülerin geliş sırası ihtiyacının tersi. | 86 |
| 62 | Gizli Üçlü | 9×22 | P + BB | G1 · P18 · K20 · B6 | A → B | Üçlü ancak bir kasa boşluğa kaydırılınca ortaya çıkar. | 86 |
| 63 | Kalabalık | 9×24 | P + BB + V | G1 · P21 · K24 · B7 · V1(2E) | A → B | Çok kasa ama çoğu süs; doğru 6 hamleyi bul. | 88 |
| 64 | Kilit Zinciri | 9×24 | V×2 + P | G1 · P12 · K14 · V2(2E+2E) | A → B | Bir vinci kilitleyen kasa, diğer tarafta bir üçlü için de gerekli. | 90 |
| 65 | Kamyon ve Kanca | 9×18 +O | O + V | A12 · K10 · V1(2E) · T4 | Kamyona yükle | Vinç bir arabanın önüne engel bırakacak; o arabayı ilk gönder. | 86 |
| 66 | Uzun Teslimat | 9×26 | P + BB | G1 · P18 · K26 · B8 | A → B | Üç ekran boyu yol, iki ara durak. | 88 |
| 67 | Renk Ödünç | 9×24 | P + V(r) | G1 · P18 · K8 · V2(2r) | A → B | Bir üçlünün eksiği diğerinden ödünç alınır; vinç sonra yerine koyar. | 92 |
| 68 | Sıkışık | 9×22 | BB yoğun | G1 · K30 · B4 | A → B | 30 kasa, sadece 4 boşluk: kayan blok bulmacası gibi. | 90 |
| 69 | Vinç Altında | 9×24 | V + P | G1 · P18 · K8 · V2(2E+1E) | A → B | Bir üçlü tam düşme karesinde kurulur; indirince kare boşalır ve vinç hemen bırakır. | 92 |
| 70 | Ağır Patlatma | 9×24 | H + P | G1 · P18 · H3 · K8 | A → B | Ağır kasa üçlüye giremez ama üçlüleri bölüyor. (Nefes) | 84 |
| 71 | Dolambaç | 9×24 | G×2 + P + BB | G2 · P15 · K22 · B7 | İki teslim | Bir görev kasası diğerinin rotasında geçici duvar görevi görür. | 94 |
| 72 | Son Boşluk | 9×24 | BB | G1 · K30 · B5 | A → B | Her hamlenin yeri belli; hata payı yok. | 96 |
| 73 | Üç Kanca Dansı | 9×24 | V×3 + BB | G1 · K24 · B6 · V3(2E+2E+1k) | A → B | Beş bırakma, iki kilit kasası, bir yardımcı kasa. Tolerans 1. | 98 |
| 74 | Renk Fırtınası | 9×26 | P (5 renk) | G1 · P27 · K8 | A → B | Dokuz üçlü; sırayı bozan artık kasa bırakır. | 98 |
| 75 | Çırak Sınavı | 9×26 +O | O iç içe + P + BB + V×3 | A14 · P21 · K22 · B6 · V3(1E+1r+1k) · T4 | Kamyona yükle | Arabalar kasaları, kasalar arabaları kapatıyor; üç vinç bekliyor. → **Terfi: Kalfa** | 105 |

### 9.6 Kalfa (76–100) · Metal kasalar

| # | Ad | Alan | Mekanik | Sayılar | Görev | Aha fikri | Zorluk |
|---|---|---|---|---|---|---|---|
| 76 | Metal Kasalar | 9×24 | P + BB | G1 · P18 · K22 · B6 | A → B | Yeni görünüm, sağlam ısınma. (Isınma) | 90 |
| 77 | Karşılıklı | 9×24 | G×2 | G2 · K24 · B6 | İki teslim | İki görev kasası birbirine ters yönde ilerliyor; aynı koridoru sırayla kullanmalılar. | 96 |
| 78 | Dar Geçit Patlatma | 9×24 | P | G1 · P18 · K10 | A → B | Bütün üçlüler tek kişilik bir koridorda kurulmalı. | 98 |
| 79 | Kancadan Görev | 9×24 | **V(g)** tanıtım | G0+1 · K20 · B6 · V1(1g+1E) | A → B | Görev kasası başta yok; vinç getirecek. Düşeceği yerden B'ye yolu önceden hazırla. | 96 |
| 80 | Dev Saha | 9×28 | P + BB + V | G1 · P21 · K28 · B8 · V1(2E) | A → B | Şimdiye kadarki en büyük alan; iki ayrı bölgede iki ayrı plan. | 100 |
| 81 | Artık İşe Yarar | 9×24 | P + V | G1 · P20 · K8 · V1(2E) | A → B | Artık kalan renkli kasa, vinci kilitlemek için tek uygun kasa. | 100 |
| 82 | Yarım Köprü | 9×24 | Ç + V(ç) + H | G1 · H2 · K16 · Ç2 · V1(1ç) | A → B | İki çukur, bir vinç kasası: diğer çukuru ağır kasayla doldurmalısın. | 100 |
| 83 | Kayan Duvar | 9×24 | BB + H | G1 · H4 · K24 · B5 | A → B | Ağır kasa sırası boşluklarla birlikte kaydırılır. | 102 |
| 84 | Karakter Köprüsü | 9×24 | Ç + V(ç) + BB | G1 · K20 · B6 · Ç1 · V1(1ç) | A → B | Köprü görev kasası için değil, **senin** karşıya geçmen için. | 98 |
| 85 | Otopark ve Kancalar | 9×20 +O | O iç içe + V×2 | A14 · K10 · V2(1E+1E) · T4 | Kamyona yükle | İki vinç iki arabanın önüne engel bırakacak; araba sırası vinçlere göre. | 102 |
| 86 | Çapraz Teslim | 9×26 | G×2 + P | G2 · P18 · K16 | İki teslim | İki görev kasasının rotaları kesişiyor; kesişimde kimin önce geçeceği her şeyi belirler. | 104 |
| 87 | Bir Hamle Önce | 9×24 | V + BB | G1 · K24 · B6 · V1(2E) | A → B | Tolerans 0: tek bir boş hamle bile bölümü çözümsüz yapar. | 106 |
| 88 | Patlat ve Kilitle | 9×26 | P + V | G1 · P21 · K10 · V2(2E+1E) | A → B | Üçlüyü indirince boşalan kare bir vincin karesi; hemen başka bir kasayla kilitle. | 106 |
| 89 | Labirent | 9×28 | P + BB + H | G1 · P21 · K26 · H2 · B7 | A → B | Kasalardan bir labirent; çıkış yolu patlatma ile açılır, kaydırma ile genişler. (Nefes) | 98 |
| 90 | Üç Durak | 9×28 | G + P + BB + V | G1 · P18 · K24 · B6 · V1(2E) | A → ara → B | Görev kasası önce ara alana, oradan B'ye; ara alanın kapısı bir üçlüyle açılır. | 108 |
| 91 | Ağır Görev | 9×26 | **G ağır** tanıtım + H | G1(ağır) · K22 · B6 | A → B | Görev kasası bu kez ağır: B'ye kadar zincirle itilmeli. | 108 |
| 92 | Renk Zinciri | 9×26 | P (6 renk) | G1 · P24 · K8 | A → B | Her üçlü bir sonrakinin kasasını serbest bırakıyor; zincirin başını bul. | 110 |
| 93 | Sabır | 9×24 | V(k) + BB | G1 · K24 · B5 · V1(1k) | A → B | Vinç tek kasa getiriyor ama çok geç; o zamana kadar her şeyi hazırla. (Nefes) | 100 |
| 94 | Dört Kanca | 9×26 | V×3 + P | G1 · P15 · K14 · V3(2E+2r+1E) | A → B | Beş bırakmanın ikisi renkli; üçlüleri vinç takvimine göre kur. | 112 |
| 95 | Büyük Otopark | 9×20 +O | O | A18 · K10 · T5 | Kamyona yükle | 18 araba; kamyon rotası iki dönemeçli; çözüm 10 adımlık bir zincir. | 108 |
| 96 | Gece Vardiyası | 9×28 | P + BB + V×2 | G1 · P21 · K26 · B7 · V2(2E+1k) | A → B | Uzun alanda iki ayrı vinç bölgesi; birini tamamen bitirmeden diğerine geçme. | 112 |
| 97 | Ayna | 9×26 | P + BB | G1 · P18 · K22 · B6 | A → B | Alan simetrik görünüyor ama tek bir kasa simetriyi bozuyor; çözüm o kasadan başlıyor. | 112 |
| 98 | İç İçe Üçlüler | 9×26 | P | G1 · P27 (6 renk) · K8 | A → B | Bir üçlünün içine başka bir renk sıkışmış; önce dıştakini çöz. | 114 |
| 99 | Son Prova | 9×28 | G×2 + P + V×2 | G2 · P18 · K20 · V2(2E+1r) | İki teslim | Kalfa sınavından önce son prova: iki görev, iki vinç. | 116 |
| 100 | Kalfa Sınavı | 9×28 +O | hepsi | A16 · G1 · P21 · K24 · H2 · B6 · V3(2E+1r+1k) · T4 | Kamyon + A → B | Önce otopark, sonra görev kasası B'ye, sonra kasalar kamyona. → **Terfi: Usta** | 125 |

### 9.7 Usta (101–125) · Premium kasalar

| # | Ad | Alan | Mekanik | Sayılar | Görev | Aha fikri | Zorluk |
|---|---|---|---|---|---|---|---|
| 101 | Usta İşi | 9×26 | P + BB + V | G1 · P18 · K22 · B6 · V1(2E) | A → B | Premium kasalarla zarif bir ısınma. (Isınma) | 110 |
| 102 | Üç Görev | 9×28 | **G×3** | G3 · K26 · B8 | Üç teslim | Üç görev kasası, üç B alanı; sıra her şey. | 118 |
| 103 | Vinç Ustası | 9×26 | V×3 | G1 · K24 · B6 · V3(2E+2E+2k) | A → B | Altı bırakma, iki kilit kasası; kilitleri doğru anda değiştir. Tolerans 1. | 120 |
| 104 | Kayan Şehir | 9×26 | BB | G1 · K36 · B4 | A → B | 36 kasa, 4 boşluk; kayan blok ustalığı. | 120 |
| 105 | Otopark Ustası | 9×22 +O | O iç içe + V | A20 · K12 · V1(2E) · T5 | Kamyona yükle | 20 araba, kasalar ve bir vinç; üçü de birbirini kilitliyor. | 120 |
| 106 | Altı Renk | 9×28 | P (6 renk) | G1 · P30 · K8 | A → B | On üçlü; her renk bir sonraki rengin kapısı. | 122 |
| 107 | Ağır Ustalık | 9×26 | H + G ağır + BB | G1(ağır) · H4 · K20 · B6 | A → B | Ağır görev kasası, ağır engeller: zincir itmenin zirvesi. | 122 |
| 108 | Uzun Yol II | 9×30 | P + BB + V | G1 · P21 · K28 · B8 · V2(1E+1k) | A → B | Oyunun en uzun alanı; iki vinç iki ayrı uçta. | 124 |
| 109 | Köprüler | 9×28 | Ç×3 + V(ç) + H | G1 · H2 · K18 · Ç3 · V2(1ç+1ç) | A → B | Üç çukur, iki vinç köprüsü, bir ağır kasa: hangisi nereye? | 124 |
| 110 | Tek Nefes | 9×26 | P + BB + V | G1 · P18 · K22 · B6 · V2(2E+1E) | A → B | Tolerans 0, her şey bir arada. | 126 |
| 111 | Renkli Kancalar | 9×28 | V(r)×3 + P | G1 · P15 · K10 · V3(2r+1r+1E) | A → B | Üçlülerin yarısı vinçlerden gelecek; takvimi oku. | 128 |
| 112 | Görev Zinciri | 9×28 | G×2 + V(g) | G1+1 · K24 · B7 · V1(1g+1E) | İki teslim | İkinci görev kasası vinçten geliyor; ilkinin rotası onun düşeceği yeri kapatıyor. | 128 |
| 113 | Kayıp Üçlü | 9×28 | P + BB | G1 · P24 · K24 · B6 | A → B | Bir rengin üçüncü kasası en dipte; onu çıkarmak için yarım alanı kaydır. | 130 |
| 114 | Sıkışan Kamyon | 9×22 +O | O iç içe | A18 · K14 · T6 | İki kamyon | İkinci kamyonun yolu, ilk kamyona yüklenmesi gereken kasayla kapalı. | 128 |
| 115 | Otopark Finali | 9×24 +O | O + V×2 + P | A20 · P12 · K10 · V2(1E+1r) · T5 | Kamyona yükle | Otopark bölümlerinin en büyüğü (sondan bir önceki). | 132 |
| 116 | Patlat ve Taşı | 9×28 | P + G×2 | G2 · P24 · K12 | İki teslim | İki görev kasası arasındaki duvar tamamen renkli kasa; doğru üçlüler iki yolu birden açar. | 132 |
| 117 | Ağır Labirent | 9×30 | H + BB + P | G1 · H6 · P15 · K22 · B6 | A → B | Ağır kasalar labirentin duvarları; patlatma kapıları. | 134 |
| 118 | Sessiz Vinçler | 9×28 | V×3 (geç bırakmalar) | G1 · K26 · B6 · V3(1E+1E+1E) | A → B | Vinçler geç bırakıyor ama hepsi kritik karelere; sakin ve kusursuz plan. (Nefes) | 124 |
| 119 | Dört Durak | 9×30 | G + ara alanlar + P + V | G1 · P21 · K24 · B6 · V2(2E+1k) | A → ara → ara → B | Görev kasası üç kapıdan geçiyor; her kapı farklı mekanikle açılıyor. | 136 |
| 120 | Usta Ayna | 9×28 | P + BB + V | G1 · P24 · K24 · B6 · V2(2E+1E) | A → B | Simetrik iki yarı; vinçler sadece bir yarıda. | 136 |
| 121 | Son Kilit | 9×28 | V×3 + P + H | G1 · P18 · H2 · K12 · V3(2E+2E+1r) | A → B | Tek ağır kasa üç vinçten birini kilitlemeye yetiyor; hangisini seçeceksin? | 138 |
| 122 | Büyük Patlatma | 9×30 | P (6 renk) + BB | G1 · P33 · K16 · B6 | A → B | On bir üçlü; zincirin her halkası bir boşluk birleştirmesi. | 140 |
| 123 | Üç Görev II | 9×30 | G×3 + P + V×2 | G3 · P18 · K22 · V2(2E+1k) | Üç teslim | Üç teslim, iki vinç; birinin teslimi diğerinin vincini kilitliyor. | 142 |
| 124 | Boss'a Doğru | 9×30 | hepsi (otoparksız) | G1 · P24 · K26 · H2 · B7 · V3(2E+1r+1g) | A → B | Final öncesi zirve. | 145 |
| 125 | Crate Boss | 9×30 +O | hepsi | A20 · G2 · P24 · K28 · H3 · B7 · V3(2E+2r+1k) · T5 | Otopark + iki teslim + kamyon | Otoparkı boşalt, iki görev kasasını teslim et, kamyonu yükle. → **Rütbe: Usta (tamamlandı)**, büyük terfi kasası | 150 |

### 9.8 Yenilik takvimi (özet)

Oyuncu her birkaç bölümde yeni bir şey görür:

| Bölüm | Yenilik |
|---|---|
| 1–5 | Temel itme (öğretici) |
| 10 | Görev kasası + boşluk birleştirme |
| 12 | Kule vinç (engel) |
| 13 | Vinci kilitleme |
| 15 | Otopark |
| 16 | Vinç itilebilir kasa getirir |
| 20 | Kasa patlatma |
| 25 | **Terfi: Stajyer** (ahşap kasalar) |
| 28 | Çukur + vinç köprüsü |
| 30 | Patlatma + boşluk birleşik |
| 33 | İki vinç |
| 35 | Otopark iç içe (kasa arabayı kapatır) |
| 36 | İlk kaydırmalı alan |
| 40 | Ağır kasa |
| 42 | Vinç renkli kasa getirir |
| 45 | İki kamyon |
| 50 | **Terfi: Çırak** (plastik kasalar) |
| 52 | Üç vinç |
| 54 | Beşli eşleşme zorunlu |
| 56 | İki görev kasası |
| 75 | **Terfi: Kalfa** (metal kasalar) |
| 79 | Görev kasasını vinç getirir |
| 90 | Ara duraklı teslimat |
| 91 | Ağır görev kasası |
| 100 | **Terfi: Usta** (premium kasalar) |
| 102 | Üç görev kasası |
| 125 | Final |


---

## 10. Bölüm Üretimi ve Doğrulama

### 10.1 Kalıp kütüphanesi

Bölümler rastgele değil, **kalıplarla** kurulur. Her kalıp küçük, doğrulanmış bir bulmaca parçasıdır. Büyük bölümler birkaç kalıbın bir alanda birleştirilmesiyle üretilir. Tablodaki "aha fikri" hangi kalıbın baskın olacağını söyler.

| Kod | Kalıp | Kısa tarif |
|---|---|---|
| S-köşe | Köşe dönüşü | Kasa köşeden döndürülür; karakter önce yanına dolaşır |
| S-sıra | Sıra önemli | Kapıya yakın hedef en son dolar |
| S-park | Geçici park | Bir kasa geçici olarak boş cebe çekilir |
| S-ters | Ters itme | Kasa önce hedeften uzaklaştırılır |
| S-fazla | Fazla kasa | Kasa sayısı > hedef; artığın seçimi |
| BB-yürüt | Boşluk yürütme | Tek boşluk bir uçtan diğerine kare kare taşınır |
| BB-topla | Boşluk toplama | Dağınık boşluklar tek koridorda birleşir |
| BB-dönüş | Dönüş yolu | Karakterin görev kasasının arkasına geçeceği yol son ana kadar korunur |
| BB-tuzak | Tuzak boşluk | En yakın boşluk yanlış seçim |
| V-kapı | Kapanan kapı | Bırakma bir geçidi kapatır; önce kullan |
| V-kilit | Kilitleme | Kasa düşme karesine park edilir |
| V-köprü | Köprü | Vinç kasası çukuru doldurur |
| V-yük | Yardımcı yük | Vinç kasası bulmaca için gereklidir |
| V-renk | Renkli yük | Vinç kasası bir üçlüyü tamamlar |
| V-görev | Görev yükü | Görev kasasını vinç getirir |
| V-kapan | Kendini kapat | Son hamlede vinç oyuncuyu kapatır, sorun değil |
| P-düz | Düz üçlü | Tek sıra |
| P-kesişim | Kesişen üçlü | Yatay + dikey aynı itmede |
| P-artık | Artık seçimi | Renk sayısı 3'ün katı değil |
| P-tuzak | Tuzak üçlü | Hazır görünen üçlüyü indirmek yolu kapatır |
| P-gizli | Gizli üçlü | Üçlü ancak bir kaydırmadan sonra oluşur |
| P-zincir | Renk zinciri | Her üçlü bir sonrakinin kasasını serbest bırakır |
| O-sıra | Araba sırası | Arabalar birbirini kilitler; doğru sıra |
| O-kasa | Kasa arabayı kapatır | Önce kasa itilir |
| O-araba | Araba kasayı kapatır | Önce araba gönderilir |
| O-vinç | Vinç arabayı kapatır | Engel düşmeden önce o araba gönderilir |
| H-zincir | Zincir itme | Ağır kasa önüne konan kasayla itilir |

### 10.2 Üretim yöntemleri

Büyük alanlarda bilgisayarın her olasılığı denemesi mümkün olmadığı için **çözümden geriye doğru üretim** kullanılır:

1. **Klasik ve boşluk birleştirme (geriye oynatma):** Önce çözülmüş durumu kur (kasalar hedefte, görev kasası B'de, koridor açık). Sonra **ters hamleler** uygula: karakter kasayı **çeker** (itmenin tersi). Rastgele ama kontrollü ters hamlelerle başlangıç durumu elde edilir. Bu yöntemle üretilen bölüm **kesinlikle çözülebilir**.
2. **Kasa patlatma (yapıcı üretim):** Önce yolun tıkalı hâlini çiz. Yolun üstüne üçlüleri ve onları kuracak "eksik kasaları" yerleştir. Kasaları ters hamlelerle dağıtırken hiçbir ara durumda kendiliğinden üçlü oluşmamasına dikkat et (oluşursa o ters hamleyi geri al).
3. **Vinç ekleme (zamanlama analizi):** Vinçsiz çözümün hamle dizisini çıkar. Bırakma karesini ve zamanını, çözümün kritik bir anına denk getir (örneğin görev kasasının geçitten geçtiği hamle + tolerans). Sonra vinçli hâli doğrula ve toleransı ölç.
4. **Otopark (bağımlılık grafiği):** Arabaların "kim kimi kapatıyor" ilişkisini önce bir zincir/ağaç olarak tasarla (hangi araba önce gitmeli), sonra yerleşimi bu ilişkiden kur. Çözüm adım sayısı = zincirin uzunluğu.
5. **Birleştirme:** Büyük bölümlerde alan bölgelere ayrılır; her bölge bir kalıptır. Bölgeler arasındaki geçişler (kapılar) bir sonraki bölgenin önkoşuludur.

### 10.3 Doğrulama

| Kontrol | Yöntem |
|---|---|
| Çözülebilirlik | Geriye üretim garantisi + ileri çözücü (sıkışma tespiti + A*; süre sınırı 60 sn) |
| En kısa çözüm (yıldız için) | Küçük ve orta alanlarda kesin; büyük alanlarda "bilinen en iyi" (çözücünün bulduğu en iyi) kullanılır, 3 yıldız sınırı **bilinen en iyi + %10** |
| Fikir | **Aceleci oyuncu testi:** kasaları sadece hedefe yaklaştıran itmelerle çözülebiliyorsa → reddet |
| Mekanik önemli mi? | Vinç kaldırılınca, patlatma kapatılınca ya da araba sırası serbest bırakılınca çözüm değişmiyorsa → reddet |
| Zorluk | Tablodaki hedefin ±10'u içinde |
| Tolerans | Vinçli bölümlerde tablodaki değer |
| Yerleşim | `Validate Layout` (§3.5) hatasız |
| Okunabilirlik | Alan genişliği ≤ 9, varsayılan yakınlaştırmada kare ≥ 24 px (çapraz kamerada) |

### 10.4 Onay süreci

- Bölümler **10'arlı paketler** hâlinde üretilir (1–10, 11–20, …).
- Her bölüm yeri için **3 aday**; her aday için rapor: harita, zorluk, tolerans, kullanılan kalıplar, en kısa (ya da bilinen en iyi) çözüm, aha fikrinin bulmacada nerede ortaya çıktığı.
- Oyun sahibi paketi oynar ve her yer için birini seçer. Seçilmeyen paket `Resources/Levels/`'a girmez.


### 10.5 Kalite filtreleri

Öğretici bölümler (1–5) hariç her bölüm şunlardan geçmeli:

| Filtre | Kural |
|---|---|
| Zorluk | §9 tablosundaki hedefin ±10'u. Zorluk puanı: `0.3·Hamle + 0.5·İtme + 4·log10(Durum) + 6·FazlaKasa + 5·MekanikSayısı + 20·TuzakOranı + 4·max(0, 5 − Tolerans)` (büyük alanlarda durum sayısı yerine çözücünün tahmini) |
| Aceleci oyuncu | Kasaları sadece hedefe yaklaştıran itmelerle çözülebiliyorsa → reddet |
| Mekanik önemli mi? | Vinç kaldırılınca / patlatma kapatılınca / araba sırası serbest bırakılınca çözüm değişmiyorsa → reddet |
| Tolerans | Vinçli bölümlerde tablodaki değer (boşa harcanabilecek en fazla hamle) |
| Fikir | `design.idea` dolu; çözümde bu fikir gerçekten kullanılıyor |
| Çeşitlilik | Çözüm imzası (itme sırası, hangi kasa nereye, makinelerin rolü) daha önce onaylanmış bir bölüme çok benziyorsa düşük puan |
| Yerleşim | `Validate Layout` hatasız |
| Boyut | §8 |

---

## 11. Ana Sayfa: Boss'un Arazisi

### 11.1 İlk açılış

- İlk kez açan oyuncu ana sayfayı görmez; **doğrudan bölüm 1**'e girer.
- Bölüm 1 bitince ana sayfa ilk kez açılır: kamyon gelir, ilk teslimat kasasını bırakır. Rehber kart: "Bu kasa senin. Hadi açalım!" İlk kasa ücretsiz açılır (öğretici).

### 11.2 Düzen (390×844 referans ekran)

**Sahne (canlı, 3D):**
- Kuş bakışı, oyun kamerasıyla aynı çapraz dil (yaw ≈25°, eğim ≈50°, daha yüksekten) şehir. Ortada **Boss'un arazileri**; etrafında şehrin geri kalanı (yollar, binalar, park alanları; oyuncak stil).
- Başta tek, küçük, boş bir arsa. Eşya yerleştirildikçe dolar; terfilerde yeni araziler açılır.
- Yollarda oyuncak arabalar gezer; karakter arazide durup el sallar (bekleme animasyonu).
- İki parmakla gezinme ve yakınlaştırma.

**Üst:**
- Sol: **rütbe rozeti** + rütbe adı + bir sonraki rütbeye ilerleme çubuğu (sayısız).
- Sağ: **CB Coin** bakiyesi.

**Sahnenin içinde:**
- Arazinin girişinde **bekleyen kasa yığını**; üstünde sayı rozeti. Dokununca kasa açma ekranı.
- Açılmış ama yerleştirilmemiş eşya varsa "Yerleştir" baloncuğu.

**Sağ kenar:** "**+30 CB Coin** · Reklam izle · 2/3" butonu.

**Sol kenar:** **Gardırop** (karakter kıyafetleri), **Düzenle** (yerleştirme modu), **Ayarlar**.

**Alt:** Büyük turuncu **OYNA** butonu; altında sıradaki bölümün **adı** (numara yok).

### 11.3 Bölüm sonu akışı

1. Kazanma paneli: yıldızlar, kazanılan CB Coin (sayarak artar), patlatma bonusları, "Reklam izle, ×2" butonu, **Devam**.
2. Kamera şehrin üstünden yükselip Boss'un arazisine uçar.
3. **Kamyon** gelir, teslimat kasasını yığına bırakır (oyuncak zıplama, bulut).
4. CB Coin yetiyorsa "Aç" butonu parlar; yetmiyorsa "Reklam izle ve aç" seçeneği (§13).
5. Oyuncu kasayı açar (§12) ya da OYNA'ya basar.

### 11.4 Terfi anı

25, 50, 75, 100, 125. bölüm sonunda ana sayfada tam ekran kutlama: rozet büyür, yeni kasa görünümü tanıtılır, **terfi kasası** iner (ücretsiz açılır, özel eşya), **yeni arazi** açılır (kamera yeni araziyi gösterir).

---

## 12. Teslimat Kasaları, Eşyalar ve Yerleştirme

### 12.1 Kasa kazanma

| Durum | Kasa |
|---|---|
| Bir bölümü **ilk kez** bitirmek | 1 teslimat kasası |
| Terfi bölümleri (25, 50, 75, 100, 125) | ek olarak 1 **terfi kasası** (ücretsiz açılır) |
| Tekrar oynama | Kasa yok |

- Kasa açma maliyeti: **30 CB Coin** (§13).
- Açılamayan kasalar **kaybolmaz**; yığında bekler (sınırsız).
- Teslimat kasasının görünümü o anki rütbenin kasası.

### 12.2 Kasa açma

1. Oyuncu yığındaki kasaya dokunur → kasa ekranın ortasına gelir, sallanır.
2. "Aç" (30 CB Coin) ya da "Reklam izle ve aç" (CB Coin yetmiyorsa).
3. Kapak fırlar (konfeti, yıldız, ses) ve **3 eşya** kart olarak çıkar (eşyanın küçük 3D önizlemesi döner).
4. Oyuncu birini seçer. Seçilmeyenler kaybolur.
5. Seçilen eşya "Yerleştir" moduna geçer (§12.4); oyuncu hemen yerleştirebilir ya da "Sonra" diyerek envantere atar.

**Seçenek kuralları:**
- 3 seçenek, **o anki rütbenin setinden** ve oyuncunun henüz sahip olmadığı eşyalardan seçilir. Set bittiyse önceki rütbelerin eksik eşyaları; hepsi bittiyse "dekor" eşyalarından (çiçek, ağaç, lamba… bunlar tekrar alınabilir).
- **Aynı eşya ikinci kez çıkmaz** (dekor hariç).
- **Garanti:** Her 5 kasada en az bir seçenek "büyük" eşyadır (ev, araç, büyük yapı).
- 3 seçenek genellikle farklı türlerden: 1 büyük, 1 orta, 1 küçük.
- Terfi kasası: o rütbenin **özel eşyası** sabit seçenektir + 2 rastgele büyük eşya.

### 12.3 Eşya setleri (taslak; model üretimi §19)

| Rütbe | Set: Boss'un hayatı | Terfi eşyası |
|---|---|---|
| Öğrenci | Küçük tek katlı ev, bisiklet, bank, saksı grubu, sokak lambası, beyaz çit, posta kutusu, köpek kulübesi, mangal, hamak, salıncak, çiçek tarhı, küçük ağaç, sebze bahçesi, limonata standı, kuş evi | Küçük oyuncak araba (hatchback) |
| Stajyer | İki katlı ev, scooter, garaj, oyun parkı, şişme havuz, basketbol potası, kamelya, fıskiye (küçük), food truck, kafe tezgâhı, ağaç ev, bahçe gölü, büyük ağaç, çiçek kemeri, trambolin, bahçe cüce heykeli | Aile arabası (station) |
| Çırak | Küçük villa, yüzme havuzu, tenis kortu, motosiklet, pikap, sera, gölet + köprü, rüzgâr gülü, Boss otoparkı, minik kafe, açık hava sineması, kamp ateşi, palmiye, heykel (kasa), bisiklet yolu | **Boss'un ilk ofisi** (küçük ofis binası) |
| Kalfa | Büyük villa, spor araba, helikopter pisti, yat (su), iskele (su kenarı), depo binası, Boss kamyonu, büyük fıskiye, mini golf, restoran, otel (küçük), park meydanı, heykel (rütbe), ışıklı ağaç sırası, saat kulesi | **Crate Boss Lojistik binası** |
| Usta | Gökdelen ofis, helikopter, lüks havuz + şezlonglar, golf sahası, özel jet + hangar, dev yat, opera/konser sahnesi, dev altın kasa heykeli, gökyüzü köprüsü, dönme dolap, botanik bahçe, stadyum (küçük), müze, gece kulübü ışıkları, marina | **Boss Kulesi** (şehrin en yüksek binası) |

Her eşyanın veri kaydı: kimlik, ad (dil tablosu anahtarı), rütbe, boyut sınıfı (küçük/orta/büyük/özel), **taban alanı** (kare olarak), **yerleşim türü** (§12.4), model prefabı, döndürülebilir mi.

### 12.4 Yerleştirme (özgür ama kurallı)

- Arazi kareli (1 birim = 1 kare). Eşyaların taban alanı var (ev 3×3, araba 1×2, ağaç 1×1…).
- **Düzenle modu:** Zemine ince ızgara gelir. Eşyayı sürükle; dokunarak 90° döndür. Eşyanın altı **yeşil** (uygun) ya da **kırmızı** (uygun değil) yanar. Kırmızıdayken bırakılırsa eski yerine (ya da envantere) döner.
- **Yerleşim türleri:**

| Tür | Nereye konabilir |
|---|---|
| Bina | Arazi zemini; **kapı tarafı** bir yol/patika karesine bakmalı |
| Araç | Yol, patika, garaj ya da otopark karesi |
| Su aracı (yat, tekne) | Su karesi (sadece su kenarı arazisinde) |
| Su kenarı (iskele, marina) | Suya bitişik arazi kenarı |
| Bahçe (ağaç, çiçek, çit, bank…) | Arazi zemini (çim) |
| Zemin süsü (havuz, kort, golf…) | Arazi zemini, tamamı arazinin içinde |

- **Patika/yol karoları:** Düzenle modunda **ücretsiz ve sınırsız** yerleştirilebilir (Boss kendi yollarını çizer). Araziyi şehrin yoluna bağlayan giriş hazırdır.
- Hiçbir eşya iç içe geçemez, arazinin dışına taşamaz, şehrin yoluna konamaz.
- §3.3 mantıklı yerleşim kuralları burada da geçerli; `Validate Layout` ana sayfa için de çalışır (oyuncunun yerleşimi kurallar sayesinde zaten geçerli olur).
- Yerleşim değişikliği her zaman ücretsiz; eşya envantere geri alınabilir.
- Envanter: alınmış ama yerleştirilmemiş eşyalar (alt panel).

### 12.5 Araziler

| Arazi | Açılış | Boyut | Özellik |
|---|---|---|---|
| Başlangıç arsası | Bölüm 1 sonrası | 10×10 | Mahalle içinde |
| Bahçe arazisi | Stajyer olunca | 12×10 | Başlangıç arsasının yanında |
| Göl kenarı | Çırak olunca | 12×12 | Küçük gölet (su karesi) |
| Sahil arazisi | Kalfa olunca | 14×12 | Deniz kenarı (yat, iskele) |
| Şehir merkezi arsası | Usta olunca | 14×14 | Gökdelen ve büyük yapılar |

Araziler şehrin içinde, birbirine yollarla bağlı. Ana sayfa kamerası açık arazileri gezer.

### 12.6 Kayıt

Eşyalar, yerleşimler (eşya kimliği + konum + yön), envanter, bekleyen kasalar, araziler `ProgressStore` ile cihazda; bozuk kayıtta yedekten dönülür. Bulut kaydı ileride.

---

## 13. CB Coin Ekonomisi

Oyunun tek parası **CB Coin** (her dilde aynı ad; simgesi üstünde "CB" yazan altın jeton). **Gerçek parayla satın alınamaz.** Değerler `EconomyConfig` (ScriptableObject) dosyasında; sonra dengelenecek.

### 13.1 Kazanma

| Kaynak | CB Coin |
|---|---|
| Bölüm ilk geçiş: ★ | 10 |
| ★★ | 18 |
| ★★★ | **30** (bir kasa açmaya tam yeter) |
| Patlatma 4'lü / 5'li bonus | +5 / +10 |
| Bölüm sonu "Reklam izle, ×2" | kazanılanı ikiye katlar (bölüm başına 1 kez) |
| Ana sayfa "+30 CB Coin" reklamı | +30 (günde en fazla 3) |
| Günlük giriş hediyesi (7 günlük seri) | 10, 15, 20, 25, 30, 40, 60 (seri bozulursa başa döner) |
| Tekrar oynama | Sadece daha iyi yıldızın farkı (ör. ★ → ★★★: +20) |

**İlke:** 3 yıldız alan oyuncu kasasını hemen açar. 1–2 yıldız alan oyuncu ya reklam izler (×2) ya da biriktirir. Kasa asla kaybolmaz.

### 13.2 Harcama

| Harcama | CB Coin |
|---|---|
| Teslimat kasası açma | 30 |
| İpucu (ücretsiz hak bitince) | 50 |
| Çözümü izle | 300 |
| Gardırop: baret rengi, yelek rengi, eldiven, şapka, ayakkabı | 100–500 |
| Eşya rengini değiştirme (bina/araç rengi seçenekleri) | 40 |

**Asla ücretli olmayanlar:** Geri Al, Yeniden Başlat, bölüm açmak, eşya yerleştirme/taşıma, patika karoları.

---

## 14. İpucu

- İpucu **sadece bir sonraki hamleyi** gösterir: hangi kasa hangi yöne (karakterin durması gereken kare dahil) ya da hangi araba hangi yöne.
- Oyuncunun **o anki durumundan** hesaplanır (mevcut `PositionAnalysis`).
- Durum çözülemez hâle geldiyse (kilitlenme): **"N hamle geri al"** der, Geri Al butonunu parlatır (geri alma yığınındaki durumlar sondan başa denenir).
- Hesaplanamazsa (çok büyük durum, süre sınırı): "Bu konumda ipucu hesaplanamadı, birkaç hamle geri almayı dene". Yanlış ipucu asla verilmez.
- **Haklar:** yeni oyuncu 3 ücretsiz; her gün ilk girişte +1 (en fazla 3). Hak bitince seçim: **50 CB Coin** ya da **reklam izle**.
- **Çözümü izle:** 300 CB Coin; bütün çözümü mevcut durumdan oynatır.

---

## 15. Reklam

### 15.1 Ödüllü reklam (oyuncu isterse)

| Yer | Ödül | Sınır |
|---|---|---|
| İpucu | 1 ipucu | — |
| Kazanma paneli | Bölüm CB Coin'i ×2 | bölüm başına 1 |
| Kasa açma (CB Coin yetmiyorsa) | Kasayı aç | günde 5 |
| Ana sayfa | +30 CB Coin | günde 3 |

Ödül butonda önceden yazar; reklam sadece oyuncu basınca açılır; ödül sadece reklam **tamamlanınca** verilir; reklama tıklamak ödüllendirilmez.

### 15.2 Geçiş reklamı

- **İlk 8 bölüm reklamsız.**
- Sadece **bölüm bitince**, kazanma panelinde "Devam"a basıldıktan sonra, ana sayfaya geçerken.
- **Asla:** bölüm ortasında, yeniden başlatmada, uygulama açılışında/çıkışında, ana sayfada gezinirken, kasa açarken.
- En az **3 bölümde bir** ve en az **120 sn** arayla; oturum başına en fazla **6**.
- Son **5 dakikada** ödüllü reklam izlenmişse gösterilmez.
- Reklamdan önce 0,3 sn kısa geçiş ekranı (yanlış tıklamayı önler).

### 15.3 Yok

- Banner yok. Satın alma (reklamsız paket dahil) yok. Uygulama açılış reklamı yok.

### 15.4 Yasal ve platform

- Hedef kitle **13+** (Google Play'de 13 ve üstü).
- Avrupa için veri izni (GDPR): AppLovin MAX + Google UMP. iPhone takip izni (ATT). Türkiye KVKK aydınlatma metni; Ayarlar'da Gizlilik.
- Gizlilik politikası taslağı: `Docs/GIZLILIK_POLITIKASI_TASLAK.md` (avukat kontrolü gerekir notuyla).
- Geliştirmede **her zaman test reklamı**. Şimdilik `TestRewardedAdService` ve `TestInterstitialAdService` (tam ekran "TEST REKLAMI" paneli; ödüllüde 5 sn geri sayım). AppLovin MAX sonra eklenecek; gerçek kimlikler sadece yayın derlemesinde (kontrol listesine yazılsın).
- Kod: `IRewardedAdService`, `IInterstitialAdService`, `AdServiceLocator`, `AdPolicy` (§15.2 kurallarının tek yeri), `AdConfig`, `AdPlacement` (enum), `AnalyticsHub.Log(...)` (şimdilik konsol).

---

## 16. Oyuncak Efektler

Genel: varışta **ease-out-back**, düşüşte **ease-in**, yere değince **ezilip esneme**. Toz = beyaz yuvarlak **çizgi film bulutları**. Parıltı = küçük yıldız ve şeker renkli konfeti. Sesler plastik "tık", "pıt", "boing"; sert ses yok. Titreşim kısa ve sadece önemli anlarda. Animasyon sürerken oyuncu girişi sıraya alınır (en fazla 1 bekleyen).

| Olay | Animasyon | Efekt | Ses |
|---|---|---|---|
| Yürüme | kare başına ≈0,12 sn, adımda hafif zıplama | — | "pıt" |
| Kasa itme | 0,12 sn kayma; varınca ezilip esneme + küçük sallanma | 2–3 bulut | sürtünme + "tık" |
| Ağır kasa (zincir) | 0,18 sn; daha ağır ezilme, çok hafif kamera titremesi | bulut | tok "tok" |
| Kasa hedefe / görev kasası B'ye | 0,15 sn zıplayıp oturur | ışık halkası + yıldızlar | gamın bir sonraki notası |
| Vinç bırakma | kanca iner, yük salınır, düşer (0,35 sn), **2 kez zıplar**, ezilip esneme | büyük bulut halkası + yıldız | "vınn" + "boing-tık" |
| Kasa çukura düşer | küçülerek düşer (0,25 sn), çukur "pat" kapanır | bulut | "pıt-tık" |
| Patlatma | kasalar sırayla şişer (0,06 sn arayla), kapaklar açılır, kasalar dönerek küçülüp iner, kapaklar zıplayıp kapanır (≈0,7 sn) | konfeti + yıldız (4'lü/5'li daha büyük) | yükselen 3 nota + "pop" |
| Araba gider | kalkışta hafif öne eğilme, yolda uzaklaşır | egzoz bulutları | "vın" |
| Araba çarpar | çarpınca ezilir, geri zıplayıp yerine döner | küçük yıldızlar | "boing" (komik) |
| Kamyon gelir / gider | geri geri yanaşır ("bip bip"); dolunca kapak kapanır, zıplayarak kalkar | egzoz bulutları | korna "düt düt" |
| Bölüm kazanma | hedefler sırayla parlar, karakter zafer dansı | konfeti yağmuru | kısa neşeli melodi |
| Kasa açma | sallanma → kapak fırlar → 3 kart açılır | konfeti, ışık hüzmesi | "ta-da" |
| Eşya yerleştirme | eşya paraşütle iner, 2 kez zıplar | bulut + yıldız | "boing" |
| CB Coin kazanma | jetonlar ekrandan sayaca uçar | parıltı | şıngırtı |
| Terfi | rozet büyür, ışık hüzmeleri | büyük konfeti | fanfar (kısa) |

---

## 17. Arayüz

### 17.1 Oyun ekranı

- **Üst çubuk:** sol "Ana sayfa" (ev ikonu), orta **bölüm adı** + küçük rütbe rozeti (numara yok), sağ "Duraklat".
- **Sayaç hapları:** Hamle · En iyi · görev durumu (hedef a/b, teslim a/b ya da kamyon a/b) · vinç çipleri (`A 3`, kilitliyse `A ⏸`) · otoparkta çarpışma sayısı. Emoji yok, çizim ikonlar.
- **Görev kartı:** bölüm başında 2 sn ("Görev kasasını B'ye götür", "Kamyonu yükle"); yeni mekanik tanıtım bölümlerinde büyük, resimli kart.
- **Alt kontrol çubuğu:** Yeniden · **Geri Al** (en büyük) · İpucu (hak rozeti; hak yoksa CB Coin/reklam seçimi) · Genel bakış (büyük alanlarda).
- Dokunma alanları ≥ 44 px; kontrast ≥ 4,5:1.

### 17.2 Kazanma paneli

Yıldızlar, Hamlen / En iyi, kazanılan CB Coin, bonuslar, "Reklam izle, ×2", **Devam** (ana sayfaya, kamyon akışı §11.3), "Tekrar oyna". Numara yok.

### 17.3 Diğer ekranlar

Ana sayfa (§11), kasa açma (§12.2), düzenle modu ve envanter (§12.4), gardırop (§13.2), terfi (§11.4), ayarlar (dil, ses, müzik, titreşim, gizlilik, izinler).

---

## 18. Dil Desteği

| Dil | Kod | Not |
|---|---|---|
| Türkçe | `tr` | Kaynak dil. `İ/ı` büyük harf kuralı (kültüre göre `ToUpper`). |
| İngilizce | `en` | Yedek dil |
| İspanyolca | `es` | |
| Arapça | `ar` | Sağdan sola; harf bağlama için **RTLTMPro** (açık kaynak, MIT) ya da eşdeğeri. Üst çubuk ve paneller aynalanır; **oyun alanı ve alt kontrol çubuğu aynalanmaz**. |
| Almanca | `de` | Uzun kelimeler: butonlar ≤ 12 karakter hedefi |
| İtalyanca | `it` | |
| Fransızca | `fr` | |
| Japonca | `ja` | Yedek font: Noto Sans JP ya da M PLUS Rounded 1c |
| Korece | `ko` | Yedek font: Noto Sans KR ya da yuvarlak bir Kore fontu |

- Unity **Localization** paketi; bütün metinler String Table'da (koda gömülü yazı yok).
- Bölüm adları (`level.<id>.name`), rütbeler, eşya adları, görev kartları, ipucu mesajları, reklam butonları, izin metinleri dahil.
- **"Crate Boss" ve "CB Coin" çevrilmez.**
- Fontlar: Fredoka (başlık), Nunito (metin) + yedek fontlar (Noto Sans Arabic / JP / KR). Japonca ve Korece için dinamik ya da tablodan üretilmiş statik atlas; uygulama boyutunu raporla.
- Sayılar her dilde 0–9.
- Cihaz dili desteklenmiyorsa İngilizce; Ayarlar'dan değiştirilebilir (her dil kendi adıyla listelenir).
- Çeviriler: Türkçeyi ve diğer 8 dili sen hazırla, hepsini "taslak" işaretle (`Docs/CEVIRI_DURUMU.md`). Yayından önce ana dili konuşan biri kontrol edecek.
- Mağaza alt başlığı taslakları `Docs/MAGAZA_METINLERI.md`: tr *Kutu İtme Bulmacası*, en *Box Push Puzzle*, es *Puzle de empujar cajas*, ar *لغز دفع الصناديق*, de *Kisten-Schiebe-Rätsel*, it *Puzzle spingi le casse*, fr *Puzzle de caisses à pousser*, ja *箱押しパズル*, ko *상자 밀기 퍼즐*.

---

## 19. Modeller, Tripo, Karakter ve Animasyonlar

### 19.1 Tripo ile üretim kuralları

- Tripo sana bağlı; modelleri sen üretirsin.
- Her prompt **ortak stil cümlesiyle** biter:
  ```
  cute toy style, chunky rounded shapes, thick soft bevels on every edge, smooth glossy molded plastic material, clean solid colors, no dirt, no rust, no scratches, no tiny details, simple readable silhouette, bright candy colors, soft studio lighting, mobile casual game asset in the style of Parking Jam, isolated on white background, no text
  ```
- Ayarlar: PBR açık, aydınlatmayı kaldır açık, doku 2K, üçgen topoloji.
- Model başına **en fazla 3 deneme**; olmazsa oyun sahibine sor.
- Her modeli oyun kamerasıyla bir önizleme sahnesinde çek; §2.2'ye göre kontrol et; `Docs/model_onizleme/` altına kaydet.
- Ham dosyalar `E:\OFIGAME\OYUNLAR\3D_Modeller\oyuncak\` altına; proje içine kopyala, `Normalize Models` ile ölçekle (tek ölçek, esnetme yok), `MODEL_RAPORU.md`'yi güncelle (üçgen, boyut, kullanılan kredi).

### 19.2 Model listesi ve öncelik

| Öncelik | Model | Poligon | Prompt (başı; sonuna stil cümlesi) |
|---|---|---|---|
| 1 | Karton koli (Öğrenci) | 1.000 | `A toy cardboard box, cube shaped, light brown with a strip of tape across the top and down the sides, slightly puffy rounded shape.` |
| 1 | Ahşap kasa (Stajyer) | 1.000 | `A toy wooden crate, cube shaped, four wide horizontal planks on each side with a thick frame around the edges, warm honey-orange color, simple painted wood look without realistic grain.` |
| 1 | Plastik kasa (Çırak) | 1.000 | `A toy plastic crate, cube shaped, bright colored plastic with rounded rectangular holes on the sides and thick rounded rim.` |
| 1 | Metal kasa (Kalfa) | 1.000 | `A toy metal crate, cube shaped, smooth painted sheet metal look with big round bolts on the corners and a thick rounded frame.` |
| 1 | Premium kasa (Usta) | 1.000 | `A toy premium crate, cube shaped, cream colored body with golden rounded corner caps and a satin ribbon tied around it.` |
| 1 | Görev kasası (5 rütbe malzemesi) | 1.000 | `A toy delivery crate, cube shaped, bright red with a big white upward arrow on each side and thick rounded edges, <MATERIAL> look.` |
| 1 | Ağır kasa | 1.200 | `A toy heavy steel block with a perfectly square footprint, equal width and depth, low and wide, dark slate grey color with thick yellow and black diagonal stripes around the sides, a few big round bolts on the corners.` |
| 1 | Sabit konteyner (vinç engeli) | 1.200 | `A toy small shipping container block with a perfectly square footprint, chunky rounded cool blue-grey corrugated sides, two simple door bars on the front.` |
| 1 | Hedef plakası | 500 | `A toy flat square floor pad, very thin, white rounded frame with a glowing mint-green inner border and a small square in the center, top view.` |
| 1 | Teslim alanı karosu (B) | 500 | `A toy flat square floor tile for a delivery zone, very thin, white rounded frame with a glowing amber inner border and a small arrow in the center, top view.` |
| 1 | Bariyer: düz / köşe / uç | 800 | `A toy yellow safety guardrail segment, two thick rounded horizontal rails on two short chunky posts, bright yellow with black stripes on the posts, straight piece exactly 1 unit long, side view.` (köşe: `90 degree corner piece, top three-quarter view`; uç: `one short chunky post with a rounded cap and short stubs of two rails`) |
| 1 | Zemin kapağı (patlatma, çukur) | 800 | `A toy square floor hatch, thick yellow and black striped rounded frame, a flat grey lid as a separate hinged part, top three-quarter view.` |
| 2 | Vinç: gövde, araba, kanca, direk parçası, bom parçası | 6.000 / 1.000 | gövde: `A toy tower crane top section, bright yellow chunky simplified lattice mast, small rounded operator cabin with a light blue window, long horizontal jib arm, short counter-jib with grey rounded counterweight blocks, no hook, no cables, side view.` · direk parçası: `A single modular toy tower crane mast section, square chunky yellow lattice column with diagonal braces on all sides, flat ends so identical sections stack seamlessly, height 1.5 times its width.` · bom parçası: `A single modular toy tower crane jib section, horizontal triangular chunky yellow lattice beam, flat open ends so identical sections connect in a straight line, a rail underneath for a trolley, length 2 times its height.` · araba: `A small toy crane trolley, chunky yellow box with four little wheels on top and a pulley underneath.` · kanca: `A chunky toy crane hook block, yellow and black striped rounded pulley housing with a big rounded grey hook below it.` |
| 2 | Kamyon (açık yükleme kasalı) | 3.000 | `A toy delivery truck, chunky rounded red cab and a white open cargo box at the back with a fold-down rear door, big round black wheels, three-quarter top view.` |
| 2 | Saha içi engel setleri (rütbe başına 3–4) | 800 | ör. `A toy group of three traffic cones on a small square base.` · `A toy stack of three wooden pallets, square footprint.` · `A toy group of three oil barrels, chunky rounded, square footprint.` · `A toy generator box, chunky rounded, square footprint.` · `A toy elegant flower planter box, square footprint.` |
| 3 | Şehir: ev (3), dükkân, ofis, ağaç (3), bank, lamba, çit, dur tabelası, çiçeklik | 1.500–4.000 | `A toy <OBJECT>, chunky rounded proportions, <COLOR DETAILS>.` |
| — | Arabalar (park, otopark, trafik) | — | **Model üretilmez.** Mevcut kodla çizilen arabalar kullanılır (oyun sahibi beğendi). Otopark arabaları da aynı kodla 1×2 çizilir. |
| 4 | Boss eşyaları (§12.3, rütbe sırasıyla) | 1.000–6.000 | `A toy <ITEM>, chunky rounded proportions, <COLOR DETAILS>, cute miniature city style.` |

Patlatma kasaları: mevcut `BoxMatch_1..5` (plastik market kasası) kullanılır; 6. renk (pembe) eksikse aynı modelle malzemeden renklendir. Sembol yok.

**Prompt kaynağı:** Ayrıntılı ve güncel promptlar `Docs/MODEL_URETIM_LISTESI.md`'dedir (81 model); bu tablo özettir, çelişirse liste geçerlidir. Stil cümlesi her prompta eklenir: *"sturdy hard injection-molded plastic toy… NOT melted, NOT blobby"* (listede tam hâli). Oyun sahibinin elle ürettiği modeller `3D_Modeller\oyuncak\sahip\` klasöründedir; önce bunlara bak, varsa yeniden üretme. `3D_Modeller`'daki dosyaları değiştirme, projeye kopyala.

### 19.3 Karakter (mevcut, korunur)

- `worker` (Mixamo, Humanoid). Yeniden üretilmez; malzemesi §2.4'e çekilir.
- Animasyonlar (mevcut): Walk, Push, PushHeavy, LookAround, Happy, Victory, Think. Bekleme: oyun sahibi `Breathing Idle` indirene kadar mevcut geçici çözüm.
- Kök hareketi kapalı; hareketi kod yapar. Yürüme ve itme hızı oyun temposuna uyarlanır.
- Ek kullanımlar: araba gönderirken karakter işaret eder (`Think` ya da kısa el hareketi); ana sayfada el sallama (`Happy`).

---

## 20. Kaldırılanlar

Aşağıdakiler oyundan **tamamen çıkarılır** (kod, bölüm verisi, testler, arayüz, dil anahtarları). Önce silinecekleri listeleyip raporla, sonra sil:

- **Forklift** (kural, çözücü, görünüm, testler)
- Kedi, sipariş renkleri, teslim sırası, vardiya çıkışı, kantar ve bariyer kapısı, bagaj bandı, büyüyen saha, altın kasa, "yolu aç" (`road`/`path` görevleri), buz kasa
- Semtler (`world` temaları), unvan sistemi, semt haritası, günlük bulmaca
- **Prim** (yerine CB Coin), primle eşya alınan **depo dükkânı** (yerine §11–§12)
- Eski fantastik temalar (Atölye, Buzul, Maden…), kaide/diorama görünümü (`PedestalBuilder`), robot karakter (`RobotView`, `RobotFaceTextures`)
- Eski konsept görselleri (`Docs/Tasarim/konsept/`)

Eski bölüm dosyaları ve aday klasörleri (`Docs/Tasarim/kroki_bolumler`, `vinc_bolumler`, `aday_bolumler`): yeni plana uyan bir harita varsa §9'daki uygun yere aday olarak taşı; gerisini sil. Ne yaptığını raporla.

---

## 21. Testler (EditMode)

**Temel:** yürüme, itme, iki kasa itilemez, ağır zincir, çukur doldurma, geri alma (tüm sistemlerle), başarısız hamle sayılmaz.

**Görev kasası:** B'ye girince teslim ve kilit; yanlış B'de teslim sayılmaz; ara durak kuralı; ağır görev kasası zinciri; vincin getirdiği görev kasası.

**Vinç:** çoklu vinç bağımsız; kilitleme; kazanma hamlesinde çalışmaz; her bırakma türü (`K`, `B`, renkli, `D`, `H`); çukura bırakma; bırakılan renkli kasanın 3'lü oluşturması (§5.6 adım 8).

**Patlatma:** yatay/dikey 3'lü iner; çapraz inmez; 4'lü/5'li bonus; kesişen sıralar; zincirleme yok; etiketsiz/görev/ağır/`K` eşleşmez; geri alma.

**Otopark:** araba sadece eksen yönünde; yol açıksa çıkar; engelde çarpıp döner ve hiçbir şey değişmez (sadece çarpışma sayacı); kasa araba yolunu kapatır; araba kasa yolunu kapatır; vinç arabanın önüne bırakır; kamyon rota boşalınca gelir; yükleme alanı dolunca gider; ikinci kamyon; rota tıkanınca kamyon gelmez; araba gönderme vinç sayacını azaltır; geri alma.

**Sıra:** §5.6 işleme sırası; kazanma koşulları; yıldız hesapları.

**Ekonomi:** CB Coin ekleme/harcama; yetersiz bakiyede reddetme; kasa açma; 3 seçenek kuralları (tekrar yok, garanti, rütbe seti, terfi eşyası); tekrar oynama ödülü; kayıt-yükleme.

**Yerleştirme:** taban alanı çakışması; yerleşim türleri (bina kapısı yola, araç yola, tekne suya); arazi dışına taşma; patika karosu; envantere geri alma.

**Reklam:** `AdPolicy` (ilk 8 bölüm, 3 bölüm / 120 sn, oturum 6, ödüllü sonrası 5 dk, yasak anlar); ödül sadece tamamlanınca.

**Arayüz/dil:** hiçbir metinde bölüm numarası yok; String Table 9 dilde dolu; font zincirinde eksik karakter yok; Türkçe büyük harf.

**Görünüm:** `Validate Layout` hatasız; kamera yaw 25°, pitch 55°, FOV 30°; `SwipeInput.FromScreen` her kamera açısında 4 kaydırma yönünü doğru ızgara yönüne çeviriyor; en büyük alanda kare ≥ 24 px.

**İlerleme:** %60 yıldız kuralı (44 yıldızla kilitli, 45 ile açık); `OpenAllForTesting=false` iken kilitler çalışıyor.

---

## 22. Fazlar

**İlk adım — durum raporu:** Mevcut kodda neler var; bu belgeyle uyumlu olanlar (özellikle §0'daki "daha önce yapılmış işler"); değişecekler; §20'ye göre silinecek dosya listesi; testlerin durumu. Belge ile mevcut uygulama arasında burada yazmayan bir çelişki bulursan **kendi başına değiştirme**, raporda sor.

| Faz | İş | Kabul |
|---|---|---|
| **F1 Temizlik** | §20: kaldırılanları sil; `README.md`'yi bu belgeye göre güncelle; eski belge atıflarını temizle | Derleme ve kalan testler geçiyor; silinenler raporlandı |
| **F2 Kurallar** | Görev kasası, teslim, ara durak, ağır görev kasası, vinç bırakma türleri, patlatma, otopark + kamyon, §5.6 sırası, kazanma, yıldızlar; çözücü (sıkışma tespiti + A*); bölüm dosyası biçimi (§6) | §21 kural testleri geçiyor |
| **F3 Görsel temel** | Kamera (§3.1), ölçek, oyuncak malzeme ve ışık (§2.4), sarı bariyer, saha içi engel seçimi (§3.4), şehir yerleşimi (kıvrımlı yollar, park alanı sahalar), `Validate Layout` | Şehir kontrol listesi (§2.6) + yerleşim raporu temiz |
| **F4 Modeller** | Önce mevcut modelleri ve `3D_Modeller\oyuncak\sahip\`'i tara; eksikleri Tripo ile üret (öncelik 1–2, §19.2, `MODEL_URETIM_LISTESI.md`); önizlemeler; `MODEL_RAPORU.md`'yi güncelle | Her model oyuncak kontrolünden geçti |
| **F5 Efektler** | §16 tamamı | Her efektin kısa video/kare dizisi |
| **F6 Rütbeler** | Rütbe verisi, kasa ve engel setlerinin rütbeye göre değişmesi, terfi anı | 5 rütbenin ekran görüntüleri |
| **F7 Ana sayfa ve Boss'un hayatı** | §11, §12 (kasa açma, 3 seçim, eşya verisi, yerleştirme, araziler, envanter), §13 ekonomi, gardırop; eşya modelleri için yer tutucu, sonra Tripo öncelik 3–4 | Akış: bölüm → kamyon → kasa aç → seç → yerleştir; testler |
| **F8 İpucu ve reklam** | §14, §15 (test reklamlarıyla) | Testler; reklam akışı videosu |
| **F9 Bölümler** | **Önce eşleme:** mevcut `LeagueLevels` (125) ve `PlanLevels` (100) bölümlerini §9 tablosuyla karşılaştır; her yer için "uyuyor / düzeltilir / yeniden üretilir" tablosu çıkar (`LIG_BOLUMLERI.md`'ye). Sonra §10: kalıp kütüphanesi, üretici (HardLab'i genişlet: geriye üretim, yapıcı üretim, otopark bağımlılık grafiği), doğrulama, filtreler; **10'arlı paketler, her yer için 3 aday** (uyan mevcut bölüm adaylardan biri olabilir) → oyun sahibinin onayı → `Resources/LeagueLevels/` (kayıtlı `solution` ile) | Eşleme tablosu; her paket raporu; onaylı bölümler |
| **F10 Dil** | §18 | 9 dilde ekran görüntüleri; eksik karakter yok |
| **F11 Oyun tanıtımı ve yayın** | `Docs/OYUN_TANITIMI.md`'yi yeniden yaz (oyunun güncel hâli, kontroller, mekanikler, ekonomi); `MAGAZA_METINLERI.md`, `GIZLILIK_POLITIKASI_TASLAK.md`, `YAYIN_KONTROL_LISTESI.md`'yi bu belgeye göre güncelle (`OpenAllForTesting=false` dahil) | — |

F9 (bölümler) F2 bittikten sonra paralel yürüyebilir: ilk paket (1–10) F3'ten önce bile hazırlanıp onaya sunulabilir.

---

## 23. Rapor (her faz sonunda)

- Değişen/eklenen/silinen dosyalar
- Test sonuçları
- `Validate Levels` ve `Validate Layout` özeti
- Ekran görüntüleri (şehir kontrol listesi + oyuncak tablosu)
- Tripo: üretilen modeller, deneme sayısı, kullanılan kredi
- Belirsiz bulup kendi kararınla çözdüğün noktalar
- Oyun sahibinden beklenenler (ör. `Breathing Idle`, AppLovin hesabı, gizlilik politikası web adresi, çeviri kontrolü, bölüm paketi onayı)
