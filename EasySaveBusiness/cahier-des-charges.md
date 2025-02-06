Le logiciel est une application Console utilisant .Net Core.

    Le logiciel doit permettre de créer jusqu'à 5 travaux de sauvegarde

    Un travail de sauvegarde est défini par

        Un nom de sauvegarde

        Un répertoire source

        Un répertoire cible

        Un type (sauvegarde complète , sauvegarde différentielle)

    Le logiciel doit être utilisable à minima par des utilisateurs anglophones et Francophones

    L'utilisateur peut demander l'exécution d'un des travaux de sauvegarde ou l'exécution séquentielle de l'ensemble des travaux.

    Le programme peut être lancé par une ligne de commande

        exemple 1 : 1-3 pour exécuter automatiquement les sauvegardes 1 à 3

        exemple 2 : 1 ;3 pour exécuter automatiquement les sauvegardes 1 et 3

    Les répertoires (sources et cibles) pourront être sur :

        Des disques locaux

        Des disques Externes

        Des Lecteurs réseaux

    Tous les éléments d'un répertoire source (fichiers et sous-répertoires ) doivent être sauvegardé.

    Fichier Log journalier :

    Le logiciel doit écrire en temps réel dans un fichier log journalier toutes les actions réalisées durant les sauvegardes (transfert d'un fichier, création d'un répertoire, ...).

    Les informations minimales attendues sont :

        Horodatage

        Nom de sauvegarde

        Adresse complète du fichier Source (format UNC)

        Adresse complète du fichier de destination (format UNC)

        Taille du fichier

        Temps de transfert du fichier en ms (négatif si erreur)

        Exemple de contenu: 2020-12-17.json [json, 991 o]

        L'écriture des informations dans un fichier Log journalier est une fonctionnalité qui servira à d'autres projets. Il vous est demandé de développer cette fonctionnalité dans une Dynamic Link Library. Toutes les évolutions de cette librairie doivent rester compatibles avec la version 1.0 du logiciel.

    Ficher Etat temps réel : Le logiciel doit enregistrer en temps réel, dans un fichier unique, l'état d'avancement des travaux de sauvegarde et l'action en cours. Les informations à enregistrer pour chaque travail de sauvegarde sont à minima:

        Appellation du travail de sauvegarde

        Horodatage de la dernière action

        Etat du travail de Sauvegarde (ex : Actif, Non Actif...)

    Si le travail est actif :

        Le nombre total de fichiers éligibles

        La taille des fichiers à transférer

        La progression

            Nombre de fichiers restants

            Taille des fichiers restants

            Adresse complète du fichier Source en cours de sauvegarde

            Adresse complète du fichier de destination

        exemple  : state.json [json, 762 o]

    Les emplacements des deux fichiers (log journalier et état temps réel) devront être étudiés pour fonctionner sur les serveurs de nos clients. De ce fait, les emplacements du type « c:\temp\ » sont à proscrire.

    Les fichiers (log journalier et état) et les éventuels fichiers de configuration seront au format JSON. Pour permettre une lecture rapide via Notepad, il est nécessaire de mettre des retours à la ligne entre les éléments JSON. Une pagination serait un plus.

Remarque importante : si le logiciel donne satisfaction, la direction vous demandera de développer une version 2.0 utilisant une interface graphique (basée sur l'architecture MVVM)