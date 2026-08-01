# DoD — feature, sortie de `Spec`

> **La question** : cette feature peut-elle être **tirée** en `In Progress`, c'est-à-dire
> découpée en incréments ?
>
> **Le flux est tiré.** Une carte entre dans une colonne quand le travail de cette colonne
> **commence**, et y reste jusqu'à ce que l'aval la tire. Une DoD n'est donc pas une condition
> de sortie que l'amont s'applique à lui-même : c'est **ce que l'aval vérifie avant de tirer**.
> Le découpeur qui bute repose la carte et pose `Rework Needed`.
>
> Corollaire : la colonne ne peut pas dire « c'est fini » — elle dit « ça se fait ici ». Le
> signal de fin est le label **`Done`** du groupe *Advancement Labels*, apposé par le relecteur
> tiers. Une spec achevée mais non encore tirée reste en `Spec`, portant `Done`.
>
> **Ce fichier se lit seul.** Il ne demande de connaître ni le fonctionnement des incréments ni
> celui des pas. Le *contenu* attendu d'une spec est en `tickets.md` §2.2 ; le *pourquoi* du
> régime de jugement en §6.3. Ici : uniquement de quoi il faut s'être acquitté pour sortir.
>
> **Un artefact, un document.** La spec est un **document Linear distinct** de celui de
> `Discovery` — elle ne le prolonge pas, elle lui succède. Deux registres dans un même document
> invitent à arbitrer en rédigeant le besoin, ce que `Discovery` s'interdit précisément
> (`tickets.md` §2.1).

## 1. L'artefact est complet

Les **huit questions** de `tickets.md` §2.2 ont chacune une réponse — ou un **« sans objet »
explicite**. L'omission silencieuse est le seul cas interdit : une spec a le droit de ne pas
trancher, pas de laisser croire que c'est tranché (`tickets.md` §5).

⚠️ **Les questions se cochent, le plan se constate.** Le gabarit de `D-054` dit *où* chaque réponse
atterrit par défaut ; ce sont les **questions** qui restent le référentiel de complétude. Une spec
qui répond aux huit dans un ordre différent n'est pas incomplète — mais elle doit dire pourquoi, et
l'écart doit servir le lecteur, pas l'auteur.

- [ ] Le document **suit le plan** de `tickets.md` §2.2 : fonctionnel, technique, état des
      décisions, annexes. Un écart est motivé dans le document lui-même
- [ ] Les **titres sont ceux du gabarit** — pas des titres maison. Un titre que seul l'auteur
      comprend est un coût payé à chaque lecture, et l'agent qui consommera le document n'a personne
      à qui demander
- [ ] Le document **ne porte pas de table « où j'ai répondu à quoi »** : cette vérification est
      celle du relecteur contre cette DoD, et une table logée dans l'artefact finit par diverger de
      lui

⚠️ **Ce qui se coche ici est une trace, pas un jugement** (`D-053`). L'arbitrage est l'acte du
**binôme** humain ↔ agent, pas du document — la spec l'enregistre. Le relecteur ne prononce donc
jamais que l'arbitrage est *bon* : il vérifie qu'il est **écrit et argumenté**. C'est le partage
conformité / justesse de `D-041`, appliqué au contenu de la spec. Une case qui ne se coche pas
signale un binôme qui n'a pas tranché, pas une rédaction à reprendre — et le remède est de
retourner interroger, pas de réécrire.

- [ ] Les options sont **arbitrées**, avec faisabilité et coût
- [ ] **Les écarts sont écrits** — ce qui a été envisagé puis écarté, et pourquoi
- [ ] La **capacité** est énoncée : une phrase à l'indicatif, pas une liste de tâches
- [ ] La **recette** de la feature est définie, en **Gherkin**, en annexe B (`D-054`)
- [ ] Les **règles d'atterrissage**, si la recette en porte — une clause exemptée de tomber dans un
      incrément, une clause qui se répartit en charge sans se répartir en référentiel — sont **dans
      le corps**, pas en annexe : ce sont des instructions au découpage, pas des scénarios
- [ ] La spec **renvoie** au document `Discovery` — elle ne recopie ni le besoin ni les pistes,
      et une spec sans ce lien est orpheline de sa raison d'être
- [ ] Le **socle** est nommé (ce qui est déjà construit, par renvoi)
- [ ] Le **pré-requis** est nommé, ou déclaré inexistant
- [ ] Les **trois registres** sont tenus : construit / tranché non construit / question ouverte
- [ ] Les **invariants à ne pas casser** sont nommés (§2.3) — et **seulement les non-dérivables** :
      ce que `CLAUDE.md` ou `architecture.md` portent déjà se renvoie, ne se recopie pas. Y restent
      les invariants **de cette feature-ci**, que rien d'autre n'écrit
- [ ] Le **plan d'architecture** existe (`D-049`) — il porte les **solutions envisageables**,
      **celle qu'on priorise** et pourquoi, **comment on compte la concevoir**, et les **grandes
      dépendances** à ajouter ou modifier, nommées
- [ ] Il conçoit **à l'échelle du système et du module** (`D-053`) : composants, frontières entre
      eux, dépendances externes. **Pas la forme des objets** — elle appartient au plan de design de
      chaque incrément, et l'anticiper ici est le symptôme le plus courant de la spec qui déborde
- [ ] Il est porté par **au moins un schéma**. Un plan d'architecture en prose seule ne se
      relit pas — c'est le seul endroit de la spec où le visuel n'est pas un agrément
- [ ] Sa **profondeur est celle dont le découpage a besoin** : assez de vue d'ensemble pour qu'on
      puisse tracer les frontières des incréments et leur donner leur orientation technique, pas
      une ligne de plus. ⚠️ Une spec qui prescrit l'implémentation ligne à ligne a mangé le plan
      de design, et elle périmera avant d'être prise

## 2. La revue a eu lieu et ses divergences sont soldées

- [ ] Une **revue tierce** a eu lieu : session neuve, **sur l'artefact seul**, sans le fil qui
      l'a produit (`D-039`)
- [ ] **Chaque divergence est soldée** — reprise dans la spec, ou refusée **avec sa raison
      écrite**. Une divergence sans suite écrite n'est pas soldée
- [ ] **L'humain prononce l'accord**

Le relecteur **liste, il ne tranche pas** (posture de *Vérification*, `CLAUDE.md`). Il n'y a
pas d'escalade ici : l'humain est déjà dans la pièce, donc c'est lui qui prononce.

## 3. Le critère opposable

> **Une spec est finie quand le découpage peut avoir lieu sans revenir poser de question.**

Il se **teste** — on tente le découpage — là où les cases ci-dessus se cochent. C'est
l'équivalent, au niveau feature, du test de départage de `tickets.md` §1. Le découpage est le
consommateur désigné de la spec : §3 q.5 en fait déjà foi (*si une part de la recette
n'atterrit dans aucun incrément, le découpage a un trou*).

C'est aussi ce qui **mesure la profondeur** du plan d'architecture (§1). Le découpage a besoin de
deux choses, et de rien d'autre : de quoi tracer les **frontières** entre incréments, et de quoi
donner à chacun son **orientation technique**. Un plan qui ne les porte pas est trop court ; un plan
qui descend aux objets est trop long — il fait le travail de `Planning`, qui n'en sait pas encore
assez pour le faire bien (`D-053`).

Si le découpage bute, le manque est dans la spec — pas dans le découpage.

## 4. Ce qui n'est *pas* un critère de sortie

- **Avoir défini la recette ne suffit pas.** La recette est un **contenu** de la spec (§2.2
  q.3) et le référentiel de `Validation`, tout à la fin. La confondre avec la fin de `Spec`
  fait clore l'étape aux trois quarts.
- **Les incréments nommés et ordonnés.** Le découpage a lieu **au passage** en `In Progress`,
  pas avant (`tickets.md` §2). Une spec peut porter une *intention* de maille, pas des cartes.
- **Le plan de design d'un incrément.** Il appartient à l'incrément et s'écrit à sa prise, en
  `Planning`, à l'échelle des objets. Le **plan d'architecture** exigé ci-dessus (§1) est d'ensemble
  et indicatif là où celui-ci est local et engageant (`D-049`, `D-053`).

## 5. Sortie latérale

`Canceled` reste légitime **jusqu'ici** — mais coûte déjà des arbitrages techniques. La sortie
bon marché était `Discovery`. Une feature annulée mérite une phrase disant pourquoi.
