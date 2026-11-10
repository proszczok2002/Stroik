# Android device verification — 1.0

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

## Candidate identity

APK: 62110 bytes; SHA-256 97e9169b68ecb012a6f9df63488890df473854f23a1360ced6f7df31939ce843.

Signing certificate SHA-256: 3a07196ac981d8a1d5dda9f2fb140e1c47226128edc25866e2c37a8f65ab1522. Private signing files remain local under ignored .signing.

## Update test

Next: publish 1.0 assets and verified manifest, then sign 1.01 with the same key and increasing versionCode. Publish and verify its assets before updating the manifest. Exercise the installed 1.0 in-app updater and Android's actual installation confirmation. Check package version, certificate and retained settings after installation. Do not replace this test with silent ADB installation.

## Remaining physical checks

Human TalkBack speech/gestures, real instrument accuracy in noise, headphone monitoring/latency, and complete 1.0 → 1.01 installation confirmation remain separate from the passed structural and audio-device checks.
