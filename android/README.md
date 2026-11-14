# Stroik Android 1.05

[Pobierz APK 1.05](https://github.com/proszczok2002/Stroik/releases/download/android-v1.05/Stroik-Android-1.05.apk) · [Wydanie i suma SHA-256](https://github.com/proszczok2002/Stroik/releases/tag/android-v1.05) · [English](README.en.md)

Wersja przenosi zaległe poprawki obsługi, dostępności i wyglądu oraz poprawki kamertonu z Windows 1.16/1.17. Trzy dolne zakładki są w jednym rzędzie; instrument to lista na stronie Stroik. Jest jeden przełącznik mikrofonu, suwak Hz z widoczną wartością i wpisem liczbowym, opcje głosu i utrzymania ekranu, nasłuch w tle oraz TTS po wygaszeniu ekranu. Zmiana motywu i widoczności sekcji zachowuje kontrolki i fokus.

Zainstalowano i sprawdzono dokładny APK na Pixelu 11 z Androidem 17 i Google TalkBack: 183 asercje fokusu PL/EN, 983 cyklu życia, 2185 interfejsu, 48 obsługi, 14 komunikatów, 60 kamertonu, 7 audio i 6 wygaszonego ekranu. Skanowanie Play Protect i systemowa instalacja przeszły; zachowano ustawienia, dane i podpis. Pełny zakres oraz ograniczenia opisuje [raport urządzenia](DEVICE_TESTS.md). Naturalne gesty i odsłuch TalkBack przez człowieka, prawdziwy instrument i słuchawki pozostają osobnymi próbami. Nie przedstawiamy lokalnej instalacji jako pełnej aktualizacji przez GitHub.

Kandydat 1.04 pozostaje historycznym prerelease. Pomysł osobnej zakładki Pianino jest zapisany w [POMYSLY.md](../POMYSLY.md), poza zakresem tej poprawki.

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
