# Stroik 1.19

## Polski

Stroik 1.19

- Widok klasyczny nie pokazuje wskazówek strojenia, opisu standardu 440 Hz ani pozostałości porad pianinowych po wyborze innego instrumentu. Pole ma krótką etykietę Częstotliwość, Hz.
- Hybryda pokazuje sterowanie mikrofonem i własne pole gestu dwóch stuknięć. Ręczne uderzenie i automat kamertonu są tylko w metodzie Kamerton. Elektroniczne przechodzenie po poprawnym wyniku działa wyłącznie w stroiku elektronicznym. Rozstrojone uderzenia resetują uzbrojony gest; oba ataki muszą zmieścić się w wybranej tolerancji i mieć świeże potwierdzenie wysokości.
- Shift+Alt+N lub Shift+Ctrl+N wraca do poprzedniego dźwięku według planu. Alt+N i Ctrl+N przechodzą dalej. Koniec planu zatrzymuje aktywne strojenie.
- Poprawiono polskie nazwy głośników, słuchawek i sposobów pomiaru. Wpis liczbowy przyjmuje kropkę i przecinek. Komunikaty używają powiadomień czytnika; przy braku wykrytego czytnika dodano asynchroniczny głos SAPI. Hybryda czeka na zakończenie własnej mowy SAPI.
- Niskie tony kamertonu mają większy poziom oraz ciche wyższe składowe pomocne małym głośnikom. Kwinty nadal grają właściwe nuty równocześnie: c razkreślne + g razkreślne to 261,63 i 392,00 Hz przy A=440. Wygładzono zakończenie tonu. Wrażenie oktawy i słyszalność basu zależą także od głośników; nie przesuwamy wzorca do innej oktawy.
- Klawisz na wizualnej klawiaturze jest czerwony przy rozstrojeniu, zielony w wybranej tolerancji, niebieski przed pomiarem. Kolor uzupełnia wynik tekstowy i wskaźnik.

Testy: PL/EN, cyfry i separatory dziesiętne, skróty, widoczność, pięć kolejnych wyników hybrydy, ścisła tolerancja gestu, widmo kwint w pięciu oktawach, SAPI start/koniec oraz dotychczasowe regresje audio/updatera. Nie potwierdzono odsłuchu NVDA ani prawdziwego instrumentu u zgłaszającego.

## English

Stroik 1.19

- Classic view hides tuning advice, the 440 Hz standard hint and stale piano guidance after instrument changes. The reference field is labelled Frequency, Hz.
- Hybrids show microphone controls and their acoustic double-strike option. Manual and automatic fork controls belong only to Fork. In-tune confirmed navigation belongs only to Electronic. Detuned attacks cancel an armed gesture; both attacks must meet the configured tolerance with fresh pitch confirmation.
- Shift+Alt+N and Shift+Ctrl+N select the previous planned note; Alt+N and Ctrl+N advance. Plan completion stops active tuning.
- Repair Polish speaker/headphone/interval labels. Numeric entry accepts either decimal separator. Reader notifications are retained; asynchronous SAPI speech is available without a detected screen reader. Speaker hybrid waits for its own SAPI speech to finish.
- Stronger bass levels with quiet upper partials help small speakers. Fifths still play the selected pair together: C4+G4 is 261.63+392.00 Hz at A=440. Tone endings fade smoothly. Bass audibility and perceived octave still depend on speakers; the reference is not transposed.
- The visual target key is red when detuned, green within the selected tolerance and blue before a reading. Text and the meter continue to report the result.

Validation: PL/EN, typed digits and decimal separators, shortcuts, visibility, five consecutive hybrid results, strict attack tolerance, five-octave fifth spectra, actual SAPI start/completion and existing audio/updater regression. Human NVDA listening and the reporter's real instrument remain unverified.
