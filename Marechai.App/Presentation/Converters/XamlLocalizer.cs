using Microsoft.Extensions.Localization;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Marechai.App.Presentation.Converters;

/// <summary>
///     Helper that applies localized strings to XAML elements after they're loaded.
///     Call ApplyLocalization on a root element to find and localize all tagged children.
/// </summary>
public static class XamlLocalizer
{
    private static IStringLocalizer _localizer;

    public static void Initialize(IStringLocalizer localizer) => _localizer = localizer;

    /// <summary>
    ///     Gets a localized string by key. Returns the key itself if not found.
    /// </summary>
    public static string Get(string key)
    {
        if(_localizer is null) return key;

        LocalizedString result = _localizer[key];

        return result.ResourceNotFound ? key : result.Value;
    }
}
