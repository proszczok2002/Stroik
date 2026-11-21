# Stroik Android 1.12

- The note slider moves one note at a time in either direction with accessibility actions or the arrow keys. It follows the selected tuning order.

- New profiles start in speaker hybrid. An ascending piano plan starts at A0. Stroik keeps existing settings.
- Choose cents, hertz, or both in either order for announcements. You can check a fifth at every fifth position in the tuning plan.
- The fork has factory calibration for 71 notes and softer upper bass partials. Fork-mode navigation requires two short taps separated by silence.
- The switch in Instrument Mapping shows whether map or standard tuning is active. Switching to standard keeps the map readings.
- Windows Calibrator 1.0.2 has automatic preview and an upper-bass-harmonics slider. Ctrl+O opens a JSON file and Ctrl+S saves changes to the same file. The first save lets you pick its name and location. Both Stroik editions can read older and newer calibration files.
- The note slider lets you enter names such as C4 and offers TalkBack actions to change octave. When Keep screen awake is on, pressing the power button to turn off the screen mutes result speech. With it off, results can be spoken with the screen off.

- In speaker hybrid, enable the two-tap checkbox. After an in-tune announcement, the reference stays silent while you tap twice. The taps can be unpitched. With no gesture, the reference repeats the same note. A detuned reading uses the configured 0–60 second pause.
- A4 = 440 Hz is the usual reference for postwar pianos. Prewar instruments may have been built for lower pitch. Have a technician assess an old piano before raising its pitch.
- In **Instrument mapping**, choose octave, note and 1–10 strikes. Select **Start mapping**, let each strike decay and wait for its announcement. Stroik assigns the average frequency in hertz to the selected note. A complete series is stored automatically. Pick the next note in the list, use the button, or tap twice after saving the reading. **Finish mapping** stops capture. **Save mapping file** (Ctrl+S) exports JSON. **Use map for tuning** makes the readings the targets for the tuner and fork. Unmapped notes use standard tuning. Both platforms read the same file.
- **Stroik-Kalibrator.exe** has one list of 88 notes. Set harmonics (0–200% of the Stroik baseline) and volume (0–150%) per note. 100% is the baseline and 0% harmonics yields a pure tone. Alt+U plays the selected note. Ctrl+S saves JSON. In Stroik, use **Settings → Open fork calibration** on either platform. Calibration affects timbre and level, while mapping sets pitch.
- Settings provide separate fork and microphone-monitoring volume sliders. Windows also exposes separate mixer sessions for the active streams. Both apps use the default audio output. Use headphones for microphone monitoring.

- Contra A now has the same harmonic support as lower notes. Both platforms use matching timbres without octave transposition. Bass audibility still depends on speakers.
- Classic hybrid starts its reference immediately without a tutorial. Beginner view reads guidance first. The existing Windows behavior is retained and has been added to Android.
- The result pause accepts 0–60 seconds, including 1 and 2. Zero resumes the reference immediately after actual speech completion and is the new default. Saved Windows delays are retained. Android offers the same setting with a slider and numeric entry.
- Android Classic note lists omit key positions. Added beginner method descriptions and black-key selection with Left/Right arrows. App TTS reads results and tutorials even with TalkBack so completion can be observed; TalkBack still handles navigation.

Stroik Android 1.10

Added German, Russian, French and Spanish alongside Polish and English. Interface, spoken messages, notes, octaves, key positions, guide and latest changes are translated. Language persists in settings and JSON profiles; older PL/EN profiles remain compatible. App speech selects the chosen language; its Android voice must be installed. New releases can include notes in all six languages, with fallback for older manifests. Updates still require Android system confirmation.


[APK 1.10](https://github.com/proszczok2002/Stroik/releases/download/android-v1.10/Stroik-Android-1.10.apk) · [Release](https://github.com/proszczok2002/Stroik/releases/tag/android-v1.10) · [PL](README.md) / [EN](README.en.md)

Classic hides guidance, including the 440 Hz standard hint and piano advice for ukulele. Choose the view only on Tuner; the duplicate Settings checkbox is removed.

Hybrid microphone controls also start its reference. Manual strike, fork automation and interval controls appear only in Fork; hybrids keep their separate double-tap checkbox. Electronic and Hybrid advance require a completed in-tune announcement. In Electronic and headphone modes, both attacks require pitch confirmation. Detuning cancels arming; a normal reading alone does not change target. Reference playback no longer invalidates an already completed reading. The fork waits for its own TTS utterance to actually finish.

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
