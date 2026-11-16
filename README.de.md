# Stroik 1.20 — Anleitung

Zugängliches Windows-Stimmgerät für blinde, sehbehinderte und sehende Musiker.
Sprache und dunkles Design stehen in Einstellungen. Sprachen: Polski, English,
Deutsch, Русский, Français, Español. Die Auswahl bleibt in JSON-Profilen erhalten.

## Methoden

- Stimmgabel: Referenz zum Stimmen nach Gehör, manuell oder automatisch.
- Elektronisch: Mikrofon einschalten, einen Ton spielen und verklingen lassen.
  Das Ergebnis nennt Richtung und Abweichung.
- Lautsprecher-Hybrid: Referenz und Mikrofon wechseln sich ab. Nach dem Ergebnis
  wartet die Referenz auf Sprache und eine einstellbare Pause von 3–60 Sekunden
  (Standard 8). Die Anfängeransicht liest zuerst die Hinweise.
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
