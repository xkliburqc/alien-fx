using System.Windows;
using System.Windows.Input;
using AlienFX.Hub.Shared;

namespace AlienFX.Hub.ViewModels;

/// <summary>
/// Class <c>NotifyIconViewModel</c> models a notification icon.
/// </summary>
public class NotifyIconViewModel
{
    /// <summary>
    /// Gets the notification icon command that show the main window.
    /// </summary>
    public ICommand ShowWindowCommand { get; } = new DelegateCommand
    {
        CanExecuteFunc = () => Application.Current.MainWindow == null || !Application.Current.MainWindow.IsVisible,
        CommandAction = () =>
        {
            Application.Current.MainWindow.Show();
        }
    };

    /// <summary>
    /// Gets the notification icon command that hide the main window.
    /// </summary>
    public ICommand HideWindowCommand { get; } = new DelegateCommand
    {
        CommandAction = () => Application.Current.MainWindow.Hide(),
        CanExecuteFunc = () => Application.Current.MainWindow != null && Application.Current.MainWindow.IsVisible
    };

    /// <summary>
    /// Gets the notification icon command that exit the application.
    /// </summary>
    public ICommand ExitApplicationCommand { get; } = new DelegateCommand { CommandAction = () => Application.Current.Shutdown() };
}
