using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Cursus.App.ViewModels;
using Cursus.Core.Projects;

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
            // machine par défaut. Les modules le reçoivent construit, ils ne le
            // composent jamais eux-mêmes.
            desktop.MainWindow = new MainWindow
            {
                DataContext = new ShellViewModel(ProjectRegistry.ForCurrentUser()),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}