# DoD — pas, sortie de `In Progress`

> **Draft non éprouvé.** Écrit d'après l'état de l'art, pas récolté sur une exécution réelle —
> `D-039` demande l'inverse. À confronter au premier usage ; en cas de désaccord,
> `docs/methode/journal-frictions.md` prime sur ce fichier.

> **La question** : la branche de ce pas peut-elle être **fusionnée** dans le `story/` de son
> incrément ? C'est cette fusion qui le fait basculer en `Done` — la colonne étant terminale, rien
> ne l'y tire (`cycle.md` §4).
>
> **Ce fichier ne porte que le mécanique.** Le verdict sur la **forme** — les tests prouvent-ils ce
> qu'ils annoncent, les noms tiennent-ils — a déjà eu lieu en `Code Review`, à l'échelle du pas
> (`cycle-pas.md` §5). Ici on ne vérifie plus que ce qu'une machine peut constater.

## 1. Le critère mécanique (`tickets.md` §6.2)

- [ ] Commit fait — arrivé dans `story/`, en squash, corps réécrit à la main
- [ ] `dotnet test` entièrement verte
- [ ] `dotnet build` zéro warning

Trois clauses suffisent ; ne pas en ajouter pour paraître sérieux (`mattpocock-skills.md` §4.1 —
*"That's it. An ADR can be a single paragraph."*).

## 2. Ce que le vert seul ne prouve pas

- [ ] Chaque test de la test list a été vu **rouge pour la bonne raison** avant d'être vert — un
      test jamais rouge ne prouve rien, même vert aujourd'hui
- [ ] La test list reflète l'état final : tout cas découvert en cours de cycle y a été ajouté,
      aucun laissé en suspens sans une ligne qui le dit

## 3. Le critère opposable

> Le vert prouve que le test passe ; il ne prouve ni le bon comportement ni la bonne formulation
> (`tickets.md` §4). `Done` ne certifie donc que la mécanique — le raffinement de la test list et
> des noms de test s'est joué en `Code Review` du pas, et la cohérence de l'ensemble se jouera en
> `Code Review` de l'incrément.

## 4. Ce qui n'est *pas* un critère ici

- **La relecture du diff comme un bloc** — la cohérence du module, la découpe en classes et le
  design se relisent en `Code Review` au niveau de l'**incrément**, jamais sur un pas isolé, qui
  est par construction plus petit qu'un comportement observable.
- **Le verdict de forme sur ce pas** — il a déjà été rendu, en `Code Review` du pas ; y revenir ici
  ferait juger deux fois la même chose, sans référentiel neuf pour trancher autrement.
