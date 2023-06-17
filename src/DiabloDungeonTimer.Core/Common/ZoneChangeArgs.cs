using DiabloDungeonTimer.Core.Enums;
using DiabloDungeonTimer.Core.Models;

namespace DiabloDungeonTimer.Core.Common;

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