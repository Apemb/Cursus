using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

using Cursus.App.ViewModels;

namespace Cursus.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    // Le sélecteur de dossier est une affaire de vue (il exige un TopLevel) ; le
    // ViewModel ne reçoit qu'un chemin et décide seul de l'inscrire ou de le
    // refuser. C'est la frontière posée par la décision de conception du loader.
    private async void OnAddProjectClick(object? sender, RoutedEventArgs e)
    {
        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Ouvrir un projet Cursus",
            AllowMultiple = false,
        });

        if (folders.Count == 0)
            return;

        var path = folders[0].TryGetLocalPath();
        if (path is not null && DataContext is ShellViewModel shell)
            shell.AddProject(path);
    }
}
