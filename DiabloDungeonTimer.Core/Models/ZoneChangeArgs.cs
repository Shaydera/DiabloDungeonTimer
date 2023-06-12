using DiabloDungeonTimer.Core.Enums;

namespace DiabloDungeonTimer.Core.Models;

public class ZoneChangeArgs : EventArgs
{
    public ZoneChangeArgs(ZoneInfo zone, ZoneChangeType changeType)
    {
        Zone = zone;
        ChangeType = changeType;
    }

    public ZoneInfo Zone { get; }

    public ZoneChangeType ChangeType { get; }
}