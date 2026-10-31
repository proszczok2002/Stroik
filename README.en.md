# Stroik 1.03

**An accessible Windows x64 instrument tuner for blind and visually impaired musicians.** It offers microphone based tuning or a synthesized tuning fork, keyboard controls, screen reader announcements, and Polish and English interfaces.

[Dokumentacja po polsku](README.md) · [Latest release](https://github.com/proszczok2002/Stroik/releases/latest) · [Changelog](CHANGELOG.md)

## Download

Download `Stroik.exe` from GitHub Releases. It is self-contained and needs no .NET installation. Keep it in a writable folder for automatic updates. The EXE is unsigned, so Windows may display a SmartScreen warning. Compare its SHA-256 with `Stroik.exe.sha256` using `Get-FileHash .\Stroik.exe -Algorithm SHA256`.

## Tuning methods

**Tuning fork** is selected on the first launch. **Strike tuning fork** plays a decaying tone for the selected note. Automatic strikes can be enabled with an interval of 1–10 seconds. This is a synthesized approximation of a struck fork, not a recording. Volume depends on Windows and your speakers. If unsure about tuning by ear, practise first on a modest, easy-to-handle instrument such as a ukulele or guitar; a broken string can still injure.

**Electronic tuner** uses the default Windows microphone. Start the microphone, play one note, then let it stop. After about 0.45 seconds of silence, the result appears in a text field and is announced through UI Automation. Buttons repeat the last result and test a screen reader announcement without the microphone. The last selected method is remembered across launches.

## Views and notes

**Beginner view** provides guidance for the selected instrument. **Classic view** uses a compact layout. Tab and Shift+Tab move between controls. Piano and chromatic modes have an octave selector and the full note list: arrows cross octave boundaries, and **I** or **S** selects the next sharp note. For string instruments, arrows move through open strings and the octave selector is hidden.

Piano and chromatic modes cover the 88-key piano range, A0–C8. The defaults are **piano and G in the one-line octave** (G4). Microphone measurement at the extremes depends heavily on hardware and harmonics; synthetic tones have been tested, not every real instrument.

## Instruments

Choose piano, guitar, ukulele, violin, bass guitar or chromatic mode in the **Instrument** list. Standard open-string presets are:

| Instrument | Strings in list order |
| --- | --- |
| Guitar | E2–A2–D3–G3–B3–E4 (strings 6–1) |
| Ukulele | G4–C4–E4–A4 (strings 4–1, high G) |
| Violin | G3–D4–A4–E5 (strings 4–1) |
| Bass guitar | E1–A1–D2–G2 (strings 4–1) |

English **B** is Polish **H**; English **B-flat** is Polish **B**. The presets use the standard tunings described by [Fender for guitar](https://www.fender.com/articles/setup/standard-tuning-how-eadgbe-came-to-be), [Kala for ukulele](https://kalabrand.com/pages/ukulele-tuning), [Yamaha for violin](https://www.yamaha.com/en/musical_instrument_guide/violin/mechanism/mechanism002.html) and [Fender for bass](https://www.fender.com/articles/instruments/5-tips-to-move-from-guitar-to-bass).

In electronic mode, select an open string or **Automatic**. With a string instrument selected, automatic mode compares the input with the nearest open string of that instrument; piano and chromatic modes use the nearest chromatic note. The tuning fork plays the selected pitch. For alternate tunings, low-G ukulele, five-string bass or fretted notes, use chromatic mode and select the note manually. The instrument is remembered and included in JSON profiles. Older profiles without this field load as piano.

**Piano limitation:** selecting piano does not measure each string's inharmonicity or calculate a stretched-octave tuning curve. A frequency indicator alone cannot replace skilled whole-piano tuning. [Piano Technicians Guild explains stretch tuning](https://my.ptg.org/communities/community-home/digestviewer/viewthread?CommunityKey=6265a40b-9fd2-4152-a628-bd7c7d770cbf&MessageKey=c2e42d9f-6450-4ec6-a2d3-48f3b6a2ae79&tab=digestviewer).

## Settings and profiles

After tuning method, choose instrument and view. The remaining controls are A reference frequency, microphone sensitivity, in-tune tolerance in cents, and target note or string. Octave selection is available for piano and chromatic modes. **A=440 Hz** is the standard. Some older instruments may need a lower pitch such as 430–435 Hz, but age alone does not determine a safe value. **Before raising an old piano to 440 Hz, ask a piano technician to assess its design, condition and appropriate pitch.** Do not assume every nineteenth-century instrument should use 430–435 Hz. [Piano Technicians Guild: pitch raising](https://www.ptg.org/teacher-resources/caring-for-a-piano/pitch-raising) · [PTG: care for older pianos](https://my.ptg.org/ptgmain/piano/care/care-faqs)

Microphone sensitivity has descriptions for quiet instruments, normal conditions and background noise. Tolerance uses cents; the reported difference also includes hertz. Settings are saved locally, including language, theme, method, instrument, view, note and fork interval.

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
