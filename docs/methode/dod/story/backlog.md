# DoD — incrément, sortie de `Backlog`

> **Draft non éprouvé.** Écrit d'après l'état de l'art, pas récolté sur une exécution réelle —
> `D-039` demande l'inverse. À confronter au premier usage ; en cas de désaccord,
> `docs/methode/journal-frictions.md` prime sur ce fichier.

> **La question** : que faut-il, pour chacune des deux populations de `Backlog`, avant qu'une
> carte puisse un jour être tirée en `Todo` ?
>
> Au niveau incrément, `Backlog` est une **salle d'attente à deux populations**, pas un point de
> départ (`tickets.md` §6.2) : ce qui est né du découpage mais **pas encore éligible** (un
> `blockedBy` ouvert), et ce qui **n'a pas de parent** — l'entrée latérale du backlog. Elles ne
> sont **pas symétriques**, et cette DoD ne les traite pas comme si elles l'étaient.

## 1. Population A — née du découpage, un `blockedBy` ouvert

- [ ] La carte satisfait déjà tout ce que [`dod/story/todo.md`](todo.md) §1 et §2 exigent — le
      découpage l'a écrite avec le même soin que ses frères, seul son tour n'est pas venu
- [ ] Le `blockedBy` référence explicitement le ou les incréments qui la retiennent

**Test** : si cette carte échoue à `todo.md` §2 pour une raison **autre** que son `blockedBy`, ce
n'est pas de la population A — le découpage a un trou, à combler avant de la laisser en `Backlog`.
Dès que son `blockedBy` se vide, elle est éligible sans repasser par une revue de contenu.

## 2. Population B — l'entrée latérale, sans parent

Le refacto orphelin, la dette autonome, ou un incrément déporté d'un découpage antérieur. Rien ne
les fait naître d'une spec — donc rien ne leur fournit de recette de niveau feature
(`tickets.md` §5, question ouverte).

- [ ] Le motif d'existence est **nommé** — refacto, dette, ou déport — pas laissé à deviner
- [ ] Le contexte tient dans la carte au même sens que [`todo.md`](todo.md) §2 — à ceci près que
      Q3 (« ce qui est déjà décidé ») n'a le plus souvent rien à citer : il n'y a pas de spec en
      amont, et l'absence de renvoi est ici légitime, pas un manque
- [ ] **La recette manquante est déclarée, pas cachée.** Sans spec, personne d'autre ne dira
      comment cette carte sera jugée faite — l'acceptation (Q5) doit donc être écrite en entier
      sur la carte elle-même, sans renvoi possible
- [ ] Le hors-périmètre (Q6) nomme un frère **si elle en a un** ; sans frère, elle le dit plutôt
      que de laisser la question vide

## 3. Le critère opposable

> **Une carte quitte `Backlog` quand elle satisfait `todo.md` — la seule différence entre les deux
> populations est ce qui l'a fait entrer, jamais ce qui la fait sortir.**

## 4. Ce qui n'est *pas* un critère

- **Un ordre entre les deux populations.** Elles coexistent ; aucune ne passe avant l'autre par
  défaut.
- **Une spec pour la population B.** C'est précisément le trou que cette DoD **documente**, sans
  inventer une exigence qu'aucune autre partie du dépôt ne pose.
