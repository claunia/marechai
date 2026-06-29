using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Microsoft.UI.Xaml;
using Uno.Extensions.Toolkit;

namespace Marechai.App.Services;

public interface IColorThemeService
{
    string                CurrentColorTheme    { get; }
    IReadOnlyList<string> AvailableColorThemes { get; }
    IThemeService        ThemeService         { get; }
    void                  ApplyColorTheme(string        themeName);
    void                  SetThemeService(IThemeService themeService);
    void                  ReapplyCurrentTheme();
}

public class ColorThemeService : IColorThemeService
{
    private const string        COLOR_THEME_KEY = "ColorTheme";
    private const string        DEFAULT_THEME   = "default";
    private const string        STYLES_NAMESPACE = "Marechai.App.Styles";
    private       IThemeService _themeService;

    // Legacy PascalCase IDs persisted by older builds; mapped to the
    // current kebab-case Blazor-aligned IDs in LoadSavedTheme().
    private static readonly Dictionary<string, string> _legacyThemeIdMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Default"]    = "default",
        ["Windows311"] = "windows311",
        ["MacOS9"]     = "macos9",
        ["DOS"]        = "dos",
        ["Amiga"]      = "amigaos",
        ["CDE"]        = "cde",
        ["CDESolaris"] = "cde-solaris"
    };

    public ColorThemeService()
    {
        LoadSavedTheme();
    }

    public string CurrentColorTheme { get; private set; } = DEFAULT_THEME;

    public IThemeService ThemeService => _themeService;

    // Kebab-case IDs that mirror Marechai.Data.Constants.ThemeIds in the Blazor app.
    // Note: "windows311" is Uno-only (no Blazor counterpart).
    public IReadOnlyList<string> AvailableColorThemes => new List<string>
    {
        DEFAULT_THEME,
        "windows311",
        "windows95",
        "macos9",
        "dos",
        "amigaos",
        "cde",
        "cde-solaris",
        "cyberpunk",
        "phosphor",
        "phosphor-amber"
    };

    public void SetThemeService(IThemeService themeService)
    {
        _themeService = themeService;

        // Now that we have the theme service, reapply saved theme (even default needs refresh)
        ReapplyCurrentTheme();
    }

    public void ReapplyCurrentTheme()
    {
        // Force refresh of the current theme
        ApplyColorTheme(CurrentColorTheme);
    }

    public void ApplyColorTheme(string themeName)
    {
        if(!AvailableColorThemes.Contains(themeName)) return;

        CurrentColorTheme = themeName;

        // Save preference
        try
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values[COLOR_THEME_KEY] = themeName;
        }
        catch
        {
            // Silently fail
        }

        // Apply the theme by rebuilding merged dictionaries
        Application app = Application.Current;

        if(app?.Resources == null) return;

        // Palette dictionaries are added as typed ResourceDictionary instances,
        // so Source is often null. Filter by dictionary type instead.
        var existingDictionaries = app.Resources.MergedDictionaries
                                      .Where(d => !IsColorPaletteDictionary(d))
                                      .ToList();

        // Clear all merged dictionaries
        app.Resources.MergedDictionaries.Clear();

        // Re-add the existing dictionaries
        foreach(ResourceDictionary dict in existingDictionaries) app.Resources.MergedDictionaries.Add(dict);

        // Add the requested palette dictionary, including the default dark theme
        ResourceDictionary newDictionary = null;

        try
        {
            newDictionary = themeName switch
                            {
                                DEFAULT_THEME    => new Marechai.App.Styles.DefaultDarkColorPalette(),
                                "windows311"     => new Marechai.App.Styles.Win311ColorPalette(),
                                "windows95"      => new Marechai.App.Styles.Windows95ColorPalette(),
                                "macos9"         => new Marechai.App.Styles.MacOS9ColorPalette(),
                                "dos"            => new Marechai.App.Styles.DOSColorPalette(),
                                "amigaos"        => new Marechai.App.Styles.AmigaColorPalette(),
                                "cde"            => new Marechai.App.Styles.CDEColorPalette(),
                                "cde-solaris"    => new Marechai.App.Styles.CDESolarisColorPalette(),
                                "cyberpunk"      => new Marechai.App.Styles.CyberpunkColorPalette(),
                                "phosphor"       => new Marechai.App.Styles.PhosphorColorPalette(),
                                "phosphor-amber" => new Marechai.App.Styles.PhosphorAmberColorPalette(),
                                _                => null
                            };
        }
        catch
        {
            // Palette class might not exist
        }

        if(newDictionary != null)
            app.Resources.MergedDictionaries.Add(newDictionary);

        // Force UI refresh by toggling the theme temporarily
        ForceThemeRefresh();
    }

    private static bool IsColorPaletteDictionary(ResourceDictionary dictionary)
    {
        Type dictionaryType = dictionary.GetType();

        return dictionaryType.Namespace == STYLES_NAMESPACE &&
               dictionaryType.Name.EndsWith("ColorPalette", StringComparison.Ordinal);
    }

    private void LoadSavedTheme()
    {
        try
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

            if(localSettings.Values.ContainsKey(COLOR_THEME_KEY))
            {
                var savedTheme = localSettings.Values[COLOR_THEME_KEY] as string;

                if(!string.IsNullOrEmpty(savedTheme))
                {
                    // Migrate legacy PascalCase IDs ("CDESolaris") to kebab-case ("cde-solaris")
                    // so users who upgraded keep their selected theme.
                    if(_legacyThemeIdMap.TryGetValue(savedTheme, out string migrated))
                        savedTheme = migrated;

                    if(AvailableColorThemes.Contains(savedTheme))
                    {
                        CurrentColorTheme = savedTheme;
                        ApplyColorTheme(CurrentColorTheme);
                    }
                }
            }
        }
        catch
        {
            // If loading fails, use default theme
        }
    }

    private async void ForceThemeRefresh()
    {
        if(_themeService == null) return;

        try
        {
            AppTheme currentTheme = _themeService.Theme;
            AppTheme tempTheme = currentTheme == AppTheme.Light ? AppTheme.Dark : AppTheme.Light;
            await _themeService.SetThemeAsync(tempTheme);
            await Task.Delay(50);
            await _themeService.SetThemeAsync(currentTheme);
        }
        catch
        {
            // Silently fail
        }
    }
}
