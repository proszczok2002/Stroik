# Stroik 1.26

## Polski

Stroik 1.26

- Stroik zapisuje ustawienia w `Stroik.settings.json` obok `Stroik.exe`. Logi, pliki aktualizatora i biblioteka NVDA trafiają do `Stroik-data`. Aby przenieść program i dane, skopiuj cały folder.
- Przy pierwszym uruchomieniu Stroik zachowuje dotychczasowy profil, mapę instrumentu i kalibrację. Zawartość `%LOCALAPPDATA%\Stroik` trafia do kopii w `Stroik-data\migracja-*`. Jeśli ustawienia są już obok EXE, zachowują pierwszeństwo. Stroik usuwa stary katalog dopiero po sprawdzeniu kopii.
- Rozpakuj ZIP do zapisywalnego folderu. Jeśli migrację przerwie brak dostępu lub blokada pliku, Stroik zachowa dane źródłowe wymagające dokończenia migracji i ponowi próbę przy następnym starcie.
- Jeśli sam plik ustawień w AppData jest chwilowo zablokowany, Stroik poczeka z utworzeniem nowego profilu. Po odblokowaniu pliku i ponownym uruchomieniu zachowa dotychczasowe ustawienia.

## English

Stroik 1.26

- Stroik saves preferences to `Stroik.settings.json` next to `Stroik.exe`. Logs, updater files and the NVDA library are under `Stroik-data`. Copy the whole folder to move the program and its data.
- On first launch Stroik preserves the profile, instrument map and fork calibration. It copies `%LOCALAPPDATA%\Stroik` to a backup under `Stroik-data\migracja-*`. Existing settings next to the EXE take priority. Stroik removes the old directory once it has verified the copy.
- Extract the ZIP to a writable folder. If a file is locked or access is denied, Stroik keeps the source data that still needs migrating and retries on the next launch.
- If the old settings file itself is temporarily locked, Stroik waits before creating a profile. Once the file is unlocked, the next launch activates the existing settings.

## Deutsch

Stroik 1.26

- Stroik speichert Einstellungen in `Stroik.settings.json` neben `Stroik.exe`. Protokolle, Aktualisierungsdateien und die NVDA-Bibliothek liegen unter `Stroik-data`. Kopieren Sie zum Umzug den gesamten Ordner.
- Beim ersten Start behält Stroik das Profil, die Instrumentenstimmung und die Stimmgabelkalibrierung bei. Es sichert `%LOCALAPPDATA%\Stroik` unter `Stroik-data\migracja-*`. Vorhandene Einstellungen neben der EXE haben Vorrang. Der alte Ordner wird nach der Prüfung der Sicherung entfernt.
- Entpacken Sie die ZIP-Datei in einen beschreibbaren Ordner. Bei einer Dateisperre oder fehlenden Rechten bleiben die noch zu übertragenden Quelldaten erhalten. Stroik versucht die Migration beim nächsten Start erneut.
- Wenn die alte Einstellungsdatei selbst vorübergehend gesperrt ist, wartet Stroik mit dem Anlegen eines Profils. Nach dem Entsperren werden beim nächsten Start die vorhandenen Einstellungen übernommen.

## Русский

Stroik 1.26

- Stroik хранит настройки в `Stroik.settings.json` рядом со `Stroik.exe`. Журналы, файлы обновления и библиотека NVDA находятся в `Stroik-data`. Для переноса программы с данными скопируйте всю папку.
- При первом запуске Stroik сохраняет профиль, карту строя и калибровку камертона. Он копирует `%LOCALAPPDATA%\Stroik` в резервную папку `Stroik-data\migracja-*`. Настройки рядом с EXE имеют приоритет. Старая папка удаляется после проверки копии.
- Распакуйте ZIP в папку с правом записи. Если файл заблокирован или доступ запрещён, оставшиеся исходные данные сохраняются. Stroik повторит перенос при следующем запуске.
- Если сам старый файл настроек временно заблокирован, Stroik откладывает создание профиля. После разблокировки файла при следующем запуске будут загружены прежние настройки.

## Français

Stroik 1.26

- Stroik enregistre ses réglages dans `Stroik.settings.json`, à côté de `Stroik.exe`. Les journaux, les fichiers de mise à jour et la bibliothèque NVDA sont dans `Stroik-data`. Copiez tout le dossier pour déplacer le logiciel avec ses données.
- Au premier lancement, Stroik conserve le profil, le relevé de l’instrument et le calibrage du diapason. Il sauvegarde `%LOCALAPPDATA%\Stroik` dans `Stroik-data\migracja-*`. Les réglages présents à côté de l’EXE sont prioritaires. Une fois la copie vérifiée, Stroik supprime l’ancien dossier.
- Extrayez le ZIP dans un dossier accessible en écriture. Si un fichier est verrouillé ou que l’accès est refusé, Stroik conserve les données sources restantes et réessaie au prochain lancement.
- Si l’ancien fichier de réglages est temporairement verrouillé, Stroik attend avant de créer un profil. Une fois le fichier déverrouillé, les réglages existants sont repris au prochain lancement.

## Español

Stroik 1.26

- Stroik guarda los ajustes en `Stroik.settings.json`, junto a `Stroik.exe`. Los registros, los archivos de actualización y la biblioteca de NVDA están en `Stroik-data`. Para trasladar el programa con sus datos, copia toda la carpeta.
- En el primer inicio, Stroik conserva el perfil, el mapa del instrumento y la calibración del diapasón. Hace una copia de `%LOCALAPPDATA%\Stroik` en `Stroik-data\migracja-*`. Los ajustes junto al EXE tienen prioridad. Solo borra la carpeta antigua tras comprobar la copia.
- Extrae el ZIP en una carpeta con permiso de escritura. Si hay un archivo bloqueado o se deniega el acceso, Stroik conserva los datos de origen pendientes y vuelve a intentarlo en el próximo inicio.
- Si el archivo de ajustes antiguo está bloqueado temporalmente, Stroik espera antes de crear un perfil. Cuando se desbloquee, el siguiente inicio recuperará los ajustes existentes.
