# Stan prac — Stroik 1.0

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