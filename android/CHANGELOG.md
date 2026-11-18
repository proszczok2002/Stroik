# Android changelog / Historia zmian Android

## 1.10 / 110 — 2026-11-18

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

## 1.09 / 109 — 2026-11-17

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

## 1.08 / 108 — 2026-11-16

PL:

Dodano niemiecki, rosyjski, francuski i hiszpański obok polskiego i angielskiego. Przetłumaczono interfejs, komunikaty głosowe, nuty, oktawy, położenia klawiszy, instrukcję i ostatnie zmiany. Wybór języka zapisuje się w ustawieniach i profilach JSON; stare profile PL/EN pozostają zgodne. Własny syntezator aplikacji wybiera język zgodny z ustawieniem; potrzebny jest odpowiedni głos Androida. Nowe wydania mogą zawierać opisy zmian we wszystkich sześciu językach, ze zgodnym odczytem starszych manifestów. Zasady systemowego potwierdzania aktualizacji pozostają zachowane.

EN:

Added German, Russian, French and Spanish alongside Polish and English. Interface, spoken messages, notes, octaves, key positions, guide and latest changes are translated. Language persists in settings and JSON profiles; older PL/EN profiles remain compatible. App speech selects the chosen language; its Android voice must be installed. New releases can include notes in all six languages, with fallback for older manifests. Updates still require Android system confirmation.

## 1.07 / 107 — 2026-11-16

PL:

Widok klasyczny ukrywa porady, także standard 440 Hz i wskazówki pianinowe przy ukulele. Widok wybierasz tylko na stronie Stroik; usunięto powtórzone pole w Ustawieniach.

W hybrydzie sterujesz mikrofonem, który uruchamia także kamerton. Ręczne uderzenie, automat i jego odstęp są tylko w metodzie Kamerton; hybryda zachowuje osobne pole dwóch akustycznych stuknięć. Przechodzenie po poprawnym wyniku dotyczy metody Elektroniczny. Oba potwierdzone ataki wymagają wybranej tolerancji, świeżej próbki i właściwej nuty. Rozstrojenie kasuje uzbrojenie; zwykły wynik sam nie zmienia celu. Kolejny wzorzec rozpoczyna świeży pomiar, odrzucając stare oczekujące wyniki. Kamerton po własnym TTS czeka na faktyczne zakończenie wypowiedzi.

Shift+Alt+N i Shift+Ctrl+N wracają do poprzedniego dźwięku według planu. Wpis wartości otwiera klawiaturę numeryczną i przyjmuje kropkę lub przecinek; suwak i profil zapisują podaną liczbę. Ustawienie prędkości mowy pozostaje w Ustawieniach.

Wzmocniono niskie tony kamertonu i wygładzono zakończenie, aby ograniczyć stuk. Kwinta od c razkreślnego zawiera c razkreślne i g razkreślne, 261,63 i 392,00 Hz przy A=440; test widma obejmuje pięć oktaw i brak składowych o oktawę niższych. Małe głośniki nadal ograniczają słyszalność basu. Wizualny klawisz zmienia się z niebieskiego na czerwony przy rozstrojeniu lub zielony w ustawionej tolerancji. Kolor uzupełnia opis tekstowy.

Próby na Pixelu z włączonym TalkBack są opisane w raporcie DEVICE_TESTS.md. Testy struktury, zdarzeń i audio nie potwierdzają wymowy, naturalnych gestów ani strojenia prawdziwego instrumentu na każdym telefonie.

EN:

Classic hides guidance, including the 440 Hz standard hint and piano advice for ukulele. Choose the view only on Tuner; the duplicate Settings checkbox is removed.

Hybrid microphone controls also start its reference. Manual strike, fork automation and interval controls appear only in Fork; hybrids keep their separate acoustic double-strike checkbox. In-tune confirmed advance belongs to Electronic. Both attacks require fresh pitch at the expected note within the chosen tolerance. Detuning cancels arming; a normal reading alone does not change target. A new reference starts a fresh capture epoch, rejecting stale pending results. The fork waits for its own TTS utterance to actually finish.

Shift+Alt+N and Shift+Ctrl+N select the previous planned note. Numeric entry opens the numeric keyboard and accepts either decimal separator; the slider and profile retain the entered value. Speech rate remains in Settings.

Stronger bass levels and a smooth tone ending reduce weak lows and end clicks. A fifth from C4 contains C4 and G4, 261.63 and 392.00 Hz at A=440; spectrum tests cover five octaves and reject half-frequency components. Small speakers still limit bass audibility. The target key changes from blue to red when detuned or green inside the selected tolerance. Color supplements text.

Pixel checks with TalkBack enabled are documented in DEVICE_TESTS.md. Structural, event and audio-state tests do not establish human pronunciation, natural gestures or real-instrument tuning on every phone.


## 1.06 / 106 — 2026-11-15

- PL: Suwaki wszystkich ustawień liczbowych z dialogiem numerycznym; prędkość mowy 0,5–3 i oczekiwanie kamertonu na koniec wypowiedzi TTS. Zatrzymanie wszystkich zasobów na końcu planu. Bez przycisku mikrofonu w kamertonie i podwójnego przystanku stanu/przycisku.
- EN: Sliders with numeric dialogs for all numeric settings; speech speed 0.5–3 and actual TTS completion gating. Stop all resources at plan end. Hide fork-mode microphone button and redundant status navigation stop.
- PL/EN: Piano positions within note options and beginner Next announcements; explicit acyclic accessibility traversal, read-only result without editing focus; standard media-session start/stop; fixed top gauge/keyboard.
- Device evidence and remaining natural-gesture / Motorola / Xiaomi checks: DEVICE_TESTS.md. No logo change.

## 1.05 / 105 — 2026-11-14

- PL: Zaległe zmiany 1.04 przetestowano na Pixelu 11 (Android 17, Google TalkBack). Naprawiono utratę fokusu przy zmianie motywu — kolory są aktualizowane bez odtwarzania kontrolek. Wybrana dolna zakładka ma kontrastowy kolor i stan dostępności.
- EN: Validate the pending 1.04 interaction changes on Pixel 11 (Android 17, Google TalkBack). Fix theme-change focus loss by updating colors without recreating controls. Selected bottom tabs have clear color and accessibility state.
- PL/EN: White-key Up/Down, chromatic Left/Right, concise explicit note/microphone feedback, immediate fork refresh after consonance/target/reference changes, valid interval range/order, queued-playback cancellation, preserved acoustic decay guard and transient updater retries/logging.
- PL/EN: Preserve focus when visible sections reflow; group collapsed spinner labels/values into one stop; show Hz beside the slider; fix light/dark system-bar and checkbox-icon contrast without replacing views.
- Final exact-APK device checks passed: focus 183, lifecycle 983, interface 2185, interaction 48, voice events 14, fork 60, audio 7 and screen-off 6. See DEVICE_TESTS.md; natural gestures/human listening and a higher-version GitHub update remain separate checks.

## 1.04 / 104 — candidate, 2026-11-12

- PL: Jeden przycisk mikrofonu, odczyt list, trzy dolne zakładki, instrument na stronie Stroik, suwak Hz, ustawienia głosu i ekranu, usługa do nasłuchu po wygaszeniu ekranu, krótkie nazwy i klawiatura ton/półton. Zmieniony poziom kamertonu. Telefon niedostępny — nowe ścieżki sprzętowe nieprzetestowane.
- EN: Microphone toggle, dropdown values, three bottom tabs, instrument in Tuner, Hz slider, voice/screen options, foreground capture for screen-off use, concise notes and keyboard navigation, fork gain shaping. Phone unavailable; new device paths untested.

## 1.03 / 103

- PL: Stałe kontrolki przy zmianie metody, współbrzmienia, instrumentu i nasłuchu stuknięć; stałe zakładki i przywracanie fokusu po zmianie języka/motywu. Poprawiono etykiety pól liczbowych oraz synchronizację nuty/oktawy bez resetowania kolejności.
- EN: Preserve controls and focus during option/tab changes; restore matching focus after language/theme changes. Fix numeric labels and note/octave synchronization without resetting the order anchor.
- Device: 198 focus assertions, 1103 lifecycle assertions, 1831 interface assertions and 7 audio checks passed. Human speech/gesture and full GitHub updater limits remain documented.
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
