# Journal des frictions — exécution du flux sans skill

> Tenu selon `D-039` : on exécute la méthode **sans** skill, on note chaque friction au fil de
> l'eau, et le journal écrit le skill — après deux ou trois passages, jamais après un seul.
> Une ligne brute par occurrence. Ce fichier n'est pas de la méthode, c'est de la matière.
>
> **Ce fichier ne dit pas si ça progresse** — il n'a aucune structure qui permette de comparer deux
> passages. C'est [`rex/`](rex/README.md) qui le fait : une fiche par exécution, rubriques fixes.
> Une fiche renvoie ici par numéro d'entrée, et ne recopie jamais.

## 2026-07-26 — Discovery de « un agent pilote Cursus »

1. **Le cadrage se présentait comme acquis et était faux.** Le projet portait *deux besoins*
   (piloter sans fenêtre / tourner sans fenêtre) sous *trois solutions* dans son titre. La
   première demi-heure a servi à défaire un titre. — *étape 1*
2. **Une carte éligible et inexécutable.** `CUR-32` n'avait aucun `blockedBy`, donc était
   prenable ; ses trois critères d'acceptation supposaient tous le daemon. Le blocage vivait
   dans la prose. Deuxième occurrence du motif `D-033`. — *étape 4, dette*
3. **Une carte qui ne passe pas le test de départage du dépôt.** `CUR-28` : acceptation
   entièrement technique, rien d'observable par le rôle produit. C'est le refacto orphelin, le
   trou que `flux.md` §5 nomme déjà sans le couvrir. — *étape 4, dette*
4. **L'étape `Spec` n'avait pas de condition d'arrêt** — il a fallu que l'utilisateur la
   réclame. `tickets.md` §2.2 donne 7 questions, §6.1 donne 3 exigences de sortie : les deux ne
   se recouvrent pas, et rien ne disait laquelle fait foi. — *étape 2*
5. **`tickets.md` §6.3 se contredit.** Le tableau dit que l'agent de revue « valide » ; le corps
   dit que la spec n'est pas délégable et que la posture est « lister les divergences, ne pas
   trancher ». Remonte à `D-036`. — *étape 3*
6. **Les critères étaient noyés dans 460 lignes.** Répondre à « cette feature peut-elle être
   prise ? » imposait de charger tout le fonctionnement des pas. D'où l'extraction en
   `docs/methode/dod/<niveau>/<statut>.md`. — *transverse*
7. **Un document unique pour Discovery + Spec invite à arbitrer en rédigeant le besoin** — ce
   que `Discovery` s'interdit précisément. La séparation en deux documents Linear rend la faute
   difficile au lieu de la déconseiller. — *étapes 1 et 2*
8. **La conversation avait déjà arbitré avant que la Discovery existe.** Transport, découpage en
   incréments, ordre : tout était sur la table avant qu'une ligne de Discovery soit écrite. Il a
   fallu **désarbitrer** pour l'écrire — remettre en pistes ce qui était déjà des conclusions.
   Friction la plus coûteuse du lot, et la moins visible. — *étape 1*
9. **Une sonde valait mieux qu'une déduction.** Conclusion tirée d'un champ absent dans une
   réponse d'API (« les labels de projet ne sont pas groupés ») — l'utilisateur a corrigé.
   Vérifier avant d'argumenter sur l'état d'un système tiers. — *transverse*
10. **Une création par API échouée n'est pas une création annulée.** Deux erreurs de socket sur
    le serveur MCP Linear ; la seconde sur la création du document. Réessayer à l'aveugle aurait
    pu produire un doublon — il a fallu **lister avant de recréer**. Le dépôt connaissait déjà le
    défaut en théorie (pas d'idempotence en création chez les trackers, d'où la clé de
    corrélation journalisée *avant* l'appel) ; c'est sa première rencontre au réel. — *transverse*
11. **Le flux est *tiré*, et ce n'était écrit nulle part.** J'ai supposé un flux poussé — donc
    laissé la carte en `Backlog` pendant que je produisais son artefact, et attendu la fin pour
    proposer de la déplacer. En flux tiré, la carte entre dans la colonne quand le travail y
    **commence**. `tickets.md` §6.1 le pratiquait déjà implicitement (« la bascule pas engagé →
    engagé tombe à l'entrée en `Spec` ») sans jamais le nommer. **Conséquence de fond** : une DoD
    n'est pas une condition de sortie que l'amont s'applique, c'est ce que **l'aval vérifie avant
    de tirer** — et c'est ce qui donne au label `Done` sa raison d'être, la colonne ne pouvant
    plus dire « fini ». Le genre de convention qu'un agent ne peut pas déduire. — *transverse*
12. **Le gabarit de `Discovery` a trois questions, l'artefact a demandé cinq sections.** Les deux
    en trop ont été improvisées : *ce que la Discovery a fait apparaître* (une découverte de
    cadrage ne rentre dans aucune des trois questions — et c'était ici le résultat le plus
    important) et *sorties légitimes*. — *étape 1*
13. **J'ai pré-arbitré dans une Discovery, et c'est le format qui m'y a poussé.** Deux cellules
    sur six de la colonne « ce qu'on sait déjà » ne sont pas des faits mais des conclusions
    (« répond à la testabilité, *pas* à l'itération » ; « contourne `WorkflowDraft`, *donc* les
    invariants »). Une colonne de commentaire par piste appelle l'argument, et l'argument appelle
    la conclusion. **La faute est arrivée avant la règle qui l'interdit** — mode de récolte
    nominal. Un futur skill `discovery` doit proscrire le tableau commenté, ou borner la colonne
    à des faits vérifiables. — *étape 1*
14. **L'artefact s'adressait au dépôt, pas à son lecteur.** Quatre des sept commentaires de
    relecture disaient la même chose sous quatre angles : renvois en chemins de fichiers
    (`trajectoire.md §Plus loin`, ni cliquable ni vérifiable depuis une carte), numérotation
    interne périssable (« ce que débloque `2·2c` »), section *Renvois* dont l'intérêt échappe,
    et **méta-commentaire de méthode** (« Ouverture, pas un choix » explique le gabarit au lieu
    de traiter le sujet). Un artefact vit là où il est lu. — *étapes 1 et 2*
15. **Mon auto-critique du pré-arbitrage était incomplète, et biaisée dans un sens.** J'avais
    relevé deux cellules défavorables à leur piste ; le relecteur en a trouvé une **favorable**
    (« le motif de l'écart était l'absence d'urgence, et le contexte a changé »). Départager,
    c'est plaider dans les deux sens — chercher seulement les arguments *contre* laisse passer
    la moitié des fautes. — *étape 1*
16. **Un déversoir vaut mieux qu'une suppression.** Le relecteur a proposé d'ouvrir le brouillon
    de la `Spec` en parallèle pour y déplacer la matière d'arbitrage exilée, au lieu de la
    couper. L'information avait de la valeur, elle était au mauvais endroit — et sans exutoire,
    la règle « ne pas arbitrer en Discovery » pousse à détruire ce qu'on vient de comprendre.
    — *étapes 1 et 2*

## 2026-07-30 — Reprise de la Discovery, et **premier tour réel du cycle de revue**

Le tour de revue a tourné en `claude -p` headless, sur le vrai Linear, avec le skill découvert par
sa seule description. C'est la première exécution du dispositif de `D-047`.

17. **La friction 9 s'est répétée à quatre jours d'écart, à l'identique.** J'ai conclu de l'absence
    du champ `parent` dans `list_project_labels` que les étiquettes de projet n'étaient pas
    groupées — exactement le motif déjà consigné, sur exactement le même sujet, corrigé par la même
    personne. Ce qui l'a tranchée : **provoquer l'erreur** plutôt que lire une absence (poser deux
    étiquettes du groupe rend un `400` qui nomme le conflit). **Une friction consignée ne protège
    que ceux qui relisent le journal** — et rien, aujourd'hui, ne le recharge au bon moment. C'est
    le mode d'échec de `task-master.md` qui se matérialise sur notre propre journal. — *transverse*
18. **Le vocabulaire d'états existait à moitié, et du mauvais côté.** Linear sépare étiquettes
    d'issue et de projet ; les six avaient été créées côté issue, alors qu'une feature **est** un
    projet. Le seul niveau qui porte ses états uniquement par étiquette était le seul à ne pas les
    avoir — et `D-047` les déclarait créées. Un « fait » déclaré sans avoir été vu. — *transverse*
19. **Trois gestes d'administration hors de portée de l'API**, découverts en trois heures : créer
    une étiquette de projet, renommer une étiquette, supprimer la racine d'un fil ancré. Le motif
    se répète assez pour être nommé — **l'API Linear couvre le travail courant, pas
    l'administration de l'espace**. Tout dispositif supposant un agent capable de préparer son
    propre tableau se trompe. — *transverse*
20. **Un skill de revue calqué sur son frère hérite de son geste mort.** `revue-discovery`, écrit
    d'après `revue-spec`, prescrivait de poser les remarques *sur le document* — ce que `D-045` a
    tué. Réinjecté à la main. Les quatre skills de revue restant à reprendre partiront du même faux
    départ si on les calque : leur squelette est bon, leur geste central ne l'est plus. — *étape 3*
21. **La revue a attrapé une faute que j'avais défendue à voix haute.** J'avais présenté « cette
    direction atteint **donc** la composition et l'observation, mais pas le déclenchement » comme
    un fait et non un verdict — en ayant écrit, quelques heures plus tôt et de ma main, que ce
    débordement passe « déguisé en constat ». **Connaître la règle ne protège pas de l'enfreindre ;
    un relecteur tiers, si.** Première mesure locale du gain d'une session neuve (`D-039`).
    — *étape 1*
22. **La CLI ne résout pas le `slugId` d'un document**, or c'est ce qu'une URL Linear donne.
    `cursus linear comment list 0a6d59f7b60a` échoue ; il faut le titre exact. L'agent a contourné
    seul, mais c'est le premier identifiant qu'il avait sous la main. — *outillage*
23. **Aucun geste d'étiquette dans la CLI.** L'agent a dû poser `Rework Needed` par le MCP, faute
    d'équivalent. Conséquence : **un agent sans MCP Linear ne peut pas fermer le cycle**, alors que
    la CLI existe précisément pour ne pas en dépendre. — *outillage*
24. **Une remarque de complétude n'a aucun passage à citer.** Le geste de `D-046` suppose un
    passage existant ; signaler un **manque** a conduit à citer le titre de section. Le repère
    fonctionne, l'ancrage est vide de contenu. Une absence ne se cite pas. — *outillage*
25. **Les remarques d'un agent sont signées du porteur de la clé**, donc indiscernables de celles
    de l'humain sur la carte. `createAsUser` reste non éprouvé, et le cycle suppose de savoir qui
    parle. — *outillage*
26. **Premier coût mesuré d'un tour de revue** : 8 min 06 s, 21 tours, 2,99 $, trois sous-agents en
    parallèle, 7 remarques rendues. Aucune permission refusée, aucune erreur d'outil, aucune
    tentative d'écrire dans l'artefact. — *transverse*
27. **La `Spec` était attachée au projet avant la revue censée la gater** — signalé hors mandat par
    le relecteur. C'est le déversoir de la friction 16, et il a raison de tiquer : un lecteur du
    tracker n'a aucun moyen de distinguer un brouillon-déversoir d'une spec commencée. — *étape 2*

### Tour 2 de la même revue — même jour

28. **Interdire un outil n'est pas interdire un geste.** `Write` et `Edit` avaient été retirés de
    l'allowlist pour tenir la clause *« ne rien réécrire »* par le harnais plutôt que par
    l'obéissance (`revue` §6). Mais la clause visait **l'artefact**, et l'allowlist a bloqué **toute
    écriture de fichier** — y compris six brouillons de corps de commentaire dans `/tmp`, hors
    dépôt. Six refus de permission, contournés seuls par des heredocs. Le harnais a tenu la clause,
    mais **plus large qu'elle**. Une interdiction se cible sur une cible, pas sur un verbe.
    — *transverse*
29. **Une reprise peut être complaisante de bonne foi.** Le binôme avait soldé sept remarques en
    retirant des **mots**, croyant retirer une **orientation** : *« l'engagement est tenu en volume,
    il ne l'est pas en direction »*. Les six remarques du tour suivant rouvrent toutes des points
    déjà soldés. La complaisance ne s'est pas manifestée comme un renoncement — elle s'est
    manifestée comme un travail réel, mais sur la mauvaise grandeur. **C'est ce qui la rend
    invisible à celui qui la commet**, et c'est exactement le cas que le cycle court parie de
    rattraper (`cycle-feature.md` §3). Le pari tient. — *étape 1*
30. **Un coût nul est invisible comme coût.** *« Rien n'est à construire dans Cursus pour cette
    voie »* est une estimation de coût — ce que `dod/feature/discovery.md` §2 interdit — mais elle
    ne ressemble pas à un chiffre, donc elle passe là où « trois semaines » serait vu. Le zéro est
    l'angle mort de la clause. — *étape 1*
31. **Le dossier `rex/` ne pollue pas les tours suivants** — vérifié, pas supposé. La fiche du tour
    1 et ce journal étaient dans le dépôt et racontaient ce que le tour 1 avait trouvé ; le
    relecteur du tour 2 ne les a **pas ouverts**. Ses constats de récidive viennent du **fil
    Linear**, la bonne source. ⚠️ Le doute était légitime et le restera : à revérifier à chaque
    fiche, parce que le dossier grossit et qu'un relecteur qui explore finira par tomber dessus.
    — *transverse*

## 2026-07-31 — Spec de « un agent pilote Cursus », premier tour de `revue-spec`

32. **Le cycle complet de `Spec` prescrit deux skills qui n'existent pas.** `cycle-feature.md` §4
    renvoie à `correction` (temps ③) et `verification` (temps ④), tous deux marqués *à écrire*. Le
    tour a donc joué `correction` **à la main** — onze soldes rédigés sans protocole, sans clause
    disant ce qui distingue une reprise d'un refus motivé. Ça a marché parce que l'auteur avait le
    fil ; un agent sans la conversation n'aurait pas su quoi écrire dans les fils. — *étape 2*
33. **Le geste central de `revue-spec` est *absent*, pas *mort*** — et la nuance change le remède.
    `D-045` a tué le geste des quatre skills de revue (poser sur le document, impossible par l'API) ;
    ici, ni `revue-spec` ni le primitif `revue` §8 ne disent **comment** poser une remarque. Le
    relecteur a trouvé `cursus linear comment add` **seul**, en lisant `cycle-feature.md`. Le tour a
    donc réussi **par exploration, pas par conception** : le même skill lancé dans un dépôt où cette
    commande n'est pas documentée à côté échouerait sans rien signaler. Un geste absent est plus
    silencieux qu'un geste mort — le mort lève une erreur, l'absent produit un agent débrouillard.
    — *outillage*
34. **La durée d'un sous-agent n'est pas celle qu'on lit.** La notification de fin a été émise deux
    fois pour la même exécution, la seconde portant 3 807 s là où le travail réel en avait pris 727 —
    l'écart étant la boucle d'attente, pas de la revue. Une fiche `rex/` qui prend le mauvais chiffre
    rend deux tours incomparables, ce qui est sa seule raison d'être. **Prendre la durée de la
    première notification.** — *outillage*

## 2026-07-31 — Spec de « un agent pilote Cursus », second tour de `revue-spec`

35. **Un schéma faux est moins détectable qu'une prose fausse — et `D-049` vient d'en rendre un
    obligatoire.** Le §8.1 de la spec tranchait « on priorise le projet dédié » ; le schéma §8.3,
    deux paragraphes plus bas, logeait l'hôte et l'adaptateur dans `Cursus.App`. Contradiction
    interne, dans le même artefact, à deux paragraphes d'écart. **Aucun des deux axes ne l'a
    relevée** — et le relecteur a fait pire que la manquer : sa remarque hors mandat écrit
    « l'hébergement dans `Cursus.App` est instruit et mesuré », c'est-à-dire qu'il a **adopté la
    version du schéma** contre le texte qui la contredit. C'est l'humain qui l'a vue, à l'œil nu,
    en relisant. Un bloc `mermaid` se lit comme une conclusion, pas comme une affirmation à
    confronter au texte : il *illustre*, donc il échappe à la lecture chicanière. Le mode de
    défaillance qu'introduit `D-049` n'a aujourd'hui **aucun garde-fou** dans `revue`. — *outillage*
36. **Le tour ② a subsumé le temps ④, et ça a tenu.** La table prescrivait `verification` (skill
    inexistant) sur `Rework Done` ; on a rejoué une revue complète, parce que le §8 était du contenu
    neuf jamais relu et que la DoD avait gagné trois cases dans l'intervalle. Résultat : **aucune
    des onze remarques du tour 1 n'a été rouverte** — la reprise tenait — et douze remarques neuves
    sont sorties, dont deux violations dures sur des passages **que le tour 1 avait lus sans rien y
    trouver**. ⚠️ Ce que ça n'établit pas : que la vérification est inutile. Une revue relit
    l'artefact, elle ne relit pas les fils ; les deux ont coïncidé ici parce que la reprise avait
    réécrit tout ce que les remarques visaient. — *étape 2*
37. **Le prompt allégé de ses deux béquilles n'a rien coûté.** Le tour 1 rappelait à la main la
    session neuve et l'interdit de déplacer la carte, ce qui rendait toute comparaison impossible
    (fiche du tour 1, §1). Retirées : le relecteur n'a pas reçu le fil de rédaction, et la colonne
    `Spec` est inchangée après coup — les deux clauses tiennent **sans rappel**. Le geste de pose
    avait lui aussi été écrit dans `revue` §6 avant le tour, et les douze remarques sont posées sur
    la carte avec leur repère. ⚠️ Ce dernier point n'est pas une mesure : on ne saura pas s'il l'a
    lu là ou retrouvé ailleurs, faute d'avoir rejoué le tour à l'identique. — *outillage*

## 2026-07-30 — troisième tour de `revue-discovery` (consigné en retard, le 2026-07-31)

38. **Un verdict `Done` avale ses jugements.** Le tour 3 a énoncé quatre observations non bloquantes
    qui n'existent **nulle part** sur la carte : les déposer aurait rouvert `open`, et un `open` non
    nul interdit le `Done`. Le relecteur a donc arbitré entre *dire ce qu'il a vu* et *laisser passer
    la carte* — et il a choisi de laisser passer, ce qui est le bon choix au regard de la porte
    mécanique, et une perte sèche au regard de ce qu'on cherche. Le geste **« poser une observation
    sans rouvrir la porte »** n'existe ni dans la CLI ni dans le cycle. ⚠️ Le motif est structurel,
    pas anecdotique : toute porte binaire calculée sur un compteur pousse à ne pas écrire ce qui
    ferait monter le compteur. — *outillage*
39. **Une clause qui vit dans le skill au lieu de la DoD n'est opposable par personne.** *« Ce qu'on
    en sait factuellement »* est prescrit par `discovery` §3 ; les trois axes de `revue-discovery`
    sont adossés à `dod/feature/discovery.md` §1/§2/§5. **Aucun axe ne porte la clause**, donc
    aucune revue ne peut voir une section vidée de sa substance — et c'est ce qui s'est produit : les
    cinq pistes ont été réduites à des définitions nues pour satisfaire l'axe *« aucun arbitrage n'a
    été rendu »*, et trois tours ont validé sans pouvoir voir le risque inverse. ⚠️ **Une revue qui
    ne sait chicaner que dans un sens produit mécaniquement la dérive opposée, et elle la certifie.**
    Le remède est dans le référentiel, pas dans le relecteur. — *étape 1*

## 2026-07-31 — écriture d'une fiche `rex/` en retard

40. **Une fiche non écrite le jour même perd ce qu'aucune trace ne rattrape.** La fiche du tour 3
    a été écrite le lendemain : la commande verbatim, le nombre de tours d'outils, les tokens et les
    sous-agents sont **définitivement perdus** — le `README.md` du dossier dit pourtant que sans la
    commande, deux tours ne se comparent pas. Ce qui a survécu est ce qui vivait ailleurs : les
    chiffres de coût et de durée (mémoire de séance), les fils Linear, l'étiquette posée. ⚠️ Le
    corollaire vaut pour toutes les fiches à venir : **ce qui n'est pas dans le dépôt ou dans Linear
    à la fin de la séance n'existera plus le lendemain.** — *transverse*
41. **Changer un cycle laisse les cartes en cours dans un état qui n'existe plus.** `D-050` a
    supprimé les temps ③ et ④ de `Spec` — donc l'étiquette `Rework Done` du vocabulaire de cette
    colonne. Or une carte la portait **au moment même où la décision était prise** : elle venait
    d'être posée une heure plus tôt, à la fin d'une reprise. Personne ne l'a vue jusqu'à ce que
    l'utilisateur demande *« on ne vient pas de passer une revue à zéro ? »* — question dont la
    réponse était non, et qui a fait apparaître au passage que l'état affiché était devenu
    illégal. ⚠️ Aucun mécanisme ne rattrape ça : les étiquettes sont posées à la main par l'agent
    qui finit son temps, et une décision de méthode ne repasse pas sur les cartes en vol. À vérifier
    **par principe** après toute décision qui touche un vocabulaire d'états — la question n'est pas
    « le document est-il à jour » mais « que portent les cartes en cours ». — *transverse*

## 2026-07-31 — troisième tour de `revue-spec`

42. **Le skill prescrit une étiquette que le cycle ne lui autorise plus.** `revue-spec` §4 dit
    « Poser `Done` si aucune violation dure ne reste sans réponse […] sinon `Rework Needed` ».
    `cycle-feature.md` §4, depuis `D-050`, donne au relecteur de `Spec` deux sorties et pas
    celles-là : `Rework Needed`, ou **`Human Review Requested` si aucune remarque** — `Done` n'est
    posable que par l'humain, une ligne plus bas. Le tour ne l'a pas départagé, puisque seize
    remarques imposaient `Rework Needed` des deux côtés. ⚠️ C'est précisément le cas *sans remarque*
    qui diverge, et c'est le seul où l'écart compte : le skill ferait sauter le passage humain que
    le cycle vient d'y placer. Deuxième occurrence du motif de l'entrée 41 — une décision de méthode
    ne repasse pas sur les fichiers qui en dépendent. — *outillage*
43. **La mémoire automatique de la session dément la clause de session neuve.** `revue-spec` §1
    exige un contexte qui n'a « vu ni le prompt ni la conversation » qui a produit la spec, et
    `D-039` en fait la condition de valeur de la relecture. Or la mémoire de projet est chargée
    **avant** toute lecture, et elle résume l'artefact par ses conclusions : le motif JetBrains qui a
    fait basculer l'hébergement, les arbitrages du second tour, et l'avertissement que deux croyances
    y sont démenties par le code. Le relecteur a pu s'abstenir d'ouvrir les fiches détaillées ; il ne
    pouvait pas ne pas lire l'index. ⚠️ La neutralité que `D-039` cherche ne s'obtient pas par une
    clause dans un skill, mais par **la façon dont la session est construite** — une clause ne peut
    rien contre un canal qu'elle ne connaît pas. — *outillage*
44. **Deux axes qui butent sur le même passage n'ont pas de règle.** `revue` §2 interdit de fondre
    les axes ; §6 demande une remarque par constat retenu. Quand les deux axes citent le **même**
    extrait — deux fois sur vingt constats ici, sur le renvoi mort du §1 et sur la clause de recette
    sans incrément — les deux clauses tirent en sens contraire : poser deux remarques dédouble la
    solde d'un même défaut, n'en poser qu'une fusionne ce que §2 sépare. Arbitré à la main, en
    fusionnant, sans que rien l'autorise. Le motif est structurel : plus les axes sont bons, plus ils
    se recouvrent. — *outillage*
45. **La pièce la plus contestable est la moins citable.** Cinq des seize remarques naissent des
    blocs `mermaid`, et **aucune n'est ancrée dessus** : `cursus linear comment add --quote` exige un
    passage présent une fois et une seule, et une ligne de nœud (`SER --> CAT`, un `style`, un
    libellé coupé par un `\n`) est un mauvais candidat. Toutes ont été ancrées sur la prose voisine,
    ce qui déplace le repère d'un ou deux paragraphes. ⚠️ Ce n'est pas une mesure — l'ancrage sur le
    bloc n'a pas été tenté, il a été évité par précaution. Mais depuis `D-049` le schéma est une
    pièce **obligatoire** de toute spec, et l'entrée 35 en a fait la pièce la plus dangereuse :
    l'outil de revue ne sait pas viser ce que la revue doit le plus regarder. — *outillage*
46. **Une section « ce que je ne décide pas » gonfle à chaque reprise, jusqu'à contredire son
    titre.** Le §8.6 de la spec s'appelait *« Ce que ce plan laisse aux plans d'archi »* et
    contenait, au bout de deux cycles de reprise, l'intention de maille, son critère de coupe et
    l'arbitrage sur le fondateur — c'est-à-dire surtout des **décisions**. Le mécanisme est
    mécanique et sans malveillance : chaque remarque soldée dépose sa réserve dans la section
    prévue pour les réserves, et personne ne recule d'un pas pour relire le titre. ⚠️ Le défaut de
    conception est en amont : **la liste de ce qu'on n'a pas tranché est infinie**, donc toute
    tentative de l'écrire est arbitraire — et elle faisait doublon avec le registre *question
    ouverte* du §6, au point que l'un disait « aucune » pendant que l'autre en listait six. Relevé
    par l'utilisateur, à la lecture, après trois revues qui ne l'avaient pas vu. La règle qui en
    sort est dans `spec` §5 : **n'écrire que ce qui est décidé, le reste est ouvert par défaut.**
    — *étape 2*
47. **Rien ne tient la forme d'un artefact d'un tour à l'autre, et l'utilisateur l'a nommé avant que
    ça coûte.** Six fiches `rex/` se tiennent parce que leur `README.md` fixe sept rubriques ; en
    face, une spec n'a que **des questions** (`tickets.md` §2.2) et **des cases** (`dod/`) — rien qui
    dise à quoi ressemble le document. D'où trois dérives observées le même jour sur le même
    artefact : une section « ce que je ne décide pas » qui gonfle jusqu'à contredire son titre (46),
    un registre en double entre §6 et §8.6 qui se contredisaient, et un décompte corrigé en une
    erreur plus précise que le flou d'origine. ⚠️ Ce n'est **pas** un appel à normaliser tout de
    suite : un gabarit écrit avant d'avoir vu diverger fige la mauvaise forme, et `D-039` interdit
    précisément d'écrire l'exécutant d'avance. Ce qu'il faut, c'est **attendre le second artefact de
    chaque espèce** et écrire le gabarit sur ce que les deux ont en commun. Piste ouverte, pas
    tâche. — *transverse*

## 2026-07-31 — quatrième tour de `revue-spec`

48. **Corriger une instance de revue laisse le primitif porter l'ancienne clause, et les deux se
    lisent dans la même exécution.** L'entrée 42 a été soldée sur `revue-spec` §4, qui prescrit
    désormais `Human Review Requested` ; mais `revue` §8 dit toujours « `Done` ou `Rework Needed` ».
    Le relecteur charge les deux fichiers dans le même tour et n'a rien pour les départager. Sans
    effet ici — seize remarques imposaient `Rework Needed` des deux côtés —, donc l'écart est
    **intact, simplement déplacé d'un fichier à l'autre**, et il mordra toujours dans le seul cas où
    il compte : un tour sans aucune remarque. ⚠️ Le motif est celui des entrées 41 et 42 à sa
    troisième occurrence, mais avec une aggravation propre : la correction *a eu lieu*, elle a même
    été écrite en connaissance du cycle, et elle n'a pas suivi la **relation instance → primitif**
    que l'architecture des skills de revue institue. Ce n'est plus « une décision ne repasse pas sur
    ses dépendants », c'est « une correction ne repasse pas sur ce dont elle hérite ». — *transverse*
49. **L'attente d'un sous-agent coûte des appels d'outils, et ne produit rien.** Douze des
    quarante-six appels du relecteur n'ont servi qu'à laisser passer le temps pendant que ses deux
    axes travaillaient, dont **dix rendus sans effet** parce qu'ils s'exécutaient en arrière-plan et
    rendaient la main aussitôt. Le coût est proportionnel à la durée de l'axe le plus lent — ici
    553 s — et il croîtra avec le nombre d'axes. ⚠️ Ce n'est pas qu'une dépense : les appels
    d'outils sont l'une des trois mesures que `rex/` compare d'un tour à l'autre, et douze appels
    d'attente pure y entrent au même titre que trente-quatre appels de travail. La ligne du tableau
    ne mesure plus ce qu'elle prétend mesurer tant qu'on ne les sépare pas. — *outillage*
50. **La clause de session neuve n'a pas de doctrine sur les faits d'état.** `revue-spec` §1 exige
    une session qui ne porte que l'artefact ; mais les trois cases de §2 de la DoD portent sur
    l'**issue des tours précédents** — les remarques posées, leur solde — et le tour 3 avait constaté
    qu'aucune n'est opposable depuis l'artefact seul. Le tour 4 les a instruites en fournissant aux
    axes trois faits chiffrés (39 remarques, toutes soldées, accord humain en aval), et **deux cases
    sur trois sont passées de « non opposable » à « tenue »**. ⚠️ Le geste a donc marché, et c'est ce
    qui le rend gênant : c'est un **résumé de l'issue des tours précédents** injecté dans une session
    que la clause veut vierge, l'orchestrateur l'a choisi seul, et rien ne règle ni sa forme ni sa
    dose. Entre « trois faits chiffrés » et « voici ce que les relecteurs précédents ont trouvé », il
    n'y a pour l'instant que le jugement de celui qui rédige le mandat. — *étape 3*

## 2026-08-01 — passe globale sur la spec, hors tour de revue

51. **Un plan de document calqué sur une grille de complétude produit mécaniquement la redite.** La
    spec suivait les huit questions de `tickets.md` §2.2, section par section, et mesurait
    **47 000 caractères** pour 7 356 mots. Le trou du partage de connexion y apparaissait **dix-huit
    fois**, *arrêter un run* **quatorze**, le projet dédié **quatorze**. ⚠️ **Chacune de ces
    occurrences est défendable prise isolément** : le même fait est légitimement pertinent sous la
    recette, sous le socle, sous les registres et sous la conception. Les huit questions sont des
    **angles**, pas des compartiments — et rien dans le référentiel ne dit **où un fait s'établit** et
    où il se contente d'être mentionné. Quatre reprises ont amplifié le phénomène ; elles ne l'ont pas
    créé. Remède appliqué : plan délié de la grille (une table de correspondance en tête la préserve
    pour la revue), ordre *décider → construire → vérifier*, et ce qui a servi à établir les décisions
    — options comparées, écarts, mesures — relégué en annexe. −19 % de caractères. — *étape 2*
52. **Une spec est périssable, et ce n'est pas un ADR — le principe manquait, et il change l'écriture.**
    Énoncé par l'utilisateur pendant la passe : *« un document de spec est par définition périssable,
    il est valable quand on construit ce qu'il décrit et après il périme ; on travaille en flux tendu
    en mode kanban »*. Ce que ça autorise et qui n'était écrit nulle part : **la spec cesse de se
    prémunir contre son propre vieillissement**. Elle n'a pas à recopier le dépôt pour survivre à ses
    changements — l'état du code se lit dans le dépôt, et ce qui doit durer se déverse en `D-NNN`,
    comme `tickets.md` §2.2 q.1 le prévoyait déjà sans en tirer la conséquence. ⚠️ **Le gain est
    mesurable en défauts évités** : le tour 3 avait démenti **trois assertions sur trois** par le
    code, dont « huit types publics » là où le dossier en porte douze. Une spec qui n'énumère pas le
    dépôt n'a plus de faits à maintenir contre lui, et cette classe entière de remarque disparaît.
    — *transverse*
53. **Le gras et le ⚠️ cessent de signaler quand ils saturent, et rien ne les budgète.** Mesuré avant
    la passe : **268 passages en gras**, soit un tous les 25 mots, et **28 ⚠️**, un tous les 260 mots
    — avec une densité de gras **identique dans les neuf sections** (entre 1/21 et 1/31). Un signal
    uniformément réparti ne hiérarchise plus rien. ⚠️ **Deux usages étaient confondus** : le
    *gras-chapeau*, qui ouvre un paragraphe et forme le squelette lisible du document, et le
    *gras-emphase* sur des mots isolés — « parité », « annexe », « arbitre » —, qui est du
    soulignement. Seul le second sature. Après retrait de **89** emphases courtes : un gras tous les
    43 mots. ⚠️ **L'objectif annoncé — diviser par trois — n'est pas atteint**, et l'écart est à
    consigner plutôt qu'arrondi : 141 gras subsistent, dont beaucoup de chapeaux légitimes.
    — *transverse*

## 2026-08-01 — cinquième tour de `revue-spec`

54. **Un référentiel qui bouge périme silencieusement le skill qui le cite.** `revue-spec` §2
    prescrit d'instruire « les **douze** cases de §1 » ; `D-054` en a porté le nombre à **dix-sept**,
    et `D-053` a renommé « plan d'implémentation » en *plan d'architecture*, nom que la même clause
    emploie encore. Un relecteur qui suivrait le skill à la lettre s'arrêterait à douze et manquerait
    les cinq neuves — **c'est le mandat de la session appelante qui a rattrapé l'écart**, pas le
    protocole, et il ne le fera pas toujours. ⚠️ Le motif ressemble aux entrées 41, 42 et 48 mais il
    est **inverse**, et c'est ce qui le rend neuf : là, une correction ne redescendait pas sur ses
    dépendants ; ici, un **référentiel** change sans que ce qui le **cite** l'apprenne. Le lien n'est
    porté par rien — ni renvoi calculé, ni test, ni case de DoD. Un skill qui compte les cases d'un
    document extérieur porte une copie de ce document, et une copie diverge. — *transverse*
55. **La citation d'ancrage bute sur les marques d'emphase, et la règle écrite ne le dit pas.**
    `revue` §6 documente une seule tolérance — « recopier un passage écrase ses retours à la ligne,
    et c'est prévu ». Rien sur le gras. Sur douze poses, un passage recopié **sans** ses astérisques
    a été **refusé** une fois et **accepté** une autre, selon que la citation commençait ou non sur
    la marque ouvrante. Résolu en raccourcissant la citation, donc sans coût mesurable — mais le
    comportement n'est ni documenté ni **prévisible depuis le texte rendu**, qui est tout ce que le
    relecteur voit. ⚠️ À rapprocher de l'entrée 45, qui reçoit au même tour son premier
    contre-exemple : une remarque a pu être ancrée sur `UI --> QRY`, une ligne interne d'un bloc
    `mermaid`. La contrainte réelle n'est donc pas « une figure ne se cite pas » mais « une citation
    doit être **unique** dans le document », ce que les identifiants de nœud satisfont souvent.
    — *outillage*

## 2026-08-01 — reprise du tour 5

56. **Un ADR append-only qui cite une section d'un document périssable produit un renvoi mort
    incorrigible.** En soldant la remarque 12 — l'annexe C renvoyait à un « §5 » que la
    renumérotation avait supprimé —, le même résidu est apparu **dans `D-052`**, qui cite « le §5 de
    la spec » et « le schéma §8.3 de la spec ». Côté spec, un `patch` suffit ; côté `decisions.md`,
    **rien ne peut être fait** : le fichier est append-only, et une entrée périmée ne se réécrit
    jamais. ⚠️ C'est **exactement le motif de la règle « ne jamais citer un hash de commit »**
    (`CLAUDE.md`, branches), transposé d'un identifiant volatil à une **section d'un document que la
    méthode déclare périssable** (journal 52). Le hash meurt au rebase, la section meurt à la
    restructuration ; dans les deux cas, l'append-only rend la mort définitive. Ce que la règle
    existante ne couvre pas : elle vise les hashes, elle ne dit rien des renvois de section — et le
    seul document que `decisions.md` cite ainsi est justement celui qu'on restructure le plus.
    ⚠️ **Deux occurrences dans la même entrée `D-052`**, donc le seuil de `D-039` est atteint d'un
    coup, mais sur un seul artefact : ce n'est pas encore une tendance. Piste sans être une décision :
    citer une spec **par ce qu'elle décide**, jamais par le numéro de la section qui le décide.
    — *transverse*
57. **Retirer une section emporte ce qu'elle contenait d'une autre nature, et personne ne le voit
    passer.** Le §8.6 de la spec MCP — l'intention de maille — a été retiré en entier en reprise du
    quatrième tour, sur un motif juste : *une spec n'a pas à décrire les lots*. Mais il portait aussi
    un **arbitrage rendu par l'utilisateur au troisième tour** — *le fondateur absorbe la descente du
    socle, pas d'incrément socle séparé* —, qui n'est pas un lot mais une **règle d'atterrissage**.
    Constaté un tour plus tard, en soldant une remarque qui redemandait exactement cette règle sans
    savoir qu'elle avait existé : elle ne vivait plus nulle part, ni dans la spec, ni dans
    `decisions.md`. ⚠️ **Ce qui rend le cas instructif** : le retrait a été discuté, motivé, et chiffré
    (4 remarques du tour 4 sur 16 visaient cette section) — la décision était bonne, c'est
    l'**exécution** qui a perdu quelque chose, et aucun des quatre documents de méthode n'a de geste
    pour ça. ⚠️ **Le coût réel a été payé par la revue** : c'est elle qui a rattrapé, un tour plus
    tard, et au prix d'une remarque. Piste sans être une décision : avant de retirer une section,
    demander ce qu'elle porte **qui ne relève pas du motif du retrait** — et le déverser en `D-NNN`
    plutôt que dans la section voisine, puisque c'est précisément ce que le journal ADR existe pour
    garder. — *transverse*

## 2026-08-01 — sixième tour de `revue-spec`

58. **Un constat faux passe le garde-fou des deux citations, parce que la contradiction interne
    n'est pas un fait citable mais une inférence.** `revue` §3 exige référentiel **et** extrait
    côte à côte, et le dit explicitement « préféré ici à la vérification empirique » — c'est un
    arbitrage assumé, pas un oubli. Ce tour en mesure le prix pour la première fois : **2 constats
    sur 21 écartés pour fausseté factuelle**, et dans les deux cas **les deux citations étaient
    exactes**. L'un opposait « trois manques du noyau » à « deux des trois manques » ; l'autre
    opposait la clause « deux worktrees » à la mesure d'annexe C, alors qu'`architecture.md`
    enregistre le provisionnement en HEAD détaché, avec son test, précisément pour ce cas.
    ⚠️ **Le motif est structurel, et il tient au §3 lui-même** : quand l'artefact est son propre
    référentiel — le cas que la clause autorise explicitement —, les deux pièces attestent chacune
    l'existence d'un passage, jamais leur **incompatibilité**, qui reste une lecture. Deux citations
    exactes ne font pas une contradiction vraie. Le garde-fou attrape l'impression sans source ; il
    ne peut rien contre l'inférence bien sourcée. ⚠️ **Ce qui a rattrapé, c'est six confrontations au
    dépôt faites hors protocole**, par initiative de l'orchestrateur — quatre assertions ont tenu,
    deux sont tombées. Aucune clause ne les prescrit, et le tour suivant peut ne pas les faire.
    Piste sans être une décision : demander la vérification empirique **pour les seules
    contradictions internes**, là où la clause l'écarte aujourd'hui pour toutes.
    — *transverse*
