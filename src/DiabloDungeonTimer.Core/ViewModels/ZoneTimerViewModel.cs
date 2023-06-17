using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using DiabloDungeonTimer.Core.Models;
using DiabloDungeonTimer.Core.Services;
using DiabloDungeonTimer.Core.Services.Interfaces;

namespace DiabloDungeonTimer.Core.ViewModels;

public sealed class ZoneTimerViewModel : WorkspaceViewModel
{
    private readonly string _historyFilename = Path.Combine(Directory.GetCurrentDirectory(), "ZoneHistory.xml");

    private readonly ILogMonitorService _logMonitorService;
    private readonly DispatcherTimer _refreshTimer;
    private readonly ISaveFileService _saveFileService;
    private readonly ISettingsProvider _settingsProvider;

    private string _lastError = string.Empty;

    public ZoneTimerViewModel(ZoneDataProvider? zoneDataProvider = null, ILogMonitorService? logMonitorService = null,
        ISettingsProvider? settingsProvider = null, ISaveFileService? saveFileService = null)
    {
        ZoneData = zoneDataProvider ?? Ioc.Default.GetRequiredService<ZoneDataProvider>();
        _logMonitorService = logMonitorService ?? Ioc.Default.GetRequiredService<ILogMonitorService>();
        _settingsProvider = settingsProvider ?? Ioc.Default.GetRequiredService<ISettingsProvider>();
        _saveFileService = saveFileService ?? Ioc.Default.GetRequiredService<ISaveFileService>();
        _refreshTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(125)
        };
        _refreshTimer.Tick += RefreshTimerOnTick;
        StartTimersCommand = new RelayCommand(OnStartTimers);
        StopTimersCommand = new RelayCommand(OnStopTimers);
        ClearHistoryCommand = new RelayCommand(OnClearHistory);
    }

    public IRelayCommand StartTimersCommand { get; }
    public IRelayCommand StopTimersCommand { get; }
    public IRelayCommand ClearHistoryCommand { get; }

    public override string Error => _lastError;

    public override string this[string columnName]
    {
        get
        {
            var result = string.Empty;

            switch (columnName)
            {
                case nameof(ZoneData.CurrentZone):
                {
                    if (ZoneData.CurrentZone == null || string.IsNullOrEmpty(ZoneData.CurrentZone.Name))
                        result = "Unknown Zone";
                    break;
                }
            }

            SetProperty(ref _lastError, result, nameof(Error));
            return result;
        }
    }

    public bool IsMonitoring => _logMonitorService.IsMonitoring();
    public ZoneDataProvider ZoneData { get; }

    private void OnStartTimers()
    {
        _logMonitorService.StartMonitor();
        _refreshTimer.Start();
        OnPropertyChanged(nameof(IsMonitoring));
    }

    private void OnStopTimers()
    {
        _logMonitorService.StopMonitor();
        _refreshTimer.Stop();
        OnPropertyChanged(nameof(IsMonitoring));
    }

    private void OnClearHistory()
    {
        ZoneData.Reset();
        if (ZoneData.CurrentZone is { EndTime: not null })
            ZoneData.CurrentZone = null;
    }

    private void RefreshTimerOnTick(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(ZoneData));
    }

    public async Task LoadHistoryAsync()
    {
        if (!_settingsProvider.Settings.KeepHistory)
            return;
        (bool, ObservableCollection<ZoneInfo>?) loadResult =
            await _saveFileService.TryLoadAsync<ObservableCollection<ZoneInfo>>(_historyFilename);
        if (loadResult is { Item1: false } || loadResult.Item2 == null)
            return;
        ZoneData.Populate(loadResult.Item2.OrderBy(info => info.StartTime));
    }

    public async Task SaveHistoryAsync()
    {
        if (_settingsProvider.Settings.KeepHistory)
            await _saveFileService.SaveAsync(ZoneData.ZoneHistory, _historyFilename);
    }
}