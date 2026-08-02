# DoD — pas, sortie de `Code Review`

> **Draft non éprouvé.** Écrit d'après l'état de l'art, pas récolté sur une exécution réelle —
> `D-039` demande l'inverse. À confronter au premier usage ; en cas de désaccord,
> `docs/methode/journal-frictions.md` prime sur ce fichier.

> **La question** : ce pas peut-il être fusionné dans le `story/` de son incrément — donc basculer
> en `Done` ?
>
> **Le flux est tiré.** Une DoD n'est pas une condition que l'amont s'applique à lui-même : c'est
> ce que **l'aval vérifie avant de tirer**. Le relecteur qui bute repose la carte et pose
> `Rework Needed`.
>
> **L'échelle est celle de la fonction** (`cycle-pas.md` §5) : ce que prouve un test, comment il se
> nomme, comment le code se lit. Le **module** — la découpe en classes, le design, la cohérence de
> l'ensemble — se relit en `Code Review` de l'**incrément**, contre `dod/story/code-review.md`. Une
> case qui semble manquer ici est peut-être une case de l'autre échelle.
>
> ⚠️ **La grande échelle ne rattrape pas la fine.** Un relecteur d'incrément parcourt un diff de
> plusieurs pas et ne relit pas chaque nom de variable. Compter sur lui pour le faire, c'est
> n'avoir personne — c'est tout le motif de cette colonne au niveau du pas.

## 1. Les tests prouvent ce qu'ils annoncent

- [ ] Chaque test de la test list a été vu **rouge pour la bonne raison** avant d'être vert —
      l'assertion a échoué, pas le chargement du module (`dod/pas/done.md` §2)
- [ ] Aucun test ne passe pour une raison étrangère à ce qu'il prétend prouver (assertion trop
      large, cas déjà vrai avant le changement)
- [ ] La test list reflète l'état final : tout cas découvert en cours de cycle y a été ajouté

## 2. La formulation se tient

- [ ] Chaque titre suit `étant donné <état>, quand <action>, alors <conséquence observable>`
      (`CLAUDE.md` §Conventions) — un titre qui décrit mal ce qu'il prouve porte sa reformulation
- [ ] Le corps de chaque test porte ses sections `// arrange`, `// act`, `// assert`
- [ ] Les noms — types, méthodes, variables — sont en anglais ; commentaires et messages de test en
      français, diacritiques complets

## 3. Le code se lit

- [ ] Les commentaires expliquent **pourquoi**, jamais **quoi** ; aucun ne paraphrase la ligne
      suivante
- [ ] Aucune variante d'objet encodée par des nullables mutuellement exclusifs là où le type
      devrait la porter (`CLAUDE.md` §Conventions)
- [ ] Le refactor a eu lieu au vert, sur le code de test comme sur le code testé

## 4. Le verdict est posé

- [ ] Chaque remarque cite son référentiel (fichier + clause) et l'extrait visé, jamais une
      impression
- [ ] `Done` ou `Rework Needed` est posé sur la carte du pas — le relecteur ne déplace jamais la
      colonne (`cycle.md` §4)

## 5. Ce qui n'est *pas* un critère ici

- **Le design, la découpe en classes, la cohérence du module.** C'est l'échelle de l'incrément, et
  la relire ici reviendrait à juger sur un diff trop étroit pour la fonder.
- **Réclamer un pas supplémentaire.** Seule la revue de l'incrément le fait : elle seule voit ce
  qui manque à l'ensemble.
- **L'acceptation de l'incrément.** Un pas n'en a pas — la suite verte et les zéro warning sont
  l'acceptation de tous les pas (`tickets.md` §4).
