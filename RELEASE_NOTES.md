# Stroik 1.0 / Stroik 1.0

## Polski

Pierwsze publiczne wydanie Stroika: dostępnego stroiciela instrumentów dla Windows x64, zaprojektowanego z myślą o osobach niewidomych i słabowidzących. Wynik trafia do pola tekstowego i jest zgłaszany czytnikom ekranu przez UI Automation. Program oferuje interfejs po polsku i angielsku, ciemny motyw, wybór nuty docelowej, regulację A4, progu strojenia i czułości mikrofonu.

Stroik sprawdza nowe stabilne wydania na GitHubie przy uruchomieniu. Po potwierdzeniu pobiera EXE i sumę SHA-256, weryfikuje plik, podmienia poprzednią wersję oraz usuwa stary plik. Program nie wysyła nagrań ani wyników do sieci.

Pobierz `Stroik.exe` (bez instalatora) oraz `Stroik.exe.sha256`. Archiwum ZIP zawiera także dokumentację PL/EN i licencję. Do automatycznych aktualizacji umieść EXE w folderze z prawem zapisu. Windows może ostrzec o niepodpisanym pliku.

Testy automatyczne obejmują wygenerowane dźwięki i analizę metadanych aktualizacji. Rzeczywisty mikrofon, instrument i odczyt NVDA nie były potwierdzone testem sprzętowym.

## English

The first public release of Stroik: an accessible Windows x64 instrument tuner designed for blind and visually impaired musicians. Results are available in a text field and announced to screen readers through UI Automation. It includes Polish and English interfaces, a dark theme, target-note selection, A4 reference tuning, an in-tune threshold, and microphone sensitivity settings.

Stroik checks for new stable GitHub releases at startup. With your confirmation, it downloads the EXE and SHA-256 file, verifies the download, replaces the previous version, and removes the old file. It does not upload recordings or tuning results.

Download `Stroik.exe` (no installer) and `Stroik.exe.sha256`. The ZIP also contains PL/EN documentation and the license. Store the EXE in a writable folder for automatic updates. Windows may warn about an unsigned executable.

Automated tests cover generated tones and update metadata parsing. Real microphone, instrument, and NVDA speech behavior have not been confirmed with hardware testing.
