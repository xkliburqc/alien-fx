using System.Windows;
using AlienFX.Hub.ViewModels;

namespace AlienFX.Hub;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindowViewModel ViewModel { get; } = new();

    public MainWindow()
    {
        InitializeComponent();

        ViewModel.Initialize();
        ViewModel.RegisterHotKeys();

        DataContext = ViewModel;
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        base.OnClosing(e);
        e.Cancel = true;
        Hide();
    }
}
