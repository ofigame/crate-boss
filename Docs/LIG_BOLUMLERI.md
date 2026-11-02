# Lig bölümleri (125) — Ana Belge §9 planı

Üretim: 01.11.2026, `E:\CLAUDE\cb_plan` (PlanLab). Her bölüm, oyunun kendi kurallarıyla (RulesEngine) kayıtlı çözümü oynatılarak doğrulandı.

## Eşleme (F9 ilk adım)

Eski `LeagueLevels` (HardLab, yalnız klasik ve vinçli odalar) ve `PlanLevels` §9 tablosuyla karşılaştırıldı. §9'un 116 yeri görev kasası, boşluk birleştirme, patlatma, otopark ya da kamyon istiyor; eski bölümlerde bunlar yok. Bu yüzden hepsi **yeniden üretildi**:

| Yer | Durum |
|---|---|
| 1–9 (klasik, hedef) | Yeniden üretildi (ters oynatma + tam çözücü; eski klasik odalar boyut/kasa sayısı tutmadığı için) |
| 10–125 | Yeniden üretildi (§10.2 yöntemleri) |

Eski 125 bölüm `E:\CLAUDE\yedek\LeagueLevels_old` altında duruyor.

## Üretim yöntemleri (Ana Belge §10.2)

- **Klasik (1–9):** Çözülmüş hâlden geriye kasa çekerek; en kısa çözüm oyunun çözücüsüyle.
- **Görev kasası, boşluk birleştirme, çukur, ağır kasa, patlatma:** Çözülmüş hâlden geriye: görev kasası kendi B alanından A işaretine çekilir; çukurlar "boşaltılır" (ileride bir kasa itilip doldurulacak); renkli üçlüler "geri doğar" (ileride üçüncü kasa itilince iner ve yolu açar); bant kasaları karıştırılıp koridor kapatılır. Ters hamleler ileri çözümdür; yürüyüşler en kısaya indirilir.
- **Vinçler:** Çözümün zaman çizelgesine göre: engel (E) çözümün işi bittiği bir kareye toleransla düşer; köprü (ç) çukuru çözüm oraya varmadan doldurur; görev kasası (g) ilk itilmeden önce gelir; yardımcı (k) ve renkli (r) kasalar artık kullanılmayan karelere düşer.
- **Otopark:** Arabalar şeritlerde (öndeki gitmeden arkadaki çıkamaz), bazı şeritler üstten yan yatmış arabalarla kapalı; kamyon rotası bir şeritten iner. Çözüm: gereken arabalar sırayla gönderilir, kamyon gelir, kasalar tek tek yüklenir.

## Bölümler

Zorluk puanı §10.5 formülüyle (tuzak oranı hariç) ölçüldü. "en kısa": çözücünün kanıtladığı en kısa çözüm; "bilinen": üreticinin çözümü (3 yıldız sınırı +%10).

| # | Ad | Hedef zorluk | Ölçülen | Hamle | Çözüm | Not |
|---|---|---|---|---|---|---|
| 1 | İlk Koli | 5 | 10 | 3 | en kısa |  |
| 2 | İki Koli | 10 | 13 | 4 | en kısa |  |
| 3 | Köşeyi Dön | 15 | 16 | 7 | en kısa |  |
| 4 | Önce Arkadaki | 22 | 24 | 13 | en kısa |  |
| 5 | Geçici Park | 28 | 28 | 15 | en kısa |  |
| 6 | Dar Koridor | 34 | 34 | 26 | en kısa |  |
| 7 | Ters Yön | 40 | 38 | 23 | en kısa |  |
| 8 | Fazla Koli | 46 | 46 | 23 | en kısa |  |
| 9 | Çember | 52 | 51 | 64 | bilinen |  |
| 10 | Görev Kolisi | 40 | 38 | 24 | en kısa |  |
| 11 | Boşluk Yürüt | 48 | 44 | 32 | en kısa |  |
| 12 | Kanca Geliyor | 45 | 42 | 23 | bilinen |  |
| 13 | Vinci Durdur | 52 | 49 | 22 | bilinen |  |
| 14 | Kapanan Kapı | 58 | 56 | 30 | bilinen |  |
| 15 | Otopark Tıkandı | 45 | 45 | 38 | bilinen |  |
| 16 | Dolu Boşluk | 55 | 52 | 25 | bilinen |  |
| 17 | Uzun Koridor | 60 | 62 | 75 | bilinen |  |
| 18 | Üç Bırakma | 62 | 53 | 26 | bilinen |  |
| 19 | Uzak Boşluk | 54 | 54 | 56 | bilinen |  |
| 20 | Üçle ve İndir | 45 | 45 | 35 | en kısa |  |
| 21 | Renk Seçimi | 55 | 51 | 52 | bilinen |  |
| 22 | Dik Üçlü | 58 | 54 | 63 | bilinen |  |
| 23 | Kesişim | 62 | 58 | 74 | bilinen |  |
| 24 | Kanca ve Renk | 64 | 66 | 59 | bilinen |  |
| 25 | Öğrenci Sınavı | 70 | 73 | 63 | bilinen |  |
| 26 | Yeni Kasalar | 55 | 55 | 52 | bilinen |  |
| 27 | Karşı Uçtan | 60 | 61 | 83 | bilinen |  |
| 28 | Köprü Kurulur | 58 | 59 | 44 | bilinen |  |
| 29 | Tek Pencere | 66 | 66 | 64 | bilinen |  |
| 30 | İkisi Bir Arada | 62 | 65 | 79 | bilinen |  |
| 31 | Kaydır ve Patlat | 64 | 63 | 73 | bilinen |  |
| 32 | Yanlış Üçlü | 68 | 66 | 84 | bilinen |  |
| 33 | Çift Kanca | 66 | 64 | 49 | bilinen |  |
| 34 | Vinç Yarışı | 70 | 70 | 57 | bilinen |  |
| 35 | Kasa Arabayı Kapatıyor | 62 | 60 | 66 | bilinen |  |
| 36 | Uzun Yol | 64 | 62 | 66 | bilinen |  |
| 37 | Dört Renk | 68 | 67 | 99 | bilinen |  |
| 38 | Kapat Beni | 70 | 68 | 57 | bilinen |  |
| 39 | Dörtlü | 62 | 61 | 82 | bilinen |  |
| 40 | Ağır Yük | 64 | 64 | 63 | bilinen |  |
| 41 | Ağır Köprü | 70 | 67 | 44 | bilinen |  |
| 42 | Renkli Teslimat | 68 | 66 | 56 | bilinen |  |
| 43 | Doğru Yere Düş | 72 | 72 | 60 | bilinen |  |
| 44 | Kesişen Koridorlar | 74 | 73 | 99 | bilinen |  |
| 45 | İki Kamyon | 72 | 74 | 72 | bilinen |  |
| 46 | Uzak B | 76 | 73 | 97 | bilinen |  |
| 47 | Ağır Renk | 76 | 73 | 101 | bilinen |  |
| 48 | Saat Gibi | 80 | 77 | 59 | bilinen |  |
| 49 | Nefes | 70 | 72 | 97 | bilinen |  |
| 50 | Stajyer Sınavı | 85 | 83 | 68 | bilinen | otopark 7×17 (plan 9×22) |
| 51 | Plastik Kasalar | 70 | 68 | 84 | bilinen |  |
| 52 | Üç Kanca | 76 | 65 | 45 | bilinen |  |
| 53 | Yükü Kullan | 76 | 79 | 76 | bilinen |  |
| 54 | Beşli | 78 | 76 | 126 | bilinen |  |
| 55 | Otopark Labirenti | 76 | 78 | 97 | bilinen |  |
| 56 | Çift Görev | 80 | 81 | 92 | bilinen |  |
| 57 | Ayrılan Yollar | 82 | 96 | 111 | bilinen |  |
| 58 | Dönüş Yok | 82 | 80 | 116 | bilinen |  |
| 59 | Tuzak Renk | 80 | 78 | 129 | bilinen |  |
| 60 | Ağır Duvar | 76 | 61 | 57 | bilinen |  |
| 61 | Köprü Zinciri | 86 | 77 | 83 | bilinen |  |
| 62 | Gizli Üçlü | 86 | 89 | 143 | bilinen |  |
| 63 | Kalabalık | 88 | 85 | 86 | bilinen |  |
| 64 | Kilit Zinciri | 90 | 85 | 108 | bilinen |  |
| 65 | Kamyon ve Kanca | 86 | 85 | 86 | bilinen | otopark 8×17 (plan 9×18) |
| 66 | Uzun Teslimat | 88 | 89 | 142 | bilinen |  |
| 67 | Renk Ödünç | 92 | 90 | 122 | bilinen |  |
| 68 | Sıkışık | 90 | 61 | 69 | bilinen |  |
| 69 | Vinç Altında | 92 | 90 | 122 | bilinen |  |
| 70 | Ağır Patlatma | 84 | 81 | 120 | bilinen |  |
| 71 | Dolambaç | 94 | 93 | 124 | bilinen |  |
| 72 | Son Boşluk | 96 | 67 | 82 | bilinen |  |
| 73 | Üç Kanca Dansı | 98 | 91 | 88 | bilinen |  |
| 74 | Renk Fırtınası | 98 | 99 | 189 | bilinen |  |
| 75 | Çırak Sınavı | 105 | 105 | 114 | bilinen | otopark 9×17 (plan 9×26) |
| 76 | Metal Kasalar | 90 | 88 | 140 | bilinen |  |
| 77 | Karşılıklı | 96 | 98 | 154 | bilinen |  |
| 78 | Dar Geçit Patlatma | 98 | 87 | 157 | bilinen |  |
| 79 | Kancadan Görev | 96 | 61 | 57 | bilinen |  |
| 80 | Dev Saha | 100 | 98 | 124 | bilinen |  |
| 81 | Artık İşe Yarar | 100 | 95 | 139 | bilinen |  |
| 82 | Yarım Köprü | 100 | 65 | 54 | bilinen |  |
| 83 | Kayan Duvar | 102 | 72 | 85 | bilinen |  |
| 84 | Karakter Köprüsü | 98 | 70 | 65 | bilinen |  |
| 85 | Otopark ve Kancalar | 102 | 104 | 125 | bilinen | otopark 7×19 (plan 9×20) |
| 86 | Çapraz Teslim | 104 | 104 | 170 | bilinen |  |
| 87 | Bir Hamle Önce | 106 | 99 | 106 | bilinen |  |
| 88 | Patlat ve Kilitle | 106 | 100 | 152 | bilinen |  |
| 89 | Labirent | 98 | 97 | 146 | bilinen |  |
| 90 | Üç Durak | 108 | 107 | 151 | bilinen |  |
| 91 | Ağır Görev | 108 | 66 | 69 | bilinen |  |
| 92 | Renk Zinciri | 110 | 94 | 172 | bilinen |  |
| 93 | Sabır | 100 | 78 | 82 | bilinen |  |
| 94 | Dört Kanca | 112 | 92 | 126 | bilinen |  |
| 95 | Büyük Otopark | 108 | 97 | 135 | bilinen |  |
| 96 | Gece Vardiyası | 112 | 115 | 175 | bilinen |  |
| 97 | Ayna | 112 | 111 | 211 | bilinen |  |
| 98 | İç İçe Üçlüler | 114 | 102 | 199 | bilinen |  |
| 99 | Son Prova | 116 | 116 | 158 | bilinen |  |
| 100 | Kalfa Sınavı | 125 | 118 | 131 | bilinen | otopark 9×19 (plan 9×28) |
| 101 | Usta İşi | 110 | 109 | 158 | bilinen |  |
| 102 | Üç Görev | 118 | 130 | 197 | bilinen |  |
| 103 | Vinç Ustası | 120 | 117 | 165 | bilinen |  |
| 104 | Kayan Şehir | 120 | 66 | 80 | bilinen |  |
| 105 | Otopark Ustası | 120 | 108 | 139 | bilinen | otopark 9×20 (plan 9×22) |
| 106 | Altı Renk | 122 | 109 | 217 | bilinen |  |
| 107 | Ağır Ustalık | 122 | 76 | 94 | bilinen |  |
| 108 | Uzun Yol II | 124 | 125 | 206 | bilinen |  |
| 109 | Köprüler | 124 | 71 | 66 | bilinen |  |
| 110 | Tek Nefes | 126 | 127 | 182 | bilinen |  |
| 111 | Renkli Kancalar | 128 | 97 | 142 | bilinen |  |
| 112 | Görev Zinciri | 128 | 129 | 216 | bilinen |  |
| 113 | Kayıp Üçlü | 130 | 119 | 232 | bilinen |  |
| 114 | Sıkışan Kamyon | 128 | 113 | 173 | bilinen | otopark 8×21 (plan 9×22) |
| 115 | Otopark Finali | 132 | 116 | 136 | bilinen | otopark 9×20 (plan 9×24) |
| 116 | Patlat ve Taşı | 132 | 129 | 242 | bilinen |  |
| 117 | Ağır Labirent | 134 | 105 | 174 | bilinen |  |
| 118 | Sessiz Vinçler | 124 | 98 | 130 | bilinen |  |
| 119 | Dört Durak | 136 | 133 | 233 | bilinen |  |
| 120 | Usta Ayna | 136 | 137 | 242 | bilinen |  |
| 121 | Son Kilit | 138 | 109 | 164 | bilinen |  |
| 122 | Büyük Patlatma | 140 | 138 | 285 | bilinen |  |
| 123 | Üç Görev II | 142 | 143 | 195 | bilinen |  |
| 124 | Boss'a Doğru | 145 | 145 | 254 | bilinen |  |
| 125 | Crate Boss | 150 | 138 | 179 | bilinen | otopark 8×20 (plan 9×30) |

## Bilinen sınırlar (oyun sahibi testinden sonra iyileştirilecek)

- Ölçülen zorluk, tablodaki hedefin çoğunlukla ±10'u içinde; ama "aha fikri" kendiliğinden kurulmuyor: bölüm tablodaki mekanikleri ve sayıları taşıyor, fikrin bulmacadaki yeri elle seçilmedi (§10.4 onayı).
- Otopark bölümleri arama sınırı yüzünden plandaki alandan küçük olabilir (not sütunu).
- Vinç "k" ve "r" bırakmaları zararsız karelere düşüyor; çözüm için gerekli hâle getirilmedi.
- Büyük alanlarda çözüm "bilinen en iyi"dir; daha kısa bir yol olabilir.
