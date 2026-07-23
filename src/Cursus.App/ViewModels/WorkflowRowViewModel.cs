using System;

using Cursus.Core.Projects;
using Cursus.Core.Workflows;

namespace Cursus.App.ViewModels;

/// <summary>
/// Une ligne de la liste des workflows : son nom, et la trace de son dernier
/// passage. C'est le seul endroit qui traduit l'issue d'un run en mots — l'écran
/// arbitre le résultat, il ne recopie pas <see cref="RunState"/> (parcours §4).
/// Non testé, comme toute la vue (§7.12) : le noyau distingue déjà les issues,
/// il ne reste ici qu'un choix de libellés.
/// </summary>
public sealed class WorkflowRowViewModel
{
    private readonly RunSummary? _lastRun;

    public WorkflowRowViewModel(WorkflowLastRun workflow)
    {
        Name = workflow.Workflow.Id;
        _lastRun = workflow.LastRun;
    }

    /// <summary>Le nom du workflow (son fichier), en tête de ligne.</summary>
    public string Name { get; }

    /// <summary>
    /// Le dernier passage en une phrase — « Échoué le 22/07 à 18:04 », ou
    /// « Jamais lancé » quand rien n'a encore tourné.
    /// </summary>
    public string LastPassage => _lastRun is null ? "Jamais lancé" : $"{Verdict(_lastRun)} {When(_lastRun)}";

    /// <summary>
    /// Le verdict lisible d'un run. <c>Failed</c> est déjà posé par le moteur
    /// quand l'étape terminale échoue sans arête de secours ; « Arrêté » n'est pas
    /// « Échoué », le noyau les sépare en <c>Aborted/Canceled</c>.
    /// </summary>
    private static string Verdict(RunSummary run) => run.State switch
    {
        RunState.Completed => "Réussi",
        RunState.Failed => "Échoué",
        RunState.Aborted when run.AbortReason == AbortReason.Canceled => "Arrêté",
        RunState.Aborted when run.AbortReason == AbortReason.Faulted => "Planté",
        RunState.Aborted => "Échoué", // boucle non convergente : un échec, pas un arrêt voulu
        _ => "En cours",
    };

    private static string When(RunSummary run) =>
        (run.EndedAt ?? run.StartedAt).ToLocalTime().ToString("'le' dd/MM 'à' HH:mm");
}
