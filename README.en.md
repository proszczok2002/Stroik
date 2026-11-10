# Stroik 1.14

[Native Android 1.03](https://github.com/proszczok2002/Stroik/releases/tag/android-v1.03) is available. See the [Android guide](android/README.en.md) and [device report](android/DEVICE_TESTS.md). Windows updates use a separate release channel.

**An accessible Windows x64 instrument tuner for blind and visually impaired musicians.** It offers microphone based tuning or a synthesized tuning fork, keyboard controls, screen reader announcements, and Polish and English interfaces.

[Dokumentacja po polsku](README.md) · [Latest release](https://github.com/proszczok2002/Stroik/releases/latest) · [Changelog](CHANGELOG.md)

## Download

Download `Stroik.exe` from GitHub Releases. It is self-contained and needs no .NET installation. Keep it in a writable folder for automatic updates. The EXE is unsigned, so Windows may display a SmartScreen warning. Compare its SHA-256 with `Stroik.exe.sha256` using `Get-FileHash .\Stroik.exe -Algorithm SHA256`.

## Tuning methods

**Tuning fork** is selected on the first launch. **Strike tuning fork** plays a decaying tone for the selected note. Automatic strikes can be enabled with an interval of 1–10 seconds. This is a synthesized approximation of a struck fork, not a recording. Volume depends on Windows and your speakers. If unsure about tuning by ear, practise first on a modest, easy-to-handle instrument such as a ukulele or guitar; a broken string can still injure.

**Electronic tuner** uses the default Windows microphone. Start the microphone, play one note, then let it stop. After about 0.45 seconds of silence, the result appears in a text field and is announced through UI Automation. Buttons repeat the last result and test a screen reader announcement without the microphone. The last selected method is remembered across launches.

### Hybrid practice and fork navigation in 1.13

Choose **Hybrid — speakers (alternating)** or **Hybrid — headphones**, then **Start microphone**. The speaker variant plays the reference for 3 seconds, ignores microphone samples for 3.4 seconds, then listens to your instrument. After the note stops, it announces the reading, allows about 5 seconds for speech and plays the reference again. Long NVDA speech or reverberation may require a manual repeat. Changing the target plays the new reference. **Stop microphone** ends the cycle.

The headphone variant listens while the tuning fork plays. Optional **Monitor microphone in headphones** is off by default. Put headphones on first: routing the microphone through speakers can cause feedback. Monitoring uses the default Windows devices, changes no system settings and has hardware and driver dependent latency.

**Two strikes advance to the next note in the selected order** works with the fork and hybrid methods. Enabling it in the open app starts the microphone; after restarting the app, press **Start microphone**. Two distinct attacks within the selected time advance according to the tuning plan and play the new reference. This is an acoustic gesture without pitch validation: two incidental noises may also trigger it. Listening waits for the app's own fork tone to decay. Automatic fork strikes are at least 6.5 seconds apart with tap navigation enabled. **Ctrl+N** still advances manually.

### Fifths and octaves

Select the lower note, then choose **Fifths** or the next item **Octaves** under **Check consonance**. C4 pairs with G4 or C5, always upward. **Off** restores normal tuning. Fork mode plays the two equal-tempered notes together. Choose a lower target if the upper note would exceed the 88-key range.

For microphone measurement, the default is **Separate notes: lower, then upper**. Play lower, mute, wait for confirmation, then play upper and mute. A wrong note is not accepted as a correct interval. Results give deviation from 700 or 1200 cents and estimated ideal-harmonic beats: |2×upper − 3×lower| Hz for fifths and |upper − 2×lower| Hz for octaves.

**Together — experimental** analyses about 1.4–1.5 seconds of audio; play the pair for about 2 seconds near the selected target (search range ±60 cents). It measures two spectral components, which need not be two separate strings. Especially for octaves, a lower note's harmonic can mimic the upper note. Confirm with separate measurement. It cannot certify that the whole instrument has no audible beats.

An equal-tempered fifth may correctly have slow beats. Piano partials are inharmonic and stretched octaves may be needed. Beat estimates from fundamentals do not measure the actual beating of every partial. [UNSW acoustics explanation](https://newt.phys.unsw.edu.au/jw/musFAQ.html).

## Views and notes

**Strikes that are too weak or too strong can cause inaccurate readings.** Play evenly and avoid microphone clipping. Beginner view also includes this advice. Since 1.13 the octave list is always visible in piano and chromatic modes, in both views. Open-string presets retain their instrument note lists; switch to chromatic mode for other octaves.

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

In electronic mode, select an open string or **Automatic**. With a string instrument selected, automatic mode compares the input with the nearest open string of that instrument; piano and chromatic modes use the nearest chromatic note. The tuning fork plays the selected pitch. For other alternate tunings, low-G ukulele, five-string bass or fretted notes, use chromatic mode and select the note manually. The instrument is remembered and included in JSON profiles. Older profiles without this field load as piano.

**Piano limitation:** selecting piano does not measure each string's inharmonicity or calculate a stretched-octave tuning curve. A frequency indicator alone cannot replace skilled whole-piano tuning. [Piano Technicians Guild explains stretch tuning](https://my.ptg.org/communities/community-home/digestviewer/viewthread?CommunityKey=6265a40b-9fd2-4152-a628-bd7c7d770cbf&MessageKey=c2e42d9f-6450-4ec6-a2d3-48f3b6a2ae79&tab=digestviewer).

## Settings and profiles

**Ctrl+Q** closes the app and stops microphone capture, tuning fork playback and update checks. **Quit (Ctrl+Q)** is also available in the context menu.

After tuning method, choose instrument and view. The remaining controls are A reference frequency, microphone sensitivity, in-tune tolerance in cents, and target note or string. Octave selection is available for piano and chromatic modes. **A=440 Hz** is the standard. Some older instruments may need a lower pitch such as 430–435 Hz, but age alone does not determine a safe value. **Before raising an old piano to 440 Hz, ask a piano technician to assess its design, condition and appropriate pitch.** Do not assume every nineteenth-century instrument should use 430–435 Hz. [Piano Technicians Guild: pitch raising](https://www.ptg.org/teacher-resources/caring-for-a-piano/pitch-raising) · [PTG: care for older pianos](https://my.ptg.org/ptgmain/piano/care/care-faqs)

Microphone sensitivity has descriptions for quiet instruments, normal conditions and background noise. Tolerance uses cents; the reported difference also includes hertz. Settings are saved locally, including language, theme, method, instrument, view, note and fork interval.

**Ctrl+S** or **Save settings file** in the context menu exports a JSON profile. **Ctrl+O** or **Open settings file** imports one. You can keep profiles for home, a café or a historic instrument. Profiles include the tuning method and no recordings. The **Settings** tab contains language, dark theme and the manual update check. The **Tuner** tab contains the tuning controls and profiles. Both tabs are keyboard accessible and have localized accessibility names.


## Double strikes in 1.07

In electronic mode, after an in-tune result, two distinct strikes at the target pitch can select the next note. **Maximum gap between two strikes** can be set from **0.5 to 2.5 seconds**; the new default is **1.2 seconds**. A longer gap gives more time for the second strike, but may increase accidental advancement. You can turn this feature off or use **Ctrl+N** to advance manually.

The detector allows strikes of different volumes, but both must exceed the microphone threshold and be recognized at the target pitch. If a quiet ukulele is still missed, try the microphone sensitivity for a quiet instrument. Wait for the result announcement to finish before striking. The chosen time is saved locally and in JSON profiles. Behavior with a particular microphone, ukulele or piano still needs a hardware test.

## Tuning fork in 1.06

When **Strike tuning fork automatically** is unchecked, **Alt+U** plays one immediate strike. The **Strike tuning fork** button does the same.

When checked, the fork strikes repeatedly at the selected interval. **Alt+U** pauses or resumes the cycle without unchecking the option. A label below the option reports whether automatic strikes are running, paused, or in manual mode; screen readers can access this status too. A checked automatic mode starts running again when the app is relaunched.
## Fixes in 1.05

Version 1.05 introduced Alt+U for the tuning fork. Its current behavior is described in the 1.06 section above. The automatic-mode choice is saved in settings and profiles.

The updater retries transient network errors and interrupted EXE downloads. The startup check result or error is shown in **Settings**; **Check for updates** retries manually. Before closing the app, it checks write access to the EXE folder. After a successful replacement, the new version retries removing the old copy if it was briefly locked. SHA-256 verification remains mandatory.
## New in 1.04

**Hands-free navigation.** In electronic mode, after an in-tune result, wait for the announcement to finish. Two distinct strikes of the same string or key, each recognized at the target pitch, select the next note. Their allowed time can be changed as described in the 1.07 section above. **Ctrl+N** or **Next note** advances manually. Detection depends on the microphone, playing level and instrument; check the selected note before moving a tuning lever.

**Piano order.** Move upward from the selected note or alternate blocks of five semitones to the right and left of the starting note. The whole-tone variant moves in two-semitone steps and skips half the notes; tune those separately afterwards. The plan guides navigation but cannot ensure safe tension distribution or replace a [technician's assessment and pitch-raising procedure](https://www.ptg.org/teacher-resources/caring-for-a-piano/pitch-raising).

**Keyboard guide.** In piano mode, a text description locates each white or black key relative to groups of two and three black keys. Enable graphics in **Settings** for a virtual keyboard with the selected key marked by a dot and a live tuning deviation meter in electronic mode. Graphics default to off when a screen reader is detected. Detection is imperfect, and the setting can always be changed manually.

**Ukulele and string position.** Standard high-G tuning, floor to ceiling, is A4–E4–C4–G4 (strings 1–4). The swapped A/G option is G4–E4–C4–A4. The choice persists in settings and JSON profiles. On guitar, floor to ceiling is E4–B3–G3–D3–A2–E2; G is string 3. [Kala: ukulele string order](https://kalabrand.com/blogs/home/ukulele-tuning-decoded) · [Fender: guitar string numbering](https://www.fender.com/articles/chords/read-guitar-chord-charts)

**Microphone correction.** Enter a manual correction from −100 to +100 cents in **Settings**, after comparing with a known reference. Positive values raise the reading. This is a fixed measurement offset, not automatic microphone calibration. The value is included in JSON profiles.
## Updates

In Settings, disable **Automatically check for updates** or choose an interval from **1 to 1440 minutes** (default 5). Disabling it stops both startup and periodic checks; **Check for updates** still works manually. Choices are saved locally and in JSON profiles.

Since 1.12, checks also run periodically while the app is open (every 5 minutes by default). Background checks do not announce that no update is available or that a connection failed. A release notes dialog appears only when a new version is detected, once per version per app session. After declining, use Check for updates to reopen the offer. Checks do not overlap, and the timer stops when the app closes.

On launch, Stroik checks the latest stable GitHub release. If a newer version is available, it shows the Polish or English release changes in a scrollable confirmation dialog. Choosing **Yes, download and install** downloads the EXE and SHA-256 file, checks the hash, closes the old process, replaces the EXE, launches the new one and deletes the old backup. It attempts rollback on replacement failure. **Check for updates** and **Latest changes** are available in the **Settings** tab. Latest changes shows the installed version's notes even offline. Automatic replacement requires a writable folder and the filename `Stroik.exe`. Recordings and results are never uploaded.

Settings also includes **About Stroik**, **Proszczok website**, **Support via PayPal** and **Support via Revolut**. About explains the app's purpose and credits its creator, Proszczok with Codex, and the original idea by Witold. The other buttons open [proszczok.pl](https://proszczok.pl/), [PayPal](https://paypal.me/proszczok) and [Revolut](https://revolut.me/r_rosiejka) in a browser. Support is voluntary; Stroik does not process payments.

After a checkbox is activated with the mouse or Space, the app announces its name and “checked” or “unchecked” to a screen reader. This applies to dark theme, graphics, automatic tuning fork strikes, double-strike navigation and the octave selector. Loading settings does not produce these announcements. Actual NVDA speech still needs testing on the user's computer.

## Limits and building

Version 1.11 improves short and decaying note detection and completes a measurement when unpitched background noise remains. Changing the target, instrument, reference pitch or sensitivity starts a fresh measurement without restarting the microphone. Capture errors are reported so the microphone can be restarted. Synthetic signal tests do not replace testing in actual playing conditions.

The approximate microphone detection range is 27.5–4200 Hz for one steady note. Displaying 0.1 Hz does not imply guaranteed accuracy. Measurement pauses for 1.5 seconds after a result to avoid mistaking a screen reader's voice for another note. Real microphone, instrument, NVDA and speaker behavior requires hardware testing.

Build on Windows with .NET SDK 10:

```powershell
dotnet run --project tests/PitchChecks.csproj -c Release
dotnet build Stroik.csproj -c Release
powershell -NoProfile -ExecutionPolicy Bypass -File .\build-release.ps1
```

The script keeps the latest EXE, SHA-256 file and ZIP at the top level of `dist`. After a successful new-version build, it moves previous release files and older build and verification directories into `niepotrzebne` at the project root (`F:\stroik\niepotrzebne`) without automatically deleting them. Increase the version in `Stroik.csproj` before the next release. The script blocks an accidental rebuild of an existing version; use `-RebuildSameVersion` only for an intentional rebuild before publication. Source code, tests and the local SDK remain available.

Pitch detection uses YIN, audio input uses Windows waveIn, and accessibility announcements use UI Automation. The project uses the [MIT license](LICENSE). Please report bugs through [Issues](https://github.com/proszczok2002/Stroik/issues).
