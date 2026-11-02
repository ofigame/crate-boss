# Model raporu (otomatik)

`Tools > Crate Boss > Normalize Models` tarafından yazıldı. Ölçüler Unity biriminde (1 = bir kare). Modeller hiçbir yönde esnetilmez, tek ölçekle büyütülür.

| Model | Prefab | Üçgen (hedef) | Ham boyut (en × boy × derinlik) | Son boyut | Ölçek |
|---|---|---|---|---|---|
| `level1_normal` | `BoxNormal` | 2.356 (2.800) | 1.00 × 0.94 × 1.00 | 0.86 × 0.81 × 0.86 | 0.86 |
| `level1_fixed` | `BoxFixed` | 2.539 (2.800) | 1.00 × 0.88 × 0.99 | 0.92 × 0.81 × 0.92 | 0.92 |
| `level1_delivery` | `BoxDelivery` | 2.439 (2.800) | 1.00 × 0.92 × 1.00 | 0.86 × 0.79 × 0.86 | 0.86 |
| `level1_ice` | `BoxIce` | 2.269 (2.800) | 1.00 × 0.95 × 1.00 | 0.86 × 0.81 × 0.86 | 0.86 |
| `match_red` | `BoxMatch_1` | 2.515 (2.800) | 0.99 × 1.00 × 0.99 | 0.86 × 0.87 × 0.86 | 0.87 |
| `match_teal` | `BoxMatch_2` | 2.744 (2.800) | 0.99 × 0.92 × 1.00 | 0.85 × 0.80 × 0.86 | 0.86 |
| `match_white` | `BoxMatch_White` | 2.557 (2.800) | 1.00 × 0.95 × 1.00 | 0.86 × 0.82 × 0.86 | 0.86 |
| `crate_heavy` | `BoxHeavy` | 1.164 (1.200) | 1.00 × 0.77 × 1.01 | 0.94 × 0.72 × 0.94 | 0.93 |
| `target_plate` | `Target` | 485 (800) | 0.50 × 1.03 × 1.05 | 0.84 × 0.41 × 0.86 | 0.82 |
| `barrier_straight` | `BarrierStraight` | 968 (1.000) | 0.98 × 0.41 × 0.17 | 1.00 × 0.42 × 0.17 | 1.02 |
| `barrier_corner` | `BarrierCorner` | 970 (1.000) | 0.98 × 0.51 × 0.98 | 1.00 × 0.52 × 1.00 | 1.02 |
| `barrier_end` | `BarrierEnd` | 2.904 (1.000) **aşıyor** | 0.81 × 0.98 × 0.57 | 0.49 × 0.60 × 0.35 | 0.61 |
| `crane_body` | `CraneBody` | 7.760 (8.000) | 0.98 × 0.69 × 0.14 | 1.43 × 6.67 × 9.45 | 9.63 |
| `crane_trolley` | `CraneTrolley` | 970 (8.000) | 1.00 × 0.99 × 0.73 | 0.55 × 0.55 × 0.40 | 0.55 |
| `crane_hook` | `CraneHook` | 970 (8.000) | 0.53 × 1.00 × 0.63 | 0.22 × 0.42 × 0.26 | 0.42 |
| `obstacle_pallets` | `Obstacle_City_0` | 776 (800) | 1.06 × 1.02 × 1.01 | 0.90 × 0.87 × 0.86 | 0.85 |
| `obstacle_concrete` | `Obstacle_City_1` | 776 (800) | 0.67 × 0.86 × 1.03 | 0.59 × 0.75 × 0.90 | 0.87 |
| `obstacle_bin` | `Obstacle_City_2` | 776 (800) | 1.03 × 0.95 × 0.89 | 0.86 × 0.79 × 0.74 | 0.83 |
| `tool_hammer` | `Hammer` | 1.418 (3.000) | 0.66 × 0.75 × 1.00 | 0.53 × 0.60 × 0.80 | 0.80 |
| `worker` | `Worker` | 29.570 (10.000) **aşıyor** | boy 1.09 | boy 1.35 | 1.24 |

Karakter klipleri: `Walk` 1.03 sn (döngü), `Push` 2.67 sn (döngü), `PushHeavy` 8.00 sn (döngü), `LookAround` 6.33 sn (döngü), `Happy` 2.93 sn, `Victory` 1.67 sn (döngü), `Think` 5.50 sn.
`Push` = `karakter@Pushing.fbx` (kısa, tok itme döngüsü), `PushHeavy` = `karakter@Push.fbx` (uzun, yavaş itme). Bekleme: `Victory` %40 hızda (Breathing Idle gelene kadar).

Notlar:
- `forklift` (18.009), `crane_body` (8.878), `barrier_straight` (7.454) ve `barrier_corner` (2.568) Blender'da basitleştirildi (`E:\CLAUDE\scripts\bl_cb_decimate.py`); önce/sonra görüntülerinde fark yok.
- `barrier_end` ve `target_plate` basitleştirilince şeritler ve yüzey bozuldu; orijinal kaldı. Bölümde az sayıda oldukları için yükleri küçük.
- `worker` 29.570 üçgen: tek karakter, basitleştirme yüz ve baret detayını bozabilir; şimdilik orijinal. İstenirse LOD ya da Blender'da ~%35 basitleştirme denenebilir.
- Kasalar küp değil, tek ölçekle büyütüldü: ahşap 0.79, buz 0.85, çelik 0.92 yüksek (belgedeki 0.77 / 0.74 / 0.62 ancak esnetmeyle olurdu).
- Hedef plakası kalın bir blok (0.29); zemine gömülü, yalnızca üstteki 3 cm görünür. Boş/dolu rengi kodla çizilen parlak çerçeveden gelir.
