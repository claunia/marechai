using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Marechai.App.Presentation.Converters;

/// <summary>
///     Converts null object to Collapsed visibility
/// </summary>
public class ObjectToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) =>
        value != null ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotImplementedException();
}

/// <summary>
///     Converts empty/null string to Collapsed visibility
/// </summary>
public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if(value is string str && !string.IsNullOrWhiteSpace(str)) return Visibility.Visible;

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotImplementedException();
}

/// <summary>
///     Converts zero count to Collapsed visibility, otherwise Visible
/// </summary>
public class ZeroToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) =>
        value is > 0 or long and > 0 ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotImplementedException();
}

/// <summary>
///     Converts a boolean value to its inverse
/// </summary>
public class InvertBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) => value is false;

    public object ConvertBack(object value, Type targetType, object parameter, string language) => value is false;
}

/// <summary>
///     Converts a boolean value to Visibility (true = Visible, false = Collapsed)
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) =>
        value is true ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        value is Visibility.Visible;
}

/// <summary>
///     Converts a boolean value to inverted Visibility (true = Collapsed, false = Visible)
/// </summary>
public class InvertBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is true ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        value is Visibility.Collapsed;
}

/// <summary>
///     Negates a boolean value (true → false, false → true)
/// </summary>
public class BoolNegationConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) =>
        value is true ? false : true;

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        value is true ? false : true;
}

/// <summary>
///     Converts null to Visibility (null = Collapsed, not null = Visible)
/// </summary>
public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) =>
        value != null ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotImplementedException();
}

/// <summary>
///     Converts DateTime/DateTimeOffset to short date string (date only, no time)
/// </summary>
public class DateOnlyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) =>
        value switch
        {
            DateTime dt       => dt.ToString("d"),
            DateTimeOffset dto => dto.DateTime.ToString("d"),
            _                 => string.Empty
        };

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotImplementedException();
}