# Stroik Android 1.05

## Polski

Stroik Android 1.05

Trzy zakładki w jednym rzędzie na dole: Stroik, Ustawienia i Pomoc. Instrument wybierasz z listy na stronie Stroik. Wybrana zakładka ma wyraźny kolor i stan dostępności. Widok klasyczny lub dla początkujących jest przed metodą strojenia.

Jeden przycisk Włącz/Wyłącz mikrofon podaje aktualny stan. Listy udostępniają bieżący wybór bez otwierania; osobne etykiety nie są kolejnymi elementami nawigacji TalkBack. Współbrzmienia pozostają w tym samym miejscu, a zmiana motywu zachowuje istniejące kontrolki i fokus.

W Ustawieniach można wyłączyć odczyt wyników w trybie wizualnym (domyślnie włączony), włączyć utrzymywanie ekranu oraz ustawić aktualizacje. Pierwsza wskazówka przy grafice pokazuje, gdzie jest opcja głosu. Ręcznie wywołana zmiana nuty lub stanu mikrofonu nadal ma komunikat. Przy wygaszonym ekranie wyniki odczytuje syntezator systemowy. Nasłuch uruchamia użytkownik; działa on w usłudze z powiadomieniem i przyciskiem zatrzymania. Usunięcie aplikacji z ostatnich zadań kończy nasłuch.

Częstotliwość A: suwak 390–466 Hz, krok dostępności 0,1 Hz; podwójne stuknięcie lub Enter otwiera pole liczbowe. Z klawiatury na liście nut góra/dół wybiera białe klawisze C D E F G A H C; lewo/prawo wszystkie półtony. Alt+N / Ctrl+N wybiera następny cel według planu. Alt+U powtarza nutę w trybie ręcznym albo wstrzymuje/wznawia zaznaczony automat.

Kwinta od c razkreślnego gra jednocześnie c razkreślne i g razkreślne; oktawa c razkreślne i c dwukreślne. Wybrana nuta jest dolną nutą pary, dlatego kwinta od g razkreślnego to g razkreślne i d dwukreślne. Zmiana współbrzmienia, nuty, oktawy lub A od razu odświeża dźwięk. Poza zakresem kamerton wstrzymuje automat zamiast powtarzać błąd. Korekta głośności wzmacnia dół i łagodzi górę; głośnik nadal ma wpływ na brzmienie.

Aktualizator ponawia chwilowe błędy sieci, sprawdza sumę SHA-256, pakiet, wersję i podpis, a instalację zatwierdzasz w systemowym oknie Androida. Nie zmienia to kanału Windows. Testy dostępności kontrolek, audio i cyklu życia są odrębne od odsłuchu TalkBack przez człowieka oraz strojenia prawdziwego instrumentu. Szczegółowy zakres prób jest w android/DEVICE_TESTS.md.

## English

Stroik Android 1.05

Three bottom tabs in one row: Tuner, Settings and Help. Instrument is a dropdown on Tuner. The selected tab has clear color and accessibility state. Classic or Beginner view precedes tuning method.

One Start/Stop microphone button exposes its current state. Collapsed lists expose the selected value; separate labels are skipped during TalkBack navigation. Consonance stays in place. Theme changes preserve existing controls and focus.

Settings includes spoken results in visual mode (on by default), keep screen awake and update preferences. The first graphics hint points to the speech option. Explicit note and microphone actions still give feedback. With the screen off, results use system TTS. Listening is user-started and runs in a foreground service with a stop notification. Removing the app from Recents stops capture.

Concert A: 390–466 Hz slider, 0.1 Hz accessibility adjustment; double tap or Enter opens numeric entry. On the note list, keyboard Up/Down select white keys C D E F G A B C; Left/Right move through all semitones. Alt+N / Ctrl+N advances according to the plan. Alt+U repeats a manual strike or pauses/resumes selected automatic playback.

A fifth from C4 simultaneously plays C4 and G4; an octave plays C4 and C5. The selected note is the lower note, so a fifth from G4 is G4 and D5. Changing consonance, note, octave or concert A immediately refreshes playback. Invalid upper notes pause the fork instead of repeating errors. Bounded gain strengthens low and softens high notes; speaker response still matters.

The updater retries temporary network failures and verifies SHA-256, package, version and certificate. Android's system installer asks for confirmation. Windows uses a separate channel. Structural accessibility, audio and lifecycle checks are separate from human TalkBack listening and real-instrument accuracy. See android/DEVICE_TESTS.md for the exact evidence.

## Weryfikacja / Verification

PL: Końcowy APK zainstalowano i sprawdzono na Pixelu 11 z Androidem 17 oraz Google TalkBack. Przeszły testy fokusu PL/EN, cyklu życia, dostępności kontrolek i komunikatów, kamertonu, mikrofonu oraz działania i TTS przy wygaszonym ekranie. Naprawiono także fokus po zmianie wysokości sekcji i wizualny znacznik zaznaczenia motywu. Naturalne gesty/odsłuch człowieka, strojenie prawdziwego instrumentu i pełna aktualizacja z GitHuba do wyższej wersji pozostają osobnymi próbami. Szczegóły: android/DEVICE_TESTS.md.

EN: The final APK was installed and checked on Pixel 11 with Android 17 and Google TalkBack. PL/EN focus, lifecycle, accessible controls/events, fork, microphone and screen-off capture/TTS checks passed. This also fixes focus after section reflow and the visible theme checkmark. Natural gestures/human listening, real-instrument tuning and a complete GitHub update to a higher version remain separate validation. See android/DEVICE_TESTS.md.
