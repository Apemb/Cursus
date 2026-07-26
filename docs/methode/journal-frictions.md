# Journal des frictions — exécution du flux sans skill

> Tenu selon `D-039` : on exécute la méthode **sans** skill, on note chaque friction au fil de
> l'eau, et le journal écrit le skill — après deux ou trois passages, jamais après un seul.
> Une ligne brute par occurrence. Ce fichier n'est pas de la méthode, c'est de la matière.

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
