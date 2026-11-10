# Stroik 1.14

[Natywna wersja Android 1.02](https://github.com/proszczok2002/Stroik/releases/tag/android-v1.02) jest dostępna. Zobacz [instrukcję Android](android/README.md) i [raport prób urządzenia](android/DEVICE_TESTS.md). Kanał aktualizacji Windows pozostaje oddzielny.

**Dostępny stroiciel dla Windows x64, zaprojektowany z myślą o osobach niewidomych i słabowidzących.** Oferuje strojenie z mikrofonem lub dźwięk kamertonu, obsługę klawiaturą, komunikaty dla czytników ekranu oraz interfejs po polsku i angielsku.

[English documentation](README.en.md) · [Najnowsze wydanie](https://github.com/proszczok2002/Stroik/releases/latest) · [Historia zmian](CHANGELOG.md)

## Pobieranie

Pobierz `Stroik.exe` z GitHub Releases. Jest to samodzielny plik, który nie wymaga instalacji .NET. Umieść go w folderze z prawem zapisu, aby działała automatyczna aktualizacja. Plik nie jest podpisany cyfrowo, więc Windows może wyświetlić ostrzeżenie SmartScreen. Sumę SHA-256 można porównać z `Stroik.exe.sha256` poleceniem `Get-FileHash .\Stroik.exe -Algorithm SHA256`.

## Metody strojenia

Przy pierwszym uruchomieniu wybrany jest **Kamerton**. Przycisk **Uderz kamerton** odtwarza gasnący ton o częstotliwości wybranego dźwięku. Można też włączyć automatyczne uderzenia i ustawić odstęp od 1 do 10 sekund. To synteza dźwięku zbliżonego do kamertonu widełkowego, a nie próbka prawdziwego kamertonu. Głośność zależy od Windows i głośników. Podczas strojenia ze słuchu na początek warto ćwiczyć na niedrogim, łatwym do obsługi instrumencie, np. ukulele lub gitarze; zerwana struna nadal może zranić.

**Stroik elektroniczny** korzysta z domyślnego mikrofonu Windows. Włącz mikrofon, zagraj jedną nutę i pozwól jej ucichnąć. Po około 0,45 sekundy ciszy wynik pojawi się w polu tekstowym i zostanie zgłoszony przez UI Automation. Dostępne są przyciski powtórzenia wyniku i sprawdzenia komunikatu NVDA bez użycia mikrofonu. Ostatnia metoda jest zapamiętywana przy ponownym uruchomieniu.

### Hybryda i sterowanie kamertonem w 1.13

Lista metod zawiera również **Hybrydowy — głośniki (naprzemiennie)** oraz **Hybrydowy — słuchawki**. Naciśnij **Włącz mikrofon**, aby rozpocząć ćwiczenie. W pierwszym wariancie wzorzec gra przez 3 sekundy; mikrofon jest pomijany przez 3,4 sekundy, potem słucha instrumentu. Po wyciszeniu nuty program ogłasza wynik, pozostawia około 5 sekund na wypowiedź i wraca do wzorca. Długi komunikat NVDA lub pogłos mogą wymagać ręcznego powtórzenia. Zmiana celu uruchamia nowy wzorzec. **Wyłącz mikrofon** kończy cykl.

W wariancie słuchawkowym mikrofon działa równocześnie z kamertonem. **Odsłuch mikrofonu w słuchawkach** jest opcjonalny, domyślnie wyłączony. Najpierw załóż słuchawki: odtwarzanie mikrofonu przez głośniki może spowodować sprzężenie. Odsłuch korzysta z domyślnych urządzeń Windows, nie zmienia ustawień systemowych; opóźnienie zależy od sprzętu i sterowników.

Pole **Dwukrotne stuknięcie przechodzi do kolejnego dźwięku zgodnego z wybraną kolejnością** działa przy kamertonie i hybrydzie. Jego włączenie podczas pracy otwartego okna uruchamia mikrofon; po ponownym uruchomieniu programu użyj **Włącz mikrofon**. Dwa oddzielne ataki w ustawionym czasie wybierają następny cel według planu i odtwarzają jego wzorzec. To gest akustyczny bez sprawdzania wysokości: dwa przypadkowe hałasy też mogą go wywołać. Po własnym tonie kamertonu nasłuch czeka na wybrzmienie. Przy automatycznych uderzeniach i tym polu odstęp wynosi co najmniej 6,5 sekundy, żeby pozostawić czas na gest. **Ctrl+N** pozostaje dostępną metodą ręczną.

### Kwinty i oktawy

Wybierz dolną nutę, następnie w liście **Sprawdź współbrzmienie** wybierz **Kwinty** albo strzałką w dół **Oktawy**. Przykład: c razkreślne → g razkreślne albo c dwukreślne. Opcja **Wyłączone** przywraca zwykły pomiar. Kamerton gra wybraną parę jednocześnie w stroju równomiernym. Gdy górna nuta wykracza poza zakres 88 klawiszy, wybierz niższą nutę.

W pomiarze mikrofonowym domyślnie graj **osobno: dolny, wyciszenie, górny, wyciszenie**. Komunikat potwierdza zapamiętanie dolnego dźwięku. Pomyłka w kolejności nie jest oceniana jako poprawny interwał. Wynik podaje odchyłkę w centach od 700 lub 1200 centów i szacowane dudnienia idealnych harmonicznych: dla kwinty |2×górny − 3×dolny|, dla oktawy |górny − 2×dolny| Hz.

**Dźwięki razem — eksperymentalnie** analizuje około 1,4–1,5 sekundy sygnału; zagraj parę przez około 2 sekundy blisko wybranego celu (zakres poszukiwania ±60 centów). To pomiar dwóch składowych widma, bez gwarancji, że pochodzą z dwóch osobnych strun. Szczególnie przy oktawie harmoniczna dolnej nuty może udawać górną. Potwierdź wynik pomiarem osobnym. Program nie stwierdza na tej podstawie, że cały instrument na pewno nie dudni.

Kwinta równomiernie temperowana może prawidłowo dawać wolne dudnienia; pianino ma nieharmoniczne składowe i może wymagać rozciągniętych oktaw. Szacowane dudnienia z częstotliwości podstawowych nie są pomiarem rzeczywistych dudnień wszystkich składowych. [Wyjaśnienie akustyczne UNSW](https://newt.phys.unsw.edu.au/jw/musFAQ.html).

## Widoki i dźwięki

**Zbyt słabe lub zbyt mocne uderzenia mogą powodować przekłamane odczyty.** Graj równomiernie i unikaj przesterowania mikrofonu. Ta wskazówka jest również w widoku dla początkujących. Od 1.13 lista oktaw jest stale widoczna w pianinie i trybie chromatycznym, zarówno w widoku klasycznym, jak i dla początkujących. Dla pustych strun użyj gotowej listy instrumentu; do innych oktaw przełącz na tryb chromatyczny.

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

W trybie elektronicznym można wybrać ręcznie strunę albo **Automatycznie**. Przy wybranym instrumencie automatyczny wynik porównuje usłyszany dźwięk z najbliższą pustą struną tego instrumentu; przy pianinie i w trybie chromatycznym z najbliższą nutą. Kamerton gra wybrany dźwięk. Dla innych alternatywnych strojów, ukulele z niskim G, pięciostrunowego basu lub gry na progach użyj trybu chromatycznego i ręcznie wybierz dźwięk. Program zapamiętuje instrument oraz zapisuje go w profilach JSON. Starsze profile bez tego pola wczytują się jako pianino.

**Ważne dla pianina:** wybór instrumentu nie mierzy indywidualnej nieharmoniczności strun i nie oblicza rozszerzonego stroju oktaw. Sam wskaźnik częstotliwości nie zastępuje fachowego strojenia całego pianina. [Piano Technicians Guild wyjaśnia strojenie rozszerzone](https://my.ptg.org/communities/community-home/digestviewer/viewthread?CommunityKey=6265a40b-9fd2-4152-a628-bd7c7d770cbf&MessageKey=c2e42d9f-6450-4ec6-a2d3-48f3b6a2ae79&tab=digestviewer).

## Ustawienia i profile

**Ctrl+Q** zamyka program, zatrzymując mikrofon, kamerton i sprawdzanie aktualizacji. Polecenie **Zamknij program (Ctrl+Q)** jest również w menu kontekstowym.

Po metodzie strojenia wybiera się instrument i widok. Dalej są częstotliwość odniesienia A, czułość mikrofonu, próg oceny w centach oraz dźwięk lub struna docelowa. Oktawa jest dostępna dla pianina i trybu chromatycznego. Standard wynosi **A=440 Hz**. Przy starym instrumencie czasem właściwy jest niższy strój, np. 430–435 Hz, ale sama epoka nie wyznacza bezpiecznej wartości. **Przed podniesieniem naciągu starego pianina do 440 Hz poproś technika o ocenę jego konstrukcji, stanu i właściwego stroju.** Nie zakładaj, że każdy instrument z XIX wieku powinien być strojony do 430–435 Hz. [Piano Technicians Guild: zmiana wysokości stroju](https://www.ptg.org/teacher-resources/caring-for-a-piano/pitch-raising) · [PTG: stan starszego instrumentu](https://my.ptg.org/ptgmain/piano/care/care-faqs)

Czułość jest opisana według warunków (cichy instrument, normalna, hałas w tle). Próg „dobrze” jest podawany w centach, a odchyłka także w hercach. Ustawienia są zapisywane lokalnie, w tym język, motyw, metoda, instrument, widok, dźwięk i odstęp kamertonu.

**Ctrl+S** lub **Zapisz plik ustawień** w menu kontekstowym zapisuje profil JSON. **Ctrl+O** lub **Wczytaj plik ustawień** przywraca profil. Możesz utworzyć profile dla domu, kawiarni i instrumentu historycznego. Plik zawiera ustawienia, w tym metodę strojenia; nie zawiera nagrań. Zakładka **Ustawienia** zawiera wybór języka, ciemny motyw i przycisk ręcznego sprawdzenia aktualizacji. Zakładka **Stroik** zawiera pozostałe parametry strojenia oraz profile. Między zakładkami można przechodzić klawiaturą; kontrolki mają polskie i angielskie nazwy dostępności.


## Dwa uderzenia w 1.07

W stroiku elektronicznym po wyniku „dobrze” dwa osobne uderzenia w ten sam dźwięk mogą wybrać następny cel. Pole **Maksymalny odstęp między dwoma uderzeniami** pozwala ustawić od **0,5 do 2,5 sekundy**; nowa wartość domyślna to **1,2 sekundy**. Dłuższy odstęp daje więcej czasu na drugie uderzenie, ale może zwiększyć ryzyko przypadkowego przejścia. Możesz wyłączyć tę funkcję albo przejść ręcznie przez **Ctrl+N**.

Detektor dopuszcza różną głośność uderzeń, lecz oba muszą przekroczyć próg mikrofonu i zostać rozpoznane jako właściwy dźwięk. Jeśli ciche ukulele nadal bywa pomijane, spróbuj ustawienia czułości dla cichego instrumentu. Odczekaj koniec komunikatu o wyniku przed uderzeniami. Ustawiony czas zapisuje się lokalnie i w profilach JSON. Zachowanie z konkretnym mikrofonem, ukulele lub pianinem wymaga próby na sprzęcie.

## Kamerton w 1.06

Gdy **Uderzaj kamerton automatycznie** jest odznaczone, **Alt+U** odtwarza jedno natychmiastowe uderzenie. Tak samo działa przycisk **Uderz kamerton**.

Gdy pole jest zaznaczone, kamerton uderza cyklicznie w ustawionym odstępie. **Alt+U** wstrzymuje albo wznawia cykl bez odznaczania pola. Etykieta pod polem informuje, czy automat działa, jest wstrzymany, czy wybrano tryb ręczny; komunikat jest dostępny także dla czytnika ekranu. Po ponownym uruchomieniu zaznaczony automat znów działa.
## Poprawki w 1.05

W wersji 1.05 dodano skrót Alt+U do kamertonu. Jego obecne działanie opisuje sekcja 1.06 powyżej. Wybór trybu automatycznego jest zapisywany w ustawieniach i profilach.

Aktualizator ponawia chwilowe błędy połączenia i pobieranie przerwane w trakcie przesyłania. Przy starcie wynik sprawdzenia lub błąd jest widoczny w zakładce **Ustawienia**; przycisk **Sprawdź aktualizacje** pozwala ponowić próbę. Przed zamknięciem programu sprawdza prawo zapisu w folderze EXE. Po udanej podmianie nowa wersja ponawia usunięcie starej kopii, jeśli plik był chwilowo zablokowany. Weryfikacja SHA-256 pozostaje obowiązkowa.
## Nowości w 1.04

**Sterowanie bez dotykania komputera.** W stroiku elektronicznym, po uzyskaniu wyniku „dobrze”, odczekaj aż komunikat się skończy. Dwa osobne uderzenia w tę samą strunę lub klawisz, rozpoznane jako ten sam dźwięk, wybierają następny cel. Czas pomiędzy nimi można ustawić zgodnie z opisem wersji 1.07 powyżej. **Ctrl+N** lub przycisk **Następny dźwięk** działa ręcznie. Rozpoznawanie zależy od mikrofonu, głośności i charakteru instrumentu; nie przestawiaj klucza stroikowego bez sprawdzenia wybranej nuty.

**Kolejność pianina.** Do wyboru jest przechodzenie w górę od wybranej nuty albo naprzemienne bloki po pięć półtonów w prawo i w lewo od nuty początkowej. Wariant całotonowy przechodzi co dwa półtony i pomija połowę nut; trzeba je później nastroić osobno. Plan pomaga w nawigacji, ale nie gwarantuje bezpiecznego rozkładu naprężeń ani nie zastępuje [oceny technika i procedury podnoszenia stroju](https://www.ptg.org/teacher-resources/caring-for-a-piano/pitch-raising).

**Przewodnik po klawiaturze.** Przy pianinie opis wskazuje położenie białego lub czarnego klawisza względem grup dwóch i trzech czarnych klawiszy. Po włączeniu grafiki w zakładce **Ustawienia** wirtualna klawiatura oznacza wybrany klawisz kropką, a stroik elektroniczny pokazuje bieżącą odchyłkę na wskaźniku. Gdy aplikacja wykryje czytnik ekranu, grafika jest domyślnie wyłączona; wykrywanie nie jest niezawodne, więc ustawienie można zmienić ręcznie.

**Ukulele i położenie strun.** Standardowy strój z wysokim G to od podłogi A4–E4–C4–G4 (struny 1–4). Wariant z zamienionymi A i G to G4–E4–C4–A4. Wybór jest zapamiętywany w ustawieniach i profilach. Na gitarze od podłogi są E4–H3–G3–D3–A2–E2, więc G to struna 3. [Kala: kolejność strun ukulele](https://kalabrand.com/blogs/home/ukulele-tuning-decoded) · [Fender: numeracja strun gitary](https://www.fender.com/articles/chords/read-guitar-chord-charts)

**Korekta mikrofonu.** W zakładce **Ustawienia** można ręcznie wprowadzić korektę od −100 do +100 centów, po porównaniu ze znanym wzorcem. Wartość dodatnia podwyższa odczyt. To stała korekta pomiaru, nie automatyczna kalibracja mikrofonu. Ustawienie zapisuje się w profilu JSON.
## Aktualizacje

W Ustawieniach można odznaczyć **Automatycznie sprawdzaj aktualizacje** oraz ustawić czas od **1 do 1440 minut** (domyślnie 5). Wyłączenie obejmuje sprawdzenie startowe i okresowe. Przycisk **Sprawdź aktualizacje** działa także po wyłączeniu automatu. Wybory zapisują się lokalnie i w profilach JSON.

Od 1.12 sprawdzanie działa również co 5 minut podczas pracy. W tle nie pojawia się komunikat o braku aktualizacji ani o błędzie połączenia. Okno z opisem zmian pojawia się tylko po wykryciu nowej wersji, raz dla tej wersji na uruchomienie programu. Po odmowie można użyć przycisku „Sprawdź aktualizacje”, aby ponownie otworzyć propozycję. Sprawdzenia nie nakładają się, a timer kończy pracę po zamknięciu aplikacji.

Program sprawdza najnowsze stabilne wydanie GitHub przy uruchomieniu. Po znalezieniu nowszej wersji pokazuje zmiany z opisu wydania po polsku lub angielsku i pyta o zgodę w przewijalnym oknie. Po wybraniu **Tak, pobierz i zainstaluj** pobiera EXE i plik SHA-256, sprawdza sumę, zamyka stary proces, podmienia EXE, uruchamia nowy i usuwa starą kopię. W razie niepowodzenia podmiany próbuje przywrócić poprzedni plik. Przyciski **Sprawdź aktualizacje** i **Ostatnie zmiany** znajdują się w zakładce **Ustawienia**. „Ostatnie zmiany” pokazują opis zainstalowanej wersji także bez internetu. Automatyczna podmiana wymaga nazwy `Stroik.exe` i prawa zapisu w jego folderze. Program nie wysyła nagrań ani wyników do internetu.

W Ustawieniach są też przyciski **O programie**, **Strona Proszczoka**, **Wesprzyj przez PayPal** i **Wesprzyj przez Revolut**. „O programie” wyjaśnia przeznaczenie aplikacji i podaje twórców: wykonawca Proszczok z Codex, pomysłodawca Witold. Pozostałe przyciski otwierają w przeglądarce odpowiednio [proszczok.pl](https://proszczok.pl/), [PayPal](https://paypal.me/proszczok) i [Revolut](https://revolut.me/r_rosiejka). Wsparcie jest dobrowolne, a Stroik nie obsługuje płatności.

Po zmianie pola wyboru myszą lub spacją program zgłasza czytnikowi jego nazwę oraz „zaznaczone” albo „odznaczone”. Działa to dla ciemnego motywu, widoku graficznego, automatycznych uderzeń kamertonu, przejścia po dwóch uderzeniach i wyboru oktawy. Wczytanie ustawień nie powoduje takich komunikatów. Rzeczywistą wypowiedź NVDA trzeba sprawdzić na komputerze użytkownika.

## Ograniczenia i budowanie

W 1.11 poprawiono wykrywanie krótkich i cichnących nut oraz kończenie pomiaru w obecności szumu bez rozpoznanej wysokości. Zmiana celu, instrumentu, stroju odniesienia lub czułości rozpoczyna nowy pomiar bez restartowania mikrofonu. Błąd przechwytywania jest zgłaszany komunikatem z możliwością ponownego włączenia mikrofonu. Sprawdzono sygnały syntetyczne; nie zastępuje to próby w rzeczywistych warunkach.

Pomiar mikrofonem działa w przybliżeniu w zakresie 27,5–4200 Hz i zakłada jeden stabilny dźwięk. Pokazywanie 0,1 Hz nie oznacza gwarantowanej dokładności. Pomiar wstrzymuje się na 1,5 sekundy po wyniku, aby głos czytnika ekranu nie został uznany za kolejną nutę. Działanie z konkretnym mikrofonem, instrumentem, NVDA i głośnikami wymaga sprawdzenia na sprzęcie.

Do budowania potrzebny jest .NET SDK 10 na Windows:

```powershell
dotnet run --project tests/PitchChecks.csproj -c Release
dotnet build Stroik.csproj -c Release
powershell -NoProfile -ExecutionPolicy Bypass -File .\build-release.ps1
```

Skrypt tworzy najnowsze EXE, SHA-256 i ZIP w głównym katalogu `dist`. Po pomyślnym zbudowaniu nowej wersji przenosi poprzednie pliki wydania oraz starsze katalogi kompilacji i weryfikacji do `niepotrzebne` w głównym folderze projektu (`F:\stroik\niepotrzebne`); niczego z tego archiwum automatycznie nie usuwa. Przed następnym wydaniem zwiększ wersję w `Stroik.csproj`. Skrypt chroni istniejące wydanie przed przypadkowym przebudowaniem z tym samym numerem. Parametr `-RebuildSameVersion` służy wyłącznie do świadomego ponownego buildu przed publikacją. Kod źródłowy, testy i lokalny SDK pozostają na miejscu.

Detekcja wysokości używa metody YIN, wejście audio — Windows waveIn, a komunikaty dostępności — UI Automation. Kod jest dostępny na [licencji MIT](LICENSE). Błędy można zgłaszać przez [Issues](https://github.com/proszczok2002/Stroik/issues).
