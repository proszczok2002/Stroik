# Stroik 1.11 / Stroik 1.11

## Polski

- Poprawiono zbieranie krótkich i cichnących dźwięków: pomiar wysokości odbywa się w każdym pełnym oknie analizy, zamiast co trzecie okno.
- Szum bez wiarygodnie rozpoznanej wysokości nie przedłuża już bez końca oczekiwania na zakończenie nuty. Usunięto też wpływ stałego przesunięcia sygnału mikrofonu na próg głośności.
- Ręczna zmiana celu, instrumentu, stroju odniesienia lub czułości rozpoczyna świeży pomiar. Stary wynik oczekujący na wyświetlenie nie zastąpi wyniku dla nowego celu.
- Bufor mikrofonu wraca do sterownika przed analizą dźwięku. Błąd przetwarzania lub ponownego kolejkowania bufora jest zgłaszany zamiast pozostawiać program w pozornie działającym stanie.

Nadal wybierz odpowiednią czułość mikrofonu, graj pojedynczy dźwięk i pozwól mu ucichnąć. Po wypowiedzeniu wyniku obowiązuje przerwa około 1,5 sekundy, chroniąca przed pomiarem głosu czytnika. Zmiany przetestowano na sygnałach syntetycznych; działanie z konkretnym mikrofonem, instrumentem i hałasem wymaga sprawdzenia na sprzęcie.

## English

- Improved collection of short and decaying notes: pitch is measured in every full analysis window rather than every third window.
- Noise without a reliable pitch no longer keeps a note open indefinitely. A constant microphone signal offset no longer raises the loudness gate.
- Manually changing the target, instrument, reference pitch or sensitivity starts a fresh measurement. A queued old result cannot overwrite the new target's result.
- Microphone buffers return to the driver before pitch analysis. Processing and buffer requeue errors are reported instead of leaving capture apparently running.

Choose suitable microphone sensitivity, play one note and let it stop. A pause of about 1.5 seconds after a result still protects against measuring the screen reader's voice. The changes were tested with synthetic signals; actual microphones, instruments and background noise still require hardware testing.
