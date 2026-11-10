# Android device verification — 1.0 through 1.03

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

## Focus report and 1.03 regression

The owner reported focus jumping during use of 1.02. A device regression reproduced the cause: selecting Electronic tuner removed the actual focused Spinner and replaced the screen hierarchy. The test failed with “Method 1: focused control was replaced.” The earlier accessibility checks established labels/events, not focus retention after every action.

Version 1.03 keeps the root, tab buttons and ordinary option controls attached. Conditional sections change visibility in place. Selecting the already-current tab is a no-op. Language changes rebuild only page content and restore the corresponding control; theme recreation saves/restores the focused control once. Programmatic note/octave selection updates the companion list without replaying the user-selection callback. EditText fields use labelFor and hint rather than contentDescription, following [Android content-label guidance](https://support.google.com/accessibility/android/answer/7158690?hl=en).

Final APK: **66204 bytes**, SHA-256 **65e8e4ca39bc3cd74784176b31cc0cd5957d93859d3c95f98f1341c451acb51a**, version 1.03 / 103, unchanged production certificate. It was installed with Android system confirmation and the owner's manual interaction. The APK pulled from the installed package matched the local final artifact exactly. The app was not silently updated through ADB.

Published as `android-v1.03`, source `e946f78e49f3ead4c7b9e211afe9dfb6d4bd6a31`. Source CI passed: Android 42850753513 and Windows 42850753784. The public APK/checksum were downloaded anonymously and matched local size/SHA-256; the public signature matches the production certificate. Windows latest remains `v1.0.14`. The Android manifest is published after artifact verification.

Passed on that exact APK with TalkBack enabled:

- **198 focus assertions** in PL/EN: all four methods, consonance choices, tap-listening toggles, note/octave synchronization, next note across an octave boundary without resetting the order anchor, result announcement without moving focus, every tab (including selecting it again), instrument choices, ukulele tuning, language restoration, and checkbox focus after theme recreation. Tests verify the actual accessibility focus and control attachment/identity. The theme check waits beyond TalkBack's asynchronous initial-window announcement.
- **1103 lifecycle assertions**, **1831 interface assertions** (4 focus events and 14 click events), and **7 real capture/playback assertions** passed. These event counts are distinct from the 198 assertions.
- Settings, signing certificate and the pre-install data marker were retained. No new crash-buffer entries appeared during the final sequence.

The focus regression initially attempted to act before TalkBack finished its initial window announcement. That legitimately moved focus to the new window heading. The final test waits for initial-window settling before testing ordinary actions; it does not suppress TalkBack, filter out later focus failures, or retry until passing.

After automated testing, the app was opened for the owner's manual TalkBack check. Human speech/gesture confirmation remains pending unless separately reported. The earlier Play Protect classification is not resolved by these focus checks or by a separate Xiaomi scanner result. As with 1.02, local system installation is distinct from an end-to-end GitHub updater round trip to 1.03.

Polski: w 1.02 odtworzono usuwanie kontrolki z fokusem po zmianie metody. Końcowa 1.03 zachowuje kontrolki i przeszła 198 asercji faktycznego fokusu dostępności w PL/EN, 1103 cyklu ekranu, 1831 interfejsu i 7 audio. Zachowano ustawienia, brak nowej awarii w tej serii. Wymowa/gesty TalkBack przez człowieka, pełna aktualizacja przez GitHub i przyczyna wcześniejszego ostrzeżenia Play Protect pozostają oddzielnymi sprawami.

## Remaining physical checks

Human TalkBack speech/gestures, real instrument accuracy in noise, headphone monitoring/latency, and complete 1.0 → 1.01 installation confirmation remain separate from the passed structural and audio-device checks.
