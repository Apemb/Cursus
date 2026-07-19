using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Cursus.App;

public partial class MainWindow : Window
{
    private int _count;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnIncrementClick(object? sender, RoutedEventArgs e)
    {
        _count++;
        CounterText.Text = $"Compteur : {_count}";
    }
}
