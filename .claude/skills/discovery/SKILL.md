---
name: discovery
description: Diagnostiquer le besoin d'une feature avant toute solution. Utiliser à la prise d'une carte de feature en colonne `Discovery`, quand on démarre un nouveau cap de la trajectoire, ou quand on demande de cadrer un besoin avant d'en discuter la faisabilité.
---

> **Draft non éprouvé.** Écrit d'après l'état de l'art, pas récolté sur une exécution réelle —
> `D-039` demande l'inverse. À confronter au premier usage ; en cas de désaccord,
> `docs/methode/journal-frictions.md` prime sur ce fichier.

Un **diagnostic**, pas une prescription : établir ce qui ne va pas et pour qui, ouvrir plusieurs
hypothèses, n'en retenir aucune. `Spec` est le moment de prescrire — la confondre avec cette étape
fait arbitrer en cadrant, ce qu'elle s'interdit précisément.

## 1. Isoler le symptôme

Écrire le besoin comme un fait observé, jamais comme une solution déguisée. « Il faut un cache »
est une prescription ; « l'écran met quatre secondes à s'ouvrir » est un symptôme. Si la phrase
nomme déjà un composant ou un verbe d'implémentation, elle décrit un traitement — la reformuler.

Complet quand : le besoin tient en une phrase, sans solution nommée dedans.

## 2. Interroger l'humain

Invoquer le skill `interrogatoire` pour établir, produit et UX autour de la table :
**pour qui** ce besoin se pose, **pourquoi maintenant** — sa place dans la trajectoire, ce qu'il
débloque, ce que coûte l'inaction.

Complet quand : les trois réponses sont écrites dans les mots de l'humain, pas déduites.

## 3. Ouvrir les pistes, sans les instruire

Nommer plusieurs directions possibles. Une seule piste n'est pas une ouverture, c'est un choix
maquillé.

La frontière se franchit sans qu'on s'en aperçoive : énoncer un **fait connu** sur une piste est
légitime (« ce transport est incompatible avec la résidence actuelle ») ; en tirer une
**conséquence** ne l'est pas (« donc cette piste ne convient pas »). Le second est déjà un
arbitrage, même déguisé en constat.

**Ne jamais produire un tableau de pistes avec une colonne de commentaire** — la colonne appelle
l'argument, l'argument appelle la conclusion, et l'ouverture se referme sans qu'on l'ait décidé.
Écrire chaque piste en paragraphe autonome : ce qu'elle est, ce qu'on en sait factuellement. Rien
après.

Complet quand : au moins deux pistes sont écrites, et aucune ne porte de raison de la préférer ou
de l'écarter.

## 4. Trancher la sortie

Tester le critère opposable : tenter mentalement le premier arbitrage. S'il faut d'abord
redemander pour qui ou pourquoi maintenant, revenir à l'étape 2 — le manque est en amont.

Deux issues légitimes :
- **Continuer** : rien n'est encore choisi, la carte peut être tirée en `Spec`.
- **Annuler** : *on ne fait pas*, ou *le besoin n'est pas celui qu'on croyait*. Écrire la phrase
  qui dit pourquoi — sans elle, la feature se re-proposera dans six mois.

Complet quand : l'une des deux issues est actée, justifiée si c'est l'annulation.

## 5. Publier le document

Écrire un document Linear **distinct** de la future spec — jamais une section qu'elle prolongera.
Il s'adresse à qui le lit dans le tracker, pas au dépôt :
- des **liens**, jamais des chemins de fichiers ;
- aucune numérotation interne périssable (« ce que débloque `2·2c` » ne dira plus rien dans trois
  mois — écrire la conséquence, lier la carte qui la porte) ;
- aucun méta-commentaire de méthode (« ceci est une ouverture, pas un choix » explique le gabarit
  au lieu du sujet — poser la question directement à la place).

Si le diagnostic a fait apparaître quelque chose que les étapes précédentes ne capturent pas — un
cadrage à défaire, deux besoins sous un même titre — l'écrire aussi : ça se perd sinon.

Complet quand : le document est publié, attaché à la feature, et ne contient aucune trace
d'arbitrage — pas de piste écartée, pas de coût, pas de faisabilité tranchée.
