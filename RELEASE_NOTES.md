# Stroik 1.17

## Polski

- Naprawiono pozostawianie starego dźwięku po zmianie współbrzmienia kamertonu. Wybranie kwinty lub oktawy od razu odtwarza oba dźwięki razem, także przy wyłączonym automacie i mikrofonie. Wybranie „Wyłączone” natychmiast wraca do pojedynczej nuty.
- Zmiana nuty bezpośrednio na liście, zmiana oktawy i częstotliwości odniesienia również od razu odświeżają wzorzec. Zachowano obsługę strzałek i „Następnego dźwięku” bez podwójnego uderzania.
- Opis przy współbrzmieniu oraz opis dostępności listy wskazują obie nuty. Wybrana nuta jest dolną nutą pary: dla c razkreślnego kwinta to c razkreślne + g razkreślne (261,63 + 392,00 Hz przy A=440). Dla g razkreślnego kwinta w górę to g razkreślne + d dwukreślne.
- Aby ponownie uderzyć tę samą nutę, odznacz automat i naciśnij Alt+U lub „Uderz kamerton”. Pomysł osobnej zakładki do grania melodii zapisano do późniejszego zaprojektowania; nie dodano jej w tej poprawce.

Testy sygnału PCM po rzeczywistych zdarzeniach interfejsu obejmują kwinty, oktawy, wyłączenie współbrzmienia, A=435/440 i ręczny/automatyczny kamerton. Nie wykonano odsłuchu na komputerze zgłaszającym problem ani testu NVDA. Poprawka dotyczy Windows.

## English

- Fixed stale fork audio after changing consonance. Selecting fifths or octaves immediately plays both notes together, including manual mode without microphone capture. Selecting Off immediately restores a single note.
- Direct note-list selection, octave changes and concert-A changes also refresh playback immediately. Arrow and Next note navigation remain supported without duplicate strikes.
- Consonance help and the list accessibility description name both notes. The selected note is the lower note: C4 gives C4 + G4 (261.63 + 392.00 Hz at A=440); G4 gives G4 + D5 for an upward fifth.
- To repeat the same note, clear automatic strikes and press Alt+U or Strike tuning fork. The separate piano-tab suggestion has been recorded for later design; it is not part of this fix.

PCM tests after actual UI events cover fifths, octaves, Off, A=435/440 and manual/automatic fork modes. Listening on the affected computer and NVDA were not tested. This is a Windows fix.
