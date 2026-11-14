# Android changelog / Historia zmian Android

## 1.05 / 105 — device validation, 2026-11-14

- PL: Zaległe zmiany 1.04 przetestowano na Pixelu 11 (Android 17, Google TalkBack). Naprawiono utratę fokusu przy zmianie motywu — kolory są aktualizowane bez odtwarzania kontrolek. Wybrana dolna zakładka ma kontrastowy kolor i stan dostępności.
- EN: Validate the pending 1.04 interaction changes on Pixel 11 (Android 17, Google TalkBack). Fix theme-change focus loss by updating colors without recreating controls. Selected bottom tabs have clear color and accessibility state.
- PL/EN: White-key Up/Down, chromatic Left/Right, concise explicit note/microphone feedback, immediate fork refresh after consonance/target/reference changes, valid interval range/order, queued-playback cancellation, preserved acoustic decay guard and transient updater retries/logging.
- Current evidence and remaining locked-device gesture checks are recorded in DEVICE_TESTS.md. The stable manifest is still 1.03 pending the final validation gate; no 1.05 public release yet.

## 1.04 / 104 — candidate, 2026-11-12

- PL: Jeden przycisk mikrofonu, odczyt list, trzy dolne zakładki, instrument na stronie Stroik, suwak Hz, ustawienia głosu i ekranu, usługa do nasłuchu po wygaszeniu ekranu, krótkie nazwy i klawiatura ton/półton. Zmieniony poziom kamertonu. Telefon niedostępny — nowe ścieżki sprzętowe nieprzetestowane.
- EN: Microphone toggle, dropdown values, three bottom tabs, instrument in Tuner, Hz slider, voice/screen options, foreground capture for screen-off use, concise notes and keyboard navigation, fork gain shaping. Phone unavailable; new device paths untested.

## 1.03 / 103

- PL: Stałe kontrolki przy zmianie metody, współbrzmienia, instrumentu i nasłuchu stuknięć; stałe zakładki i przywracanie fokusu po zmianie języka/motywu. Poprawiono etykiety pól liczbowych oraz synchronizację nuty/oktawy bez resetowania kolejności.
- EN: Preserve controls and focus during option/tab changes; restore matching focus after language/theme changes. Fix numeric labels and note/octave synchronization without resetting the order anchor.
- Device: 198 focus assertions, 1103 lifecycle assertions, 1831 interface assertions and 7 audio checks passed. Human speech/gesture and full GitHub updater limits remain documented.
## 1.02 / 102

- Fix incompatible widget-state restoration after theme/configuration changes; retain the current tab, scroll and last result.
- Add real-device regression for legacy state and repeated PL/EN page recreation/theme switching.
- Original 1.0 crash reproduced on the device; candidate verification and Play Protect status are recorded in DEVICE_TESTS.md.
## 1.01 / 101

- Updater test release, signed with the same production key as 1.0.
- Updated installed release notes; tuning algorithms remain compatible.
- Two-row bottom navigation fits large text better; corrected Polish octave dropdown grammar.

## 1.0 / 100

- Native accessible PL/EN Android tuner: fork, microphone, hybrid modes, instruments, octaves, intervals, profiles and update installer.
- Device coverage on POCO F8 Ultra Android 16: 1831 interface assertions and 7 capture/playback checks with TalkBack enabled.
- Human pronunciation/gesture listening, real instrument accuracy and headphone monitoring are documented separately from automated coverage.
