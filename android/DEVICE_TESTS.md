# Android device test state

Device observed: POCO F8 Ultra, serial suffix `eff8`, model `25102PCBEG`, Android 16 / API 36. A TalkBack-derived service (`com.nirenr.talkman`) is enabled, alongside two other accessibility services.

## Passed locally

- Java SDK compilation, resource packaging, DEX compilation, ZIP alignment and release signing.
- APK signing certificate remains local under ignored `.signing`; release APK is not a debug-key build.
- JVM core suite: 22 pitch/interval cases at 44.1/48 kHz, piano extremes, silence, DC offset, recovery after unpitched noise, paired fork synthesis and double strikes.
- Test APK compilation. `DeviceTests` requests `FLAG_DONT_SUPPRESS_ACCESSIBILITY_SERVICES` to preserve TalkBack during checks.
- GitHub Actions passed for source commit `f7c3f42124560f1acab761af76b6699cd9209713`: [Android build/core tests](https://github.com/proszczok2002/Stroik/actions/runs/42830350350) and [Windows regression build](https://github.com/proszczok2002/Stroik/actions/runs/42830350428). The CI Android APK uses its own disposable key and must not be used to update the locally signed release candidate.

## Device gate

Initial installation was attempted using authorized ADB deployment. The phone returned `INSTALL_FAILED_USER_RESTRICTED: Install canceled by user`. The app was not installed or launched. The user has been asked to enable/confirm Install via USB on the phone. No installation restriction was bypassed.

The signed candidate was also copied to the phone's Downloads as `Stroik-Android-1.0-test-20261110.apk` for optional manual installation through the ordinary Android installer. Candidate: 62110 bytes, SHA-256 `97e9169b68ecb012a6f9df63488890df473854f23a1360ced6f7df31939ce843`. Signing certificate SHA-256: `3a07196ac981d8a1d5dda9f2fb140e1c47226128edc25866e2c37a8f65ab1522`.

## Pending before publication

1. Install and launch 1.0; inspect crash logs and all screens on the actual phone.
2. Run DeviceTests: PL/EN pages, all four methods, fifth/octave selectors, octave dropdown, checkbox state events, TalkBack accessibility focus, touch sizes and labels.
3. Human listening/gesture check with TalkBack across the app; structural tests cannot prove correct spoken navigation.
4. Confirm microphone permission and check capture/playback lifecycle, target changes, start/stop and leaving the app. Real instrument accuracy and headphone monitoring need physical audio equipment.
5. Publish Android 1.0 with public APK/checksum checks and a manifest pointing to it.
6. Build/sign 1.01 with the same key; publish its assets and manifest last. From installed 1.0, use the in-app updater, confirm Android's system installation, verify 1.01/package/signature and retained settings. Do not use silent ADB replacement for this updater test.

Current source/APK is a development candidate; neither requested Android release has been published yet.
