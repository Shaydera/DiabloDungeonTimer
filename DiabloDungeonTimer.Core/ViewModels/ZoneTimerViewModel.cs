using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using DiabloDungeonTimer.Core.Enums;
using DiabloDungeonTimer.Core.Models;
using DiabloDungeonTimer.Core.Services.Interfaces;

namespace DiabloDungeonTimer.Core.ViewModels;

public sealed class ZoneTimerViewModel : WorkspaceViewModel
{
    private readonly string _historyFilename = Path.Combine(Directory.GetCurrentDirectory(), "ZoneHistory.xml");

    private readonly ILogMonitorService _logMonitorService;
    private readonly DispatcherTimer _refreshTimer;
    private readonly ISaveFileService _saveFileService;
    private readonly ISettingsService _settingsService;

    private ZoneInfo? _currentZone;
    private string _lastError = string.Empty;

    public ZoneTimerViewModel(ILogMonitorService? logMonitorService = null,
        ISettingsService? settingsService = null, ISaveFileService? saveFileService = null)
    {
        _logMonitorService = logMonitorService ?? Ioc.Default.GetRequiredService<ILogMonitorService>();
        _logMonitorService.ZoneChange += OnZoneChanged;
        _settingsService = settingsService ?? Ioc.Default.GetRequiredService<ISettingsService>();
        _saveFileService = saveFileService ?? Ioc.Default.GetRequiredService<ISaveFileService>();
        _refreshTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(125)
        };
        _refreshTimer.Tick += RefreshTimerOnTick;
        ZoneHistory = new ObservableCollection<ZoneInfo>();
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
                case nameof(CurrentZone):
                {
                    if (CurrentZone == null || string.IsNullOrEmpty(CurrentZone.Name))
                        result = "Unknown Zone";
                    break;
                }
            }

            SetProperty(ref _lastError, result, nameof(Error));
            return result;
        }
    }

    public bool IsMonitoring => _logMonitorService.IsMonitoring();
    public ObservableCollection<ZoneInfo> ZoneHistory { get; }

    public ZoneInfo? CurrentZone
    {
        get => _currentZone;
        set => SetProperty(ref _currentZone, value);
    }

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
        ZoneHistory.Clear();
        if (CurrentZone is { EndTime: not null })
            CurrentZone = null;
    }

    private void RefreshTimerOnTick(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(CurrentZone));
    }

    private void OnZoneChanged(object? sender, ZoneChangeArgs e)
    {
        switch (e.ChangeType)
        {
            case ZoneChangeType.Entered:
                CurrentZone = e.Zone;
                break;
            case ZoneChangeType.Exited:
                ZoneHistory.Insert(0, e.Zone);
                break;
            default:
                throw new NotImplementedException();
        }
    }

    public async Task LoadHistoryAsync()
    {
        if (!_settingsService.Settings.KeepHistory)
            return;
        (bool, ObservableCollection<ZoneInfo>?) loadResult =
            await _saveFileService.TryLoadAsync<ObservableCollection<ZoneInfo>>(_historyFilename);
        if (loadResult is { Item1: false } || loadResult.Item2 == null)
            return;
        ZoneHistory.Clear();
        foreach (ZoneInfo zoneInfo in loadResult.Item2.OrderBy(info => info.StartTime).Reverse())
            ZoneHistory.Add(zoneInfo);
    }

    public async Task SaveHistoryAsync()
    {
        if (_settingsService.Settings.KeepHistory)
            await _saveFileService.SaveAsync(ZoneHistory, _historyFilename);
    }
}