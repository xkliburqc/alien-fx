using System.Windows;
using AlienFX.Hub.ViewModels;

namespace AlienFX.Hub;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// The <c>ViewModel</c> of the main window.
    /// </summary>
    public MainWindowViewModel ViewModel { get; } = new();

    /// <summary>
    /// This constructor initialize the main window and register hotkeys.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
        ViewModel.Initialize();
        DataContext = ViewModel;
    }

    /// <summary>
    /// This event override the closing mechanism to use <see cref="Window.Hide"/> instead.
    /// </summary>
    /// <param name="e"></param>
    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        base.OnClosing(e);
        e.Cancel = true;
        Hide();
    }
}
