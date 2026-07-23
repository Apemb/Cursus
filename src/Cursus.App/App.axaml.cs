using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Cursus.App.ViewModels;
using Cursus.Core.Projects;
using Cursus.Persistence;

namespace Cursus.App;

public partial class App : Application
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
            desktop.MainWindow = new MainWindow
            {
                DataContext = new ShellViewModel(
                    ProjectRegistry.ForCurrentUser(),
                    project => new ProjectWorkspace(
                        SqliteProjectHost.Open(project),
                        new RunArtifactStore(project.ArtifactsRoot))),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}