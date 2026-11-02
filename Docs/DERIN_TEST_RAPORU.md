# Derin test raporu — 125 lig bölümü (01.11.2026)

Her bölüm Unity dışında, oyunun kendi kurallarıyla (Core + Solver) üç kez sınandı:

1. **Kayıtlı çözüm oynatıldı.** Bölümün dosyasındaki çözüm gerçek kurallarla baştan sona oynanır ve son hamlede kazanmalıdır.
2. **Küçük arama (30 bin durum, BFS).** Bölüm bu aramayla çözülüyorsa fazla kolaydır.
3. **Büyük arama (A*, 1 milyon durum, 60 sn).** Bu arama en kısa çözümü bulur. Çözemiyorsa bölümün durum uzayı çok büyüktür, yani zordur.

## Sonuç

| | |
|---|---|
| Kayıtlı çözümü kazanan | **125 / 125**: çözülemeyen bölüm yok |
| Küçük aramayla çözülen | 4 / 125 (yalnız öğretici 1–4) |
| Büyük aramanın bile çözemediği | 108 / 125 |
| Yeniden üretilen (fazla kolaydı) | 26, 33, 52, 68, 104 |
| Kayıtlı çözümü kısaltılan | 9 (64 → 27), 38 (57 → 39), 48 (59 → 39) |

**Yeniden üretilenler.** Bu beş bölüm kendi sıralarına göre fazla kolaydı. Örneğin 104 Usta rütbesinde olmasına rağmen 45 bin durumla çözülüyordu. Her biri için 6 farklı tohumla aday üretildi. Seçilen aday, büyük aramanın 1 milyon durumda çözemediği adaydır. Bölümlerin kuralları değişmedi: renkli kasalar yalnızca orijinal üç renkte (kırmızı, turkuaz, beyaz) ve görev kasasının yolunda bir arada.

**Kısaltılan çözümler.** Yıldızlar kayıtlı çözüme göre verilir (★★★ için en çok kayıtlı çözüm +%10). Bu üç bölümde çözüm en kısa hâline indirildi, böylece üç yıldız gerçekten iyi oynamak demek.

Öğretici bölümler (1–14) bilerek kolay; ilk zor duvar 15. bölümden sonra başlıyor.

## Bölüm bölüm

| # | Ad | Tür | Kayıtlı çözüm | Hamle | Büyük arama | Not |
|---|---|---|---|---|---|---|
| 1 | İlk Koli | hedef | geçti | 3 | en kısa 3 hamle, 10 durum |  |
| 2 | İki Koli | hedef | geçti | 4 | en kısa 4 hamle, 14 durum |  |
| 3 | Köşeyi Dön | hedef | geçti | 7 | en kısa 7 hamle, 122 durum |  |
| 4 | Önce Arkadaki | hedef | geçti | 13 | en kısa 13 hamle, 1108 durum |  |
| 5 | Geçici Park | hedef | geçti | 15 | en kısa 15 hamle, 2027 durum |  |
| 6 | Dar Koridor | hedef | geçti | 26 | en kısa 26 hamle, 14352 durum |  |
| 7 | Ters Yön | hedef | geçti | 23 | en kısa 23 hamle, 259592 durum |  |
| 8 | Fazla Koli | hedef | geçti | 23 | en kısa 23 hamle, 163830 durum |  |
| 9 | Çember | hedef | geçti | 27 | en kısa 27 hamle, 598485 durum | kayıtlı çözüm en kısaya indirildi (64 → 27) |
| 10 | Görev Kolisi | görev | geçti | 24 | en kısa 24 hamle, 24643 durum |  |
| 11 | Boşluk Yürüt | görev | geçti | 32 | en kısa 32 hamle, 205805 durum |  |
| 12 | Kanca Geliyor | görev | geçti | 23 | en kısa 23 hamle, 25129 durum |  |
| 13 | Vinci Durdur | görev | geçti | 22 | en kısa 22 hamle, 22064 durum |  |
| 14 | Kapanan Kapı | görev | geçti | 30 | en kısa 30 hamle, 57374 durum |  |
| 15 | Otopark Tıkandı | kamyon | geçti | 38 | 1 milyon durumda çözülemedi (zor) |  |
| 16 | Dolu Boşluk | görev | geçti | 25 | 1 milyon durumda çözülemedi (zor) |  |
| 17 | Uzun Koridor | görev | geçti | 75 | 1 milyon durumda çözülemedi (zor) |  |
| 18 | Üç Bırakma | görev | geçti | 26 | en kısa 26 hamle, 34068 durum |  |
| 19 | Uzak Boşluk | görev | geçti | 56 | 1 milyon durumda çözülemedi (zor) |  |
| 20 | Üçle ve İndir | görev | geçti | 32 | 1 milyon durumda çözülemedi (zor) |  |
| 21 | Renk Seçimi | görev | geçti | 57 | 1 milyon durumda çözülemedi (zor) |  |
| 22 | Dik Üçlü | görev | geçti | 66 | 1 milyon durumda çözülemedi (zor) |  |
| 23 | Kesişim | görev | geçti | 76 | 1 milyon durumda çözülemedi (zor) |  |
| 24 | Kanca ve Renk | görev | geçti | 51 | 1 milyon durumda çözülemedi (zor) |  |
| 25 | Öğrenci Sınavı | kamyon | geçti | 63 | 1 milyon durumda çözülemedi (zor) |  |
| 26 | Yeni Kasalar | görev | geçti | 47 | 1 milyon durumda çözülemedi (zor) | yeniden üretildi (eskisi kolaydı) |
| 27 | Karşı Uçtan | görev | geçti | 87 | 1 milyon durumda çözülemedi (zor) |  |
| 28 | Köprü Kurulur | görev | geçti | 44 | 1 milyon durumda çözülemedi (zor) |  |
| 29 | Tek Pencere | görev | geçti | 64 | 1 milyon durumda çözülemedi (zor) |  |
| 30 | İkisi Bir Arada | görev | geçti | 73 | 1 milyon durumda çözülemedi (zor) |  |
| 31 | Kaydır ve Patlat | görev | geçti | 68 | 1 milyon durumda çözülemedi (zor) |  |
| 32 | Yanlış Üçlü | görev | geçti | 86 | 1 milyon durumda çözülemedi (zor) |  |
| 33 | Çift Kanca | görev | geçti | 54 | 1 milyon durumda çözülemedi (zor) | yeniden üretildi (eskisi kolaydı) |
| 34 | Vinç Yarışı | görev | geçti | 58 | 1 milyon durumda çözülemedi (zor) |  |
| 35 | Kasa Arabayı Kapatıyor | kamyon | geçti | 66 | 1 milyon durumda çözülemedi (zor) |  |
| 36 | Uzun Yol | görev | geçti | 79 | 1 milyon durumda çözülemedi (zor) |  |
| 37 | Dört Renk | görev | geçti | 92 | 1 milyon durumda çözülemedi (zor) |  |
| 38 | Kapat Beni | görev | geçti | 39 | en kısa 39 hamle, 562394 durum | kayıtlı çözüm en kısaya indirildi (57 → 39) |
| 39 | Dörtlü | görev | geçti | 85 | 1 milyon durumda çözülemedi (zor) |  |
| 40 | Ağır Yük | görev | geçti | 63 | 1 milyon durumda çözülemedi (zor) |  |
| 41 | Ağır Köprü | görev | geçti | 44 | 1 milyon durumda çözülemedi (zor) |  |
| 42 | Renkli Teslimat | görev | geçti | 56 | 1 milyon durumda çözülemedi (zor) |  |
| 43 | Doğru Yere Düş | görev | geçti | 56 | 1 milyon durumda çözülemedi (zor) |  |
| 44 | Kesişen Koridorlar | görev | geçti | 91 | 1 milyon durumda çözülemedi (zor) |  |
| 45 | İki Kamyon | kamyon | geçti | 72 | 1 milyon durumda çözülemedi (zor) |  |
| 46 | Uzak B | görev | geçti | 108 | 1 milyon durumda çözülemedi (zor) |  |
| 47 | Ağır Renk | görev | geçti | 103 | 1 milyon durumda çözülemedi (zor) |  |
| 48 | Saat Gibi | görev | geçti | 39 | en kısa 39 hamle, 168930 durum | kayıtlı çözüm en kısaya indirildi (59 → 39) |
| 49 | Nefes | görev | geçti | 84 | 1 milyon durumda çözülemedi (zor) |  |
| 50 | Stajyer Sınavı | kamyon | geçti | 68 | 1 milyon durumda çözülemedi (zor) |  |
| 51 | Plastik Kasalar | görev | geçti | 85 | 1 milyon durumda çözülemedi (zor) |  |
| 52 | Üç Kanca | görev | geçti | 74 | 1 milyon durumda çözülemedi (zor) | yeniden üretildi (eskisi kolaydı) |
| 53 | Yükü Kullan | görev | geçti | 76 | 1 milyon durumda çözülemedi (zor) |  |
| 54 | Beşli | görev | geçti | 122 | 1 milyon durumda çözülemedi (zor) |  |
| 55 | Otopark Labirenti | kamyon | geçti | 97 | 1 milyon durumda çözülemedi (zor) |  |
| 56 | Çift Görev | görev | geçti | 89 | 1 milyon durumda çözülemedi (zor) |  |
| 57 | Ayrılan Yollar | görev | geçti | 111 | 1 milyon durumda çözülemedi (zor) |  |
| 58 | Dönüş Yok | görev | geçti | 121 | 1 milyon durumda çözülemedi (zor) |  |
| 59 | Tuzak Renk | görev | geçti | 129 | 1 milyon durumda çözülemedi (zor) |  |
| 60 | Ağır Duvar | görev | geçti | 57 | 1 milyon durumda çözülemedi (zor) |  |
| 61 | Köprü Zinciri | görev | geçti | 83 | 1 milyon durumda çözülemedi (zor) |  |
| 62 | Gizli Üçlü | görev | geçti | 131 | 1 milyon durumda çözülemedi (zor) |  |
| 63 | Kalabalık | görev | geçti | 93 | 1 milyon durumda çözülemedi (zor) |  |
| 64 | Kilit Zinciri | görev | geçti | 118 | 1 milyon durumda çözülemedi (zor) |  |
| 65 | Kamyon ve Kanca | kamyon | geçti | 86 | 1 milyon durumda çözülemedi (zor) |  |
| 66 | Uzun Teslimat | görev | geçti | 129 | 1 milyon durumda çözülemedi (zor) |  |
| 67 | Renk Ödünç | görev | geçti | 134 | 1 milyon durumda çözülemedi (zor) |  |
| 68 | Sıkışık | görev | geçti | 76 | 1 milyon durumda çözülemedi (zor) | yeniden üretildi (eskisi kolaydı) |
| 69 | Vinç Altında | görev | geçti | 136 | 1 milyon durumda çözülemedi (zor) |  |
| 70 | Ağır Patlatma | görev | geçti | 125 | 1 milyon durumda çözülemedi (zor) |  |
| 71 | Dolambaç | görev | geçti | 130 | 1 milyon durumda çözülemedi (zor) |  |
| 72 | Son Boşluk | görev | geçti | 82 | 1 milyon durumda çözülemedi (zor) |  |
| 73 | Üç Kanca Dansı | görev | geçti | 88 | 1 milyon durumda çözülemedi (zor) |  |
| 74 | Renk Fırtınası | görev | geçti | 190 | 1 milyon durumda çözülemedi (zor) |  |
| 75 | Çırak Sınavı | kamyon | geçti | 114 | 1 milyon durumda çözülemedi (zor) |  |
| 76 | Metal Kasalar | görev | geçti | 135 | 1 milyon durumda çözülemedi (zor) |  |
| 77 | Karşılıklı | görev | geçti | 154 | 1 milyon durumda çözülemedi (zor) |  |
| 78 | Dar Geçit Patlatma | görev | geçti | 197 | 1 milyon durumda çözülemedi (zor) |  |
| 79 | Kancadan Görev | görev | geçti | 57 | 1 milyon durumda çözülemedi (zor) |  |
| 80 | Dev Saha | görev | geçti | 133 | 1 milyon durumda çözülemedi (zor) |  |
| 81 | Artık İşe Yarar | görev | geçti | 155 | 1 milyon durumda çözülemedi (zor) |  |
| 82 | Yarım Köprü | görev | geçti | 54 | 1 milyon durumda çözülemedi (zor) |  |
| 83 | Kayan Duvar | görev | geçti | 85 | 1 milyon durumda çözülemedi (zor) |  |
| 84 | Karakter Köprüsü | görev | geçti | 65 | 1 milyon durumda çözülemedi (zor) |  |
| 85 | Otopark ve Kancalar | kamyon | geçti | 125 | 1 milyon durumda çözülemedi (zor) |  |
| 86 | Çapraz Teslim | görev | geçti | 170 | 1 milyon durumda çözülemedi (zor) |  |
| 87 | Bir Hamle Önce | görev | geçti | 106 | 1 milyon durumda çözülemedi (zor) |  |
| 88 | Patlat ve Kilitle | görev | geçti | 165 | 1 milyon durumda çözülemedi (zor) |  |
| 89 | Labirent | görev | geçti | 142 | 1 milyon durumda çözülemedi (zor) |  |
| 90 | Üç Durak | görev | geçti | 158 | 1 milyon durumda çözülemedi (zor) |  |
| 91 | Ağır Görev | görev | geçti | 69 | 1 milyon durumda çözülemedi (zor) |  |
| 92 | Renk Zinciri | görev | geçti | 196 | 1 milyon durumda çözülemedi (zor) |  |
| 93 | Sabır | görev | geçti | 82 | 1 milyon durumda çözülemedi (zor) |  |
| 94 | Dört Kanca | görev | geçti | 189 | 1 milyon durumda çözülemedi (zor) |  |
| 95 | Büyük Otopark | kamyon | geçti | 135 | 1 milyon durumda çözülemedi (zor) |  |
| 96 | Gece Vardiyası | görev | geçti | 172 | 1 milyon durumda çözülemedi (zor) |  |
| 97 | Ayna | görev | geçti | 199 | 1 milyon durumda çözülemedi (zor) |  |
| 98 | İç İçe Üçlüler | görev | geçti | 221 | 1 milyon durumda çözülemedi (zor) |  |
| 99 | Son Prova | görev | geçti | 147 | 1 milyon durumda çözülemedi (zor) |  |
| 100 | Kalfa Sınavı | kamyon | geçti | 131 | 1 milyon durumda çözülemedi (zor) |  |
| 101 | Usta İşi | görev | geçti | 155 | 1 milyon durumda çözülemedi (zor) |  |
| 102 | Üç Görev | görev | geçti | 197 | 1 milyon durumda çözülemedi (zor) |  |
| 103 | Vinç Ustası | görev | geçti | 165 | 1 milyon durumda çözülemedi (zor) |  |
| 104 | Kayan Şehir | görev | geçti | 96 | 1 milyon durumda çözülemedi (zor) | yeniden üretildi (eskisi kolaydı) |
| 105 | Otopark Ustası | kamyon | geçti | 139 | 1 milyon durumda çözülemedi (zor) |  |
| 106 | Altı Renk | görev | geçti | 280 | 1 milyon durumda çözülemedi (zor) |  |
| 107 | Ağır Ustalık | görev | geçti | 94 | 1 milyon durumda çözülemedi (zor) |  |
| 108 | Uzun Yol II | görev | geçti | 203 | 1 milyon durumda çözülemedi (zor) |  |
| 109 | Köprüler | görev | geçti | 66 | 1 milyon durumda çözülemedi (zor) |  |
| 110 | Tek Nefes | görev | geçti | 170 | 1 milyon durumda çözülemedi (zor) |  |
| 111 | Renkli Kancalar | görev | geçti | 215 | 1 milyon durumda çözülemedi (zor) |  |
| 112 | Görev Zinciri | görev | geçti | 216 | 1 milyon durumda çözülemedi (zor) |  |
| 113 | Kayıp Üçlü | görev | geçti | 257 | 1 milyon durumda çözülemedi (zor) |  |
| 114 | Sıkışan Kamyon | kamyon | geçti | 173 | 1 milyon durumda çözülemedi (zor) |  |
| 115 | Otopark Finali | kamyon | geçti | 136 | 1 milyon durumda çözülemedi (zor) |  |
| 116 | Patlat ve Taşı | görev | geçti | 251 | 1 milyon durumda çözülemedi (zor) |  |
| 117 | Ağır Labirent | görev | geçti | 261 | 1 milyon durumda çözülemedi (zor) |  |
| 118 | Sessiz Vinçler | görev | geçti | 130 | 1 milyon durumda çözülemedi (zor) |  |
| 119 | Dört Durak | görev | geçti | 223 | 1 milyon durumda çözülemedi (zor) |  |
| 120 | Usta Ayna | görev | geçti | 250 | 1 milyon durumda çözülemedi (zor) |  |
| 121 | Son Kilit | görev | geçti | 246 | 1 milyon durumda çözülemedi (zor) |  |
| 122 | Büyük Patlatma | görev | geçti | 293 | 1 milyon durumda çözülemedi (zor) |  |
| 123 | Üç Görev II | görev | geçti | 195 | 1 milyon durumda çözülemedi (zor) |  |
| 124 | Boss'a Doğru | görev | geçti | 234 | 1 milyon durumda çözülemedi (zor) |  |
| 125 | Crate Boss | kamyon | geçti | 132 | 1 milyon durumda çözülemedi (zor) |  |
