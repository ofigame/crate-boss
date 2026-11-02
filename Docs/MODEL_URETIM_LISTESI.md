# Crate Boss — üretilecek modeller ve promptları

31.10.2026. Sahip modelleri kendisi üretecek; bu liste oyundaki **her nesneyi** kapsar (park etmiş ve yolda giden arabalar hariç: onlar kodla çiziliyor, sahip onları beğendi).

## Kurallar

- **Görünüm:** gerçek nesnenin oyuncak kopyası; **sağlam, sert enjeksiyon plastik** (erimiş, şişmiş, damla gibi değil). Net düz yüzeyler, küçük ve eşit yuvarlatılmış kenarlar, saten parlaklık, düz şeker renkleri. Kir, pas, yazı, logo yok.
- Her promptun sonunda aynı **stil cümlesi** vardır (aşağıdaki promptlara zaten eklendi, olduğu gibi kopyala).
- **Dosya:** GLB (tercih) ya da FBX, PBR dokulu (base color, normal, roughness/metallic). Tek nesne, tek dosya. Dosya adı tablodaki **Dosya** sütunu (`crate_wood.glb` gibi).
- **Konum:** `E:/OFIGAME/OYUNLAR/3D_Modeller/oyuncak/sahip/` klasörüne koy; ben oradan alıp ölçeklerim, yüzeyini düşürürüm, oyuna bağlarım.
- **Ölçü:** tablodaki ölçü oyundaki son ölçüdür (1 = bir kare). Model hangi ölçüde gelirse gelsin tek ölçekle büyütüp küçültürüm; **oranları** tabloya uysun yeter (kasa küp, araba yok, vb.).
- **Yön:** "+Z yönünde" = nesnenin önü kameraya göre ileri. Tutmazsa sorun değil, ben çeviririm; yalnız **taban düz ve aşağıda** olsun.
- **Yüzey sayısı:** sütundaki üçgen bütçesi hedeftir; fazlası gelirse düşürürüm.
- Görsel önce üretilip beğenilirse 3D'ye çevirmek daha kontrollüdür (Ana Belge §19.1).

## Stil cümlesi

```
toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

## Oyun alanı — kasalar

### 1. Ahşap kasa

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `crate_wood.glb` | 0,86 × 0,86 × 0,86 | 1.000 | Kare taban, kapak üstte |

```
A toy replica of a real wooden shipping crate: a closed cube with a flat lid, four wide horizontal planks on each side held by a thick flat frame around every edge, warm honey-orange painted wood colour (#F6B868 to #E0913F), plank lines shown as clean shallow grooves. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 2. Buz kasa

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `crate_ice.glb` | 0,86 × 0,86 × 0,86 | 1.000 | Kare taban |

```
A toy replica of an ice block crate: a closed cube with slightly rounded edges, light turquoise frosted plastic that looks like ice, a white snowflake emblem raised on the top face, a thin white frost rim along the top edges. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 3. Çelik (ağır) kasa

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `crate_heavy.glb` | 0,94 × 0,94 × 0,76 | 1.200 | Kare taban, alçak ve geniş |

```
A toy replica of a heavy steel cargo block: low and wide box with a perfectly square footprint, dark slate grey plastic, thick yellow and black diagonal hazard stripes running around all four sides, four big round bolt heads on the top corners, two chunky lifting lugs on top. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 4. Altın kasa

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `crate_gold.glb` | 0,86 × 0,86 × 0,86 | 1.000 | Kare taban |

```
A toy replica of a treasure crate: a closed cube with a flat lid and a thick frame on every edge, glossy golden yellow plastic, a raised five-pointed star emblem in the middle of every side and on the lid. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 5. Teslim kasası (D)

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `crate_delivery.glb` | 0,86 × 0,86 × 0,86 | 1.000 | Kare taban, ok +Z yönünde |

```
A toy replica of a parcel delivery crate: a closed cube with a flat lid and a thick frame on every edge, bright red plastic, a big raised white arrow pointing forward on the lid and a white upward arrow on each side. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 6. Eşleşme kasası – kırmızı

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `crate_match_red.glb` | 0,86 × 0,86 × 0,86 | 1.000 | Kare taban |

```
A toy replica of a stackable plastic market crate (the kind used for fruit and bottles): a sturdy open-top box with a solid bottom, ribbed side walls with rectangular vent holes in a neat grid, two hand-hold slots on opposite sides, glossy red plastic (#EF4444). toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 7. Eşleşme kasası – mavi

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `crate_match_blue.glb` | 0,86 × 0,86 × 0,86 | 1.000 | Kare taban |

```
A toy replica of a stackable plastic market crate (the kind used for fruit and bottles): a sturdy open-top box with a solid bottom, ribbed side walls with rectangular vent holes in a neat grid, two hand-hold slots on opposite sides, glossy blue plastic (#3D8BF0). toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 8. Eşleşme kasası – yeşil

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `crate_match_green.glb` | 0,86 × 0,86 × 0,86 | 1.000 | Kare taban |

```
A toy replica of a stackable plastic market crate (the kind used for fruit and bottles): a sturdy open-top box with a solid bottom, ribbed side walls with rectangular vent holes in a neat grid, two hand-hold slots on opposite sides, glossy green plastic (#3DCC6B). toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 9. Eşleşme kasası – sarı

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `crate_match_yellow.glb` | 0,86 × 0,86 × 0,86 | 1.000 | Kare taban |

```
A toy replica of a stackable plastic market crate (the kind used for fruit and bottles): a sturdy open-top box with a solid bottom, ribbed side walls with rectangular vent holes in a neat grid, two hand-hold slots on opposite sides, glossy yellow plastic (#FFD23F). toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 10. Eşleşme kasası – mor

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `crate_match_purple.glb` | 0,86 × 0,86 × 0,86 | 1.000 | Kare taban |

```
A toy replica of a stackable plastic market crate (the kind used for fruit and bottles): a sturdy open-top box with a solid bottom, ribbed side walls with rectangular vent holes in a neat grid, two hand-hold slots on opposite sides, glossy purple plastic (#9B5DE5). toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 11. Vinçle gelen konteyner (K)

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `container_drop.glb` | 0,90 × 0,90 × 0,74 | 1.200 | Kare taban |

```
A toy replica of a short shipping container cut down to a cube-like block: corrugated steel-look side walls as clean vertical ribs, double doors with two locking bars on the front, corner castings on all eight corners, dark blue plastic with lighter blue corners. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

## Oyun alanı — zemin parçaları

### 12. Hedef yuvası

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `target_plate.glb` | 0,86 × 0,86, kalınlık 0,04 | 500 | Düz, üst yüz yukarı |

```
A toy replica of a painted parking bay floor marker: a thin flat square plate with a raised white frame and a glowing mint-green inner border, a small square in the centre, completely flat underside. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 13. Varış noktası (A)

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `arrival_bay.glb` | 0,9 × 0,9, kalınlık 0,05 | 500 | Düz |

```
A toy replica of a loading bay finish pad: a thin flat square plate with a thick raised yellow frame, a black and white chequered flag symbol raised in the middle, flat underside. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 14. Zemin kapağı (eşleşme, çukur)

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `floor_hatch.glb` | 0,96 × 0,96, kalınlık 0,08 | 800 | Düz; iki kapak ayrı parça |

```
A toy replica of a square industrial floor hatch: a thick square frame with yellow and black diagonal hazard stripes, two flat grey lid halves as separate hinged parts that meet in the middle, small round hinge pins on the outer edges. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 15. Kantar plakası

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `weighbridge.glb` | 0,86 × 0,86, kalınlık 0,05 | 800 | Düz |

```
A toy replica of a vehicle weighing scale plate: a thin square grey plate with a raised yellow frame, a small round dial gauge sticking up at one side. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 16. Bagaj bandı karosu

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `conveyor_tile.glb` | 1 × 1, boy 0,12 | 800 | Ok +Z yönünde |

```
A toy replica of one square section of an airport baggage conveyor belt: dark grey rubber belt on top with rounded silver side rails, small rollers visible at the ends, a big yellow arrow painted on the belt pointing forward. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

## Oyun alanı — makineler

### 17. Forklift

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `forklift.glb` | 0,9 × 0,9 (çatallar dahil), boy ≈0,9 | 3.000 | Çatallar +Z yönünde, yere inik |

```
A toy replica of a compact warehouse forklift: chunky bright orange body, black overhead safety cage, big black wheels, a short upright mast with two flat forks lowered to the ground at the front, a small orange beacon light on the cage roof, seat and steering wheel visible, compact proportions that fit a square footprint. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 18. Kule vinç direk parçası

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `crane_mast.glb` | 0,9 × 0,9, boy 1,35 | 1.000 | Dik; aynı parçalar üst üste eklenir |

```
A toy replica of one modular tower crane mast section: a square yellow lattice column with straight vertical corner tubes and diagonal braces on all four sides, flat top and bottom ends so identical sections stack seamlessly, height 1.5 times its width. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 19. Kule vinç üst kısmı

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `crane_top.glb` | bom ≈7, kule başı 0,9 | 6.000 | Bom +Z yönünde |

```
A toy replica of a tower crane top: a small square slewing platform, a rounded operator cabin with large light blue windows, a long straight yellow lattice jib pointing forward, a short counter-jib backwards carrying grey concrete counterweight blocks, a small peak with tie bars, no hook, no cables. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 20. Vinç arabası

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `crane_trolley.glb` | 0,6 × 0,6, boy 0,4 | 1.000 | Tekerlekler üstte |

```
A toy replica of a crane trolley: a compact yellow box frame with four small wheels on top that run on a rail, a black pulley block hanging underneath. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 21. Vinç kancası

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `crane_hook.glb` | 0,45 × 0,45, boy 0,5 | 1.000 | Kanca aşağıda |

```
A toy replica of a crane hook block: a chunky pulley housing with yellow and black stripes, a big grey steel-look hook below it with a safety latch. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 22. Vinç beton temeli

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `crane_base.glb` | 1,4 × 1,4, boy 0,3 | 800 | Kare taban |

```
A toy replica of a tower crane foundation: a square light grey concrete slab with four thick anchor bolts sticking up at the corners and a yellow-black striped edge. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 23. Yükleme kamyonu

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `truck.glb` | kabin 2 × 1,2; kasa yükleme alanı kadar | 3.000 | Kabin +Z yönünde |

```
A toy replica of a delivery lorry: chunky red cab with big windows, a long flat white open cargo bed with low side walls and a fold-down rear tailgate, big round black wheels, simple round headlights. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

## Oyun alanı — kedi

### 24. Kedi

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `cat.glb` | 0,5 × 0,7, boy 0,45 | 2.000 | Yüzü +Z yönünde |

```
A toy replica of an orange tabby cat figure: standing pose, round head with pointed ears, short legs, tail curled up, simple painted stripes, big friendly eyes painted on. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 25. Kedi sepeti

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `cat_basket.glb` | 0,8 × 0,8, boy 0,2 | 600 | Düz |

```
A toy replica of a round woven cat bed: a low round basket with a basket-weave pattern, light brown, a soft red cushion inside. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

## Oyun alanı — kapılar ve bariyerler

### 26. Bariyer kolu (kantar kapısı)

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `gate_arm.glb` | 1 × 0,2, boy 0,6 | 800 | Kol +X yönünde, inik |

```
A toy replica of a parking barrier gate: a chunky grey post box with a small round red lamp on top, a long arm with red and white stripes, arm in the down position. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 27. Vardiya çıkışı

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `exit_gate.glb` | 1 × 0,3, boy 0,8 | 800 | Kol +X yönünde |

```
A toy replica of a factory exit turnstile gate: two short grey posts with a green and white striped arm between them, a small green lamp and a door icon sign on one post. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 28. Sarı bariyer – düz

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `barrier_straight.glb` | 1 × 0,15, boy 0,45 | 800 | Uzunluk X yönünde, tam 1 birim |

```
A toy replica of a yellow traffic safety guardrail segment: two thick straight horizontal rails on two short square posts, bright yellow with black stripes on the posts, exactly 1 unit long. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 29. Sarı bariyer – uç direk

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `barrier_post.glb` | 0,15 × 0,15, boy 0,5 | 400 | Dik |

```
A toy replica of a single yellow safety bollard post: a short square post with a rounded cap and two black stripes, on a small square foot. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

## Saha içi engeller (2 karelik park etmiş araba kodla çiziliyor; üretme)

### 30. Palet yığını (Şehir)

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `obstacle_pallets.glb` | 0,9 × 0,9, boy 0,8 | 800 | Kare taban |

```
A toy replica of a stack of three wooden pallets with a wrapped load on top: light honey wood slats and blocks clearly separated, a pale blue stretch-wrapped box load on the top pallet. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 31. Beton blok (Şehir)

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `obstacle_concrete.glb` | 0,9 × 0,9, boy 0,6 | 800 | Kare taban |

```
A toy replica of a concrete road barrier block: light grey block with a sloped profile and a flat top, one yellow reflective stripe across the front, square footprint. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 32. Çöp konteyneri (Şehir)

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `obstacle_dumpster.glb` | 0,9 × 0,9, boy 0,8 | 800 | Kare taban |

```
A toy replica of a street wheelie dumpster: green body with a black hinged lid, four small black wheels, a push handle at the back, square footprint. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 33. Trafik konisi grubu

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `obstacle_cones.glb` | 0,9 × 0,9, boy 0,6 | 800 | Kare taban |

```
A toy replica of three orange traffic cones with white reflective bands standing on a small square black rubber base. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 34. Tek katlı konteyner (Liman)

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `obstacle_container1.glb` | 0,96 × 0,96, boy 0,76 | 1.000 | Kare taban |

```
A toy replica of a short shipping container shaped as a cube: corrugated walls as clean vertical ribs, double doors with locking bars, corner castings, faded teal blue plastic. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 35. Halat babası (Liman)

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `obstacle_bollard.glb` | 0,9 × 0,9, boy 0,7 | 800 | Kare taban |

```
A toy replica of a harbour mooring bollard: a thick black iron-look post with a wide cap, standing on a square grey concrete plinth. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 36. Bagaj arabası (Havalimanı)

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `obstacle_baggage_cart.glb` | 0,9 × 0,9, boy 0,7 | 1.000 | Çeki demiri +Z yönünde |

```
A toy replica of an airport baggage cart: low flat bed on four small black wheels, low side rails, a tow bar at the front, two colourful suitcases on it. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 37. Hava kargo konteyneri (Havalimanı)

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `obstacle_uld.glb` | 0,9 × 0,9, boy 0,8 | 800 | Kare taban |

```
A toy replica of an air cargo ULD container: silver-grey box with one slanted side, a blue curtain door on the front, a flat base rim. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 38. Soğuk kasa yığını (Soğuk Depo)

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `obstacle_reefer.glb` | 0,9 × 0,9, boy 0,8 | 800 | Kare taban |

```
A toy replica of two stacked white refrigerated boxes with dark vent slots on the front and a small blue snowflake sticker on each. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 39. Büyük buz kalıbı (Soğuk Depo)

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `obstacle_iceblock.glb` | 0,9 × 0,9, boy 0,7 | 800 | Kare taban |

```
A toy replica of a large frosted ice block on a small wooden pallet: opaque pale blue-white block with a white frost rim on top. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 40. Yol barikatı (Gümrük)

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `obstacle_barricade.glb` | 0,9 × 0,9, boy 0,6 | 800 | Kare taban |

```
A toy replica of a concrete road barricade: light grey block with red and white stripes on the front, square footprint. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 41. Mühürlü konteyner (Gümrük)

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `obstacle_sealed.glb` | 0,96 × 0,96, boy 0,76 | 1.000 | Kare taban |

```
A toy replica of a short sealed shipping container shaped as a cube: corrugated walls, double doors with a big red customs seal tag on the handles, dark green plastic. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 42. Kontrol kulübesi (Gümrük)

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `obstacle_booth.glb` | 0,9 × 0,9, boy 0,9 | 1.000 | Kapı +Z yönünde |

```
A toy replica of a small checkpoint booth: white walls, a flat dark roof slightly larger than the walls, a big front window, a small door on the side. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 43. Konteyner yığını (2×2 kare)

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `obstacle_containers2x2.glb` | 1,9 × 1,9, boy 1,4 | 2.000 | Kare taban |

```
A toy replica of three shipping containers stacked neatly: two containers side by side on the ground and one on top across them, each with corrugated walls, doors and corner castings, in red, blue and yellow. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

## Şehir — yapılar

### 44. Ev – iki katlı

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `house_1.glb` | 3 × 3, boy 3 | 3.000 | Kapı +Z yönünde |

```
A toy replica of a two-storey house: pastel blue walls, red pitched roof, white window frames, a front door with a small porch roof, a chimney. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 45. Ev – tek katlı

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `house_2.glb` | 3 × 3, boy 2,5 | 3.000 | Kapı +Z yönünde |

```
A toy replica of a one-storey cottage: cream walls, orange tiled roof, two windows with green shutters, a front door with a step. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 46. Ev – dar sıra ev

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `house_3.glb` | 2,5 × 3, boy 3,5 | 3.000 | Kapı +Z yönünde |

```
A toy replica of a narrow city townhouse: pink walls, flat roof with a small parapet, tall windows on two floors, a front door with a little awning. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 47. Apartman

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `apartment.glb` | 4 × 4, boy 4 | 4.000 | Giriş +Z yönünde |

```
A toy replica of a small three-storey apartment block: light yellow walls, rows of square windows with white frames, small balconies, a flat roof with a water tank, an entrance canopy. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 48. Köşe dükkânı

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `shop.glb` | 4 × 3, boy 2,8 | 3.000 | Vitrin +Z yönünde |

```
A toy replica of a corner shop: white walls, a big front shop window, a red and white striped awning, a blank sign board above the door (no text), two plant pots by the door. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 49. Depo binası

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `warehouse.glb` | 5 × 4, boy 3,5 | 3.000 | Kapı +Z yönünde |

```
A toy replica of a small warehouse: light grey corrugated walls, a curved roof, two big roller doors at the front, a small office window. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 50. Soğuk hava deposu binası

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `cold_storage.glb` | 5 × 4, boy 3,5 | 3.000 | Kapı +Z yönünde |

```
A toy replica of a cold storage building: white insulated panel walls, flat roof with two rooftop cooling units, a big insulated loading door with a blue frame. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 51. Gümrük binası

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `customs_house.glb` | 5 × 4, boy 3,5 | 3.000 | Giriş +Z yönünde |

```
A toy replica of a small customs office building: beige walls, flat roof with a flag pole, large windows, a covered entrance with columns. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 52. Kontrol kulesi (Havalimanı)

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `control_tower.glb` | 2 × 2, boy 6 | 3.000 | Dik |

```
A toy replica of an airport control tower: a slim white tower with a wide glass cab on top, a red and white beacon on the roof. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 53. Kargo uçağı (park halinde)

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `cargo_plane.glb` | 8 × 8, boy 2,5 | 4.000 | Burun +Z yönünde |

```
A toy replica of a chubby cargo airplane parked on the ground: white fuselage, big round nose, high wings with two engines, a large rear cargo ramp, a blue stripe along the side. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 54. Liman rıhtım vinci

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `gantry_crane.glb` | 3 × 2, boy 6 | 4.000 | Bom +Z yönünde (denize) |

```
A toy replica of a ship-to-shore container crane: tall red A-frame legs on wheels, a long horizontal boom sticking forward over the water, a small operator cabin under the boom. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 55. Konteyner gemisi

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `ship.glb` | 10 × 3, boy 2,5 | 4.000 | Pruva +X yönünde |

```
A toy replica of a small container ship: dark blue hull with a red bottom stripe, white bridge tower at the back, a deck loaded with colourful stacked containers. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

## Şehir — sokak eşyaları

### 56. Ağaç – yuvarlak taç

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `tree_round.glb` | 1 × 1, boy 2 | 1.500 | Dik |

```
A toy replica of a street tree: a straight brown trunk and a big round leafy crown made of a few smooth lumps, fresh green. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 57. Ağaç – çam

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `tree_cone.glb` | 1 × 1, boy 2,2 | 1.500 | Dik |

```
A toy replica of a small pine tree: a short brown trunk and three stacked green cone layers. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 58. Çalı

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `tree_bush.glb` | 1 × 1, boy 0,6 | 800 | Dik |

```
A toy replica of a round garden bush: a low cluster of smooth green lumps on the ground. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 59. Sokak lambası

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `street_lamp.glb` | 0,3 × 0,9, boy 2,2 | 800 | Kol +Z yönünde |

```
A toy replica of a street lamp: a dark grey pole with a curved arm at the top and a lamp head pointing forward, a small square base. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 60. Bank

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `bench.glb` | 0,9 × 0,4, boy 0,5 | 800 | Oturma yüzü +Z yönünde |

```
A toy replica of a park bench: warm brown wooden slats for the seat and back, dark grey metal legs. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 61. Dur tabelası

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `stop_sign.glb` | 0,3 × 0,3, boy 1,2 | 400 | Yüzü +Z yönünde |

```
A toy replica of a stop sign: a red octagon sign on a grey pole, plain white border, no text. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 62. Çiçeklik

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `planter.glb` | 0,8 × 0,8, boy 0,5 | 800 | Kare taban |

```
A toy replica of a square street planter: a grey concrete box filled with green leaves and small red and yellow flowers. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 63. Meydan çeşmesi

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `fountain.glb` | 2 × 2, boy 1 | 2.000 | Dik |

```
A toy replica of a small round town square fountain: a low stone basin with light blue water and a short centre column. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 64. Bayrak direği

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `flag_pole.glb` | 0,4 × 0,4, boy 3 | 600 | Dik |

```
A toy replica of a flag pole: a white pole on a round base with a plain red flag (no emblem) waving at the top. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 65. Pist lambası

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `runway_light.glb` | 0,3 × 0,3, boy 0,3 | 300 | Dik |

```
A toy replica of an airfield edge light: a short yellow post with a round clear light dome on top. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 66. Gümrük giriş bariyeri

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `checkpoint_barrier.glb` | 3 × 1, boy 1,2 | 1.500 | Kol +X yönünde |

```
A toy replica of a road checkpoint: a small booth with a big window and a long red and white striped barrier arm across the lane. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

## Boss'un Deposu eşyaları

### 67. Raflar

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `depot_racks.glb` | 2 × 1, boy 2 | 2.000 | Ön +Z yönünde |

```
A toy replica of warehouse pallet racks: blue upright frames and orange beams, two shelves holding wrapped boxes. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 68. Ofis konteyneri

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `depot_office.glb` | 3 × 2, boy 2,2 | 2.000 | Kapı +Z yönünde |

```
A toy replica of a site office container: white container with a door and two windows, a small set of steps at the door. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 69. Kamyon rampası

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `depot_ramp.glb` | 3 × 2, boy 1 | 2.000 | Rampa +Z yönünde |

```
A toy replica of a truck loading dock: a raised concrete platform with a yellow-edged ramp and black rubber bumpers. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 70. Saksılar

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `depot_planters.glb` | 1 × 0,5, boy 0,6 | 800 | Dik |

```
A toy replica of two terracotta flower pots with green plants. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 71. Kedi yatağı (depo)

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `depot_catbed.glb` | 1 × 1, boy 0,3 | 800 | Düz |

```
A toy replica of a round cushioned pet bed: soft pink rim and a cream cushion. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 72. Soğuk oda kapısı

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `depot_colddoor.glb` | 2 × 0,5, boy 2,2 | 1.500 | Ön +Z yönünde |

```
A toy replica of a cold room door set in a short white wall section: thick insulated sliding door with a blue frame and a big handle. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 73. Uçak heykeli

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `depot_plane_statue.glb` | 2 × 2, boy 2 | 2.000 | Burun +Z yönünde |

```
A toy replica of a small airplane statue on a round grey stone pedestal: a white propeller plane with blue stripes. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

## Dükkân — giyilenler ve kasa kaplamaları

### 74. Baret (renk kodla değişir)

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `wear_hardhat.glb` | baş üstü | 800 | Siperlik +Z yönünde |

```
A toy replica of a construction hard hat: smooth dome with a short front brim and a raised centre ridge, plain white plastic so it can be recoloured. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 75. Kasket

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `wear_cap.glb` | baş üstü | 800 | Siperlik +Z yönünde |

```
A toy replica of a flat cap: soft rounded top with a short front peak, dark grey. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 76. İkaz yeleği (renk kodla değişir)

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `wear_vest.glb` | gövde | 1.000 | Ön +Z yönünde |

```
A toy replica of a high-visibility safety vest: sleeveless vest with two silver reflective bands around the chest, plain white so it can be recoloured. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 77. Eldiven çifti (renk kodla değişir)

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `wear_gloves.glb` | el | 600 | — |

```
A toy replica of a pair of work gloves: thick padded gloves with a cuff, plain white so they can be recoloured. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 78. Kasa kaplaması – hediye kutusu

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `skin_gift.glb` | 0,86 küp | 1.000 | Kare taban |

```
A toy replica of a gift box crate: a closed cube wrapped in red paper with a wide white ribbon crossing over the lid and a bow on top. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 79. Kasa kaplaması – karpuz kasası

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `skin_melon.glb` | 0,86 küp | 1.000 | Kare taban |

```
A toy replica of a fruit crate full of watermelons: an open wooden crate with green striped watermelons visible on top. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 80. Kasa kaplaması – oyuncak sandığı

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `skin_toychest.glb` | 0,86 küp | 1.000 | Kare taban |

```
A toy replica of a toy chest: a purple box with a curved lid, yellow star decorations and round corner caps. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

### 81. Kedi tasması

| Dosya | Oyundaki ölçü | Üçgen | Duruş |
|---|---|---|---|
| `cat_collar.glb` | kedi boynu | 300 | — |

```
A toy replica of a pet collar: a red band with a small round golden bell. toy replica of a real object, sturdy hard injection-molded plastic like a premium toy model, crisp clean shapes with small even rounded bevels, firm flat surfaces and straight edges, NOT melted, NOT blobby, NOT inflated, no drips, slightly simplified proportions, smooth satin plastic finish, clean solid colors, no dirt, no rust, no scratches, no tiny details, no text, no logos, single object, centered, isolated on plain white background, soft even studio lighting, mobile casual game asset
```

---

Toplam **81** model. Öncelik: önce kasalar, zemin parçaları, makineler ve saha içi engeller (oyunun kendisi), sonra şehir yapıları ve sokak eşyaları, en son depo ve dükkân eşyaları.
