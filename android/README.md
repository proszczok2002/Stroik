# Stroik Android 1.09

- A kontra otrzymało takie samo wsparcie harmonicznych jak niższe nuty. Obie platformy używają tej samej barwy bez zmiany oktawy. Słyszalność basu nadal zależy od głośników.
- Klasyczna hybryda uruchamia kamerton od razu, bez samouczka. W widoku dla początkujących najpierw czytane są wskazówki. Działanie Windows zachowano i dodano je w Androidzie.
- Przerwa po wyniku wynosi 0–60 sekund, także 1 lub 2. Zero oznacza powrót wzorca od razu po zakończeniu mowy i jest nowym ustawieniem domyślnym. Zapisane przerwy Windows są zachowane. Android ma tę samą regulację z suwakiem i wpisem liczby.
- Najpierw cały wynik, potem kamerton i możliwość przejścia. W hybrydzie przejście wymaga dobrego wyniku oraz dwóch uderzeń o potwierdzonej wysokości. W samym kamertonie nadal działa gest akustyczny.
- Android: klasyczna lista bez opisów położenia klawiszy, opisy metod dla początkujących oraz czarne klawisze strzałkami lewo/prawo. Wyniki i samouczek czyta syntezator aplikacji także przy TalkBack, aby znać koniec mowy; TalkBack nadal obsługuje nawigację.

Stroik Android 1.09

Dodano niemiecki, rosyjski, francuski i hiszpański obok polskiego i angielskiego. Przetłumaczono interfejs, komunikaty głosowe, nuty, oktawy, położenia klawiszy, instrukcję i ostatnie zmiany. Wybór języka zapisuje się w ustawieniach i profilach JSON; stare profile PL/EN pozostają zgodne. Własny syntezator aplikacji wybiera język zgodny z ustawieniem; potrzebny jest odpowiedni głos Androida. Nowe wydania mogą zawierać opisy zmian we wszystkich sześciu językach, ze zgodnym odczytem starszych manifestów. Zasady systemowego potwierdzania aktualizacji pozostają zachowane.


[APK 1.09](https://github.com/proszczok2002/Stroik/releases/download/android-v1.09/Stroik-Android-1.09.apk) · [Release](https://github.com/proszczok2002/Stroik/releases/tag/android-v1.09) · [PL](README.md) / [EN](README.en.md)

Widok klasyczny ukrywa porady, także standard 440 Hz i wskazówki pianinowe przy ukulele. Widok wybierasz tylko na stronie Stroik; usunięto powtórzone pole w Ustawieniach.

W hybrydzie sterujesz mikrofonem, który uruchamia także kamerton. Ręczne uderzenie, automat i jego odstęp są tylko w metodzie Kamerton; hybryda zachowuje osobne pole dwóch uderzeń potwierdzonych wysokością. Przechodzenie po poprawnym wyniku dotyczy metody Elektroniczny i Hybryda. Oba potwierdzone ataki wymagają wybranej tolerancji, świeżej próbki i właściwej nuty. Rozstrojenie kasuje uzbrojenie; zwykły wynik sam nie zmienia celu. Kolejny wzorzec nie odrzuca już gotowego wyniku. Kamerton po własnym TTS czeka na faktyczne zakończenie wypowiedzi.

Shift+Alt+N i Shift+Ctrl+N wracają do poprzedniego dźwięku według planu. Wpis wartości otwiera klawiaturę numeryczną i przyjmuje kropkę lub przecinek; suwak i profil zapisują podaną liczbę. Ustawienie prędkości mowy pozostaje w Ustawieniach.

Wzmocniono niskie tony kamertonu i wygładzono zakończenie, aby ograniczyć stuk. Kwinta od c razkreślnego zawiera c razkreślne i g razkreślne, 261,63 i 392,00 Hz przy A=440; test widma obejmuje pięć oktaw i brak składowych o oktawę niższych. Małe głośniki nadal ograniczają słyszalność basu. Wizualny klawisz zmienia się z niebieskiego na czerwony przy rozstrojeniu lub zielony w ustawionej tolerancji. Kolor uzupełnia opis tekstowy.

Próby na Pixelu z włączonym TalkBack są opisane w raporcie DEVICE_TESTS.md. Testy struktury, zdarzeń i audio nie potwierdzają wymowy, naturalnych gestów ani strojenia prawdziwego instrumentu na każdym telefonie.

[Device evidence / Raport urządzenia](DEVICE_TESTS.md).

## Build / Budowanie

Windows, JDK 17+, Android SDK Platform 36 / Build Tools 36.0.0. No external libraries / bez bibliotek zewnętrznych.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File android/build.ps1 -Sdk 'C:\Android\Sdk' -JavaBin 'C:\Java\bin'
```

PL: Prywatny klucz i hasło w ignorowanym android/.signing muszą być zachowane do kolejnych aktualizacji. Nie publikować. Opcja -Ci korzysta z osobnego klucza do testów. Bieżące pliki są w android/releases; stare buildy zachowuje główny folder niepotrzebne.

EN: Preserve the private key/password in ignored android/.signing for compatible updates; never publish them. -Ci uses a separate test key. Current files are in android/releases; old builds are archived in the project-root niepotrzebne folder.

PL: Aktualizacje korzystają z android/update.json. Weryfikowane są SHA-256, pakiet, wersja i podpis. Instalacja wymaga systemowego potwierdzenia Androida. Nie odinstalowuj aplikacji przed aktualizacją. Kanał Windows pozostaje oddzielny.

EN: Updates use android/update.json with SHA-256, package, version and certificate verification. Installation requires Android system confirmation. Do not uninstall before updating. Windows uses a separate release channel.

[Deutsch](README.de.md) · [Русский](README.ru.md) · [Français](README.fr.md) · [Español](README.es.md)
