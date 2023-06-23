using CommunityToolkit.Mvvm.ComponentModel;

namespace DiabloDungeonTimer.Core.Models;

public class ZoneStatistic : ObservableObject
{
    private readonly string _zone;
    private TimeSpan _averageTime = TimeSpan.Zero;
    private int _count;
    private TimeSpan _fastestTime = TimeSpan.Zero;
    private TimeSpan _slowestTime = TimeSpan.Zero;

    public ZoneStatistic(string zoneName)
    {
        SetProperty(ref _zone, zoneName);
    }

    public string Zone => _zone;

    public int Count
    {
        get => _count;
        set => SetProperty(ref _count, value);
    }

    public TimeSpan FastestTime
    {
        get => _fastestTime;
        set => SetProperty(ref _fastestTime, value);
    }

    public TimeSpan SlowestTime
    {
        get => _slowestTime;
        set => SetProperty(ref _slowestTime, value);
    }

    public TimeSpan AverageTime
    {
        get => _averageTime;
        set => SetProperty(ref _averageTime, value);
    }
}