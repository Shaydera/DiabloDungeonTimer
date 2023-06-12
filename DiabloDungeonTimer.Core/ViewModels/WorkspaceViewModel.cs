using System.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DiabloDungeonTimer.Core.ViewModels;

/// <summary>
///     Expands the ViewModelBase functionality with IDataErrorInfo and a default close command.
/// </summary>
public abstract class WorkspaceViewModel : ViewModelBase, IDataErrorInfo
{
    private bool _closeEnabled = true;
    private bool _inputEnabled = true;

    protected WorkspaceViewModel(string displayName) : base(displayName)
    {
        CloseCommand = new RelayCommand(OnRequestClose);
    }

    public IRelayCommand CloseCommand { get; }

    public bool CloseEnabled
    {
        get => _closeEnabled;
        protected set => SetProperty(ref _closeEnabled, value);
    }

    public bool InputEnabled
    {
        get => _inputEnabled;
        protected set => SetProperty(ref _inputEnabled, value);
    }

    public abstract string Error { get; }

    public abstract string this[string columnName] { get; }


    public event EventHandler? RequestClose;

    private void OnRequestClose()
    {
        RequestClose?.Invoke(this, EventArgs.Empty);
    }
}