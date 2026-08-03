# DoD — incrément, sortie de `Todo`

> **Draft non éprouvé.** Écrit d'après l'état de l'art, pas récolté sur une exécution réelle —
> `D-039` demande l'inverse. À confronter au premier usage ; en cas de désaccord,
> `docs/methode/journal-frictions.md` prime sur ce fichier.

> **La question** : cet incrément peut-il être **tiré** en `Planning` ? Il y passe toujours — ce
> qui se saute quand aucun plan n'est requis, c'est le **plan**, jamais la colonne : la carte y
> reçoit la phrase qui le dit, plus `Done`, et `decoupage-pas` l'en tire (`cycle-increment.md` §4).
>
> **Le flux est tiré.** Une DoD n'est pas une condition de sortie que l'amont s'applique à
> lui-même : c'est **ce que l'aval vérifie avant de tirer**. Ici, l'aval est celui qui prend
> l'incrément — humain ou agent — et qui **n'a pas eu la conversation du découpage**.
>
> Le *contenu* attendu d'un incrément est en `tickets.md` §3 (les six questions). Ici : uniquement
> de quoi il faut s'être acquitté pour que `Todo` tienne sa promesse — **la colonne d'éligibilité**
> (`tickets.md` §6.2).
>
> **C'est la DoD la plus consultée de ce dossier.** Elle est le référentiel pressenti du prédicat
> de déclenchement machine (`CUR-5`) : le jour où un exécutant automatique décide seul si une carte
> est prenable, c'est ce fichier qu'il applique. Ce qui n'est pas testable ici ne le sera pas
> davantage par une machine.

## 1. Aucun `blockedBy` ouvert

- [ ] Chaque incrément listé en `blockedBy` est `Done`

Mécanique, binaire — rien à juger.

## 2. Le contexte tient dans la carte — la clause la plus importante

> **Une carte est prête quand une session neuve — qui n'a pas eu la conversation du découpage —
> répond aux six questions de `tickets.md` §3 en lisant uniquement la carte et ce qu'elle atteint
> par navigation (le projet parent, les incréments frères, les renvois `D-NNN` cités), sans
> reposer aucune question à l'humain.**

Le test se joue, il ne se lit pas : ouvrir la carte à froid et répondre aux six questions. La
première qui bloque dit où le manque se situe.

- [ ] **Q1 — le comportement** est formulé à l'indicatif, observable de l'extérieur (« le jeton vit
      dans le trousseau »), pas comme une tâche (« gérer les secrets »)
- [ ] **Q2 — pourquoi maintenant** répond sans renvoyer à une conversation non écrite
- [ ] **Q3 — ce qui est déjà décidé** porte ses renvois (`D-NNN`, `architecture.md §X`) — un
      renvoi, jamais une paraphrase qui peut diverger de sa source
- [ ] **Q4 — les pièges connus** sont rappelés s'il y en a ; l'absence est légitime, le silence sur
      un piège déjà payé ne l'est pas
- [ ] **Q5 — l'acceptation** est observable et cochable case par case ; « ça marche » est refusé
- [ ] **Q6 — le hors-périmètre** nomme les incréments frères, pas une abstraction

Deux règles de durabilité, transverses aux six questions (`AGENT-BRIEF`, transposé) :

- [ ] **Aucun chemin de fichier, aucun numéro de ligne.** Un renvoi au code se fait par nom de type
      ou de module — la structure aura bougé avant que la carte soit prise
- [ ] **Comportemental, jamais procédural.** La carte dit ce que le système doit faire, jamais
      quelle méthode ouvrir ni quelle ligne éditer — ça, c'est le plan de design, écrit à la prise

## 3. Le critère opposable

> **Un incrément est prêt pour `Todo` quand il peut être pris par une session neuve sans qu'elle
> revienne vers la feature ou vers l'auteur du découpage.**

Il se teste en le tentant : prendre l'incrément à froid, et voir où ça bute.

## 4. Ce qui n'est *pas* un critère

- **Avoir un plan de design.** Il s'écrit à la prise, en `Planning` — pas avant (`tickets.md` §3).
- **Avoir une test list.** Elle s'écrit au pas, jamais en amont.
- **Une acceptation exhaustive.** L'acceptation est la part de la recette de la feature qui revient
  à *cet* incrément, pas une couverture de tous les cas imaginables.
- **Répéter le standard du dépôt.** `0` warning, régime TDD, frontière testé/non testé,
  conventions de modélisation : ils vivent dans `CLAUDE.md`, chargé automatiquement. Les répéter
  par carte coûte sans rien gagner (`tickets.md` §7).
