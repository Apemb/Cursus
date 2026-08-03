using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Cursus.App.ViewModels;
using Cursus.Application;
using Cursus.Core.Projects;
using Cursus.Core.Secrets;
using Cursus.Core.Tasks;
using Cursus.Persistence;
using Cursus.Trackers.Linear;

namespace Cursus.App;

// La classe de base est qualifiée : depuis le namespace Cursus.App, le simple nom
// « Application » se résout d'abord sur les membres de Cursus — donc sur le
// namespace Cursus.Application, jamais sur le using d'Avalonia. Sans la
// qualification, CS0118 (« namespace employé comme un type »).
public partial class App : Avalonia.Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Racine de composition du driver : le registre à son emplacement
            // machine par défaut, et le préréglage SQLite comme fabrique de hosts.
            // C'est l'unique endroit qui lie les deux mondes — les modules
            // reçoivent leurs dépendances construites, ils ne composent jamais.
            // Le trousseau et le registre des connexions vivent au-dessus des projets,
            // comme le registre des projets lui-même : un jeton n'appartient pas à un
            // dépôt. Le panneau reçoit une fabrique (trousseau, clé) → tableau — c'est
            // le seul endroit qui sait que le tracker est Linear.
            var secrets = new KeychainSecretStore();
            var trackers = TrackerRegistry.ForCurrentUser();

            desktop.MainWindow = new MainWindow
            {
                DataContext = new ShellViewModel(
                    ProjectRegistry.ForCurrentUser(),
                    project => new ProjectWorkspace(
                        SqliteProjectHost.Open(project),
                        new RunArtifactStore(project.ArtifactsRoot),
                        new WorkflowCatalog(project)),
                    () => new TrackerSettingsViewModel(
                        trackers,
                        secrets,
                        (store, key) => new LinearTaskBoard(store, key)),

                    // L'écran des tâches reçoit le registre des connexions et une
                    // fabrique (connexion) → tableau. La clé du trousseau se déduit de
                    // la connexion elle-même : la laisser composer ici, c'est risquer
                    // qu'un second appelant la compose autrement.
                    (project, openSettings) => new TaskBoardViewModel(
                        project,
                        trackers,
                        connection => new LinearTaskBoard(secrets, connection.SecretKey),
                        openSettings)),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}