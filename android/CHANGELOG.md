# Android changelog / Historia zmian Android

## 1.02 / 102

- Fix incompatible widget-state restoration after theme/configuration changes; retain the current tab, scroll and last result.
- Add real-device regression for legacy state and repeated PL/EN page recreation/theme switching.
- Original 1.0 crash reproduced on the device; candidate verification and Play Protect status are recorded in DEVICE_TESTS.md.
## 1.01 / 101

- Updater test release, signed with the same production key as 1.0.
- Updated installed release notes; tuning algorithms remain compatible.
- Two-row bottom navigation fits large text better; corrected Polish octave dropdown grammar.

## 1.0 / 100

- Native accessible PL/EN Android tuner: fork, microphone, hybrid modes, instruments, octaves, intervals, profiles and update installer.
- Device coverage on POCO F8 Ultra Android 16: 1831 interface assertions and 7 capture/playback checks with TalkBack enabled.
- Human pronunciation/gesture listening, real instrument accuracy and headphone monitoring are documented separately from automated coverage.
