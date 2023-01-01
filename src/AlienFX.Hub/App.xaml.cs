using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;

namespace AlienFX.Hub;
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private TaskbarIcon? _notifyIcon;

    /// <summary>
    /// Raises the <see cref="Application.Startup"/> event.
    /// <Remarks>This will also create the notification icon.</Remarks>
    /// </summary>
    /// <param name="e">A <see cref="StartupEventArgs"/> that contains the event data.</param>
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        _notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");
    }

    /// <summary>
    /// Raises the <see cref="Application.Exit"/> event.
    /// <Remarks>This will also dispose the notification icon.</Remarks>
    /// </summary>
    /// <param name="e">An <see cref="ExitEventArgs"/> that contains the event data.</param>
    protected override void OnExit(ExitEventArgs e)
    {
        _notifyIcon!.Dispose();
        base.OnExit(e);
    }
}
