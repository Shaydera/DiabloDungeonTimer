using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;
using DiabloDungeonTimer.Core.Enums;
using DiabloDungeonTimer.Core.Utils;

namespace DiabloDungeonTimer.Core.Models;

[SuppressMessage("ReSharper", "NotAccessedField.Global")]
public partial record LogEntry
{
    public readonly string Message;
    public LogLevel Level;

    public DateTime TimeStamp;

    public string Type;

    private LogEntry(LogLevel level, DateTime timeStamp, string type, string message)
    {
        Level = level;
        TimeStamp = timeStamp;
        Type = type;
        Message = message;
    }

    public static bool TryParse(string logLine, out LogEntry? logEntry)
    {
        logEntry = null;
        if (string.IsNullOrEmpty(logLine))
            return false;

        Match match = LogParseRegex().Match(logLine);
        if (!match.Success)
            return false;

        logEntry = new LogEntry(EnumUtils.ParseLogLevel(match.Groups[1].Value),
            DateTime.ParseExact(match.Groups[2].Value, "yyyy.MM.dd HH:mm:ss.ffffff", CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal), match.Groups[5].Value, match.Groups[6].Value);
        return true;
    }

    [GeneratedRegex(@"([A-Z])\s(\d{4}(\.\d{2}){2}\s(\d{2}:){2}\d{2}\.\d{6})\s?\d*\t\[(\w+)\]\s(.+)", RegexOptions.IgnoreCase,
        "en-US")]
    private static partial Regex LogParseRegex();
}