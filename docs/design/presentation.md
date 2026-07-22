# La couche de présentation et la composition

> **Statut** : document de conception, écrit avant le jalon 6. **Rien de ce qui suit n'est construit.**
> Il détient le *comment* de la jonction UI ; `architecture.md` §7.12 en détient la décision et les
> écarts. Les trois registres du dépôt s'appliquent ici aussi : **CONSTRUIT** (rien, à ce jour) /
> **TRANCHÉ, NON CONSTRUIT** / **QUESTION OUVERTE**.

---

## 1. Le critère, et pourquoi il remplace un nom de patron

Un patron d'architecture se discute sans fin parce qu'il ne peut pas échouer. Le critère retenu, lui,
peut échouer :

> **L'UI n'est qu'une façon d'instancier la logique et d'afficher des données. Un mode headless ou une
> CLI doit être réalisable sans réécrire une ligne de logique métier.**

Tout le reste de ce document en découle. Et §7 le rend **exécutable** : deux tests le font tomber le
jour où il cesse d'être vrai — ce qu'aucune relecture de code ne garantit.

Le cadre théorique correspondant est le **Humble Object** de Meszaros : extraire toute la logique du
composant difficile à tester, ne laisser qu'un adaptateur si mince que ne pas le tester n'est pas un
drame. MVVM en est une déclinaison, VIPER en est une autre. On garde le principe, pas le vocabulaire.

### 1.1 VIPER — écarté, et pourquoi l'écart mérite d'être écrit

VIPER (View / Interactor / Presenter / Entity / Router) a été **sérieusement envisagé** comme moyen de
rendre la couche visuelle testable. Il est écarté, pour trois raisons dont deux sont mesurées sur ce
dépôt :

1. **Deux de ses cinq composants existent parce qu'UIKit n'a ni binding ni navigation déclarative.** Le
   *Presenter* pousse à la main des valeurs formatées dans des vues — c'est exactement ce que le moteur
   de binding XAML fait gratuitement. Le *Router* pilote une pile de contrôleurs — Avalonia n'en a pas.
2. **Le Router serait vide ici, et c'est vérifiable** : une seule `Window` dans le dépôt, une seule
   surface (un `Grid` à trois colonnes), aucune modale, aucune pile de navigation. L'*Entity* est déjà
   là : `WorkflowDefinition`, `WorkflowRun`, `StepRun`, `Project` sont immuables et testés dans le
   noyau.
3. **Le bénéfice visé est acquis par défaut en Avalonia.** Un `ObservableObject` de
   `CommunityToolkit.Mvvm` est du POCO sans dépendance de framework UI : il se teste en `[Fact]`
   ordinaire, sans harnais. Ce qui manque n'est donc pas une architecture, c'est une **règle** — celle
   du §1, plus le fait de ne plus mettre de logique dans le code-behind.

Ce qu'il en reste, et qui est retenu : *la vue ne fait que binder*, et **ne pas la tester est assumé**.

⚠️ Cette condition était **massivement violée** avant le jalon 6c·1. Elle ne l'est plus **pour le neuf** :
le loader (§7 de `parcours.md`, `architecture.md` §4.14) met un `ShellViewModel` sur le chemin, et toute
sa logique vit en Core, testée. La violation **survit uniquement dans la dette sessions**, désormais
isolée : les ~90 lignes de comportement réel (réconciliation de collection, cycle de vie des PTY,
visibilité, focus) et le XAML qui binde directement sur un type du domaine ont été **extraits tels quels**
dans `Views/SessionsView.axaml.cs` — gelés, pas reproduits (§8). Le neuf respecte la règle ; l'ancien est
mis de côté en attendant que la forme des sessions soit connue.

---

## 2. Hexagonal partiel, et l'asymétrie est délibérée

Le dépôt est déjà à moitié hexagonal, sans que le mot ait été écrit. **Quatre interfaces publiques**
dans tout `src/`, toutes dans `Cursus.Core/Workflows/` : `IProcessRunner`, `IRunJournal`,
`IRunJournalReader`, `IClock`. Ce sont des **ports de sortie**, avec leurs adaptateurs — `ProcessRunner`,
`InMemoryRunJournal`, et `SqliteRunJournal` / `RunArtifactStore` dans `Cursus.Persistence`.

Ce qui manquait : le **port d'entrée**, par lequel un pilote (l'UI, une CLI, un cron) actionne le
système. C'est l'objet du §3.

**L'asymétrie assumée.** Un hexagonal orthodoxe exigerait aussi un `IFileSystem` : `ProjectStore`,
`WorkflowCatalog` et `RunContext` touchent le disque directement, et `ProcessRunner`
(`System.Diagnostics.Process`) vit dans le noyau, à côté de son propre port. **On ne le fait pas**, et
la règle qui l'explique :

> On inverse ce qu'on a besoin de doubler.

L'inversion de l'exécution a été payée parce qu'elle achetait un moteur testable en millisecondes sur
un double. L'inversion du système de fichiers n'achèterait rien : le dépôt teste déjà l'I/O contre les
binaires POSIX réels et des dossiers temporaires — décision assumée (`CLAUDE.md`), qui *teste
davantage* qu'un système de fichiers simulé. Payer la symétrie pour la symétrie serait le travers même
qui a fait écarter VIPER : une structure qui a l'air propre sans rien racheter.

---

## 3. `ProjectHost` — le composition root réifié — TRANCHÉ, NON CONSTRUIT

### 3.1 Ce que c'est

Une classe que l'on instancie **une fois par projet ouvert**, avec ses dépendances (ou des fabriques
permettant de les construire paresseusement), et qui **expose les modules déjà correctement montés**.

```
ProjectHost (racine de composition)
   ├── Project              — le projet ouvert
   ├── Workflows            — le catalogue (WorkflowCatalog)
   └── Runs                 — RunSupervisor : lancer, observer, annuler
```

### 3.2 La règle de sens unique — l'invariant central

> **`ProjectHost` construit les modules et leur passe leurs dépendances par constructeur. Aucun module
> ne connaît `ProjectHost`.**

C'est la seule chose qui puisse mal tourner ici, et l'erreur est à un cheveu : passer le host aux
modules pour qu'ils y prennent ce dont ils ont besoin transformerait le composition root en **Service
Locator**. Un module qui dépend du host ne se teste plus qu'en construisant le host entier — c'est-à-dire
en instanciant le vrai journal SQLite pour tester une projection d'affichage.

Corollaire à tenir : **chaque module doit rester construisible à la main** avec des doubles. `ProjectHost`
est une commodité pour la production, jamais un passage obligé pour les tests.

### 3.3 Pourquoi une racine plutôt que « l'appelant compose »

Parce que la composition n'est pas neutre, et le code le prouve : `SqliteRunJournal` détient **une seule
`SqliteConnection`, ouverte au constructeur, non synchronisée**, et il est `IDisposable`. Deux instances
sur le même fichier, ou une instance partagée entre deux threads, ne sont pas sûres — et l'erreur ne se
manifesterait pas par une exception franche mais par de la corruption intermittente. Laisser cette
règle à la charge de chaque appelant, c'est la voir violée au premier appelant distrait.

Deux conséquences que le host assume :

- **il est `IDisposable`** et possède le cycle de vie du journal ;
- **un projet ouvert = un host**. Ouvrir un autre projet, c'est disposer celui-ci et en construire un
  nouveau, jamais muter le courant. Sans quoi un run peut continuer d'écrire dans le journal du projet
  précédent.

Le paresseux se fait avec `Lazy<T>` (thread-safe par défaut). Gain concret : on peut lister les
workflows d'un projet **sans créer `cursus.db`**.

### 3.4 Où il vit — et comment le noyau reste ignorant de SQLite

`ProjectHost` vit dans **`Cursus.Core`**. Il doit pourtant fournir un `SqliteRunJournal`, alors que
`Cursus.Core` ignore `Cursus.Persistence` (§7.11, décision à ne pas casser). La sortie : le host reçoit
une **fabrique** (`Func<Project, IRunJournal>` ou équivalent) et ne sait pas ce qu'il y a derrière.

Pour que le câblage concret n'existe qu'**en un seul exemplaire** — sinon l'App et une future CLI le
dupliqueraient, ce qui est précisément ce qu'on cherche à éviter — **`Cursus.Persistence` fournit le
préréglage** : une fabrique unique qui construit un `ProjectHost` avec les bonnes dépendances. Les tests,
eux, construisent le host à la main avec `InMemoryRunJournal`.

*Alternative écartée* : un quatrième projet dédié à la composition. `Cursus.Persistence` est déjà le seul
endroit du dépôt qui connaît les deux mondes ; un projet de plus n'achèterait rien.

### 3.5 Ce que `RunSupervisor` contient — et ce qu'il ne contient pas

**Critère d'admission : la façade n'accueille que ce qui demande une composition.**

Les actions « ouvrir », « lister », « charger » sont des délégations d'une à trois lignes vers du noyau
déjà testé ; les emmurer produirait des passe-plats. `ProjectStore` et `WorkflowCatalog` restent donc
accessibles directement (le host les expose, il ne les réenveloppe pas).

Ce qui *demande* une composition, et n'existe nulle part aujourd'hui :

1. **assembler** moteur + runner + journal + magasin d'artefacts (fait uniquement dans les tests) ;
2. **posséder la politique de thread et d'annulation** — qui détient le `CancellationTokenSource`, sur
   quel thread tourne l'exécution, combien de runs simultanés ;
3. **traduire les sorties exceptionnelles en états d'écran** : `ProjectNotFoundException`,
   `InvalidProjectException`, `FileNotFoundException` (identifiant inconnu), plus les deux invariants
   que le moteur **relance intacts** après avoir clos le run — `UnknownStepException` et
   `PathEscapesWorkspaceException`. Une UI qui ne les attrape pas plante.

*Écart assumé avec « une porte d'entrée unique »* : il y a une racine unique, mais plusieurs modules. Un
objet-façade absorbant tout grandirait par construction (le tracker au jalon 7, l'éditeur au jalon 8) et
serait aux trois cinquièmes composé de délégations d'une ligne.

---

## 4. Observer un run — la couture, et le mur

### 4.1 Le mur, à connaître avant de dessiner quoi que ce soit

**Rien n'est émis entre `StepStarted` et `StepFinished`.** Deux verrous indépendants, qu'il faut lever
tous les deux pour streamer :

- le journal n'émet qu'aux **frontières d'étape** ;
- `ProcessRunner` lance bien la lecture des deux tubes en parallèle, mais ne les attend qu'**après** la
  fin du process : la sortie n'existe qu'à la mort de l'enfant.

Conséquence concrète et immédiate : sur le workflow `verifier` commité dans ce dépôt, pendant les une à
trois minutes du `dotnet test`, **le seul signal disponible est « `StepStarted(tester, 1)` a été reçu il
y a N secondes »**. Une progression indéterminée et un chronomètre : c'est tout ce que le code permet.
La maquette et le parcours doivent **assumer ce vide**, pas le masquer par un artifice qui laisserait
croire à une progression réelle.

### 4.2 La couture retenue : décorateur d'`IRunJournal` → `Channel` — TRANCHÉ, NON CONSTRUIT

`IRunJournal` est une interface à **une seule méthode**, `Append`, de retour `void`, appelée **en ligne
dans la boucle du moteur**. Un décorateur qui délègue puis publie est donc le point d'accroche évident,
et il ne touche pas une ligne de `WorkflowEngine`.

⚠️ **La publication doit être non bloquante.** `Append` s'exécute sur le thread du moteur : une UI lente
ralentirait le run, et si le run était lancé depuis le thread UI, ce serait un interblocage.

Le décorateur écrit donc dans un `Channel`, que le ViewModel consomme en `await foreach` **depuis le
thread UI**. Trois bénéfices, et c'est ce qui fait retenir cette forme :

- **zéro `Dispatcher` dans le code testable** : on part du thread UI, chaque `await` y revient ;
- l'écriture côté moteur reste non bloquante ;
- **en test, on pousse une séquence d'événements dans un `Channel`** et le test s'énonce : *étant donné
  cette séquence d'événements, l'écran affiche ceci.* C'est le cycle unidirectionnel visé, obtenu sans
  une seule classe de cérémonie.

*Alternative écartée* : le **sondage** de `IRunJournalReader`. Il introduit un second pilote du cycle,
indépendant des intentions de l'utilisateur, et exige une **seconde instance** de `SqliteRunJournal`
(connexion unique non partageable). À réserver au cas où l'on voudrait suivre un run écrit par un autre
process — ce qui n'existe pas.

### 4.3 Une seule source de vérité — TRANCHÉ

Il y a aujourd'hui deux descriptions du même état : le flux d'événements, et le `WorkflowRun` rendu à la
fin par `ExecuteAsync`. Elles sont redondantes par construction — `History` est exactement la suite des
`StepFinished`, `(State, AbortReason)` exactement la charge de `RunFinished`.

> **Le flux fait foi pendant le run. Le `WorkflowRun` rendu ne sert qu'à savoir que la tâche s'est
> terminée, et à attraper les exceptions.**

Sans cette règle : deux écrivains sur le même état, un écran qui « saute » à la fin, des lignes en
double.

### 4.4 Concurrence : un run à la fois — TRANCHÉ, et ce n'est pas une simplification de confort

`SqliteRunJournal` a une connexion unique et non synchronisée ; `InMemoryRunJournal` est un `List<>` nu ;
`Append` n'est protégé nulle part. **Un run à la fois est la seule configuration que le code supporte
aujourd'hui sans travail supplémentaire.** L'assumer explicitement, ou payer la synchronisation — mais
ne pas le découvrir en production.

Une précision utile pour 6b, apportée par la conception du 2026-07-21 : **la contention porte sur le
journal, pas sur les sorties**. Celles-ci vont déjà en fichiers (`RunArtifactStore`, un par visite), donc
N étapes concurrentes écrivent dans N fichiers distincts sans se gêner. Ce qui reste à synchroniser est le
journal, qui ne reçoit que les **frontières d'étape** — des écritures rares. Le problème de 6b est donc
plus petit qu'il n'y paraît, à condition de ne pas faire passer les sorties par la base.

### 4.5 Ce que l'écran calcule, et que le noyau ne dit pas — TRANCHÉ, NON CONSTRUIT

`RunState` est documenté « **état terminal** » : `Completed`, `Failed`, `Aborted`, plus un `AbortReason`.
**Il n'existe aucun état « en cours » dans le noyau**, et rien du tout pour l'attente. Tout ce que l'écran
affiche au-delà de ces trois valeurs est donc **calculé par la présentation** — ce qui n'est pas une
dérive mais la démonstration que l'arbitrage lui appartient (`parcours.md` §4, point 4).

| Affiché | Calculé à partir de | Existe ? |
|---|---|---|
| **En cours** | un `StepStarted` sans `StepFinished` correspondant | après 6a |
| **En attente** | aucune étape vivante, et l'autorisation de démarrer refusée (§7.13.1 d'`architecture.md`) | non construit |
| **Arrêt en cours** | intention d'arrêt posée localement **et** une étape encore vivante. **Jamais journalisé** : c'est un état de l'écran, pas du run | non construit |
| **Arrêté** | `Aborted` + `Canceled` | ✅ noyau |
| **Réussi** | `Completed` **et** dernière étape en code 0 | arbitrage d'écran |
| **Échoué** | `Failed`, **ou** `Completed` avec une dernière étape en code non nul — le cas du dépôt | arbitrage d'écran |
| **Boucle non convergente** | `Aborted` + `LoopNotConverging` | ✅ noyau |
| **Planté** | `Aborted` + `Faulted`, et l'exception remonte intacte — une UI qui ne l'attrape pas plante avec (§3.5) | ✅ noyau |

**Huit états d'écran pour trois états de noyau.** Le corollaire est la seule chose à retenir ici : *ce
calcul est de la logique*, donc il se teste en `[Fact]` nu, et **il n'a rien à faire dans un code-behind**.

Trois autres calculs relèvent exactement de la même règle, et forment ensemble le contenu testable de
l'écran de run :

- **Le regroupement par tours** — découper `History` en itérations pour la vue liste (« tour 1 / tour 2 /
  sortie de boucle »). Une projection pure sur `StepRun.Iteration`, qui existe déjà.
- **Le placement des nœuds** pour la vue graphe — un tri topologique par niveaux. ⚠️ Il **suppose un graphe
  acyclique**, ce qu'un workflow avec boucle n'est pas : il faut détecter l'arête de retour et l'exclure
  avant de répartir. Estimation : de l'ordre de 150 lignes de logique pure. C'est ce qui rend le graphe
  bien moins cher qu'un éditeur, et testable sans UI.
- **La sélection, partagée entre les deux vues.** Sélectionner une étape dans le graphe et basculer en
  liste doit surligner la visite correspondante. **Une seule notion de sélection, deux rendus qui ne se
  connaissent pas** — sinon on écrit deux fois la même logique de navigation et elles divergent.

---

## 5. Le terminal : là où « vue passive » se cogne au réel

`TerminalControl` n'est pas la projection d'un état : il **est** l'état — process enfant, moteur VT,
scrollback, sélection — et cet état vit dans l'arbre visuel. Deux faits du code l'imposent :

- **le PTY démarre sur un événement de vue** : `StartPty` est différé au `Loaded`, parce que les bounds
  doivent être connues. Le déclencheur est un fait que **seule la vue connaît** ; un présentateur qui
  commanderait « démarre maintenant » aurait tort par construction ;
- **le contrôle survit à sa propre invisibilité** : bascule par `IsVisible`, jamais par recréation
  (« façon TMUX »). Un `ItemsControl` bindé sur la collection — réflexe même de la vue passive —
  recyclerait les conteneurs et **tuerait les PTY**.

**Forme retenue : réconciliation idempotente.** La couche testable produit un **état déclaratif** — la
liste des identifiants de terminaux attendus, et lequel est visible. La vue en fait un réconciliateur :
créer les manquants, détruire les partis, régler la visibilité, poser le focus. C'est exactement ce que
le code-behind fait déjà, moins la plomberie d'abonnement. La clé stable existe : `TerminalSession.Id`.

Le contrat testable n'est donc **pas** une interface autour de `TerminalControl` (l'`ITerminalSession`
jamais écrite, §6.4 d'`architecture.md`) mais **le calcul de l'état déclaratif**, qui est pur.

---

## 6. Pièges Avalonia à ne pas apprendre par l'échec

Relevés lors de la passe de recherche du 2026-07-21. Les points marqués ⚠️ n'ont pas été vérifiés
sur ce dépôt : ils sont à confirmer au moment du câblage.

**Le générateur de faux-verts n°1** — proscrit par règle :

```csharp
if (Application.Current?.Dispatcher is { } d) d.Post(...);   // NON
```

En test, `Application.Current` est `null` : la branche est **silencieusement sautée**, le test passe
sans que le code testé s'exécute. Variante : `Dispatcher.CurrentDispatcher` en **crée un** sur le thread
appelant — un dispatcher fantôme dont la file n'est jamais pompée, donc des callbacks qui ne
s'exécutent jamais. En TDD strict, un rouge→vert obtenu ainsi ne prouve rien.

Règle : **jamais de garde silencieuse sur un service UI**. Si le service manque, c'est une erreur de
câblage, pas un no-op.

**Pas de `Progress<T>`** pour la progression d'un run : il capture le `SynchronizationContext` **à la
construction** ; sans contexte, les rapports partent sur le pool, potentiellement dans le désordre → test
intermittent. Utiliser un `IProgress<T>` synchrone, ou le `Channel` du §4.2.

**`CanExecute` n'est jamais réévalué automatiquement** (pas de `CommandManager` façon WPF) : il faut
`NotifyCanExecuteChanged()` ou `[NotifyCanExecuteChangedFor]`. C'est **un comportement à tester** — le
bouton qui ne se dégrise pas est l'oubli classique, et il ne se voit qu'à l'exécution.

**Ne pas asserter la séquence exacte de `PropertyChanged`**, sauf là où l'ordre *est* le comportement.
C'est un test fragile qui se casse à chaque refactor de setter. Asserter l'état final.

⚠️ **Avalonia 12 change deux choses** : les bindings compilés sont activés par défaut (le plugin
d'annotations de données ne l'est plus — les validations par attributs ne remontent plus seules dans
l'UI, mais restent testables au niveau du ViewModel), et le framework supporte **plusieurs dispatchers,
un par thread**, en recommandant aux bibliothèques de ne plus s'appuyer sur `Dispatcher.UIThread`. Si un
dispatcher devient inévitable, l'**injecter** derrière une petite interface maison — jamais le capturer
en dur.

---

## 7. Les deux tests qui rendent le critère exécutable — PARTIELLEMENT CONSTRUIT

1. **Test d'architecture** : `Cursus.Core` ne référence aucun assembly `Avalonia.*`. Il tombe le jour où
   quelqu'un glisse une dépendance UI dans le noyau — y compris par mégarde transitive. ✅ **Construit au
   jalon 6c·1** (`ArchitectureTests`, `architecture.md` §4.14) — non-vacuité prouvée en le retournant un
   instant sur un assembly réellement présent.
2. **Test end-to-end headless** : ouvrir le projet, lister, charger un workflow, le lancer, consommer le
   flux d'événements, vérifier l'état final — **sans instancier une seule ligne d'Avalonia**. **Pas encore
   construit** : il attend qu'un projet s'ouvre en mode run et qu'un workflow se lance depuis l'UI (marches
   suivantes du loader).

Le second est le critère du §1 transformé en assertion. S'il passe, le mode CLI existe déjà et il ne
reste qu'à lui écrire un point d'entrée ; s'il devient impossible à écrire, c'est que de la logique a fui
dans l'UI — et on le saura le jour où ça arrive, pas trois jalons plus tard. Il a aussi une vertu de
conception : il **force `ProjectHost` à être suffisant**, ce qu'aucune relecture ne garantit.

---

## 8. Questions ouvertes

| Question | Statut |
|---|---|
| **`INotifyPropertyChanged` dans `Cursus.Core`** — `SessionWorkspace` hérite d'`ObservableObject` et expose une `ObservableCollection` ; il *est* le view-model des sessions. **Gel décidé** : on n'y touche pas, la forme des sessions n'étant pas encore connue (`SessionKind.Agent` est mort et attend son jalon). Mais **l'invariant s'applique à tout ce qui est neuf** : le noyau publie des valeurs et des événements immuables, la transformation en état observable n'a lieu que dans `Cursus.App`. | **Reporté, non écarté.** Symptôme à connaître : `TerminalSession.Title` est mutable **sans** notification, alors que le XAML le binde — renommer une session ne rafraîchirait pas la liste |
| **La liste chronologique du run partage-t-elle la surface des terminaux ?** Il n'y a qu'un `TerminalHost`. Même panneau, troisième volet, ou onglet ? | **Ouverte** — c'est du produit, elle sera tranchée par le parcours utilisateur et les maquettes. Elle décide aussi si la question d'un routeur se rouvre |
| **Tests headless** — ⚠️ `Avalonia.Headless.XUnit` 12.x dépendrait de **xUnit v3**, alors que les deux projets de tests du dépôt sont en **xUnit 2.9.3**. À confirmer. | **Ouverte, et non bloquante** : le headless est de toute façon inadapté au cycle TDD (pas de parallélisme, isolation par test coûteuse). À réserver plus tard, ciblé sur l'intégration du contrôle RoyalTerminal, dans un projet séparé |
| **Stratégie `PATH`** — ré-enrichir dans `ProcessRunner`, déclarer dans `project.json`, ou exiger des chemins absolus | **Ouverte**, fléchée sur le jalon 6 par `architecture.md` §9.2-15. Sans réponse, les workflows commités de ce dépôt échoueront en `LaunchFailed` depuis l'app installée |
| **Nom du module de run** — `RunSupervisor` est retenu comme hypothèse de travail ; il lance, observe et annule | À confirmer à l'écriture. Écartés : `WorkflowRunner` (trois termes voisins existent déjà : `IProcessRunner`, `ProcessRunner`, `WorkflowEngine`), tout ce qui contient `Session` (mot déjà pris par les sessions terminal) |

---

## 9. Le langage visuel — TRANCHÉ sur les intentions, NON CONSTRUIT

Issu de la passe de maquettes du 2026-07-21 (archivées dans `docs/design/maquettes/jalon-6.html`, sans
autorité et non tenues à jour). Ce qui suit, lui, fait foi — et la frontière entre les deux est la seule
chose à comprendre pour entretenir cette section :

> **On consigne ce qui encode une décision, jamais ce qui l'exécute.** Que l'ambre soit *réservé* à
> l'attente est une décision — elle vient du troisième état du §7.13.1, pas du goût. Que cet ambre soit
> `#C9902E` est une exécution, à régler sur un écran réel et sans intérêt ici.

Une section de langage visuel qui liste des codes hexadécimaux vieillit en trois semaines et personne ne
la corrige. Une section qui dit *pourquoi cette couleur est prise* survit au premier changement de palette.

### 9.1 L'existant, et sa contradiction — À RÉSOUDRE AU CÂBLAGE

`App.axaml` déclare `RequestedThemeVariant="Default"`, qui **suit le thème système**. Mais
`MainWindow.axaml` code des couleurs **en dur** : `#1E1E24` pour la colonne, `#111114` pour l'hôte de
terminaux, `#33FFFFFF` pour les séparateurs. Les deux ne peuvent pas être vrais ensemble : en thème clair,
la fenêtre est claire et la colonne reste sombre.

Deux sorties, et il faut en choisir une plutôt que laisser le conflit :

- **assumer un thème unique sombre** — défendable pour un outil qui vit à côté d'un terminal, et cohérent
  avec ce que le code fait déjà. Alors `RequestedThemeVariant` doit dire `Dark`, pas `Default` ;
- **passer par des ressources de thème** et laisser le système décider. Plus de travail, et c'est le seul
  chemin si l'application doit un jour se montrer en plein jour à quelqu'un d'autre.

Non tranché. À poser au moment du câblage, pas avant : c'est une décision qui se juge sur un écran.

### 9.2 La disposition — quatre zones, et ce qui ne doit jamais y bouger

```
┌──────┬──────────────┬───────────────────────────────┐
│ rail │ colonne      │ surface principale            │
│      │ du projet    │ (liste des workflows, run,    │
│ pro- │ ┌──────────┐ │  configuration, Application)  │
│ jets │ │Run·Sess ⚙│ ├───────────────────────────────┤
│      │ └──────────┘ │ détail de la sélection        │
│ ──── │  listes      │ (log en direct, source, …)    │
│ état │              │                               │
└──────┴──────────────┴───────────────────────────────┘
```

Quatre invariants, chacun payé par une erreur commise pendant la passe :

1. **Une seule surface principale à la fois.** Jamais deux en compétition pour le même panneau — c'est le
   « pas de routeur » de `parcours.md` §4, et c'est ce qui a fait écarter les runs en onglets.
2. **Le panneau du bas montre le détail de ce qui est sélectionné dans la surface, et rien d'autre.**
   J'y avais proposé la trajectoire complète du run : elle décrit le run entier, pas l'étape sélectionnée.
   L'erreur est facile parce que la place est libre ; l'invariant est ce qui l'empêche.
3. **L'état de l'application vit en pied de rail**, hors de la hiérarchie des projets. Le poser dans la
   colonne du projet le ferait lire comme un état *de ce projet*.
4. **La configuration d'un projet est un engrenage près de son nom**, pas une destination de même poids que
   les modes. Le placement dit de quoi elle règle la configuration — *ce* projet, par opposition à l'écran
   Application, qui règle la machine.

### 9.3 La couleur porte l'état, et l'accent ne doit pas s'y mêler

Quatre valeurs sémantiques, et une seule règle qui les gouverne : **elles décrivent l'état d'un run, d'une
étape ou d'une ressource, jamais autre chose.**

| Valeur | Ce qu'elle signifie | Pourquoi elle est réservée |
|---|---|---|
| **réussi** | code 0, étape ou run terminé sans échec | — |
| **échoué** | code non nul, `Failed`, ou `Aborted` — y compris arrêté par l'humain | — |
| **en cours** | une étape est vivante | — |
| **en attente** | ⚠️ **réservée au troisième état** du §7.13.1 : ni échec, ni exécution — l'étape ne peut pas *encore* démarrer | Ne pas la dépenser pour un avertissement générique. Le jour où l'attente arrive, il n'y aurait plus de valeur libre pour elle, et elle serait rendue comme un échec — exactement ce que le §7.13.1 cherche à éviter |

Deux règles qui comptent autant que la liste :

- **L'accent d'interface — sélection, focus — doit être distinct des quatre.** Sans quoi « sélectionné »
  et « en cours » se confondent, et c'est la superposition la plus fréquente à l'écran : on sélectionne
  presque toujours l'étape qui tourne.
- **Jamais la couleur seule.** Chaque état porte aussi un libellé ou une forme. L'argument n'est pas
  seulement le daltonisme : une pastille verte sans mot ne dit pas si elle signifie « réussi » ou « prêt ».

**Point de départ, explicitement révisable** : le ton sombre déjà posé par le dépôt (§9.1), neutre à biais
froid. Il vient du monde du terminal, à côté duquel l'application vit.

### 9.4 La typographie sépare ce que la machine dit de ce qu'un humain a écrit

| Fonte | Ce qu'elle rend | Exemples |
|---|---|---|
| **Monospace** | ce qui vient du système ou du fichier | `tirerLeTicket`, `code 1`, `2 min 04 s`, `dev.2.stdout`, les logs, les chemins |
| **Proportionnelle** | ce qu'un humain a rédigé | « Passer toute la suite au vert » — le champ `name` d'une étape, les libellés d'interface |

Ce n'est pas un effet de style : c'est ce qui rend lisible **d'un coup d'œil ce qui est modifiable dans le
JSON**. Un identifiant d'étape et son libellé métier sont deux choses de nature différente, et l'écran doit
le dire sans avoir à l'expliquer.

Corollaire mécanique : **chiffres alignés** (`tabular-nums` ou son équivalent Avalonia) partout où ils
s'empilent en colonne — durées, pourcentages, compteurs. Sinon une liste de durées ondule et devient
illisible sans qu'on sache pourquoi.

### 9.5 Le terminal est une exception, et elle est assumée

RoyalTerminal détient son propre rendu et son propre fond (§5 : le contrôle *est* son état). **Un terminal
reste sombre dans une application claire** — c'est ce que tout le monde attend, et chercher à l'harmoniser
avec le thème de la fenêtre serait un travail contre l'attente de l'utilisateur. La même exception vaut
pour le panneau de sortie d'une étape, qui est du texte de terminal même s'il n'est pas interactif.

### 9.6 Ce qui reste ouvert

- **Les valeurs exactes** — palette, graisses, densité. À régler sur un écran réel, pas dans un document.
- **Le thème unique ou double** (§9.1), à trancher au câblage.
- **L'icône de l'application** — trou `architecture.md` §9.2-17, toujours ouvert.
- **La densité de la liste des visites** : lisible à huit visites dans la maquette ; à trente, il faudra
  décider entre compacter et faire défiler.
