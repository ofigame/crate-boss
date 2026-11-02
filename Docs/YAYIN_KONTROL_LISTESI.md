# Crate Boss — Yayın Kontrol Listesi

Mağazaya göndermeden önce her madde işaretlenmeli.

## Reklam (Ana Belge §15)
- [ ] Gerçek reklam SDK'sı eklendi ve `AdServiceLocator.Use(...)` ile test servisleri yerine gerçek servisler bağlandı. Şu an **test reklamları** (`TestRewardedAdService`, `TestInterstitialAdService`) kullanılıyor.
- [ ] **Test reklam birimi kimlikleri gerçekleriyle değiştirildi** (yalnızca yayın derlemesinde). Geliştirme derlemelerinde her zaman test kimlikleri.
- [ ] Ödül yalnızca "reklam tamamlandı" bildiriminde veriliyor (`RewardedAds`); reklama tıklamak ödüllendirilmiyor.
- [ ] Geçiş reklamı kuralları `Resources/AdConfig` (yoksa koddaki varsayılanlar) ile doğru: ilk 8 saha reklamsız, 3 sahada bir, 120 sn aralık, oturumda en fazla 6, ödüllüden sonra 5 dk yok.
- [ ] Banner yok, açılış reklamı yok, satın alma yok.
- [ ] Analitik: `AnalyticsHub.Log` gerçek bir arka uca bağlandı (ya da konsolda kalması bilinçli bir karar).

## İzinler ve yasal (Ana Belge §15.4)
- [ ] AB/AEA ve Birleşik Krallık için izin ekranı (Google UMP) çalışıyor; izin yoksa kişiselleştirilmemiş reklam.
- [ ] iPhone: ATT izin sorusu ve `NSUserTrackingUsageDescription` metni 9 dilde.
- [ ] Gizlilik politikası yayında: `GameController.PrivacyUrl` (https://ofigame.com/crateboss/gizlilik) açılıyor. Taslak: `Docs/GIZLILIK_POLITIKASI_TASLAK.md` (avukat kontrolü).
- [ ] KVKK aydınlatma metni gizlilik sayfasında; haritadaki "Gizlilik" düğmesi bu sayfayı açıyor.
- [ ] Google Play: hedef yaş grubu 13+; Families programı yok; veri güvenliği formu reklam SDK'sına göre dolduruldu.
- [ ] App Store: gizlilik etiketleri (reklam kimliği, cihaz bilgisi) dolduruldu.

## Oyun
- [ ] Hiçbir ekranda saha numarası yok (`AdPolicyTests.NoUiText_ShowsALevelNumber`).
- [ ] Tüm testler geçiyor; ekran görüntüsü turu hatasız.
- [ ] `Resources/Levels` oyun sahibinin onayladığı 75 saha.
- [ ] Sürüm numarası ve derleme numarası artırıldı.

- [ ] **Bütün bölümler açık (test):** `GameController.OpenAllForTesting` yayından önce `false` yapılmalı (sahip 31.10.2026 test için açtırdı).
- [ ] **Ana Belge F9 bölümleri:** oyun sahibi her 10'lu paketi oynayıp onaylamalı (§10.4); şimdiki 125 bölüm üreticinin seçimi.
- [ ] **Tripo modelleri:** rütbe 2–5 kasaları, saha engelleri, kamyon, Boss eşyaları (§19.2); şimdilik kodla çizilen yer tutucular.
- [ ] **Gerçek reklam:** AppLovin MAX + UMP; gerçek kimlikler yalnız yayın derlemesinde.
