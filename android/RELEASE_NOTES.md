# Stroik Android 1.02

## Polski

Poprawka zamykania aplikacji przy odtwarzaniu ekranu — np. po zmianie motywu, obrocie telefonu lub powrocie do aplikacji.

- Rozdzielono zapis ustawień od automatycznego zapisu dynamicznych kontrolek, aby stan pola wyboru nie trafiał do listy rozwijanej.
- Przywracana jest właściwa zakładka, przewinięcie i ostatni wynik. Mikrofon po przerwie uruchom ponownie.
- Dodano regresję dla stanu starszych wydań i wielokrotnego odtwarzania wszystkich zakładek PL/EN oraz zmiany motywu.
- versionName 1.02, versionCode 102; zachowany klucz podpisu. Nie odinstalowuj poprzedniej wersji, aby zachować ustawienia.

Końcowy APK na POCO przeszedł 1103 asercje odtwarzania ekranów, 1831 interfejsu/dostępności i 7 audio, bez nowej awarii w tej serii. Systemowa instalacja z lokalnego APK zachowała ustawienia; pełne pobranie i aktualizacja 1.02 przez GitHub nie były osobnym testem.

Na testowym POCO Play Protect blokował instalację kandydatów jako potencjalnie niebezpiecznych, bez szczegółowej przyczyny. Instalacja testowa wymagała ręcznej decyzji właściciela. Przyczyna klasyfikacji nie jest ustalona; nie traktujemy zgodności podpisu jako rozstrzygnięcia ostrzeżenia. Szczegóły i aktualny zakres prób: [raport urządzenia](https://github.com/proszczok2002/Stroik/blob/main/android/DEVICE_TESTS.md).

## English

Fix for a crash during screen restoration, including theme changes, rotation or returning to the app.

- Settings and dynamic widget state are handled separately, preventing checkbox state from being restored into a dropdown.
- The correct tab, scroll position and last result are restored. Restart the microphone after an interruption.
- Added legacy-state and repeated recreation coverage for every PL/EN tab and theme switching.
- versionName 1.02, versionCode 102; unchanged signing key. Update without uninstalling to retain settings.

The final APK passed 1103 lifecycle assertions, 1831 interface/accessibility assertions and 7 audio checks on the POCO, with no new crash during that sequence. System installation from the local APK retained settings; a complete GitHub download/update to 1.02 was not separately tested.

On the test POCO, Play Protect blocked candidate installations as potentially unsafe without a detailed cause. Test installation required the owner's manual decision. The classification cause remains unknown; a matching signature does not resolve the warning. Actual test scope and current results: [device report](https://github.com/proszczok2002/Stroik/blob/main/android/DEVICE_TESTS.md).
