# Stroik Android 1.03

## Polski

Poprawiono utrzymywanie fokusu TalkBack podczas korzystania z aplikacji.

- Metoda strojenia, współbrzmienie, instrument i nasłuch dwóch stuknięć pokazują lub ukrywają właściwe pola bez wymiany kontrolki z fokusem.
- Przyciski zakładek pozostają tymi samymi kontrolkami. Ponowne wybranie bieżącej zakładki nie resetuje formularza.
- Zmiana języka i motywu przywraca odpowiadającą kontrolkę. Pola liczbowe korzystają z etykiety i podpowiedzi; opis nie zasłania wartości.
- Lista oktaw podąża za następną nutą bez ponownego wykonywania akcji użytkownika i resetowania punktu startowego kolejności.

Końcowy APK na POCO Android 16: 198 asercji fokusu PL/EN, 1103 odtwarzania ekranów, 1831 interfejsu i 7 audio — zaliczone, bez nowej awarii podczas serii. Ustawienia zachowano po systemowej instalacji lokalnego APK. Wymowa i pełne gesty TalkBack przez człowieka oraz pełna aktualizacja 1.03 z GitHuba pozostają odrębnymi próbami. Wcześniejsza klasyfikacja Play Protect jest nadal niewyjaśniona. [Raport urządzenia](https://github.com/proszczok2002/Stroik/blob/main/android/DEVICE_TESTS.md).

versionName 1.03, versionCode 103; ten sam podpis. Aktualizuj bez odinstalowywania, aby zachować ustawienia.

## English

Improved TalkBack focus retention during normal use.

- Method, consonance, instrument and tap-listening changes show/hide the relevant fields without replacing the focused control.
- Tab buttons keep their identity; choosing the current tab again does not reset the form.
- Language/theme changes restore the corresponding control. Numeric labels/hints no longer mask entered values.
- The octave list follows the next note without replaying user actions or resetting the tuning-order anchor.

Final APK on POCO Android 16: 198 PL/EN focus assertions, 1103 lifecycle assertions, 1831 interface assertions and 7 audio checks passed, with no new crash during the sequence. System installation from the local APK retained settings. Human TalkBack pronunciation/full gestures and a complete GitHub update to 1.03 remain separate checks. The earlier Play Protect classification remains unexplained. [Device report](https://github.com/proszczok2002/Stroik/blob/main/android/DEVICE_TESTS.md).

versionName 1.03, versionCode 103; unchanged signing key. Update without uninstalling to retain settings.