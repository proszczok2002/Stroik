# Stroik Android 1.06

[Download APK 1.06](https://github.com/proszczok2002/Stroik/releases/download/android-v1.06/Stroik-Android-1.06.apk) · [GitHub Release](https://github.com/proszczok2002/Stroik/releases/tag/android-v1.06) · [PL](README.md) / [EN](README.en.md)

Fork mode has no microphone button or separate microphone-state navigation stop. Optional acoustic double-tap listening uses its own checkbox. Reaching the end of the selected order stops all capture and playback, including hybrids and with automatic advance disabled.

Every numeric setting uses a slider with its current value. Drag or use TalkBack adjustment gestures; double tap or Enter opens numeric keyboard entry. Out-of-range entries stay open for correction. Settings includes Stroik screen-off speech speed: 0.5–3, default 1. This does not change TalkBack speed. The alternating speaker fork waits for the synthesizer's actual utterance-completion callback instead of estimating speech duration.

Piano note options include positions relative to black-key groups. Next note also announces position in Beginner view; Classic stays concise. Accessibility traversal is ordered and the result field no longer takes editing focus. Natural flicks on Motorola and Xiaomi still need user confirmation.

TalkBack two-finger double tap uses standard media controls: microphone in Electronic, playback in Fork, both in Hybrid. Starting the microphone with the app closed requires opening it. Gesture routing depends on TalkBack settings and active media; system Play/Pause events were checked on Pixel, while the natural gesture needs a separate trial.

Enabled graphics sit above scrolling settings: a semicircular cents gauge and a keyboard marking the selected note. Graphics add no TalkBack stops. Guides and feedback are available in Polish and English.

In-app guide: [guide.en.txt](app/src/main/assets/guide.en.txt). [Device evidence / Raport urządzenia](DEVICE_TESTS.md).

## Build / Budowanie

Windows, JDK 17+, Android SDK Platform 36 / Build Tools 36.0.0. No external libraries / bez bibliotek zewnętrznych.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File android/build.ps1 -Sdk 'C:\Android\Sdk' -JavaBin 'C:\Java\bin'
```

PL: Prywatny klucz i hasło w ignorowanym android/.signing muszą być zachowane do kolejnych aktualizacji. Nie publikować. Opcja -Ci korzysta z osobnego klucza do testów. Bieżące pliki są w android/releases; stare buildy zachowuje główny folder niepotrzebne.

EN: Preserve the private key/password in ignored android/.signing for compatible updates; never publish them. -Ci uses a separate test key. Current files are in android/releases; old builds are archived in the project-root niepotrzebne folder.

PL: Aktualizacje korzystają z android/update.json. Weryfikowane są SHA-256, pakiet, wersja i podpis. Instalacja wymaga systemowego potwierdzenia Androida. Nie odinstalowuj aplikacji przed aktualizacją. Kanał Windows pozostaje oddzielny.

EN: Updates use android/update.json with SHA-256, package, version and certificate verification. Installation requires Android system confirmation. Do not uninstall before updating. Windows uses a separate release channel.
