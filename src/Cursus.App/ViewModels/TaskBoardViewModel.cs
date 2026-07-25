using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Cursus.Core.Projects;
using Cursus.Core.Tasks;

namespace Cursus.App.ViewModels;

/// <summary>
/// L'écran des tâches d'un projet : le tableau qu'il déclare viser, tel que le jeton de
/// ce poste le donne à voir. Quatrième module de la surface projet (liste ⇄ run ⇄ page
/// ⇄ tâches), sans routeur — <c>D-016</c>.
///
/// <para>
/// Sa seule vraie question est <b>quelle connexion interroger</b>, et elle a deux
/// moitiés qui ne vivent pas au même endroit : la <see cref="TrackerBinding"/> versionnée
/// dans le <c>project.json</c>, et les <see cref="TrackerConnection"/> du registre
/// machine qui portent les jetons. Les apparier peut donner quatre situations, et
/// <b>aucune n'est un détail</b> : c'est ici que se voit un dépôt qui vise un tableau
/// que ce poste ne sait pas joindre — l'erreur qui, sans cet écran, ne se manifesterait
/// qu'en déplaçant une carte au mauvais endroit.
/// </para>
///
/// <para>
/// ⚠️ Il ne nomme jamais Linear. La résolution passe par
/// <see cref="TrackerBinding.Matches"/> et <see cref="TrackerConnection.ToBinding"/>,
/// tous deux portés par les sous-types ; le tableau lui-même arrive par une fabrique.
/// Non testé, comme toute la vue (§7.12) : le seul comportement qui méritait une
/// garantie est cet appariement, verrouillé en Core.
/// </para>
/// </summary>
public partial class TaskBoardViewModel : ObservableObject
{
    private readonly TrackerRegistry _trackers;
    private readonly Func<TrackerConnection, ITaskBoard> _boardFor;
    private readonly Action _openSettings;

    // Muté par une déclaration : ProjectStore rend un projet frais, l'ancien est
    // immuable. L'instantané que le rail garde devient périmé — c'est sans danger
    // depuis que tout écrivain de project.json relit le disque avant d'écrire.
    private Project _project;

    // Vrai quand l'utilisateur a demandé à pointer ailleurs : le choix qui suit doit
    // alors réécrire la déclaration, et non seulement servir la session.
    private bool _redeclaring;

    public TaskBoardViewModel(
        Project project,
        TrackerRegistry trackers,
        Func<TrackerConnection, ITaskBoard> boardFor,
        Action openSettings)
    {
        _project = project;
        _trackers = trackers;
        _boardFor = boardFor;
        _openSettings = openSettings;
        Resolve();
    }

    /// <summary>Les connexions parmi lesquelles choisir, quand il y a lieu de choisir.</summary>
    public ObservableCollection<TrackerConnectionRow> Candidates { get; } = [];

    /// <summary>L'arbre du tableau : les projets du tracker, leurs tâches, les sous-tâches sous leur mère.</summary>
    public ObservableCollection<TaskProject> Projects { get; } = [];

    /// <summary>La connexion qui sert l'écran, ou <c>null</c> tant qu'aucune n'est arrêtée.</summary>
    [ObservableProperty]
    private TrackerConnection? _connection;

    /// <summary>Ce qu'on demande ou ce qu'on constate, selon la situation ; jamais une erreur technique.</summary>
    [ObservableProperty]
    private string? _message;

    /// <summary>Le refus du tableau à afficher — jeton révoqué, réseau absent —, ou <c>null</c>.</summary>
    [ObservableProperty]
    private string? _error;

    [ObservableProperty]
    private bool _isLoading;

    /// <summary>Vrai quand l'écran attend un choix : rien de déclaré, ou plusieurs jetons pour la même cible.</summary>
    public bool IsChoosing => Connection is null && Candidates.Count > 0;

    /// <summary>
    /// Vrai quand l'écran ne peut rien montrer et qu'il n'y a rien à choisir : aucune
    /// connexion sur ce poste, ou aucune qui desserve ce que le dépôt déclare. Le
    /// <see cref="Message"/> dit lequel des deux.
    /// </summary>
    public bool IsStuck => Connection is null && Candidates.Count == 0;

    /// <summary>Vrai quand une connexion est arrêtée : c'est le tableau qui occupe l'écran.</summary>
    public bool IsShowingBoard => Connection is not null;

    /// <summary>Le nom de la connexion qui sert — pour qu'on sache toujours quel jeton parle.</summary>
    public string ConnectionLabel => Connection?.Label ?? "";

    /// <summary>Vrai dès qu'un projet du tableau avoue avoir plus de tâches que la réponse n'en portait.</summary>
    public bool IsTruncated => Projects.Any(project => project.IsTruncated);

    /// <summary>
    /// Apparie ce que le dépôt déclare et ce que ce poste sait joindre, et arrête la
    /// situation de l'écran. Quatre issues, dans cet ordre de traitement :
    /// rien de déclaré · rien qui corresponde · une seule · plusieurs.
    /// </summary>
    private void Resolve()
    {
        Candidates.Clear();
        Projects.Clear();
        Connection = null;
        Error = null;

        if (_project.Tracker is not { } declaration || _redeclaring)
        {
            // Rien de déclaré : on ne demande pas de saisir un espace, on montre les
            // connexions et le choix vaudra déclaration (D-035 poussé d'un cran).
            Offer(_trackers.Connections);
            Message = Candidates.Count > 0
                ? "Sur quel tableau ce projet suit-il ses tickets ? Le choix sera inscrit dans son project.json, partagé avec l'équipe."
                : "Aucune connexion configurée sur ce poste. Ajoutez un jeton dans les réglages pour voir vos tâches ici.";
            NotifySituation();
            return;
        }

        var matching = _trackers.Connections.Where(declaration.Matches).ToList();
        var declared = TrackerBindingRow.For(declaration).Label;

        switch (matching.Count)
        {
            case 0:
                // La divergence, seule raison d'avoir versionné la déclaration : ce
                // dépôt vise un tableau que ce poste ne sait pas joindre, et il vaut
                // mieux l'apprendre ici qu'en déplaçant une carte ailleurs.
                Message = $"Ce dépôt suit {declared}, pour lequel ce poste n'a aucun jeton.";
                NotifySituation();
                return;

            case 1:
                Use(matching[0]);
                return;

            default:
                // Deux clés pour la même cible — une de compte, une de projet : le cas
                // même qui a fait renoncer à une clé de trousseau par espace (D-034).
                Offer(matching);
                Message = $"Plusieurs jetons desservent {declared}. Lequel utiliser ?";
                NotifySituation();
                return;
        }
    }

    private void Offer(IEnumerable<TrackerConnection> connections)
    {
        foreach (var connection in connections)
            Candidates.Add(TrackerConnectionRow.For(connection));
    }

    /// <summary>
    /// Arrête la connexion choisie. Si rien n'était déclaré — ou si l'on redéclare —,
    /// le choix s'<b>inscrit</b> : la déclaration est la conséquence d'un choix de
    /// connexion, jamais un formulaire à remplir.
    /// </summary>
    [RelayCommand]
    private void Choose(TrackerConnectionRow? row)
    {
        if (row is null)
            return;

        if (_project.Tracker is null || _redeclaring)
        {
            _project = ProjectStore.DeclareTracker(_project, row.Connection.ToBinding());
            _redeclaring = false;
        }

        Candidates.Clear();
        Use(row.Connection);
    }

    /// <summary>Fait pointer ce projet vers une autre connexion, en réécrivant sa déclaration.</summary>
    [RelayCommand]
    private void Redeclare()
    {
        _redeclaring = true;
        Resolve();
    }

    /// <summary>Le renvoi vers le panneau des connexions — l'issue d'une divergence.</summary>
    [RelayCommand]
    private void OpenSettings() => _openSettings();

    private void Use(TrackerConnection connection)
    {
        Connection = connection;
        Message = null;
        NotifySituation();

        // Le tableau se lit tout de suite : un écran qui attendrait un clic pour se
        // remplir ferait douter qu'il ait compris la connexion.
        _ = LoadCommand.ExecuteAsync(null);
    }

    /// <summary>
    /// Relit le tableau. Aucun sondage périodique : l'auto-déclenchement sur l'état
    /// d'une carte est une question ouverte à part entière (§7.10.6), et un écran qui
    /// interroge sans qu'on le lui demande brûle le quota d'API en silence.
    /// </summary>
    [RelayCommand]
    private async Task LoadAsync()
    {
        if (Connection is not { } connection)
            return;

        IsLoading = true;
        Error = null;

        try
        {
            var projects = await _boardFor(connection).ListProjectsAsync().ConfigureAwait(true);

            Projects.Clear();
            foreach (var project in projects)
                Projects.Add(project);

            OnPropertyChanged(nameof(IsTruncated));
        }
        catch (TrackerRejectedException refusal)
        {
            // Distinguer les deux vaut son code : on ne part pas vérifier son réseau
            // quand sa clé est révoquée (D-034).
            Error = refusal.Message;
        }
        catch (TrackerUnreachableException failure)
        {
            Error = failure.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void NotifySituation()
    {
        OnPropertyChanged(nameof(IsChoosing));
        OnPropertyChanged(nameof(IsStuck));
        OnPropertyChanged(nameof(IsShowingBoard));
        OnPropertyChanged(nameof(ConnectionLabel));
        OnPropertyChanged(nameof(IsTruncated));
    }
}
