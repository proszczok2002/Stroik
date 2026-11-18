# Android device verification

## 1.10 / 110 — Pixel 11, 2026-11-18

- Pixel 11 connected over Wi-Fi ADB. BackTalk, PixelSpeechEQ and AutoSpeaker stayed enabled. The display was unlocked for interface and installation checks.
- The production 1.09 updater fetched the public 1.10 manifest and APK. The app offered the update with the Polish release notes. The visible Android update confirmation and Play Protect scan completed, followed by the visible install action. The upgrade from 109 to 110 preserved the signer, existing settings and an app-data test marker. The installed APK hash matched the public file, `b96f51029194cc14e2a0f5a1bec18d7cf5ce30ff6906420d054d576e1fda7617` (152747 bytes).
- `mapping110.txt` passed **55 assertions**. Mapping exposed the 1–10 strikes, nine octaves and 88 notes, and opened and released the actual microphone without playing a reference. Injected PCM measurements of 421 and 433 Hz averaged to approximately 427 Hz under the selected A4. Two synthetic taps advanced the completed mapping. Shared mapping/calibration JSON was read through the Android model and preserved the standard fallback for unmapped notes.
- `speaker-taps110.txt` passed **5 assertions** against the production capture loop with synthetic PCM and mock speech completion. Taps before an in-tune result did not advance. An in-tune result left its reference quiet, prompted for the gesture, and advanced once after two short unpitched taps. One tap alone did not advance.
- `hybrid109.txt` repeated the actual Classic microphone/reference and Beginner TTS sequencing on 1.10. Measured reference resumption after result speech was 51/1093/2065 ms for pause settings 0/1/2. Five detuned injected PCM readings each produced a result without advancing. No physical notes were played.
- `languages.txt` passed **2370 assertions** covering all six languages, controls for all four tuning methods, settings, labels and the installed guide assets. These instrumented checks cover interface structure and application speech timing. Human listening, natural gestures, physical headphones and instruments, and other phone models remain untested for 1.10.

Local evidence is in `android/device-results/pixel11-1.10-20261118`.

## 1.09 / 109 — Pixel 11, 2026-11-17

- Pixel 11, Android 17; connected by discovered Wi-Fi ADB endpoint. Google TalkBack, AutoSpeaker and PixelSpeechEQ remained enabled. The screen was awake during UI tests.
- The initial 108 → 109 installation used the visible Android update confirmation and Play Protect scan. Further prepublication 109 candidates used the same system installer. Each installation verified the existing signer, all model settings and an app-data marker. Production APKs were not installed with `adb install -r`; that command was used only for the test harness.
- Final signed APK: 136364 bytes, SHA-256 `7d2606b4aecb026b6adc6382c87c7e700f6e345d8d03b059739c45ae856cecdf`. The installed final APK was pulled back and matched this hash. This is local system installation evidence; a higher-version installation through the public updater is not claimed for this release.
- `hybrid109-final-artifact.txt`: actual AudioRecord microphone capture and immediate Classic reference without an app tutorial; Beginner app TTS guidance completed before reference playback. Results paused the reference until the application's own utterance completion. Measured additional time to playback for 0/1/2 seconds: **98/1108/2114 ms**. Stopping during speech did not restart capture or playback.
- The same scenario injected five detuned PCM notes with hybrid tap navigation enabled: all five results arrived and no navigation occurred. Speech completion was mocked only in this synthetic PCM subtest; the preceding timing checks used actual Android TTS and playback.
- `languages-final.txt`: **2208 assertions PASS** across PL/EN/DE/RU/FR/ES, all four methods, profile/language round trips, installed guide/change assets, classic labels without key positions, and hybrid pause range 0–60. Accessibility adjustments reached 2 → 1 → 0 and retained the existing slider view. This caught and corrected an initial fractional-step error before publication.
- `focus.txt`: **177 assertions PASS** for actual accessibility focus and view retention. `state-events.txt`: **14 assertions PASS** for microphone state, concise next-note events and muted visual readings. These two runs preceded the final documentation-only APK rebuild; executable Java code was identical.
- The first tutorial timing test incorrectly observed the shared TTS engine's `isSpeaking` state before the app's delayed tutorial had started. Diagnostic output showed the app tutorial still speaking. The corrected test tracks the app's own utterance ID through its completion callback; retain the initial failed logs as test-harness evidence, not as an app regression.
- JVM core tests passed, including bass upper-partial checks around G-sharp/A/A-sharp in the contra octave, black-key navigation and pause validation. Windows regression tests separately cover matching PCM synthesis, 245 unclipped note/interval combinations, and keyboard editing down to zero. A visible Windows probe opened the actual microphone; NVDA/SAPI result completion preceded the reference, with 0/1/2 second pauses. The existing NVDA beginner-guidance probe also passed.
- No instrument was played. These checks do not establish human pronunciation/listening, subjective bass loudness on the owner's speakers, all natural TalkBack/NVDA gestures, physical headphone monitoring, or Moto/Xiaomi behavior. Older evidence below remains bounded to its original versions.

Local evidence: `android/device-results/pixel11-1.09-20261117` and `android/device-results/install-20261117-*`. Superseded APKs and verification copies are preserved under the project-root `niepotrzebne` archive.

## Original 1.0 through 1.03 checks

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

## 1.04 candidate — 12 November 2026, phone unavailable

The new report concerns speech/visual settings, screen-off listening, duplicated microphone actions and labels, list values, focus, consonance visibility, reference-frequency control and bottom tabs. These are implemented in 1.04 source, including a user-started foreground microphone/mediaPlayback service, partial wake lock, stop notification and system TTS with the screen off. The service does not start on reboot, uses START_NOT_STICKY and stops when removed from Recents. Three tabs share one horizontal parent; Instrument is a dropdown on Tuner. The slider exposes Hz range/progress with validation, 0.1 Hz accessibility adjustments and double-tap/Enter numeric entry.

Updated JVM DSP/note checks passed. Android source/APK and the separate device test APK compile. New interaction instrumentation is prepared for selected dropdown values, one microphone button, Hz actions/bounds, keep-screen flags, Alt+N, three tabs and persistent consonance. Focus/lifecycle tests were adapted to three pages. None of these updated device scenarios has been run for 1.04. The 1.03 totals are not 1.04 results.

Device acceptance still required: visible-start microphone and permission dialogs; one button and TalkBack on/off states; selected list values and skipped labels; touch/TalkBack slider and numeric entry; focus across instrument/method/consonance changes; screen off/on and TTS with visual voice on/off; notification/Recents stop releasing recording and wake lock; Activity recreation during capture; hardware keys; service errors; real audio interference/levels; system update persistence. Keep TalkBack enabled and check POCO battery/vendor restrictions.

The candidate is a separate prerelease; android/update.json remains on stable 1.03 pending device testing. The earlier Play Protect classification is unresolved. Polski: mikrofon w tle, TalkBack, suwak, gesty i wygaszony ekran w 1.04 wymagają próby na telefonie; skompilowanie kodu nie jest próbą sprzętową.

### Published 1.04 candidate identity

Source/tag: android-v1.04 at 6e0df7495ff9f8b5b3a2a67af04924100aa732f4. Public APK: 74395 bytes, SHA-256 AF7075CBEC89B2172E46C026465E3C0C8C7AACCE01166A441D2F0245885DA82B. Its checksum and APK were downloaded anonymously and matched local output. The signing certificate remains 3a07196ac981d8a1d5dda9f2fb140e1c47226128edc25866e2c37a8f65ab1522. Android source CI 43246043983 and tag CI 43246771342 passed. These runs compile/test core code, not the unavailable physical phone. The release is marked prerelease and does not change the Android automatic manifest (1.03/103). GitHub latest is the separately verified Windows v1.0.15.

## 1.05 / 105 — Pixel 11, 14 November 2026, validation in progress

This continues the untested 1.04 candidate and brings Windows 1.16/1.17 note/fork fixes to Android. The actual connected device identifies as Pixel 11 (cubs), Android 17 / API 37. Google TalkBack remains enabled throughout instrumentation; the owner's other accessibility services were not changed.

The current installed candidate is **78492 bytes**, SHA-256 **80CED060066A4F52FE4A33F1CCA72A141D2E4F6EA8988CD2A50C013AACA99FFF**. Its production certificate is unchanged. A fresh first installation used ADB because this phone had no Stroik package. The later repaired candidate used the actual Android package-install confirmation. Play Protect requested a scan and then displayed “Ta aplikacja wygląda bezpiecznie” and “Możesz ją zainstalować”; installation continued through its Install button. No protection block was bypassed. The APK pulled from the installed package matches the local candidate hash; all model settings, the signing certificate and a saved data marker were retained. This was a local same-version candidate installation, not an end-to-end GitHub updater test or evidence for all future Play Protect decisions.

A real focus regression reproduced lost checkbox focus after theme-triggered Activity recreation. The repaired theme changes colors/tints in place instead of replacing controls. Passed on the installed repaired candidate:

- 183 PL/EN focus assertions, including method, consonance, targets/octaves, tabs, instrument, ukulele tuning, language and both in-place theme changes.
- 983 lifecycle assertions across repeated three-page recreation and both themes.
- 2185 interface assertions covering labels, control geometry and state; 6 accessibility-focus events and 14 click events are separate counts.
- 6 screen-off assertions: actual device sleep, foreground AudioRecord still recording, system TTS starting with graphics on and visual speech off, and recording release on Stop. TTS.isSpeaking does not establish human pronunciation or speaker audibility.

The first 1.05 candidate separately passed 60 fork assertions (actual AudioTrack replacement after real UI settings, synthesis-component checks and white-key dispatch), 48 interaction assertions and 7 audio assertions. These scenarios still need to be rerun on the repaired candidate. A synthetic keyboard test initially failed because it did not establish input focus while Android was in touch mode and left old engine configuration before its second language; the setup was fixed without ignoring assertions. Production keyboard behavior still uses normal focus. One UI-bridge test process crashed when a second UiAutomation connection was attempted concurrently; this was not a production Stroik crash, and subsequent checks run sequentially.

After the successful screen-off test the phone is locked. An interaction attempt while keyguard was showing reported “Consonance disappeared”; it does not establish a failure in the unlocked app. The owner was asked to unlock the phone. Remaining: rerun fork/interaction/audio, actual TalkBack gestures, final screenshots and crash-buffer review. Human speech listening, real instrument accuracy and headphone monitoring remain separate. No 1.05 release or updater-manifest publication has happened yet.

Polski: bieżąca 1.05 jest zainstalowana przez systemowe potwierdzenie, podpis/ustawienia/hash zgodne. Naprawiono odtworzoną stratę fokusu przy zmianie motywu. Na poprawionej kompilacji przeszły testy fokusu, odtwarzania stron, struktury dostępności i rzeczywistego nasłuchu/TTS przy wygaszonym ekranie. Po tej ostatniej próbie telefon jest zablokowany; dalsze testy wymagają odblokowania. Nie jest to jeszcze zakończone wydanie ani test pełnej aktualizacji z GitHuba.

## Final 1.05 acceptance — 14 November 2026

This section supersedes the earlier in-progress candidate status. The final APK is **78492 bytes**, SHA-256 **EB2ACC14C2DE5CBF9878E885159120DB0CDA92AAC1A22EDB42956832A5093D62**, version 1.05 / 105. The installed APK was pulled and compared byte-for-byte by SHA-256. Its production certificate remains **3a07196ac981d8a1d5dda9f2fb140e1c47226128edc25866e2c37a8f65ab1522**. Earlier 80CED, 45A1, FE47 and F58F hashes identify intermediate local builds, not this release.

The final local candidate passed the actual Android Update confirmation and Play Protect scan. The scan displayed “Ta aplikacja wygląda bezpiecznie”; installation continued through its Install action. No block was overridden. Settings, the certificate and the pre-install marker were preserved. Initial ADB installation on an empty phone and later system-confirmed candidate replacements are distinct; production candidates were not updated with adb install -r. The instrumentation package was updated with ADB as a test harness.

All of these were rerun successfully on the exact final APK with Google TalkBack enabled on Pixel 11 / Android 17 (API 37):

| Scenario | Assertions | Evidence |
| --- | ---: | --- |
| PL/EN focus | 183 | Method, interval, targets/octaves, tabs, instruments, language and both in-place theme changes |
| Lifecycle | 983 | Repeated three-page recreation and both themes |
| Interface | 2185 | Accessible labels, native roles, states and control geometry; 4 focus events and 14 click events are separate counts |
| Interaction | 48 | One microphone button, collapsed selected values, slider Hz actions/bounds, awake flag, three bottom tabs and Alt+N |
| State/voice events | 14 | Immediate microphone-off announcement, muted automatic visual readings, concise explicit Next-note announcements in PL/EN |
| Fork | 60 | Real AudioTrack replacement after UI selection, expected synthesis components, keyboard white keys and upper-range pause |
| Audio devices | 7 | AudioRecord/AudioTrack start/stop and alternating-speaker reference guard |
| Screen off | 6 | Actual sleep, recording maintained, TTS started despite muted visual feedback, microphone released on Stop |

A further focus regression occurred when making the microphone section visible shifted the focused checkbox outside the viewport. The fix captures focus before changing visibility, then restores that still-attached visible control once after layout. It does not repeatedly force focus. A stronger gesture experiment exposed unreliable remote-swipe evidence: earlier six events alternated only between two stops; another attempt hit the post-install dialog. A one-minute requested natural-gesture window received no user interaction. **Natural user gestures and human TalkBack pronunciation/listening are not claimed as passed.** This limitation is separate from the passed actual-focus and accessibility-event checks.

Final screenshots were inspected for one-row bottom navigation, readable light/dark text, selected-tab contrast, system-bar contrast, visible Hz value, the checked theme icon, popup guidance for visual speech and the meter/virtual keyboard. A stale theme checkbox animation was replaced with the current theme's native drawable without replacing its View; spinner arrows are tinted for contrast. The collapsed spinner exposes one labelled accessibility stop while popup rows retain native accessibility.

No headphones were connected. Real instrument/noise accuracy, headphone monitoring and a complete GitHub download-to-higher-version system update remain separate from the passing local installer and audio tests. Logs/screenshots are retained locally under the ignored device-results folder. The stable Android manifest will be updated only after public APK/hash verification; Windows latest stays separate.

Polski: końcowy APK EB2ACC... przeszedł całą powyższą serię na Pixelu 11 z włączonym TalkBackiem, z zachowaniem danych po systemowej instalacji. Naprawiono fokus przy motywie i zmianie wysokości sekcji, podwójne przystanki list, odświeżanie kwint/oktaw, zakres kamertonu oraz kontrast i widoczną wartość Hz. Naturalne gesty/odsłuch człowieka, prawdziwy instrument, słuchawki i pełna aktualizacja GitHub do wyższej wersji nie są uznane za przetestowane.


## 1.06 / 106 — Pixel 11, 15 November 2026

The final local production APK is **82589 bytes**, SHA-256 **D25A0BAE04BA84F4B1A32A004B9889D88DC2E55AA5EC4379289B79DA56559968**. Device: Pixel 11 (cubs), Android 17 / API 37, connected through wireless debugging. Google TalkBack and the owner's other accessibility services stayed enabled. The installed package was pulled and its hash matched this file. The production certificate remains **3a07196ac981d8a1d5dda9f2fb140e1c47226128edc25866e2c37a8f65ab1522**.

The earlier 1.05 installation was upgraded to a 1.06 candidate using Android's visible confirmation; subsequent local candidates also used the system installer. Play Protect scan reported the app safe before Install was selected. No security block was bypassed. Settings, certificate and saved data marker were retained; profile comparison accounts for the new default speechRate field through the same model migration used by production. Only the separate test harness was replaced using ADB. This is a system-confirmed local version upgrade, not a complete GitHub-updater round trip.

Passed on this exact APK:

| Scenario | Assertions | Scope |
| --- | ---: | --- |
| Requested changes PL/EN | 174 | Numeric ranges, note positions, top graphics, hidden fork microphone control, MediaController and system AudioManager Play/Pause for all four methods; finished manual tone becomes Paused and can be repeated; end-of-plan stops all methods with advance on/off |
| Numeric dialogs / traversal PL/EN | 3172 | Every visible numeric slider opens an editable dialog, rejects invalid input and saves a valid number; every explicit traversal chain reaches the heading without cycles or hidden references |
| Focus PL/EN | 177 | Actual accessibility focus retained across method/interval/instrument/target/tab/theme changes |
| Lifecycle | 863 | Repeated page recreation, preserved model and both themes |
| Interaction | 48 | Native control semantics, collapsed values, reference range, microphone toggle, tabs and shortcuts |
| State/voice events | 14 | Immediate microphone-off feedback and explicit Next note with muted automatic visual readings |
| Fork | 60 | Actual AudioTrack replacement after UI settings, paired synthesis, arrow dispatch and range pause |
| Audio | 7 | AudioRecord/AudioTrack start/stop and speaker-reference capture guard |
| Interface | 1869 | Labels, geometry, native states and 4 focus / 14 click events (event counts are separate) |

A slow real system-TTS utterance with speed 0.5 took approximately 7.5 seconds in the timing scenario. A deliberately overdue repeat deadline did not trigger the fork during speech; playback resumed after the actual completion callback. Screen-off evidence is recorded below.

One earlier test run crashed in Android ViewGroup.buildOrderedChildList while accessibility nodes were being built simultaneously by the instrumentation thread and the UI/TalkBack path. The test harness now builds all direct nodes on the UI thread. Requests then passed twice, sliders and interaction passed again, without suppressing services. This failed run is retained in local logs; it is not counted as a passing run or proof of general device reliability.

Light/dark screenshots were inspected: the entire semicircular gauge and marked keyboard sit above the scroll container, bottom tabs remain on one row, contrast and selected-note marking are readable. Graphics are skipped by accessibility traversal. No replacement logo was created.

**Limits:** the remote swipe experiment did not produce usable natural TalkBack traversal (only heading focus), so it failed and is not counted as proof that the reported Motorola/Xiaomi loop is resolved. Explicit ordering, focus retention and dialog actions passed independently. Natural two-finger double tap, every right/left flick, human pronunciation/listening, Motorola G72/Xiaomi behavior, real instrument accuracy and physical headphones remain user/device checks. Standard media events passed; that is distinct from the natural TalkBack gesture. No other media player was used in the media-routing test. When the app is closed, starting microphone capture still requires opening it under Android microphone restrictions.

PL: Na dokładnym APK przeszły powyższe próby suwaków/dialogów, opisów nut, fokusu, cyklu życia, zdarzeń, sterowania mediami i audio. Systemowa instalacja zachowała ustawienia oraz podpis. Grafika jest widoczna bez przewijania. Naturalne gesty i odsłuch człowieka oraz zgłoszona pętla na Moto/Xiaomi wymagają osobnego potwierdzenia; nie zastępujemy tego testem struktury. Pełny updater GitHub do wyższej wersji pozostaje odrębny od lokalnego potwierdzenia instalacji i pobierania publicznego APK.


Screen-off final check: **65 assertions passed** on the same APK. The phone was physically put to sleep in alternating-speaker hybrid mode with graphics enabled, visual speech muted and application speech rate 0.5. Foreground AudioRecord kept recording; system TTS started; a forced overdue fork deadline did not overlap the utterance; the fork resumed after completion; Stop released capture/service. The phone returned to the keyguard afterwards. This observes TTS/AudioTrack state, not human audibility or pronunciation.

Polski: próba przy fizycznie wygaszonym ekranie zakończona — 65 asercji PASS, mikrofon działał, wolny TTS dokończył wypowiedź przed kamertonem, następnie odtwarzanie wróciło, a Stop zwolnił zasoby. Telefon po próbie jest zablokowany.


## Final 1.07 / 107 — Pixel 11, 16 November 2026

Final production APK: **86684 bytes**, SHA-256 **C7168767121EF328740AC93B625F1DEC0C0A1954AFBC60E6AE81349CADCDE2DD**. The unchanged signer is **3a07196ac981d8a1d5dda9f2fb140e1c47226128edc25866e2c37a8f65ab1522**. System Update confirmation and Play Protect safe scan were used for candidate replacement; the installed APK was pulled and matched by hash. Settings, signing certificate and the saved data marker were retained. Only the instrumentation package used adb install -r. The first local update moved 1.06 to 1.07; later candidates kept 1.07. This is not a complete GitHub updater round trip.

Device: Pixel 11 (cubs), Android17/API37, wireless debugging. Google TalkBack, AutoSpeaker and PixelSpeechEQ stayed enabled. All final release-* scenarios below ran sequentially on the exact APK; the microphone and service were stopped after tests.

| Scenario | Assertions | What passed |
| --- | ---: | --- |
| Regression 1.07 | 514 | PL/EN Classic and six instruments, no duplicate view setting, hybrid controls, reverse shortcuts; five consecutive detuned top-note hybrid capture results without any advance |
| Previous feature checks | 172 | Sliders/ranges, top graphics, note positions, media start/stop, manual playback completion and end-of-plan stop |
| Numeric dialogs / traversal | 2608 | Each numeric slider opens a dialog, rejects invalid input, saves a changed value including 440→435.5 entered with a comma; explicit traversal chains are acyclic and reach the heading |
| Focus | 177 | Actual PL/EN accessibility focus retained across method/interval/instrument/note/tab/language/theme changes |
| Lifecycle | 845 | Page recreation, model preservation and both themes |
| Interaction | 48 | Slider/toggle/list/tab semantics and shortcuts |
| State/voice events | 14 | Immediate mic-off and explicit Next-note feedback |
| Fork | 60 | UI selection changes replace real AudioTrack, expected pair components, keyboard dispatch and range pause |
| Audio | 7 | Real AudioRecord/AudioTrack start/stop and speaker-reference guard |
| Interface | 1746 | Accessible labels/geometry/native states; 6 focus and 12 click events are separate counts |
| Screen off | 64 | Actual screen sleep, foreground recording, slow TTS despite visual-speech mute, no overlapping fork, post-speech resumption and resource release |

The timing scenario additionally completed **three consecutive real slow-TTS / resumed-fork cycles** at speech rate0.5, with an intentionally overdue repeat deadline. It did not hang after the second cycle. Hybrid sample injection uses the same AudioEngine.process path as live AudioRecord; it is not a substitute for playing a physical instrument. Strict confirmed-strike logic was checked with correct and detuned attacks, including fresh-window waiting. Separate JVM/core tests verify both expected fifth components across C2/C3/C4/C5/C6 and reject half-frequency components. This establishes generated sample frequencies, not the perceived pitch on every speaker.

A candidate focus run failed during language replacement. The app now restores corresponding focus once after layout settles, only if focus is empty/on the heading; it does not steal focus from another user-selected control. The full final focus test passed after this fix. An early high-C8 test reproduced missed sharp readings because the old detector ceiling rejected them; the analysis range now reaches4600Hz. Failed candidate results are retained locally and are not counted as final passing evidence. No new production FATAL exception was found in the inspected 16November crash buffer.

Screenshots were inspected for red/green target keys in both themes, readable native dropdowns, the top gauge/keyboard and visible numeric IME with the selected440.0 value, Save and Cancel. An initial visual capture contained the post-install confirmation dialog; it was repeated after dismissal and the unobstructed screenshots were inspected. The screenshot test calls reading with controlled deviations while capture is active; it does not prove microphone accuracy. Tests/settings are restored afterwards.

**Remaining manual/device checks:** human NVDA/TalkBack pronunciation and notification audibility, natural flick/MagicTap gestures, the previously reported Motorola/Xiaomi loop, real instruments/noisy rooms, headphones and the reporter's perception of fifth octaves/bass response. No headphones were connected. A phone already on107 cannot demonstrate a higher-version public update to107; public download/certificate verification and local system installation are separate stages.

Polski: końcowy APK przeszedł wszystkie powyższe próby na Pixelu z włączonym TalkBackiem. Classic bez porad, poprawione sterowanie hybrydą, pięć rozstrojonych pomiarów bez przejścia dalej, trzy cykle wolnej mowy, rzeczywisty zapis nowej liczby, fokus, grafika i wygaszony ekran — PASS. Odsłuch człowieka, gesty, Moto/Xiaomi, prawdziwy instrument i pełny updater do kolejnego numeru wersji nadal wymagają osobnego potwierdzenia.


## 1.08 / 108 — localization, 16 November 2026

Local JVM checks passed for all six locales, note/position names, speech locale selection, language validation and the existing DSP/gesture regressions. Shared catalog checks cover 306 templates per added language, including format placeholders. Signed production APK and device-test harness compiled successfully. A new `languages` instrumentation scenario covers six-language controls, selected notes, JSON round trips and localized guide/change assets without suppressing accessibility services.

At the initial publication checkpoint no ADB device was connected: mDNS found none and the previous endpoint refused the connection. The languages scenario, system update 107→108 and TalkBack interactions were pending at that point. The follow-up below records later device results. Earlier 1.07 results above remain historical, not evidence for 1.08. No human pronunciation, natural gestures, instrument, Moto or Xiaomi checks are claimed. The owner was asked for the current wireless-debugging endpoint.


## 1.08 follow-up — Pixel 11, Android 17 / API 37, 16 November 2026

The owner re-enabled the Pixel connection. TalkBack, AutoSpeaker and PixelSpeechEQ remained enabled. The installed baseline was production **1.07 / 107**.

A real public GitHub update completed **107 → 108**: the old application displayed the new-version dialog and Polish changes, downloaded the public APK, opened Android’s Update confirmation and went through the standard Play Protect scan. Only the instrumentation package used `adb install -r`; production Stroik was updated by Android’s installer. The installed APK was pulled: **132268 bytes**, SHA-256 **028EA31F97E4F2C26EB242C0CD7935E0EE925DB1E6435E5C936C5DF9BB7EA78A**, identical to the published APK. The persistence scenario confirmed version108, unchanged signing certificate, all model settings and the saved app-data marker. The original production fetch had also independently verified the public manifest, package, version, signer and hash before installation.

| Check on installed 1.08 | Result | Scope |
| --- | --- | --- |
| Six-language scenario | 2154 assertions PASS | PL/EN/DE/RU/FR/ES, four methods, controls, selected note/language, JSON language round trip, TTS locale selection, guides and change assets |
| Focus | 177 assertions PASS | PL/EN actual accessibility focus retained after method/interval/target/octave/tab/instrument/theme/language changes |
| State events | 14 assertions PASS | Immediate microphone-off announcement and concise next-note message, muted automatic visual-mode readings |
| Injected swipe experiment | FAIL, not counted | Reached only the heading, as with the earlier experiment; this does not establish natural TalkBack traversal or diagnose an app loop |
| Owner gesture observation | 86 focus events, PASS | No remote swipes were injected in this one-minute window; observed movement across lists, sliders, buttons and dialogs in the Polish beginner screen |
| Additional interface sweep | INVALID, not counted | Phone was locked/Dozing: zero visible controls and focus events. The sweep was not treated as an app regression; lifecycle/sliders repetitions were not started |

Evidence: ignored `android/device-results/pixel11-1.08-20261116/`. Real-instrument accuracy, human pronunciation/listening, physical headphones, Moto/Xiaomi and full natural gesture coverage remain separate checks.

The owner’s one-minute gesture observation is recorded in `natural-gestures-window.txt`. It establishes those observed focus changes, not exhaustive gesture coverage or human pronunciation quality. A subsequent broad interface sweep ran after the screen locked and returned zero visible controls, so its failure was classified as an invalid locked-screen test; it does not supersede the valid six-language, focus and state-event checks. No additional unlock was requested just to repeat unchanged legacy checks.
