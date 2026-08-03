# Journal des frictions — exécution du flux sans skill

> Tenu selon `D-039` : on exécute la méthode **sans** skill, on note chaque friction au fil de
> l'eau, et le journal écrit le skill — après deux ou trois passages, jamais après un seul.
> Une ligne brute par occurrence. Ce fichier n'est pas de la méthode, c'est de la matière.
>
> **Ce fichier ne dit pas si ça progresse** — il n'a aucune structure qui permette de comparer deux
> passages. C'est [`rex/`](rex/README.md) qui le fait : une fiche par exécution, rubriques fixes.
> Une fiche renvoie ici par numéro d'entrée, et ne recopie jamais.
>
> ## ⚖️ Le marqueur des décisions en attente
>
> Une entrée qui **attend une décision** porte la balance et la mention en capitales juste après son
> numéro. Le balayage **est** l'index des arbitrages ouverts — il reste dérivé du journal, donc il ne
> peut pas diverger de lui, là où un registre séparé le pourrait : une copie d'un référentiel diverge
> en silence (entrée 54).
>
> ```bash
> grep -n "^[0-9]\+\. ⚖️" docs/methode/journal-frictions.md
> ```
>
> L'ancre `^<numéro>.` est ce qui rend l'index exact : elle ne prend que les entrées, jamais les
> mentions du marqueur en prose — à commencer par ce préambule. Retirer le marqueur quand la
> décision est prise, en renvoyant à l'entrée `decisions.md` qui la porte.
>
> ⚠️ **Il ne vise qu'un cas : une décision dont les branches sont ouvertes.** Pas une tâche dont la
> réponse est connue et qui attend son tour (« à écrire dans tel skill »), pas une mesure à suivre,
> pas un constat. Un marqueur qui couvre les deux rendra vingt résultats dans un mois et redeviendra
> illisible — c'est exactement ce qui est arrivé à « question ouverte » dans `decisions.md`.
>
> ⚠️ **Une décision, un marqueur** — même quand elle a été rencontrée plusieurs fois. L'entrée
> marquée nomme alors ses occurrences antérieures ; celles-ci restent nues.
>
> **Convention posée le 2026-08-03.** Les entrées antérieures à la 62 n'ont pas été balayées : la
> plupart ont été tranchées depuis par une entrée de `decisions.md`, et les marquer après coup
> produirait des faux positifs que rien ne permet de vérifier.

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

## 2026-08-01 — relevé des référentiels, hors tour de revue

59. **Aucun skill de production ne cite la DoD contre laquelle son artefact sera jugé.** Mesuré sur
    les six skills concernés : `spec` **0**, `discovery` **0**, `plan-design` **0** ; `revue-spec`
    **4**, `revue-discovery` **5**. Le producteur reçoit `tickets.md` — *ce qu'il faut mettre
    dedans* — et jamais `dod/` — *ce qui sera opposé*. Les deux ne coïncident pas : les huit
    questions d'une spec ne disent rien des titres du gabarit, ni du coût par écart, ni des trois
    registres. ⚠️ **Le symptôme le plus net vit dans `revue-spec` §2** : l'exigence de cohérence
    figure ⇄ prose y est écrite pour le juge, et pour lui seul. On demande à l'auteur de passer un
    examen dont il n'a pas le programme. ⚠️ **Et l'écart n'est pas seulement d'accès mais de
    contenu** : sur les dix-neuf remarques du sixième tour, deux opposent une case de la DoD et sept
    une contradiction interne que la DoD ne mentionne nulle part — c'est `revue` §3 qui les rend
    opposables. Deux référentiels se partageaient l'exigence, et celui que l'auteur lit n'en portait
    qu'un dixième. Soldé le jour même par `D-060`, qui écarte l'issue inverse — assouplir la sortie
    du cycle. — *transverse*

## 2026-08-02 — septième tour de `revue-spec`

60. **Compter les remarques d'une carte en expose le contenu, et le protocole demande ce décompte
    sans dire quand.** La fiche d'un tour doit attester le mouvement de la carte — ici `99 → 111` —
    et le seul moyen de l'établir est de lister les fils, ce qui rend leur **corps** avec les
    compteurs. Le relecteur a donc lu la réponse d'un tour antérieur, c'est-à-dire une pièce du fil
    de production que `revue-spec` §1 lui interdit d'ouvrir. ⚠️ **Sans effet ce tour** : le décompte
    a été fait **après** la production et la pose des douze constats. C'est le placement qui a sauvé,
    et rien ne le prescrit — un relecteur qui commencerait par attester l'état de la carte, geste
    parfaitement naturel, se contaminerait avant d'avoir lu l'artefact. ⚠️ **La tension est entre
    deux exigences légitimes** : la fiche veut un chiffre vérifiable, la clause de session neuve veut
    un relecteur ignorant. Le remède tient probablement en une ligne — *n'attester l'état de la carte
    qu'une fois les constats posés* — mais il appartient au skill, pas au journal. Voisine de la
    friction 50, qui portait sur les faits d'état **fournis** ; celle-ci porte sur ceux que le
    relecteur va **chercher**. — *`revue-spec`*

61. **L'outil de pose des remarques exige d'être appelé depuis un projet Cursus, et rien ne le dit.**
    Le premier lot de trois remarques a échoué **en bloc**, le relecteur travaillant depuis le
    répertoire de ses fichiers de travail, hors dépôt. Coût réel : un appel. ⚠️ **Ce qui mérite
    d'être écrit n'est pas la panne mais sa forme** : l'échec est groupé et silencieux sur sa cause
    de chemin, alors même que le dispositif recommandé depuis le tour 4 — matérialiser l'artefact
    dans un fichier de travail hors dépôt — **conduit** à s'y placer. Deux consignes de la méthode
    se contrarient donc sans le savoir, et la seconde se paiera à chaque tour tant qu'elle n'est pas
    écrite quelque part. — *outillage*

62. **La friction 49 peut rester à zéro occurrence tout en ayant coûté un appel, et la fiche ne sait
    pas l'exprimer.** Les tours 5 et 6 avaient supprimé les appels d'attente en lançant les axes en
    synchrone ; ce tour les a lancés en arrière-plan. Les notifications ont bien évité toute attente
    active — **zéro appel d'attente**, la ligne du tableau est exacte — mais un outil de veille a été
    chargé « au cas où » et n'a jamais servi. ⚠️ **Le point n'est pas le coût, dérisoire, c'est que la
    métrique a un angle mort** : elle compte les appels d'attente et ignore les appels de
    **préparation à l'attente**, si bien que deux dispositifs de coût différent rendent le même
    chiffre. Une mesure qui ne distingue plus ce qu'elle existe pour distinguer cesse d'arbitrer
    entre synchrone et arrière-plan. — *méthode de mesure*

## 2026-08-02 — reprise du septième tour

63. **Un total agrégé écrit dans une spec est invérifiable, et il se propage.** Le §2.3 de la spec
    *Un agent pilote Cursus* annonçait « douze points de traversée » ventilés en cinq termes. Le
    chiffre avait été recompté **à la main, en ouvrant le dépôt**, et corrigé deux fois avant d'être
    écrit — il omettait un ViewModel entier (trois écritures), et sa règle de comptage variait d'un
    terme à l'autre. ⚠️ **Ce qui rend le défaut coûteux n'est pas l'erreur mais la forme** : un total
    est lu par le découpage, puis par chaque plan de design, et **aucun des deux ne rouvre le
    dépôt**. Une énumération, elle, se recontrôle ligne à ligne par n'importe quel lecteur, et une
    règle de comptage énoncée se conteste. Remède appliqué : la liste remplace le total, et la règle
    est écrite avant elle — « un point de traversée est un appel d'un ViewModel qui écrit », avec ce
    qu'elle exclut. ⚠️ **Le coût réel n'était pas le chiffre** : c'est la phrase qu'il portait — « un
    second incrément d'authoring n'a rien à recabler » — qui était fausse, et elle aurait dimensionné
    un incrément. **Un agrégat faux ne trompe pas sur sa valeur, il trompe sur la conclusion qu'on en
    tire.** — *écriture de spec*

**Seconde occurrence de la friction 61, et elle en élargit la portée** : ce n'est pas l'outil de
*pose* qui exige un projet Cursus, c'est **toute** la CLI — `linear doc show` a échoué de la même
façon, depuis le même répertoire de travail. La contrainte vaut donc pour lire comme pour écrire, et
elle se paiera à chaque tour tant que le dispositif recommandera de matérialiser l'artefact hors
dépôt.

64. **Interdire d'ouvrir la mémoire ne protège pas d'une présentation d'office.** Le mandat du
    huitième tour de `revue-spec` interdisait explicitement d'ouvrir tout fichier de mémoire de
    session, motif écrit : ils portaient les attentes du binôme sur ce tour-là. Le relecteur a tenu la
    consigne — **aucun fichier ouvert**, rien transmis aux deux axes — et un **extrait lui a
    néanmoins été présenté d'office** dans son contexte de session. ⚠️ **Ce qui est neuf, ce n'est pas
    l'occurrence, c'est ce qu'elle apprend** : l'injection n'est pas un geste du relecteur, donc
    aucune clause adressée au relecteur ne peut l'empêcher — et la clause de session neuve n'a
    **aucune doctrine** pour un contexte qu'on ne choisit pas. Sixième occurrence de la friction 43,
    et la première où la consigne était explicite. Le tour y survit par chance et non par
    construction : l'extrait portait bien les attentes, mais **pas** le nombre de cases, qui était la
    mesure du tour. — *revue de spec, huitième tour*

65. **« Zéro constat écarté » ne distingue pas un tour vérifié d'un tour qui ne l'est pas.** Les
    sixième et septième tours confrontaient au dépôt une demi-douzaine d'assertions de leurs axes
    avant de poser, et la ligne du tableau mesurait alors un **taux d'erreur**. Le huitième n'a
    confronté aucune assertion et rend la **même valeur**. ⚠️ **Le reproche ne vise pas la
    vérification manquante** — le protocole ne la prescrit nulle part, les deux tours l'avaient
    ajoutée d'eux-mêmes : il vise le fait que **la ligne ne dit pas si elle a eu lieu**. Deux
    dispositifs de fiabilité très différente rendent un chiffre identique, et rien dans le tableau ne
    permet de les départager. Même famille que la friction 62 — une métrique dont la valeur ne porte
    pas son mode d'obtention. Remède provisoire : l'écrire en prose à chaque tour, jusqu'à ce que la
    ligne soit refaite. — *revue de spec, huitième tour*

**Occurrence dérivée de la friction 44, et la première qui *infirme* un critère.** Le septième tour
avait dégagé un départage — citation identique → une remarque, citations distinctes → deux. Le
huitième a rencontré deux axes citant **le même passage au caractère près** et a posé **deux**
remarques, parce que les deux constats opposaient deux référentiels différents. Le critère est donc
faux tel qu'écrit : ce qui départage n'est pas la citation, c'est **le référentiel opposé**.

**Le listage des remarques d'une carte contredit ses propres compteurs.** L'outil rend `total: 111` et
`open: 0` en tête, puis un tableau de 222 entrées dont 111 portent `resolved: false` — les réponses en
fil, qui n'héritent pas de la résolution de leur racine. Un décompte naïf contredit frontalement le
fait d'état fourni par l'appelant, et il a fallu deux appels pour établir que les compteurs avaient
raison. ⚠️ **Le coût réel est ailleurs** : le relecteur a failli consigner une divergence sur l'état
de la carte, c'est-à-dire ouvrir un litige sur le seul fait qu'il ne peut pas établir lui-même.
Rattaché ici plutôt que numéroté à part : c'est un défaut d'ergonomie de la CLI, de la même famille
que la friction 61.

## 2026-08-02 — découpage de la feature *Un agent pilote Cursus*

66. **Un axe de revue a produit un « instrument réutilisable », et l'instrument n'existait plus.** Le
    huitième tour de `revue-spec` a rendu un découpage candidat complet — neuf incréments, leurs
    frontières, ce que chacun livre, leur recette, et les 31 lignes de l'inventaire réparties sans
    reste. La fiche de rex le célèbre nommément : *« c'est la première fois qu'un axe rend un
    instrument réutilisable plutôt qu'une liste de manques »*, et *« cette sortie survivra au tour qui
    l'a produite, ce qu'aucune remarque ne fait »*. Elle a survécu **comme affirmation** : la rubrique
    4 en atteste l'existence et en donne le résumé, mais **ne porte pas la pièce**. Au découpage réel,
    il a fallu la reconstruire entièrement depuis la spec. ⚠️ **Ce que le cas apprend n'est pas « il
    fallait le copier »** : c'est qu'une fiche de rex est un **jugement sur une exécution**, jamais un
    dépôt d'artefact — et qu'un dispositif qui produit une pièce durable doit nommer **où elle est
    déposée**, sinon la seule trace en est l'éloge. Le coût est mesurable : le travail a été fait
    deux fois, la seconde sans le contexte qui l'avait produit. Même famille que la friction 63 — ce
    qui n'est pas vérifiable se recopie plutôt qu'il ne se recontrôle, ici jusqu'à disparaître. —
    *découpage, premier usage réel du skill*

**Le skill `decoupage` a tenu son premier tour réel, et ses huit étapes ont servi.** Rien à redire sur
la mécanique — geler la recette, trancher verticalement, dimensionner, ordonner par les arêtes,
déposer le hors-périmètre en nommant les frères, faire trancher l'humain, publier. ⚠️ **Un manque
pourtant** : le skill ne dit nulle part **qui répond aux questions que la revue a portées au
découpage**. Cinq remarques de la spec avaient été soldées avec le motif *« ça se tranche en
coupant »*, et rien dans les huit étapes ne les réclame. Elles ont été posées à l'humain parce que le
binôme les avait en mémoire, pas parce que le dispositif les demandait — sur une session neuve, elles
auraient été tranchées en silence par celui qui coupe.

**L'arbitrage de granularité rendu par l'utilisateur vaut au-delà de ce découpage** : *« à chaque fois
que l'on peut avoir un petit incrément recettable, on le prend »* — le plus petit incrément traverse
le flux le plus facilement. Appliqué, il fait passer le découpage de neuf incréments à **dix-sept**,
la coupe se faisant entre une lecture et une écriture, ou entre deux objets que le rôle produit nomme
séparément. ⚠️ **Une seule exception a été assumée et dite** : trois lignes d'un même objet dont
aucune ne se distingue pour le rôle produit ne se coupent pas — la carte des connexions tracker.

## 2026-08-02 — relecture du découpage par six agents

67. **Un découpage relu tout de suite rend ce qu'une revue de spec ne rendait plus.** Six relecteurs
    lancés sur les dix-huit cartes fraîchement écrites ont trouvé, en un tour : une **régression réelle
    et invisible** — entre les deux premiers incréments, plus rien ne matérialisait la base d'un projet,
    si bien qu'un projet neuf se créait puis refusait de s'ouvrir —, une douzaine de **faits faux**
    écrits de mémoire (une coupe attribuée à un `.gitignore` qui ne la porte pas, un nom de projet qu'on
    disait vivre à deux endroits, une clôture référentielle qui refuse une arête qu'elle accepte en
    réalité, deux renvois ADR pour l'un l'autre), et deux cartes qui **revendiquaient chacune le même
    rang** dans une liste de trois. ⚠️ **Le contraste est le fait de méthode** : l'analyse de série
    venait d'établir que huit tours de revue de spec avaient rendu **une seule** remarque
    d'architecture et 68 remarques invisibles au code. Ici, un seul tour sur l'artefact **suivant**
    rend un défaut qui aurait cassé la fenêtre en production. La différence n'est pas le dispositif —
    c'est que le découpage se confronte au **code**, quand la spec ne se confrontait qu'à elle-même.
    — *découpage, relecture immédiate*

**Ce qui a rendu la régression trouvable, et il faut le nommer.** Aucun des cinq relecteurs de contenu
ne l'a vue : chacun jugeait son lot, et le défaut ne vit **dans aucune carte** — il vit dans
l'**intervalle** entre deux. C'est l'axe de cohérence d'ensemble, seul à lire les dix-huit et interdit
d'écriture, qui l'a produit. Un découpage n'est donc pas relisible carte par carte, et un dispositif
qui ne relit que des cartes conclura qu'il est conforme.

**Un renvoi périmé né le jour même.** Cinq hors-périmètre citaient un frère par un titre **presque**
exact — « Enregistrer un brouillon, et le conflit visible » pour « …, et **rendre** le conflit
visible ». Aucun renommage n'avait eu lieu : la dérive était là à la première écriture, parce que le
rédacteur citait de mémoire ce qu'il venait d'écrire. C'est l'argument de `tickets.md` §3 pour les
identifiants (« Pas ici, c'est `CUR-12` »), et il ne vaut pas seulement contre le temps.

**Une carte fantôme a survécu au découpage.** `CUR-32` — le serveur MCP en daemon sans fenêtre, avec sa
question de jeton « à trancher » — est restée dans le projet, priorisée, pendant que le découpage
créait la carte qui l'absorbe. La spec disait pourtant qu'elle « reste celle qui portera ce sujet ».
Le découpage a créé du neuf sans inventorier ce qui existait déjà dans le projet : rien dans le skill
`decoupage` ne le demande, et c'est le second manque du dispositif, avec celui des questions portées.

## 2026-08-02 — premier plan de design, et un ticket poussé

68. **Un skill a poussé une carte, parce que la règle n'avait que sa moitié amont.** `plan-design`
    §6 prescrivait « Place ensuite la carte en `Plan Review` », quand `cycle-increment.md` §4 posait
    l'étiquette `Done` et laissait la carte en `Planning`. L'exécutant a suivi le skill.
    ⚠️ **Le fait de méthode n'est pas la contradiction, c'est ce qui l'a rendue inévitable** : la
    règle du flux tiré était écrite partout du côté de celui qui **pose** (« un skill ne déplace
    jamais la carte »), et nulle part du côté de celui qui **tire**. Un audit de quatre lots l'a
    confirmé : neuf frontières de colonne sur seize n'avaient **aucun tireur écrit**, et toutes
    avaient la même forme — celle qui suit une pose de `Done`. Une interdiction sans contrepartie ne
    produit pas l'abstention : elle produit la transgression, parce qu'il faut bien que la carte
    avance. — *étape 5, dette de méthode*

69. **La règle manquait là où elle aurait servi : dans le seul fichier chargé d'office.** `CLAUDE.md`
    ne contenait aucune occurrence de « tiré », « pousser », `Done` ni *Advancement Labels*. Un agent
    qui charge `CLAUDE.md` puis un skill n'avait donc aucun moyen de savoir que le skill était
    fautif. Corollaire général : **une règle transverse qui ne vit que dans les documents de méthode
    n'est appliquée que par qui les ouvre**, et un exécutant n'ouvre que ce que sa tâche appelle.
    — *étape 5, dette de méthode*

70. **Deux affirmations écrites étaient fausses, et personne ne les avait contredites.**
    `cycle-pas.md` §1 (« Aucune étiquette, à aucun moment ») et `dod/pas/done.md` §4 (« le pas n'a
    pas de tiers qui juge ») décrivaient un niveau pas sans signal ni revue. L'utilisateur a tranché
    l'inverse : le pas porte les mêmes étiquettes que l'incrément, et `Code Review` existe à deux
    échelles — la **fonction** au pas, le **module** à l'incrément, le même skill contre deux
    référentiels. ⚠️ **Ce que le dispositif n'a pas su faire** : ces deux clauses étaient cohérentes
    entre elles et se citaient l'une l'autre, ce qui les rendait indétectables par une relecture
    croisée. Seul un humain qui connaît son intention pouvait les démentir. — *étape 7, correction*

71. **La règle écrite l'avant-veille a été re-commise en l'appliquant.** `D-069` a établi qu'une
    interdiction sans son geste positif produit la transgression. Deux jours plus tard, le chantier
    de `D-070` écrit dans `prendre-un-pas` *« ne pose aucune étiquette sur l'incrément »* — une
    interdiction — **sans réattribuer le `Done` qu'elle supprimait**, laissant la frontière
    `In Progress` › `Code Review` avec un tireur et plus aucun poseur. Exactement la forme que
    l'audit de `D-069` avait relevée neuf fois. ⚠️ **Connaître une règle n'empêche pas de la
    re-commettre au moment de l'appliquer** : elle avait été lue, citée dans le commit, et enfreinte
    dans le même diff. C'est l'argument le plus concret pour faire relire un chantier de méthode par
    un tiers plutôt que par son auteur — et c'est ce que `D-071` a inscrit le lendemain.
    — *étape 5, dette de méthode*

72. **Un décalage de numérotation ne se rattrape pas au `grep` naïf.** L'insertion d'une étape dans
    `flux.md` a décalé les six suivantes. Le balayage a cherché `étape N` et `| N |`, et manqué la
    troisième forme — `#N`, employée dans les *descriptions* de skills (`revue-code` renvoyait à
    `flux.md #8 et #9`, devenus #9 et #10). Deux comptes en toutes lettres ont survécu de même
    (« ces huit skills », « s'ajoutent aux huit »). ⚠️ **Un renvoi numérique s'écrit sous au moins
    trois formes dans ce dépôt** ; les chercher toutes, ou n'en écrire aucun.
    — *étape 5, dette de méthode*

---

## 2026-08-03 — les cartes naissent en `Todo` (`D-072`)

73. **Une dette a été classée « à documenter » alors qu'elle était « à supprimer ».** L'audit de
    `D-069` avait relevé que la frontière `Backlog` › `Todo` ne nommait aucun tireur, et l'avait
    rangée parmi les manques de documentation — sans se demander si un tireur était seulement
    **possible**. Il ne l'était pas : le déblocage n'est le travail de personne, il survient quand un
    *autre* incrément passe `Done`. ⚠️ **Le tri « manque écrit » / « manque structurel » n'était pas
    fait**, et le premier avale silencieusement le second : documenter un trou le rend plausible, et
    la question de son existence ne se repose plus. Le test qui aurait suffi : *qui, nommément,
    tirerait à travers cette frontière — et si la réponse est « personne, elle s'ouvre toute seule »,
    la frontière est en trop.* — *étape 5, dette de méthode*

74. **La colonne dupliquait une donnée que l'outil portait déjà, et avait divergé sans qu'on le
    sache.** `Backlog` ré-encodait en statut ce que Linear tient en relation `blockedBy` ; `CUR-6`
    s'est retrouvé en `Todo` en étant bloqué — un état que la colonne prétendait rendre impossible.
    Le défaut avait été vu à l'époque et traité comme une **erreur de saisie** (tableau corrigé, règle
    non assouplie), alors qu'il était le symptôme d'une redondance. ⚠️ **Une divergence entre deux
    représentations du même fait se lit d'abord comme une faute de l'opérateur** ; c'est la seconde
    occurrence qui révèle que le fait était représenté deux fois. Ne pas attendre la seconde :
    demander, à la première, *qui d'autre porte cette information.* — *étape 5, dette de méthode*

---

## 2026-08-03 — premier tour de `revue-plan`, et l'essai de `D-071`

75. **Une clause de DoD a durci sa règle en la recopiant, et le durcissement n'est apparu qu'à la
    première application réelle.** La règle « un piège s'accroche à son objet, jamais à un pas »
    s'énonce en cinq endroits ; `dod/story/plan-review.md` §1 était le seul à ajouter *« dans la
    table »*. Le tour a produit une **violation dure** sur un plan qui tenait pourtant l'invariant —
    ses huit pièges nommaient chacun leur objet, dans une section dédiée. ⚠️ **Le test qui manquait
    à la clause : est-ce qu'elle teste le mal qu'elle vise ?** « Dans la table » testait une mise en
    page. Et le durcissement était doublement piégeux, parce que `CLAUDE.md` — chargé d'office, donc
    le seul texte que l'auteur d'un plan a réellement sous les yeux — ne l'exige pas : une DoD qui
    demande plus que la règle chargée d'office fabrique des violations chez qui suit les règles.
    Tranché : la clause coche le **nom de l'objet**, jamais l'endroit. — *étape 6, correction*

76. **Le relecteur a eu l'honnêteté de dire que le mal visé était absent — et c'est ce qui a rendu
    la remarque utile.** Au lieu de trancher seul entre les deux formulations, il a posé *« à
    trancher : lequel des deux documents change »*, en citant les trois. Sans cette forme, la
    remarque se serait soldée par une correction du plan et le dépôt garderait sa divergence.
    ⚠️ **Une remarque de conformité peut mettre en cause son propre référentiel** ; l'interdire
    reviendrait à faire de la DoD une source infaillible, ce qu'aucun document du dépôt n'est.
    — *étape 6, dette de méthode*

77. **Deux relectures du même artefact, contre le même référentiel, ont divergé sur la
    conformité.** `revue-plan` a posé trois remarques d'axe Conformité — dont une arête de schéma
    tenue pour inversée et une bijection schéma ↔ table tenue pour rompue ; la relecture interne à
    trois axes a déclaré les **six clauses tenues**, bijection comprise, après l'avoir appariée
    nœud par nœud. ⚠️ **`tickets.md` §6.3 déclare la conformité délégable parce qu'il existe un
    référentiel opposable** — le fait est vrai, mais il n'entraîne pas que deux agents y
    convergeront. Ce qui se délègue est la **question**, pas la réponse. À garder en tête avant de
    traiter un verdict de conformité comme une mesure plutôt que comme un avis.
    — *étape 6, dette de méthode*

78. **Le dispositif d'une relecture pèse plus que sa position dans le flux.** Même artefact, même
    modèle, même référentiel : un relecteur unique rend quatre constats, trois axes parallèles en
    rendent dix — et les deux plus graves n'apparaissent qu'à trois. ⚠️ **Un dispositif inscrit au
    singulier parce que le pluriel n'avait pas été mesuré coûte le plus gros de sa valeur.**
    `D-071` avait été écrit sur un essai à un agent ; c'est le nombre de lentilles, pas la
    présence du relecteur, qui portait le gain. Le réflexe à garder : quand un skill prescrit un
    sous-agent, se demander **combien**, et contre **quels axes** — jamais laisser le singulier par
    défaut. — *étape 6, correction*

---

## 2026-08-03 — première correction de plan, à la main (`correction` n'existe pas)

79. **Deux remarques se sont soldées en faisant *disparaître* l'écart, pas en le documentant.** La
    revue avait posé la question ouverte — *« l'invariant couvre-t-il la racine multi-projets ? si
    oui, ce qui l'en sépare ; si non, l'écart mérite d'être écrit »* — et la bonne réponse n'était
    ni l'une ni l'autre : `ProjectsTool` n'avait besoin que du **registre**, donc la dépendance à la
    racine a été supprimée et la question est devenue sans objet. ⚠️ **Une remarque qui offre deux
    issues en a souvent une troisième, et c'est la meilleure.** Le réflexe à écrire dans
    `correction` : avant de choisir entre les branches proposées, chercher ce qui rend le choix
    inutile. — *étape 6, correction*

80. **Les remarques ne sont pas indépendantes, et les traiter dans l'ordre les fait mentir.**
    Reprendre `ProjectsTool → ProjectRegistry` a rendu une remarque voisine sans objet (la cellule
    qui devait déclarer une lecture qui n'existe plus) et **déplacé** une troisième (la course
    n'était pas sur `ProjectWorkspaces` mais sur le registre). Un correcteur qui aurait répondu fil
    par fil dans l'ordre de la liste aurait écrit trois réponses incohérentes entre elles, chacune
    juste isolément. ⚠️ **Lire les remarques *toutes* avant d'en reprendre *une*** — et le dire dans
    la réponse quand une reprise en déplace une autre, sinon le vérificateur relit un fil dont la
    justification vit ailleurs. C'est l'exact symétrique de l'axe d'ensemble en revue.
    — *étape 6, dette de méthode*

81. **Un identifiant de remarque tronqué échoue *sans* arrêter le lot.** Un `id` recopié à huit
    caractères depuis un affichage abrégé a fait répondre à Linear *« Entity not found: Comment »*,
    et le script a poursuivi en affichant sa ligne de fin — `set -e` ne voit pas l'échec, la CLI
    sortant en 0. ⚠️ **Vérifier le compteur `open` après un lot, jamais la sortie du script** :
    c'est la seule mesure qui ne ment pas. — *étape 6, dette d'outillage*

---

## 2026-08-03 — première vérification de plan, à la main (`verification` n'existe pas)

82. ⚖️ **À TRANCHER** — **Répondre et solder sont le même geste, donc le compteur cesse de mesurer
    quoi que ce soit.**
    `cursus linear comment resolve` est le seul verbe qui écrit dans un fil : la correction a donc
    soldé les douze remarques *en y répondant*, et le compteur affichait `open: 0 / total: 12`
    **avant** que la vérification commence. Or `cycle-increment.md` §5 lui donne pour critère de
    sortie « `Done` si `open` vaut 0 » — un critère déjà satisfait, qu'un vérificateur qui n'aurait
    rien lu aurait rempli à l'identique. ⚠️ **L'état Linear est structurellement incapable de
    distinguer « soldé » de « soldé et vérifié ».** Deux issues, et il faut en choisir une avant
    d'écrire les skills : donner à la CLI un verbe qui répond **sans** solder — et alors c'est la
    vérification qui solde, ce qui rend le compteur exact —, ou renoncer au compteur comme critère
    et exiger du vérificateur une trace écrite par fil. La première est meilleure : elle rétablit
    l'accord entre le geste et la mesure au lieu d'ajouter une cérémonie.
    — *étape 7, dette d'outillage · dette de méthode*

83. **Une reprise s'est justifiée par un document que le vérificateur n'a pas le droit d'ouvrir.**
    Une réponse se concluait sur « consigné en frictions 75 et 76 de `journal-frictions.md` », et le
    mandat de la vérification interdit ce fichier — il raconte le tour en cours, donc il souffle.
    Ici le solde tenait sans cette pièce, et le vérificateur l'a dit ; mais la forme est fautive.
    ⚠️ **Une reprise ne peut se fonder que sur des artefacts que l'étape suivante peut ouvrir** — le
    plan, le code, un référentiel de méthode, une entrée `decisions.md`. Le journal des frictions et
    les fiches de REX n'en sont pas : ils décrivent l'exécution, pas l'objet.
    — *étape 6, dette de méthode*

84. **Il existe une troisième façon de solder, et aucun référentiel ne la prévoit.** `D-067` en
    nomme deux — la reprise faite, ou le refus motivé. Une remarque s'est soldée par la troisième :
    **amender le référentiel qu'elle invoquait**, la clause de DoD s'étant révélée fautive en
    cochant une mise en page plutôt qu'un contenu. Ni la DoD ni `cycle-increment.md` §5 ne disent ce
    qu'un vérificateur en fait — ce n'est ni une reprise de l'artefact ni un refus. Il a tranché sur
    le fait matériel que la DoD *avait* changé et que la contrepartie promise existait, ce qui est le
    bon critère : **vérifier l'amendement, pas la remarque.** À écrire dans `verification`.
    — *étape 7, trou de référentiel*

85. **La mémoire a de nouveau fuité d'office dans un mandat qui l'interdisait — et pour la première
    fois, la parade s'est laissée mesurer.** Septième occurrence de la friction 43 : un extrait de
    `MEMORY.md` a été présenté au vérificateur dans son rappel d'ouverture, alors que son mandat
    nommait ce fichier en interdit. Nouveauté : l'extrait annonçait **l'état** (colonne, étiquette,
    « zéro remarque ouverte sur douze », reprise menée à la main) et **pas la réponse** — aucun fil
    n'y était jugé, parce que la mémoire avait été délibérément vidée de son contenu avant le tour.
    Le vérificateur a pu le constater et le déclarer. ⚠️ **C'est la confirmation directe de la
    friction 64** : puisqu'on ne peut pas empêcher la fuite, la seule variable qu'on tienne est *ce
    qu'il y a à fuiter*. Une mémoire qui ne dit que l'état ne contamine pas un jugement.
    — *étape 7, dette d'outillage*

86. **`cursus linear comment list` rend les fils à plat, et l'appariement est manuel.** Trente-sept
    kilo-octets de JSON non groupé, remarques et réponses mêlées dans un ordre qui n'est ni celui du
    document ni celui des fils ; le vérificateur a reconstitué les douze couples à la main par
    `parentId`, au prix d'un aller-retour de plus (la sortie déborde et part en fichier). `--unresolved`
    est inutilisable ici puisqu'il ne rendrait rien. ⚠️ **Le format de sortie d'un outil de revue est
    une décision de méthode** : ce qu'il rend difficile à lire, l'étape suivante le lira mal.
    — *étape 7, dette d'outillage*

---

## 2026-08-03 — écrire la fiche de la boucle, et découvrir deux dettes du dossier `rex/`

87. **Une fiche de REX écrite après compaction perd la rubrique que le README juge la moins
    bureaucratique.** Le dossier exige que la commande figure **verbatim et rejouable**, parce que le
    mode d'échec redouté — *« la méthode a l'air en place et n'est pas chargée »* — est muet. Or les
    mandats des quatre temps n'étaient plus en contexte au moment d'écrire la fiche. ⚠️ **Le remède
    est mécanique et il a marché** : extraire du transcript de session le seul champ `prompt` des
    invocations de sous-agents, par grep ciblé — jamais lire le transcript en entier, qui noierait le
    contexte du rédacteur. À écrire comme clause du `rex/README.md`. ⚠️ **Mais il ne couvre pas
    tout** : un geste mené **en session**, sans sous-agent, n'a aucun prompt à extraire. Le mandat de
    la correction est irrécupérable **par construction**, pas par accident — et c'est un argument de
    plus pour que les gestes qu'on veut mesurer passent par un sous-agent.
    — *étape 7, dette du dossier `rex/`*

88. ⚖️ **À TRANCHER** — **La règle de comptage des « constats de fond » n'existe pas, et la mesure
    de l'essai en dépend.**
    La table qui oppose les trois dispositifs de relecture compare **4 · 10 · 11** quand les
    dispositifs ont matériellement rendu **5 · 10 · 12** : l'écart vient d'un tri implicite entre
    constat de fond et constat procédural, que personne n'a écrit. La conclusion de l'essai ne bouge
    pas — l'ordre de grandeur tient dans les deux dénombrements — mais **un dénombrement non
    reproductible ne se compare pas au tour suivant**. Même famille que la friction 65 : deux
    dispositifs de fiabilité différente rendent un chiffre que rien ne qualifie. ⚠️ **Écrire la règle
    de tri avant le prochain essai, pas après** — écrite après, elle se choisira sur les nombres
    qu'elle doit départager.

    ⚠️ **Cette entrée porte le marqueur pour toute une famille**, rencontrée trois fois avant d'être
    nommée : la **62** (la métrique compte les appels d'attente et ignore ceux de *préparation* à
    l'attente) et la **65** (« zéro constat écarté » rend la même valeur qu'un tour ait vérifié ses
    assertions ou non) en sont les deux occurrences antérieures, laissées nues. La décision est la
    même dans les trois cas : **une ligne de mesure doit porter son mode d'obtention**, sans quoi
    deux dispositifs de fiabilité différente rendent un chiffre identique.
    — *étape 7, dette du dossier `rex/`*

---

## 2026-08-03 — premier tour de `decoupage-pas` (`CUR-47`)

89. **Le plan avait nommé quatre gestes « pour qu'aucun ne se perde au découpage », et le découpage
    les a perdus.** La section « Ce que cet incrément met à jour dans la documentation » existait
    précisément parce qu'une remarque de revue l'avait exigée ; au moment de couper, aucun des huit
    pas ne la portait — chacun décrivait du code, et les quatre mises à jour d'`architecture.md` et
    de `decisions.md` n'appartenaient à aucun. Seule **l'étape 5** — la relecture de l'**ensemble**,
    qui cherche le geste qu'aucune pièce ne porte — les a rattrapées. ⚠️ **Une précaution écrite dans
    l'artefact amont ne se transmet pas toute seule à l'aval** : elle survit parce qu'une étape la
    cherche, jamais parce qu'elle est écrite. C'est la meilleure justification de l'axe d'ensemble
    qu'on ait eue, et elle est arrivée à son premier tour. — *étape 8, méthode éprouvée*

90. ⚖️ **À TRANCHER** — **L'étape 5 n'a aucun dispositif, et c'est devenu une incohérence du corpus
    le jour même.**
    `decoupage-pas` §5 rend la relecture d'ensemble **obligatoire** et lui confie explicitement ce
    qui « tient lieu de la porte humaine » — mais elle est faite par le producteur, en session, sur
    son propre découpage. Or `D-073`, écrit quelques heures plus tôt, vient d'établir l'inverse pour
    `discovery` et `spec` : sous-agent par axe, en session neuve, agrégateur **distinct du binôme**,
    au motif mesuré qu'une auto-évaluation « ne voit pas ce que son auteur ne peut pas voir ».
    ⚠️ **Le seul endroit du flux qui n'a *aucun* relecteur en aval est aussi le seul dont la
    relecture est faite par l'auteur.** Ici le tour s'en est bien sorti (friction 89), mais un
    succès ne mesure pas un dispositif. À trancher : généraliser `D-073` à `decoupage-pas` §5, ou
    écrire pourquoi ce cas s'en dispense. — *étape 8, dette de méthode*

91. **La maille visée par le plan était basse de 40 %, et l'écart n'était pas une erreur.** Le plan
    annonçait « cinq à six pas » ; le découpage en a rendu **huit**. Trois pas que la maille comptait
    pour un — la naissance du projet socle, la garde des workspaces, la protection du registre —
    portent des acceptations de natures différentes (un déménagement, une non-régression de
    sélection, un invariant de collection), et les fondre aurait produit un pas qu'aucune fenêtre
    fraîche ne tient. ⚠️ **Un point de mesure, pas une règle** : à retenir pour savoir, au troisième
    ou quatrième découpage, si la maille d'un plan sous-estime **systématiquement**. Si oui, c'est
    `plan-design` §4 qu'il faudra corriger, pas les découpages. — *étape 8, mesure à suivre*

## 2026-08-03 — premier tour de `prendre-un-pas` (`CUR-65`)

92. **La branche de story n'a pas de créateur écrit, et le premier pas la fabrique en passant.**
    `prendre-un-pas` §1 dit « créer, ou reprendre, la branche `pas/<identifiant>-slug` **depuis la
    branche de la story** ». Or aucune branche de story n'existait, et **aucun skill ne dit qui la
    crée** : `decoupage-pas` ne touche pas à git, `plan-design` non plus. Le premier pas l'a donc
    créée lui-même, en silence, avant de créer la sienne. ⚠️ **C'est exactement le motif de la
    tentation de pousser** (`CLAUDE.md`, moitié aval de `D-069`) transposé à git : quand une
    frontière ne nomme personne, celui qui passe fait le geste sans que ce soit écrit nulle part.
    Le geste est bon ; c'est son silence qui est le défaut. — *étape 1, dette de méthode*

93. **La convention de nom de branche d'un pas date d'avant que les pas aient des cartes.**
    `flux.md` §6 donne `pas/CUR-45-3-slug` — un identifiant de story suivi d'un **rang**. Cette
    forme suppose que le pas n'a pas d'identité propre, ce qui a cessé d'être vrai : `D-069` a donné
    aux pas leurs propres étiquettes et leur propre `Code Review`, et `decoupage-pas` les crée
    comme sous-tâches numérotées par Linear. La branche a donc été nommée `pas/CUR-65-…`, ce que la
    table n'autorise pas littéralement — alors que la phrase juste au-dessus d'elle (« le nom porte
    l'identifiant Linear, ce qui suffit à Linear pour rattacher seul la branche ») l'exige.
    ⚠️ **La table et sa propre justification se contredisent** ; c'est la table qui a vieilli.
    — *étape 1, incohérence de corpus*

94. **Un test de garde ne peut pas être rouge tout seul, et aucun skill ne dit qu'on a le droit de
    le falsifier.**
    Le seul comportement neuf du pas était un test d'architecture — « cet assembly ne référence pas
    Avalonia ». Il est vert dès qu'il est écrit, par construction : c'est ce qu'il garde qui est
    déjà vrai. Le rendre rouge a exigé de **casser volontairement** le code de production, d'observer
    l'assertion tomber, puis de retirer la casse. ⚠️ **`prendre-un-pas` §3 interdit le rouge « pour
    la mauvaise raison » mais ne prévoit pas le cas où il n'y a *aucune* bonne raison disponible**,
    et sa clause d'échappement — « un test vert du premier coup est légitime » — invite à sauter
    l'étape. Or c'est précisément ici qu'il ne faut pas : un test de garde jamais vu rouge peut être
    silencieusement inopérant, et celui-ci l'aurait été si l'on s'était contenté d'ajouter une
    `PackageReference` (le reflet ne rend que les assemblies **employées**). — *étape 3, dette de
    méthode*

95. **La liste de mises à jour documentaires du plan était nommée « pour qu'aucune ne se perde », et
    elle était incomplète.**
    Le plan de design nommait quatre gestes ; le découpage les a répartis (friction 89). Mais en
    exécutant le premier pas, **trois autres documents sont devenus faux** que personne n'avait
    nommés : l'inventaire des projets et le graphe de dépendances d'`architecture.md`, et la carte
    des couches de `schemas.md` — un assembly qui naît change les trois par construction.
    ⚠️ **Une liste écrite en amont protège de l'oubli, pas de son propre angle mort** : le plan a
    listé ce que *sa décision* rendait faux, jamais ce que *l'existence d'un nouvel objet* rend
    faux. Les cartes d'état ne se déduisent pas des arbitrages, elles se déduisent du dépôt.
    À voir si le cas se répète : la parade serait une clause de `prendre-un-pas` — après le vert,
    balayer les cartes d'état avant de clore —, pas une liste plus longue en amont. — *étape 5,
    dette de méthode*

96. ⚖️ **À TRANCHER** — **Personne ne crée la PR, et la règle de push l'interdirait de toute façon.**
    Le corpus **suppose** la PR partout — `flux.md` §6 la met dans sa table (« un niveau, une
    branche, une PR »), `tickets.md` la range dans les adresses d'un ticket, et surtout §6 justifie
    l'existence même de la strate `pas/` par le fait que « la revue d'un pas peut avoir lieu après
    le commit, **sur la PR** ». Mais **aucun document ne dit qui l'ouvre ni quand** :
    `prendre-un-pas` §5 s'arrête à poser `Done`, `revue-code` la trouve déjà là, et aucun des deux
    ne touche à git au-delà de la branche.
    ⚠️ **Et le geste est bloqué par défaut** : ouvrir une PR exige de pousser, or la convention de
    commit dit « ne jamais pousser sans demande explicite ». Écrire le geste dans un skill ne
    suffirait donc pas — il faut d'abord distinguer *pousser une branche de travail* de *pousser sur
    `main`*, ce que la règle ne fait pas.
    **Ce qui n'est pas bloqué, en revanche** : `revue-code` n'a pas besoin d'une PR GitHub. Il relit
    un `git diff <base>...HEAD` contre un point fixe, et les remarques vivent sur la **carte
    Linear** (`D-045`), pas dans le fil de la PR. Une revue de pas peut donc tourner sur la branche
    locale, aujourd'hui. La PR est nécessaire à la **fusion**, pas à la relecture.
    À trancher : PR ouverte en fin de pas (ce que le motif de la strate `pas/` réclame), ou à la
    fusion (moins de bruit sur un dépôt public, mais §6 perd sa justification). — *frontière
    pas → revue, dette de méthode*
