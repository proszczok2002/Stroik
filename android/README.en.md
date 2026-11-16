# Stroik Android 1.08

Stroik Android 1.08

Added German, Russian, French and Spanish alongside Polish and English. Interface, spoken messages, notes, octaves, key positions, guide and latest changes are translated. Language persists in settings and JSON profiles; older PL/EN profiles remain compatible. App speech selects the chosen language; its Android voice must be installed. New releases can include notes in all six languages, with fallback for older manifests. Updates still require Android system confirmation.


[APK 1.08](https://github.com/proszczok2002/Stroik/releases/download/android-v1.08/Stroik-Android-1.08.apk) · [Release](https://github.com/proszczok2002/Stroik/releases/tag/android-v1.08) · [PL](README.md) / [EN](README.en.md)

Classic hides guidance, including the 440 Hz standard hint and piano advice for ukulele. Choose the view only on Tuner; the duplicate Settings checkbox is removed.

Hybrid microphone controls also start its reference. Manual strike, fork automation and interval controls appear only in Fork; hybrids keep their separate acoustic double-strike checkbox. In-tune confirmed advance belongs to Electronic. Both attacks require fresh pitch at the expected note within the chosen tolerance. Detuning cancels arming; a normal reading alone does not change target. A new reference starts a fresh capture epoch, rejecting stale pending results. The fork waits for its own TTS utterance to actually finish.

Shift+Alt+N and Shift+Ctrl+N select the previous planned note. Numeric entry opens the numeric keyboard and accepts either decimal separator; the slider and profile retain the entered value. Speech rate remains in Settings.

Stronger bass levels and a smooth tone ending reduce weak lows and end clicks. A fifth from C4 contains C4 and G4, 261.63 and 392.00 Hz at A=440; spectrum tests cover five octaves and reject half-frequency components. Small speakers still limit bass audibility. The target key changes from blue to red when detuned or green inside the selected tolerance. Color supplements text.

Pixel checks with TalkBack enabled are documented in DEVICE_TESTS.md. Structural, event and audio-state tests do not establish human pronunciation, natural gestures or real-instrument tuning on every phone.

[Device evidence / Raport urządzenia](DEVICE_TESTS.md).

## Build / Budowanie

Windows, JDK 17+, Android SDK Platform 36 / Build Tools 36.0.0. No external libraries / bez bibliotek zewnętrznych.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File android/build.ps1 -Sdk 'C:\Android\Sdk' -JavaBin 'C:\Java\bin'
```

PL: Prywatny klucz i hasło w ignorowanym android/.signing muszą być zachowane do kolejnych aktualizacji. Nie publikować. Opcja -Ci korzysta z osobnego klucza do testów. Bieżące pliki są w android/releases; stare buildy zachowuje główny folder niepotrzebne.

EN: Preserve the private key/password in ignored android/.signing for compatible updates; never publish them. -Ci uses a separate test key. Current files are in android/releases; old builds are archived in the project-root niepotrzebne folder.

PL: Aktualizacje korzystają z android/update.json. Weryfikowane są SHA-256, pakiet, wersja i podpis. Instalacja wymaga systemowego potwierdzenia Androida. Nie odinstalowuj aplikacji przed aktualizacją. Kanał Windows pozostaje oddzielny.

EN: Updates use android/update.json with SHA-256, package, version and certificate verification. Installation requires Android system confirmation. Do not uninstall before updating. Windows uses a separate release channel.

[Deutsch](README.de.md) · [Русский](README.ru.md) · [Français](README.fr.md) · [Español](README.es.md)
