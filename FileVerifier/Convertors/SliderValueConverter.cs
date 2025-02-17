using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace AvaloniaDraft.Convertors;

public class SliderValueConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double doubleValue)
        {
            // Format the value to two decimal places
            return doubleValue.ToString("F2", culture);
        }
        return value?.ToString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string stringValue)
        {
            return new BindingNotification(new ArgumentException("Expected a string value."), BindingErrorType.Error);
        }

        if (string.IsNullOrWhiteSpace(stringValue))
        {
            return new BindingNotification(new ArgumentException("Value cannot be empty."), BindingErrorType.DataValidationError);
        }

        // Attempt to parse the input string to a double
        if (!double.TryParse(stringValue, NumberStyles.Any, culture, out var parsedValue))
        {
            return new BindingNotification(new FormatException("Please enter a valid number."), BindingErrorType.DataValidationError);
        }

        // Validate decimal places in the input string
        var decimalSeparator = culture.NumberFormat.NumberDecimalSeparator;
        var parts = stringValue.Split([decimalSeparator], StringSplitOptions.None);

        if (parts.Length > 1 && parts[1].Length > 2)
        {
            return new BindingNotification(
                new ArgumentException("Maximum of two decimal places allowed."),
                BindingErrorType.DataValidationError
            );
        }

        return parsedValue;
    }
}