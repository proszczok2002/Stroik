# Stroik 1.03

**Dostępny stroiciel dla Windows x64, zaprojektowany z myślą o osobach niewidomych i słabowidzących.** Oferuje strojenie z mikrofonem lub dźwięk kamertonu, obsługę klawiaturą, komunikaty dla czytników ekranu oraz interfejs po polsku i angielsku.

[English documentation](README.en.md) · [Najnowsze wydanie](https://github.com/proszczok2002/Stroik/releases/latest) · [Historia zmian](CHANGELOG.md)

## Pobieranie

Pobierz `Stroik.exe` z GitHub Releases. Jest to samodzielny plik, który nie wymaga instalacji .NET. Umieść go w folderze z prawem zapisu, aby działała automatyczna aktualizacja. Plik nie jest podpisany cyfrowo, więc Windows może wyświetlić ostrzeżenie SmartScreen. Sumę SHA-256 można porównać z `Stroik.exe.sha256` poleceniem `Get-FileHash .\Stroik.exe -Algorithm SHA256`.

## Metody strojenia

Przy pierwszym uruchomieniu wybrany jest **Kamerton**. Przycisk **Uderz kamerton** odtwarza gasnący ton o częstotliwości wybranego dźwięku. Można też włączyć automatyczne uderzenia i ustawić odstęp od 1 do 10 sekund. To synteza dźwięku zbliżonego do kamertonu widełkowego, a nie próbka prawdziwego kamertonu. Głośność zależy od Windows i głośników. Podczas strojenia ze słuchu na początek warto ćwiczyć na niedrogim, łatwym do obsługi instrumencie, np. ukulele lub gitarze; zerwana struna nadal może zranić.

**Stroik elektroniczny** korzysta z domyślnego mikrofonu Windows. Włącz mikrofon, zagraj jedną nutę i pozwól jej ucichnąć. Po około 0,45 sekundy ciszy wynik pojawi się w polu tekstowym i zostanie zgłoszony przez UI Automation. Dostępne są przyciski powtórzenia wyniku i sprawdzenia komunikatu NVDA bez użycia mikrofonu. Ostatnia metoda jest zapamiętywana przy ponownym uruchomieniu.

## Widoki i dźwięki

**Widok dla początkujących** pokazuje wskazówki właściwe dla wybranego instrumentu. **Widok klasyczny** zachowuje zwięzły układ. Tab przechodzi do następnej kontrolki, Shift+Tab wraca do poprzedniej. Przy pianinie i w trybie chromatycznym dostępny jest wybór oktawy oraz pełna lista dźwięków; strzałki przechodzą przez granice oktaw, a **I** lub **S** wybiera kolejny dźwięk z krzyżykiem. Przy instrumentach strunowych strzałki przechodzą po pustych strunach, a wybór oktawy jest ukryty.

Pianino i tryb chromatyczny obejmują 88 dźwięków od A subkontra do c pięciokreślnego. Domyślnie wybrane jest **pianino i g w oktawie razkreślnej** (G4). Przy wysokich i bardzo niskich dźwiękach wynik mikrofonu zależy szczególnie od sprzętu i harmonicznych; sprawdzono tony syntetyczne, nie każdy instrument.

## Wybór instrumentu

W polu **Instrument** wybierz pianino / fortepian, gitarę, ukulele, skrzypce, gitarę basową albo tryb chromatyczny. Lista dźwięków zmienia się na puste struny w standardowym stroju:

| Instrument | Struny w kolejności listy |
| --- | --- |
| Gitara | E2–A2–D3–G3–H3–E4 (struny 6–1) |
| Ukulele | G4–C4–E4–A4 (struny 4–1, wysokie G) |
| Skrzypce | G3–D4–A4–E5 (struny 4–1) |
| Gitara basowa | E1–A1–D2–G2 (struny 4–1) |

Polskie **H** odpowiada angielskiemu **B**, a polskie **B** — angielskiemu **B-flat**. Listy strun odnoszą się do [standardowej gitary](https://www.fender.com/articles/setup/standard-tuning-how-eadgbe-came-to-be), [ukulele](https://kalabrand.com/pages/ukulele-tuning), [skrzypiec](https://www.yamaha.com/en/musical_instrument_guide/violin/mechanism/mechanism002.html) i [basu](https://www.fender.com/articles/instruments/5-tips-to-move-from-guitar-to-bass).

W trybie elektronicznym można wybrać ręcznie strunę albo **Automatycznie**. Przy wybranym instrumencie automatyczny wynik porównuje usłyszany dźwięk z najbliższą pustą struną tego instrumentu; przy pianinie i w trybie chromatycznym z najbliższą nutą. Kamerton gra wybrany dźwięk. Dla alternatywnych strojów, ukulele z niskim G, pięciostrunowego basu lub gry na progach użyj trybu chromatycznego i ręcznie wybierz dźwięk. Program zapamiętuje instrument oraz zapisuje go w profilach JSON. Starsze profile bez tego pola wczytują się jako pianino.

**Ważne dla pianina:** wybór instrumentu nie mierzy indywidualnej nieharmoniczności strun i nie oblicza rozszerzonego stroju oktaw. Sam wskaźnik częstotliwości nie zastępuje fachowego strojenia całego pianina. [Piano Technicians Guild wyjaśnia strojenie rozszerzone](https://my.ptg.org/communities/community-home/digestviewer/viewthread?CommunityKey=6265a40b-9fd2-4152-a628-bd7c7d770cbf&MessageKey=c2e42d9f-6450-4ec6-a2d3-48f3b6a2ae79&tab=digestviewer).

## Ustawienia i profile

Po metodzie strojenia wybiera się instrument i widok. Dalej są częstotliwość odniesienia A, czułość mikrofonu, próg oceny w centach oraz dźwięk lub struna docelowa. Oktawa jest dostępna dla pianina i trybu chromatycznego. Standard wynosi **A=440 Hz**. Przy starym instrumencie czasem właściwy jest niższy strój, np. 430–435 Hz, ale sama epoka nie wyznacza bezpiecznej wartości. **Przed podniesieniem naciągu starego pianina do 440 Hz poproś technika o ocenę jego konstrukcji, stanu i właściwego stroju.** Nie zakładaj, że każdy instrument z XIX wieku powinien być strojony do 430–435 Hz. [Piano Technicians Guild: zmiana wysokości stroju](https://www.ptg.org/teacher-resources/caring-for-a-piano/pitch-raising) · [PTG: stan starszego instrumentu](https://my.ptg.org/ptgmain/piano/care/care-faqs)

Czułość jest opisana według warunków (cichy instrument, normalna, hałas w tle). Próg „dobrze” jest podawany w centach, a odchyłka także w hercach. Ustawienia są zapisywane lokalnie, w tym język, motyw, metoda, instrument, widok, dźwięk i odstęp kamertonu.

**Ctrl+S** lub **Zapisz plik ustawień** w menu kontekstowym zapisuje profil JSON. **Ctrl+O** lub **Wczytaj plik ustawień** przywraca profil. Możesz utworzyć profile dla domu, kawiarni i instrumentu historycznego. Plik zawiera ustawienia, w tym metodę strojenia; nie zawiera nagrań. Zakładka **Ustawienia** zawiera wybór języka, ciemny motyw i przycisk ręcznego sprawdzenia aktualizacji. Zakładka **Stroik** zawiera pozostałe parametry strojenia oraz profile. Między zakładkami można przechodzić klawiaturą; kontrolki mają polskie i angielskie nazwy dostępności.

## Aktualizacje

Program sprawdza najnowsze stabilne wydanie GitHub przy uruchomieniu. Po znalezieniu nowszej wersji pyta o zgodę. Po wybraniu **Tak** pobiera EXE i plik SHA-256, sprawdza sumę, zamyka stary proces, podmienia EXE, uruchamia nowy i usuwa starą kopię. W razie niepowodzenia podmiany próbuje przywrócić poprzedni plik. Przycisk **Sprawdź aktualizacje** znajduje się w zakładce **Ustawienia**. Automatyczna podmiana wymaga nazwy `Stroik.exe` i prawa zapisu w jego folderze. Program nie wysyła nagrań ani wyników do internetu.

## Ograniczenia i budowanie

Pomiar mikrofonem działa w przybliżeniu w zakresie 27,5–4200 Hz i zakłada jeden stabilny dźwięk. Pokazywanie 0,1 Hz nie oznacza gwarantowanej dokładności. Pomiar wstrzymuje się na 1,5 sekundy po wyniku, aby głos czytnika ekranu nie został uznany za kolejną nutę. Działanie z konkretnym mikrofonem, instrumentem, NVDA i głośnikami wymaga sprawdzenia na sprzęcie.

Do budowania potrzebny jest .NET SDK 10 na Windows:

```powershell
dotnet run --project tests/PitchChecks.csproj -c Release
dotnet build Stroik.csproj -c Release
powershell -NoProfile -ExecutionPolicy Bypass -File .\build-release.ps1
```

Skrypt tworzy najnowsze EXE, SHA-256 i ZIP w głównym katalogu `dist`. Po pomyślnym zbudowaniu nowej wersji przenosi poprzednie pliki wydania oraz starsze katalogi kompilacji i weryfikacji do `dist/niepotrzebne`; niczego z tego archiwum automatycznie nie usuwa. Przed następnym wydaniem zwiększ wersję w `Stroik.csproj`. Skrypt chroni istniejące wydanie przed przypadkowym przebudowaniem z tym samym numerem. Parametr `-RebuildSameVersion` służy wyłącznie do świadomego ponownego buildu przed publikacją. Kod źródłowy, testy i lokalny SDK pozostają na miejscu.

Detekcja wysokości używa metody YIN, wejście audio — Windows waveIn, a komunikaty dostępności — UI Automation. Kod jest dostępny na [licencji MIT](LICENSE). Błędy można zgłaszać przez [Issues](https://github.com/proszczok2002/Stroik/issues).
