---
name: decoupage
description: Découper une spec de feature validée en incréments, avec leurs frontières et leur ordre. À invoquer à l'étape Découpage du flux — juste après l'accord sur une spec, pour produire les issues de niveau incrément avant qu'un plan d'archi ne démarre. Ne pas l'utiliser pour concevoir un incrément (c'est `plan-archi`) ni pour écrire une test list de pas (c'est `prendre-un-pas`).
---

> **Draft non éprouvé.** Écrit d'après l'état de l'art, pas récolté sur une exécution réelle —
> `D-039` demande l'inverse. À confronter au premier usage ; en cas de désaccord,
> `docs/methode/journal-frictions.md` prime sur ce fichier.

# Découpage — de la spec aux incréments

Le mot qui guide cette skill est **frontière**. Découper, ce n'est pas produire une liste : c'est
tracer, entre des incréments, des lignes qui n'existaient pas — et les déposer avant qu'elles ne
disparaissent avec la session qui les a vues. Chaque étape ci-dessous se termine sur un critère
qu'on peut vérifier, pas sur une intention.

## 1. Geler la recette de la spec

Le point d'entrée est la spec validée (`tickets.md` §2.2), recette comprise. Chaque clause de la
recette doit atterrir dans au moins un incrément — « si une part de la recette n'atterrit dans
aucun incrément, le découpage a un trou » (`tickets.md` §3 q.5).

**Critère** : chaque clause de la recette est assignée à un incrément nommé. Une clause sans
incrément est un trou à combler avant de continuer.

## 2. Trancher verticalement, jamais par couche

Chaque incrément candidat traverse **toutes** les couches qu'il touche — jamais une couche isolée
(« la persistance », « l'UI ») livrée seule. Deux tests, complémentaires, à appliquer à chaque
candidat :

- **Le niveau** (`tickets.md` §1) : livré seul et arrêté là, le **rôle produit** le remarquerait-il ?
  Non → c'est un pas, pas un incrément.
- **La complétude de la traversée** : le candidat est-il démontrable seul, sans dépendre d'un autre
  incrément non encore fait pour être recettable ? Non → c'est une tranche horizontale, à refondre
  avec ses voisines.

**Critère** : chaque incrément retenu passe les deux tests. Un candidat fusionné ou scindé pour
cette raison le porte dans son historique de découpage, pas seulement dans le résultat final.

## 3. Dimensionner sur une session fraîche

Une tranche qui traverse bien les couches peut rester trop grosse. L'unité opposable : un incrément
dont le contexte ne tiendrait pas dans une **session fraîche** est trop gros — le scinder.

**Critère** : pour chaque incrément, on peut dire en quelques phrases ce qu'une session neuve
devrait savoir pour l'exécuter. Si la réponse déborde, scinder plutôt que publier tel quel.

## 4. Ordonner par les arêtes de blocage

Chaque incrément porte son `blockedBy` — les incréments qui doivent être `Done` avant qu'il puisse
commencer. Un incrément sans blocage naît en `Todo` ; un incrément avec un blocage ouvert naît en
`Backlog` (`tickets.md` §6.2). Il n'y a pas d'ordre total à écrire, seulement des arêtes.

**Critère** : chaque incrément porte une liste `blockedBy` explicite, vide ou non — et sa colonne de
naissance en découle mécaniquement, sans arbitrage supplémentaire.

## 5. Déposer le hors-périmètre en nommant les frères

Chaque incrément répond à la question 6 de `tickets.md` §3 : ce qui reste explicitement dehors,
écrit **en regard des frères** plutôt que dans l'absolu — nommer l'incrément voisin qui porte ce
qui n'est pas ici vaut mieux qu'une abstraction (« hors périmètre »).

**Critère** : tout incrément qui a au moins un frère en nomme au moins un dans son hors-périmètre.
Un incrément seul dans sa feature le dit explicitement plutôt que de laisser la question vide.

## 6. Faire trancher l'humain sur la granularité — avant publication

Rien ne se publie avant accord. Trois questions à poser, dans l'ordre, et à itérer jusqu'à
accord : la granularité convient-elle (trop grossière / trop fine) ? les arêtes de blocage
sont-elles justes ? faut-il fusionner ou scinder des incréments ?

**Critère** : un accord explicite de l'humain porte sur le découpage complet — pas incrément par
incrément — avant toute création de carte.

## 7. Créer les cartes et transitionner la feature

Une fois l'accord obtenu : chaque incrément devient une issue rattachée au projet de la feature,
`blockedBy` posé, née en `Todo` ou en `Backlog` selon l'étape 4. La feature passe de `Spec` à
`In Progress`.

**Critère** : chaque incrément retenu existe comme carte, et la feature porte le nouveau statut.

## 8. Ce que le découpage ne fait pas

Le découpage capture des frontières, il ne conçoit pas. Le **plan d'archi** attend l'étape
`Planning`, à la prise de chaque incrément — pas ici, faute d'en savoir assez : ce qu'on apprend en
faisant le premier incrément change ce qu'on sait au quatrième. La **test list**, elle, attend la
prise du pas. Un découpage qui prescrit le comment a mangé le plan de l'incrément.

## Annexe — si le travail à découper est un refactor large

*(à lire seulement quand la feature est un refactor plutôt qu'une fonctionnalité — la tranche
verticale de l'étape 2 ne s'y applique pas)*

Traiter en **expand–contract** : des lots dimensionnés par **rayon d'impact** (un paquet, un
dossier) plutôt qu'en tranches verticales, avec un incrément d'**intégration final** où la suite
verte n'est promise que là. Les lots intermédiaires n'ont pas chacun à être vert de bout en bout —
seul le dernier l'engage.
