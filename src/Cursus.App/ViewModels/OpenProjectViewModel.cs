using System;
using System.Collections.Generic;
using System.Linq;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Cursus.Core.Projects;

namespace Cursus.App.ViewModels;

/// <summary>
/// Un projet ouvert, tel qu'il occupe la surface de droite. Il tient deux
/// contenus d'une même surface — <b>sans routeur</b> : la liste de ses workflows,
/// ou l'écran du run courant quand on en lance (ou rouvre) un. Il accueillera plus
/// tard le sélecteur run/sessions et l'engrenage de configuration — d'où ce nom,
/// qui désigne le conteneur d'un projet ouvert et non le seul mode run.
///
/// <para>
/// Adaptateur mince : la jointure workflows × runs vit dans <see cref="ProjectHost"/>
/// (Core), déjà testée ; ici on ne fait que binder, et déléguer le montage d'un
/// <see cref="RunViewModel"/> à des fabriques reçues (la coquille les câble sur le
/// host et le magasin d'artefacts du projet). Non testé, comme toute la vue (§7.12).
/// </para>
/// </summary>
public partial class OpenProjectViewModel : ObservableObject
{
    private readonly Func<string, RunViewModel> _startRun;
    private readonly Func<WorkflowRowViewModel, RunViewModel> _openPastRun;

    public OpenProjectViewModel(
        string name,
        IReadOnlyList<WorkflowLastRun> workflows,
        Func<string, RunViewModel> startRun,
        Func<WorkflowRowViewModel, RunViewModel> openPastRun)
    {
        Name = name;
        _startRun = startRun;
        _openPastRun = openPastRun;
        Workflows = workflows.Select(workflow => new WorkflowRowViewModel(workflow)).ToList();
    }

    /// <summary>Le libellé du projet, affiché en tête de la surface.</summary>
    public string Name { get; }

    /// <summary>Les workflows du projet, chacun avec son dernier passage.</summary>
    public IReadOnlyList<WorkflowRowViewModel> Workflows { get; }

    /// <summary>
    /// L'écran du run occupant la surface, ou <c>null</c> quand la surface montre
    /// la liste. Un seul à la fois — la liste ou le run, pas les deux.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsShowingRun))]
    private RunViewModel? _currentRun;

    /// <summary>Vrai quand le run occupe la surface ; pilote le basculement liste ⇄ run.</summary>
    public bool IsShowingRun => CurrentRun is not null;

    /// <summary>Lance le workflow d'une ligne et ouvre son run vif sur la surface.</summary>
    [RelayCommand]
    private void StartRun(WorkflowRowViewModel? row)
    {
        if (row is null)
            return;

        SwapRun(_startRun(row.Name));
    }

    /// <summary>Rouvre le dernier passage d'une ligne en relecture — même écran, figé.</summary>
    [RelayCommand]
    private void OpenPastRun(WorkflowRowViewModel? row)
    {
        if (row is null || !row.HasLastRun)
            return;

        SwapRun(_openPastRun(row));
    }

    /// <summary>Referme le run et revient à la liste.</summary>
    [RelayCommand]
    private void CloseRun() => SwapRun(null);

    private void SwapRun(RunViewModel? next)
    {
        CurrentRun?.Dispose();
        CurrentRun = next;
    }
}
