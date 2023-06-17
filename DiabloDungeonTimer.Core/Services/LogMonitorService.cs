using System.Diagnostics;
using System.IO;
using System.Windows.Threading;
using DiabloDungeonTimer.Core.Common;
using DiabloDungeonTimer.Core.Enums;
using DiabloDungeonTimer.Core.Models;
using DiabloDungeonTimer.Core.Services.Interfaces;

namespace DiabloDungeonTimer.Core.Services;

public sealed class LogMonitorService : ILogMonitorService, IDisposable
{
    private readonly FileSystemWatcher _fileSystemWatcher;
    private readonly DispatcherTimer _readLogTimer;
    private readonly ISettingsService _settingsService;
    private ZoneInfo? _currentZoneInfo;

    private bool _logChanged;


    public LogMonitorService(ISettingsService settingsService)
    {
        _settingsService = settingsService;
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
        if (!_settingsService.IsValid())
            return false;
        _fileSystemWatcher.Path = _settingsService.Settings.GameDirectory;
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
            string lastZoneLine = File.ReadLines(_settingsService.Settings.GameDirectory + "\\FenrisDebug.txt")
                .Last(line => line.Contains("[Game] Client entered world"));
            if (!LogEntry.TryParse(lastZoneLine, out LogEntry? logEntry))
                return;
            if (logEntry != null && _currentZoneInfo != null && _currentZoneInfo.StartTime.Equals(logEntry.TimeStamp))
                return;
            if (!ZoneInfo.TryParse(logEntry, out ZoneInfo? newZoneInfo) || newZoneInfo == null)
                return;
            if (_currentZoneInfo is { EndTime: null })
            {
                _currentZoneInfo.EndTime = newZoneInfo.StartTime;
                ZoneChange?.Invoke(this, new ZoneChangeArgs(_currentZoneInfo, ZoneChangeType.Exited));
            }

            if (newZoneInfo.Name.Equals("Limbo") || newZoneInfo.Name.Equals("FrontendStartup") ||
                newZoneInfo.Name.Equals("Sanctuary_Eastern_Continent"))
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
        if (!e.FullPath.Equals(_settingsService.Settings.GameDirectory + "\\FenrisDebug.txt"))
            return;
        _logChanged = true;
    }

    private static void OnFileSystemError(object sender, ErrorEventArgs e)
    {
        Debug.Print(e.ToString());
    }
}