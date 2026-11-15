# Stroik Android 1.06

[Pobierz APK 1.06](https://github.com/proszczok2002/Stroik/releases/download/android-v1.06/Stroik-Android-1.06.apk) · [GitHub Release](https://github.com/proszczok2002/Stroik/releases/tag/android-v1.06) · [PL](README.md) / [EN](README.en.md)

W kamertonie nie ma przycisku mikrofonu ani osobnego przystanku z jego stanem. Opcjonalny nasłuch dwóch stuknięć włącza się własnym polem wyboru. Po końcu wybranej kolejności kończy się cały aktywny nasłuch i odtwarzanie, również w hybrydzie i przy wyłączonym automatycznym przechodzeniu.

Wszystkie ustawienia liczbowe są suwakami z bieżącą wartością. Przesuń palec lub użyj gestu regulacji TalkBack; podwójne stuknięcie albo Enter otwiera pole z klawiaturą numeryczną. Wpis poza zakresem pozostaje w dialogu do poprawienia. Ustawienia obejmują prędkość mowy Stroika przy wygaszonym ekranie: 0,5–3, domyślnie 1. Nie zmienia to prędkości TalkBacka. W hybrydzie głośnikowej własny kamerton czeka na rzeczywisty koniec wypowiedzi syntezatora, zamiast zgadywać jej długość.

Lista nut pianina zawiera opisy położenia względem grup czarnych klawiszy. Następny dźwięk w widoku dla początkujących czyta także położenie; widok klasyczny zachowuje krótką nazwę. Uporządkowano kolejność dostępności i usunięto przejmowanie fokusu przez pole wyniku. Naturalne flicki na Motoroli i Xiaomi wymagają jeszcze potwierdzenia użytkownika.

Podwójne stuknięcie dwoma palcami TalkBack korzysta ze standardowego sterowania mediami: mikrofon w stroiku elektronicznym, kamerton w metodzie kamertonowej, oba w hybrydzie. Przy zamkniętej aplikacji rozpoczęcie mikrofonu wymaga jej otwarcia. Działanie gestu zależy od konfiguracji TalkBack i aktywnych mediów; systemowe zdarzenia Play/Pause sprawdzono na Pixelu, naturalny gest wymaga osobnej próby.

Włączona grafika jest nad przewijanymi ustawieniami: półkolisty wskaźnik odchyłki w centach oraz klawiatura z oznaczeniem wybranej nuty. Grafika nie dodaje przystanków TalkBack. Dokumentacja i komunikaty są po polsku i angielsku.

Instrukcja aplikacji: [guide.pl.txt](app/src/main/assets/guide.pl.txt). [Device evidence / Raport urządzenia](DEVICE_TESTS.md).

## Build / Budowanie

Windows, JDK 17+, Android SDK Platform 36 / Build Tools 36.0.0. No external libraries / bez bibliotek zewnętrznych.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File android/build.ps1 -Sdk 'C:\Android\Sdk' -JavaBin 'C:\Java\bin'
```

PL: Prywatny klucz i hasło w ignorowanym android/.signing muszą być zachowane do kolejnych aktualizacji. Nie publikować. Opcja -Ci korzysta z osobnego klucza do testów. Bieżące pliki są w android/releases; stare buildy zachowuje główny folder niepotrzebne.

EN: Preserve the private key/password in ignored android/.signing for compatible updates; never publish them. -Ci uses a separate test key. Current files are in android/releases; old builds are archived in the project-root niepotrzebne folder.

PL: Aktualizacje korzystają z android/update.json. Weryfikowane są SHA-256, pakiet, wersja i podpis. Instalacja wymaga systemowego potwierdzenia Androida. Nie odinstalowuj aplikacji przed aktualizacją. Kanał Windows pozostaje oddzielny.

EN: Updates use android/update.json with SHA-256, package, version and certificate verification. Installation requires Android system confirmation. Do not uninstall before updating. Windows uses a separate release channel.
