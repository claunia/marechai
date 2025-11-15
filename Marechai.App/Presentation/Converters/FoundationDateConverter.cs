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
        if(value is DateTime dateTime) return dateTime.ToString("MMMM d, yyyy");

        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotImplementedException();
}