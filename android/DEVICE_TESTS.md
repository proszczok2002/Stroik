# Android device verification — 1.0, 1.01 and 1.02

Device: POCO F8 Ultra, model 25102PCBEG, Android 16 / API 36. During the original 1.0 checks, the TalkBack-derived service from com.nirenr.talkman remained enabled. The initial USB installation restriction was resolved by the owner; ADB installation and launch then succeeded. The final 1.02 service and results are recorded separately below.

## Passed on 1.0

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

The stability fix is published as `android-v1.02`, source `b53495f4efaed379ccf1c3135c7e1c5bafc04838`. Its APK and checksum were downloaded anonymously and matched local size/hash; the public APK signature matches the production certificate. Source CI passed: Android 42847113032 and Windows 42847113044. The installed phone APK matches the same public artifact; Windows latest remains `v1.0.14`.

Signing certificate SHA-256: 3a07196ac981d8a1d5dda9f2fb140e1c47226128edc25866e2c37a8f65ab1522. Private signing files remain local under ignored .signing.

## Update test

Installed 1.0 detected public 1.01 and displayed its Polish release notes and download confirmation. Before testing, the complete model settings, signing certificate and an app-data marker were saved as a baseline. An initial stale manifest response was observed from the app's Java HTTP connection; after propagation it detected 101 without replacing the installed app. The `manifest` test scenario can inspect this response from the actual updater code.

The owner manually enabled Android's app-specific unknown-source permission for Stroik. The installed 1.0 downloaded and verified 1.01, then opened the actual Android package installer with the question “Zaktualizować tę aplikację?” and an “Aktualizuj” button. This confirmation was exercised through the system UI, without an ADB replacement install.

Google Play Protect then requested a scan. After scanning, it displayed “Zablokowano szkodliwą aplikację” / “Ta aplikacja może być niebezpieczna.” The expanded details supplied no specific cause. The installed package remained 1.0 / 100. **A complete update and post-update persistence verification have not yet passed.** The owner was asked whether to retain the block or explicitly allow a one-time installation despite that warning; no override was performed while waiting.

The local production APK still has the published SHA-256 and original certificate. A basic source/package inspection found only the declared microphone, Internet and installation permissions; the APK contains the manifest, resources, four text assets, compiled application classes and signature files, with no bundled native libraries or other APKs. This inspection does not establish why Play Protect classified it or prove that its warning is false. Google's [developer guidance](https://developers.google.com/android/play-protect/warning-dev-guidance) describes classification review and appeal; no appeal has been submitted.

Source 1.01 CI passed: Android 42840776407, Windows 42840776445. Tag CI passed: 42841104370 / 42841104717. Manifest commit CI passed: 42841173397 / 42841173405.

Polski: wydania 1.0 i 1.01 są publiczne, pliki i podpis sprawdzone. Stroik 1.0 wykrył i pobrał 1.01, pokazał zmiany i systemowe potwierdzenie aktualizacji. Po skanowaniu Play Protect zablokował instalację jako potencjalnie niebezpieczną, bez podania konkretnej przyczyny. Na telefonie pozostaje 1.0; pełna aktualizacja i zachowanie ustawień nie są jeszcze potwierdzone. Testy struktury dostępności i uruchamiania audio przeszły, odsłuch TalkBack przez człowieka i testy z instrumentem pozostają osobnymi próbami.

## Crash report and 1.02 regression

The owner reported apparently random exits while touching controls. The actual crash buffer contained repeated `ClassCastException: CompoundButton$SavedState cannot be cast to Spinner$SavedState` in Android's restoration code. Version 1.0 deterministically reproduced this fatal error when recreating the Settings activity. The original screen matrix did not cover activity recreation and did not establish lifecycle stability.

Dynamic layouts reused sequential view IDs for different widget types and always recreated the Tuner page. Version 1.02 excludes that dynamic subtree from automatic hierarchy saving/restoration and explicitly preserves the page, scroll position, last result and fork pause state. Settings remain in the existing model/preferences. It does not merely catch and hide the exception.

Final 1.02 APK: 62108 bytes, SHA-256 **06a5de6a6c69ea0d2906558f402498f5f3c84a05c7fa908ff2a56afd5c279862**. Package/version 1.02 / 102 and unchanged production signing certificate were checked. The APK pulled back from the actual installed package matched this hash exactly. The owner completed system installation from a local APK after the Play Protect warning. The agent did not silently replace the production app through ADB or disable Play Protect.

Passed on that exact final APK:

- **1103 lifecycle assertions**: initial Settings recreation, legacy checkbox hierarchy replay into a different page, three cycles recreating all four pages in PL/EN, matching model settings/page, and both actual theme checkbox changes. These exercise Android activity recreation; they are not a claim that every physical rotation or arbitrary touch sequence was tested.
- **1831 interface/accessibility assertions**, with 5 focus and 6 click events; all four methods and interval choices, octave selection, PL/EN and dark theme.
- **7 capture/playback assertions**, including the speaker-hybrid handover. No headphones were connected.
- System installation retained the complete saved model, signing certificate and pre-install app-data marker. The final test log uses the label “System install verified”; it does not represent an end-to-end GitHub updater test for 1.02.
- No new entries appeared in the crash buffer during the final test sequence. A screenshot confirmed the two-row tab labels fit at the device's large font size.

TalkBack remained enabled. Earlier tests used the service from `com.nirenr.talkman`; final checks observed the official `com.google.android.marvin.talkback/.TalkBackService`. The agent did not change these services. Human speech/gesture listening remains unverified.

The completed installation from the local APK is separate from the earlier 1.0 → GitHub 1.01 detection/download/installer test, which Play Protect blocked. A complete GitHub updater round trip to 1.02 has not been exercised; do not report it as passed. The Play Protect classification cause is still unknown.

Polski: odtworzono rzeczywistą awarię 1.0 przy ponownym tworzeniu Ustawień. Końcowy APK 1.02 jest zainstalowany i zgodny z lokalną sumą; przeszedł 1103 asercje odtwarzania ekranów, 1831 interfejsu/dostępności oraz 7 audio, bez nowej awarii podczas tej serii. Ustawienia i dane zachowano po systemowej instalacji lokalnego APK. Nie jest to potwierdzenie pełnej aktualizacji 1.02 z GitHuba ani rozstrzygnięcie ostrzeżenia Play Protect.

## Remaining physical checks

Human TalkBack speech/gestures, real instrument accuracy in noise, headphone monitoring/latency, and complete 1.0 → 1.01 installation confirmation remain separate from the passed structural and audio-device checks.
