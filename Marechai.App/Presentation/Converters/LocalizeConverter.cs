using System;
using Microsoft.Extensions.Localization;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Marechai.App.Presentation.Converters;

/// <summary>
///     A value converter that looks up localized strings from IStringLocalizer.
///     Usage in XAML: Text="{Binding Converter={StaticResource Localize}, ConverterParameter='KeyName.Property'}"
///     Or simpler: Text="{Binding ConverterParameter='KeyName.Property', Converter={StaticResource Localize}}"
/// </summary>
public sealed class LocalizeConverter : IValueConverter
{
    private static IStringLocalizer _localizer;

    public static void Initialize(IStringLocalizer localizer) => _localizer = localizer;

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if(parameter is not string key || _localizer is null)
            return value?.ToString() ?? string.Empty;

        LocalizedString result = _localizer[key];

        return result.ResourceNotFound ? key : result.Value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotImplementedException();
}

/// <summary>
///     Attached property that replaces x:Uid functionality using IStringLocalizer.
///     Usage: loc:Loc.Key="ResourceKey" will set the appropriate property (Text for TextBlock, Content for Button, etc.)
/// </summary>
public static class Loc
{
    private static IStringLocalizer _localizer;

    public static readonly DependencyProperty KeyProperty =
        DependencyProperty.RegisterAttached("Key", typeof(string), typeof(Loc),
                                            new PropertyMetadata(null, OnKeyChanged));

    public static void Initialize(IStringLocalizer localizer) => _localizer = localizer;

    public static string GetKey(DependencyObject obj) => (string)obj.GetValue(KeyProperty);

    public static void SetKey(DependencyObject obj, string value) => obj.SetValue(KeyProperty, value);

    private static void OnKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if(e.NewValue is not string key || _localizer is null)
            return;

        // Look up "<key>.Text" for TextBlock, "<key>.Content" for Button, "<key>.Header" for ComboBox, etc.
        // Also try "<key>.Title" for InfoBar, "<key>.PlaceholderText" for search boxes
        switch(d)
        {
            case Microsoft.UI.Xaml.Controls.TextBlock textBlock:
            {
                string value = TryGetValue($"{key}.Text") ?? TryGetValue(key);

                if(value != null)
                    textBlock.Text = value;

                break;
            }

            // AppBarButton must come before Button (it's a subclass)
            case Microsoft.UI.Xaml.Controls.AppBarButton appBarButton:
            {
                string label = TryGetValue($"{key}.Label") ?? TryGetValue($"{key}.Content") ?? TryGetValue(key);

                if(label != null)
                    appBarButton.Label = label;

                break;
            }

            case Microsoft.UI.Xaml.Controls.Button button:
            {
                string value = TryGetValue($"{key}.Content") ?? TryGetValue(key);

                if(value != null)
                    button.Content = value;

                break;
            }

            case Microsoft.UI.Xaml.Controls.AutoSuggestBox autoSuggestBox:
            {
                string placeholder = TryGetValue($"{key}.PlaceholderText") ?? TryGetValue(key);

                if(placeholder != null)
                    autoSuggestBox.PlaceholderText = placeholder;

                break;
            }

            case Microsoft.UI.Xaml.Controls.ComboBox comboBox:
            {
                string header = TryGetValue($"{key}.Header") ?? TryGetValue(key);

                if(header != null)
                    comboBox.Header = header;

                break;
            }

            case Microsoft.UI.Xaml.Controls.InfoBar infoBar:
            {
                string title = TryGetValue($"{key}.Title");

                if(title != null)
                    infoBar.Title = title;

                string message = TryGetValue($"{key}.Message");

                if(message != null)
                    infoBar.Message = message;

                break;
            }

            case Microsoft.UI.Xaml.Controls.TextBox textBox:
            {
                string placeholder = TryGetValue($"{key}.PlaceholderText") ?? TryGetValue(key);

                if(placeholder != null)
                    textBox.PlaceholderText = placeholder;

                break;
            }
        }
    }

    private static string TryGetValue(string key)
    {
        if(_localizer == null) return null;

        LocalizedString result = _localizer[key];

        return result.ResourceNotFound ? null : result.Value;
    }
}
