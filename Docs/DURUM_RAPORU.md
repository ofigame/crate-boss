# Durum Raporu (Ana Belge §22 ilk adım)

01.11.2026 · Proje: `E:\OFIGAME\OYUNLAR\CrateBoss` · 212 betik, 33 test dosyası, son test koşusu 216/216 geçti (31.10).

> **Dikkat:** Proje bir git deposu değil. F1'de dosya silmeden önce projenin tam yedeğini `E:\CLAUDE\yedek\CrateBoss_F0.zip` olarak alacağım.

## 1. Kasa modelleri (yeni, `3dModeller\crateboss\kasalar`)

Yedi model de ≈2.300–2.700 üçgen, 1×1×0,9 birim, tabanı sıfırda, 2K PBR dokulu. Ölçek hazır; sadece Unity'ye alınması gerekiyor.

| Dosya | Oyundaki yeri | Belgeyle durum |
|---|---|---|
| `bosluk_acma_hareketli_kasa` (turuncu, X kapaklı) | Normal kasa `B` (Öğrenci) | Uyumlu (belge karton koli diyor; bu model geçerli) |
| `seviye1_sabit_kasa` (ahşap çerçeveli) | Saha içi duvarlar `#` (Öğrenci) | Belge §3.4 iç duvarı koni/araba/blok yapıyor; **bu model geçerli** sayıyorum |
| `seviye1_gorev_kasasi` (sarı, pati desenli) | Görev kasası `D` | Belge "kırmızı, beyaz oklu" diyor; **bu model geçerli** sayıyorum |
| `seviye1_buz_kasa` (mavi kafes) | Buz kasa | Belge siliyordu; **oyun sahibi: kalsın** |
| `birlestirme_seker_kasa_kirmizi / turkuaz / beyaz` | Patlatma kasaları | 3 model; **oyun sahibi: eksik renkler boyanarak** |

Modeli gelmeyenler (ağır kasa, vinç konteyneri `K`, hedef, bariyer) şimdilik mevcut kodla çizilen görünümde kalır. `Resources/Models/BoxMatch_1..5` belgede "var" deniyor ama projede yok; yerine yeni şeker kasaları girecek.

## 2. Belgeyle uyumlu, korunacaklar

- **Kurallar:** itme, geri alma (`UndoStack`), ağır kasa zinciri, çukur, çoklu vinç (`CraneSystem/State/Definition/Drop`), sınır haritası (`BoundaryMap`), `RulesEngine`, `StateHash`.
- **Görev kasası:** `BoxType.Delivery` ve `GoalType.Deliver` var (tek görev kasası, tek teslim alanı).
- **Patlatma:** `MatchSystem` var: 3'lü ve üstü yatay/dikey iner, çapraz inmez.
- **Kamyon:** `TruckSystem` var. Yükleme alanı dolunca kamyon gider; rota ve otopark yok.
- **İlerleme:** lig sistemi (`LeagueLevels` 125, `LeagueRules`, `MetaService.Unlocked` %60), `StoredSolution` ipucu, `TutorialDirector/View`, `OpenAllForTesting = true`.
- **Görünüm:** `CameraFramer` (yaw 25°, pitch 55°, FOV 30°, açılışta süzülme, büyük alanda takip), `SwipeInput.FromScreen`, basılı tutup sürükleyince kaydırma, sabit kasa görünümü, sarı bariyer (`BarrierBuilder`), kodla çizilen arabalar, şehir (`DistrictBuilder`, `RoadNetwork`), `LayoutValidator`.
- **Altyapı:** çözücü altyapısı (`LevelSolver`, `DeadCrates`, `PositionAnalysis`), işçi karakteri (`WorkerView`, animasyonlar), dil altyapısı (`Loc`, 9 dil, `ArabicShaper`).
- **Reklam:** `AdPolicy`, `AdServiceLocator`, test reklam servisleri (belge §15.4'teki adlarla aynı).

## 3. Değişecekler (F2–F8)

| Konu | Şimdi | Belge |
|---|---|---|
| Zemin harfleri | `A` = teslim yeri, `B` = yol başlangıcı (path görevi) | `B` = teslim alanı, `A` = sadece görsel başlangıç, `M` = ara durak (oyun sahibi: belgedeki gibi) |
| Görev kasası | Tek kasa, tek teslim; girince kilitlenmiyor | 1–3 kasa, `deliveries`, kendi B alanı, kilitlenme, ara durak, ağır görev kasası `Dh`, vinçten görev kasası |
| Patlatma | İnen kare "açık" kalıyor, **dolum vinci** (`RefillSystem`, Yolu Aç) yeni kasa bırakıyor | İnen kare zemin olur, dolum yok; 4'lü/5'li bonus |
| Vinç bırakması | Sadece `K` konteyner | `K`, `B`, `1–6`, `D`, `H`; çukura düşme; bırakmadan sonra patlatma |
| Otopark | Yok (kamyon yükleme alanı var) | 1×2 arabalar, çıkış, çarpışma sayacı, kamyon rotası, birden çok kamyon |
| Hamle sırası | `RulesEngine` eski EK1 sırası (kedi, bant, kantar, büyüyen saha, forklift adımları) | §5.6 sekiz adım |
| Yıldız | Hamle sınırı ve can sistemiyle bağlı (`StarRating`, `MoveLimits`, `LivesRules`) | En iyi çözüm, +%25 ve +%10; kaybetme yok |
| Para | Prim + depo dükkânı | CB Coin, teslimat kasası, Boss'un arazisi (F7) |
| Kasa görünümü | Semt/dünya temasına göre | Rütbeye göre (F6); şimdilik herkes 1. seviye kasalarla |
| Gezinme | Basılı tut ve sürükle (her yerde) | Yedek olarak bu; asıl yöntem iki parmakla sürükleme, iki parmakla yakınlaştırma, genel bakış butonu |
| Çözücü | BFS ve sıkışma tespiti | + A*, 60 sn sınır, tolerans ölçümü |
| Bölümler | HardLab'den 125 klasik/vinçli bölüm | §9 planı, 10'arlı paketler, her yer için 3 aday (F9) |

## 4. Silinecekler (F1, §20)

**Kurallar (Core):** `CatEvent`, `CatState`, `CatSystem` · `ConveyorSystem` · `ForkliftDefinition/End/Event/EventKind/State/System` · `GateDefinition`, `GateSystem` · `StageSystem` · `RefillDefinition/DropEvent/State/System` · `TagColor` · `BoxType.Gold` (buz kalır) · zemin türleri `Basket`, `Exit`, `Plate`, `Gate`, `Bonus`, `Conveyor*`, `Road`, `PathStart` · görevler `Cats`, `Exit`, `Road`, `Path`, `Reach`.

**Veri ve çözücü:** `ForkliftData`, `GateData`, `RefillData` · bu türlere bağlı `LevelParser` dalları · `CraneTolerance` içindeki forklift kısmı.

**Meta:** `DailyPuzzlePicker`, `DailyRules`, `Titles`, `Shop`, `ShopCatalog`, `ShopCategory`, `ShopItem`, `ShopState` · Prim (`Wallet` CB Coin'e çevrilir) · `LivesRules`, `MoveLimits`.

**Görünüm ve arayüz:** `CatView`, `ForkliftView`, `ForkliftRouteView`, `RefillView` · `PedestalBuilder` · `RobotView`, `RobotFaceTextures` · `WorldTheme`, `WorldThemes` · `DepotView`, `DepotHud`, `DepotShapes`, `ShopPanel`, `ShopPictures` (yerine F7'de ana sayfa gelir) · `Resources/Models/Forklift.prefab`; `BoxIce.prefab` yeni buz modeliyle değişir.

**Testler:** `CatTests`, `ConveyorGoldRoadTests`, `ForkliftTests`, `ExitStageGateTests`, `TagAndOrderTests`, `MatchRefillTests` (dolum kısmı), `DeliverPathTests` (path kısmı), `LivesAndLimitsTests`.

**Bölüm klasörleri:** `DailyLevels` (108), `YolLevels` (4), `CandidateLevels` (51), `DemoLevels` (12): kaldırılan mekanik kullananlar silinir, kalanlar F9 aday havuzuna not edilir · `Docs/Tasarim/konsept/` · `Docs/Tasarim/kroki_bolumler`, `vinc_bolumler`, `aday_bolumler` (§9'a uyan aday çıkarsa F9 havuzuna taşınır, gerisi silinir).

**Dokunulmayacak:** `Resources/Levels` (önceki kuralın: dokunma). `PlanLevels` (100) F9'da aday havuzu olarak kalır, oyunda kullanılmaz.

Her silinen dosya `Docs/DURUM_RAPORU.md` sonuna eklenecek F1 listesinde yer alacak.

## 5. Kendi kararımla çözeceğim küçük noktalar

- **Görünüm:** yeni modeller geçerli. Normal kasa, sabit kasa ve görev kasası olarak bütün oyunda (5 rütbede de) 1. seviye kasalar kullanılır; rütbe kasaları gelince F6'da değişir.
- **Şehir:** belgede "lig ilerledikçe şehir gelişir" yok. Rütbeye göre değişen şehir yerine §12'deki Boss arazisi ilerlemeyi gösterir. Lig başına dünya alanı (`world`) silinir.
- **Kamera:** §3.1'deki "saha dışında sürükleme en fazla 3 birim" ile senin 31.10 isteğin ("basılı tutup sürükleyince ekran kayar") çelişmiyor. Basılı tut-sürükle kalır, iki parmak eklenir.
- **Belge §10.4:** belge "onaylanan bölümler `Resources/Levels/`'a girer" diyor; F9'a göre `LeagueLevels` doğru, onu kullanırım.

## 6. Oyun sahibinin kararları (01.11)

1. **Buz kasa kalır:** kural, model ve testleriyle birlikte. Bu yüzden §4'teki silinecekler listesinde yok; §9 planındaki yeri F9 eşlemesinde önerilecek.
2. **Patlatma renkleri:** kırmızı, turkuaz ve beyaz modeller kullanılır. Eksik renkler aynı modelin dokusu renklendirilerek yapılır; renk numaraları F2'de sabitlenir.
3. **Zemin harfleri belgedeki gibi:** `B` teslim alanı, `A` başlangıç işareti, `M` ara durak. Mevcut bölüm dosyaları otomatik çevrilir.
4. **Can ve hamle sınırı silinir:** `LivesRules`, `MoveLimits`, `LivesAndLimitsTests`.

---

# F1 Temizlik raporu (01.11.2026)

Yedek: `E:\CLAUDE\yedek\CrateBoss_F0.tgz` (silmeden önceki tam proje, Library hariç).

## Silinen kod (61 dosya)

- **Kurallar (Core):** `CatEvent`, `CatState`, `CatSystem`, `ConveyorSystem`, `ForkliftDefinition/End/Event/EventKind/State/System`, `GateDefinition`, `GateSystem`, `StageSystem`, `RefillDefinition/DropEvent/State/System`, `TagColor`.
- **Veri:** `ForkliftData`, `GateData`, `RefillData`.
- **Çözücü:** `ProgressionPlan`, `LevelSlot` (eski 75 yerlik plan; forklift, kedi, semt, renk etiketi üzerine kuruluydu).
- **Meta:** `DailyPuzzlePicker`, `LivesRules`, `MoveLimits`, `Shop`, `ShopCatalog`, `ShopCategory`, `ShopItem`, `ShopState`, `Titles`.
- **Görünüm ve arayüz:** `CatView`, `ForkliftView`, `ForkliftRouteView`, `RefillView`, `PedestalBuilder`, `RobotView`, `RobotFaceTextures`, `FaceExpression`, `PlayerStyle`, `SiteStyle`, `DepotView`, `DepotShapes`, `ShopPictures`, `DepotHud`, `ShopPanel`.
- **Editör:** `Candidate`, `CandidateGenerator`, `CandidateLayout`, `IdeaWriter`, `DailyPoolGenerator`, `SolverBench` (eski aday üreticisi; F9'da HardLab'ten yeniden kurulacak).
- **Testler:** `CatTests`, `ConveyorGoldRoadTests`, `ForkliftTests`, `ExitStageGateTests`, `TagAndOrderTests`, `LivesAndLimitsTests`. `MatchRefillTests` → `MatchTests` (dolum ve aşama kısmı çıktı), `DeliverPathTests` → `DeliverTests` (reach/path çıktı).

## Kod içinden çıkarılanlar

- `BoxType.Gold`; zemin türleri `Basket`, `Exit`, `Plate`, `Gate`, `Bonus`, `Conveyor*`, `Road`, `PathStart`; görevler `cats`, `exit`, `road`, `reach`, `path`.
- `GameState`: forklift, kedi, kantar, aşama, renk/sıra etiketi, dolum vinci alanları. `Box.Tag`.
- `RulesEngine`: hamle sırası Ana Belge §5.6'ya göre: hamle sayısı → patlatma → kamyon → kazanma → vinçler → (vinç kasası) patlatma. Patlatmada inen kare zemin kalır, dolum yok. 4'lü +5, 5'li +10 CB Coin.
- `LevelParser`, `LevelData`, `LevelLoader`, `LevelJsonWriter`: eski alanlar (forklifts, gates, tags, stages, refill, closeBehind, world). Yazıcı artık `league` ve `solution` alanlarını da yazar.
- **Semtler:** `world` alanı ve `WorldThemes.Get` kalktı; tek görünüm `WorldThemes.Current` (oyuncak şehir). Bölüm listesi rütbe bantlarıyla gruplanır.
- **Prim → CB Coin:** `SaveData.coins` (eski kayıttaki `prim` yüklenirken `coins`'e aktarılır), `Wallet`, `RewardRules` (altın kasa bonusu çıktı), kazanma paneli, dil tablosu.
- **Can ve hamle sınırı:** HUD'daki kalp, kaybetme kartı, can reklamı kalktı. Yıldızlar §5.8: en iyi çözüm ★★★, +%25 ★★, gerisi ★.
- **Depo ve dükkân:** ana ekran F7'ye kadar bölüm listesi (`ShowHome`). Gardırop F7'de gelecek; `Outfit` şimdilik boş.
- **Ses ve ikon:** kedi miyavı, forklift sesleri (kamyon için `Horn` kaldı); forklift, kedi, aşama, dükkân, etiket şekli ikonları.
- **Reklam:** `AdPlacement.LifeRewarded` ve günlük bulmaca kuralı çıktı; `FreePrim` → `HomeCoins`, `OpenCrate` eklendi (F7/F8).
- **Dil tablosu:** 95 satır silindi (dükkân, unvan, semt, dünya, günlük bulmaca, can, kaybetme, eski görev kartları). Prim geçen satırlar CB Coin olarak 9 dilde yeniden yazıldı (taslak). `ui.depot` → `ui.home`.
- **Modeller:** `Art/Models/forklift`, `Resources/Models/Forklift.prefab`, Normalize Models'daki forklift kaydı.

## Silinen bölüm ve belge dosyaları

- `Resources/DailyLevels` (108) ve `Resources/YolLevels` (4) tamamen.
- Kaldırılan mekanikleri kullanan bölümler (93): `CandidateLevels` 23, `DemoLevels` 10, `PlanLevels` 58. Kalan: `CandidateLevels` 28, `PlanLevels` 42, `DemoLevels` 2 (F9 aday havuzu).
- `Resources/Levels` ve `Resources/LeagueLevels`'a dokunulmadı.
- `Docs/Tasarim/konsept/` silindi. `kroki_bolumler`, `vinc_bolumler`, `aday_bolumler`: kaldırılan mekanik içermeyen 37 harita `Docs/Tasarim/f9_havuz/`'a taşındı, 23'ü ve klasörler silindi.
- `Builds/Windows/Yolu_Ac_Ornekleri.bat` silindi.

## Belgeler

- `README.md` Ana Belge'ye göre yeniden yazıldı.
- Kod yorumlarındaki eski belge atıfları (EK1, EK-2, CRATE_BOSS_GOREV, VINC, YOLU_AC, "main doc", "design doc"; 93 dosya) Ana Belge bölümlerine çevrildi.
- `CEVIRI_DURUMU.md`, `MODEL_URETIM_LISTESI.md`, `YAYIN_KONTROL_LISTESI.md`, `YERLESIM_RAPORU.md`, `GIZLILIK_POLITIKASI_TASLAK.md` atıfları ve Prim ifadeleri güncellendi.
- `Docs/referans/parking_jam_stil.webp`: oyun sahibinin verdiği stil referansı.

## Kendi kararımla çözdüklerim

- `DailyRules` silinecekler listesindeydi ama giriş serisi ve günlük ipucu hediyesi de onda; belge bunları koruyor (§13.1, §14). Dosya kaldı, yalnızca günlük bulmaca kısmı silindi.
- Semt görünümlerinin kodu (`DistrictBuilder`'daki liman/havalimanı/soğuk depo/gümrük dalları, `Art/Environment` altındaki 4 semt klasörü) artık hiç çağrılmıyor. Şehir F3'te Parking Jam tarzında yeniden kurulacağı için o fazda tamamen silinecek; şimdi silmek aynı dosyaları iki kez elden geçirmek olurdu.
- Rütbe terfi kartı (`PromotionView`) kaldı; F6'da terfi anına dönüşecek.

## Sonuçlar

| Kontrol | Sonuç |
|---|---|
| EditMode testleri | **153/153 geçti** (önce 216; fark silinen mekaniklerin testleri) |
| Validate Levels | Çalıştı; `Resources/Levels`'taki eski 15 bölümün boyut ve zorluk uyarıları öncekiyle aynı (`E:\CLAUDE\crateboss_validate.log`) |
| Validate Layout | 125 bölüm, **0 hata**; uyarılar bina yükseklikleri (§3.2 ölçek, F3'te düzelecek) |
| Ekran turu (`-quick`) | 125 bölüm açıldı, 0 hata (`E:\CLAUDE\crateboss_shots_f1`) |

## Oyun sahibinden beklenen

- F2'ye (Kurallar) geçiş onayı.

---

# F2–F11 raporu (01.11.2026, oyun sahibinin "tüm fazları tamamla, exe ve apk çıkart" isteğiyle tek seferde)

## F2 Kurallar

- **Görev kasası (§5.1):**
  - Zemin `B` (ya da `1`–`3`) teslim alanı, `A` başlangıç işareti (görsel), `M` ara durak.
  - Kasa kendi alanına girince kilitlenir. Yanlış alan ya da ara durak atlandıysa reddedilir; alan kırmızı yanıp söner ve kasa itilebilir kalır.
  - En çok 3 teslimat olabilir (`deliveries` listesi).
  - Ağır görev kasası `deliveries[].heavy` ile tanımlanır, çünkü nesne katmanında her kareye tek harf düşüyor ("Dh" yazılamıyor).
- **Vinç bırakma türleri (§5.3):**
  - Türler: `K`, `B`, `I`, `H`, `1`–`6`, `D` (görev kasası, `deliveryIndex` ile).
  - Kasa çukura düşerse köprü olur.
  - Vincin bıraktığı renkli kasa üçlü tamamlarsa 8. adımda iner.
  - Araba duran kareye vinç bırakmaz, bekler.
- **Otopark ve kamyon (§5.5):**
  - Arabalar 1×2 ve yalnız eksenleri boyunca gider.
  - Çıkışa kadar yol açıksa araba gider; yol kapalıysa çarpıp geri döner. Çarpışma hamle sayılmaz, çarpışma sayacına yazılır ve geri almayla silinmez.
  - Kamyon, rotası boşalınca yanaşır; kasası dolunca gider ve sıradaki kamyon gelir. Eski `trucks` bloğu (rotasız kamyon) okunmaya devam ediyor.
- **Hamle sırası (§5.6):** sekiz adım, oyunda, çözücüde ve testlerde birebir aynı.
- **Kazanma ve yıldızlar (§5.7, §5.8):**
  - Görev `targets`, `deliver` ve `truck` olabilir; birden çok görev birlikte verilebilir.
  - Yıldızlar: en iyi çözüm kadar hamle ★★★, %25 fazlasına kadar ★★. Kayıtlı (bilinen en iyi) çözümlerde ★★★ sınırı +%10.
  - Otoparkta çarpışma: 0 çarpışma ★★★, en çok 2 çarpışma ★★.
- **Çözücü (§10.3):**
  - Araba eylemleri de aranıyor.
  - A* için alt sınır var: hedefe ve teslim alanına uzaklık. Buz, ağır kasa ya da kasa bırakan vinç olan bölümlerde bu sınır kapalı.
  - Süre sınırı 60 saniye; ipucu araması A* ile yapılıyor.
  - Çözüm metninde araba eylemi `3r` gibi yazılır (3 numaralı araba sağa).
- **Bölüm dosyası (§6):** `rank`, `deliveries`, `parking`, `design.patterns`, `design.targetDifficulty`, `design.targetTolerance` eklendi. Yazıcı da bu alanları yazıyor.
- **Testler:** `DeliverTests`, `ParkingTests` ve `RuleOrderTests` yeni. `HomeTests` F7 için eklendi.

## F3 Görsel temel

- Semtler tamamen kalktı: `DistrictKind` artık yalnız şehir; liman, havalimanı, soğuk depo ve gümrük kodları ile 4 semt prefab klasörü silindi.
- Evler ve apartmanlar §3.2 ölçeğine indirildi; yükseklik ≤ 4.
- Ağır kasa itilirken kamera hafifçe titriyor (§16).
- İç duvarlar oyun sahibinin sabit kasa modeli. §3.4'teki koni, araba ve blok yerine bu modeli kullandım, çünkü oyun sahibi "tüm oyunu bu kasalarla oluştur" dedi.

## F4 Modeller

Oyun sahibinin 7 kasası projede (`Art/Models/level1_*`, `match_*`); ölçek tablosu `MODEL_RAPORU.md`'de.

| Prefab | Kullanım |
|---|---|
| BoxNormal | normal kasa |
| BoxFixed | iç duvarlar ve vincin bıraktığı konteyner |
| BoxDelivery | görev kasası |
| BoxIce | buz kasa |
| BoxMatch_1, BoxMatch_2 | kırmızı ve turkuaz renkli kasa |
| BoxMatch_White | beyaz model; renk 3–6 bundan boyanarak yapılıyor |

- Renk 2 turkuaz oldu: belge "mavi" diyor, ama oyun sahibinin modeli turkuaz.
- Eski `crate_wood` ve `crate_ice` modelleri silindi.
- Tripo kullanılmadı (kredi 0).

## F5 Efektler

§16'daki zamanlamalar zaten yerindeydi. Yeni eklenenler:

| Olay | Efekt |
|---|---|
| Araba gider | öne eğilir, bulut çıkarır |
| Araba çarpar | ezilip esner, yıldız çıkar, zıplayıp yerine döner |
| Kamyon | geri geri "bip bip" yanaşır, korna çalarak gider |
| Teslim | kasa zıplar, alan yeşile döner, yıldızlar çıkar |
| Ret | alan kırmızı yanıp söner |
| Kasa açma | konfeti |
| Eşya yerleştirme | bulut |
| Ana sayfa | kamyon gelir, kasa yığına düşer |

## F6 Rütbeler

- Rütbe listesi veri dosyasında (`Resources/Data/Ranks.txt`): rütbe adı, kasa model eki, açtığı arazi. 8 rütbe tanımlı.
- Oynanan bölümün rütbesine göre kasa modeli aranıyor (örneğin `BoxNormal_R2`). Model yoksa 1. seviye kasa kullanılıyor, bu yüzden şimdilik her rütbede aynı kasalar.
- Terfi: rütbenin son bölümü bitip %60 yıldız toplanınca terfi kartı çıkıyor, ücretsiz terfi kasası ve yeni arazi geliyor.

## F7 Ana sayfa ve Boss'un hayatı

- **İlk açılış:** Yeni oyuncu doğrudan 1. bölümle başlıyor. Bölüm bitince ana sayfa açılıyor ve ilk kasa ücretsiz.
- **Kasalar:**
  - Bölümü ilk bitirişte teslimat kasası geliyor; kamyon onu yığına bırakıyor.
  - Kasa açma 30 CB Coin. CB Coin yetmezse reklamla açılıyor (günde 5).
  - Her kasada 3 eşya seçeneği: rütbe setinden ve sahip olunmayanlardan seçiliyor, 5 kasada bir mutlaka büyük eşya var. Terfi kasasında rütbenin özel eşyası çıkıyor.
- **Eşya kataloğu:** 63 eşya (`Resources/Data/Items.txt`). Modeller kodla çizilen oyuncak yer tutucular.
- **Düzenleme modu (§12.4):**
  - Izgara, yeşil/kırmızı gölge, döndürme, envanter; yerleştirilmiş eşyaya dokununca geri alınıyor.
  - Patikalar ücretsiz.
  - Kurallar: bina kapısı yola ya da patikaya bakmalı, araç patikada durur, tekne suda, iskele su kenarında.
- **Araziler:** 5 arazi var, rütbeyle açılıyor.
- **Diğer:** Gardırop (9 parça, 100–500 CB Coin); ana sayfada reklamla +30 CB Coin (günde 3); rütbe çubuğu sayısız.
- **Ekonomi (§13):**
  - Bölüm ödülü ★10 / ★★18 / ★★★30; tekrar oynamada yalnız fark ödeniyor.
  - Giriş serisi 10→60.
  - İpucu 50, çözümü izle 300.

## F8 İpucu ve reklam

- İpucu yalnız bir sonraki hamleyi gösteriyor; bu hamle bir araba olabilir.
- Oyuncu takıldığında "N hamle geri al" diyor (§14). N, geri alma geçmişinde kanıtlanmış bir konumdan hesaplanıyor; yanlış sayı verilmiyor.
- Reklam kuralları zaten §15.2'deki değerlerdeydi. Geçiş reklamı artık "Devam"dan sonra, ana sayfaya giderken çıkıyor.

## F9 Bölümler

- 125 bölüm §9 tablosundan üretildi (`E:\CLAUDE\cb_plan`, PlanLab). Yöntemler ve bölüm tablosu `LIG_BOLUMLERI.md`'de.
- Her bölümün kayıtlı çözümü test ediliyor (`LeagueLevelTests`).
- **§10.4 onayı yapılmadı.** Oyun sahibi "test ettikten sonra devam" dediği için her yer için en iyi adayı ben seçtim.

## F10 Dil

Yeni metinler 9 dilde taslak olarak eklendi: ana sayfa, kasa açma, düzenleme, gardırop, araziler, 63 eşya adı ve 125 bölüm adı (`CEVIRI_DURUMU.md`).

## F11 Belgeler

`OYUN_TANITIMI.md` yeniden yazıldı; `YAYIN_KONTROL_LISTESI.md`, `CEVIRI_DURUMU.md` ve `GIZLILIK_POLITIKASI_TASLAK.md` güncellendi; `README.md` F1'de yenilenmişti.

## Kendi kararımla çözdüklerim

- **Renkli kasa ve hedef:** Renkli kasa hedefi doldurmuş sayılmıyor (eski kural korundu). Belge yalnız konteyneri hariç tutuyor; bunu sana soruyorum.
- **Çarpışma ve geri alma:** Araba çarpışması geri almayla silinmiyor, yoksa otopark yıldız kuralı anlamsız olurdu.
- **Ana sayfa yerleşimi:** Kasa yığını sahnede, ayrıca sağda bir "Kasalar" düğmesi var. "Ayarlar" bölüm listesini açıyor; ses, kalite, dil ve gizlilik oradan.
- **Yer tutucular:** Eşya önizlemesi 3D döner model yerine renkli kart; eşya modelleri yer tutucu.
- **Belgede yazmadığı için:** Ağır görev kasası `deliveries[].heavy` ile tanımlanıyor. Birden çok teslimat alanı zemin katmanında `1`, `2`, `3` harfleriyle yazılıyor.

## Oyun sahibinden beklenenler

- **Bölümler:** oyna, beğenmediğin yerleri söyle; 10'arlı paketler hâlinde iyileştireceğim.
- **Rütbe kasaları:** 2–5. rütbe kasalarının modelleri.
- **Tripo kredisi:** saha engelleri, kamyon ve Boss eşyaları için.
- **Breathing Idle:** işçinin bekleme animasyonu.
- **Yayın öncesi:**
  - AppLovin hesabı.
  - Gizlilik politikası web adresi.
  - Çevirilerin ana dili konuşan biri tarafından kontrolü.
  - `OpenAllForTesting = false` yapılması.
