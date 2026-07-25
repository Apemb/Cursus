using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Cursus.Core.Secrets;
using Cursus.Core.Tasks;

namespace Cursus.App.ViewModels;

/// <summary>
/// Le panneau des connexions tracker : ce que cette machine sait joindre, et le flux
/// d'ajout d'une connexion de plus.
///
/// <para>
/// Il appartient à la coquille et non à la surface d'un projet : un jeton dessert des
/// projets <em>du tracker</em>, pas un projet Cursus — le loger dans la surface
/// laisserait croire qu'on le ressaisit à chaque dépôt.
/// </para>
///
/// <para>
/// Il ignore tout de Linear : la racine de composition lui remet une fabrique
/// (trousseau, clé) → tableau. Le jour où un second tracker arrive, ce panneau ne
/// change pas.
/// </para>
/// </summary>
public partial class TrackerSettingsViewModel : ObservableObject
{
    private readonly TrackerRegistry _registry;
    private readonly ISecretStore _secrets;
    private readonly Func<ISecretStore, string, ITaskBoard> _boardFor;

    public TrackerSettingsViewModel(
        TrackerRegistry registry,
        ISecretStore secrets,
        Func<ISecretStore, string, ITaskBoard> boardFor)
    {
        _registry = registry;
        _secrets = secrets;
        _boardFor = boardFor;
        Connections = new ObservableCollection<TrackerConnectionRow>(
            registry.Connections.Select(connection => new TrackerConnectionRow(connection)));
    }

    public ObservableCollection<TrackerConnectionRow> Connections { get; }

    /// <summary>Vrai quand aucune connexion n'est configurée — l'état d'une installation neuve.</summary>
    public bool HasNoConnection => Connections.Count == 0;

    /// <summary>L'ajout en cours, ou <c>null</c> quand le panneau ne montre que la liste.</summary>
    [ObservableProperty]
    private TrackerConnectionDraft? _draft;

    [RelayCommand]
    private void BeginAdd() => Draft = new TrackerConnectionDraft();

    [RelayCommand]
    private void CancelAdd() => Draft = null;

    /// <summary>
    /// Éprouve le jeton collé et montre ce qu'il donne à voir. C'est l'étape qui rend
    /// l'ajout possible : la portée d'une clé n'est pas déclarable — Linear en délivre
    /// qui couvrent le compte et d'autres un seul projet —, elle se constate en
    /// interrogeant.
    /// </summary>
    [RelayCommand]
    private async Task ProbeAsync()
    {
        if (Draft is not { } draft)
            return;

        // Un retour à la ligne collé avec le presse-papiers casserait l'en-tête HTTP
        // sans rien dire d'exploitable.
        var token = draft.Token.Trim();
        if (token.Length == 0)
        {
            draft.Error = "Collez d'abord un jeton.";
            return;
        }

        draft.IsProbing = true;
        draft.Error = null;

        try
        {
            // Le jeton n'est pas encore rangé : on l'éprouve depuis un trousseau de
            // passage, pour n'avoir rien à reprendre s'il est refusé.
            var board = _boardFor(new TransientSecretStore(token), "");
            var projects = await board.ListProjectsAsync().ConfigureAwait(true);

            draft.Projects.Clear();
            foreach (var project in projects)
                draft.Projects.Add(new TrackerProjectChoiceRow(project));

            draft.HasProbed = true;

            if (draft.Label.Length == 0)
                draft.Label = projects.Count == 1 ? projects[0].Name : "Linear";
        }
        catch (TrackerRejectedException refusal)
        {
            draft.Error = refusal.Message;
        }
        catch (TrackerUnreachableException failure)
        {
            draft.Error = failure.Message;
        }
        finally
        {
            draft.IsProbing = false;
        }
    }

    /// <summary>
    /// Inscrit la connexion, puis range son jeton sous la clé que l'inscription vient
    /// de lui donner — dans cet ordre : la clé n'existe qu'une fois l'identifiant
    /// attribué.
    /// </summary>
    [RelayCommand]
    private async Task SaveAsync()
    {
        if (Draft is not { HasProbed: true } draft)
            return;

        var scope = draft.CoversEverything
            ? new TrackerScope.WholeWorkspace()
            : (TrackerScope)new TrackerScope.SelectedProjects(
                [.. draft.Projects.Where(project => project.IsSelected).Select(project => project.Id)]);

        var label = draft.Label.Trim();
        var connection = _registry.Add(label.Length == 0 ? "Linear" : label, scope);

        try
        {
            await _secrets.WriteAsync(connection.SecretKey, draft.Token.Trim()).ConfigureAwait(true);
        }
        catch (InvalidOperationException failure)
        {
            // Une connexion inscrite dont le jeton n'a pas pu être rangé serait une
            // ligne muette qui échoue à chaque usage : on défait l'inscription.
            _registry.Remove(connection.Id);
            draft.Error = failure.Message;
            return;
        }

        Connections.Add(new TrackerConnectionRow(connection));
        OnPropertyChanged(nameof(HasNoConnection));
        Draft = null;
    }

    /// <summary>Oublie une connexion — et le jeton qu'elle désignait, sans quoi il resterait orphelin.</summary>
    [RelayCommand]
    private async Task RemoveAsync(TrackerConnectionRow? row)
    {
        if (row is null)
            return;

        _registry.Remove(row.Connection.Id);
        await _secrets.DeleteAsync(row.Connection.SecretKey).ConfigureAwait(true);

        Connections.Remove(row);
        OnPropertyChanged(nameof(HasNoConnection));
    }
}
