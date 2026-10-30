# Stroik 1.0

**Dostępny stroiciel instrumentów dla Windows, zaprojektowany z myślą o osobach niewidomych i słabowidzących.** Program działa z klawiaturą i udostępnia wyniki czytnikom ekranu, w tym NVDA. Mierzy wysokość pojedynczego dźwięku z domyślnego mikrofonu Windows i podaje nutę, kierunek strojenia oraz odchyłkę w hercach.

[English documentation](README.en.md) · [Wydania](https://github.com/proszczok2002/Stroik/releases)

## Pobieranie i uruchamianie

Pobierz `Stroik.exe` z [najnowszego wydania](https://github.com/proszczok2002/Stroik/releases/latest). To samodzielny plik dla Windows x64; nie wymaga instalacji .NET. Umieść go w folderze, do którego masz prawo zapisu, i uruchom. Windows może wyświetlić ostrzeżenie SmartScreen dla niepodpisanego pliku.

Wydanie zawiera także `Stroik.exe.sha256`. Sumę pobranego pliku można sprawdzić poleceniem `Get-FileHash .\Stroik.exe -Algorithm SHA256` w PowerShell.

## Strojenie z czytnikiem ekranu

1. Włącz NVDA i uruchom `Stroik.exe`.
2. Naciśnij **Włącz mikrofon**. Program korzysta z domyślnego wejścia audio Windows.
3. Zagraj jedną nutę, po czym pozwól jej ucichnąć. Po około 0,45 sekundy ciszy wynik zostanie umieszczony w polu tekstowym i zgłoszony przez UI Automation.
4. **Powtórz wynik** ponownie zgłasza ostatnią nutę. **Sprawdź komunikat NVDA** sprawdza samo powiadomienie bez mikrofonu.
5. Do wszystkich elementów można przechodzić klawiszem Tab; przyciski mają skróty z klawiszem Alt.

Można wybrać konkretny **dźwięk docelowy** albo wykrywać najbliższą nutę automatycznie. A4 domyślnie wynosi 440 Hz. Próg „dobrze” jest podawany w centach, a komunikat w hercach. Czułość mikrofonu pomaga przy cichych instrumentach lub hałasie. Polskie H odpowiada angielskiemu B, a polskie B angielskiemu B-flat.

W ustawieniach można przełączyć **Polski / English** i **ciemny motyw**. Wybór jest zapisywany w lokalnym profilu użytkownika. Na obsługiwanych wersjach Windows ciemny motyw obejmuje także pasek tytułu; wygląd natywnej listy rozwijanej może zależeć od ustawień Windows.

## Aktualizacje przez GitHub

Przy uruchomieniu Stroik sprawdza najnowsze stabilne wydanie na GitHubie. Gdy znajdzie nowszą wersję, pokazuje okno z pytaniem. Po wybraniu **Tak** pobiera `Stroik.exe` i jego plik SHA-256 z tego samego wydania, weryfikuje sumę, zamyka program, zastępuje dotychczasowe EXE i uruchamia nową wersję. Stary plik jest usuwany po udanej podmianie. Jeśli podmiana się nie uda, program próbuje przywrócić poprzedni plik. Dostępny jest także przycisk **Sprawdź aktualizacje**.

Automatyczna podmiana wymaga, aby plik nazywał się `Stroik.exe` i znajdował się w folderze z prawem zapisu. Przy pracy offline strojenie nadal działa. Program pobiera wyłącznie metadane wydania i, po zgodzie, pliki aktualizacji; nie wysyła nagrań ani wyników do sieci.

## Zakres i ograniczenia

Zakres pomiaru to około 55–1200 Hz. Program zakłada jeden względnie stabilny dźwięk. Wyświetlenie 0,1 Hz oznacza rozdzielczość wyniku, nie gwarantowaną dokładność. Dokładność zależy od mikrofonu, instrumentu i warunków akustycznych. Po ogłoszeniu wyniku pomiar jest wstrzymywany na 1,5 sekundy, aby głos czytnika z głośników nie został uznany za nową nutę. Słuchawki mogą pomóc.

Testy automatyczne korzystają z generowanych tonów. Działanie z konkretnym mikrofonem, instrumentem i wersją NVDA wymaga osobnej próby na sprzęcie.

## Budowanie ze źródeł

Potrzebny jest .NET SDK 10 na Windows. W katalogu projektu:

```powershell
dotnet run --project tests/PitchChecks.csproj -c Release
dotnet build Stroik.csproj -c Release
.\build-release.ps1
```

Skrypt tworzy `dist/Stroik.exe`, `dist/Stroik.exe.sha256` i archiwum ZIP. Każde kolejne wydanie musi mieć zwiększoną wersję w `Stroik.csproj`, tag `vX.Y.Z` oraz oba pliki EXE i SHA-256 w GitHub Releases, aby aktualizator je wykrył.

Detektor wykorzystuje metodę YIN opisaną przez de Cheveigné i Kawaharę w „YIN, a fundamental frequency estimator for speech and music”, *JASA* 2002. Wejście audio korzysta z Windows `waveIn`, a powiadomienia czytnika z UI Automation.

Kod jest udostępniony na [licencji MIT](LICENSE). Zgłoszenia błędów i pomysły są mile widziane w [Issues](https://github.com/proszczok2002/Stroik/issues).
