namespace Cursus.Core.Workflows.Editing;

/// <summary>
/// La surface d'édition mutable d'un graphe. <see cref="WorkflowDefinition"/> est
/// un instantané immuable, parfait à exécuter mais inutilisable à éditer : ce type
/// porte l'identité mutable et les opérations pour <b>construire</b> un graphe
/// depuis rien (ajouter une étape, poser l'entrée, décrire un script, tracer une
/// arête) autant que pour le <b>remanier</b> (renommer, supprimer).
///
/// <para>
/// Deux invariants s'y distinguent. L'un est <b>tenu</b> : l'unicité d'id — les
/// opérations travaillent par id, donc <c>AddStep</c> désambiguïse et
/// <c>RenameStep</c> refuse une collision. L'autre est <b>toléré</b> : la validité
/// du graphe (entrée posée, cibles existantes) reste au validateur — « brouillons
/// permis ». D'où l'asymétrie des gardes : le <b>sujet</b> d'une opération doit
/// exister, sa <b>référence</b> (cible d'arête, point d'entrée) peut pendre.
/// </para>
/// </summary>
public sealed class WorkflowDraft
{
    public WorkflowDraft(WorkflowDefinition definition)
    {
        EntryStep = definition.EntryStep;
        _steps = [.. definition.Steps];
    }

    private readonly List<StepDefinition> _steps;

    /// <summary>Le point d'entrée en cours d'édition.</summary>
    public string EntryStep { get; private set; }

    /// <summary>
    /// Ajoute une étape à partir de son titre humain : l'id en est le slug, le
    /// libellé reste tel quel. L'étape naît avec un script vide (elle ne fait rien
    /// encore, brouillon permis), une visite, aucune arête. Retourne l'id, que
    /// l'appelant référence aussitôt (poser l'entrée, tracer une arête).
    /// </summary>
    public string AddStep(string name)
    {
        var id = Uniquify(Slug.From(name));
        _steps.Add(new ScriptStep(id, name, new ScriptSpec("", []), 1, []));
        return id;
    }

    /// <summary>
    /// Ajoute une étape-agent à partir de son titre — jumelle d'<see cref="AddStep"/>
    /// pour l'autre kind : même slug désambiguïsé, mais l'étape naît confiée au seul
    /// harness connu (<see cref="AgenticHarness.ClaudeCode"/>) sur son premier modèle,
    /// avec un prompt vide (elle ne demande rien encore, brouillon permis — le
    /// validateur signalera le prompt vide). Le harness est en dur ici parce qu'il
    /// n'y en a qu'un ; le choix du modèle et la rédaction du prompt suivent
    /// (<see cref="SetModel"/>, <see cref="SetPrompt"/>). Retourne l'id.
    /// </summary>
    public string AddAgentStep(string name)
    {
        var id = Uniquify(Slug.From(name));
        var harness = AgenticHarness.ClaudeCode;
        _steps.Add(new AgentStep(id, name, harness.Name, harness.Models[0].Id, "", 1, []));
        return id;
    }

    /// <summary>
    /// Rend un id libre à partir d'un id souhaité, en suffixant au besoin
    /// (« compiler », « compiler-2 », « compiler-3 »…). L'unicité d'id est un
    /// invariant du brouillon — ses propres opérations travaillent par id — donc
    /// un ajout ne peut pas produire de collision.
    /// </summary>
    private string Uniquify(string desired)
    {
        var candidate = desired;
        for (var suffix = 2; _steps.Any(s => s.Id == candidate); suffix++)
            candidate = $"{desired}-{suffix}";

        return candidate;
    }

    /// <summary>
    /// Désigne le point d'entrée. Permissif : viser une étape pas-encore-créée est
    /// une intention légitime en cours d'édition — le validateur signalera
    /// l'entrée inconnue. Le brouillon ne garde que l'unicité d'id, pas la validité
    /// du graphe.
    /// </summary>
    public void SetEntryStep(string id) => EntryStep = id;

    /// <summary>
    /// Renomme une étape et fait suivre toute référence : les arêtes qui la
    /// visaient sont retargées. Sans quoi le renommage laisserait le graphe
    /// incohérent avec lui-même. Refuse (<see cref="DuplicateStepIdException"/>) de
    /// viser un id déjà pris : l'unicité d'id est un invariant que ce type garde.
    /// </summary>
    public void RenameStep(string oldId, string newId)
    {
        if (newId != oldId && _steps.Any(s => s.Id == newId))
            throw new DuplicateStepIdException(newId);

        if (EntryStep == oldId)
            EntryStep = newId;

        for (var i = 0; i < _steps.Count; i++)
            if (_steps[i].Id == oldId)
                _steps[i] = _steps[i] with { Id = newId };

        MapEdges(e => e.Target == oldId ? e with { Target = newId } : e);
    }

    /// <summary>
    /// Supprime une étape et purge toute référence : les arêtes qui la visaient
    /// disparaissent, le point d'entrée se vide s'il la désignait. Les références
    /// suivent le sort de leur cible — le graphe reste clos plutôt que de laisser
    /// pendre des arêtes vers un fantôme créé par la suppression elle-même.
    /// </summary>
    public void RemoveStep(string id)
    {
        if (EntryStep == id)
            EntryStep = "";

        _steps.RemoveAll(s => s.Id == id);

        MapEdges(e => e.Target == id ? null : e);
    }

    /// <summary>
    /// Remplace le script d'une étape en bloc — <see cref="ScriptSpec"/> porte déjà
    /// tout ce que l'éditeur règle. Le sujet doit exister : on n'édite pas une étape
    /// fantôme (là où viser un fantôme depuis une arête reste permis).
    /// </summary>
    public void SetScript(string id, ScriptSpec script)
    {
        var index = IndexOf(id);
        // Le script est propre au ScriptStep : régler un script suppose une étape-script.
        // Le cast est le garant du kind — poser un script sur une étape-agent est un
        // bug d'appelant, pas un état représentable. SetPrompt/SetModel sont ses jumeaux
        // pour l'autre kind.
        _steps[index] = ((ScriptStep)_steps[index]) with { Script = script };
    }

    /// <summary>
    /// Remplace le prompt d'une étape-agent. Jumeau de <see cref="SetScript"/> pour
    /// l'autre kind : même choke <see cref="IndexOf"/> (le sujet doit exister), et le
    /// cast sur <see cref="AgentStep"/> garde l'invariant de kind — on ne règle un
    /// prompt que sur une étape-agent.
    /// </summary>
    public void SetPrompt(string id, string prompt)
    {
        var index = IndexOf(id);
        _steps[index] = ((AgentStep)_steps[index]) with { Prompt = prompt };
    }

    /// <summary>
    /// Remplace le modèle d'une étape-agent (l'identifiant offert par son harness).
    /// Jumeau de <see cref="SetPrompt"/> ; permissif sur la valeur — un modèle inconnu
    /// est signalé par le validateur, pas refusé ici (brouillon permis).
    /// </summary>
    public void SetModel(string id, string modelId)
    {
        var index = IndexOf(id);
        _steps[index] = ((AgentStep)_steps[index]) with { ModelId = modelId };
    }

    /// <summary>
    /// Ajoute une arête gardée en fin de liste des sorties de <paramref name="from"/>.
    /// Le sujet (<paramref name="from"/>) doit exister ; la cible peut être encore
    /// absente (le validateur signalera une cible inconnue). Les doublons d'arêtes
    /// sont permis — c'est un brouillon.
    /// </summary>
    public void AddEdge(string from, Guard guard, string target)
    {
        var index = IndexOf(from);
        _steps[index] = _steps[index] with { OutEdges = [.. _steps[index].OutEdges, new Edge(guard, target)] };
    }

    /// <summary>
    /// Retire l'arête de <paramref name="from"/> à la position <paramref name="index"/>.
    /// Par position parce que deux arêtes peuvent partager une cible sous des gardes
    /// distinctes : l'index (le rang de la ligne dans l'UI) est le seul discriminant
    /// non ambigu. Un index hors bornes laisse remonter le
    /// <see cref="ArgumentOutOfRangeException"/> du framework.
    /// </summary>
    public void RemoveEdge(string from, int index)
    {
        var step = IndexOf(from);
        var edges = _steps[step].OutEdges.ToList();
        edges.RemoveAt(index);
        _steps[step] = _steps[step] with { OutEdges = edges };
    }

    /// <summary>Fige la surface de travail en un instantané que le catalogue persiste.</summary>
    public WorkflowDefinition ToDefinition() => new(EntryStep, [.. _steps]);

    /// <summary>
    /// La position d'une étape, ou la levée si elle est absente. Choke commun des
    /// opérations dont l'étape est le <b>sujet</b> (éditer son script, partir une
    /// arête d'elle) — par opposition à celles où elle n'est qu'une <b>référence</b>
    /// (cible d'arête, point d'entrée), tolérées absentes.
    /// </summary>
    private int IndexOf(string id)
    {
        var index = _steps.FindIndex(s => s.Id == id);
        if (index < 0)
            throw new UnknownStepException(id);

        return index;
    }

    /// <summary>
    /// L'opération référentielle commune : réécrit les arêtes sortantes de chaque
    /// étape. Une transformation qui rend <c>null</c> purge l'arête ; c'est ainsi
    /// que renommer les retarge et que supprimer les retire.
    /// </summary>
    private void MapEdges(Func<Edge, Edge?> transform)
    {
        for (var i = 0; i < _steps.Count; i++)
            _steps[i] = _steps[i] with
            {
                OutEdges = [.. _steps[i].OutEdges.Select(transform).OfType<Edge>()],
            };
    }
}
