using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Data;

namespace Marechai.App.Presentation.Converters;

/// <summary>
///     Converter to display a list of roles as a comma-separated string
/// </summary>
public class RolesListConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if(value is IEnumerable<string> roles) return string.Join(", ", roles);

        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotImplementedException();
}