# DoD — incrément, sortie de `QA Review`

> **Draft non éprouvé.** Écrit d'après l'état de l'art, pas récolté sur une exécution réelle —
> `D-039` demande l'inverse. À confronter au premier usage ; en cas de désaccord,
> `docs/methode/journal-frictions.md` prime sur ce fichier.

> **La question** : cet incrément peut-il être tiré en `Done` ?
>
> **Le flux est tiré.** Une DoD n'est pas une condition de sortie que l'amont s'applique à
> lui-même : c'est **ce que l'aval vérifie avant de tirer**. Ici, l'aval n'est pas un tiers
> agent mais un **humain, irréductiblement** — régime *Œil* (`tickets.md` §6.3), le seul qui
> ne se délègue pas. Qui bute repose la carte et pose `Rework Needed`.
>
> Le *contenu* attendu de l'acceptation d'un incrément est en `tickets.md` §3 q.5. Ici :
> uniquement de quoi il faut s'être acquitté pour sortir.

## 1. La conditionnalité — la clause qui compte

C'est elle qui rend cette DoD utile plutôt que rituelle : `QA Review` n'est **pas** une colonne
que tout incrément traverse.

- [ ] **Le diff a été regardé** : touche-t-il la présentation (`architecture.md` §7.12 — la
      zone non testée, hors `Cursus.Core`) ?
  - **Oui** → `QA Review` est **obligatoire**. La colonne se traverse réellement.
  - **Non** (incrément purement Core) → `QA Review` **se saute** ; la carte va directement de
    `Code Review` à `Done`.

- [ ] **Qui décide** : la personne qui s'apprête à tirer la carte au-delà de `Code Review` —
      celle qui aurait fait la QA Review si elle avait eu lieu. Ce n'est jamais un verdict
      qu'un agent prononce, même quand la frontière Core/présentation est mécanique à
      constater (quels fichiers ont changé) : le régime est *Œil*, humain irréductiblement.

- [ ] **Où la trace vit** : un incrément sauté ne laisse pas une colonne vide en silence. Un
      commentaire sur la carte dit pourquoi, une phrase suffit — *« purement Core, §7.12 non
      engagé, QA Review sautée »*. Sans elle, personne ne peut distinguer plus tard *sauté à
      raison* de *oublié* — c'est exactement ce que vise `tickets.md` §6.2 : « le dire vaut
      mieux que traverser la colonne pour la forme ».

## 2. Quand elle a lieu : le parcours est rejoué, pas relu

- [ ] L'app est **lancée pour de vrai**, pas déduite du code. Pour la démarrer, invoquer le
      skill `run` — il sait déjà lancer ce dépôt-ci et retrouve seul le patron adapté ;
      redécrire la commande ici créerait un entretien parallèle qui divergerait de
      l'implémentation réelle du skill.
- [ ] Le **comportement que l'incrément promettait** (`tickets.md` §3 q.1, l'indicatif) est
      rejoué à la main, du geste d'entrée à l'effet observable — pas un sous-ensemble qui
      évite le chemin délicat.
- [ ] La **preuve négative**, si l'acceptation en portait une (`tickets.md` §3 q.5) — ce qui
      doit rester vrai, le cas d'échec — est rejouée aussi, pas seulement le chemin heureux.

## 3. Le critère opposable

> **Un `QA Review` est passé quand quelqu'un a vu, dans l'app réellement lancée, l'effet que
> l'incrément promettait — et « sauté » ne veut jamais dire « oublié » : la carte porte la
> phrase qui dit pourquoi.**

Il se teste en deux temps selon la branche : soit le parcours a été rejoué et personne ne peut
le contester sans relancer l'app soi-même, soit l'absence de passage est **écrite**, pas
seulement vraie.

## 4. Ce qui n'est *pas* un critère

- **Relire le diff.** C'est fait en `Code Review` ; `QA Review` ne relit pas du code, il
  éprouve un comportement.
- **La suite de tests verte.** C'est l'acceptation de `Done`, pas de `QA Review` — et c'est
  précisément parce que le vert ne couvre pas la présentation (§7.12) que cette étape existe.
- **Un compte-rendu détaillé du parcours.** Un constat oui/non, avec ce qui a coincé s'il y a
  lieu — pas un procès-verbal.
- **Tester des cas que l'incrément ne promettait pas.** `QA Review` recette contre
  l'acceptation de **cet** incrément, pas contre une exploration libre — élargir ici, c'est
  refaire `Validation` en petit.
