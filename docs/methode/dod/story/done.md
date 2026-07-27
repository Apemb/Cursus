# DoD — incrément, sortie de `QA Review` / entrée en `Done`

> **Draft non éprouvé.** Écrit d'après l'état de l'art, pas récolté sur une exécution réelle —
> `D-039` demande l'inverse. À confronter au premier usage ; en cas de désaccord,
> `docs/methode/journal-frictions.md` prime sur ce fichier.

> **La question** : cet incrément peut-il être tiré en `Done`, c'est-à-dire compté comme livré ?
>
> **Le flux est tiré.** La colonne ne dit pas « c'est fini », elle dit « ça se fait ici » — c'est
> **l'aval** qui vérifie avant de tirer. Ce fichier est ce que vérifie quiconque s'apprête à poser
> `Done` : celui qui a fait `QA Review` quand elle était due, l'agent de `Code Review` sinon.
>
> `Code Review` a déjà statué sur le **comportement** ([`dod/story/code-review.md`](code-review.md)).
> Ici, on statue sur l'**incrément** — son acceptation, et sa recette manuelle si elle était due.

## 1. L'acceptation est cochée case par case

- [ ] Chaque case de l'acceptation du ticket (`tickets.md` §3 q.5) est cochée **individuellement**
      — « ça marche » n'est jamais une entrée valide, seule une case observable l'est
- [ ] La **preuve négative**, quand le ticket en portait une (ce qui doit rester vrai, le cas
      d'échec), est vérifiée au même titre que la preuve positive
- [ ] Aucune case n'est cochée par déduction depuis le vert de la suite : une case d'acceptation
      qui recoupe un test est vérifiée par ce test précisément nommé, pas par « la suite passe »

## 2. La conditionnalité de `QA Review` est explicite et tracée

C'est la clause qui se traverse en silence si on ne l'écrit pas. `QA Review` est **obligatoire**
dès que l'incrément touche la présentation — la couche non testée (`tickets.md` §7.12,
`architecture.md`) — et **sautée** pour un incrément purement Core. Sauter sans le dire n'est pas
distinguable, pour qui lit la carte ensuite, d'avoir oublié.

- [ ] La carte porte une ligne explicite : **`QA Review : requise`** ou **`QA Review : sautée`**
      — jamais un silence sur la question
- [ ] Si `requise` : le test décisif a été appliqué — *le diff touche-t-il un fichier sous la
      couche présentation, ou change-t-il ce qu'un utilisateur voit ou manipule ?* — et le
      parcours a été **rejoué à la main**, app lancée, par un humain
- [ ] Si `sautée` : la raison est écrite en une phrase — *« Core pur, aucun fichier de
      présentation touché »* suffit, mais elle doit être **écrite**, pas seulement vraie
- [ ] Dire qu'on la saute vaut mieux que traverser la colonne pour la forme — une `QA Review`
      cochée sans parcours rejoué est pire qu'une `QA Review` ouvertement sautée : elle fait
      croire à une vérification qui n'a pas eu lieu

## 3. Le critère opposable

> **Un incrément est fini quand quelqu'un qui ne lit pas le code peut constater, case par case,
> que la capacité promise est là — et que si elle touchait ce qu'on voit, quelqu'un l'a
> effectivement regardé.**

Il se teste : reprendre l'acceptation du ticket ligne à ligne, sans ouvrir le code, seulement en
utilisant l'app ou en lisant les cases déjà cochées par `Code Review`.

## 4. Ce qui n'est *pas* un critère de sortie

- **Que `Code Review` ait posé `Done`.** C'est un pré-requis pour entrer ici, pas ce fichier —
  voir [`dod/story/code-review.md`](code-review.md). Confondre les deux fait sauter cette DoD.
- **Que la suite soit verte.** Nécessaire depuis `Code Review`, pas suffisant ici : le vert prouve
  le comportement, pas que la capacité promise à l'échelle de l'incrément est complète.
- **Un parcours manuel quand `QA Review` était sautée à bon droit.** Rejouer l'app pour un
  incrément purement Core ne prouve rien de plus que la suite ne prouvait déjà — c'est
  précisément le cas que la conditionnalité existe pour éviter.
