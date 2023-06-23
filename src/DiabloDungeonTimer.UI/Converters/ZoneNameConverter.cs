using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace DiabloDungeonTimer.UI.Converters;

public partial class ZoneNameConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string zoneName)
            throw new ArgumentException("Value must be a string");
        if (string.IsNullOrEmpty(zoneName))
            return string.Empty;

        zoneName = ZoneNameRegex().Replace(zoneName, "$2");
        return CamelCaseRegex().Replace(zoneName, " ");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    [GeneratedRegex(@"^([A-Za-z0-9]+_){1,2}(\w+$)")]
    private static partial Regex ZoneNameRegex();
    
    [GeneratedRegex(@"(?<=[A-Z])(?=[A-Z][a-z])|(?<=[^A-Z])(?=[A-Z])|(?<=[A-Za-z])(?=[^A-Za-z])")]
    private static partial Regex CamelCaseRegex();
}