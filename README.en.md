# Stroik 1.0

**An accessible instrument tuner for Windows, designed for blind and visually impaired musicians.** It works with the keyboard and exposes results to screen readers, including NVDA. It measures a single note through the default Windows microphone and reports the note, tuning direction, and difference in hertz.

[Dokumentacja po polsku](README.md) · [Releases](https://github.com/proszczok2002/Stroik/releases)

## Download and run

Download `Stroik.exe` from the [latest release](https://github.com/proszczok2002/Stroik/releases/latest). It is a self-contained Windows x64 executable; no .NET installation is needed. Put it in a folder where you have write permission. Windows may show a SmartScreen warning for the unsigned executable.

The release also contains `Stroik.exe.sha256`. To verify the download in PowerShell, run `Get-FileHash .\Stroik.exe -Algorithm SHA256` and compare the result.

## Tuning with a screen reader

1. Start NVDA and run `Stroik.exe`.
2. Select **Start microphone**. Stroik uses the default Windows audio input.
3. Play one note and let it stop. After about 0.45 seconds of silence, the result appears in a text field and is announced through UI Automation.
4. **Repeat result** announces the last note again. **Test screen reader announcement** checks the notification without using the microphone.
5. Navigate all controls with Tab; buttons also have Alt access keys.

Choose a specific **target note**, or let Stroik find the nearest note automatically. A4 defaults to 440 Hz. The in-tune threshold is set in cents, while the spoken difference is in hertz. Adjust microphone sensitivity for quiet instruments or background noise. English B corresponds to Polish H; English B-flat corresponds to Polish B.

Settings include **Polski / English** and a **dark theme**. Preferences are saved locally in your Windows user profile. On supported Windows versions, the selected theme also applies to the title bar; native drop-down lists may still follow Windows settings.

## GitHub updates

At startup, Stroik checks the latest stable GitHub release. If it finds a newer version, it asks for confirmation. Choosing **Yes** downloads `Stroik.exe` and its SHA-256 file from the same release, verifies the checksum, closes the app, replaces the current EXE, and starts the new version. The old file is deleted after a successful replacement. If replacement fails, Stroik attempts to restore the previous file. You can also use **Check for updates**.

Automatic replacement requires the file to be named `Stroik.exe` and stored in a writable folder. Tuning continues to work offline. Stroik retrieves release metadata and, only after consent, update files. It does not upload audio recordings or results.

## Range and limitations

The measurement range is approximately 55–1200 Hz. Stroik expects one reasonably steady note. Displaying 0.1 Hz is output resolution, not guaranteed accuracy. Actual accuracy depends on the microphone, instrument, and room. Measurement pauses for 1.5 seconds after an announcement to avoid interpreting the screen reader's voice from speakers as another note. Headphones may help.

Automated tests use generated tones. Real NVDA speech, microphone behavior, and instrument accuracy require separate hardware testing.

## Build from source

Use .NET SDK 10 on Windows. In the project folder:

```powershell
dotnet run --project tests/PitchChecks.csproj -c Release
dotnet build Stroik.csproj -c Release
.\build-release.ps1
```

The script creates `dist/Stroik.exe`, `dist/Stroik.exe.sha256`, and a ZIP archive. Each future release must increase the version in `Stroik.csproj`, use a `vX.Y.Z` tag, and publish both the EXE and SHA-256 file in GitHub Releases for the updater to detect it.

Pitch detection uses the YIN method described by de Cheveigné and Kawahara in “YIN, a fundamental frequency estimator for speech and music,” *JASA* 2002. Audio input uses Windows `waveIn`; screen reader notifications use UI Automation.

The code is available under the [MIT license](LICENSE). Please report bugs and ideas through [Issues](https://github.com/proszczok2002/Stroik/issues).
