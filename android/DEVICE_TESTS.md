# Android device verification — 1.0 and 1.01

Device: POCO F8 Ultra, model 25102PCBEG, Android 16 / API 36. The TalkBack-derived service from com.nirenr.talkman remained enabled throughout instrumentation. The initial USB installation restriction was resolved by the owner; ADB installation and launch then succeeded.

## Passed

- Signed production candidate 1.0 / versionCode 100 installed and launched on the actual phone.
- DeviceTests completed **1831 assertions**: PL/EN Tuner, Instrument, Settings and Help; all four tuning methods; fifth/octave choices; nine octave choices; dark rendering; input labels, focusability and minimum 48 dp touch targets. Controls are designed for 64 dp minimum height.
- Actual accessibility events: **5 accessibility focus changes and 6 click events**. Tests used FLAG_DONT_SUPPRESS_ACCESSIBILITY_SERVICES, preserving TalkBack.
- Device-side algorithm/profile/update-origin checks passed. JVM tests separately covered 22 pitch/interval cases at 44.1/48 kHz, lowest/highest piano notes, silence, DC, noise recovery, paired fork output and double attacks.
- With microphone permission granted through Android, **7 audio assertions** passed: AudioRecord was recording; AudioTrack fork playback was active; stop released recording; speaker hybrid suppressed its reference and reopened listening after the decay interval.
- No headphones were connected, so physical microphone monitoring was not exercised.
- GitHub CI source build/core checks passed before device testing. CI uses a disposable key; its APK must not replace production-signed APKs.

## Scope of accessibility evidence

The complete screen matrix was checked for accessible structure, geometry and focus/state events. This is not a human listening test of TalkBack pronunciation or every gesture. An additional remote swipe attempt was blocked by the phone with INJECT_EVENTS permission errors; the setting was not bypassed. The user has been asked to check spoken names and checkbox state directly.

## Published artifacts

APK: 62110 bytes; SHA-256 97e9169b68ecb012a6f9df63488890df473854f23a1360ced6f7df31939ce843.

Release 1.01 / 101: 62108 bytes; SHA-256 05932dc48a072ebbf8e9a5238591417c9716be06a7ba554b7b9858357a2f04dd. Two-row bottom navigation improves large-text labels; Polish octave dropdown grammar was corrected.

Both releases are published under `android-v1.0` and `android-v1.01`. Public APK/checksum files were downloaded and compared against local size/SHA-256. The 1.01 APK was also downloaded anonymously. Manifest 101 was published only after asset verification. Windows latest remains `v1.0.14`.

Signing certificate SHA-256: 3a07196ac981d8a1d5dda9f2fb140e1c47226128edc25866e2c37a8f65ab1522. Private signing files remain local under ignored .signing.

## Update test

Installed 1.0 detected public 1.01 and displayed its Polish release notes and download confirmation. Before testing, the complete model settings, signing certificate and an app-data marker were saved as a baseline. An initial stale manifest response was observed from the app's Java HTTP connection; after propagation it detected 101 without replacing the installed app. The `manifest` test scenario can inspect this response from the actual updater code.

Android then opened its app-specific unknown-source installation permission screen. The phone refused an automated accessibility click on the permission switch; no ADB app-op grant or security bypass was used. The owner has been asked to enable the permission manually and return to Stroik. **System installation and post-update persistence verification are still pending at this point.** Do not replace this test with silent ADB installation of 1.01.

Source 1.01 CI passed: Android 42840776407, Windows 42840776445. Tag CI passed: 42841104370 / 42841104717. Manifest commit CI passed: 42841173397 / 42841173405.

Polski: wydania 1.0 i 1.01 są publiczne, pliki i podpis sprawdzone. Stroik 1.0 wykrył 1.01 i pokazał zmiany. Telefon wymaga ręcznego włączenia „Zezwalaj z tego źródła” dla Stroika; instalacja systemowa i porównanie ustawień po aktualizacji oczekują na ten krok. Testy struktury dostępności i uruchamiania audio przeszły, odsłuch TalkBack przez człowieka i testy z instrumentem pozostają osobnymi próbami.

## Remaining physical checks

Human TalkBack speech/gestures, real instrument accuracy in noise, headphone monitoring/latency, and complete 1.0 → 1.01 installation confirmation remain separate from the passed structural and audio-device checks.
