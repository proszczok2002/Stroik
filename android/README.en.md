# Stroik Android 1.01

Native Android 11+ tuner for blind, partially sighted and sighted musicians, using standard controls and Polish/English UI. [Polski](README.md).

[Download APK 1.01](https://github.com/proszczok2002/Stroik/releases/download/android-v1.01/Stroik-Android-1.01.apk) · [Release and SHA-256](https://github.com/proszczok2002/Stroik/releases/tag/android-v1.01).

**Update test status:** Play Protect blocked installation of 1.01 after scanning on the test POCO, without a specific cause. Detection, download and opening the system installer work; a complete update and retained settings remain unverified. See the [device report](DEVICE_TESTS.md).

See [in-app guide](app/src/main/assets/guide.en.txt). Features: fork, microphone tuner, hybrids, instruments/octaves, fifth/octave checks, JSON profiles, headphone monitoring, manual correction, graphics, support and credits.

Updates use this repository's `android/update.json`. Android releases use `android-v1.0` and `android-v1.01` tags and are not marked as the latest Windows release. Installation requires Android's system confirmation. Package `pl.proszczok.stroik`, increasing versionCode and an unchanged signing certificate are required. Do not uninstall before updating if you want to retain settings.

Build with Windows, JDK 17+ (locally JDK 25), Android SDK Platform 36 and Build Tools 36.0.0. No external libraries. Run `android/build.ps1 -Sdk <sdk> -JavaBin <jdk-bin>`. The private signing key and password are created in ignored `android/.signing`; preserve a secure backup for future updates. `-Ci` uses a separate throwaway signing key. Outputs go to `android/releases`; old release files are archived under the project-root `niepotrzebne` folder.

Device evidence and test limits are recorded in [DEVICE_TESTS.md](DEVICE_TESTS.md); the local work log is the root `STAN_PRAC.md`. Structural checks do not replace actual TalkBack speech and gesture testing.
