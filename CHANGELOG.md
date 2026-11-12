# Changelog / Historia zmian

## 1.0.15 (Stroik 1.15) — 2026-11-12

- PL: Naprawa Alt+N, napisu Wyłączone, nowy cel odtwarzany w kamertonie, krótsza wypowiedź, widok przed metodą, nawigacja ton/półton, opcja głosu w grafice i ograniczona korekta poziomu tonów.
- EN: Fix Alt+N and Polish Off text; play the next fork target; concise announcements, view before method, tone/semitone keys, visual-mode voice preference and bounded pitch-dependent gain.

## 1.0.14 (Stroik 1.14) — 2026-11-10

- Retain the consonance, hybrid practice, octave visibility and update settings introduced in 1.13.
- Stop the active microphone and hybrid cycle before importing a settings profile so a different method starts cleanly; add a lifecycle regression check.

## 1.0.13 (Stroik 1.13) — 2026-11-10

- Restore the octave selector for both piano/chromatic views, independently of the legacy visibility preference.
- Add ascending fifth/octave fork pairs, sequential interval readings and an explicitly experimental targeted FFT mode for simultaneous components.
- Add separate acoustic tap navigation for fork/hybrid practice, using the existing note order and gap setting.
- Add alternating speaker practice, concurrent headphone practice and opt-in bounded microphone monitoring through WinMM.
- Add persisted automatic update enable/disable and a 1–1440 minute interval.
- Add bilingual beginner advice about weak/strong strikes and detailed limits for intervals, beat estimates and monitoring.

## 1.0.12 (Stroik 1.12) — 2026-11-09

- Check for updates silently at startup and every five minutes while the app runs.
- Suppress status changes and error dialogs during background checks unless a new release is offered.
- Prevent concurrent checks and repeated automatic prompts for the same version in one session; retain manual retry.
- Stop the timer and ignore late network results after app closure.
- Add Ctrl+Q and a context menu command to quit the app with normal audio and timer cleanup.

## 1.0.11 (Stroik 1.11) — 2026-11-07

- Measure pitch in every full audio window to collect short and decaying notes consistently.
- End a note after reliable pitch stops, even if unpitched noise remains above the sensitivity threshold; exclude DC offset from RMS.
- Reset measurement on target/reference/sensitivity changes and discard queued results from an older measurement generation.
- Requeue native microphone buffers before analysis, wait for the worker before freeing buffers, and report processing/requeue errors.
- Add regression checks for short quiet tones, background noise, recovery, DC offset and target changes during a pending measurement.

## 1.0.10 (Stroik 1.10) — 2026-11-03

- Announce the new checked or unchecked state in Polish or English immediately after user activation of each checkbox.
- Queue the notification after the native WinForms click, using the checkbox's own UI Automation object; do not announce programmatic settings changes.
- Keep the 1.09 update dialog and embedded latest changes so an installed 1.09 can show the 1.10 changelog before download.

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
