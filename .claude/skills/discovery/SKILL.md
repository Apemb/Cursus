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

## 6. Relire contre ce qui sera opposé

Avant de reposer la carte en `Review Requested`, ouvrir **`docs/methode/dod/feature/discovery.md`**
et passer le document contre ses cases : c'est le référentiel que `revue-discovery` applique, clause
par clause. **Ne jamais recopier ses cases ici** — une copie d'un référentiel diverge de lui en
silence (journal 54).

⚠️ **Le faire une fois le document fini, pas en l'écrivant.** Viser une grille en rédigeant produit
un document qui coche ; la lire après attrape ce que l'écriture a laissé.

Complet quand : les cases ont été passées sur le document fini.

## 7. Faire relire par un tiers — avant de lâcher la carte

L'étape 6 est une **auto-évaluation**, et elle a la limite de toutes : elle ne voit pas ce que son
auteur ne peut pas voir. Ce que le développement obtient de sa suite de tests, un document ne
l'obtient que d'un relecteur.

**Lancer un agent de relecture**, qui lance lui-même **un sous-agent par axe, en parallèle** :

- **les axes sont ceux de la revue qui suivra** — ici les **trois** de
  [`revue-discovery`](../revue-discovery/SKILL.md) §2 : Complétude, Non-arbitrage, Adresse au
  lecteur. Relire contre les axes qu'on subira ensuite est ce qui rend la relecture comparable à la
  revue ;
- **session neuve, artefact seul** (`D-039`), pour l'agrégateur comme pour chaque axe — leur donner
  le document et le référentiel (`docs/methode/dod/feature/discovery.md`), **jamais** le
  raisonnement qui a produit le document, ni ce qu'on soupçonne d'être faible. Un relecteur à qui on
  annonce la réponse la trouve ;
- **jamais fusionnés** : chaque constat garde son axe, sa citation et sa nature ;
- **il relit, il ne réécrit pas** : il liste les divergences, chacune avec son extrait et sa clause,
  et s'abstient explicitement là où le référentiel manque.

⚠️ **L'agrégateur n'est pas un greffier, et il n'est pas toi.** Il revérifie les citations porteuses
avant de rendre, et il a le droit d'affaiblir ou de retirer un constat de ses propres axes —
mesuré : sur le premier essai, il a retiré un sous-point faux et produit la contre-preuve d'un
autre. C'est aussi pour cela qu'il est **distinct du binôme** : celui qui a écrit le document ne
peut pas être celui qui décide quels constats survivent à l'agrégation.

**Le motif du nombre est mesuré, pas supposé** (`D-073`). Sur le même artefact et le même modèle, un
relecteur unique a rendu **quatre** constats de fond ; trois axes parallèles en ont rendu **dix**,
dont les deux plus graves — que le relecteur unique n'avait pas vus. Un seul agent n'est pas une
version économique de ce dispositif, c'en est une version qui rate l'essentiel.

**Son rapport se dépose avant toute correction.** ⚠️ C'est la clause qui compte, et elle n'est pas
une formalité : un cycle interne qui converge en silence supprime la trace de ce qu'il a rattrapé,
et `D-039` fait du **journal des frictions la source qui écrit les skills**. Corriger sans déposer
tarit la seule boucle d'amélioration du dépôt. Ce qui a valeur d'enseignement va dans
`docs/methode/journal-frictions.md` ; le rapport lui-même reste sous les yeux du binôme.

⚠️ **Pas de remarque Linear ici.** Les remarques posées sur la carte appartiennent au cycle de revue
et se comptent (`cursus linear comment list --unresolved`) : en poser avant `Review Requested`
fausserait le compteur de la revue qui suit.

**Puis solder, une par une** — reprise faite, ou refus motivé. C'est le **binôme** qui solde, jamais
un sous-agent : en `Discovery`, reprendre c'est arbitrer, et l'arbitrage appartient au binôme
(`D-053`). Un constat qu'on ne sait pas trancher **empêche de poser `Review Requested`** : il remonte
à l'humain, qui est déjà à la table.

Complet quand : le rapport existe, chaque constat est soldé ou explicitement remonté, et
`Review Requested` n'a été posée qu'après.
