---
name: interrogatoire
description: Interroger l'humain sans relâche, une décision à la fois, jusqu'à un accord partagé — le fait, l'agent le cherche seul ; la décision revient à l'humain. Utiliser quand un skill ou une tâche en cours bute sur une décision que seul l'humain peut trancher, ou quand l'utilisateur demande explicitement d'être interrogé sur un sujet avant d'agir.
---

> **Draft non éprouvé.** Écrit d'après l'état de l'art, pas récolté sur une exécution réelle —
> `D-039` demande l'inverse. À confronter au premier usage ; en cas de désaccord,
> `docs/methode/journal-frictions.md` prime sur ce fichier.

Mène l'interrogatoire sur chaque aspect en jeu, jusqu'à un accord partagé. Parcours l'arbre de
décision branche par branche, en résolvant les dépendances entre décisions une par une — jamais
en éparpillant plusieurs questions à la fois.

## Fait ou décision

Avant de poser une question, classe-la.

- **C'est un fait** si l'environnement le contient déjà — le dépôt, le tracker, le système de
  fichiers, un outil. Va le chercher toi-même.
- **C'est une décision** si trancher engage un choix que rien dans l'environnement ne fixe. Pose-la
  à l'humain, et seulement à lui.

Critère d'achèvement : chaque information encore manquante est classée dans l'une des deux
catégories, sans reste — aucune n'est devinée faute d'avoir tranché laquelle des deux elle était.

## Une question, une réponse recommandée

Pose une seule question à la fois, et attends la réponse avant de poursuivre — plusieurs questions
d'un coup désoriente l'humain plutôt que de lui faire gagner du temps.

Pour chaque question, propose ta **réponse recommandée**. L'humain arbitre en approuvant,
corrigeant ou renversant ta proposition, plutôt que d'avoir à formuler une réponse depuis rien.

## Pas de plafond

Beaucoup de questions parce que le sujet est sous-spécifié est le fonctionnement normal : trois
questions suffisent à un sujet simple, cinquante peuvent être nécessaires à un sujet touffu.
Continue tant que des décisions restent à trancher, quel que soit le compte déjà atteint.

Une question qui **redonde** avec une réponse déjà obtenue est un défaut de préparation, pas un
motif d'arrêt : reformule-la ou saute-la, plutôt que de la reposer telle quelle.

## Toujours vers l'humain, jamais à sa place

Chaque question posée attend la réponse de l'humain, elle seule. Une question sans réponse humaine
reste ouverte jusqu'au tour où quelqu'un y répond — aucune évidence apparente, aucune déduction de
ta part ne la ferme à sa place.

## Le gate de fin

N'agis pas tant que l'humain n'a pas confirmé explicitement que la compréhension est partagée. Une
dernière question de synthèse clôt l'interrogatoire : récapitule les décisions prises et demande la
confirmation. Le silence ou une réponse partielle ne valent pas confirmation.
