# Stroik 1.12 / Stroik 1.12

## Polski

- Stroik sprawdza aktualizacje przy uruchomieniu i co 5 minut podczas pracy programu. Sprawdzanie w tle nie pokazuje komunikatów ani nie zmienia widocznego statusu, gdy nie ma nowej wersji lub połączenie z internetem nie działa.
- Po znalezieniu nowej wersji pojawia się okno z opisem zmian i pytaniem o pobranie. Ta sama wersja jest proponowana automatycznie raz na uruchomienie programu; po wybraniu „Nie” można do niej wrócić przyciskiem „Sprawdź aktualizacje” w Ustawieniach.
- Sprawdzenia nie nakładają się na siebie. Zamknięcie programu zatrzymuje timer i zapobiega pokazaniu okna po zakończeniu zapytania sieciowego.

Pobieranie i instalowanie nadal wymagają potwierdzenia użytkownika. Plik EXE jest weryfikowany sumą SHA-256 przed podmianą.

## English

- Stroik checks for updates at startup and every 5 minutes while running. Background checks show no messages and leave the visible status unchanged when no update is available or the connection fails.
- A new version opens a dialog with its release notes and a download question. Each version is offered automatically once per app session; after choosing No, use Check for updates in Settings to return to it.
- Checks cannot overlap. Closing the app stops the timer and prevents a late network response from opening a dialog.

Downloading and installing still require user confirmation. The EXE is verified against its SHA-256 checksum before replacement.
