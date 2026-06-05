# WholeTrack

WholeTrack est une application WPF .NET Framework 4.8 pour visualiser et modifier une timeline de personnages historiques ou personnels.

## Description

L’application permet de :

- saisir des personnages avec prénom, nom, profession, genre, dates de naissance et de décès
- gérer les dates inconnues ou en avant Jésus-Christ (BC)
- afficher une timeline horizontale interactive
- cliquer sur un personnage pour charger ses informations dans le formulaire de gauche
- enregistrer les personnages dans un fichier `persons.json`
- conserver la position de la fenêtre et le niveau de zoom du slider entre les sessions

## Fonctionnalités

- Formulaire d’ajout et de modification de personnage
- Support des dates inconnues et des dates BC
- Timeline réactive avec jalons annuels, décennaux ou centennaux selon l’étendue des dates
- Persistance automatique des données et des paramètres d’interface
- Navigation par clic sur les éléments de la timeline

## Fichiers importants

- `WholeTrack/MainWindow.xaml` : définition de l’interface utilisateur
- `WholeTrack/MainWindow.xaml.cs` : logique de l’application, rendu de la timeline, traitement des événements
- `WholeTrack/Models/Person.cs` : modèle de données pour un personnage
- `WholeTrack/Models/UniversalDateTime.cs` : représentation de dates inconnues et BC
- `WholeTrack/bin/Debug/persons.json` : fichier de données généré à l’exécution (ne pas versionner si dynamique)
- `WholeTrack/windowsettings.json` : paramètres de fenêtre et de zoom sauvegardés

## Utilisation

1. Lancez l’application depuis Visual Studio ou l’exécutable dans `WholeTrack/bin/Debug`.
2. Saisissez un personnage dans le formulaire de gauche.
3. Définissez la date de naissance et la date de décès si disponible.
4. Cliquez sur `Ajouter` pour ajouter le personnage à la timeline.
5. Cliquez sur un point ou un nom dans la timeline pour charger le personnage et modifier ses données.
6. Utilisez le slider de zoom pour ajuster l’échelle de la timeline.

## Comportement de la timeline

- Si la période affichée est courte, la timeline affiche des jalons tous les ans.
- Pour des périodes intermédiaires, les jalons apparaissent tous les 10 ans.
- Pour des périodes longues, les jalons apparaissent tous les 100 ans.

## Notes

- Les données sont sauvegardées automatiquement dans `persons.json` à la fermeture de l’application.
- La position de la fenêtre et la valeur du zoom sont conservées entre les sessions.
- Le modèle `UniversalDateTime` gère les dates inconnues et les dates avant notre ère.
