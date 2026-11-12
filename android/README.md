# Stroik Android 1.04 — wersja testowa

Trzy zakładki w jednej linii na dole: Stroik, Ustawienia i Pomoc; instrument jest listą na stronie Stroik. Dodano jeden przełącznik mikrofonu, suwak Hz z edycją liczbową, głos w grafice, utrzymywanie ekranu i usługę nasłuchu przy wygaszonym ekranie.

[Testowy APK 1.04](https://github.com/proszczok2002/Stroik/releases/download/android-v1.04/Stroik-Android-1.04.apk) · [Wydanie testowe](https://github.com/proszczok2002/Stroik/releases/tag/android-v1.04).

**Telefon jest obecnie niedostępny.** Kod, APK i testy urządzenia skompilowano; testy sygnałów i logiki przeszły. Nowych ścieżek mikrofonu w tle, TalkBack, suwaka i aktualizatora nie sprawdzono na urządzeniu. Automatyczny manifest pozostaje na 1.03 do testu telefonu. Wcześniejsze ostrzeżenie Play Protect pozostaje niewyjaśnione. [Zakres prób](DEVICE_TESTS.md).

## Poprzednie wydanie 1.03

Natywna aplikacja Android 11+ dla osób niewidomych, słabowidzących i widzących. Interfejs PL/EN oparty na standardowych kontrolkach Androida. [English](README.en.md).

[Pobierz APK 1.03](https://github.com/proszczok2002/Stroik/releases/download/android-v1.03/Stroik-Android-1.03.apk) · [Wydanie i suma SHA-256](https://github.com/proszczok2002/Stroik/releases/tag/android-v1.03).

**Dostępność w 1.03:** zmiana metody, współbrzmienia i instrumentu zachowuje kontrolki zamiast przebudowywać ekran. Zakładki pozostają stałe, język i motyw przywracają fokus do odpowiadającej kontrolki. Pola liczbowe mają etykiety i podpowiedzi bez zasłaniania wpisanej wartości. Na POCO przeszło 198 asercji fokusu PL/EN, 1103 cyklu ekranu, 1831 interfejsu i 7 audio. Ustawienia zachowano po systemowej instalacji lokalnego APK. Odsłuch i gesty TalkBack przez człowieka są odrębną próbą; pełna aktualizacja 1.03 przez GitHub nie była testowana. Wcześniejsze ostrzeżenie Play Protect pozostaje niewyjaśnione. [Raport urządzenia](DEVICE_TESTS.md).

Instrukcja aplikacji: [guide.pl.txt](app/src/main/assets/guide.pl.txt). Kamerton, stroik mikrofonowy, hybryda, instrumenty, oktawy, kwinty/oktawy, profile JSON, odsłuch w słuchawkach, korekta pomiaru, grafika, wsparcie i informacje o autorach.

Aktualizator korzysta z `android/update.json` w tym repozytorium. Pliki wydań mają tagi `android-v1.0`, `android-v1.01` i nie zmieniają najnowszego wydania Windows. Instalacja wymaga systemowego potwierdzenia Androida; APK musi mieć ten sam podpis, właściwy pakiet `pl.proszczok.stroik` i wyższy versionCode. Nie odinstalowuj aplikacji przed aktualizacją, żeby zachować ustawienia.

## Budowanie

Wymagane: Windows, JDK 17+ (lokalnie JDK 25), Android SDK Platform 36 i Build Tools 36.0.0. Brak bibliotek zewnętrznych.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File android/build.ps1 -Sdk 'C:\Android\Sdk' -JavaBin 'C:\Program Files\Java\jdk-25\bin'
```

Skrypt tworzy prywatny klucz w `android/.signing` przy pierwszym buildzie. Zachowaj bezpieczną kopię tego folderu; bez klucza nie da się wydać zgodnej aktualizacji. Klucz i hasło nie trafiają do GitHuba. Opcja `-Ci` tworzy oddzielny klucz wyłącznie do kompilacji kontrolnej. Artefakty są w `android/releases`; wcześniejsze pliki wydania są przenoszone do głównego `niepotrzebne`.

Rzeczywiste wyniki urządzenia i zakres wykonanych prób są zapisane w [DEVICE_TESTS.md](DEVICE_TESTS.md), a lokalny stan prac w głównym `STAN_PRAC.md`. Testy struktury kontrolek nie zastępują odsłuchu i gestów TalkBack.
