# Stroik 1.16

## Polski

- Góra/dół na liście pianina i w trybie chromatycznym wybiera kolejne białe klawisze: C–D–E–F–G–A–H–C. Naprawiono E→Fis oraz H→Cis. Lewo/prawo przechodzi o półton, po wszystkich dźwiękach. Działa także na rozwiniętej liście i podczas automatycznego kamertonu.
- Po strzałce oraz Alt+N / Ctrl+N program zgłasza samą nazwę wybranej nuty przez obiekt dostępności listy. Kamerton natychmiast odtwarza nowy cel. Rosnący plan „Następny dźwięk” zachowuje wszystkie półtony; nie jest tym samym co nawigacja po białych klawiszach.
- Gdy górna nuta kwinty lub oktawy wykracza poza zakres, kamerton wstrzymuje automat i podaje wskazówkę zamiast wielokrotnie otwierać dialog błędu. Plan kończy się na ostatniej dostępnej parze. Wybierz niższy cel albo wyłącz współbrzmienia.
- Aktualizator ogranicza czas bezczynnego pobierania, ponawia chwilowe blokady plików i sprawdza SHA-256. Nowy pomocnik przygotowuje EXE przed zamknięciem programu, podmienia plik i czeka na okno nowej wersji. Nieudany restart przywraca poprzedni plik; stara kopia jest usuwana dopiero po udanym uruchomieniu. Szczegóły awarii trafiają do `%LOCALAPPDATA%\Stroik\update.log`.
- Numer wersji w nagłówku i „O programie” jest zgodny z wydaniem.

Sprawdzono programowo nawigację natywnej listy PL/EN, częstotliwości generowanych tonów, granice współbrzmień oraz podmianę, wycofanie po błędzie i ponawianie blokady pliku. NVDA, odsłuch na instrumencie i komputery zgłaszające problem aktualizacji wymagają osobnego sprawdzenia. Wydanie dotyczy Windows.

## English

- Up/Down on piano and chromatic note lists select consecutive white keys: C–D–E–F–G–A–B–C. Fixed E→F-sharp and B→C-sharp. Left/Right move one semitone through all notes. This works on open dropdowns and during automatic fork playback.
- Arrows and Alt+N / Ctrl+N announce just the selected note through the list accessibility object and immediately play its fork tone. The ascending Next note plan still includes every semitone; it is separate from white-key navigation.
- If the upper fifth/octave note exceeds the range, the fork pauses automatic playback and gives guidance instead of repeatedly opening error dialogs. The plan stops at its last valid pair. Choose a lower target or turn consonance off.
- The updater bounds stalled downloads, retries temporary file locks and verifies SHA-256. The new helper stages the EXE before closing the app, replaces it and waits for the new window. Failed restarts restore the previous file; the old backup is removed only after successful startup. Diagnostics: `%LOCALAPPDATA%\Stroik\update.log`.
- Window headings and About now show the actual release version.

Programmatic checks cover native PL/EN list navigation, generated tone frequencies, interval boundaries, replacement, failed-start rollback and sharing-lock retries. NVDA, instrument listening and the affected remote computers require separate validation. This release is for Windows.
