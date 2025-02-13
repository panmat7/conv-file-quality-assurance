using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace AvaloniaDraft.Convertors;

public class SliderValueConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double doubleValue)
        {
            // Format the value to always show 2 decimal places
            return doubleValue.ToString("F2", culture);
        }
        return value;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}