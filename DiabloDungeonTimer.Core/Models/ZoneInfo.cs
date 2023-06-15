using System.Text.RegularExpressions;

namespace DiabloDungeonTimer.Core.Models;

public partial class ZoneInfo
{
    private ZoneInfo(string name, DateTime startTime, DateTime? endTime)
    {
        Name = name;
        StartTime = startTime;
        EndTime = endTime;
    }

    public string Name { get; }
    public DateTime StartTime { get; }
    public DateTime? EndTime { get; set; }
    public TimeSpan Duration => (EndTime ?? DateTime.Now).Subtract(StartTime);

    public static bool TryParse(LogEntry? logEntry, out ZoneInfo? zoneInfo)
    {
        zoneInfo = null;
        if (logEntry == null || string.IsNullOrEmpty(logEntry.Message))
            return false;

        Match match = ZoneInfoRegex().Match(logEntry.Message);
        if (!match.Success)
            return false;

        zoneInfo = new ZoneInfo(match.Groups[1].Value, logEntry.TimeStamp, null);
        return true;
    }

    [GeneratedRegex(@"Client entered world \| world: (\w+)")]
    private static partial Regex ZoneInfoRegex();
}