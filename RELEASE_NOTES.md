# Stroik 1.06 / Stroik 1.06

## Polski

Poprawiono obsługę Alt+U w trybie kamertonu:

- Gdy automatyczne uderzenia są odznaczone, Alt+U wywołuje jedno natychmiastowe uderzenie.
- Gdy automatyczne uderzenia są zaznaczone, kamerton działa cyklicznie, a Alt+U wstrzymuje lub wznawia cykl bez odznaczania pola.
- Pod polem widoczny jest stan: tryb ręczny, automat działający lub wstrzymany. Status jest także dostępny dla czytnika ekranu.

W repozytorium zapisano zasadę przyszłych wydań: starsze EXE, ZIP, sumy i kompilacje trafiają do dist/niepotrzebne; w głównym dist pozostaje najnowsza wersja, a kod źródłowy i testy pozostają dostępne do dalszej pracy.

Pobierz samodzielny, niepodpisany cyfrowo Stroik.exe dla Windows x64. Plik Stroik.exe.sha256 służy do weryfikacji. ZIP zawiera EXE, sumę, instrukcje PL/EN i licencję. Starsze wersje mogą zaproponować aktualizację z GitHub po uruchomieniu.

## English

Alt+U in tuning fork mode now works as follows:

- With automatic strikes unchecked, Alt+U plays one immediate strike.
- With automatic strikes checked, the fork plays repeatedly and Alt+U pauses or resumes the cycle without changing the checkbox.
- A status below the checkbox shows manual, running or paused mode and is available to screen readers.

The repository now records a release rule: archive older EXEs, ZIPs, checksums and build output in dist/niepotrzebne; keep the latest release in the main dist folder and retain editable source and tests.

Download the self-contained, unsigned Stroik.exe for Windows x64. Stroik.exe.sha256 verifies it. The ZIP includes the EXE, checksum, Polish and English guides, and license. Earlier versions can offer a GitHub update at startup.