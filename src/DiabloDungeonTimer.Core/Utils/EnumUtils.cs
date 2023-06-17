using DiabloDungeonTimer.Core.Enums;

namespace DiabloDungeonTimer.Core.Utils;

public static class EnumUtils
{
    private static LogLevel ParseLogLevel(char logLevel)
    {
        return logLevel switch
        {
            'T' => LogLevel.Trace,
            'D' => LogLevel.Debug,
            'I' => LogLevel.Information,
            'W' => LogLevel.Warning,
            'E' => LogLevel.Error,
            'C' => LogLevel.Critical,
            _ => LogLevel.None
        };
    }

    public static LogLevel ParseLogLevel(string logLevel)
    {
        if (logLevel.Length == 1)
            return ParseLogLevel(logLevel[0]);

        return logLevel switch
        {
            "Trace" => LogLevel.Trace,
            "Debug" => LogLevel.Debug,
            "Information" => LogLevel.Information,
            "Warning" => LogLevel.Warning,
            "Error" => LogLevel.Error,
            "Critical" => LogLevel.Critical,
            _ => LogLevel.None
        };
    }
}