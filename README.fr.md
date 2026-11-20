# Stroik 1.23 — Guide

- Stroik enregistre ses réglages dans `Stroik.settings.json`, à côté de `Stroik.exe`. Copiez les deux fichiers pour déplacer le logiciel et ses réglages sur un autre PC ou une clé USB.
- Au premier lancement, si le fichier manque à côté du programme, Stroik copie le profil depuis `%LOCALAPPDATA%\Stroik\settings.json`, y compris le relevé actif et le calibrage du diapason. L’original est conservé. Un fichier existant à côté du programme est prioritaire.
- Si le dossier du programme ne permet pas de créer un fichier, Stroik utilise le profil Windows. Pour les réglages portables, lancez Stroik depuis un dossier accessible en écriture.

- En hybride haut-parleurs, activez l’option des deux frappes. Après un résultat juste, la référence reste silencieuse pour le geste. Sans geste, elle répète la même note. Si la note est fausse, la pause configurée de 0–60 secondes s’applique.
- La4 = 440 Hz est la référence habituelle des pianos d’après-guerre. Les instruments d’avant-guerre peuvent être conçus pour un diapason plus bas. Faites examiner un vieux piano avant de relever son diapason.
- Dans **Relevé de l’accord**, choisissez octave, note et 1–10 frappes. Lancez le relevé. Laissez chaque note s’éteindre et attendez le message. La fréquence moyenne en hertz est associée à la note choisie. Passez à la suivante par liste, bouton ou deux frappes après l’enregistrement. Terminer arrête le micro. Ctrl+S exporte le JSON. Activez le relevé pour la mesure et la référence. Les notes absentes gardent l’accord standard. Le fichier est commun aux deux plateformes.
- Le calibrateur Windows réunit 88 notes dans une liste, avec harmoniques 0–200 % et volume 0–150 % par note. 100 % correspond au réglage initial de Stroik. Alt+U joue, Ctrl+S exporte en JSON. Ouvrez ce fichier dans les paramètres Stroik sur les deux plateformes pour ajuster timbre et niveau.
- Volumes séparés du diapason et du retour micro dans les paramètres. Windows présente aussi deux sessions actives dans le mixeur système. La sortie audio par défaut est utilisée. Écoutez le retour micro au casque.

- Le la de contre-octave reçoit le même soutien harmonique que les notes inférieures. Les deux plateformes utilisent le même timbre sans transposition. L’audibilité du grave dépend des haut-parleurs.
- L’hybride classique démarre immédiatement sans tutoriel. En vue débutant, les conseils sont lus avant la référence. Le comportement Windows est conservé et ajouté à Android.
- La pause après résultat accepte 0–60 secondes, dont 1 et 2. À 0, la référence reprend dès la fin réelle de la parole. Nouvelle valeur par défaut : 0 ; les pauses Windows enregistrées sont conservées. Android propose un curseur et la saisie numérique.
- Android masque les positions dans la liste classique. Ajout des descriptions des méthodes pour débutants et du choix des touches noires avec gauche/droite. La synthèse de l’application lit résultats et tutoriels même avec TalkBack pour connaître la fin de parole ; TalkBack gère la navigation.

Accordeur Windows accessible aux musiciens aveugles, malvoyants et voyants.
Langue et thème sombre se choisissent dans les paramètres. Langues : Polski,
English, Deutsch, Русский, Français, Español. Le choix est conservé dans les profils JSON.

## Méthodes

- Diapason : référence pour accorder à l’oreille, manuelle ou automatique.
- Électronique : activez le microphone, jouez une note et étouffez-la.
  Le résultat indique le sens et l’écart.
- Hybride sur haut-parleurs : référence et microphone alternent. Après le résultat,
  le diapason attend la fin de la parole et une pause réglable de 0–60 secondes
  (0 par défaut). En vue débutant, les conseils sont lus avant la référence.
- Hybride au casque : référence et microphone simultanés. Activez le retour
  uniquement au casque pour éviter la rétroaction acoustique.

## Utilisation

Alt+N ou Ctrl+N sélectionne la note suivante du plan, avec Maj la précédente.
Dans la liste, haut/bas choisissent les touches blanches, gauche/droite uniquement
les noires. I/S sélectionnent les notes altérées. Alt+U frappe une fois en manuel
ou met en pause/reprend l’automatisme. Les champs numériques acceptent point et
virgule ; haut/bas change leur valeur. Ctrl+S enregistre un profil, Ctrl+Q ferme.
F1 ouvre les conseils accessibles au clavier. La vue classique reste concise ;
la vue débutant comprend les explications et la position des touches.

En mode diapason, seule la case des deux frappes commande le microphone.
Un silence initial est nécessaire ; la référence ne compte pas comme geste.
Avec cette option, l’intervalle automatique est d’au moins 6,5 secondes.
NVDA 2024.1 ou ultérieur signale la fin des conseils initiaux ; l’état de parole
exige NVDA 2026.3. Les autres lecteurs disposent d’un bouton pour démarrer après
les conseils. Bibliothèque LGPL : THIRD_PARTY_NOTICES.md.

## Instruments et mesure

Piano, guitare, ukulélé, violon, basse et mode chromatique. Tessiture du piano
A0–C8, cible initiale sol4 selon la numérotation scientifique. Quinte et octave
se construisent vers le haut : do4+sol4 ou do4+do5 dans cette numérotation.
La mesure simultanée est expérimentale : une harmonique peut imiter une seconde
note. Vérifiez aussi séparément. Les quintes tempérées peuvent battre ;
le piano peut exiger des octaves étirées.

Des frappes trop faibles ou fortes faussent les mesures. Jouez régulièrement,
évitez la saturation, réglez la sensibilité. 440 Hz est la norme ; faites examiner
un instrument ancien avant d’augmenter la tension. Corrigez le microphone
uniquement avec une référence connue. Le son n’est pas enregistré. Des harmoniques
soutiennent les notes graves ; les petits haut-parleurs restent limités.
Rouge signifie désaccordé, vert dans la tolérance ; le texte reste disponible.

## Profils, mises à jour et projet

JSON conserve méthode, instrument, note, langue, thème et options de mise à jour.
La vérification automatique peut être désactivée et son intervalle réglé.
Les nouvelles versions affichent les modifications ; le téléchargement est vérifié
par SHA-256. Le fichier EXE est remplacé et l’application redémarre.
Conservez le programme dans un dossier accessible en écriture.

Réalisé par Proszczok avec Codex. Idée originale : Witold. Licence MIT.

https://github.com/proszczok2002/Stroik · https://proszczok.pl/

Soutenir : https://paypal.me/proszczok · https://revolut.me/r_rosiejka

[Android](android/README.fr.md) · [English](README.en.md) · [Polski](README.md)
