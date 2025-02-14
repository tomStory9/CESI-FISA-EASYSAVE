eEasySave - Guide Utilisateur

## Introduction
EasySave est un logiciel permettant d'effectuer des sauvegardes de fichiers et de repertoires. Ce guide vous aidera a utiliser la version console et la version graphique du logiciel.

---

## Installation et Lancement

### Version Console :
1. Ouvrez l'explorateur de fichiers et accedez au repertoire EasySave.
2. Naviguez vers :
EasySaveConsole/bin/Debug/net9.0
3. Faites un clic droit dans le repertoire et selectionnez **Afficher d'autres options**.
4. Choisissez **Ouvrir dans le terminal**.
5. Dans le terminal, executez la commande :
./EasySave.exe
6. Tapez `help` pour afficher la liste des commandes disponibles.

### Version Graphique :
1. Ouvrez l'explorateur de fichiers et accedez au repertoire EasySave.
2. Naviguez vers :
EasySaveDesktop/bin/Debug/net9.0
3. Double-cliquez sur le fichier executable pour lancer l'application.

---

## Utilisation de la version console

### Commandes disponibles :
| Commande | Description |
|----------|------------|
| `./easysave create <name> <sourceDirectory> <targetDirectory> <type>` | Cree une nouvelle configuration de sauvegarde. |
| `./easysave remove <id>` | Supprime une configuration de sauvegarde a partir de son ID. |
| `./easysave <id1;id2;id3>` | Execute plusieurs sauvegardes specifiques. |
| `./easysave <id1-id3>` | Execute une serie de sauvegardes consecutives. |
| `./easysave help` | Affiche l'aide des commandes. |

---

## Explication des parametres :
- `<name>` : Nom de la sauvegarde (ex : `sauvegarde_journalière`).
- `<sourceDirectory>` : Chemin du repertoire source (ex : `C:\Users\Images`).
- `<targetDirectory>` : Chemin du repertoire de destination (ex : `D:\Backup\Images`).
- `<type>` : Type de sauvegarde (`Full` pour complete, `Differential` pour differentielle).
- `<id>` : Identifiant du travail de sauvegarde.
- `<id1;id2;id3>` : Liste d'ID de sauvegarde a executer en parallele.
- `<id1-id3>` : Intervalle d'ID de sauvegarde a executer successivement.

---

## Exemples d'utilisation :
- Creer une nouvelle sauvegarde complete :
./easysave create sauvegardePerso C:\Users\Exemple D:\Backup Full
- Executer une sauvegarde specifique :
./easysave 1
- Executer plusieurs sauvegardes non consecutives :
./easysave 1;3;5
- Executer plusieurs sauvegardes consecutives :
./easysave 2-4

---

## Credits
**Developpe par :**  
**Groupe 3 - ProSoft**

---

Ce README fournit les bases pour utiliser EasySave. Pour toute information complementaire, consultez la documentation complete.
