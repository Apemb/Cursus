# DoD — feature, entrée en `In Progress`

> **Draft non éprouvé.** Écrit d'après l'état de l'art, pas récolté sur une exécution réelle —
> `D-039` demande l'inverse. À confronter au premier usage ; en cas de désaccord,
> `docs/methode/journal-frictions.md` prime sur ce fichier.

> **La question** : cette feature peut-elle être **tirée** de `Spec` vers `In Progress` ?
>
> Cette DoD n'a pas de « sortie » symétrique aux précédentes : `Spec` en a déjà une
> ([`dod/feature/spec.md`](spec.md)), qui gate le **début** du découpage. Celle-ci gate son
> **résultat** — ce que `tickets.md` §6.1 nomme l'exigence de `In Progress` : *« le découpage a eu
> lieu … et au moins l'un d'eux a démarré »*. Deux moments distincts du même passage, deux DoD.
>
> **Qui vérifie.** Aucun tiers n'est assigné à cette transition (`tickets.md` §6.3 ne la liste dans
> aucun des trois régimes) — régime *Production*, pas *Trio*. C'est donc **celui qui prend le
> premier incrément** qui la vérifie : la feature est déjà en `In Progress`, tirée par l'humain
> après l'accord sur la spec, et ce fichier lui dit ce que le découpage devait lui laisser.
>
> ⚠️ **Ce n'est pas au découpeur de se l'appliquer**, et l'inverse a été écrit ici jusqu'au
> 2026-08-02. Une DoD que l'amont s'applique à lui-même n'est plus une DoD (`cycle.md` §4) : elle
> devient une case qu'on coche en poussant sa propre carte, là où elle existe pour donner à l'aval
> de quoi refuser.

## 1. L'artefact est complet

- [ ] Chaque incrément issu du découpage existe comme carte, rattachée au projet
- [ ] Chaque incrément porte ses **frontières** (`tickets.md` §3 q.6) telles que déposées au
      découpage — non recalculées depuis
- [ ] Chaque incrément porte son `blockedBy`, vide ou non
- [ ] Toute clause de la recette de la spec atterrit dans au moins un incrément

## 2. La clause la plus importante — un incrément a réellement démarré

« Le découpage a eu lieu » ne suffit pas seul : une feature découpée mais dont aucun incrément n'a
été pris reste, au sens de ce dépôt, une feature qui **attend** — pas une feature **en cours**.

- [ ] Au moins un incrément est en `Planning`, `Plan Review` ou `In Progress`
- [ ] Ce n'est pas nécessairement le premier de l'ordre — seulement un incrément dont le
      `blockedBy` était vide au moment où il a été pris

## 3. Le critère opposable

> **Une feature est légitimement `In Progress` quand le découpage est publié et qu'un exécutant a
> déjà pu prendre un incrément sans revenir vers la feature.**

Le test : regarder si un incrément est sorti de `Todo`. Aucun ne l'est encore → le découpage est
peut-être fini, mais la transition n'est pas due — la carte reste en `Spec`, `Done` posé sur sa
propre DoD.

## 4. Ce qui n'est *pas* un critère

- **Tous les incréments démarrés, ou même terminés.** Un seul suffit ; les autres attendent leur
  tour en `Todo`, normalement — y compris ceux qu'un `blockedBy` retient (`D-072`).
- **Un ordre total entre incréments.** Seules les arêtes de blocage (`blockedBy`) sont exigées.
- **Le plan de design de l'incrément démarré.** Il vit dans sa propre DoD (`Planning` /
  `Plan Review`), pas ici.
