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