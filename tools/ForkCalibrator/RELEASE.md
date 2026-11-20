# Releasing the fork calibrator

Increase `<Version>` in `ForkCalibrator.csproj` when the calibrator changes. Use a three-part version. Stroik compares that version against the EXE's Windows file version. Rebuilding the same version for a new Stroik ZIP does not prompt existing users to download it again.

Release the calibrator as `Stroik-Kalibrator.exe` with `Stroik-Kalibrator.exe.sha256`. A Windows tag such as `v1.0.24` can carry both applications. For a release containing only the calibrator, use `calibrator-v<version>` and mark it `--latest=false` so the main Stroik updater continues to find the latest Windows release. Download both public assets and verify their sizes and hashes against the local build.

Update the root `calibrator-update.json` on `main` after asset verification. Keep `schema` at `1` and set `version`, `releaseTag`, `sha256` and `size` from the final public EXE (size in bytes). Verify the public raw manifest too. Stroik checks and updates the calibrator independently of its own version. The full Windows ZIP always contains both EXEs and their checksums.
