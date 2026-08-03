# DoD — incrément, sortie de `Backlog`

> **Draft non éprouvé.** Écrit d'après l'état de l'art, pas récolté sur une exécution réelle —
> `D-039` demande l'inverse. À confronter au premier usage ; en cas de désaccord,
> `docs/methode/journal-frictions.md` prime sur ce fichier.

> **La question** : que faut-il à une carte entrée **latéralement** avant qu'elle puisse un jour
> être tirée en `Todo` ?
>
> Au niveau incrément, `Backlog` est l'**entrée latérale**, et rien d'autre (`tickets.md` §6.2,
> `D-072`) : ce qui arrive **sans parent**, donc sans spec — le refacto qu'aucune feature ne tire,
> la dette autonome, l'incrément déporté d'un découpage antérieur.
>
> ⚠️ **Une carte née d'un découpage n'entre pas ici**, même bloquée : elle naît en `Todo` et y
> attend, portant son `blockedBy`. Ce fichier ne traite donc plus qu'**une** population.

## 1. Le motif d'existence est nommé

- [ ] Refacto, dette, ou déport — écrit, pas laissé à deviner. C'est ce qui remplace le « pourquoi
      maintenant » qu'une spec aurait fourni

## 2. Le contexte tient dans la carte

Au même sens que [`todo.md`](todo.md) §2 — les six questions de `tickets.md` §3, répondues sur la
carte par une session neuve —, à deux nuances près qui viennent de l'absence de spec :

- [ ] **Q3 (« ce qui est déjà décidé ») n'a le plus souvent rien à citer.** Il n'y a pas de spec en
      amont ; l'absence de renvoi est ici légitime, pas un manque
- [ ] **La recette manquante est déclarée, pas cachée.** Sans spec, personne d'autre ne dira comment
      cette carte sera jugée faite — l'acceptation (Q5) doit donc être écrite **en entier** sur la
      carte elle-même, sans renvoi possible. C'est la clause qui coûte, et c'est le prix de l'entrée
      latérale
- [ ] **Q6 — le hors-périmètre** nomme un frère **si elle en a un** ; sans frère, elle le dit plutôt
      que de laisser la question vide

## 3. Le critère opposable

> **Une carte quitte `Backlog` quand elle satisfait `todo.md` — et la seule chose qu'elle doive
> produire de plus qu'une carte née d'un découpage, c'est sa propre recette.**

## 4. Ce qui n'est *pas* un critère

- **Un `blockedBy` soldé.** Ce n'est plus ce que cette colonne traite (`D-072`) : une carte bloquée
  vit en `Todo`, et c'est celui qui tire qui vérifie.
- **Une spec.** C'est précisément le trou que cette DoD **documente**, sans inventer une exigence
  qu'aucune autre partie du dépôt ne pose (`flux.md` §5, question ouverte).
- **Un ordre par rapport aux cartes de `Todo`.** Elles coexistent ; ni l'une ni l'autre ne passe
  devant par défaut.
