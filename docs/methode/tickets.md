# Anatomie d'un ticket

> **À quoi sert ce fichier.** Il dit ce que doit contenir un ticket du backlog, niveau par
> niveau, sous forme des **questions auxquelles chacun répond**. Il ne dit pas *quoi*
> construire — ça, c'est `trajectoire.md` — ni *pourquoi le découpage est ce qu'il est* —
> ça, c'est `architecture.md`.
>
> **Pourquoi il existe.** Deux raisons, et la seconde est la vraie. La première : un backlog
> dont les tickets ne se ressemblent pas se relit mal. La seconde : la destination du projet
> est que **Cursus consomme ces tickets** (`trajectoire.md` §La destination). Le jour où un
> `AgentStep` prend une carte, le ticket devient son **unique brief** — il n'aura pas la
> conversation qui l'a précédé, ni le contexte de celui qui l'a écrit. Un gabarit explicite
> cesse alors d'être du confort d'équipe pour devenir le **contrat d'entrée de la boucle**.
>
> **Corollaire à garder en tête en écrivant** : tout ce qu'on laisse implicite parce que
> « ça va de soi » est exactement ce qui manquera à l'agent.
>
> **Ce document est de la méthode, pas de la conception** : il bouge rarement. Si un ticket
> révèle un gabarit insuffisant, on l'amende — mais on n'y consigne ni décision produit ni
> état d'avancement.

---

## 1. Les trois niveaux, et la question qui les départage

| Niveau | Linear | La question à laquelle il répond | Portée |
|---|---|---|---|
| **Feature** | Projet | *De quoi sera-t-on capable qu'on ne pouvait pas ?* | Un cap de la trajectoire |
| **Incrément** (US) | Issue | *Quel comportement observable s'ajoute ?* | Livrable seul, suite verte |
| **Pas** | Sous-tâche | *Quel est le prochain pas concret ?* | Un commit, un cycle TDD |

**Le test qui départage un incrément d'un pas** : si on le livrait seul et qu'on arrêtait
là, quelqu'un le remarquerait-il ? Oui → c'est un **incrément**. Non → c'est un **pas**.

Ce test est plus fiable que la taille. « Écrire l'adaptateur Keychain » est petit mais reste
un pas : seul, il ne change rien pour personne. « Le jeton vit dans le trousseau » est un
incrément, même s'il tient en deux commits — après lui, un secret ne traîne plus en clair.

**Corollaire** : un incrément qui n'a qu'un seul pas n'a pas besoin de sous-tâches. On ne
découpe pas pour découper ; on découpe quand l'ordre des pas est **une information**.

---

## 2. Ce que contient une **feature** (projet Linear)

Une feature est un **cap**, pas un thème. « Round-trip Linear » en est un ; « Améliorations
diverses » n'en est pas un — un fourre-tout n'a pas de fin, donc pas de moment où on le
ferme.

Les questions, dans l'ordre où elles se posent au lecteur :

1. **Quel est le but ?** Une phrase, en capacité gagnée. Pas une liste de tâches.
2. **Où en est-on déjà ?** Ce qui est **construit** et sert de socle — sans ça, le lecteur
   suivant refait ce qui existe. Renvoyer au `D-NNN` plutôt que recopier.
3. **Que reste-t-il ?** Les incréments, nommés, dans leur ordre de dépendance.
4. **Quel est le pré-requis ?** Ce qui doit exister avant que cette feature ait un sens.
   Souvent une autre feature ; parfois rien.
5. **Qu'est-ce qui est déjà tranché, et qu'est-ce qui ne l'est pas ?** Les trois registres
   de `architecture.md` valent ici aussi : **construit** / **tranché mais pas construit** /
   **question ouverte**. Un « prévu » présenté comme un « fait » désoriente autant dans un
   ticket que dans un document.
6. **Quelles vertus doivent survivre ?** Les invariants que l'implémentation ne doit pas
   casser en chemin — souvent la partie la plus facile à perdre.

---

## 3. Ce que contient un **incrément** (issue Linear)

C'est le niveau qui porte la charge. Un agent qui reçoit une carte reçoit **ça**.

### Les six questions

1. **Quel comportement s'ajoute ?** En une phrase, à l'indicatif, observable de
   l'extérieur. « Le jeton Linear vit dans le trousseau » — pas « Gérer les secrets ».
2. **Pourquoi maintenant ?** Ce qui rend ce comportement nécessaire *à ce moment*. Un
   incrément sans réponse à cette question est peut-être prématuré.
3. **Qu'est-ce qui est déjà décidé, et qu'il ne faut pas rouvrir ?** Les décisions prises
   ailleurs qui contraignent celle-ci, avec leur renvoi (`D-NNN`, § du document). C'est ce
   qui évite qu'on rejoue un débat déjà tranché — le risque principal quand l'exécutant
   n'était pas là pour la discussion.
4. **Quels pièges sont déjà payés ?** Les gotchas connus qui touchent ce terrain. Le dépôt
   en a une réserve chèrement acquise ; les rappeler dans le ticket est ce qui évite de les
   repayer. C'est la section que l'agent ne peut **pas** deviner.
5. **Quelle est l'acceptation ?** Des cases à cocher **observables**. « Ça marche » n'est
   pas une acceptation ; « un run rouge ne ferme pas la carte » en est une. Y inclure la
   preuve *négative* quand elle existe : ce qui doit **rester** vrai, et le cas d'échec.
6. **Qu'est-ce qui reste explicitement dehors ?** Le hors-périmètre assumé, et ce qui est
   volontairement laissé ouvert. Sans ça, l'exécutant élargit — ou pire, croit avoir
   trouvé un oubli et « corrige » une décision.

### Ce qu'un incrément **ne** contient pas

- **Le comment.** La conception vit dans le **plan gaté** exigé par `CLAUDE.md` dès qu'un
  changement crée une classe, traverse des modules ou implique une découpe non évidente. Le
  ticket dit *quoi* et *pourquoi* ; le plan dit *comment*. Un ticket qui prescrit
  l'implémentation ligne à ligne a mangé le plan — et il sera périmé avant d'être pris.
- **La liste des tests.** La *test list* naît du plan, pas du backlog. Le ticket peut nommer
  l'invariant à prouver ; il n'énumère pas les cas.
- **Ce que le dépôt dit déjà.** On **renvoie** (`D-032`, `architecture.md` §7.10.5), on ne
  recopie pas : une copie diverge, un renvoi vieillit honnêtement.

---

## 4. Ce que contient un **pas** (sous-tâche Linear)

Trois lignes suffisent souvent. Les questions :

1. **Quel est le pas ?** Un titre qui tient en une action.
2. **Pourquoi celui-là, à cette place ?** Surtout quand l'ordre n'est pas évident — « les
   trois opérations en dépendent, la faire d'abord évite de la disperser en trois copies ».
3. **Quel est le piège local ?** S'il y en a un. Sinon, ne rien écrire vaut mieux qu'un
   paragraphe de remplissage.

Un pas **n'a pas besoin d'acceptation formelle** : son acceptation est le cycle TDD lui-même
— un rouge observé, un vert, un refactor. La suite verte et les 0 warning sont l'acceptation
de **tous** les pas, elle n'a pas à être répétée dans chacun.

---

## 5. Règles transverses

**Le pourquoi, jamais le quoi.** Même règle que pour les commentaires de code : un ticket
qui paraphrase son titre est du bruit ; un ticket qui explique un arbitrage vaut de l'or.

**Écrire les écarts.** Quand une alternative a été envisagée puis écartée, le dire dans le
ticket — comme dans les commits et dans `decisions.md`. C'est ce qui évite qu'on la propose à
nouveau six mois plus tard, en croyant l'inventer.

**Nommer les questions ouvertes comme telles.** Un ticket a le droit de ne pas trancher. Il
n'a pas le droit de laisser croire que c'est tranché. Formulation à préférer : « à décider
explicitement, pas par défaut ».

**Renvoyer, systématiquement.** `D-NNN` pour le pourquoi historique, `architecture.md` §X
pour le découpage, `trajectoire.md` pour la place dans le chemin. Un ticket sans renvoi est
un ticket qui a perdu sa généalogie.

**Une story ne se juge pas à sa taille mais à sa conséquence.** « Renommer le projet ouvert
rafraîchit son titre » est minuscule et légitime : la conséquence est visible. « Refactorer
le sérialiseur » ne l'est pas tant qu'on n'a pas dit ce que ça change pour quelqu'un.

**La langue.** Français pour les tickets, comme pour les commits et la documentation ;
anglais pour les identifiants de code cités. Diacritiques corrects et complets.

---

## 6. Écrire pour un agent

Ce qui suit est **tranché mais pas encore éprouvé** — aucun agent n'a encore consommé de
ticket de ce backlog. À amender dès que la boucle tournera pour de vrai (`CUR-9`).

Le point de départ : un agent arrive **sans la conversation**. Ce que l'humain compense par
le contexte partagé, le ticket doit le porter. En pratique, quatre choses qu'un exécutant
humain du projet sait sans qu'on le lui dise :

- **Le standard non négociable** — `dotnet build` 0 warning, `dotnet test` entièrement vert,
  à *chaque* commit. Il n'a pas à être répété par ticket, mais il conditionne toute
  acceptation.
- **Le régime TDD** — rouge observé *pour la bonne raison* d'abord, jamais de code de
  production sans test qui le réclame. Un ticket qui demanderait « implémente puis teste »
  contredirait le dépôt.
- **La frontière testé / non testé** — la couche présentation est hors du périmètre testé
  (`architecture.md` §7.12) et validée à la main. Un ticket qui touche l'UI doit le dire,
  sinon l'agent écrira des tests là où le dépôt n'en veut pas — ou l'inverse.
- **Les conventions de modélisation** — en particulier : pas de nullable pour distinguer des
  **types** d'objets. C'est la règle la plus facile à violer de bonne foi.

**Ce qui reste à décider** quand la boucle tournera : jusqu'où le ticket doit porter ces
rappels lui-même, et jusqu'où l'amorce de l'agent (son prompt système) les porte à sa place.
Dupliquer dans chaque carte est coûteux et vieillit mal ; ne rien dire suppose une amorce
qu'on n'a pas encore écrite. **Question ouverte, à trancher au premier round-trip réel.**

---

## 7. Correspondance avec Linear

| Ici | Linear | Note |
|---|---|---|
| Feature | Projet | Un projet = un cap qui se ferme |
| Incrément | Issue | Le niveau qui porte la charge |
| Pas | Sous-tâche (`parentId`) | Rattachée aussi au projet, pour rester visible |
| Ordre | `blockedBy` | Ce qui empêche de prendre une carte trop tôt |

La correspondance entre les **statuts** Linear et les états métier de Cursus (`modele-metier.md`
§5.1) est d'une autre nature — c'est un contrat **machine**, lu par le prédicat de
disponibilité et écrit par les étapes-tâches. Elle ne vit pas ici : voir `CUR-5` et
`architecture.md` §7.10.5.

> Rappel qui vaut d'être écrit une fois : **le succès d'un agent n'est pas la fermeture de la
> tâche.** `HumanReview` est un état de première classe. Un ticket dont l'acceptation
> impliquerait qu'un run vert ferme la carte tout seul contredit le modèle métier.
