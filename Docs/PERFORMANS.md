# Performans — kasma düzeltmesi (01.11.2026)

Ölçüm: oyunu `-perf <dosya>` ile başlatınca ana sayfayı ve dokuz bölümü (1, 10, 25, 40, 60, 75, 90, 110, 125) kendisi açıyor. Her bölümde önce boşta bekliyor, sonra kayıtlı çözümden 14 hamle oynuyor, ardından 4 kez geri alıyor. Kare süreleri, çöp toplama sayısı, çizilen parça ve üçgen sayısı dosyaya yazılıyor (`Scripts/Game/PerfProbe.cs`). Bu sayfadaki ölçümler Windows exe'de, 540×960 pencerede alındı.

## Bulunan nedenler ve çözümler

| Neden | Etkisi | Çözüm |
|---|---|---|
| Her hamleden sonra arka planda 150 bin durumluk çözücü araması başlıyordu ("takıldın" kontrolü için). | Oynarken 6–10 saniyede 500–900 çöp toplama vardı; her biri bütün oyunu bir an durduruyordu. | Oyuncu kayıtlı çözümün yolundaysa arama yapılmıyor, çünkü bölüm kazanılabilir. Yolun dışındaysa arama oyuncu 0,8 sn durunca başlıyor ve düşük öncelikli kendi iş parçacığında çalışıyor. |
| Şehir süsü ve sarı bariyer binlerce küçük parçadan oluşuyordu. | Bir bölümde 2.000–3.000 ayrı çizim yapılıyordu. | Hareket etmeyen süs, aynı malzemeye göre 8×8 m'lik bölgelerde tek ağa birleştiriliyor (`MeshMerger`). Ekran dışındaki bölgeler yine çizilmiyor. Şeffaf, hareketli ve yazılı parçalara dokunulmuyor. |
| Her kasa modeli sahneye konurken kendi malzeme kopyalarını yaratıyordu. | Yüzlerce gereksiz malzeme oluşuyordu. | Oyuncak görünümlü malzeme her model malzemesi için bir kez yapılıp ortak kullanılıyor. Görünümü tek başına değişen kasa (parlama, saydamlaşma) yine kendi kopyasını alıyor. |
| Geri Al, bariyer ve saha süsü dahil bütün kartı baştan kuruyordu. | Her geri almada bir kare takılıyordu. | Bölümün düzeni değişmediyse bariyer ve saha süsü olduğu gibi kullanılıyor. |
| Bariyer direklerinin tepesindeki minik toplar 768 üçgenliydi. | Yalnızca bu toplar yaklaşık 100 bin üçgen tutuyordu. | 168 üçgenlik küre kullanılıyor. Ekranda birkaç piksellik parçalar olduğu için fark görünmüyor. |

Görüntü kalitesine dokunulmadı. Gölgeler, MSAA, ışık, malzemeler ve modeller aynı. Önceki ve sonraki ekran görüntüleri birebir aynı.

## Önce ve sonra

| | Önce | Sonra |
|---|---|---|
| Oynarken çöp toplama (bölüm başına) | 120–900 | 0–2 |
| Çizilen parça (bölüm 90) | 3.066 | 1.032 |
| Çizilen parça (bölüm 1) | 2.072 | 491 |
| En kötü kare, oynarken | 48–74 ms | 17 ms |
| En kötü kare, geri almada (önce: birleştirmeden sonra ölçüldü) | 20–37 ms | 17–20 ms |
| 33 ms'yi aşan kare (bir bölümde) | 8'e kadar | 0 |
