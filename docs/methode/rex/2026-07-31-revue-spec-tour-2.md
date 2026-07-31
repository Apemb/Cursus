# 2026-07-31 — `revue-spec`, seconde exécution

> Second tour sur le même artefact, *Un agent pilote Cursus*, le jour même. Deux différences avec
> le tour 1, et elles sont voulues : le **prompt est allégé** des deux clauses qu'il rappelait à la
> main, et le **geste de pose est écrit** dans le primitif.
>
> ⚠️ **Comparabilité partielle.** La DoD a gagné trois cases entre les deux tours (`D-049`), et
> l'artefact a gagné une section entière. Les chiffres se comparent, les remarques non.

## 1. Ce qui a tourné

`revue-spec` sur le document `Spec — Un agent pilote Cursus`, carte en colonne `Spec` +
`Review Requested`. Un sous-agent `general-purpose` en arrière-plan, qui invoque le skill, lequel
passe le mandat au primitif `revue` avec deux axes ouverts en sous-agents séparés.

**Le tour n'était pas celui que la table prescrit.** `cycle-feature.md` §4 appelle `verification`
(temps ④) sur `Rework Done`, pas une revue. Le temps ② a été rejoué à la place, pour un motif
propre à cet artefact : le §8 — le plan d'implémentation — était **du contenu neuf que personne
n'avait relu**, né de `D-049` le soir du tour 1, et la DoD avait gagné trois cases dans la foulée.
Une vérification étroite aurait refermé la boucle en laissant la moitié technique de la spec sans
relecture. Le pari — qu'une revue subsume la vérification — est instruit au §4 et en journal 36.

**Deux préparatifs, tous deux issus de la réserve du tour 1** :

- le geste `cursus linear comment add` **est entré dans `revue` §6**, avec sa garde d'ambiguïté et
  l'interdit `D-045`. Il vit dans le primitif et non dans l'instance, parce que les trois instances
  posent sur la carte ;
- le prompt **a perdu les deux clauses** que le tour 1 rappelait — session neuve, ne pas déplacer
  la carte. C'était la seule façon de mesurer ce que le skill tient seul.

## 2. Chiffres

| | Tour 1 | Tour 2 |
|---|---|---|
| Durée de travail | 727 s | **619 s** |
| Jetons du sous-agent | ~135 000 | **82 447** |
| Appels d'outils | 36 | **24** |
| Sous-agents ouverts | 2 | **2** (un par axe) |
| Remarques posées | 11 | **12** — 6 Conformité (dont **2 violations dures**), 5 Découpabilité, 1 hors mandat |
| Retenues à la reprise | 11 / 11 | **12 / 12 — aucun refus motivé** |
| Défauts trouvés par l'humain, pas par la revue | 0 | **1** (voir §4) |

Moins cher, plus court, plus de remarques. À ne pas lire comme un progrès du skill : l'artefact
avait changé, et un document déjà repris une fois se relit plus vite.

## 3. Conformité au protocole

| Clause | Tenue | Ce qui l'atteste |
|---|---|---|
| Session neuve, sur l'artefact seul (`revue-spec` §1, `D-039`) | **oui, sans rappel** | Le fil de rédaction ne lui a pas été transmis, et le prompt ne le lui demandait plus |
| Exactement deux axes, jamais fondus (§2) | **oui** | Deux sous-agents séparés, deux rapports distincts |
| Deux citations par constat (`revue` §3) | **oui** | Chaque remarque porte son référentiel et le passage visé |
| Écarter la justesse (§3) | **oui** | 1 constat rangé « hors mandat — justesse », hors verdict |
| Étiqueter la confiance (`revue` §5) | **oui** | 2 *violation dure*, 9 *jugement*, 1 hors mandat |
| Lister sans réécrire (`revue` §6) | **oui** | Document inchangé jusqu'à la reprise |
| Poser la remarque sur la carte (`revue` §6, `D-045`) | **oui** | 12 remarques ancrées, aucune sur le document |
| Poser l'étiquette, ne jamais déplacer (§4) | **oui, sans rappel** | `Rework Needed` posé, colonne `Spec` inchangée |

**La réserve du tour 1 est levée.** Les deux clauses retirées du prompt ont tenu seules, et le
geste de pose n'a plus eu à être trouvé. ⚠️ Ce dernier point n'est pas mesuré : rien ne dit si le
relecteur l'a lu dans le skill ou retrouvé comme au tour 1 (journal 37).

## 4. Qualité de la sortie

**Douze remarques sur douze retenues, aucun refus motivé** — second tour d'affilée à ce score, sur
un artefact que son auteur croyait complet pour la seconde fois.

Les deux violations dures ont été **vérifiées dans le code avant reprise**, et elles tiennent :

- **« Les gestes existent tous côté noyau » (§4) est faux.** *Arrêter un run* figure dans
  l'inventaire opposable de la parité et n'est méthode d'aucun type du noyau : `ProjectHost`
  n'expose ni `Stop` ni `Cancel`, le geste vit dans `RunViewModel.Stop()` et son
  `CancellationTokenSource` appartient au view-model qui a lancé le run.
- **« Le serveur est un adaptateur, et rien d'autre » (§8.2) ne tient pas pour le lancement.**
  `LaunchAsync` frappe le `runId` en interne et son `Task<WorkflowRun>` n'aboutit qu'à la
  terminaison d'un run qui dure des heures — donc un outil ne peut ni rendre l'identifiant du run
  démarré ni le laisser arrêter sans que le serveur détienne un état entre deux appels.

Les deux portent sur des passages **que le tour 1 avait lus sans rien y trouver**. C'est
l'argument le plus fort en faveur d'un second tour sur artefact repris.

### Et le défaut que la revue n'a pas vu

**Le §8.1 tranchait « on priorise le projet dédié » ; le schéma §8.3 logeait l'hôte et l'adaptateur
dans `Cursus.App`.** Contradiction interne, deux paragraphes d'écart. Aucun des deux axes ne l'a
relevée — pire, la remarque hors mandat écrit *« l'hébergement dans `Cursus.App` est instruit et
mesuré »*, c'est-à-dire qu'elle **adopte la version du schéma** contre le texte qui la contredit.

C'est l'humain qui l'a vue, en relisant à l'œil nu, pendant que la revue tournait. Et le défaut
n'était pas cosmétique : il masquait une conséquence structurelle — un projet dédié ne peut pas
résoudre un projet vers son host, puisque cette résolution n'existe que comme lambda dans
`App.axaml.cs`. La racine doit descendre hors de la présentation, ce que ni §8.1 ni §8.6 ne
disaient.

**Ce que ça coûte de savoir** : `D-049` vient de rendre un schéma obligatoire dans chaque spec, et
le mode de défaillance qu'il introduit n'a aucun garde-fou (journal 35).

## 5. Frictions

Journal des frictions, entrées **35** (le schéma faux échappe à la lecture chicanière), **36** (le
tour ② subsume le temps ④), **37** (le prompt allégé n'a rien coûté).

## 6. Ce que le tour a changé

- **Le primitif `revue` porte enfin son geste de pose** (§6), avec la garde d'ambiguïté et
  l'interdit `D-045`. Placé en §6 plutôt qu'en section neuve pour ne pas renuméroter §8, vers
  laquelle `cycle.md` §4 et deux fiches renvoient — une fiche est figée, son renvoi serait
  incorrigible.
- **La spec a gagné ce que douze remarques réclamaient**, dont : une **intention de maille** en
  cinq incréments avec son critère de coupe (*ce qui se recette seul*, pas l'écran) ; la
  définition de ce qu'est un incrément *mutant* ; le sort de la clause 3, qui répartit sa **charge**
  sans répartir son **référentiel** ; et le lancement asynchrone traité comme la seule entorse au
  sans-état.
- **Deux arbitrages rendus par l'humain** : la parité intégrale est maintenue comme périmètre de
  première feature, et le texte fait foi contre le schéma — donc la racine descend.
- **Un geste entre au périmètre** : l'arrêt d'un run, qui n'existe pas au noyau et que la spec
  annonçait comme acquis.

## 7. Verdict pour `revue-spec`

**Confirmé, avec un angle mort nommé.**

Deux tours, vingt-trois remarques, vingt-trois retenues. Le skill trouve ce que le binôme ne voit
pas, et le fait sans les béquilles que le tour 1 lui donnait. Le critère de `D-043` est atteint une
seconde fois.

L'angle mort n'est pas dans le skill mais dans le primitif : **`revue` ne dit nulle part de
confronter un schéma à la prose qui l'entoure**, et c'est désormais un défaut qui a coûté. Le
remède appartient à `revue` — un axe, une clause de §3, ou une mention dans l'instance `revue-spec`
puisque `D-049` fait du schéma une pièce obligatoire de cet artefact-là. À écrire avant un tour 3.

**Ce que ce tour n'établit toujours pas** : les temps ③ et ④. `correction` a été jouée à la main
pour la seconde fois ; `verification` n'a pas eu lieu, et le motif de son remplacement est
circonstanciel — il ne vaudra pas pour un artefact repris à la marge.
