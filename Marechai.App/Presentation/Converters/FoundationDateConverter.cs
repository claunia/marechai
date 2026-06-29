using System;
using Microsoft.UI.Xaml.Data;

namespace Marechai.App.Presentation.Converters;

/// <summary>
///     Converts DateTime to formatted foundation date string, returns empty if null
/// </summary>
public class FoundationDateConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        int? precision = parameter is int precisionValue ? precisionValue : null;

        if(parameter is string precisionText && int.TryParse(precisionText, out int parsedPrecision))
            precision = parsedPrecision;

        if(value is DateTime dateTime) return DatePrecisionFormatter.Format(dateTime, precision);
        if(value is DateTimeOffset dateTimeOffset) return DatePrecisionFormatter.Format(dateTimeOffset, precision);

        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotImplementedException();
}
