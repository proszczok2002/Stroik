# Stroik 1.02

**An accessible Windows x64 instrument tuner for blind and visually impaired musicians.** It offers microphone based tuning or a synthesized tuning fork, keyboard controls, screen reader announcements, and Polish and English interfaces.

[Dokumentacja po polsku](README.md) · [Latest release](https://github.com/proszczok2002/Stroik/releases/latest) · [Changelog](CHANGELOG.md)

## Download

Download `Stroik.exe` from GitHub Releases. It is self-contained and needs no .NET installation. Keep it in a writable folder for automatic updates. The EXE is unsigned, so Windows may display a SmartScreen warning. Compare its SHA-256 with `Stroik.exe.sha256` using `Get-FileHash .\Stroik.exe -Algorithm SHA256`.

## Tuning methods

**Tuning fork** is selected on the first launch. **Strike tuning fork** plays a decaying tone for the selected note. Automatic strikes can be enabled with an interval of 1–10 seconds. This is a synthesized approximation of a struck fork, not a recording. Volume depends on Windows and your speakers. If unsure about tuning by ear, practise first on a modest, easy-to-handle instrument such as a ukulele or guitar; a broken string can still injure.

**Electronic tuner** uses the default Windows microphone. Start the microphone, play one note, then let it stop. After about 0.45 seconds of silence, the result appears in a text field and is announced through UI Automation. Buttons repeat the last result and test a screen reader announcement without the microphone. The last selected method is remembered across launches.

## Views and notes

**Beginner view** provides guidance and an octave selector. **Classic view** uses a compact layout; its octave selector can be enabled with the checkbox just before the note list. Tab and Shift+Tab move between controls. Arrow keys move continuously across octave boundaries in the note list. Press **I** or **S** there to move to the next sharp note.

The list covers the full 88-key piano range, A0–C8. The default is **G in the one-line octave** (formerly G4). The electronic tuner also offers automatic nearest-note detection. English B corresponds to Polish H; English B-flat is Polish B. Microphone measurement at the extremes depends heavily on hardware and harmonics; synthetic tones have been tested, not every real instrument.

## Settings and profiles

Settings appear in this order: A reference frequency, microphone sensitivity, in-tune tolerance in cents, octave and target note. **A=440 Hz** is the standard. Some older instruments may need a lower pitch such as 430–435 Hz, but age alone does not determine a safe value. **Before raising an old piano to 440 Hz, ask a piano technician to assess its design, condition and appropriate pitch.** Do not assume every nineteenth-century instrument should use 430–435 Hz. [Piano Technicians Guild: pitch raising](https://www.ptg.org/teacher-resources/caring-for-a-piano/pitch-raising) · [PTG: care for older pianos](https://my.ptg.org/ptgmain/piano/care/care-faqs)

Microphone sensitivity has descriptions for quiet instruments, normal conditions and background noise. Tolerance uses cents; the reported difference also includes hertz. Settings are saved locally, including language, theme, method, view, note and fork interval.

**Ctrl+S** or **Save settings file** in the context menu exports a JSON profile. **Ctrl+O** or **Open settings file** imports one. You can keep profiles for home, a café or a historic instrument. Profiles include the tuning method and no recordings. The **Settings** tab contains language, dark theme and the manual update check. The **Tuner** tab contains the tuning controls and profiles. Both tabs are keyboard accessible and have localized accessibility names.

## Updates

On launch, Stroik checks the latest stable GitHub release. If a newer version is available, it asks for consent. Choosing **Yes** downloads the EXE and SHA-256 file, checks the hash, closes the old process, replaces the EXE, launches the new one and deletes the old backup. It attempts rollback on replacement failure. **Check for updates** is available in the **Settings** tab. Automatic replacement requires a writable folder and the filename `Stroik.exe`. Recordings and results are never uploaded.

## Limits and building

The approximate microphone detection range is 27.5–4200 Hz for one steady note. Displaying 0.1 Hz does not imply guaranteed accuracy. Measurement pauses for 1.5 seconds after a result to avoid mistaking a screen reader's voice for another note. Real microphone, instrument, NVDA and speaker behavior requires hardware testing.

Build on Windows with .NET SDK 10:

```powershell
dotnet run --project tests/PitchChecks.csproj -c Release
dotnet build Stroik.csproj -c Release
powershell -NoProfile -ExecutionPolicy Bypass -File .\build-release.ps1
```

The script keeps the latest EXE, SHA-256 file and ZIP at the top level of `dist`. After a successful new-version build, it moves previous release files and older build and verification directories into `dist/niepotrzebne` without automatically deleting them. Increase the version in `Stroik.csproj` before the next release. The script blocks an accidental rebuild of an existing version; use `-RebuildSameVersion` only for an intentional rebuild before publication. Source code, tests and the local SDK remain available.

Pitch detection uses YIN, audio input uses Windows waveIn, and accessibility announcements use UI Automation. The project uses the [MIT license](LICENSE). Please report bugs through [Issues](https://github.com/proszczok2002/Stroik/issues).
