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
| **Feature** | Projet | *À quel besoin répond-on, et qu'est-ce qu'on construit ?* | Un cap de la trajectoire |
| **Incrément** (story) | Issue | *Quel comportement observable s'ajoute ?* | Livrable seul, suite verte |
| **Pas** | Sous-tâche | *Quel est le prochain pas concret ?* | Un commit, un cycle TDD |

**Le test qui départage un incrément d'un pas** : si on le livrait seul et qu'on arrêtait
là, **le rôle produit** le remarquerait-il ? Autrement dit : est-ce **recettable** par
quelqu'un qui ne lit pas le code ? Oui → c'est un **incrément**. Non → c'est un **pas**.

Le lecteur compte autant que la question. Formulée avec un « quelqu'un » anonyme, elle laisse
chacun choisir son juge — l'utilisateur de l'app, le développeur du lendemain, l'agent — et
ils ne répondent pas pareil. Le juge est le **rôle produit**, sur ce dépôt une casquette de
l'auteur plutôt qu'une personne distincte.

Ce test est plus fiable que la taille. « Écrire l'adaptateur Keychain » est petit mais reste
un pas : seul, il ne change rien pour personne. « Le jeton vit dans le trousseau » est un
incrément, même s'il tient en deux commits — après lui, un secret ne traîne plus en clair.
Un client Linear **en lecture seule**, qui ne touche jamais le vrai tableau, est un pas
malgré ses cinq commits : rien de recettable n'en sort.

**Corollaire** : un incrément qui n'a qu'un seul pas n'a pas besoin de sous-tâches. On ne
découpe pas pour découper ; on découpe quand l'ordre des pas est **une information**.

### Chaque niveau produit son propre artefact

Ce ne sont pas trois formes du même « plan », et les confondre fait écrire la mauvaise chose
au mauvais endroit :

| Niveau | Artefact | Quand il s'écrit | Sa fraîcheur |
|---|---|---|---|
| Feature | Une **spec** | Avant l'ouverture, en `Discovery` puis `Spec` | Datée — c'est un contrat, il ne bouge pas sous les pieds |
| Incrément | Un **plan d'archi** | **Une fois, au découpage**, par qui a la vue d'ensemble | Datée |
| Pas | Une **test list** | **À la prise du pas** | **Vivante** — un cas découvert au rouge s'y ajoute |

Les deux régimes cohabitent sans se contredire : planifier l'architecture d'avance ne périme
pas, parce qu'elle décrit des frontières ; planifier *tous* les cas de test d'avance périmerait,
parce que ce qu'on apprend au pas 1 change ce qu'on sait au pas 4.

**Équivalence Jira**, pour qui arrive avec ce vocabulaire : feature ≈ epic, incrément ≈ US,
pas ≈ sous-tâche. Les mots du dépôt restent ceux-ci — « epic » désigne couramment un
conteneur thématique sans fin, exactement ce que « un cap qui se ferme » refuse.

---

## 2. Ce que contient une **feature** (projet Linear)

Une feature est un **cap**, pas un thème. « Round-trip Linear » en est un ; « Améliorations
diverses » n'en est pas un — un fourre-tout n'a pas de fin, donc pas de moment où on le
ferme.

Son artefact est la **spec**, et elle s'écrit en **deux temps** — qui sont deux colonnes,
mais surtout **deux compositions**. C'est le changement de composition qui fait la frontière,
pas un moment du raisonnement.

### 2.1 `Discovery` — à quel besoin répond-on ?

Autour de la table : le **produit** et l'**UX**. Une seule question, et elle est assez
importante pour occuper une colonne à elle seule.

1. **Quel besoin, et pour qui ?** Pas une solution déguisée en besoin. « Il faut un cache »
   n'est pas un besoin ; « l'écran met quatre secondes à s'ouvrir » en est un.
2. **Pourquoi ce besoin mérite-t-il qu'on s'y arrête maintenant ?** Sa place dans la
   trajectoire, ce qu'il débloque, ce qu'il coûte de ne rien faire.
3. **Quelles pistes existent ?** Une **ouverture**, pas un choix. Nommer des directions
   possibles est utile pour jauger le terrain ; les départager ne se fait pas ici.

**Ce que `Discovery` ne fait pas : arbitrer.** Et c'est sa raison d'être. Garder ce temps
séparé, c'est se réserver le droit de **tuer une feature avant d'avoir dépensé le moindre
arbitrage technique** — « on ne fait pas », ou « le besoin n'est pas celui-là », sont des
sorties légitimes et bon marché.

### 2.2 `Spec` — qu'est-ce qu'on construit ?

La **tech** et la **QA** rejoignent la table. C'est ce qui rend l'arbitrage possible : on
n'arbitre pas une faisabilité sans la tech, et on ne définit pas une recette sans la QA.

1. **Quelles options, à quel coût ?** L'étude de faisabilité et l'estimation — légère, elle
   sert à *arbitrer*, pas à s'engager. **L'écart mérite d'être écrit autant que le choix** :
   ce qui a été envisagé puis écarté, et pourquoi. Un arbitrage structurant se déverse
   ensuite en `D-NNN`.
2. **Qu'est-ce qu'on construit ?** La capacité gagnée, énoncée précisément. Une phrase à
   l'indicatif, pas une liste de tâches.
3. **Comment le recettera-t-on ?** La QA à la table sert à ça, et c'est **d'ici que descend
   l'acceptation** : la recette de la feature est ce contre quoi `Validation` jugera, et le
   découpage la répartira ensuite entre les incréments. Une spec sans recette est un document
   d'intention ; avec elle, c'est un contrat.
4. **Où en est-on déjà ?** Ce qui est **construit** et sert de socle — sans ça, le lecteur
   suivant refait ce qui existe. Renvoyer au `D-NNN` plutôt que recopier.
5. **Quel est le pré-requis ?** Ce qui doit exister avant que cette feature ait un sens.
   Souvent une autre feature ; parfois rien.
6. **Qu'est-ce qui est déjà tranché, et qu'est-ce qui ne l'est pas ?** Les trois registres
   de `architecture.md` valent ici aussi : **construit** / **tranché mais pas construit** /
   **question ouverte**. Un « prévu » présenté comme un « fait » désoriente autant dans un
   ticket que dans un document.
7. **Quelles vertus doivent survivre ?** Les invariants que l'implémentation ne doit pas
   casser en chemin — souvent la partie la plus facile à perdre.

### Ce qu'une feature **ne** contient pas

- **Ses incréments, nommés et ordonnés.** Le découpage a lieu au **passage en
  `In Progress`**, pas à l'écriture de la spec. Ce qu'elle peut porter, c'est une *intention*
  de découpage — une idée de la maille. Les cartes naissent à l'ouverture, pas au backlog.
- **Le plan d'archi.** Il appartient à l'incrément. La feature décide *quelle solution et si
  elle vaut le coup* ; l'incrément décide *comment c'est structuré*. Le premier est un
  arbitrage, le second une conception.

---

## 3. Ce que contient un **incrément** (issue Linear)

C'est le niveau qui porte la charge, et son artefact est le **plan d'archi** — celui que
`CLAUDE.md` exige dès qu'un changement crée une classe, traverse des modules ou implique une
découpe non évidente, schéma-delta compris. Il s'écrit **une fois, au découpage**, par qui a
la vue d'ensemble de la feature ; c'est aussi le moment où les incréments naissent, et le seul
où quelqu'un voit leurs frontières les unes par rapport aux autres.

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
   Elle ne s'invente pas ici : elle est **la part de la recette de la feature** (§2.2) qui
   revient à cet incrément. Si une part de la recette n'atterrit dans aucun incrément, le
   découpage a un trou.
6. **Qu'est-ce qui reste explicitement dehors ?** Le hors-périmètre assumé, et ce qui est
   volontairement laissé ouvert. Sans ça, l'exécutant élargit — ou pire, croit avoir
   trouvé un oubli et « corrige » une décision.

   À écrire **en regard des frères**, pas dans l'absolu. Un exécutant qui peut remonter à la
   feature et lire les incréments voisins voit du même coup *tout ce qui reste à faire* : la
   navigation rend cette question plus critique, pas moins. « Pas ici, c'est `CUR-12` » vaut
   mieux que « hors périmètre ».

### Ce qu'un incrément **ne** contient pas

- **Le comment.** La conception vit dans le **plan gaté** exigé par `CLAUDE.md` dès qu'un
  changement crée une classe, traverse des modules ou implique une découpe non évidente. Le
  ticket dit *quoi* et *pourquoi* ; le plan dit *comment*. Un ticket qui prescrit
  l'implémentation ligne à ligne a mangé le plan — et il sera périmé avant d'être pris.
- **La liste des tests.** Un incrément qui énumère ses cas a mangé le plan ; il peut nommer
  l'invariant à prouver, pas les cas. La *test list* appartient au **pas** (§4), et elle s'y
  écrit à sa prise — ce qui est proscrit, c'est la test list **en amont du découpage**, pas la
  test list dans le backlog.
- **Ce que le dépôt dit déjà.** On **renvoie** (`D-032`, `architecture.md` §7.10.5), on ne
  recopie pas : une copie diverge, un renvoi vieillit honnêtement.

---

## 4. Ce que contient un **pas** (sous-tâche Linear)

Le pas est **entièrement technique** — et c'est le niveau destiné à être **entièrement
automatisé** : plan, développement, revue. Son artefact est la **test list**, et elle
s'écrit à la prise du pas, pas au découpage : ce qu'on apprend au pas 1 change ce qu'on sait
au pas 4, et une test list planifiée d'avance serait un petit *waterfall* qui périmerait. Elle
reste vivante pendant le cycle — un cas découvert au rouge s'y ajoute.

Les questions :

1. **Quel est le pas ?** Un titre qui tient en une action.
2. **Pourquoi celui-là, à cette place, et où s'arrête-t-il ?** **La question la plus
   importante des trois, et la seule qui ne se rattrape pas.** Au découpage, quelqu'un avait
   toute la feature en tête et voyait les frontières entre les pas ; cette vue disparaît avec
   la session qui l'a produite. Ce qui n'est pas écrit là n'existe plus. Nommer le frère
   voisin vaut mieux qu'une justification abstraite — « les trois opérations en dépendent, la
   faire d'abord évite de la disperser en trois copies ».
3. **Quel est le piège local ?** S'il y en a un. Sinon, ne rien écrire vaut mieux qu'un
   paragraphe de remplissage.

Un pas **n'a pas besoin d'acceptation formelle** : la suite verte et les 0 warning sont
l'acceptation de **tous** les pas, elle n'a pas à être répétée dans chacun.

En revanche, **le vert n'est pas une validation de la test list**. Il prouve que le test
passe ; il ne dit ni que le bon comportement est prouvé, ni qu'il est bien formulé — un test
peut être vert, conforme à la convention `étant donné / quand / alors`, et vérifier la
mauvaise chose. Le raffinement de la test list et de la formulation des comportements relève
donc de la revue (§6), au même titre que le code.

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

## 6. Le cycle de vie — ce que chaque étape exige, par niveau

**Le principe.** Le statut dit *où en est le travail* ; le niveau dit *ce que cette étape
exige*. Les deux ne se déduisent pas l'un de l'autre : « In Progress » sur une sous-tâche
veut dire « un cycle TDD tourne », sur un incrément « la série de cycles est en cours », sur
une feature « au moins un de ses incréments a démarré ». Même mot, trois exigences.

**Les trois chemins n'ont pas la même longueur**, et c'est voulu :

```
Sous-tâche   Todo → In Progress → Done
Incrément    Backlog → Todo → [Planning → Plan Review] → In Progress → Code Review → [QA Review] → Done
Feature      Backlog → Planned → In Progress → Completed
```

Les crochets marquent les étapes **conditionnelles** — voir la matrice. Un chemin court
n'est pas un chemin bâclé : la revue d'une sous-tâche existe, elle a simplement lieu au
niveau de l'incrément, là où le comportement est enfin observable.

### La matrice

| Statut | **Feature** (projet) | **Incrément** (issue) | **Pas** (sous-tâche) |
|---|---|---|---|
| **Backlog** | Le cap est nommé et argumenté, pas encore ordonné dans la trajectoire | L'incrément est décrit ; son « pourquoi maintenant » n'a pas encore de réponse | Y séjourne quand elle a été écrite d'avance parce que l'**ordre des pas était une information** |
| **Todo** / *Planned* | Le cap est le prochain à ouvrir ; ses pré-requis sont levés | **La colonne d'éligibilité** : plus aucun `blockedBy` ouvert, le contexte tient dans la carte | Son incrément est `In Progress` et ce pas est le suivant |
| **Planning** | — *(un projet ne se planifie pas, il s'ordonne)* | **Conditionnel** : seulement si le changement crée ou supprime une classe, traverse plusieurs modules, ou implique une découpe non évidente. Sinon **on saute** directement à `In Progress` | — *(le plan est porté par l'incrément parent)* |
| **Plan Review** | — | Le plan est écrit, porte son **chemin de fichier en tout premier** et son **schéma-delta** ; il **attend une validation humaine** | — |
| **In Progress** | Au moins un incrément a démarré | La série de cycles TDD tourne ; la documentation se met à jour **au fil**, pas à la fin | Un cycle : rouge observé *pour la bonne raison*, vert, refactor |
| **Code Review** | — | Le comportement est **complet** ; le diff se relit d'un bloc, les commits sont argumentés, `architecture.md` / `decisions.md` sont à jour | — *(on ne relit pas un commit isolé, on relit un comportement)* |
| **QA Review** | — | **Conditionnel** : obligatoire dès que l'incrément touche la présentation (§7.12, non testée) — l'app est lancée, le parcours refait à la main. **Sautée** pour un incrément purement Core, et le dire vaut mieux que traverser la colonne pour la forme | — |
| **Done** / *Completed* | Tous ses incréments sont `Done`, `trajectoire.md` acte la jambe, un `D-NNN` est écrit si un arbitrage a été tranché | L'acceptation est cochée **case par case**, la validation manuelle est faite si elle était due | Commit fait, suite verte, **0 warning** |

### Les deux colonnes de rendez-vous

`Plan Review` et `QA Review` ne sont pas des formalités : ce sont les deux endroits où le
travail **s'arrête et attend un humain**. Elles matérialisent dans l'outil ce que
`modele-metier.md` §5.1 pose comme état de première classe — `HumanReview`. Une carte ne les
traverse pas toute seule, quel que soit le verdict de la gate.

C'est aussi la réponse opérationnelle à « le succès d'un agent n'est pas la fermeture de la
tâche » : quand la boucle tournera, un `AgentStep` déplacera une carte de `Todo` vers
`In Progress` à l'entrée, puis vers `Code Review` à la sortie — **jamais jusqu'à `Done`**. Le
dernier pas reste humain, par construction et non par prudence temporaire.

### Ce qui n'est pas une étape

`Canceled` et `Duplicate` sont des **sorties**, pas des colonnes de travail. Une carte
annulée mérite une phrase disant pourquoi : un abandon non expliqué se re-proposera.

> **Registre.** Les chemins ci-dessus sont **tranchés** ; leur usage par une machine est
> **tranché mais pas éprouvé** — aucun agent n'a encore déplacé de carte. Ce qui reste
> **ouvert** : quelle colonne exactement porte l'éligibilité au déclenchement automatique
> (`Todo` seul, ou `Todo` + une étiquette), et si une reprise après échec de gate renvoie la
> carte en `Todo` ou la laisse en `In Progress` marquée. Voir `CUR-5`.

---

## 7. Écrire pour un agent

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

## 8. Correspondance avec Linear

| Ici | Linear | Note |
|---|---|---|
| Feature | Projet | Un projet = un cap qui se ferme |
| Incrément | Issue | Le niveau qui porte la charge |
| Pas | Sous-tâche (`parentId`) | Rattachée aussi au projet, pour rester visible |
| Ordre | `blockedBy` | Ce qui empêche de prendre une carte trop tôt |

**Ce que la §6 couvre, et ce qu'elle ne couvre pas.** Elle dit ce qu'un statut **exige** — de
quoi il faut s'être acquitté pour en sortir, selon le niveau. C'est une convention de
travail, lue par des humains, et elle a sa place ici.

Le **contrat machine** est autre chose : quels identifiants de colonnes et d'étiquettes le
prédicat de disponibilité observe, et lesquels les étapes-tâches écrivent. Il vit dans
`project.json`, se lit sans ambiguïté et ne tolère pas la nuance — d'où sa séparation. Voir
`CUR-5` et `architecture.md` §7.10.5.

La frontière est facile à perdre de vue : « une carte en `Code Review` attend un humain » est
une convention ; « la colonne `Code Review` porte l'identifiant `b972d7e7…` » est un contrat.
Écrire le second ici le condamnerait à diverger du premier renommage de colonne.
