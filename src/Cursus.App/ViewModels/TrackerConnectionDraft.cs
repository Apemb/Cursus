using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;

namespace Cursus.App.ViewModels;

/// <summary>
/// Une connexion en cours d'ajout. Un objet plutôt que six propriétés éparses sur le
/// panneau, parce que l'ajout a des <b>états</b> — jeton à coller, découverte en
/// cours, portée à cocher, refus — qui se lisent mieux ensemble.
///
/// <para>
/// ⚠️ Le jeton reste <b>en mémoire</b> jusqu'à l'enregistrement, et n'est rangé au
/// trousseau qu'une fois la connexion inscrite, sous sa clé définitive. Le ranger
/// d'abord obligerait, en cas d'abandon ou de refus, à revenir effacer un secret
/// devenu orphelin — un nettoyage qu'on oublie une fois sur deux.
/// </para>
/// </summary>
public partial class TrackerConnectionDraft : ObservableObject
{
    /// <summary>Le jeton collé. Jamais réaffiché une fois la connexion enregistrée.</summary>
    [ObservableProperty]
    private string _token = "";

    /// <summary>Le nom que portera la connexion dans la liste.</summary>
    [ObservableProperty]
    private string _label = "";

    /// <summary>Vrai pendant l'interrogation du tracker.</summary>
    [ObservableProperty]
    private bool _isProbing;

    /// <summary>Le refus à afficher, ou <c>null</c> si rien n'a encore échoué.</summary>
    [ObservableProperty]
    private string? _error;

    /// <summary>
    /// Vrai une fois le jeton éprouvé : la portée n'est proposée qu'à partir de là,
    /// puisqu'on ne peut pas cocher des projets avant de savoir lesquels le jeton voit.
    /// </summary>
    [ObservableProperty]
    private bool _hasProbed;

    /// <summary>
    /// Vrai quand la connexion doit couvrir tout ce que le jeton voit. Décoché, ce
    /// sont les cases individuelles qui décident.
    /// </summary>
    [ObservableProperty]
    private bool _coversEverything = true;

    /// <summary>Les projets que ce jeton donne à voir — constatés, jamais déclarés.</summary>
    public ObservableCollection<TrackerProjectChoiceRow> Projects { get; } = [];
}
