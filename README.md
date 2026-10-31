# Stroik 1.01

**Dostępny stroiciel dla Windows x64, zaprojektowany z myślą o osobach niewidomych i słabowidzących.** Oferuje strojenie z mikrofonem lub dźwięk kamertonu, obsługę klawiaturą, komunikaty dla czytników ekranu oraz interfejs po polsku i angielsku.

[English documentation](README.en.md) · [Najnowsze wydanie](https://github.com/proszczok2002/Stroik/releases/latest) · [Historia zmian](CHANGELOG.md)

## Pobieranie

Pobierz `Stroik.exe` z GitHub Releases. Jest to samodzielny plik, który nie wymaga instalacji .NET. Umieść go w folderze z prawem zapisu, aby działała automatyczna aktualizacja. Plik nie jest podpisany cyfrowo, więc Windows może wyświetlić ostrzeżenie SmartScreen. Sumę SHA-256 można porównać z `Stroik.exe.sha256` poleceniem `Get-FileHash .\Stroik.exe -Algorithm SHA256`.

## Metody strojenia

Przy pierwszym uruchomieniu wybrany jest **Kamerton**. Przycisk **Uderz kamerton** odtwarza gasnący ton o częstotliwości wybranego dźwięku. Można też włączyć automatyczne uderzenia i ustawić odstęp od 1 do 10 sekund. To synteza dźwięku zbliżonego do kamertonu widełkowego, a nie próbka prawdziwego kamertonu. Głośność zależy od Windows i głośników. Podczas strojenia ze słuchu na początek warto ćwiczyć na niedrogim, łatwym do obsługi instrumencie, np. ukulele lub gitarze; zerwana struna nadal może zranić.

**Stroik elektroniczny** korzysta z domyślnego mikrofonu Windows. Włącz mikrofon, zagraj jedną nutę i pozwól jej ucichnąć. Po około 0,45 sekundy ciszy wynik pojawi się w polu tekstowym i zostanie zgłoszony przez UI Automation. Dostępne są przyciski powtórzenia wyniku i sprawdzenia komunikatu NVDA bez użycia mikrofonu. Ostatnia metoda jest zapamiętywana przy ponownym uruchomieniu.

## Widoki i dźwięki

**Widok dla początkujących** pokazuje wskazówki i wybór oktawy. **Widok klasyczny** zachowuje zwięzły układ; wybór oktawy można w nim włączyć polem przed listą dźwięków. Tab przechodzi do następnej kontrolki, Shift+Tab wraca do poprzedniej. Na liście dźwięków strzałki przechodzą bez przerwy przez granice oktaw. Naciśnij **I** lub **S**, aby przejść do kolejnego dźwięku z krzyżykiem: cis, dis, fis lub gis.

Lista obejmuje 88 klawiszy fortepianu, od A subkontra do c pięciokreślnego. Domyślnie wybrane jest **g w oktawie razkreślnej** (dawne G4). W trybie elektronicznym można też wybrać automatyczne rozpoznanie najbliższej nuty. Polskie H odpowiada angielskiemu B, a polskie B — angielskiemu B-flat. Przy wysokich i bardzo niskich dźwiękach wynik mikrofonu zależy szczególnie od sprzętu i harmonicznych; sprawdzono tony syntetyczne, nie każdy instrument.

## Ustawienia i profile

Kolejność ustawień to częstotliwość odniesienia A, czułość mikrofonu, próg oceny w centach, oktawa i dźwięk docelowy. Standard wynosi **A=440 Hz**. Przy starym instrumencie czasem właściwy jest niższy strój, np. 430–435 Hz, ale sama epoka nie wyznacza bezpiecznej wartości. **Przed podniesieniem naciągu starego pianina do 440 Hz poproś technika o ocenę jego konstrukcji, stanu i właściwego stroju.** Nie zakładaj, że każdy instrument z XIX wieku powinien być strojony do 430–435 Hz. [Piano Technicians Guild: zmiana wysokości stroju](https://www.ptg.org/teacher-resources/caring-for-a-piano/pitch-raising) · [PTG: stan starszego instrumentu](https://my.ptg.org/ptgmain/piano/care/care-faqs)

Czułość jest opisana według warunków (cichy instrument, normalna, hałas w tle). Próg „dobrze” jest podawany w centach, a odchyłka także w hercach. Ustawienia są zapisywane lokalnie, w tym język, motyw, metoda, widok, dźwięk i odstęp kamertonu.

**Ctrl+S** lub **Zapisz plik ustawień** w menu kontekstowym zapisuje profil JSON. **Ctrl+O** lub **Wczytaj plik ustawień** przywraca profil. Możesz utworzyć profile dla domu, kawiarni i instrumentu historycznego. Plik zawiera ustawienia, w tym metodę strojenia; nie zawiera nagrań. Język i ciemny motyw można zmienić w oknie programu.

## Aktualizacje

Program sprawdza najnowsze stabilne wydanie GitHub przy uruchomieniu. Po znalezieniu nowszej wersji pyta o zgodę. Po wybraniu **Tak** pobiera EXE i plik SHA-256, sprawdza sumę, zamyka stary proces, podmienia EXE, uruchamia nowy i usuwa starą kopię. W razie niepowodzenia podmiany próbuje przywrócić poprzedni plik. Dostępny jest również przycisk **Sprawdź aktualizacje**. Automatyczna podmiana wymaga nazwy `Stroik.exe` i prawa zapisu w jego folderze. Program nie wysyła nagrań ani wyników do internetu.

## Ograniczenia i budowanie

Pomiar mikrofonem działa w przybliżeniu w zakresie 27,5–4200 Hz i zakłada jeden stabilny dźwięk. Pokazywanie 0,1 Hz nie oznacza gwarantowanej dokładności. Pomiar wstrzymuje się na 1,5 sekundy po wyniku, aby głos czytnika ekranu nie został uznany za kolejną nutę. Działanie z konkretnym mikrofonem, instrumentem, NVDA i głośnikami wymaga sprawdzenia na sprzęcie.

Do budowania potrzebny jest .NET SDK 10 na Windows:

```powershell
dotnet run --project tests/PitchChecks.csproj -c Release
dotnet build Stroik.csproj -c Release
.\build-release.ps1
```

Skrypt tworzy EXE, SHA-256 i ZIP w katalogu `dist`. Detekcja wysokości używa metody YIN, wejście audio — Windows waveIn, a komunikaty dostępności — UI Automation. Kod jest dostępny na [licencji MIT](LICENSE). Błędy można zgłaszać przez [Issues](https://github.com/proszczok2002/Stroik/issues).
