# DoD — feature, sortie de `Discovery`

> **La question** : cette feature peut-elle être **tirée** en `Spec`, c'est-à-dire arbitrée ?
>
> **Le flux est tiré.** Une DoD n'est pas une condition de sortie que l'amont s'applique à
> lui-même : c'est **ce que l'aval vérifie avant de tirer**. Le spécifieur qui découvre qu'il
> doit d'abord redéfinir le besoin repose la carte et pose `Rework Needed`.
>
> Le *contenu* attendu est en `tickets.md` §2.1. Ici : uniquement de quoi il faut s'être
> acquitté. La `Discovery` est **l'unique sortie bon marché** du flux — une feature y meurt sans
> qu'aucun arbitrage technique ait été dépensé.

## 1. L'artefact est complet

- [ ] Le **besoin** est établi, et **formulé sans sa solution**. « Il faut un cache » n'est pas
      un besoin ; « l'écran met quatre secondes à s'ouvrir » en est un
- [ ] **Pour qui** est nommé
- [ ] **Pourquoi maintenant** : la place dans la trajectoire, ce que ça débloque, et **ce que
      coûte l'inaction**
- [ ] **Plusieurs pistes** sont ouvertes — une piste unique n'est pas une ouverture, c'est un
      choix déguisé en constat
- [ ] **Chaque piste porte ce qu'on en sait factuellement** — pas seulement ce qu'elle est. Une
      piste réduite à sa définition (*« un serveur MCP local : Cursus expose ses gestes comme les
      outils d'un protocole »*) n'apprend rien à qui devra l'arbitrer : elle nomme sans informer.
      Ce qui compte est le **fait connu** — un protocole existe et son SDK est stable, le dépôt
      n'en porte aucune brique, une contrainte technique est déjà documentée ailleurs
- [ ] Ce que la Discovery a **fait apparaître** est écrit, s'il y a lieu : un cadrage à défaire,
      deux besoins sous un même titre, un besoin qui n'était pas celui qu'on croyait. Ce
      résultat-là ne rentre dans aucune des questions de §2.1 et se perd si rien ne l'accueille

## 2. Aucun arbitrage n'a été rendu

C'est la clause la plus importante, parce que **ne pas arbitrer est la raison d'être de
l'étape** (`tickets.md` §2.1) : c'est ce qui permet de tuer une feature avant d'avoir dépensé.

- [ ] **Chaque piste est encore vivante.** Test : si l'une est présentée avec une raison de ne
      pas la retenir, l'étape a débordé sur la `Spec`
- [ ] Aucune estimation de coût, aucune faisabilité tranchée — c'est `Spec` §2.2 q.1

**La frontière est fine et se franchit sans s'en apercevoir** : *énoncer un fait connu* sur une
piste est légitime (« ce transport est incompatible avec la résidence ») ; *en tirer une
conséquence* ne l'est pas (« donc cette piste ne convient pas »). Le second est déjà de
l'arbitrage, même déguisé en constat. Se méfier des formats qui invitent à commenter chaque
piste : la colonne de commentaire appelle l'argument, et l'argument appelle la conclusion.

⚠️ **Cette clause et la matière factuelle de §1 tirent en sens contraire, et c'est voulu.** Une
relecture qui ne chicane que dans un sens produit **mécaniquement** la dérive opposée, et elle la
certifie : c'est arrivé le 2026-07-30, où trois tours ont validé une Discovery dont les cinq pistes
avaient été vidées de leur matière **pour satisfaire cette clause-ci**. Le relecteur doit donc
tester les deux : *cette piste est-elle déjà écartée ?* **et** *cette piste apprend-elle quelque
chose ?* Une piste qui n'apprend rien est aussi défaillante qu'une piste déjà jugée — la première
défaillance est simplement plus silencieuse, parce qu'elle ressemble à de la rigueur.

## 3. Le critère opposable

> **Une Discovery est finie quand la `Spec` peut commencer à arbitrer sans avoir à redéfinir le
> besoin.**

Il se **teste** : on tente le premier arbitrage. S'il faut d'abord se demander *pour qui* ou
*pourquoi maintenant*, le manque est en amont.

## 4. Ce qui n'est *pas* un critère

- **Avoir choisi une piste.** C'est `Spec`, et c'est même la définition de `Spec`.
- **Avoir listé toutes les pistes.** Une Discovery n'a pas à être exhaustive ; la `Spec` peut en
  ajouter. Exiger l'exhaustivité transformerait l'ouverture en inventaire.
- **Avoir chiffré quoi que ce soit.**
- **Une section listant les sorties possibles.** Elles vivent dans cette DoD, pas dans
  l'artefact : un document parle de son sujet, jamais de son propre processus.

## 5. L'artefact s'adresse à son lecteur, pas au dépôt

Une spec ou une discovery se lit **dans le tracker**, souvent par quelqu'un qui n'a pas le dépôt
sous la main. Trois conséquences, chacune payée par une relecture :

- [ ] **Les références sont des liens**, pas des chemins de fichiers. `trajectoire.md §Plus loin`
      n'est ni cliquable ni vérifiable depuis une carte
- [ ] **Aucune numérotation interne périssable.** « ce que débloque `2·2c` » ne dira plus rien
      dans trois mois ; **expliciter la conséquence** et lier la carte qui la porte
- [ ] **Aucun méta-commentaire de méthode.** « Ouverture, pas un choix » explique le gabarit au
      lieu de traiter le sujet. Poser la question directement — *« face à ce besoin, comment
      pourrait-on y répondre ? »* — fait le même travail sans se regarder écrire

## 6. Sortie latérale

`Canceled` est ici une **issue de plein droit**, pas un échec : *on ne fait pas*, ou *le besoin
n'est pas celui-là*. C'est le seul endroit du flux où elle est bon marché. Une feature annulée
mérite une phrase disant pourquoi — sans quoi elle se re-proposera.
