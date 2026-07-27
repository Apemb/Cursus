---
name: revue-code
description: Relit un comportement complet — un pas ou un incrément — contre un point fixe,
  pour l'étape Code Review du flux (`flux.md` #8). Use quand une carte entre en `Code Review`,
  quand on demande une revue de code, une relecture de PR `pas/` ou `story/`, ou de raffiner
  une test list après un cycle TDD.
---

> **Draft non éprouvé.** Écrit d'après l'état de l'art, pas récolté sur une exécution réelle —
> `D-039` demande l'inverse. À confronter au premier usage ; en cas de désaccord,
> `docs/methode/journal-frictions.md` prime sur ce fichier.

## Le bloc

On relit un **comportement**, jamais un commit isolé — c'est pour ça que `Code Review` n'existe
pas au niveau du pas (`tickets.md` §6.2). L'objet est le **cumul** d'une branche `pas/` ou
`story/` contre son point fixe (`git diff <base>...HEAD`) : la base de la story pour une PR
`pas/`, la base de la feature ou de `main` pour une PR `story/`. Jamais « la PR » en général,
toujours ce diff-là contre ce point-là. Identifie le point fixe avant d'ouvrir quoi que ce soit.

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

## Critère d'achèvement

La carte porte `Done` (le diff tire vers `QA Review` ou `Done` selon `tickets.md` §6.2) ou
`Rework Needed` avec le point de désaccord écrit — jamais un déplacement de colonne par ce skill.
Avant de poser l'étiquette : la [DoD `code-review`](../../../docs/methode/dod/story/code-review.md)
est cochée case par case, pas résumée.
