# 2026-07-30 — `revue-discovery`, tour 2 : la reprise passe au crible

> Second tour sur le **même artefact, repris** entre-temps. Le tour 1 est en
> [`2026-07-30-revue-discovery-tour-1.md`](2026-07-30-revue-discovery-tour-1.md) ; rubriques fixes
> dans [`README.md`](README.md).

## 1. Ce qui a tourné

**La commande est identique au tour 1, au fichier de sortie près** — c'est la condition pour que les
deux fiches se comparent :

```bash
claude -p "Fais la revue de la Discovery de la feature « Un agent pilote Cursus ». \
C'est un projet Linear de l'équipe CUR, espace cursus-app." \
  --output-format stream-json --verbose \
  --allowedTools Read Grep Glob Bash "mcp__claude_ai_Linear__*" \
  > tour-2.jsonl 2>&1
```

**Le skill s'est de nouveau déclenché seul**, sans être nommé — `Skill` avec
`{"skill":"revue-discovery","args":"Feature « Un agent pilote Cursus » — projet Linear, équipe CUR,
espace cursus-app"}`. La découverte par description est donc reproductible, et non un coup de
chance du premier tour.

**Ce qui a changé entre les deux tours** : l'artefact. Il porte les sept reprises du matin, et son
fil porte les sept remarques soldées.

## 2. Chiffres

| Mesure | Tour 1 | Tour 2 |
|---|---|---|
| Durée | 8 min 06 s | **9 min 33 s** (+18 %) |
| Tours | 21 | **31** |
| Coût | 2,99 $ | **3,54 $** |
| Tokens de sortie | 30 452 | 32 417 |
| Sous-agents | 3, parallèles | 3, parallèles |
| Remarques | 7 — 4 dures, 3 jugements | **6 — 1 dure, 5 jugements** |
| Permissions refusées | 0 | **6** |
| Erreurs d'outil | 0 | 0 |

**Le déplacement dur → jugement est le chiffre à lire.** Les manquements grossiers ont été purgés au
tour 1 ; ce qui reste demande d'argumenter plutôt que d'opposer une clause. Un tour plus cher qui
rend moins de violations n'est pas un tour moins bon.

## 3. Conformité au protocole

Toutes les clauses tenues, comme au tour 1 — session neuve, trois axes séparés, deux citations par
remarque, étiquettes de confiance, aucune réécriture, remarques sur la carte par la CLI, étiquette
posée sans déplacer la colonne (`labels: ["Rework Needed"]`, `status: Discovery`).

Deux raffinements que le tour 1 n'avait pas montrés :

- Chaque prompt d'axe porte l'**interdit croisé** explicite — *« Ne prononce pas non plus sur
  l'arbitrage (§2) ni sur la forme (§5) : d'autres axes les couvrent »* — et recopie les **quatre
  reproches interdits** de §4. L'isolation n'est pas seulement structurelle, elle est instruite.
- **Le fil du tour 1 est resté chez l'orchestrateur.** Les trois axes ont reçu le document inline,
  sans l'historique : ils ont jugé à l'aveugle, et c'est l'orchestrateur qui a croisé leurs constats
  avec les remarques soldées. C'est la bonne répartition — un axe qui verrait le fil serait ancré
  par ce que le tour précédent a trouvé.

**Un incident** : six tentatives de `Write` vers `/tmp/revue-disco/*.md` (des corps de commentaire
en brouillon), toutes refusées par le harnais, puis contournées seul par des heredocs. Aucune
tentative d'écrire dans le dépôt ni dans le document. Voir §5.

## 4. Qualité de la sortie

**Les six remarques rouvrent des points soldés au tour 1. Aucune ne porte sur du neuf.** C'est le
résultat central, et il vise la reprise plutôt que l'artefact :

> *« Vous vous étiez engagé, au tour précédent, à reprendre **par le bas** : "chaque piste revient à
> la même maigreur factuelle". L'engagement est tenu **en volume**. Il ne l'est pas en
> **direction**. »*

> *« Le tour précédent avait diagnostiqué ce registre sur la seule piste CLI, et la reprise a bien
> retiré l'excès côté MCP. L'échelle, elle, est restée — elle est simplement devenue symétrique. »*

Le binôme avait retiré des **mots** en croyant retirer une **orientation**. Le jugement-vedette du
tour 1 revient à l'identique, augmenté d'un adverbe : *« un lecteur sort **encore** de la section 3
avec un ordre de préférence qu'aucune phrase n'énonce »*.

**La violation dure est la plus fine des deux tours** : *« Rien n'est à construire dans Cursus pour
cette voie »* est une **estimation de coût**, que §2 interdit — mais elle vaut *zéro*, donc elle ne
ressemble pas à un chiffre. Un coût nul se glisse là où un coût chiffré serait vu.

Complétude et Adresse au lecteur passent en **accord** (le second avec un jugement non bloquant :
« invariants d'authoring », « couche d'édition », « le noyau » ne sont jamais introduits pour qui lit
depuis le tracker). Seul Non-arbitrage reste en désaccord.

## 5. Frictions

Renvoi au [journal](../journal-frictions.md), entrées **28 à 31**.

La plus instructive est l'incident `Write` (28) : la clause visait *« ne pas réécrire l'artefact »*,
et l'allowlist a bloqué **toute** écriture de fichier, y compris des brouillons hors dépôt.
**Interdire un outil n'est pas interdire un geste** — le harnais a tenu la clause, mais plus large
qu'elle, au prix de six tours.

## 6. Ce que le tour a changé

- **Il a invalidé la reprise du matin**, qui était complaisante de bonne foi. La correction reste à
  faire, et elle porte sur la direction des pistes, pas sur leur longueur.
- **Il lève la crainte d'ancrage par ce dossier.** La fiche du tour 1 et le journal étaient dans le
  dépôt et racontaient ce que le tour 1 avait trouvé ; il ne les a **pas lus**. Les seuls fichiers
  ouverts sont `dod/feature/discovery.md`, `revue/SKILL.md` et `tickets.md`. Ses constats de
  récidive viennent du **fil Linear**, la bonne source. Le dossier `rex/` peut donc grossir sans
  polluer les tours suivants — sous réserve que ça reste vrai, ce qui se revérifie à chaque fiche.
- **Il valide le pari du cycle court.** `cycle-feature.md` §3 laisse le binôme solder ses propres
  remarques, *« tenable parce qu'un tour de revue de plus suit toujours »*. Le pari était non
  éprouvé ; il vient de l'être, dans le seul cas qui compte — une reprise sincère et fausse.

## 7. Verdict pour `revue-discovery`

**Promu**, et la réserve du tour 1 est levée pour moitié : la conformité et l'autonomie se
reproduisent sur un artefact **repris**, plus seulement sur un artefact fraîchement écrit, et la
découverte par description n'était pas un hasard.

⚠️ **Ce qui reste non éprouvé** : les deux tours ont porté sur **le même artefact**, écrit par le
binôme, et le skill est de la même main. Le troisième tour utile n'est pas un troisième passage
ici — c'est un premier passage sur une Discovery que le binôme n'aura pas rédigée. Tant qu'il n'a
pas eu lieu, on mesure un auteur autant qu'un skill.

⚠️ **Et ce que ni l'un ni l'autre tour ne dit** : personne n'a vérifié que le relecteur n'a rien
**manqué**. Une revue qui ne trouve rien et une revue qui ne cherche pas se ressemblent — c'était
déjà la limite du tour 1, elle est intacte.
