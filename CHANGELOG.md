# Changelog / Historia zmian

## 1.0.9 (Stroik 1.09) — 2026-11-03

- Show localized GitHub release notes in a scrollable, screen-reader-accessible update confirmation dialog.
- Embed the current bilingual release notes in the EXE for the offline Latest changes button in Settings.
- Add an About dialog with the app purpose and credits for Proszczok with Codex and idea author Witold.
- Add website, PayPal and Revolut buttons in Settings with the owner-provided destinations.
- Exclude the project-root `niepotrzebne` archive from compilation after moving old build files there.

## 1.0.8 (Stroik 1.08) — 2026-11-03

- Removed the redundant accessible name of the tab list, so a screen reader can announce the selected tab without repeating the whole tab group name.
- Kept the native tab list role, keyboard navigation, and Polish/English names for the individual Tuner and Settings pages.
- Added a regression check for tab switching and the accessible names and role.

## 1.0.7 (Stroik 1.07) — 2026-11-02

- Added a bilingual, keyboard-accessible setting for the maximum gap between two pitch-confirmed strikes: 0.5–2.5 seconds, default 1.2 seconds.
- Made onset detection less dependent on the second strike matching the first strike's loudness.
- Saved the gap in JSON profiles while retaining the 1.2-second default for older profiles.
- Added checks for a quiet second strike, shorter and longer configured gaps, sustained-note rejection and PL/EN control labeling.

## 1.0.6 (Stroik 1.06) — 2026-11-02

- Corrected Alt+U: one immediate fork strike in manual mode; pause/resume cyclic strikes when automatic mode is selected, without changing the checkbox.
- Added a bilingual, screen-reader-accessible running/paused/manual status beneath the automatic-strike control.
- Added tests for all three shortcut actions, automatic timer state and both language labels.
- Recorded the release housekeeping rule in AGENTS.md: archive old binaries/builds in dist/niepotrzebne, keep the latest release and editable source.
## 1.0.5 (Stroik 1.05) — 2026-11-01

- Added Alt+U to toggle automatic tuning fork strikes; switching off stops the timer and current tone.
- Retries transient GitHub errors and interrupted EXE downloads while retaining SHA-256 verification.
- Shows startup update status and errors in Settings in Polish and English, with manual retry.
- Checks target-folder write access before closing the old app and retries cleanup of a temporarily locked old EXE.
- Added deterministic retry tests and an opt-in live GitHub API/download probe.
## 1.0.4 (Stroik 1.04) — 2026-11-01

- Added optional hands-free advancement after two pitch-verified strikes following an in-tune result, plus Ctrl+N manual navigation.
- Added piano plans: ascending notes or alternating five-note blocks on both sides of the starting note, in semitone or whole-tone steps.
- Added bilingual piano key-position descriptions, an optional marked keyboard and a live pitch deviation meter.
- Corrected physical string orientation and added standard and A/G-swapped high-G ukulele presets.
- Added manual microphone reading correction in cents and best-effort screen reader detection for default graphics visibility.
- Saved all new choices in backward-compatible JSON profiles; updated PL/EN guidance and tests.
## 1.0.3 (Stroik 1.03) — 2026-10-31

- Added piano, guitar, ukulele, violin, bass guitar and chromatic instrument choices, with standard open-string lists and localized labels.
- Automatic electronic tuning now compares a string instrument against its nearest open string; piano and chromatic modes retain the full 88-key range.
- Added instrument-specific beginner guidance, saved instrument choice and backward-compatible JSON profiles.
- Updated Polish and English documentation and tests for presets, automatic selection and UI lists.

## 1.0.2 (Stroik 1.02) — 2026-10-31

- Moved language, dark theme and manual update check to a dedicated Settings tab.
- Localized Tuner/Settings tab labels and accessibility names in Polish and English.
- Styled tab headers for the dark theme and kept the Tuner tab selected on startup.
- Added structural tests for tab placement and both interface languages.
## 1.0.1 (Stroik 1.01) — 2026-10-31

- Two tuning methods: synthesized struck tuning fork with manual or timed strikes, and electronic microphone tuner. The last method is remembered.
- Beginner and classic views, with keyboard-accessible octave selection and continuous navigation through the 88-key piano range.
- Traditional Polish and English octave names, default G in the one-line octave, and I/S navigation among sharp notes.
- JSON settings profiles with save (Ctrl+S/context menu) and open (Ctrl+O/context menu).
- A=440 Hz guidance and a careful warning about assessing older pianos before raising string tension.
- Expanded synthetic pitch tests across the piano range. Polish and English UI and documentation updated.

## 1.0.0 — 2026-10-30

- Accessible Windows instrument tuner with keyboard controls and UI Automation announcements.
- Polish and English interface and documentation.
- Dark theme and saved language/theme preferences.
- Opt-in GitHub Release updates with SHA-256 verification and automatic EXE replacement.
- Self-contained Windows x64 release and synthetic-tone pitch tests.
