using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using DiabloDungeonTimer.Core.Common;
using DiabloDungeonTimer.Core.Enums;
using DiabloDungeonTimer.Core.Models;
using DiabloDungeonTimer.Core.Services.Interfaces;

namespace DiabloDungeonTimer.Core.Services;

public class ZoneDataProvider : ObservableObject
{
    private readonly ILogMonitorService _logMonitorService;

    private ZoneInfo? _currentZoneInfo;

    public ZoneDataProvider(ILogMonitorService? logMonitorService = null)
    {
        _logMonitorService = logMonitorService ?? Ioc.Default.GetRequiredService<ILogMonitorService>();
        _logMonitorService.ZoneChange += OnZoneChanged;
        ZoneHistory.CollectionChanged += ZoneHistoryOnCollectionChanged;
    }

    public ZoneInfo? CurrentZone
    {
        get => _currentZoneInfo;
        set => SetProperty(ref _currentZoneInfo, value);
    }

    public ObservableCollection<ZoneStatistic> ZoneStatistics { get; } = new();
    public ObservableCollection<ZoneInfo> ZoneHistory { get; } = new();

    private void ZoneHistoryOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e is { Action: NotifyCollectionChangedAction.Add, NewItems.Count: > 0 })
            OnZoneAdded((ZoneInfo)e.NewItems[0]!);
    }

    public void Populate(IEnumerable<ZoneInfo> zoneData)
    {
        Reset();
        foreach (ZoneInfo zoneInfo in zoneData)
            AddZone(zoneInfo);
    }

    private void OnZoneAdded(ZoneInfo zoneInfo)
    {
        if (!ZoneStatistics.Any(x => x.Zone.Equals(zoneInfo.Zone)))
            ZoneStatistics.Add(new ZoneStatistic(zoneInfo.Zone));
        ZoneStatistic zoneStatistic = ZoneStatistics.Single(x => x.Zone.Equals(zoneInfo.Zone));
        zoneStatistic.Count++;
        if (zoneStatistic.FastestTime == TimeSpan.Zero || zoneInfo.Duration < zoneStatistic.FastestTime)
            zoneStatistic.FastestTime = zoneInfo.Duration;
        if (zoneStatistic.SlowestTime == TimeSpan.Zero || zoneInfo.Duration > zoneStatistic.SlowestTime)
            zoneStatistic.SlowestTime = zoneInfo.Duration;
        zoneStatistic.AverageTime =
            TimeSpan.FromTicks((zoneStatistic.AverageTime.Ticks * (zoneStatistic.Count - 1) + zoneInfo.Duration.Ticks) /
                               zoneStatistic.Count);
    }

    private void AddZone(ZoneInfo zoneInfo)
    {
        ZoneHistory.Insert(0, zoneInfo);
    }

    private void OnZoneChanged(object? sender, ZoneChangeArgs e)
    {
        switch (e.ChangeType)
        {
            case ZoneChangeType.Entered:
                CurrentZone = e.Zone;
                break;
            case ZoneChangeType.Exited:
                AddZone(e.Zone);
                break;
            default:
                throw new NotImplementedException();
        }
    }

    public void Reset()
    {
        ZoneStatistics.Clear();
        ZoneHistory.Clear();
    }
}