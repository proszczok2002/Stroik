# Stroik 1.15

## Polski

- Naprawiono napis „Wyłączone” w liście współbrzmień.
- Alt+N oraz Ctrl+N przechodzą do następnego dźwięku w kamertonie; nowy cel jest od razu odtwarzany. Komunikat aplikacji zawiera samą nazwę nuty, np. „g razkreślne”. Czytnik ekranu nadal opisuje kontrolki przy nawigacji.
- Wybór widoku jest przed metodą strojenia. Na zamkniętej liście dźwięków strzałki góra/dół zmieniają cel o cały ton (2 półtony), lewo/prawo o półton; otwarta lista zachowuje zwykłą nawigację.
- W Ustawieniach dodano domyślnie włączony „Odczyt głosowy w trybie wizualnym”. Odznaczenie wycisza komunikaty aplikacji w widoku graficznym, bez wyłączania nawigacji NVDA.
- Kamerton ma ograniczoną korektę poziomu zależną od częstotliwości: mocniejszy dół, łagodniejsza góra. Nie zmienia wysokości tonów ani częstotliwości interwałów. Efekt akustyczny zależy od głośnika; nie obiecujemy identycznej odczuwanej głośności.
- Opisy położenia klawiszy pozostają dostępne przy pianinie. Kwinty są budowane o 7 półtonów w górę od wskazanej nuty, oktawy o 12: C4–G4 oraz C4–C5. G4–D5 jest również kwintą; para przekracza granicę nazwanych oktaw. Nie zastępujemy jej kwartą przez zawijanie nut.

Testy programowe i kontrole okna Windows przeszły. Odsłuch kamertonu, rzeczywiste instrumenty i NVDA nie były ponownie testowane w tym wydaniu. Android ma osobny kanał; kandydat Android 1.04 wymaga testu telefonu, którego obecnie nie ma.

## English

- Fixed the corrupted Polish Off label in the consonance list.
- Alt+N and Ctrl+N advance in fork mode and immediately play the new target. The app announces just the note name. Screen reader navigation still describes controls.
- View selection precedes tuning method. On the closed note list, Up/Down move a whole tone (2 semitones), Left/Right one semitone. The open dropdown retains normal list navigation.
- Added Spoken feedback in visual mode, enabled by default, under Settings. Clearing it suppresses app announcements in graphics mode without disabling NVDA navigation.
- Bounded frequency-dependent fork levels strengthen low notes and soften high notes without shifting pitch or interval frequencies. Actual loudness depends on the speaker; equal perceived loudness is not guaranteed.
- Piano key-position descriptions remain available. Fifths are 7 semitones upward and octaves 12: C4–G4 and C4–C5. G4–D5 is also a fifth, crossing named octaves; wrapping notes must not turn it into a fourth.

Programmatic checks and Windows window tests passed. Fork listening, real instruments and NVDA were not retested for this release. Android uses a separate channel; Android 1.04 is a candidate awaiting a currently unavailable phone.