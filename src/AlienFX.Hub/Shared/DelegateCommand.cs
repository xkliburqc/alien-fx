using System;
using System.Windows.Input;

namespace AlienFX.Hub.Shared;
internal class DelegateCommand : ICommand
{
    public Action? CommandAction { get; set; }
    public Func<bool>? CanExecuteFunc { get; set; }
    

    public bool CanExecute(object? parameter) => CanExecuteFunc == null || CanExecuteFunc();

    public void Execute(object? parameter) => CommandAction!();

    public event EventHandler? CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }
}
