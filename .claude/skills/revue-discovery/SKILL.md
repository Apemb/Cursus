---
name: revue-discovery
description: Valider la conformité d'une discovery de feature contre `docs/methode/dod/feature/discovery.md`, avant que la spec ne la tire (`cycle-feature.md` §3). Utiliser à la prise d'une carte de feature en colonne `Discovery` portant `Review Requested`, ou quand on demande de relire, valider ou faire la revue d'une discovery.
---

> **Draft non éprouvé.** Écrit d'après l'état de l'art, pas récolté sur une exécution réelle —
> `D-039` demande l'inverse. À confronter au premier usage ; en cas de désaccord,
> `docs/methode/journal-frictions.md` prime sur ce fichier.

Une **instance** du skill `revue` — l'invoquer (« Suis le protocole du skill `revue` ») avec le
mandat ci-dessous. Ce skill ne prononce que sur la **conformité** ; il ne juge jamais si le besoin
vaut d'être traité, ni quelle piste vaut mieux qu'une autre.

**Ce qui distingue cette revue des deux autres**, et c'est tout son intérêt : les autres cherchent
ce qui **manque**, celle-ci cherche aussi ce qui est **en trop**. Une discovery qui arbitre a mangé
l'étape suivante, et le tort ne se voit pas — il ressemble à de la rigueur. Un relecteur qui
n'aurait pas lu §3 réclamerait spontanément ce que la DoD interdit d'exiger.

## 1. Exiger un contexte neuf

Refuser de continuer si cette session a participé à l'écriture de la discovery, ou a vu le prompt
ou la conversation qui l'a produite. Un agent qui a co-écrit ne valide pas — sollicité quand même,
il rendra un verdict, et un faux accord est pire qu'aucune relecture (`cycle-feature.md` §4, *le
piège du binôme*). En `Discovery` la clause mord plus fort qu'ailleurs : l'auteur est un **binôme
humain ⇄ agent** (`cycle.md` §5), donc l'agent qui a tenu la plume a aussi entendu les réponses
de l'humain — et jugera « le besoin est établi » sur ce qu'il a entendu, pas sur ce qui est écrit.

Complet quand : soit la session ne porte que le document `Discovery`, sans le fil qui l'a produit,
soit elle s'arrête ici et le dit.

## 2. Passer le mandat à `revue`

Fournir l'artefact — le document `Discovery` attaché à la feature — et exactement trois axes.

**Axe Complétude** (référentiel : `docs/methode/dod/feature/discovery.md` §1, les cinq cases). Une
case sans réponse **et** sans « sans objet » explicite est une **violation dure**. Attention à la
cinquième, *ce que la discovery a fait apparaître* : elle n'est due que **s'il y a lieu**, et son
absence n'est une violation que si le document laisse voir un cadrage défait ou deux besoins
séparés en route sans que rien ne l'accueille.

**Axe Non-arbitrage** (référentiel : `docs/methode/dod/feature/discovery.md` §2). L'axe le plus
subtil des treize DoD, et *« le premier candidat à produire un faux succès »* (`cycle-feature.md`
§8). Il ne se coche pas, il se **teste piste par piste** :

> Cette piste est-elle présentée avec une raison de ne pas la retenir ?

La frontière est fine et se franchit sans qu'on s'en aperçoive. *Énoncer un fait connu* sur une
piste est légitime — « ce transport suppose le service résident » ; *en tirer une conséquence* ne
l'est pas — « donc cette piste ne convient pas ». Le second est déjà de l'arbitrage, même déguisé
en constat, et c'est sous cette forme-là qu'il passe. Deux formats le fabriquent presque toujours,
et leur seule présence vaut d'être relevée : un **tableau de pistes à colonne de commentaire**, et
toute **estimation de coût ou de faisabilité**.

**Axe Adresse au lecteur** (référentiel : `docs/methode/dod/feature/discovery.md` §5, les trois
cases). Une discovery se lit **dans le tracker**, par quelqu'un qui n'a pas le dépôt sous la main :
chemins de fichiers au lieu de liens, numérotation interne périssable, méta-commentaire de méthode
qui explique le gabarit au lieu de traiter le sujet. Cet axe est séparé des deux autres parce qu'il
juge la **forme**, et qu'un défaut de forme rangé à côté d'un défaut de fond emprunte son poids.

⚠️ **`cycle-feature.md` §3 annonce deux axes, pas trois** — il a été écrit avant que §5 de la DoD
n'existe sous forme de cases opposables. Le troisième axe est délibéré : le fondre dans la
complétude reviendrait à mélanger deux natures de jugement, ce que `revue` §2 interdit.

Complet quand : les trois rapports sont reçus séparément, sans mélange entre eux.

## 3. Refuser les exigences que la DoD interdit

Le mode de défaillance propre à cette revue est le **relecteur zélé** : il réclame de la discovery
ce qui appartient à la spec, et son reproche a toutes les apparences du sérieux. Le §4 de
`docs/methode/dod/feature/discovery.md` les nomme — aucun des quatre n'est une divergence, et en
consigner un est une faute du relecteur, pas de l'artefact :

- **« aucune piste n'est choisie »** — c'est la définition même de l'étape ;
- **« la liste des pistes n'est pas exhaustive »** — exiger l'inventaire referme l'ouverture, et la
  spec peut toujours en ajouter une ;
- **« rien n'est chiffré »** — le coût est `tickets.md` §2.2 q.1 ;
- **« les sorties possibles ne sont pas listées »** — elles vivent dans la DoD ; un document parle
  de son sujet, jamais de son propre processus.

Complet quand : aucune ligne des trois axes ne réclame l'une de ces quatre choses.

## 4. Écarter ce qui relève de la justesse

Si un constat porte sur **si ce besoin mérite qu'on s'y arrête** — l'outillage ne vaut pas son
coût, le vrai besoin est ailleurs, la feature devrait être annulée — ce n'est aucun des trois axes.
C'est une question de **justesse**, sans référentiel dans la DoD ; elle revient à l'humain, qui la
prononce en tirant la carte vers `Spec` ou vers `Canceled`. La noter à part, sous une ligne
**« hors mandat — justesse »**, jamais glissée dans l'un des axes.

C'est ici que la sortie `Canceled` se joue, et elle est **de plein droit** : la `Discovery` est
l'unique sortie bon marché du flux. Un relecteur qui la pressent le signale sans la prononcer.

Complet quand : aucune ligne des trois axes ne discute si la feature vaut le coup.

## 5. Poser les remarques sur la carte, par la ligne de commande

Une remarque déposée sur le **document** est invisible : l'ancre de Linear est une marque dans
l'état de l'éditeur, qu'aucune API n'écrit (`D-045`, `docs/reference/linear-api.md` §10d). Le geste
correct vise la **carte** — ici le projet, puisqu'une feature *est* un projet :

```bash
cursus linear comment add <référence-du-document> --quote "<le passage visé>" --body "<la remarque>"
```

La citation est vérifiée contre le document : introuvable ou ambiguë, la commande refuse — et c'est
le passage **du document** qui part, jamais la frappe de l'appelant. Le repère de section est
calculé, donc ni omissible ni falsifiable par le relecteur.

La porte de sortie du cycle se lit de la même façon, et elle est mécanique :

```bash
cursus linear comment list <référence-du-document> --unresolved
```

dont le champ `open` ne compte que les remarques **racines** — une réponse qui solde reste non
résolue, et la compter empêcherait la porte de se refermer (`D-046`).

⚠️ **Ne jamais réécrire le document.** Cette revue ne réécrit rien (`revue` §6), et une écriture
par l'API qui traverse une marque désancre les commentaires qu'elle croise (`linear-api.md`
§10g–§10h) — un relecteur qui corrigerait effacerait les remarques qu'il corrige.

Complet quand : aucune remarque n'a été posée sur le document, et le décompte des ouvertes vient de
la commande plutôt que d'un comptage à la main.

## 6. Poser le verdict, et savoir qui il convoque

Poser `Done` si aucune violation dure ne reste, sinon `Rework Needed`, avec le point en litige de
chaque axe qui motive le refus. Ne jamais déplacer la carte (`cycle.md` §4).

⚠️ **`Rework Needed` en `Discovery` n'attend pas une machine : elle attend une personne**
(`cycle.md` §5). C'est le cycle **court** — ni correcteur ni vérificateur agent, parce que ce qui
manque à une remarque de discovery n'est presque jamais de la prose mais de la **matière** : un
entretien qui n'a pas eu lieu, une piste qu'on n'a pas explorée. Rédiger une remarque en supposant
qu'un agent la corrigera seul produit une reprise plus lisse sur un besoin toujours aussi mal
établi. **Écrire chaque remarque pour la personne qui devra aller chercher ce qui manque**, et
nommer ce qui manque plutôt que le passage à récrire.

Corollaire : le binôme solde ses propres remarques, ce qui serait complaisant dans un cycle
complet. Ce qui le rend tenable, c'est qu'**un tour de revue suit toujours** — donc relire le fil
entier, et pas seulement le dernier état du document (`cycle-feature.md` §3).

Complet quand : une étiquette est posée, jamais les deux, et chaque remarque nomme ce qui manque
plutôt que de proposer un texte de remplacement.
