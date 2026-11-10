# Stroik Android 1.01

Natywna aplikacja Android 11+ dla osób niewidomych, słabowidzących i widzących. Interfejs PL/EN oparty na standardowych kontrolkach Androida. [English](README.en.md).

[Pobierz APK 1.01](https://github.com/proszczok2002/Stroik/releases/download/android-v1.01/Stroik-Android-1.01.apk) · [Wydanie i suma SHA-256](https://github.com/proszczok2002/Stroik/releases/tag/android-v1.01).

**Status próby aktualizacji:** na testowym POCO Play Protect zablokował instalację 1.01 po skanowaniu, bez podania konkretnej przyczyny. Wykrywanie, pobranie i otwarcie systemowego instalatora działają; pełna aktualizacja i zachowanie ustawień pozostają niepotwierdzone. Szczegóły w [raporcie urządzenia](DEVICE_TESTS.md).

Instrukcja aplikacji: [guide.pl.txt](app/src/main/assets/guide.pl.txt). Kamerton, stroik mikrofonowy, hybryda, instrumenty, oktawy, kwinty/oktawy, profile JSON, odsłuch w słuchawkach, korekta pomiaru, grafika, wsparcie i informacje o autorach.

Aktualizator korzysta z `android/update.json` w tym repozytorium. Pliki wydań mają tagi `android-v1.0`, `android-v1.01` i nie zmieniają najnowszego wydania Windows. Instalacja wymaga systemowego potwierdzenia Androida; APK musi mieć ten sam podpis, właściwy pakiet `pl.proszczok.stroik` i wyższy versionCode. Nie odinstalowuj aplikacji przed aktualizacją, żeby zachować ustawienia.

## Budowanie

Wymagane: Windows, JDK 17+ (lokalnie JDK 25), Android SDK Platform 36 i Build Tools 36.0.0. Brak bibliotek zewnętrznych.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File android/build.ps1 -Sdk 'C:\Android\Sdk' -JavaBin 'C:\Program Files\Java\jdk-25\bin'
```

Skrypt tworzy prywatny klucz w `android/.signing` przy pierwszym buildzie. Zachowaj bezpieczną kopię tego folderu; bez klucza nie da się wydać zgodnej aktualizacji. Klucz i hasło nie trafiają do GitHuba. Opcja `-Ci` tworzy oddzielny klucz wyłącznie do kompilacji kontrolnej. Artefakty są w `android/releases`; wcześniejsze pliki wydania są przenoszone do głównego `niepotrzebne`.

Rzeczywiste wyniki urządzenia i zakres wykonanych prób są zapisane w [DEVICE_TESTS.md](DEVICE_TESTS.md), a lokalny stan prac w głównym `STAN_PRAC.md`. Testy struktury kontrolek nie zastępują odsłuchu i gestów TalkBack.
