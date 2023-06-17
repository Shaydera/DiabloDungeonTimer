namespace DiabloDungeonTimer.Core.Utils;

public static class Extensions
{
    public static string ToDisplayString(this TimeSpan span)
    {
        var result = @$"{span:mm\:ss\.f}";
        if (span.TotalHours >= 1)
            result = $@"{span:hh}:{result}";
        if (span.TotalDays >= 1)
            result = $@"{span:%d}.{result}";
        return result;
    }
}