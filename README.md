# Stroik 1.26

- Przy pierwszym pobraniu wybierz pełny ZIP Windows. Rozpakuj go do jednego folderu, aby od razu mieć Stroik.exe, Stroik-Kalibrator.exe, instrukcje i sumy kontrolne.
- Jeśli kalibratora brakuje lub jest starszy, Stroik przy uruchomieniu pyta o pobranie. Po odmowie nie ponawia pytania o brakujący program ani o tę samą wersję. Możesz wyłączyć **Sprawdzaj kalibrator przy uruchomieniu** i zawsze użyć **Sprawdź kalibrator teraz** w Ustawieniach.
- Po potwierdzeniu Stroik sprawdza rozmiar, SHA-256 i wersję, po czym zapisuje kalibrator obok swojego EXE. Kalibrator 1.0.1 pokazuje własną wersję w tytule okna.

- Stroik zapisuje ustawienia w `Stroik.settings.json` obok `Stroik.exe`. Logi, pliki aktualizatora i biblioteka NVDA trafiają do `Stroik-data`. Aby przenieść program i dane, skopiuj cały folder.
- Przy pierwszym uruchomieniu Stroik zachowuje dotychczasowy profil, mapę instrumentu i kalibrację. Zawartość `%LOCALAPPDATA%\Stroik` trafia do kopii w `Stroik-data\migracja-*`. Jeśli ustawienia są już obok EXE, zachowują pierwszeństwo. Stroik usuwa stary katalog dopiero po sprawdzeniu kopii.
- Rozpakuj ZIP do zapisywalnego folderu. Jeśli migrację przerwie brak dostępu lub blokada pliku, Stroik zachowa dane źródłowe wymagające dokończenia migracji i ponowi próbę przy następnym starcie.
- Jeśli sam plik ustawień w AppData jest chwilowo zablokowany, Stroik poczeka z utworzeniem nowego profilu. Po odblokowaniu pliku i ponownym uruchomieniu zachowa dotychczasowe ustawienia.

- W hybrydzie głośnikowej włącz opcję dwóch stuknięć. Po komunikacie o dobrym stroju kamerton pozostawia ciche okno na dwa krótkie stuknięcia. Nie trzeba dwukrotnie zagrać czystej nuty. Bez gestu kamerton powtarza ten sam wzorzec. Po złym wyniku obowiązuje ustawiona przerwa od 0 do 60 sekund.
- Dla pianin powojennych A4 = 440 Hz to zwykły punkt odniesienia. Przedwojenne instrumenty mogły być przewidziane do niższego stroju. Przed podnoszeniem stroju starego pianina warto uzyskać ocenę technika.
- W zakładce **Mapowanie instrumentu** wybierz oktawę, nutę i od 1 do 10 uderzeń. Kliknij **Rozpocznij mapowanie**, po każdym uderzeniu pozwól nucie ucichnąć i poczekaj na komunikat. Stroik przypisuje średnią częstotliwość w hercach do wybranej nuty. Po zebraniu całej serii wynik jest zapamiętany. Przejdź do następnej nuty listą, przyciskiem albo dwoma stuknięciami. **Zakończ mapowanie** zatrzymuje mikrofon. **Zapisz plik mapingu** (Ctrl+S) zapisuje JSON. **Użyj mapy do strojenia** włącza ją dla miernika i kamertonu. Nuty spoza mapy korzystają ze zwykłego stroju. Ten sam plik otworzysz na obu systemach.
- **Stroik-Kalibrator.exe** ma jedną listę 88 nut. Suwaki zapisują harmoniczne (0–200% wartości Stroika) i głośność (0–150%) oddzielnie dla każdej nuty. 100% to punkt wyjścia, a 0% harmonicznych daje czysty ton. Alt+U odtwarza nutę, Ctrl+S zapisuje JSON. W Stroiku na obu systemach wybierz **Ustawienia → Wczytaj kalibrację kamertonu**. Kalibracja zmienia barwę i poziom, a mapa instrumentu zmienia częstotliwości.
- W ustawieniach są osobne suwaki głośności kamertonu i odsłuchu z mikrofonu. Na Windows odpowiadają im także osobne sesje aktywnych źródeł w mikserze systemowym. Obie aplikacje korzystają z domyślnego wyjścia audio. Odsłuch mikrofonu uruchamiaj w słuchawkach.

- A kontra otrzymało takie samo wsparcie harmonicznych jak niższe nuty. Obie platformy używają tej samej barwy bez zmiany oktawy. Słyszalność basu nadal zależy od głośników.
- Klasyczna hybryda uruchamia kamerton od razu, bez samouczka. W widoku dla początkujących najpierw czytane są wskazówki. Działanie Windows zachowano i dodano je w Androidzie.
- Przerwa po wyniku wynosi 0–60 sekund, także 1 lub 2. Zero oznacza powrót wzorca od razu po zakończeniu mowy i jest nowym ustawieniem domyślnym. Zapisane przerwy Windows są zachowane. Android ma tę samą regulację z suwakiem i wpisem liczby.
- Android: klasyczna lista bez opisów położenia klawiszy, opisy metod dla początkujących oraz czarne klawisze strzałkami lewo/prawo. Wyniki i samouczek czyta syntezator aplikacji także przy TalkBack, aby znać koniec mowy; TalkBack nadal obsługuje nawigację.

Aktualne zmiany Windows 1.26 opisano powyżej; starsze sekcje wersji są historyczne.

[Natywna wersja Android 1.10](https://github.com/proszczok2002/Stroik/releases/tag/android-v1.10) jest dostępna. Zobacz [instrukcję Android](android/README.md) i [raport prób urządzenia](android/DEVICE_TESTS.md). Kanał aktualizacji Windows pozostaje oddzielny.

**Dostępny stroiciel dla Windows x64, zaprojektowany z myślą o osobach niewidomych i słabowidzących.** Oferuje strojenie z mikrofonem lub dźwięk kamertonu, obsługę klawiaturą, komunikaty dla czytników ekranu oraz interfejs w językach PL, EN, DE, RU, FR i ES.

[English documentation](README.en.md) · [Najnowsze wydanie](https://github.com/proszczok2002/Stroik/releases/latest) · [Historia zmian](CHANGELOG.md)

[Deutsch](README.de.md) · [Русский](README.ru.md) · [Français](README.fr.md) · [Español](README.es.md)

## Pobieranie

Przy pierwszym pobraniu wybierz pełny ZIP w GitHub Releases. Jeśli potrzebujesz samego stroika, możesz pobrać `Stroik.exe` osobno. Jest to samodzielny plik, który nie wymaga instalacji .NET. Umieść go w folderze z prawem zapisu, aby działała automatyczna aktualizacja. Plik nie jest podpisany cyfrowo, więc Windows może wyświetlić ostrzeżenie SmartScreen. Sumę SHA-256 można porównać z `Stroik.exe.sha256` poleceniem `Get-FileHash .\Stroik.exe -Algorithm SHA256`.

## Metody strojenia

Przy pierwszym uruchomieniu wybrany jest **Kamerton**. Przycisk **Uderz kamerton** odtwarza gasnący ton o częstotliwości wybranego dźwięku. Można też włączyć automatyczne uderzenia i ustawić odstęp od 1 do 10 sekund. To synteza dźwięku zbliżonego do kamertonu widełkowego, a nie próbka prawdziwego kamertonu. Głośność zależy od Windows i głośników. Podczas strojenia ze słuchu na początek warto ćwiczyć na niedrogim, łatwym do obsługi instrumencie, np. ukulele lub gitarze; zerwana struna nadal może zranić.

**Stroik elektroniczny** korzysta z domyślnego mikrofonu Windows. Włącz mikrofon, zagraj jedną nutę i pozwól jej ucichnąć. Po około 0,45 sekundy ciszy wynik pojawi się w polu tekstowym i zostanie odczytany przez NVDA lub syntezator aplikacji. Dostępne są przyciski powtórzenia wyniku i sprawdzenia komunikatu NVDA bez użycia mikrofonu. Ostatnia metoda jest zapamiętywana przy ponownym uruchomieniu.

### Hybryda i sterowanie kamertonem w 1.13

Lista metod zawiera również **Hybrydowy — głośniki (naprzemiennie)** oraz **Hybrydowy — słuchawki**. Naciśnij **Włącz mikrofon**, aby rozpocząć ćwiczenie. W pierwszym wariancie wzorzec gra przez 3 sekundy; mikrofon jest pomijany przez 3,4 sekundy, potem słucha instrumentu. Po wyciszeniu nuty program ogłasza wynik, czeka na zakończenie mowy SAPI lub NVDA 2026.3+ i wybraną przerwę (0–60 s, domyślnie 0; zapisane przerwy są zachowane). W widoku początkującym najpierw odczytuje wskazówki; synchroniczne API wymaga NVDA 2024.1+. Inny czytnik otrzymuje dostępny dialog z przyciskiem rozpoczęcia po wskazówkach. Pogłos nadal może wpływać na pomiar. Zmiana celu uruchamia nowy wzorzec. **Wyłącz mikrofon** kończy cykl.

W wariancie słuchawkowym mikrofon działa równocześnie z kamertonem. **Odsłuch mikrofonu w słuchawkach** jest opcjonalny, domyślnie wyłączony. Najpierw załóż słuchawki: odtwarzanie mikrofonu przez głośniki może spowodować sprzężenie. Odsłuch korzysta z domyślnych urządzeń Windows, nie zmienia ustawień systemowych; opóźnienie zależy od sprzętu i sterowników.

Pole **Dwukrotne stuknięcie przechodzi do kolejnego dźwięku zgodnego z wybraną kolejnością** działa przy kamertonie i hybrydzie. W metodzie Kamerton pole samodzielnie włącza i wyłącza mikrofon; osobne przyciski mikrofonu są ukryte. Zapisany nasłuch rozpoczyna się także po ponownym otwarciu okna. Przy hybrydzie rozpocznij przyciskiem mikrofonu. Dwa oddzielne ataki w ustawionym czasie wybierają następny cel według planu i odtwarzają jego wzorzec. W samym kamertonie to gest akustyczny: dwa przypadkowe hałasy też mogą go wywołać. W hybrydzie głośnikowej po odczytaniu dobrego wyniku jest ciche okno na dwa stuknięcia. W hybrydzie słuchawkowej oba uderzenia muszą mieć właściwą wysokość. Po własnym tonie kamertonu nasłuch czeka na wybrzmienie. Przy automatycznych uderzeniach i tym polu odstęp wynosi co najmniej 6,5 sekundy, żeby pozostawić czas na gest. **Ctrl+N** pozostaje dostępną metodą ręczną.

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

Aktualizator ponawia chwilowe błędy połączenia i pobieranie przerwane w trakcie przesyłania. Sprawdzenia startowe i okresowe nie pokazują błędów połączenia; przycisk **Sprawdź aktualizacje** pozwala ponowić próbę ręcznie. Przed zamknięciem programu sprawdza prawo zapisu w folderze EXE. Po udanej podmianie nowa wersja ponawia usunięcie starej kopii, jeśli plik był chwilowo zablokowany. Weryfikacja SHA-256 pozostaje obowiązkowa. Szczegóły błędów są w `Stroik-data\update.log` obok EXE.
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




## Zmiany w 1.17

- Naprawiono pozostawianie starego dźwięku po zmianie współbrzmienia kamertonu. Wybranie kwinty lub oktawy od razu odtwarza oba dźwięki razem, także przy wyłączonym automacie i mikrofonie. Wybranie „Wyłączone” natychmiast wraca do pojedynczej nuty.
- Zmiana nuty bezpośrednio na liście, zmiana oktawy i częstotliwości odniesienia również od razu odświeżają wzorzec. Zachowano obsługę strzałek i „Następnego dźwięku” bez podwójnego uderzania.
- Opis przy współbrzmieniu oraz opis dostępności listy wskazują obie nuty. Wybrana nuta jest dolną nutą pary: dla c razkreślnego kwinta to c razkreślne + g razkreślne (261,63 + 392,00 Hz przy A=440). Dla g razkreślnego kwinta w górę to g razkreślne + d dwukreślne.
- Aby ponownie uderzyć tę samą nutę, odznacz automat i naciśnij Alt+U lub „Uderz kamerton”. Pomysł osobnej zakładki do grania melodii zapisano do późniejszego zaprojektowania; nie dodano jej w tej poprawce.

Testy sygnału PCM po rzeczywistych zdarzeniach interfejsu obejmują kwinty, oktawy, wyłączenie współbrzmienia, A=435/440 i ręczny/automatyczny kamerton. Nie wykonano odsłuchu na komputerze zgłaszającym problem ani testu NVDA. Poprawka dotyczy Windows.


## Zmiany w 1.18

Opisy położenia klawiszy pianina są częścią opcji na liście dźwięków. W widoku dla początkujących program zgłasza położenie także po wybraniu kolejnego celu; widok klasyczny zachowuje krótką nazwę. Przykład: g razkreślne — biały klawisz między pierwszym a drugim czarnym klawiszem grupy trzech. Opisy obejmują również czarne klawisze.


## Zmiany w 1.19

Stroik 1.19

- Widok klasyczny nie pokazuje wskazówek strojenia, opisu standardu 440 Hz ani pozostałości porad pianinowych po wyborze innego instrumentu. Pole ma krótką etykietę Częstotliwość, Hz.
- Hybryda pokazuje sterowanie mikrofonem i własne pole gestu dwóch stuknięć. Ręczne uderzenie i automat kamertonu są tylko w metodzie Kamerton. Elektroniczne przechodzenie po poprawnym wyniku działa wyłącznie w stroiku elektronicznym. Rozstrojone uderzenia resetują uzbrojony gest; oba ataki muszą zmieścić się w wybranej tolerancji i mieć świeże potwierdzenie wysokości.
- Shift+Alt+N lub Shift+Ctrl+N wraca do poprzedniego dźwięku według planu. Alt+N i Ctrl+N przechodzą dalej. Koniec planu zatrzymuje aktywne strojenie.
- Poprawiono polskie nazwy głośników, słuchawek i sposobów pomiaru. Wpis liczbowy przyjmuje kropkę i przecinek. Komunikaty używają powiadomień czytnika; przy braku wykrytego czytnika dodano asynchroniczny głos SAPI. Hybryda czeka na zakończenie własnej mowy SAPI.
- Niskie tony kamertonu mają większy poziom oraz ciche wyższe składowe pomocne małym głośnikom. Kwinty nadal grają właściwe nuty równocześnie: c razkreślne + g razkreślne to 261,63 i 392,00 Hz przy A=440. Wygładzono zakończenie tonu. Wrażenie oktawy i słyszalność basu zależą także od głośników; nie przesuwamy wzorca do innej oktawy.
- Klawisz na wizualnej klawiaturze jest czerwony przy rozstrojeniu, zielony w wybranej tolerancji, niebieski przed pomiarem. Kolor uzupełnia wynik tekstowy i wskaźnik.

Testy: PL/EN, cyfry i separatory dziesiętne, skróty, widoczność, pięć kolejnych wyników hybrydy, ścisła tolerancja gestu, widmo kwint w pięciu oktawach, SAPI start/koniec oraz dotychczasowe regresje audio/updatera. Nie potwierdzono odsłuchu NVDA ani prawdziwego instrumentu u zgłaszającego.

[SAPI completion API](https://learn.microsoft.com/en-us/previous-versions/windows/desktop/ee125654(v=vs.85)).


## Zmiany w 1.20

Stroik 1.20

- Dodano niemiecki, rosyjski, francuski i hiszpański obok polskiego i angielskiego: interfejs, komunikaty, nuty, wskazówki, profile i instrukcje.
- Kamerton ma wyraźniejsze harmoniczne w najniższym rejestrze bez zmiany oktawy. Małe głośniki nadal mogą ograniczać bas.
- W metodzie Kamerton pole dwóch stuknięć samodzielnie steruje mikrofonem; ukryto osobne przyciski mikrofonu. Rozruch wymaga krótkiej ciszy, aby nie liczyć go jako uderzenia. Automat pozostawia czas na gest.
- Hybryda dla początkujących czeka na odczyt wskazówek NVDA/SAPI. Inny czytnik otrzymuje dostępne okno z przyciskiem rozpoczęcia po wskazówkach. Klasyczny widok rozpoczyna od razu. Dodano przerwę po wyniku 3–60 sekund, domyślnie 8; NVDA 2026.3+ i SAPI blokują wzorzec podczas mowy.
- Krótsze odczyty w widoku klasycznym: fortepian, lista nut bez opisów położenia, automat bez dodatkowej instrukcji. Liczby są pojedynczymi polami edycji z obsługą strzałek i wpisu przecinkiem/kropką, bez roli pokrętła.
- Góra/dół wybierają białe klawisze, lewo/prawo wyłącznie czarne. Następny/poprzedni według planu nadal obejmuje jego wszystkie nuty.
- Dodano opisy zastosowania czterech metod oraz dostępne z klawiatury wskazówki instrumentu w widoku początkującym; F1 otwiera je także na żądanie.

NVDA Controller Client: [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md), LGPL 2.1.
