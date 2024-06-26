using System.Diagnostics;
using System.IO;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.DependencyInjection;
using DiabloDungeonTimer.Core.Common;
using DiabloDungeonTimer.Core.Enums;
using DiabloDungeonTimer.Core.Models;
using DiabloDungeonTimer.Core.Services.Interfaces;

namespace DiabloDungeonTimer.Core.Services;

public sealed class LogMonitorService : ILogMonitorService, IDisposable
{
    private readonly FileSystemWatcher _fileSystemWatcher;

    private readonly HashSet<string> _ignoredZones = new()
        { "Limbo", "FrontendStartup", "Sanctuary_Eastern_Continent" };

    private readonly DispatcherTimer _readLogTimer;
    private readonly ISettingsProvider _settingsProvider;
    private ZoneInfo? _currentZoneInfo;
    private bool _logChanged;


    public LogMonitorService(ISettingsProvider? settingsProvider = null)
    {
        _settingsProvider = settingsProvider ?? Ioc.Default.GetRequiredService<ISettingsProvider>();
        _fileSystemWatcher = new FileSystemWatcher
        {
            EnableRaisingEvents = false,
            Filter = "FenrisDebug.txt",
            NotifyFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime |
                           NotifyFilters.LastWrite | NotifyFilters.Size,
            IncludeSubdirectories = false
        };
        _fileSystemWatcher.Changed += OnFileSystemChange;
        _fileSystemWatcher.Created += (_, _) => { _logChanged = false; };
        _fileSystemWatcher.Deleted += (_, _) => { _logChanged = false; };
        _fileSystemWatcher.Renamed += (_, _) => { _logChanged = false; };
        _fileSystemWatcher.Error += OnFileSystemError;

        //TODO: This is a lazy workaround so we don't have to deal with event invocations from the file system watcher thread.
        //TODO: Remove this and properly invoke the events from the file system watcher instead.
        _readLogTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromSeconds(2)
        };
        _readLogTimer.Tick += ReadLogLogTimerTick;
    }

    public void Dispose()
    {
        _fileSystemWatcher.Dispose();
    }

    public bool StartMonitor()
    {
        StopMonitor();
        if (!_settingsProvider.IsValid())
            return false;
        _fileSystemWatcher.Path = _settingsProvider.Settings.GameDirectory;
        _fileSystemWatcher.EnableRaisingEvents = true;
        _readLogTimer.Start();
        return true;
    }

    public bool IsMonitoring()
    {
        return _fileSystemWatcher.EnableRaisingEvents && _readLogTimer.IsEnabled;
    }

    public bool StopMonitor()
    {
        _fileSystemWatcher.EnableRaisingEvents = false;
        _readLogTimer.Stop();
        return true;
    }

    public event EventHandler<ZoneChangeArgs>? ZoneChange;

    private void ReadLogLogTimerTick(object? sender, EventArgs e)
    {
        if (!_logChanged)
            return;
        _logChanged = false;
        try
        {
            string lastZoneLine = File.ReadLines(_settingsProvider.Settings.GameDirectory + "\\FenrisDebug.txt")
                .Last(line => line.Contains("[Game] Client entered world"));
            if (!LogEntry.TryParse(lastZoneLine, out LogEntry? logEntry) || logEntry == null)
                return;
            if (_currentZoneInfo != null && _currentZoneInfo.StartTime.Equals(logEntry.TimeStamp))
                return;
            if (!ZoneInfo.TryParse(logEntry, out ZoneInfo? newZoneInfo) || newZoneInfo == null)
                return;
            if (_currentZoneInfo is { Finished: false })
            {
                _currentZoneInfo.EndTime = newZoneInfo.StartTime;
                ZoneChange?.Invoke(this, new ZoneChangeArgs(_currentZoneInfo, ZoneChangeType.Exited));
            }

            if (_ignoredZones.Contains(newZoneInfo.Zone))
                return;
            ZoneChange?.Invoke(this, new ZoneChangeArgs(newZoneInfo, ZoneChangeType.Entered));
            _currentZoneInfo = newZoneInfo;
        }
        catch (Exception exception)
        {
            if (exception is FileNotFoundException or IOException or ArgumentNullException or InvalidOperationException)
                return;
            throw;
        }
    }

    private void OnFileSystemChange(object sender, FileSystemEventArgs e)
    {
        if (e.ChangeType != WatcherChangeTypes.Changed)
            return;
        if (!e.FullPath.Equals(_settingsProvider.Settings.GameDirectory + "\\FenrisDebug.txt"))
            return;
        _logChanged = true;
    }

    private static void OnFileSystemError(object sender, ErrorEventArgs e)
    {
        Debug.Print(e.ToString());
    }
}