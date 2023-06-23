using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace DiabloDungeonTimer.Core.Models;

public partial record ZoneInfo
{
    private ZoneInfo()
    {
        Zone = string.Empty;
        StartTime = DateTime.MinValue;
        EndTime = null;
    }

    [XmlAttribute("Name")] public string Zone { get; set; }
    [XmlElement] public DateTime StartTime { get; set; }
    [XmlElement] public DateTime? EndTime { get; set; }
    public TimeSpan Duration => (EndTime ?? DateTime.Now).Subtract(StartTime);

    public static bool TryParse(LogEntry? logEntry, out ZoneInfo? zoneInfo)
    {
        zoneInfo = null;
        if (logEntry == null || string.IsNullOrEmpty(logEntry.Message))
            return false;

        Match match = ZoneInfoRegex().Match(logEntry.Message);
        if (!match.Success)
            return false;

        zoneInfo = new ZoneInfo
        {
            Zone = match.Groups[1].Value,
            StartTime = logEntry.TimeStamp
        };
        return true;
    }

    [GeneratedRegex(@"Client entered world \| world: (\w+)")]
    private static partial Regex ZoneInfoRegex();
}