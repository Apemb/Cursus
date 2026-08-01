> **Draft non éprouvé.** Écrit d'après l'état de l'art, pas récolté sur une exécution réelle —
> `D-039` demande l'inverse. À confronter au premier usage ; en cas de désaccord,
> `docs/methode/journal-frictions.md` prime sur ce fichier.

# Concevoir deux fois

Annexe de `plan-design`, appelée uniquement quand son étape 5 le juge nécessaire — une découpe où
plusieurs formes radicalement différentes sont défendables, pas seulement leur ordre.

D'après Ousterhout : la première idée n'est presque jamais la meilleure. Le remède n'est pas d'y
réfléchir plus longtemps seul, c'est de forcer des **contraintes différentes** à produire des
**interfaces différentes**, puis de comparer.

## Le dispositif

Lance **trois ou quatre sous-agents en parallèle**, chacun sur le même candidat (le ou les objets
que l'étape 4 hésite à découper), chacun sous **une seule** contrainte :

1. **Minimiser la surface** — le plus petit nombre de méthodes et propriétés publiques possible.
2. **Maximiser la flexibilité** — anticiper le prochain `StepKind`, la prochaine variante, sans
   sur-construire ce que rien ne réclame encore.
3. **Optimiser pour l'appelant d'aujourd'hui** — la forme la plus directe pour ce qui consomme
   l'objet dans *cet* incrément, quitte à généraliser plus tard.
4. **Ports & adaptateurs stricts** — le patron déjà en place dans le dépôt
   (`docs/design/architecture.md` §7.10.5, `docs/design/schemas.md` §2.1) : une interface définie
   par le domaine, une réalisation qui ne la connaît que par sa forme.

Chaque sous-agent produit un schéma-delta candidat complet, pas un paragraphe d'intention.

## Comparer, sur trois axes fixés

Ne compare pas les propositions au hasard — sur ces trois axes, empruntés au vocabulaire de
`schemas.md` :

- **Profondeur** — combien la façade cache-t-elle, combien fuit vers l'appelant ?
- **Localité** — la responsabilité tient-elle dans un bloc, ou se disperse-t-elle entre
  plusieurs ?
- **Placement des coutures** — où tombent les frontières testables (`schemas.md` §5) ? Une
  couture mal placée oblige à mocker ce qui devrait être direct, ou inversement.

## Trancher

Une recommandation **assumée**, pas un vote — elle peut être hybride. Écris dans le plan, en une
phrase, ce que les autres formes auraient coûté : c'est l'écart qui évite de rejouer le débat
(`CLAUDE.md` §Entretenir le document d'architecture).
