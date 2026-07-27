# DoD — pas, sortie de `In Progress`

> **Draft non éprouvé.** Écrit d'après l'état de l'art, pas récolté sur une exécution réelle —
> `D-039` demande l'inverse. À confronter au premier usage ; en cas de désaccord,
> `docs/methode/journal-frictions.md` prime sur ce fichier.

> **La question** : ce pas peut-il être **tiré** en `Done` ?
>
> Aucun tiers ne juge ici (`tickets.md` §6.3 : la ligne « pas » ne porte personne « autour de la
> table ») — le critère est **mécanique**, pas un verdict.

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
> des noms de test est le travail de la `Code Review`, au niveau de l'incrément.

## 4. Ce qui n'est *pas* un critère ici

- **La relecture du diff comme un bloc** — c'est `Code Review`, au niveau de l'incrément, jamais
  du pas isolé.
- **Une étiquette `Done` / `Rework Needed` sur la carte** — le pas n'a pas de tiers qui juge.
