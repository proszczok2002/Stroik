# Stan prac — Stroik

Stan na 30 października 2026 r. Prace nad wydaniem 1.0 zostały zapisane i opublikowane.

## Publikacja

- Repozytorium: https://github.com/proszczok2002/Stroik
- Wydanie: https://github.com/proszczok2002/Stroik/releases/tag/v1.0.0
- Tag wydania: v1.0.0; commit źródłowy: e29438f03ec1f652983253388f2dc9b8dcdfb9b2.
- Wydanie zawiera samodzielny Stroik.exe dla Windows x64, archiwum ZIP i plik sumy SHA-256.
- Suma SHA-256 pliku Stroik.exe: 33554ac469565640e51a9db4311f98c1cc7402fae77829e6fd30fda78e6f1c70.

## Wykonane

- Interfejs po polsku i angielsku, ciemny motyw, dokumentacja w obu językach.
- Funkcje dostępności dla obsługi klawiaturą i czytnikami ekranu.
- Sprawdzanie najnowszego stabilnego wydania GitHub przy uruchomieniu oraz ręcznie.
- Po potwierdzeniu aktualizacji pobranie EXE i sumy SHA-256, weryfikacja, podmiana pliku przez proces pomocniczy, ponowne uruchomienie i usunięcie starej kopii po sukcesie.
- Testy automatyczne algorytmu z generowanymi tonami, parsowania wydania i sum kontrolnych.
- Dwa przebiegi GitHub Actions dla wydania 1.0 zakończyły się powodzeniem.
- Publiczne pliki wydania zostały pobrane przez HTTPS i porównane rozmiarem oraz SHA-256 z plikami lokalnymi.

## Stan aktualizatora

W odizolowanym teście podmiany stary plik Stroik.exe został zastąpiony plikiem z wydania, proces pomocniczy zakończył się kodem 0, suma nowego pliku była poprawna, a stara kopia została usunięta. Publiczne API GitHub poprawnie wskazuje aktualne wydanie 1.0.0.

Nie wykonano jeszcze pełnej próby aktualizacji po kliknięciu „Tak” z uruchomionej wersji 1.0.0 do rzeczywiście opublikowanej nowszej wersji, ponieważ taka wersja jeszcze nie istnieje. Tę próbę należy wykonać przy kolejnym wydaniu. Działanie z konkretnym mikrofonem, instrumentem i rzeczywistym NVDA również wymaga próby na sprzęcie.

## Przy następnym wydaniu

1. Zwiększyć wersję programu i przygotować nowe stabilne wydanie z Stroik.exe oraz Stroik.exe.sha256.
2. Uruchomić zainstalowany Stroik 1.0.0, potwierdzić okno aktualizacji i sprawdzić nową wersję, uruchomienie oraz usunięcie starego pliku.
3. Zweryfikować pliki nowego wydania pobrane publicznie przez HTTPS.

## Wydanie 1.01 — 31 października 2026 r.

- Kod wydania: commit 99c793e990b18f7240ed9f35e50573e9c2d58ef9, tag v1.0.1, tytuł „Stroik 1.01”.
- Wydanie publiczne: https://github.com/proszczok2002/Stroik/releases/tag/v1.0.1
- Dodano kamerton, dwa widoki, tradycyjne nazwy dźwięków, pełną listę 88 klawiszy oraz profile JSON.
- Lokalny build i testy zakończyły się powodzeniem. GitHub Actions dla commita wydania: https://github.com/proszczok2002/Stroik/actions/runs/41180572987 — sukces.
- Publiczne EXE, plik SHA-256 i ZIP pobrano bez uwierzytelniania; każda suma jest identyczna z lokalnym plikiem. SHA-256 EXE: 3BB0093234A2D81D061A64789CC29D892845684E000390B561DFA74F78D2F832.
- Kod aktualizatora z tagu v1.0.0 wykrył publiczną wersję 1.0.1, pobrał EXE o rozmiarze 116168828 bajtów i zweryfikował jego SHA-256.
- Izolowana podmiana starego EXE 1.0 nowym EXE 1.01: kod procesu pomocniczego 0, nowy plik ma poprawną sumę, nowa aplikacja uruchomiła się, stara kopia została usunięta.
- Pełne przejście przez graficzny przycisk „Tak” w uruchomionym EXE 1.0 nie zostało wykonane, ponieważ narzędzie sterowania oknem nie uruchomiło się w tym środowisku. Nie wykonano też próby z rzeczywistym NVDA, mikrofonem, głośnikami i instrumentem.

## Wydanie 1.02 — 31 października 2026 r.

- Kod wydania: commit 574f626fc95e045cff56b89638d1845c8aa68911, tag v1.0.2, tytuł „Stroik 1.02”.
- Wydanie publiczne: https://github.com/proszczok2002/Stroik/releases/tag/v1.0.2
- Język, ciemny motyw i ręczne sprawdzanie aktualizacji przeniesiono do osobnej zakładki Ustawienia / Settings. Główna zakładka Stroik / Tuner pozostaje miejscem strojenia i profili.
- Testy lokalne i GitHub Actions https://github.com/proszczok2002/Stroik/actions/runs/41181927009 zakończyły się sukcesem. Test struktury WinForms sprawdził umieszczenie kontrolek, domyślną zakładkę i polskie oraz angielskie nazwy dostępności.
- Publiczne EXE, SHA-256 i ZIP pobrano bez uwierzytelniania; ich sumy są identyczne z plikami lokalnymi. SHA-256 EXE: EFA4914705544ABD5DCB5A9FCB3554E42B0D21F34886070EEA122846C874F689. Publiczne API najnowszego wydania wskazuje v1.0.2.
- Użytkownik zgłosił w rozmowie, że aktualizator z poprzedniego wydania zadziałał. Tego zgłoszenia nie należy mylić z osobnym testem NVDA lub sprzętu audio.
- Bezpośrednia próba odczytu nowego interfejsu w NVDA nie została wykonana; narzędzie do sterowania oknem nie uruchamia się w tym środowisku.

## Lokalny stan i zasada kolejnych wydań — 31 października 2026 r.

- Bieżąca publiczna wersja nadal wynosi 1.02 (tag v1.0.2). Ta zmiana dotyczy lokalnego porządku wydań i dokumentacji; nie tworzy nowej wersji programu.
- W głównym katalogu dist pozostały tylko aktualne Stroik.exe, Stroik.exe.sha256 i Stroik-1.0.2-Windows-x64.zip, aktualne kopie dokumentacji/licencji oraz build-version.txt. SHA-256 EXE: EFA4914705544ABD5DCB5A9FCB3554E42B0D21F34886070EEA122846C874F689, identyczne z publicznym wydaniem.
- Stare ZIP-y, wcześniejsze EXE, kopie testów aktualizatora i publicznych pobrań, wcześniejsze archiwum oraz zbędny ponowny build przeniesiono bez kasowania do dist/niepotrzebne. Kod źródłowy, testy, lokalny SDK oraz Git pozostają na miejscu.
- build-release.ps1 uruchamia testy, buduje w osobnym katalogu, sprawdza SHA-256, po sukcesie archiwizuje poprzednie pliki i pozostawia nową wersję w dist. Przy nowym numerze wersji archiwizuje też stare katalogi bin/obj oraz katalogi weryfikacji poprzedniego wydania. Skrypt blokuje przypadkowy ponowny build tej samej wersji; świadomy wyjątek przed publikacją wymaga -RebuildSameVersion.
- Sprawdzono blokadę ponownego buildu bez zmiany aktywnego EXE. Na izolowanej kopii przetestowano przejście z atrapą wersji 1.0.1 do 1.0.2: stare EXE, ZIP, checksum i katalogi kompilacji trafiły do archiwum, ich sumy pozostały zgodne, a aktywny katalog zawierał jeden nowy ZIP i znacznik wersji. Następnie na tej samej kopii zasymulowano przejście do 1.0.3: stare katalogi weryfikacji trafiły do archiwum, a plik projektu pozostał. Wersja 1.0.3 istnieje tylko w izolowanym teście wewnątrz dist/niepotrzebne; nie jest wydaniem ani aktualną wersją projektu.
- Przy kolejnym wydaniu: zwiększyć wersję w Stroik.csproj, uruchomić build-release.ps1, sprawdzić lokalne pliki i archiwum, poczekać na GitHub Actions, opublikować tag/wydanie oraz porównać pliki pobrane publicznie przez HTTPS.

## Wydanie 1.03 — 31 października 2026 r.

- Kod wydania: commit d5222e8f073f2d9bb1479a2c73d028a1e447db42, tag v1.0.3, tytuł „Stroik 1.03”.
- Wydanie publiczne: https://github.com/proszczok2002/Stroik/releases/tag/v1.0.3
- Dodano wybór pianina, gitary, ukulele, skrzypiec, gitary basowej i trybu chromatycznego. Listy strun mają standardowe dźwięki i właściwe oktawy; pianino i tryb chromatyczny zachowują pełny zakres 88 klawiszy.
- Automatyczny pomiar w trybach strunowych porównuje wynik z najbliższą pustą struną. Wybrany instrument zapisuje się lokalnie i w profilu JSON; stary profil bez tego pola używa pianina. Wskazówki i nazwy kontrolek są po polsku i angielsku.
- Lokalny build i testy zakończyły się powodzeniem. GitHub Actions dla commita wydania: https://github.com/proszczok2002/Stroik/actions/runs/41183814187 — sukces.
- Publiczne EXE, plik SHA-256 i ZIP pobrano bez uwierzytelniania; rozmiary i sumy są identyczne z lokalnymi. SHA-256 EXE: FAD405C014E33F17E761485FFE6F61E866A43062E9CBD7D1DD473DDA62CA3B3F.
- Publiczne API najnowszego wydania wskazuje v1.0.3 jako stabilne wydanie z oboma plikami wymaganymi przez aktualizator. Nie wykonano osobnej próby kliknięcia „Tak” z działającego EXE 1.02 do 1.03; użytkownik wcześniej zgłosił, że aktualizator działał. NVDA, rzeczywisty mikrofon, głośniki i instrumenty nadal wymagają próby na sprzęcie.
- W głównym katalogu dist pozostają tylko aktualne EXE, checksum, ZIP, kopie bieżącej dokumentacji i znacznik wersji. Wersja 1.02, wcześniejsza kompilacja 1.03 oraz kopie weryfikacyjne znajdują się w dist/niepotrzebne. Kod źródłowy i testy pozostają dostępne do dalszej pracy.
## Wydanie 1.04 — 1 listopada 2026 r.

- Kod wydania: commit 99684ee9e10dffd03bd65817ee009da8c46fd2ce, tag v1.0.4, publiczne wydanie: https://github.com/proszczok2002/Stroik/releases/tag/v1.0.4
- Dodano przejście do następnego dźwięku po dwóch rozpoznanych uderzeniach od nastrojonego wyniku, Ctrl+N, plany kolejności pianina, przewodnik po klawiaturze, opcjonalny wskaźnik i klawiaturę graficzną, ręczną korektę mikrofonu oraz standardowy i zamieniony A/G układ ukulele. Ustawienia są zapisywane w profilach JSON; instrukcje i interfejs są po polsku i angielsku.
- Testy syntetycznego pomiaru i kompilacja lokalna przeszły bez ostrzeżeń; GitHub Actions dla main i tagu zakończyły się sukcesem: https://github.com/proszczok2002/Stroik/actions/runs/41321432545 oraz https://github.com/proszczok2002/Stroik/actions/runs/41321556033.
- Publiczne EXE, SHA-256 i ZIP pobrano przez HTTPS. Publiczne EXE ma 116201596 bajtów i SHA-256 2E0A3F9A8320D7711052DDFA4EBA857F5897110E47881730E71C1ADCE1AB6F5A, zgodne z plikiem SHA-256 i lokalnym EXE. Publiczny ZIP ma 45900125 bajtów i SHA-256 3543F64923C8850994D8D318981BC5CF0FEC7C198DF41A2CF315098F2D1BA7D8, zgodne z lokalnym ZIP.
- W katalogu głównym dist pozostają pliki najnowszej wersji 1.0.4. Pliki 1.0.3 i stare katalogi kompilacji są w dist/niepotrzebne/v1.0.3-20261101-162832-29296. Kopie pobranych publicznych plików są w dist/verify-public i przy następnym nowym buildzie zostaną przeniesione do archiwum.
- Nie przeprowadzono próby z rzeczywistym NVDA, mikrofonem ani instrumentem. Narzędzie automatyzacji okna Windows dwukrotnie zakończyło się awarią przy uruchomieniu, więc nie potwierdzono interakcji UI. Użytkownik wcześniej potwierdził działanie aktualizatora przy poprzedniej wersji; przejścia z 1.03 do 1.04 nie testowano osobno.
## Wydanie 1.05 — 1 listopada 2026 r.

- Kod wydania: commit 651c47914c22bee4cfba08b7d3e5a21f18aeb568, tag v1.0.5; publiczne wydanie: https://github.com/proszczok2002/Stroik/releases/tag/v1.0.5
- Alt+U w trybie kamertonu przełącza automatyczne uderzenia; wyłączenie zatrzymuje timer i aktualnie grany ton. Test interfejsu sprawdził oba kierunki przełączenia.
- Aktualizator ponawia chwilowe błędy HTTP/sieciowe i przerwane pobranie EXE, pokazuje status automatycznego sprawdzenia w Ustawieniach PL/EN, sprawdza możliwość zapisu przed zamknięciem starej wersji i ponawia usunięcie starej kopii po podmianie. Zachowano weryfikację SHA-256. Testy kontrolowane sprawdziły odpowiedzi HTTP 503 oraz 404.
- Próba z rzeczywistym API GitHub i pełnym pobraniem przez kod aktualizatora wykryła 1.0.4 oraz pobrała EXE o SHA-256 2E0A3F9A8320D7711052DDFA4EBA857F5897110E47881730E71C1ADCE1AB6F5A. W izolowanym folderze uruchomiono pomocnika podmiany: kopia 1.04 została zastąpiona 1.05, nowy program wystartował, a plik .old nie pozostał. Nie była to próba kliknięcia „Tak” w oknie starego programu.
- Testy lokalne oraz GitHub Actions dla main i tagu zakończyły się sukcesem: https://github.com/proszczok2002/Stroik/actions/runs/41345554734 i https://github.com/proszczok2002/Stroik/actions/runs/41345687344.
- Publiczne pliki 1.05 pobrane przez HTTPS są zgodne z lokalnymi: EXE 116209788 bajtów, SHA-256 3A9B53F6119414671BCDBC578293C211670C9C8130D1AA9B41C98F6A8ABAA6AB; ZIP 45900730 bajtów, SHA-256 2E8825D8B4006EC0D9E0A4F654AFCE635317BAC1E111558D19EF4CDD44DC3470. Publiczne API latest wskazuje v1.0.5 z EXE i plikiem sumy.
- Główny katalog dist zawiera 1.0.5. Pliki 1.0.4 i starsze kompilacje są w dist/niepotrzebne/v1.0.4-20261101-210957-32624; kopie prób są w dist/verify-updater i dist/verify-public i zostaną zarchiwizowane przy następnym nowym buildzie.
- Nadal potrzebna jest próba przejścia z działającego EXE 1.04 do 1.05 po kliknięciu „Tak” oraz próba na docelowym komputerze z czytnikiem ekranu i sprzętem audio. Starsza wersja 1.04 nie ma jeszcze ponawiania błędów sieciowych, więc jej pierwsza próba pobrania 1.05 może wymagać ponownego uruchomienia lub ręcznego pobrania.
## Wydanie 1.06 — 2 listopada 2026 r.

- Kod wydania: commit 018e09616a55f48c94491db009427279665f9dcc, tag v1.0.6; publiczne wydanie: https://github.com/proszczok2002/Stroik/releases/tag/v1.0.6
- Po uwagach testera poprawiono Alt+U. Przy odznaczonym automacie skrót wybiera pojedyncze natychmiastowe uderzenie. Przy zaznaczonym automacie Alt+U wstrzymuje lub wznawia cykliczne uderzenia bez zmiany zaznaczenia. Dodano widoczny status i opis dostępności po polsku i angielsku. Zaznaczony tryb automatyczny po ponownym uruchomieniu znów pracuje.
- Projekt ma teraz AGENTS.md z utrwaloną zasadą wydawania: starsze EXE, ZIP, sumy, kopie weryfikacyjne i kompilacje archiwizuje się w dist/niepotrzebne; w głównym dist zostaje najnowsze wydanie, a bieżący kod, testy, narzędzia i Git pozostają do dalszej pracy. build-release.ps1 wykonał tę archiwizację dla 1.05 do dist/niepotrzebne/v1.0.5-20261102-093157-33140.
- Testy lokalne sprawdziły wybór trzech działań Alt+U, pauzę/wznowienie timera bez odznaczenia pola, etykiety PL/EN oraz wcześniejsze pomiary. Kompilacja bez ostrzeżeń. Izolowana próba podmiany 1.05 na 1.06 zakończyła się uruchomieniem nowego EXE i brakiem pliku .old. Nie wykonano automatycznego odsłuchu prawdziwego dźwięku w trybie ręcznym ani pełnego kliknięcia „Tak” w działającym EXE.
- GitHub Actions dla main i tagu przeszły: https://github.com/proszczok2002/Stroik/actions/runs/41413849626 oraz https://github.com/proszczok2002/Stroik/actions/runs/41414021175.
- Publiczne EXE, SHA-256 i ZIP pobrano przez HTTPS. EXE ma 116209788 bajtów, SHA-256 FCEBCEC779044FF7E18FAE3EB6D86608B3F0661EF01F6559C9C04AEB69BA849A, zgodne z lokalnym EXE i plikiem sumy. ZIP ma 45901347 bajtów, SHA-256 20F1C06A017993F28FE4402BE11929D9108D6B302BF60E99D2D481610A31FC01, zgodne z lokalnym ZIP. Publiczne API latest wskazuje v1.0.6.
- Tester przekazał, że aktualizacja do 1.05 przebiegła szybko; jest to relacja z jego komputera, bez osobnego logu technicznego. Rzeczywiste zachowanie Alt+U z kamertonem i czytnikiem ekranu nadal wymaga odsłuchu na docelowym komputerze.
## Wydanie 1.07 — 2 listopada 2026 r.

- Kod wydania: commit 4a3b819babda6b26b672a9b439fc3c45a75c1acc, tag v1.0.7; publiczne wydanie: https://github.com/proszczok2002/Stroik/releases/tag/v1.0.7
- W stroiku elektronicznym dodano pole „Maksymalny odstęp między uderzeniami” z zakresem 0,5–2,5 s, domyślnie 1,2 s zamiast stałego 0,9 s. Ustawienie zapisuje się lokalnie i w profilach JSON; starsze profile otrzymują nową wartość domyślną. Ułatwiono wykrywanie cichszego drugiego uderzenia przy zachowaniu sprawdzenia wysokości obu dźwięków.
- Lokalne testy obejmują różną głośność dwóch uderzeń, krótszy i dłuższy odstęp, odrzucenie jednego wybrzmiewającego dźwięku, zapis profilu oraz strukturę kontrolki PL/EN. Kompilacja bez ostrzeżeń. Izolowana podmiana 1.06 na 1.07 uruchomiła nowy EXE i usunęła plik .old. Nie wykonano próby z realnym mikrofonem, ukulele, pianinem ani NVDA; zachowanie wymaga potwierdzenia na sprzęcie użytkownika.
- GitHub Actions dla main i tagu przeszły: https://github.com/proszczok2002/Stroik/actions/runs/41513018958 oraz https://github.com/proszczok2002/Stroik/actions/runs/41513223571.
- Publiczne EXE, SHA-256 i ZIP pobrano przez HTTPS. EXE ma 116213884 bajty, SHA-256 8FBBDBD1979A04AB38E0A955C62B34FD6533FBF0ECEAA5E6702226ABEE007BB1, zgodne z lokalnym EXE i plikiem sumy. ZIP ma 45902872 bajty, SHA-256 43C91825EDE918C8D1DB0BE55DD08655B99E836B992CD6AD129579767FC0E3AC, zgodne z lokalnym ZIP. Publiczne API latest wskazuje v1.0.7.
- Główny katalog dist zawiera wydanie 1.0.7. Pliki 1.0.6 i starsze katalogi kompilacji są w dist/niepotrzebne/v1.0.6-20261102-204545-22952. Kopie prób są w dist/verify-updater i dist/verify-public; skrypt zarchiwizuje je przy następnym nowym buildzie. Kod, testy, lokalny SDK i Git pozostają dostępne.

## Wydanie 1.08 — 3 listopada 2026 r.

- Kod wydania: commit 8d201718bd8b8f96180cafbaeddf3e3d525fabc5, tag v1.0.8; publiczne wydanie: https://github.com/proszczok2002/Stroik/releases/tag/v1.0.8
- Po uwadze użytkownika usunięto dodatkową nazwę dostępności całej listy zakładek. Nazwy kart „Stroik” i „Ustawienia” pozostają w obu językach; standardowa rola listy zakładek oraz nawigacja klawiaturą zostały zachowane. Nie dodawano zapowiedzi nazwy mikrofonu, gdyż użytkownik uznał ją za opcjonalną i potencjalnie zbyt gadatliwą.
- Testy lokalne objęły przełączenie kart, nazwy i rolę dostępności oraz wcześniejsze kontrole pomiaru. Skrypt build-release.ps1 zbudował samodzielny Windows x64 EXE i przeniósł poprzednie wydanie i kopie weryfikacyjne do dist/niepotrzebne/v1.0.7-20261103-101657-35980. GitHub Actions dla main i tagu zakończyły się sukcesem: https://github.com/proszczok2002/Stroik/actions/runs/41609812562 i https://github.com/proszczok2002/Stroik/actions/runs/41610009555.
- Pobrane publicznie pliki EXE, SHA-256 i ZIP są identyczne z lokalnymi. EXE: 116213884 bajty, SHA-256 83617A2B3AB9B00635CF74249E314BF30675EB54EA10B78B65323833F8FAFFFC. ZIP: 45902928 bajtów, SHA-256 C6F25407F97D4965B30C809B82C9D3F6B6DCFB98C069363CA542E2E37CF30857. Publiczny plik sumy zgadza się z EXE, a GitHub latest wskazuje v1.0.8.
- Rzeczywistej wypowiedzi NVDA po zmianie zakładki nie udało się sprawdzić automatycznie: uruchomiony czytnik był widoczny, lecz interfejs automatyzacji Windows nie był dostępny. Zmiana wymaga potwierdzenia na komputerze użytkownika. Nie wykonywano nowej próby mikrofonu, instrumentu ani pełnej aktualizacji z 1.07 do 1.08; kod aktualizatora poza numerem wersji pozostał bez zmian.

## Porządek lokalnego archiwum — 3 listopada 2026 r.

- Na życzenie właściciela całe dotychczasowe archiwum przeniesiono z `F:\stroik\dist\niepotrzebne` do `F:\stroik\niepotrzebne`, czyli do folderu w głównym katalogu projektu. Po przeniesieniu zgadzały się liczba 700 plików i ich łączny rozmiar 1 474 535 454 bajtów; żadnych plików archiwum nie usuwano.
- `build-release.ps1`, `AGENTS.md`, instrukcje PL/EN oraz `.gitignore` wskazują teraz główny folder `niepotrzebne` jako miejsce na stare wydania, dawne kompilacje i kopie weryfikacyjne przy kolejnych nowych wersjach. Aktywne wydanie 1.08, jego ZIP, suma i bieżąca dokumentacja pozostają w `dist`; kod, testy, lokalny SDK i Git pozostają gotowe do pracy. Historyczne wpisy powyżej zachowują ścieżki obowiązujące w czasie ich powstania.

## Wydanie 1.09 — 3 listopada 2026 r.

- Kod wydania: commit 0ed1104b64efbd07644cc784fb6d89d29650b63c, tag v1.0.9; publiczne wydanie: https://github.com/proszczok2002/Stroik/releases/tag/v1.0.9
- Okno proponujące aktualizację pobiera z API GitHub opis nowego wydania i pokazuje właściwą sekcję PL/EN w przewijalnym, dostępnym polu tekstowym przed wyborem Tak/Nie. W Ustawieniach przycisk „Ostatnie zmiany” pokazuje bez internetu opis bieżącej wersji osadzony w EXE. Dodano „O programie” z opisem Stroika i autorstwem: wykonawca Proszczok z Codex, pomysłodawca Witold. Przyciski wsparcia i strony otwierają dokładnie podane przez właściciela adresy PayPal, Revolut i proszczok.pl.
- Po przeniesieniu archiwum do głównego katalogu projektu wykryto, że standardowa kompilacja .NET włączała stare pliki `.cs` z `niepotrzebne`; projekt wyklucza teraz ten folder z kompilacji. Testy lokalne sprawdziły pobieranie i wybór języka opisu, osadzoną dokumentację, nazwy przycisków PL/EN, strukturę okien, bezpieczną domyślną odpowiedź Nie oraz wcześniejsze kontrole pomiaru. Kompilacja bez ostrzeżeń. Skrypt wydania przeniósł 1.08 i stare kopie weryfikacyjne do `F:\stroik\niepotrzebne\v1.0.8-20261103-104029-5776`.
- GitHub Actions dla main i tagu zakończyły się sukcesem: https://github.com/proszczok2002/Stroik/actions/runs/41612991487 i https://github.com/proszczok2002/Stroik/actions/runs/41613185222. Publiczne EXE, SHA-256 i ZIP pobrano przez HTTPS i porównano z lokalnymi. EXE: 116222076 bajtów, SHA-256 36C6D8C7D0B9672E0382739ADB4017940A6DFC0BD4DEB07B359B16E87347CB24. ZIP: 45900828 bajtów, SHA-256 933ED43485401570AD96A2D409566B85D276D85AFEE89F9AA2B9053A09E03EEA. Publiczny plik sumy zgadza się z EXE; GitHub latest wskazuje v1.0.9, trzy pliki oraz sekcje Polski i English.
- Nie wykonano odsłuchu prawdziwego NVDA ani kliknięcia przycisków w działającym EXE z otwarciem przeglądarki. Nie wykonano nowej pełnej próby podmiany 1.08 na 1.09; kod podmiany poza numerem wersji nie zmienił się. Program 1.08 ma stare okno aktualizacji, więc opis zmian pojawi się dopiero przy aktualizacji uruchamianej z wersji 1.09 lub nowszej.

## Wydanie 1.10 — 3 listopada 2026 r.

- Kod wydania: commit de7074fdb2d38a4edf3e96f3a915a375d8a4409b, tag v1.0.10; publiczne wydanie: https://github.com/proszczok2002/Stroik/releases/tag/v1.0.10
- Po zgłoszeniu użytkownika dodano powiadomienie UI Automation na samym polu wyboru po jego kliknięciu. Zawiera dostępną nazwę oraz nowy stan „zaznaczone”/„odznaczone” lub „checked”/„unchecked”. Obejmuje wszystkie pięć pól: automatyczne uderzenia, dwa uderzenia, wybór oktawy, wskaźnik graficzny i ciemny motyw. Powiadomienie jest kolejkowane po natywnym zdarzeniu kliknięcia; programowe wczytywanie profilu nie wywołuje kliknięcia.
- Testy lokalne sprawdziły nazwy pól i komunikaty stanu w PL/EN, opis zmian 1.10 osadzony w EXE oraz wcześniejsze kontrole. Kompilacja zakończyła się bez ostrzeżeń. Skrypt wydania przeniósł 1.09 i starsze kopie weryfikacyjne do `F:\stroik\niepotrzebne\v1.0.9-20261103-105337-21272`.
- GitHub Actions dla main i tagu zakończyły się sukcesem: https://github.com/proszczok2002/Stroik/actions/runs/41614757881 i https://github.com/proszczok2002/Stroik/actions/runs/41614912772. Publiczne EXE, SHA-256 i ZIP pobrano przez HTTPS; rozmiary i sumy są zgodne z lokalnymi. EXE: 116222078 bajtów, SHA-256 7DC54297B2E9F427810B28D0790F0C405F86032FAFDD9D8A586BE3E75B97DE76. ZIP: 45900158 bajtów, SHA-256 1182D75597200CCB12E85EFF718214EDB6CD8E50E2120F1DF234999CD939DC88. Publiczny plik sumy zgadza się z EXE; GitHub latest wskazuje v1.0.10 z trzema plikami i opisem PL/EN.
- Próba `--verify-public-changelog 1.0.9.0 1.0.10` odczytała rzeczywiste najnowsze wydanie przez API GitHuba i potwierdziła, że logika aktualizatora wersji 1.09 dostaje opis zmian po polsku i angielsku. Nie udało się przeprowadzić próby mowy NVDA ani kliknięcia „Tak” w działającym EXE: interfejs automatyzacji Windows zgłosił brak natywnego połączenia. Pełna podmiana 1.09 → 1.10 i odsłuch natychmiastowego komunikatu pól wyboru wymagają sprawdzenia na komputerze użytkownika.

## Wydanie 1.11 — 7 listopada 2026 r.

- Kod wydania: commit 0d8bcfdf127c04b2b50de3cf0eb45af3e5d3696d, tag v1.0.11; wydanie: https://github.com/proszczok2002/Stroik/releases/tag/v1.0.11
- Po zgłoszeniu nieregularnego wykrywania i pozornego zawieszania mikrofonu poprawiono trzy znalezione przyczyny: pomiar odbywa się w każdym pełnym oknie zamiast co trzecie; szum bez wiarygodnej wysokości nie odwleka zakończenia nuty; zmiana celu, instrumentu, stroju odniesienia lub czułości rozpoczyna świeżą sesję. Numer generacji zabezpiecza przed wyświetleniem starego wyniku po zmianie ustawień. RMS pomija stałe przesunięcie sygnału. Bufory wracają do sterownika przed analizą; zakończenie mikrofonu czeka na wątek przed zwolnieniem pamięci; błędy analizy i kolejkowania są jawnie zgłaszane.
- Testy regresji objęły krótki cichy ton (~0,34 s) ze stałym przesunięciem, szum bez wysokości po stabilnym tonie, kolejną nutę po szumie i zmianę celu podczas otwartej sesji z aktywną przerwą. Wcześniejsze testy przeszły. Build bez ostrzeżeń; pliki 1.10, stare kompilacje i kopie weryfikacyjne są w `F:\stroik\niepotrzebne\v1.0.10-20261107-183527-34408`.
- GitHub Actions main i tag: https://github.com/proszczok2002/Stroik/actions/runs/42397100617 oraz https://github.com/proszczok2002/Stroik/actions/runs/42397198058 — sukces. Publiczne pliki pobrano przez HTTPS i porównano rozmiary oraz SHA-256 z lokalnymi. EXE: 116226174 bajty, SHA-256 4F042456398F80B7C6373C43DA51069A6381910A0049D41662642CEDF622D00E. ZIP: 45898867 bajtów, SHA-256 46DB0F38AF98BDD7DFD8778342B5027E2508115C230DA391341C76FD6746C95D. Publiczna suma zgadza się z EXE.
- Próba publicznego changeloga dla klienta 1.0.10.0 wykryła 1.0.11 oraz obie sekcje PL/EN. Nie wykonano pełnej podmiany działającego EXE, odsłuchu NVDA ani rzeczywistej próby mikrofonu, sterownika, instrumentu i hałasu. Nie można na podstawie testów syntetycznych potwierdzić usunięcia wszystkich zawieszeń opisanych przez testera. Pozostaje celowa przerwa 1,5 s po komunikacie wyniku; zmiana celu usuwa ją przy następnym buforze.
- Użytkownik po wydaniu 1.10 potwierdził „jest okki” w odpowiedzi na poprawkę komunikatów pól wyboru; zapis dotyczy tej poprawki, a nie obecnych testów audio.
