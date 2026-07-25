using CommunityToolkit.Mvvm.ComponentModel;

using Cursus.Core.Tasks;

namespace Cursus.App.ViewModels;

/// <summary>
/// Une connexion en cours d'ajout. Un objet plutôt que cinq propriétés éparses sur le
/// panneau, parce que l'ajout a des <b>états</b> — jeton à coller, épreuve en cours,
/// espace constaté, refus — qui se lisent mieux ensemble.
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

    /// <summary>Vrai pendant l'épreuve du jeton.</summary>
    [ObservableProperty]
    private bool _isProbing;

    /// <summary>Le refus à afficher, ou <c>null</c> si rien n'a encore échoué.</summary>
    [ObservableProperty]
    private string? _error;

    /// <summary>
    /// L'espace que le jeton dessert, une fois constaté ; <c>null</c> tant qu'il ne
    /// l'a pas été. Il ne se choisit pas : une clé Linear est attachée à exactement un
    /// espace — l'épreuve du jeton est donc aussi ce qui révèle le périmètre.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasProbed))]
    [NotifyPropertyChangedFor(nameof(WorkspaceLabel))]
    private TrackerWorkspace? _workspace;

    /// <summary>Vrai une fois le jeton éprouvé : l'enregistrement n'est offert qu'à partir de là.</summary>
    public bool HasProbed => Workspace is not null;

    /// <summary>L'espace en clair, pour que l'utilisateur voie à quoi il vient de se connecter.</summary>
    public string WorkspaceLabel =>
        Workspace is { } workspace ? $"{workspace.Name} ({workspace.Key})" : "";
}
