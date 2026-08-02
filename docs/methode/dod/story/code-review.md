# DoD — incrément, sortie de `Code Review`

> **Draft non éprouvé.** Écrit d'après l'état de l'art, pas récolté sur une exécution réelle —
> `D-039` demande l'inverse. À confronter au premier usage ; en cas de désaccord,
> `docs/methode/journal-frictions.md` prime sur ce fichier.

> **La question** : ce comportement peut-il être tiré en `QA Review` ou en `Done`
> (`tickets.md` §6.2) ?
>
> **Le flux est tiré.** Une DoD n'est pas une condition que l'amont s'applique à lui-même : c'est
> ce que **l'aval vérifie avant de tirer**. Le relecteur qui bute repose la carte et pose
> `Rework Needed`.
>
> On relit un **comportement**, jamais un commit isolé (`tickets.md` §6.2) : le cumul de la branche
> `story/` contre son point fixe (`D-042`).
>
> **L'échelle est celle du module** (`cycle-pas.md` §5) : la découpe en classes, le design, la
> cohérence de l'ensemble — et cette revue seule a le droit de réclamer des **pas supplémentaires**
> pour corriger. La **fonction** — ce que prouve chaque test, sa formulation, le nommage — a déjà
> été relue pas par pas, contre `dod/pas/code-review.md`. Ne pas la rejuger ici : le diff est trop
> large pour qu'on la voie, et elle a eu son relecteur.

## 1. Le comportement est complet

- [ ] Le diff, relu **d'un bloc** contre son point fixe, réalise entièrement ce que la carte
      annonçait — pas une partie, pas une version dégradée
- [ ] `dotnet build` sans warning et `dotnet test` entièrement vert, sur ce diff précisément
- [ ] Aucun test désactivé ou marqué à revoir n'a été laissé pour faire passer la suite

## 2. Le diff se relit d'un bloc

- [ ] Le point fixe est identifié explicitement (base de la story pour une PR `pas/`, base de la
      feature ou de `main` pour une PR `story/`) — pas « la PR » en général
- [ ] Les deux axes de revue (`.claude/skills/revue-code/SKILL.md`) ont statué sur le **même**
      diff, et leurs constats ne sont pas fusionnés

## 3. Les commits sont argumentés

- [ ] Chaque commit qui reste dans l'historique après fusion (le squash `pas/`, le commit
      `story/` en cascade) explique le **pourquoi**, pas seulement le quoi — corps réécrit à la
      main, jamais la concaténation des WIP que GitHub propose par défaut (`flux.md` §6)
- [ ] Une alternative envisagée puis écartée pendant le développement est écrite, pas seulement
      tranchée en silence (`tickets.md` §5)

## 4. `architecture.md` et `decisions.md` sont à jour

- [ ] `docs/design/architecture.md` reflète tout type ajouté, supprimé ou renommé, et toute
      frontière de responsabilité déplacée par ce diff
- [ ] Un `D-NNN` existe si une décision structurante a été prise ou renversée dans ce
      comportement ; renvoyé depuis le commit si utile
- [ ] Si un `D-NNN` a été ajouté, son numéro a été pris **à l'écriture** — la fin de
      `decisions.md` relue juste avant de commiter, pas au moment où la revue a commencé (gotcha
      payé deux fois le même jour, deux sessions parallèles ayant choisi le même numéro)

## 5. Ce que seul l'ensemble révèle

- [ ] Chaque comportement observable dans le diff **d'un bloc** sans test qui le nomme a été ajouté
      à la test list du pas concerné — écrit sur la carte, pas seulement mentionné dans le verdict.
      C'est le trou qu'aucune revue de pas ne peut voir : il naît **entre** deux pas
- [ ] Un manque qui exige du code neuf donne lieu à un **pas supplémentaire** sur cet incrément,
      pas à une remarque qu'on espère voir reprise au passage

## 6. Le verdict est posé, l'escalade est traçable si elle a eu lieu

- [ ] Le verdict de chaque axe cite le référentiel (fichier + clause) et l'extrait visé, jamais
      une impression
- [ ] `Done` ou `Rework Needed` est posé sur la carte — ce skill ne déplace jamais la colonne
- [ ] Si le désaccord a dépassé deux ou trois tours, la carte porte le compteur de tours et est
      **assignée** à un humain, avec le point en litige reconstituable en une minute
      (`tickets.md` §6.4)

## 7. Ce qui n'est *pas* un critère de sortie

- **L'accord des deux axes sur tout.** Un désaccord tranché et écrit vaut mieux qu'un accord de
  façade — c'est le litige, pas son absence, qui doit être traçable.
- **La validation manuelle du parcours.** C'est `QA Review`, conditionnelle
  ([`dod/story/done.md`](done.md)), et elle n'a pas lieu ici.
- **L'acceptation cochée case par case.** C'est le critère de `Done`, pas de `Code Review` — ce
  fichier vérifie que le comportement est complet, pas encore que chaque case de l'incrément est
  pointée.
