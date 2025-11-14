using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Marechai.App.Presentation.Converters;

/// <summary>
///     Converts boolean value to collapse/expand arrow icon
/// </summary>
public class CollapseExpandIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if(value is bool isOpen) return isOpen ? "◄" : "►";

        return "►";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotImplementedException();
}

/// <summary>
///     Converts boolean value to collapse/expand tooltip text
/// </summary>
public class CollapseExpandTooltipConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if(value is bool isOpen) return isOpen ? "Collapse Sidebar" : "Expand Sidebar";

        return "Expand Sidebar";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotImplementedException();
}

/// <summary>
///     Converts boolean value to GridLength for sidebar column width
/// </summary>
public class SidebarWidthConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if(value is bool isOpen)
        {
            // 280 when open, 60 when collapsed (to keep toggle button visible)
            double width = isOpen ? 280 : 60;

            return new GridLength(width, GridUnitType.Pixel);
        }

        return new GridLength(280, GridUnitType.Pixel);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotImplementedException();
}