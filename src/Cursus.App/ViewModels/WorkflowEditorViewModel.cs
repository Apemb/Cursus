using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Cursus.Core.Projects;
using Cursus.Core.Workflows;
using Cursus.Core.Workflows.Editing;
using Cursus.Core.Workflows.Validation;

namespace Cursus.App.ViewModels;

/// <summary>
/// Le module d'édition d'un workflow : un adaptateur mince autour d'un
/// <see cref="WorkflowDraft"/>. Toute la logique métier — unicité d'id tenue,
/// validité tolérée, retarge et purge d'arêtes — reste dans le brouillon (Core,
/// déjà TDD) ; ce VM ne fait que binder les gestes et <b>re-projeter</b> le
/// brouillon en lignes après chaque mutation structurelle, en validant en direct.
/// Non testé, comme toute la vue (§7.12).
///
/// <para>
/// Monté par la fabrique <see cref="Open"/>, sœur de
/// <c>RunViewModel.StartLive</c>/<c>Replay</c> : la coquille ne construit jamais le
/// brouillon elle-même, elle reçoit l'éditeur câblé sur le catalogue du projet.
/// </para>
/// </summary>
public partial class WorkflowEditorViewModel : ObservableObject
{
    private readonly WorkflowCatalog _catalog;
    private readonly Action _onSaved;
    private readonly WorkflowDraft _draft;

    // Vrai le temps d'une projection : la ré-affectation d'EntryStep depuis le
    // brouillon ne doit pas être confondue avec un choix de l'utilisateur (sans
    // quoi projeter rappellerait SetEntryStep en boucle).
    private bool _projecting;

    private WorkflowEditorViewModel(string id, WorkflowCatalog catalog, Action onSaved, WorkflowDraft draft)
    {
        Id = id;
        _catalog = catalog;
        _onSaved = onSaved;
        _draft = draft;
        _entryStep = "";
        Steps = new ObservableCollection<StepEditorRow>();
        StepIds = new ObservableCollection<string>();
        Project();
    }

    /// <summary>
    /// Ouvre un workflow pour l'éditer : lit sa définition parsée par la porte sœur
    /// <see cref="WorkflowCatalog.Open"/> (rend le graphe même invalide, <c>D-020</c>)
    /// et en tire un brouillon. Repli sur un brouillon vide si le document est
    /// illisible — un cas de corruption manuelle ; <see cref="WorkflowCatalog.Create"/>
    /// n'en produit jamais.
    /// </summary>
    public static WorkflowEditorViewModel Open(string id, WorkflowCatalog catalog, Action onSaved)
    {
        var parsed = catalog.Open(id);
        var draft = new WorkflowDraft(parsed.Definition ?? new WorkflowDefinition("", []));
        return new WorkflowEditorViewModel(id, catalog, onSaved, draft);
    }

    /// <summary>L'identifiant de fichier du workflow édité, affiché en tête du module.</summary>
    public string Id { get; }

    /// <summary>Les étapes du graphe, re-projetées du brouillon à chaque mutation structurelle.</summary>
    public ObservableCollection<StepEditorRow> Steps { get; }

    /// <summary>Les identifiants d'étapes, pour les listes de choix (point d'entrée, cible d'arête).</summary>
    public ObservableCollection<string> StepIds { get; }

    /// <summary>Le point d'entrée, choisi parmi les étapes ; permissif (le validateur signale une entrée inconnue).</summary>
    [ObservableProperty]
    private string _entryStep;

    /// <summary>Le titre saisi pour une nouvelle étape ; vidé après un ajout.</summary>
    [ObservableProperty]
    private string _newStepTitle = "";

    /// <summary>Le rapport de validation en une liste lisible — recalculé à chaque mutation.</summary>
    [ObservableProperty]
    private string _problems = "";

    /// <summary>Vrai quand le graphe est prêt à lancer ; sinon <see cref="Problems"/> dit pourquoi.</summary>
    [ObservableProperty]
    private bool _isValid;

    /// <summary>Le message « enregistré » qui suit un <see cref="Save"/> ; effacé dès la mutation suivante.</summary>
    [ObservableProperty]
    private string? _savedNotice;

    /// <summary>Choisir le point d'entrée le pose sur le brouillon, puis re-valide.</summary>
    partial void OnEntryStepChanged(string value)
    {
        if (_projecting)
            return;

        _draft.SetEntryStep(value ?? "");
        Project();
    }

    /// <summary>Ajoute une étape depuis son titre : le brouillon en slugifie et désambiguïse l'id (<c>D-021</c>).</summary>
    [RelayCommand]
    private void AddStep()
    {
        var title = NewStepTitle.Trim();
        if (title.Length == 0)
            return;

        _draft.AddStep(title);
        NewStepTitle = "";
        Project();
    }

    /// <summary>
    /// Enregistre le brouillon tel quel (brouillons permis, <c>D-019</c>) puis
    /// rafraîchit la liste de la surface. Rapatrie d'abord les champs de script de
    /// chaque ligne : filet contre le cas où le tout dernier champ édité n'aurait
    /// pas encore validé son binding avant le clic (les champs descendent d'ordinaire
    /// dans le brouillon dès la perte de focus, <see cref="UpdateScript"/>).
    /// </summary>
    [RelayCommand]
    private void Save()
    {
        FlushScripts();
        _catalog.Save(Id, _draft.ToDefinition());
        _onSaved();
        SavedNotice = "Workflow enregistré.";
    }

    /// <summary>Supprime une étape ; le brouillon purge les arêtes qui la visaient (<c>D-020</c>).</summary>
    public void RemoveStep(string id)
    {
        _draft.RemoveStep(id);
        Project();
    }

    /// <summary>
    /// Descend les champs de script d'une ligne dans le brouillon — appelé dès qu'un
    /// champ change (perte de focus). <b>Pas de re-projection</b> : re-projeter
    /// recréerait la ligne en cours d'édition et lui volerait le focus ; poser le
    /// script sur le brouillon suffit, il n'entre dans aucune règle de validation.
    /// Garde contre une ligne qui survivrait brièvement à la suppression de son
    /// étape (le binding ne doit alors plus rien poser).
    /// </summary>
    public void UpdateScript(string id, string fileName, string arguments)
    {
        if (!StepIds.Contains(id))
            return;

        _draft.SetScript(id, new ScriptSpec(fileName, SplitArguments(arguments)));
        SavedNotice = null;
    }

    /// <summary>Rapatrie les scripts de toutes les lignes dans le brouillon (filet d'enregistrement).</summary>
    private void FlushScripts()
    {
        foreach (var step in Steps)
            _draft.SetScript(step.Id, new ScriptSpec(step.FileName, SplitArguments(step.Arguments)));
    }

    /// <summary>
    /// Découpe la chaîne d'arguments aux espaces. <b>Simplification assumée</b> de
    /// l'éditeur minimal : un argument contenant une espace est hors de portée (le
    /// format JSON, lui, les distingue déjà token par token).
    /// </summary>
    private static IReadOnlyList<string> SplitArguments(string arguments) =>
        arguments.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    /// <summary>Trace une arête gardée depuis une étape ; cible permise même absente (le validateur signale).</summary>
    public void AddEdge(string fromId, string guardLabel, string target)
    {
        if (string.IsNullOrEmpty(target))
            return;

        _draft.AddEdge(fromId, GuardFromLabel(guardLabel), target);
        Project();
    }

    /// <summary>Retire une arête d'une étape par sa position (le rang de la ligne).</summary>
    public void RemoveEdge(string fromId, int index)
    {
        _draft.RemoveEdge(fromId, index);
        Project();
    }

    /// <summary>
    /// Reconstruit lignes et listes de choix depuis le brouillon, et re-valide.
    /// Le seul point qui traduit l'état du brouillon en surface — appelé après
    /// chaque mutation structurelle.
    /// </summary>
    private void Project()
    {
        // Garde de ré-entrance sur TOUTE la méthode. Vider puis regarnir StepIds fait
        // transiter le ComboBox d'entrée par une sélection nulle : lié en TwoWay, il
        // réécrit alors EntryStep="" et rappellerait OnEntryStepChanged → Project() en
        // pleine reconstruction des collections (crash « Collection was modified »).
        // Le drapeau neutralise cet aller-retour ; on le repose en finally.
        if (_projecting)
            return;

        _projecting = true;
        try
        {
            var definition = _draft.ToDefinition();
            var ids = definition.Steps.Select(step => step.Id).ToList();

            StepIds.Clear();
            foreach (var id in ids)
                StepIds.Add(id);

            // Après avoir regarni les choix : l'entrée retrouve son item dans la liste
            // (sinon le ComboBox, item disparu le temps du Clear, resterait sur nul).
            EntryStep = definition.EntryStep;

            Steps.Clear();
            foreach (var step in definition.Steps)
                Steps.Add(new StepEditorRow(this, step, ids));

            var report = WorkflowValidator.Validate(definition);
            IsValid = report.IsValid;
            Problems = report.IsValid
                ? "Graphe valide, prêt à lancer."
                : string.Join("\n", report.Issues.Select(issue => "• " + issue.Message));
            SavedNotice = null;
        }
        finally
        {
            _projecting = false;
        }
    }

    /// <summary>Traduit un libellé de garde de l'UI en fabrique <see cref="Guard"/> ; défaut prudent : succès.</summary>
    internal static Guard GuardFromLabel(string label) => label switch
    {
        "Échec" => Guard.OnFailure,
        "Toujours" => Guard.Default,
        _ => Guard.OnSuccess,
    };

    /// <summary>Le libellé d'une garde existante, pour l'afficher sur son arête.</summary>
    internal static string GuardLabel(Guard guard) => guard switch
    {
        Guard.FailureGuard => "Échec",
        Guard.AlwaysGuard => "Toujours",
        Guard.ExitCodeGuard exitCode => $"Code {exitCode.Code}",
        _ => "Succès",
    };
}
