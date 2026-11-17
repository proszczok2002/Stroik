# Stroik Android 1.09 — Guide

- Le la de contre-octave reçoit le même soutien harmonique que les notes inférieures. Les deux plateformes utilisent le même timbre sans transposition. L’audibilité du grave dépend des haut-parleurs.
- L’hybride classique démarre immédiatement sans tutoriel. En vue débutant, les conseils sont lus avant la référence. Le comportement Windows est conservé et ajouté à Android.
- La pause après résultat accepte 0–60 secondes, dont 1 et 2. À 0, la référence reprend dès la fin réelle de la parole. Nouvelle valeur par défaut : 0 ; les pauses Windows enregistrées sont conservées. Android propose un curseur et la saisie numérique.
- Le résultat est lu intégralement avant la référence et l’autorisation du passage. En hybride, une note juste puis deux attaques de hauteur confirmée sont nécessaires. Le diapason seul conserve son geste acoustique.
- Android masque les positions dans la liste classique. Ajout des descriptions des méthodes pour débutants et du choix des touches noires avec gauche/droite. La synthèse de l’application lit résultats et tutoriels même avec TalkBack pour connaître la fin de parole ; TalkBack gère la navigation.

Accordeur Android accessible aux musiciens aveugles, malvoyants et voyants.
Langue et thème sombre se choisissent dans les paramètres. Langues : Polski,
English, Deutsch, Русский, Français, Español. Le choix est conservé dans les profils JSON.

## Méthodes

Diapason : référence manuelle ou automatique pour accorder à l’oreille. Électronique : activez le microphone, jouez une note puis étouffez-la ; le résultat indique le sens et l’écart. Hybride sur haut-parleurs : référence, extinction, mesure de l’instrument, lecture du résultat puis répétition. Hybride au casque : référence et microphone simultanés. Activez le retour du microphone uniquement au casque pour éviter la rétroaction acoustique.

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
par SHA-256. Android demande confirmation avant chaque installation.

Réalisé par Proszczok avec Codex. Idée originale : Witold. Licence MIT.

https://github.com/proszczok2002/Stroik · https://proszczok.pl/

Soutenir : https://paypal.me/proszczok · https://revolut.me/r_rosiejka


## Android / TalkBack

Utilisez TalkBack pour naviguer. Les trois boutons de page occupent une ligne en bas. Les curseurs modifient les nombres ; un double appui ouvre la saisie numérique. Un seul bouton active ou désactive le microphone. Écran éteint, la synthèse système peut lire les résultats ; vitesse et maintien de l’écran se règlent dans les paramètres. La voix choisie doit être installée dans Android. Le mode hybride sur haut-parleurs attend sa propre synthèse vocale. Les mises à jour demandent toujours la confirmation d’installation Android ; aucune installation silencieuse.
