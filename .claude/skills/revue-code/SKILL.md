---
name: revue-code
description: Relit contre un point fixe, à deux échelles — la fonction pour un pas, le module pour
  un incrément (`flux.md` #9 et #10). Use quand une carte de pas ou d'incrément porte `Done` et
  attend sa revue, quand on demande une revue de code, une relecture de PR `pas/` ou `story/`, ou
  de raffiner une test list après un cycle TDD.
---

> **Draft non éprouvé.** Écrit d'après l'état de l'art, pas récolté sur une exécution réelle —
> `D-039` demande l'inverse. À confronter au premier usage ; en cas de désaccord,
> `docs/methode/journal-frictions.md` prime sur ce fichier.

## Tirer la carte — c'est toi qui la déplaces

La carte t'attend **portant `Done`**, dans la colonne où son auteur s'est arrêté : un ticket n'est
jamais poussé (`cycle.md` §4). **Tire-la en `Code Review` et retire l'étiquette** — un seul geste, à
la prise. C'est ton **unique** déplacement de carte : le verdict rendu, elle reste où elle est, et
c'est l'aval qui la tirera plus loin. Pas même sur escalade, qui **assigne** sans bouger.

## Le bloc, et à quelle échelle

**Deux échelles, deux référentiels, et la grande ne rattrape pas la fine** :

| Niveau | Ce qui se relit | Référentiel |
|---|---|---|
| **Pas** | La **fonction** : ce que prouve chaque test, sa formulation, le nommage, la forme du code | [`dod/pas/code-review.md`](../../../docs/methode/dod/pas/code-review.md) |
| **Incrément** | Le **module** : la découpe en classes, le design, la cohérence de l'ensemble — et **lui seul** peut réclamer des pas supplémentaires | [`dod/story/code-review.md`](../../../docs/methode/dod/story/code-review.md) |

⚠️ **Ne rejuge pas la fonction à l'échelle du module** : le diff de plusieurs pas est trop large
pour qu'on y voie un nom de variable, et chaque pas a déjà eu son relecteur. Inversement, ne juge
pas le design sur un pas : le diff est trop étroit pour le fonder.

L'objet est le **cumul** d'une branche `pas/` ou `story/` contre son point fixe
(`git diff <base>...HEAD`) : la base de la story pour une PR `pas/`, la base de la feature ou de
`main` pour une PR `story/`. Jamais « la PR » en général, toujours ce diff-là contre ce point-là.
Identifie le point fixe avant d'ouvrir quoi que ce soit.

## Exécution

Invoque le skill `revue` sur ce diff. Ses garanties tiennent lieu de contrat ici : deux axes
minimum en sous-agents parallèles jamais fusionnés, citation obligatoire du référentiel et de
l'extrait, abstention si le référentiel manque, distinction violation dure / jugement, verdict
structuré, le relecteur liste et ne réécrit jamais, `Done`/`Rework Needed` posé sans jamais
déplacer la carte.

**Nos deux axes** :

- **Standards** — sans le ticket ni la DoD, jamais. Référentiel : `CLAUDE.md` (0 warning,
  `étant donné/quand/alors`, section `// arrange` `// act` `// assert`, pas de nullable pour
  distinguer des types, commentaires qui expliquent le pourquoi), `docs/design/architecture.md`,
  la frontière testé/non-testé (`tickets.md` §7.12 — un test manquant en présentation n'est pas
  une violation). Les douze code smells de Fowler restent un vocabulaire de secours à citer par
  leur nom quand un smell touche le diff sans être couvert par ces documents écrits — pas une
  annexe à charger : l'agent les connaît déjà, et nos standards écrits et opposables couvrent
  l'essentiel que Fowler laisse générique.
- **Conformité** — reçoit le ticket de l'incrément et
  [`dod/story/code-review.md`](../../../docs/methode/dod/story/code-review.md). Juge si
  le diff fait ce que la carte demandait, rien de plus, rien de moins.

**Priver Standards du ticket, en pratique** : lance-le en sous-agent de **session neuve**, dont le
prompt contient uniquement le diff, `CLAUDE.md` et `architecture.md` — jamais l'identifiant de
carte, jamais son texte, jamais le fil qui a produit le diff. C'est la condition mesurée qui fait
gagner une relecture (`docs/reference/skills.md` §5.2, *le contexte séparé bat la répétition*) :
donner l'intention au relecteur l'ancre sur elle.

## Raffiner la test list et la formulation des comportements

Le vert d'un pas prouve que le test passe, pas qu'il prouve la bonne chose ni qu'il est bien
nommé (`tickets.md` §4). C'est ici que ça se corrige — une production à part entière, pas un
sous-produit du verdict :

- pour chaque test dont le titre ne suit pas `étant donné/quand/alors` ou décrit mal ce qu'il
  prouve, propose la reformulation ;
- pour chaque comportement observé dans le diff sans test qui le nomme, ajoute le cas à la test
  list du pas concerné.

**Critère d'achèvement** : chaque écart de formulation relevé porte sa correction proposée, et
chaque cas ajouté est écrit dans la carte du pas — pas seulement mentionné dans le verdict.

## Documentation à jour

Avant de poser une étiquette, vérifie que `docs/design/architecture.md` a suivi le diff — type
structurant ajouté ou renommé, frontière de responsabilité déplacée — et qu'un `D-NNN` existe si
une décision structurante a été prise ou renversée dans ce comportement.

**Gotcha payé deux fois le même jour** : `decisions.md` est append-only, et deux sessions
parallèles qui lisent sa fin avant que l'autre n'écrive choisissent le même numéro. Le numéro se
prend **à l'écriture**, jamais à la lecture — relis la fin du fichier juste avant de commiter le
`D-NNN`, pas au moment où tu as commencé la revue.

## L'escalade

Après deux ou trois tours sans accord entre les deux axes, escalade : **assigne la carte** à un
humain — non assignée, elle continue de boucler ; assignée, elle attend un arbitre. Porte le
compteur de tours sur la carte, garde à chaque tour un verdict structuré (accord/désaccord et le
point en litige, jamais de prose libre) pour qu'un tiers reconstitue le litige en une minute.

## À l'accord d'un pas — fusionner, et rien d'autre

⚠️ **Cette section ne vaut qu'à l'échelle du pas** (`D-076`). Un pas que tu accordes n'a **aucun
aval** : il n'existe pas d'étape après sa revue, et `cycle-pas.md` §5 dit que la fusion *est* ce qui
le tire vers `Done`. Nommer un tiers pour ce seul geste aurait créé un acteur sans travail propre.
Une fois `Done` posé sur le pas :

1. **Fusionne** `pas/` dans `story/`, en **squash**, corps réécrit à la main — GitHub y colle par
   défaut la concaténation des WIP, et c'est ce commit-là qui reste dans l'histoire. Le corps dit le
   comportement ajouté et les alternatives écartées, pas la liste des commits.
2. **Si c'était le dernier pas** de l'incrément : pose `Done` sur l'incrément et **ouvre la PR de la
   story**. C'est le seul instant où la branche de story contient réellement tout l'incrément — le
   poser plus tôt ferait relire un diff incomplet à la revue de module.

⚠️ **Le prédicat du « dernier pas » se lit, il ne se devine pas.** Ce n'est **pas** le pas au plus
grand numéro ni celui que le découpage avait mis en fin de chaîne : c'est *« tous les frères de cet
incrément sont dans leur colonne terminale »*, évalué **au moment où tu fusionnes**. Le découpage
n'est pas figé — la revue d'un incrément peut réclamer des pas supplémentaires (`cycle-pas.md` §5),
et un pas né en cours de route déplace la fin sans prévenir personne. Interroge l'état, jamais
l'ordre prévu.

⚠️ **À l'échelle de l'incrément, tu ne fusionnes rien.** Une story a des étapes **après** ta revue —
`QA Review`, puis sa colonne terminale : fusionner à ton accord y ferait entrer un incrément que la
recette n'a pas vu. Qui fusionne `story/` n'a pas encore de porteur, et c'est écrit (`D-076` §4).

## Critère d'achèvement

La carte porte `Done` ou `Rework Needed` avec le point de désaccord écrit — et elle **n'a pas
bougé** depuis que tu l'as tirée. Ce que `Done` autorise ensuite, c'est un **autre** qui l'exerce —
sauf au pas, où la fusion ci-dessus t'appartient. À l'incrément, c'est celui qui recette, vers
`QA Review` ou vers `Done` selon `tickets.md` §6.2.
Avant de poser l'étiquette : la [DoD `code-review`](../../../docs/methode/dod/story/code-review.md)
est cochée case par case, pas résumée.
