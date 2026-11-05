# Fart Rafter — Google Play'e yükleme

Google Play APK kabul etmiyor; imzalı **Android App Bundle (.aab)**, **targetSdk 36 (Android 16)** ve her yüklemede artan **versionCode** istiyor. Bunları `FartRaft > Google Play AAB Derle (imzalı)` menüsü yapıyor. Eski `Android APK Derle` menüsü telefona doğrudan kurmak için duruyor.

## 1. Upload key oluştur (bir kez)

Bu anahtar kaybolursa uygulamayı güncelleyemezsin (Play Console'dan sıfırlama talebi gerekir). Dosyayı ve şifreleri Git dışında bir yerde yedekle.

**Seçenek A — Unity:** `Edit > Project Settings > Player > Android > Publishing Settings > Keystore Manager > Create New`.
Dosyayı `Keystore/fartrafter-upload.keystore` olarak kaydet, alias: `upload`, geçerlilik 25+ yıl (varsayılan 50 uygun).

**Seçenek B — komut satırı** (JDK'nın `keytool`'u; Unity'nin kendi JDK'sı `Editor/Data/PlaybackEngines/AndroidPlayer/OpenJDK/bin` altında):

```
keytool -genkeypair -v -keystore Keystore/fartrafter-upload.keystore -alias upload -keyalg RSA -keysize 2048 -validity 10000
```

Sonra `Keystore/upload.properties.example` dosyasını `Keystore/upload.properties` olarak kopyala ve şifreleri yaz. `Keystore/` klasörü `.gitignore`'da; sadece örnek dosya commit'lenir.

CI'da dosya yerine ortam değişkenleri kullanılabilir: `FR_KEYSTORE_PATH`, `FR_KEYSTORE_PASS`, `FR_KEY_ALIAS`, `FR_KEY_PASS`.

## 2. AAB derle

Unity'de `FartRaft > Google Play AAB Derle (imzalı)`. Her çalıştırmada versionCode 1 artar; çıktı `Builds/Android/FartRafter-<sürüm>-<kod>.aab`.

- Sürüm adını değiştirmek için: `Project Settings > Player > Version` (ör. `0.2.0`).
- Aynı kodla tekrar derlemek (ör. Play'e henüz yüklenmemiş bir build'i düzeltmek) için: `... (sürüm kodunu artırmadan)`.
- Komut satırı:
  ```
  Unity -batchmode -quit -projectPath . -executeMethod FartRafters.EditorTools.PlayStoreBuild.BuildFromCommandLine
  ```
  İsteğe bağlı `FR_VERSION_CODE` / `FR_VERSION_NAME` ile sürüm sabitlenebilir.

Build'in yaptıkları: AAB formatı, release (development kapalı), IL2CPP + yalnız ARM64, targetSdk 36, native sembol tablosu paketin içinde (Play Console'daki "debug sembolü yok" uyarısı çıkmaz), keystore şifreleri build bittikten sonra ProjectSettings'ten silinir.

Unity Hub'da Android SDK'da **API 36 platformu** kurulu olmalı. Build "platform 36 bulunamadı" derse: Unity Hub > Installs > 6000.3.26f1 > Add modules > Android SDK & NDK Tools'u yeniden kur ya da `Preferences > External Tools`'daki SDK'ya `sdkmanager "platforms;android-36"` ile ekle.

## 3. Play Console

1. **Uygulama oluştur** → paket adı `com.fartrafters.game` (yüklendikten sonra değiştirilemez).
2. **Play App Signing** açık kalsın (varsayılan). Google asıl imza anahtarını tutar, senin anahtarın "upload key" olur.
3. İlk AAB'yi **Test > Dahili test**'e yükle, birkaç cihazda dene.
4. Kişisel geliştirici hesabıysa üretime çıkmadan önce **kapalı test** şartı var (en az 12 test kullanıcısı, 14 gün kesintisiz).
5. **Uygulama içeriği** formları:
   - *Gizlilik politikası URL'si*: `Docs/privacy-policy.html` herkese açık bir adreste olmalı (ör. GitHub Pages).
   - *Veri güvenliği*: Unity Gaming Services ile anonim oyuncu kimliği, oyuncu adı, skorlar/lider tablosu, arkadaş listesi işleniyor. Gizlilik politikasıyla tutarlı doldur; aktarımda şifreleme (HTTPS) var.
   - *Hedef kitle*: 13 yaş altını seçersen Aileler politikası devreye girer (daha sıkı reklam/veri kuralları). Online sohbet/isim alanı olduğu için buna dikkat.
   - *İçerik derecelendirmesi* (IARC anketi), *Reklam içeriyor mu*: hayır.
6. Mağaza girişi: 512×512 ikon, 1024×500 öne çıkan görsel, en az 2 ekran görüntüsü (yatay oyun için 16:9 uygun).

## Android 16'da test edilecekler

- Tablet / katlanabilir (600dp+) ekranlarda Android 16 yön kilidini yok sayabiliyor; oyun dikey açılırsa düzen bozulmamalı.
- Ekran kenardan kenara (edge-to-edge) çiziliyor; HUD'un çentik/gezinme çubuğu altında kalmadığını kontrol et.
- Titreşim (Haptics) ve geri tuşu davranışı.
