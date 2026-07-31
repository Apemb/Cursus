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
