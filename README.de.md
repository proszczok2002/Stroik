# Stroik 1.26 — Anleitung

- Für den ersten Download wählen Sie das vollständige Windows-ZIP. Entpacken Sie es in einen Ordner, um Stroik.exe, Stroik-Kalibrator.exe, Anleitungen und Prüfsummen zusammen zu erhalten.
- Fehlt der Kalibrator oder ist er veraltet, fragt Stroik beim Start nach dem Download. Bei Ablehnung wird für das fehlende Programm oder dieselbe Version nicht erneut gefragt. In den Einstellungen können Sie die Startprüfung abschalten und den Kalibrator weiterhin manuell prüfen.
- Nach Bestätigung prüft Stroik Größe, SHA-256 und Version und speichert den Kalibrator neben seiner EXE. Kalibrator 1.0.1 zeigt die Version im Fenstertitel.

- Stroik speichert Einstellungen in `Stroik.settings.json` neben `Stroik.exe`. Protokolle, Aktualisierungsdateien und die NVDA-Bibliothek liegen unter `Stroik-data`. Kopieren Sie zum Umzug den gesamten Ordner.
- Beim ersten Start behält Stroik das Profil, die Instrumentenstimmung und die Stimmgabelkalibrierung bei. Es sichert `%LOCALAPPDATA%\Stroik` unter `Stroik-data\migracja-*`. Vorhandene Einstellungen neben der EXE haben Vorrang. Der alte Ordner wird nach der Prüfung der Sicherung entfernt.
- Entpacken Sie die ZIP-Datei in einen beschreibbaren Ordner. Bei einer Dateisperre oder fehlenden Rechten bleiben die noch zu übertragenden Quelldaten erhalten. Stroik versucht die Migration beim nächsten Start erneut.
- Wenn die alte Einstellungsdatei selbst vorübergehend gesperrt ist, wartet Stroik mit dem Anlegen eines Profils. Nach dem Entsperren werden beim nächsten Start die vorhandenen Einstellungen übernommen.

- Aktivieren Sie im Lautsprecher-Hybrid die Zwei-Klopfzeichen-Option. Nach einem korrekt gestimmten Ergebnis bleibt die Referenz kurz stumm. Zwei Klopfzeichen genügen. Ohne Geste wird derselbe Ton wiederholt. Bei verstimmtem Ton gilt die eingestellte Pause von 0–60 Sekunden.
- Für Nachkriegsklaviere ist A4 = 440 Hz üblich. Vorkriegsinstrumente können für niedrigere Stimmung gebaut sein. Vor einer Erhöhung ein altes Klavier von einem Techniker beurteilen lassen.
- Unter **Instrumentenstimmung erfassen** wählen Sie Oktave, Ton und 1–10 Anschläge. Starten, jeden Ton ausklingen lassen und die Ansage abwarten. Der Mittelwert in Hertz wird dem ausgewählten Ton zugeordnet. Weiter über Liste, Taste oder nach dem Speichern mit zwei Klopfzeichen. Beenden stoppt das Mikrofon. Strg+S speichert die JSON-Datei. Aktivieren Sie die Stimmung für Messung und Referenz. Nicht erfasste Töne verwenden die Standardstimmung. Die Datei funktioniert auf beiden Plattformen.
- Der Windows-Kalibrator bietet 88 Töne in einer Liste, Obertöne 0–200 % und Lautstärke 0–150 % je Ton. 100 % entspricht Stroik. Alt+U spielt, Strg+S speichert JSON. Öffnen Sie die Datei in den Stroik-Einstellungen beider Plattformen. Sie ändert Klangfarbe und Pegel.
- Getrennte Regler für Stimmgabel und Mikrofon-Mithören sind in den Einstellungen. Windows führt aktive Quellen auch getrennt im Systemmixer. Das Standard-Ausgabegerät wird verwendet. Mithören nur mit Kopfhörern.

- Kontra-A erhält dieselbe Obertonstütze wie tiefere Töne. Beide Plattformen verwenden dieselbe Klangfarbe ohne Oktavverschiebung. Die Basswiedergabe hängt vom Lautsprecher ab.
- Der klassische Hybrid startet sofort ohne Anleitung. Für Anfänger werden zuerst Hinweise gelesen. Das Windows-Verhalten bleibt erhalten und wurde für Android ergänzt.
- Die Ergebnispause erlaubt 0–60 Sekunden, auch 1 und 2. Bei 0 beginnt die Referenz sofort nach dem tatsächlichen Sprachende. Neuer Standard ist 0; gespeicherte Windows-Pausen bleiben erhalten. Android bietet Schieberegler und Zahleneingabe.
- Android zeigt klassisch keine Tastenpositionen mehr. Methodenhinweise für Anfänger und schwarze Tasten mit Links/Rechts wurden ergänzt. Ergebnisse und Anleitung spricht die App auch mit TalkBack selbst; TalkBack steuert weiter die Navigation.

Zugängliches Windows-Stimmgerät für blinde, sehbehinderte und sehende Musiker.
Sprache und dunkles Design stehen in Einstellungen. Sprachen: Polski, English,
Deutsch, Русский, Français, Español. Die Auswahl bleibt in JSON-Profilen erhalten.

## Methoden

- Stimmgabel: Referenz zum Stimmen nach Gehör, manuell oder automatisch.
- Elektronisch: Mikrofon einschalten, einen Ton spielen und verklingen lassen.
  Das Ergebnis nennt Richtung und Abweichung.
- Lautsprecher-Hybrid: Referenz und Mikrofon wechseln sich ab. Nach dem Ergebnis
  wartet die Referenz auf Sprache und eine einstellbare Pause von 0–60 Sekunden
  (Standard 0). Die Anfängeransicht liest zuerst die Hinweise.
- Kopfhörer-Hybrid: Referenz und Mikrofon gleichzeitig. Optionales Mithören
  nur mit Kopfhörern einschalten, um Rückkopplung zu vermeiden.

## Bedienung

Alt+N oder Strg+N wählt den nächsten geplanten Ton, mit Umschalt den vorherigen.
In der Tonliste wählen Auf/Ab weiße, Links/Rechts nur schwarze Tasten. I/S wählt
erhöhte Töne. Alt+U schlägt manuell einmal an oder pausiert/setzt die Automatik
fort. Zahlenfelder akzeptieren Punkt und Komma; Auf/Ab ändert ihren Wert.
Strg+S speichert ein Profil, Strg+Q beendet. F1 öffnet Hinweise mit Tastaturzugang.
Die klassische Ansicht verzichtet auf Tastenbeschreibungen; die Anfängeransicht
enthält Erklärungen und Tastenpositionen.

Bei der Stimmgabel steuert allein das Kontrollkästchen für zwei Anschläge das
Mikrofon. Es braucht zuerst eine Ruhephase; Referenztöne zählen nicht als Geste.
Mit dieser Option beträgt das automatische Intervall mindestens 6,5 Sekunden.
Für NVDA unterstützt der Controller ab 2024.1 das Ende der Anfangshinweise;
Sprechstatus benötigt 2026.3. Andere Leser erhalten einen Startknopf nach den
Hinweisen. Details zur LGPL-Bibliothek: THIRD_PARTY_NOTICES.md.

## Instrumente und Messung

Flügel, Gitarre, Ukulele, Geige, Bass und chromatischer Modus. Klavierbereich
A0–C8; Anfangsziel G in der eingestrichenen Oktave. Quinten und Oktaven werden
nach oben gebildet: C4+G4 oder C4+C5. Gleichzeitige Messung ist experimentell;
Obertöne können einen zweiten Ton vortäuschen. Auch getrennt prüfen.
Gleichstufige Quinten dürfen schweben; Klaviere können gestreckte Oktaven brauchen.

Zu schwache oder starke Anschläge verfälschen Ergebnisse. Gleichmäßig spielen,
Übersteuerung vermeiden und Empfindlichkeit anpassen. 440 Hz ist Standard;
alte Instrumente vor Erhöhung der Spannung fachlich prüfen lassen. Manuelle
Mikrofonkorrektur nur gegen bekannte Referenz. Audio wird nicht gespeichert.
Obertöne helfen tiefen Referenztönen; kleine Lautsprecher bleiben begrenzt.
Rot bedeutet verstimmt, Grün innerhalb der Toleranz; Text bleibt verfügbar.

## Profile, Updates und Projekt

JSON speichert Methode, Instrument, Ziel, Sprache, Design und Update-Optionen.
Automatische Prüfung ist abschaltbar, ihr Intervall einstellbar. Neue Versionen
zeigen Änderungen; Download und SHA-256 werden geprüft. Die EXE wird ersetzt
und neu gestartet. Den Download in einem beschreibbaren Ordner aufbewahren.

Erstellt von Proszczok mit Codex. Idee: Witold. MIT-Lizenz.

https://github.com/proszczok2002/Stroik · https://proszczok.pl/

Unterstützung: https://paypal.me/proszczok · https://revolut.me/r_rosiejka

[Android](android/README.de.md) · [English](README.en.md) · [Polski](README.md)
