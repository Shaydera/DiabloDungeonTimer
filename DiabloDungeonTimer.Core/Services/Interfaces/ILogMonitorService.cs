using System.Diagnostics.CodeAnalysis;
using DiabloDungeonTimer.Core.Models;

namespace DiabloDungeonTimer.Core.Services.Interfaces;

[SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
public interface ILogMonitorService
{
    public bool StartMonitor();
    public bool StopMonitor();
    public bool IsMonitoring();

    public event EventHandler<ZoneChangeArgs> ZoneChange;
}