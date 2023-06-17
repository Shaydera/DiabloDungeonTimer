using System;
using System.Globalization;
using System.Windows.Data;
using DiabloDungeonTimer.Core.Utils;

namespace DiabloDungeonTimer.UI.Converters;

public class TimeSpanDisplayConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return string.Empty;
        if (value is not TimeSpan timeSpan)
            throw new ArgumentException("Value must be a TimeSpan");

        return timeSpan.ToDisplayString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}