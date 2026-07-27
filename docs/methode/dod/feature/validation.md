# DoD — feature, sortie de `Validation`

> **Draft non éprouvé.** Écrit d'après l'état de l'art, pas récolté sur une exécution réelle —
> `D-039` demande l'inverse. À confronter au premier usage ; en cas de désaccord,
> `docs/methode/journal-frictions.md` prime sur ce fichier.

> **La question** : cette feature peut-elle être tirée en `Completed` ?
>
> **Le flux est tiré.** Une DoD n'est pas une condition de sortie que l'amont s'applique à
> lui-même : c'est **ce que l'aval vérifie avant de tirer**. Ici encore, l'aval est un
> **humain, irréductiblement** — régime *Œil* (`tickets.md` §6.3). Le signal de fin est le
> label `Done`, posé directement par cet humain : contrairement au régime *Trio*
> (`Discovery`/`Spec`), il n'y a pas de tiers distinct qui juge la conformité pendant que la
> production reste ailleurs — la même personne rejoue la recette et prononce.
>
> Le *contenu* attendu d'une spec, y compris sa recette, est en `tickets.md` §2.2. Ici :
> uniquement de quoi il faut s'être acquitté pour sortir.

## 1. Pourquoi ce n'est pas redondant avec les `QA Review` déjà passées

C'est la clause la plus importante, parce que c'est la raison d'être de l'étape
(`tickets.md` §6.1) : **toutes les stories peuvent porter `Done`, chaque `QA Review` due
peut avoir eu lieu, sans que la capacité promise soit là.** Chaque niveau se recette contre
son **propre** artefact, jamais contre celui d'un niveau voisin :

| Niveau | Se recette contre |
|---|---|
| Pas | Sa test list (le vert) |
| Incrément | Son acceptation (`QA Review`, quand due) |
| Feature | Sa **spec** — précisément ce que `Validation` vérifie |

- [ ] **Aucune conclusion n'est tirée du fait que les incréments sont `Done`.** Ce fait est un
      pré-requis d'*entrée* en `Validation` (`tickets.md` §6.1, ligne `In Progress`), pas une
      preuve de recette. Le confondre, c'est déjà avoir clos l'étape avant de l'avoir faite.

## 2. La recette est rejouée contre la spec, item par item

- [ ] Le document `Spec` est **rouvert** — celui qui porte la recette (`tickets.md` §2.2 q.3),
      pas un résumé qu'on en aurait gardé en tête.
- [ ] **Chaque item de la recette est rejoué**, un par un, contre le produit livré — pas
      déduit de la liste des tickets fermés.
- [ ] Pour chaque item, un verdict à trois issues, jamais une prose libre :
  - [ ] **tenu** — l'item se vérifie tel quel ;
  - [ ] **manquement** — l'item ne se vérifie pas : la capacité promise est absente ou fausse.
        **Bloquant** ; la feature n'est pas tirable en `Completed` ;
  - [ ] **écart accepté** — le produit diverge de la recette écrite, mais l'écart est jugé
        acceptable. **Écrit**, avec sa raison — sur la carte ou dans la spec — sinon un
        manquement se déguise en écart accepté.
- [ ] **Un seul manquement suffit à reposer la carte.** `Validation` ne fait pas la moyenne.

## 3. Le critère opposable

> **Une feature est validée quand chaque item de sa recette a été rejoué contre le produit
> livré, item par item — pas déduit du nombre de tickets fermés.**

Il se teste directement : pour chaque item de la recette, demander *« qui l'a vu marcher, et
quand »*. Une réponse qui remonte à un ticket fermé plutôt qu'à un geste fait dans l'app dit
que la case n'a pas vraiment été cochée.

## 4. Ce qui n'est *pas* un critère

- **Repasser chaque `QA Review`.** Déjà fait ; `Validation` ne les rejoue pas une à une, elle
  vérifie la **capacité composée**, que des `QA Review` locales et vertes ne garantissent pas.
- **Un chiffre de couverture.** La recette n'est pas un pourcentage de tests ; c'est une liste
  d'items qu'on regarde un par un.
- **Redéfinir la recette ici.** Si un item ne fait plus sens, ce n'est pas une découverte de
  `Validation` mais un retour en amont — la spec est un contrat, on ne l'amende pas en la
  validant.

## 5. Sortie

`Validation` n'a pas de sortie bon marché : contrairement à `Discovery`, buter ici a déjà
coûté toute une feature construite. Un manquement repose la carte en `Rework Needed`, il ne
l'annule pas.
