using System;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Cursus.Core.Workflows;

namespace Cursus.App.ViewModels;

/// <summary>
/// Une ligne de run : un <see cref="RunSummary"/> rendu en mots — verdict lisible et
/// date. Partagée par la liste des workflows (pour son <em>dernier</em> passage) et
/// par l'historique d'une page (pour <em>chaque</em> passage) : une seule règle de
/// libellé, un seul endroit. La ligne n'arbitre pas le résultat, elle le libelle —
/// le noyau distingue déjà les issues (parcours §4). Non testée, comme toute la vue
/// (§7.12).
/// </summary>
public partial class RunRowViewModel : ObservableObject
{
    private readonly Action? _reopen;

    /// <param name="reopen">
    /// Ce qu'il faut faire pour rouvrir ce passage en relecture ; <c>null</c> pour une
    /// ligne qui n'est qu'un libellé (le dernier passage d'un workflow dans la liste).
    /// </param>
    public RunRowViewModel(RunSummary run, Action? reopen = null)
    {
        Run = run;
        _reopen = reopen;
    }

    /// <summary>Le résumé du run que cette ligne libelle.</summary>
    public RunSummary Run { get; }

    /// <summary>Le verdict lisible du passage (« Réussi », « Échoué », …).</summary>
    public string Verdict => FormatVerdict(Run);

    /// <summary>L'instant du passage, en clair et en heure locale.</summary>
    public string When => FormatWhen(Run);

    /// <summary>Le passage en une phrase — « Échoué le 22/07 à 18:04 ».</summary>
    public string Label => $"{Verdict} {When}";

    /// <summary>Vrai quand la ligne sait rouvrir son run — masque le geste sur une ligne purement descriptive.</summary>
    public bool CanReopen => _reopen is not null;

    /// <summary>Rouvre ce passage en relecture ; sans effet si la ligne n'a pas de quoi rouvrir.</summary>
    [RelayCommand]
    private void Reopen() => _reopen?.Invoke();

    /// <summary>
    /// Le verdict lisible d'un run. <c>Failed</c> est déjà posé par le moteur quand
    /// l'étape terminale échoue sans arête de secours ; « Arrêté » n'est pas
    /// « Échoué », le noyau les sépare en <c>Aborted/Canceled</c>.
    /// </summary>
    public static string FormatVerdict(RunSummary run) => run.State switch
    {
        RunState.Completed => "Réussi",
        RunState.Failed => "Échoué",
        RunState.Aborted when run.AbortReason == AbortReason.Canceled => "Arrêté",
        RunState.Aborted when run.AbortReason == AbortReason.Faulted => "Planté",
        RunState.Aborted => "Échoué", // boucle non convergente : un échec, pas un arrêt voulu
        _ => "En cours",
    };

    /// <summary>L'instant à afficher : la fin si le run est clos, sinon son départ.</summary>
    public static string FormatWhen(RunSummary run) =>
        (run.EndedAt ?? run.StartedAt).ToLocalTime().ToString("'le' dd/MM 'à' HH:mm");
}
