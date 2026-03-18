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
            return parameter?.ToString() ?? string.Empty;

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

        // Look up resource keys. In .resx files, ".Text" and ".Content" suffixes are stripped
        // (e.g., "LatestNewsTitle" instead of "LatestNewsTitle.Text").
        // Other suffixes use underscore: "_Placeholder", "_Title", "_Message", "_Label", "_Header"
        switch(d)
        {
            case Microsoft.UI.Xaml.Controls.TextBlock textBlock:
            {
                string value = TryGetValue(key);

                if(value != null)
                    textBlock.Text = value;

                break;
            }

            // AppBarButton must come before Button (it's a subclass)
            case Microsoft.UI.Xaml.Controls.AppBarButton appBarButton:
            {
                string label = TryGetValue($"{key}_Label") ?? TryGetValue(key);

                if(label != null)
                    appBarButton.Label = label;

                break;
            }

            case Microsoft.UI.Xaml.Controls.Button button:
            {
                string value = TryGetValue(key);

                if(value != null)
                    button.Content = value;

                break;
            }

            case Microsoft.UI.Xaml.Controls.AutoSuggestBox autoSuggestBox:
            {
                string placeholder = TryGetValue($"{key}_Placeholder") ?? TryGetValue(key);

                if(placeholder != null)
                    autoSuggestBox.PlaceholderText = placeholder;

                break;
            }

            case Microsoft.UI.Xaml.Controls.ComboBox comboBox:
            {
                string header = TryGetValue($"{key}_Header") ?? TryGetValue(key);

                if(header != null)
                    comboBox.Header = header;

                break;
            }

            case Microsoft.UI.Xaml.Controls.InfoBar infoBar:
            {
                string title = TryGetValue($"{key}_Title");

                if(title != null)
                    infoBar.Title = title;

                string message = TryGetValue($"{key}_Message");

                if(message != null)
                    infoBar.Message = message;

                break;
            }

            case Microsoft.UI.Xaml.Controls.TextBox textBox:
            {
                string placeholder = TryGetValue($"{key}_Placeholder") ?? TryGetValue(key);

                if(placeholder != null)
                    textBox.PlaceholderText = placeholder;

                break;
            }
        }
    }

    private static string TryGetValue(string key)
    {
        if(_localizer == null) return null;

        // Try exact key first (handles "LatestNewsTitle.Text" from .resx)
        LocalizedString result = _localizer[key];

        if(!result.ResourceNotFound)
            return result.Value;

        // .resx ResourceManager may need dots replaced — try common alternatives
        // Some resource managers store "Key.Property" keys with dots intact
        string withUnderscore = key.Replace('.', '_');
        result = _localizer[withUnderscore];

        return result.ResourceNotFound ? null : result.Value;
    }
}
