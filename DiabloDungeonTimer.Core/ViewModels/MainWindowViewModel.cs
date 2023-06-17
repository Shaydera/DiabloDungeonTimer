using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Data;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using DiabloDungeonTimer.Core.Services.Interfaces;

namespace DiabloDungeonTimer.Core.ViewModels;

/// <summary>
///     ViewModel for a MDI Parent Window, which manages creation and display of all MDI Children
/// </summary>
public sealed class MainWindowViewModel : WorkspaceViewModel
{
    private readonly ISettingsService _settingsService;

    public MainWindowViewModel(ISettingsService? settingsService = null)
    {
        _settingsService = settingsService ?? Ioc.Default.GetRequiredService<ISettingsService>();
        Workspaces = new ObservableCollection<WorkspaceViewModel>();
        Workspaces.CollectionChanged += OnWorkspacesChanged;
        CollectionViewSource.GetDefaultView(Workspaces).CurrentChanging += CurrentWorkspaceChanging;
        CollectionViewSource.GetDefaultView(Workspaces).CurrentChanged += CurrentWorkspaceChanged;
        StartupCommand = new AsyncRelayCommand(OnStartupAsync);
        ChangeConfigCommand = new RelayCommand(ChangeConfiguration);
    }

    private ObservableCollection<WorkspaceViewModel> Workspaces { get; }

    public WorkspaceViewModel CurrentWorkspace
    {
        get
        {
            if (CollectionViewSource.GetDefaultView(Workspaces)?.CurrentItem is WorkspaceViewModel viewModel)
                return viewModel;
            return this;
        }
    }

    public IAsyncRelayCommand StartupCommand { get; }

    public IRelayCommand ChangeConfigCommand { get; }

    public override string Error => throw new NotImplementedException();

    public override string this[string columnName] => throw new NotImplementedException();

    private void CurrentWorkspaceChanging(object? sender, CurrentChangingEventArgs e)
    {
        e.Cancel = false;
        if (e.IsCancelable && CurrentWorkspace is ConfigurationViewModel)
            e.Cancel = _settingsService.IsValid();
        if (CurrentWorkspace is ZoneTimerViewModel timerVm)
            timerVm.StopTimersCommand.Execute(null);
    }

    private void CurrentWorkspaceChanged(object? sender, EventArgs e)
    {
        if (CurrentWorkspace is ZoneTimerViewModel timerVm && _settingsService.GameDirectoryValid())
            timerVm.StartTimersCommand.Execute(null);
        OnPropertyChanged(nameof(CurrentWorkspace));
    }

    private void OnWorkspacesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is { Count: > 0 })
            foreach (WorkspaceViewModel workspace in e.NewItems)
                workspace.RequestClose += OnWorkspaceRequestClose;

        if (e.OldItems is { Count: > 0 })
            foreach (WorkspaceViewModel workspace in e.OldItems)
                workspace.RequestClose -= OnWorkspaceRequestClose;

        if (!Workspaces.Any())
            CloseCommand.Execute(null);
    }

    private void OnWorkspaceRequestClose(object? sender, EventArgs e)
    {
        if (sender is not WorkspaceViewModel workspace)
            throw new ArgumentNullException(nameof(sender),
                $"{nameof(OnWorkspaceRequestClose)}: {nameof(sender)} is null or not a valid {nameof(WorkspaceViewModel)}.");

        if (workspace is ConfigurationViewModel && !_settingsService.IsValid())
            return;

        Workspaces.Remove(workspace);
    }

    private void ChangeConfiguration()
    {
        WorkspaceViewModel? configWorkspace =
            Workspaces.SingleOrDefault(workspace => workspace is ConfigurationViewModel);
        if (configWorkspace == null)
        {
            configWorkspace = Ioc.Default.GetService<ConfigurationViewModel>() ??
                              new ConfigurationViewModel(_settingsService);
            Workspaces.Add(configWorkspace);
        }

        SetActiveWorkspace(configWorkspace);
    }

    private async Task ShowTimer()
    {
        WorkspaceViewModel? timerWorkspace =
            Workspaces.SingleOrDefault(workspace => workspace is ZoneTimerViewModel);
        if (timerWorkspace == null)
        {
            timerWorkspace = Ioc.Default.GetService<ZoneTimerViewModel>() ??
                             new ZoneTimerViewModel(null, _settingsService);
            await (timerWorkspace as ZoneTimerViewModel)!.LoadHistoryAsync();
            Workspaces.Add(timerWorkspace);
        }

        SetActiveWorkspace(timerWorkspace);
    }


    private void SetActiveWorkspace(WorkspaceViewModel workspace)
    {
        Debug.Assert(Workspaces.Contains(workspace));
        ICollectionView? workspaceView = CollectionViewSource.GetDefaultView(Workspaces);
        workspaceView?.MoveCurrentTo(workspace);
    }

    private async Task OnStartupAsync()
    {
        await ShowTimer();
        if (!_settingsService.IsValid())
            ChangeConfiguration();
    }
}