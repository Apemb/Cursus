# DoD — feature, `Completed`

> **Draft non éprouvé.** Écrit d'après l'état de l'art, pas récolté sur une exécution réelle —
> `D-039` demande l'inverse. À confronter au premier usage ; en cas de désaccord,
> `docs/methode/journal-frictions.md` prime sur ce fichier.

> **La question** : cette feature peut-elle être tirée en `Completed` ?
>
> Distinct de `Validation` (`dod/feature/validation.md`) : recetter la capacité contre la
> spec est un jugement de fond, fait une fois ; **fermer** la feature est une discipline
> d'écriture, à ne pas confondre avec la première — une feature peut être recettée avec
> succès et rester ouverte parce que personne n'a mis à jour le dépôt. Le contenu attendu est
> `tickets.md` §6.1, ligne `Completed`. Ici : uniquement de quoi il faut s'être acquitté.

## 1. Les trois exigences

- [ ] **Tous les incréments de la feature portent `Done`.** Vérifié carte par carte, pas par
      un compte « X / X fermés » qui masquerait un incrément resté dans un autre statut.
- [ ] **`trajectoire.md` acte la jambe.** La jambe que cette feature fait avancer est mise à
      jour dans `docs/design/trajectoire.md` — close si elle l'est, précisée si la feature
      n'en couvre qu'une partie. Une jambe qui reste « en cours » dans `trajectoire.md` alors
      que sa feature est `Completed` dans le tracker est une désynchronisation, pas un
      détail.
- [ ] **Un `D-NNN` est écrit si un arbitrage structurant a été tranché** pendant la feature —
      pas systématiquement. Le seuil est celui déjà retenu par ce dépôt
      (`docs/reference/mattpocock-skills.md` §4.1) : **irréversible et surprenant et
      arbitrage réel**. Si l'un des trois manque, ne rien écrire est le bon choix — *« That's
      it. An ADR can be a single paragraph »* vaut aussi pour la décision de ne pas en écrire
      un.

## 2. Le gotcha du numéro — à ne jamais rater

C'est la clause qui a déjà coûté cher, deux fois le même jour : deux sessions qui travaillent
en parallèle et lisent chacune la fin de `decisions.md` **avant** que l'autre n'ait écrit
choisissent le **même** numéro `D-NNN`. Le fichier est **append-only** : une fois le doublon
commité, on ne le corrige pas en réécrivant, on l'assume dans l'historique.

- [ ] **Le numéro se prend à l'écriture, pas à la lecture.** Relire la fin de
      `decisions.md` **juste avant de commiter** l'entrée — pas au moment où on a commencé à
      la rédiger. L'écart entre les deux instants est exactement la fenêtre où une autre
      session peut avoir écrit entre-temps.

## 3. Le critère opposable

> **Une feature est `Completed` quand quelqu'un qui n'a jamais ouvert Linear peut, depuis le
> seul dépôt, dire quelle jambe a avancé et pourquoi — sans qu'aucune information ne soit
> restée uniquement dans le tracker.**

Il se teste en fermant l'onglet du tracker et en lisant `trajectoire.md`, puis
`decisions.md` si un arbitrage était attendu.

## 4. Ce qui n'est *pas* un critère

- **Recetter à nouveau contre la spec.** Déjà fait en `Validation` ; le répéter ici confond
  les deux gates.
- **Écrire un `D-NNN` par défaut.** Le seuil (§1) exclut l'arbitrage mineur ou attendu ; une
  feature qui n'a rien tranché de surprenant n'en doit aucun.
- **Recopier l'historique des commits dans `trajectoire.md`.** Ce document dit *où en est la
  jambe*, pas *ce qui s'est passé commit par commit* — l'historique git s'en charge déjà
  (`CLAUDE.md`).
