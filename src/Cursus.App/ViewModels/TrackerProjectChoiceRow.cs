using CommunityToolkit.Mvvm.ComponentModel;

using Cursus.Core.Tasks;

namespace Cursus.App.ViewModels;

/// <summary>
/// Un projet du tracker, tel qu'on le coche pendant l'ajout d'une connexion.
/// Enveloppe mince sur <see cref="TaskProject"/> — immuable — pour loger la seule
/// chose qui varie ici : la case.
/// </summary>
public partial class TrackerProjectChoiceRow : ObservableObject
{
    public TrackerProjectChoiceRow(TaskProject project)
    {
        Id = project.Id;
        Name = project.Name;
    }

    public string Id { get; }

    public string Name { get; }

    [ObservableProperty]
    private bool _isSelected;
}
