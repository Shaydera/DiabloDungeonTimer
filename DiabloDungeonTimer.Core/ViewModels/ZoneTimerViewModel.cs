using System.Collections.ObjectModel;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using DiabloDungeonTimer.Core.Enums;
using DiabloDungeonTimer.Core.Models;
using DiabloDungeonTimer.Core.Services.Interfaces;
using DiabloDungeonTimer.Core.Utils;

namespace DiabloDungeonTimer.Core.ViewModels;

public sealed class ZoneTimerViewModel : WorkspaceViewModel
{
    private readonly ILogMonitorService _logMonitorService;
    private readonly DispatcherTimer _refreshTimer;

    private ZoneInfo? _currentZone;
    private string _lastError = string.Empty;

    public ZoneTimerViewModel(string displayName, ILogMonitorService? logMonitorService = null) : base(displayName)
    {
        _logMonitorService = logMonitorService ?? Ioc.Default.GetRequiredService<ILogMonitorService>();
        _logMonitorService.ZoneChange += OnZoneChanged;
        _refreshTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(125)
        };
        _refreshTimer.Tick += RefreshTimerOnTick;
        StartTimersCommand = new RelayCommand(OnStartTimers);
        StopTimersCommand = new RelayCommand(OnStopTimers);
        ZoneHistory = new ObservableCollection<ZoneInfo>();
    }

    public IRelayCommand StartTimersCommand { get; }
    public IRelayCommand StopTimersCommand { get; }

    public override string Error => _lastError;

    public override string this[string columnName]
    {
        get
        {
            var result = string.Empty;

            switch (columnName)
            {
                case nameof(CurrentZoneName):
                {
                    if (_currentZone == null)
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
    public string CurrentZoneName => _currentZone?.Name ?? "Waiting for Zone...";

    public string CurrentZoneTime
    {
        get
        {
            if (_currentZone == null)
                return string.Empty;
            DateTime compareTime = _currentZone.EndTime ?? DateTime.Now;
            TimeSpan span = compareTime.Subtract(_currentZone.StartTime);
            return span.ToDisplayString();
        }
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

    private void RefreshTimerOnTick(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(CurrentZoneTime));
    }

    private void OnZoneChanged(object? sender, ZoneChangeArgs e)
    {
        switch (e.ChangeType)
        {
            case ZoneChangeType.Entered:
                _currentZone = e.Zone;
                OnPropertyChanged(nameof(CurrentZoneName));
                OnPropertyChanged(nameof(CurrentZoneTime));
                break;
            case ZoneChangeType.Exited:
                ZoneHistory.Insert(0, e.Zone);
                break;
            default:
                throw new NotImplementedException();
        }
    }
}