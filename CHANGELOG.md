# Changelog / Historia zmian

## 1.0.25 (Stroik 1.25)

PL:

- Stroik zapisuje ustawienia w `Stroik.settings.json` obok `Stroik.exe`. Logi, pliki aktualizatora i biblioteka NVDA trafiają do `Stroik-data`. Aby przenieść program i dane, skopiuj cały folder.
- Przy pierwszym uruchomieniu Stroik zachowuje dotychczasowy profil, mapę instrumentu i kalibrację. Zawartość `%LOCALAPPDATA%\Stroik` trafia do kopii w `Stroik-data\migracja-*`. Jeśli ustawienia są już obok EXE, zachowują pierwszeństwo. Stroik usuwa stary katalog dopiero po sprawdzeniu kopii.
- Rozpakuj ZIP do zapisywalnego folderu. Jeśli migrację przerwie brak dostępu lub blokada pliku, Stroik zachowa dane źródłowe wymagające dokończenia migracji i ponowi próbę przy następnym starcie.

EN:

- Stroik saves preferences to `Stroik.settings.json` next to `Stroik.exe`. Logs, updater files and the NVDA library are under `Stroik-data`. Copy the whole folder to move the program and its data.
- On first launch Stroik preserves the profile, instrument map and fork calibration. It copies `%LOCALAPPDATA%\Stroik` to a backup under `Stroik-data\migracja-*`. Existing settings next to the EXE take priority. Stroik removes the old directory once it has verified the copy.
- Extract the ZIP to a writable folder. If a file is locked or access is denied, Stroik keeps the source data that still needs migrating and retries on the next launch.

## 1.0.24 (Stroik 1.24)

PL:

- Przy pierwszym pobraniu wybierz pełny ZIP Windows. Rozpakuj go do jednego folderu, aby od razu mieć Stroik.exe, Stroik-Kalibrator.exe, instrukcje i sumy kontrolne.
- Jeśli kalibratora brakuje lub jest starszy, Stroik przy uruchomieniu pyta o pobranie. Po odmowie nie ponawia pytania o brakujący program ani o tę samą wersję. Możesz wyłączyć **Sprawdzaj kalibrator przy uruchomieniu** i zawsze użyć **Sprawdź kalibrator teraz** w Ustawieniach.
- Po potwierdzeniu Stroik sprawdza rozmiar, SHA-256 i wersję, po czym zapisuje kalibrator obok swojego EXE. Kalibrator 1.0.1 pokazuje własną wersję w tytule okna.

EN:

- For a first download, use the complete Windows ZIP. Extract it to one folder to get Stroik.exe, Stroik-Kalibrator.exe, guides and checksums together.
- If the calibrator is missing or out of date, Stroik asks at startup whether to download it. Declining skips further offers for the missing program or the same version. Disable **Check for the calibrator at startup** at any time, or use **Check calibrator now** in Settings.
- After you confirm, Stroik verifies size, SHA-256 and version before saving the calibrator next to the tuner EXE. Calibrator 1.0.1 shows its version in the window title.

## 1.0.23 (Stroik 1.23) — 2026-11-20

PL:

- Stroik zapisuje ustawienia w pliku `Stroik.settings.json` obok `Stroik.exe`. Aby przenieść ustawienia na inny komputer lub pendrive, skopiuj oba pliki.
- Jeśli przy pierwszym uruchomieniu obok programu brakuje pliku ustawień, Stroik kopiuje aktualny profil z `%LOCALAPPDATA%\Stroik\settings.json`, w tym aktywną mapę instrumentu i kalibrację kamertonu. Zachowuje oryginał. Istniejący plik obok programu ma pierwszeństwo.
- Jeśli folder programu nie pozwala utworzyć pliku, Stroik korzysta z ustawień w profilu Windows. Do przenośnego zapisu uruchamiaj `Stroik.exe` z folderu, do którego masz prawo zapisu.

EN:

- Stroik saves its preferences to `Stroik.settings.json` next to `Stroik.exe`. Copy both files when moving the program and its settings to another PC or USB drive.
- On first launch, if there is no settings file next to the program, Stroik copies the current profile from `%LOCALAPPDATA%\Stroik\settings.json`, including the active instrument map and fork calibration. It preserves the original. An existing file next to the program takes priority.
- If the program folder does not allow a new file, Stroik uses the Windows user profile. Run `Stroik.exe` from a writable folder to use portable settings.

## 1.0.22 (Stroik 1.22) — 2026-11-18

PL:

- W hybrydzie głośnikowej włącz opcję dwóch stuknięć. Po komunikacie o dobrym stroju kamerton pozostawia ciche okno na dwa krótkie stuknięcia. Nie trzeba dwukrotnie zagrać czystej nuty. Bez gestu kamerton powtarza ten sam wzorzec. Po złym wyniku obowiązuje ustawiona przerwa od 0 do 60 sekund.
- Dla pianin powojennych A4 = 440 Hz to zwykły punkt odniesienia. Przedwojenne instrumenty mogły być przewidziane do niższego stroju. Przed podnoszeniem stroju starego pianina warto uzyskać ocenę technika.
- W zakładce **Mapowanie instrumentu** wybierz oktawę, nutę i od 1 do 10 uderzeń. Kliknij **Rozpocznij mapowanie**, po każdym uderzeniu pozwól nucie ucichnąć i poczekaj na komunikat. Stroik przypisuje średnią częstotliwość w hercach do wybranej nuty. Po zebraniu całej serii wynik jest zapamiętany. Przejdź do następnej nuty listą, przyciskiem albo dwoma stuknięciami. **Zakończ mapowanie** zatrzymuje mikrofon. **Zapisz plik mapingu** (Ctrl+S) zapisuje JSON. **Użyj mapy do strojenia** włącza ją dla miernika i kamertonu. Nuty spoza mapy korzystają ze zwykłego stroju. Ten sam plik otworzysz na obu systemach.
- **Stroik-Kalibrator.exe** ma jedną listę 88 nut. Suwaki zapisują harmoniczne (0–200% wartości Stroika) i głośność (0–150%) oddzielnie dla każdej nuty. 100% to punkt wyjścia, a 0% harmonicznych daje czysty ton. Alt+U odtwarza nutę, Ctrl+S zapisuje JSON. W Stroiku na obu systemach wybierz **Ustawienia → Wczytaj kalibrację kamertonu**. Kalibracja zmienia barwę i poziom, a mapa instrumentu zmienia częstotliwości.
- W ustawieniach są osobne suwaki głośności kamertonu i odsłuchu z mikrofonu. Na Windows odpowiadają im także osobne sesje aktywnych źródeł w mikserze systemowym. Obie aplikacje korzystają z domyślnego wyjścia audio. Odsłuch mikrofonu uruchamiaj w słuchawkach.

EN:

- In speaker hybrid, enable the two-tap checkbox. After an in-tune announcement, the reference stays silent while you tap twice. The taps can be unpitched. With no gesture, the reference repeats the same note. A detuned reading uses the configured 0–60 second pause.
- A4 = 440 Hz is the usual reference for postwar pianos. Prewar instruments may have been built for lower pitch. Have a technician assess an old piano before raising its pitch.
- In **Instrument mapping**, choose octave, note and 1–10 strikes. Select **Start mapping**, let each strike decay and wait for its announcement. Stroik assigns the average frequency in hertz to the selected note. A complete series is stored automatically. Pick the next note in the list, use the button, or tap twice after saving the reading. **Finish mapping** stops capture. **Save mapping file** (Ctrl+S) exports JSON. **Use map for tuning** makes the readings the targets for the tuner and fork. Unmapped notes use standard tuning. Both platforms read the same file.
- **Stroik-Kalibrator.exe** has one list of 88 notes. Set harmonics (0–200% of the Stroik baseline) and volume (0–150%) per note. 100% is the baseline and 0% harmonics yields a pure tone. Alt+U plays the selected note. Ctrl+S saves JSON. In Stroik, use **Settings → Open fork calibration** on either platform. Calibration affects timbre and level, while mapping sets pitch.
- Settings provide separate fork and microphone-monitoring volume sliders. Windows also exposes separate mixer sessions for the active streams. Both apps use the default audio output. Use headphones for microphone monitoring.

## 1.0.21 (Stroik 1.21) — 2026-11-17

PL:

- A kontra otrzymało takie samo wsparcie harmonicznych jak niższe nuty. Obie platformy używają tej samej barwy bez zmiany oktawy. Słyszalność basu nadal zależy od głośników.
- Klasyczna hybryda uruchamia kamerton od razu, bez samouczka. W widoku dla początkujących najpierw czytane są wskazówki. Działanie Windows zachowano i dodano je w Androidzie.
- Przerwa po wyniku wynosi 0–60 sekund, także 1 lub 2. Zero oznacza powrót wzorca od razu po zakończeniu mowy i jest nowym ustawieniem domyślnym. Zapisane przerwy Windows są zachowane. Android ma tę samą regulację z suwakiem i wpisem liczby.
- Najpierw cały wynik, potem kamerton i możliwość przejścia. W hybrydzie przejście wymaga dobrego wyniku oraz dwóch uderzeń o potwierdzonej wysokości. W samym kamertonie nadal działa gest akustyczny.
- Android: klasyczna lista bez opisów położenia klawiszy, opisy metod dla początkujących oraz czarne klawisze strzałkami lewo/prawo. Wyniki i samouczek czyta syntezator aplikacji także przy TalkBack, aby znać koniec mowy; TalkBack nadal obsługuje nawigację.

EN:

- Contra A now has the same harmonic support as lower notes. Both platforms use matching timbres without octave transposition. Bass audibility still depends on speakers.
- Classic hybrid starts its reference immediately without a tutorial. Beginner view reads guidance first. The existing Windows behavior is retained and has been added to Android.
- The result pause accepts 0–60 seconds, including 1 and 2. Zero resumes the reference immediately after actual speech completion and is the new default. Saved Windows delays are retained. Android offers the same setting with a slider and numeric entry.
- The whole result finishes before the reference and navigation are released. Hybrid advance requires an in-tune result and two pitch-confirmed attacks. Fork-only mode keeps its acoustic gesture.
- Android Classic note lists omit key positions. Added beginner method descriptions and black-key selection with Left/Right arrows. App TTS reads results and tutorials even with TalkBack so completion can be observed; TalkBack still handles navigation.

## 1.0.20 (Stroik 1.20) — 2026-11-16

PL:

- Dodano niemiecki, rosyjski, francuski i hiszpański obok polskiego i angielskiego: interfejs, komunikaty, nuty, wskazówki, profile i instrukcje.
- Kamerton ma wyraźniejsze harmoniczne w najniższym rejestrze bez zmiany oktawy. Małe głośniki nadal mogą ograniczać bas.
- W metodzie Kamerton pole dwóch stuknięć samodzielnie steruje mikrofonem; ukryto osobne przyciski mikrofonu. Rozruch wymaga krótkiej ciszy, aby nie liczyć go jako uderzenia. Automat pozostawia czas na gest.
- Hybryda dla początkujących czeka na odczyt wskazówek NVDA/SAPI. Inny czytnik otrzymuje dostępne okno z przyciskiem rozpoczęcia po wskazówkach. Klasyczny widok rozpoczyna od razu. Dodano przerwę po wyniku 3–60 sekund, domyślnie 8; NVDA 2026.3+ i SAPI blokują wzorzec podczas mowy.
- Krótsze odczyty w widoku klasycznym: fortepian, lista nut bez opisów położenia, automat bez dodatkowej instrukcji. Liczby są pojedynczymi polami edycji z obsługą strzałek i wpisu przecinkiem/kropką, bez roli pokrętła.
- Góra/dół wybierają białe klawisze, lewo/prawo wyłącznie czarne. Następny/poprzedni według planu nadal obejmuje jego wszystkie nuty.
- Dodano opisy zastosowania czterech metod oraz dostępne z klawiatury wskazówki instrumentu w widoku początkującym; F1 otwiera je także na żądanie.

EN:

- Added German, Russian, French and Spanish alongside Polish and English: interface, messages, notes, guidance, profiles and guides.
- Stronger upper partials support the lowest fork register without transposing it. Small speakers can still limit bass.
- In Fork, the two-strike checkbox owns microphone capture; separate microphone buttons are hidden. Startup requires a brief quiet period so it cannot count as an attack. Automatic playback leaves time for the gesture.
- Beginner hybrid waits for NVDA/SAPI guidance. Other readers get an accessible Start-after-guidance dialog. Classic starts immediately. The new result pause is 3–60 seconds, default 8; NVDA 2026.3+ and SAPI hold the reference while speaking.
- Shorter Classic feedback: Piano, note lists without positions, automatic checkbox without extra manual-mode text. Numbers use a single editable text control with arrow keys and comma/dot entry, without a spin-button role.
- Up/Down select white keys; Left/Right select only black keys. Planned Next/Previous still visits all notes in the selected plan.
- Added purpose descriptions for all four methods and keyboard-accessible instrument guidance in Beginner; F1 also opens it on demand.

## 1.0.19 (Stroik 1.19) — 2026-11-16

PL:

- Widok klasyczny nie pokazuje wskazówek strojenia, opisu standardu 440 Hz ani pozostałości porad pianinowych po wyborze innego instrumentu. Pole ma krótką etykietę Częstotliwość, Hz.
- Hybryda pokazuje sterowanie mikrofonem i własne pole gestu dwóch stuknięć. Ręczne uderzenie i automat kamertonu są tylko w metodzie Kamerton. Elektroniczne przechodzenie po poprawnym wyniku działa wyłącznie w stroiku elektronicznym. Rozstrojone uderzenia resetują uzbrojony gest; oba ataki muszą zmieścić się w wybranej tolerancji i mieć świeże potwierdzenie wysokości.
- Shift+Alt+N lub Shift+Ctrl+N wraca do poprzedniego dźwięku według planu. Alt+N i Ctrl+N przechodzą dalej. Koniec planu zatrzymuje aktywne strojenie.
- Poprawiono polskie nazwy głośników, słuchawek i sposobów pomiaru. Wpis liczbowy przyjmuje kropkę i przecinek. Komunikaty używają powiadomień czytnika; przy braku wykrytego czytnika dodano asynchroniczny głos SAPI. Hybryda czeka na zakończenie własnej mowy SAPI.
- Niskie tony kamertonu mają większy poziom oraz ciche wyższe składowe pomocne małym głośnikom. Kwinty nadal grają właściwe nuty równocześnie: c razkreślne + g razkreślne to 261,63 i 392,00 Hz przy A=440. Wygładzono zakończenie tonu. Wrażenie oktawy i słyszalność basu zależą także od głośników; nie przesuwamy wzorca do innej oktawy.
- Klawisz na wizualnej klawiaturze jest czerwony przy rozstrojeniu, zielony w wybranej tolerancji, niebieski przed pomiarem. Kolor uzupełnia wynik tekstowy i wskaźnik.

Testy: PL/EN, cyfry i separatory dziesiętne, skróty, widoczność, pięć kolejnych wyników hybrydy, ścisła tolerancja gestu, widmo kwint w pięciu oktawach, SAPI start/koniec oraz dotychczasowe regresje audio/updatera. Nie potwierdzono odsłuchu NVDA ani prawdziwego instrumentu u zgłaszającego.

EN:

- Classic view hides tuning advice, the 440 Hz standard hint and stale piano guidance after instrument changes. The reference field is labelled Frequency, Hz.
- Hybrids show microphone controls and their acoustic double-strike option. Manual and automatic fork controls belong only to Fork. In-tune confirmed navigation belongs only to Electronic. Detuned attacks cancel an armed gesture; both attacks must meet the configured tolerance with fresh pitch confirmation.
- Shift+Alt+N and Shift+Ctrl+N select the previous planned note; Alt+N and Ctrl+N advance. Plan completion stops active tuning.
- Repair Polish speaker/headphone/interval labels. Numeric entry accepts either decimal separator. Reader notifications are retained; asynchronous SAPI speech is available without a detected screen reader. Speaker hybrid waits for its own SAPI speech to finish.
- Stronger bass levels with quiet upper partials help small speakers. Fifths still play the selected pair together: C4+G4 is 261.63+392.00 Hz at A=440. Tone endings fade smoothly. Bass audibility and perceived octave still depend on speakers; the reference is not transposed.
- The visual target key is red when detuned, green within the selected tolerance and blue before a reading. Text and the meter continue to report the result.

Validation: PL/EN, typed digits and decimal separators, shortcuts, visibility, five consecutive hybrid results, strict attack tolerance, five-octave fifth spectra, actual SAPI start/completion and existing audio/updater regression. Human NVDA listening and the reporter's real instrument remain unverified.


## 1.0.18 (Stroik 1.18) — 2026-11-15

- PL: Opisy położenia klawiszy pianina bezpośrednio w liście dźwięków; w widoku dla początkujących także przy ogłaszaniu nowego celu. Krótkie komunikaty w widoku klasycznym.
- EN: Piano key positions embedded in note options and Beginner target announcements. Classic announcements remain concise.
- PL/EN: PL/EN guides and navigation regression updated; retained fork/interval/updater regressions. Android 1.06 uses its own release channel.

## 1.0.17 (Stroik 1.17) — 2026-11-13

- PL: Zmiana kwinty/oktawy, nuty na liście, oktawy lub A odniesienia od razu zastępuje poprzedni dźwięk kamertonu, również bez mikrofonu i bez automatu. Poprzednio lista mogła wskazywać kwintę, gdy wciąż brzmiała pojedyncza nuta. Opis dostępności wskazuje obie nuty i dolny cel.
- EN: Changing consonance, target, octave or concert A immediately replaces the fork reference, including manual mode without microphone capture. Previously the list could show a fifth while a single old note was still playing. Accessible help names both notes and the lower target.
- PL/EN: Added UI-event-to-PCM regression checks for C4–G4, G4–D5, octaves, A=435/440, direct list/octave changes and Off. Synthetic arrow checks no longer depend on the user's physical Ctrl key. Separate piano-tab idea recorded in POMYSLY.md.

## 1.0.16 (Stroik 1.16) — 2026-11-13

- PL: Białe klawisze pod strzałkami góra/dół, półtony lewo/prawo także na otwartej liście; odczyt celu i test częstotliwości kamertonu. Zakończenie planu na granicy współbrzmień bez powtarzanych dialogów błędu.
- EN: White-key Up/Down and chromatic Left/Right on open/closed lists; selected-note notifications and fork-frequency regressions. Stop at valid interval boundaries without recurring error dialogs.
- PL/EN: Updater: helper readiness before app closure, local staging and SHA-256, file-lock retries, atomic replacement, visible-window startup acknowledgement, rollback on restart failure and diagnostic update.log; stalled response-body timeout. Actual version in the title/About.

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
