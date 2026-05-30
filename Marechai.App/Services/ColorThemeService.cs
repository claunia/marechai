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
    private const string        DEFAULT_THEME   = "Default";
    private       IThemeService _themeService;

    public ColorThemeService()
    {
        LoadSavedTheme();
    }

    public string CurrentColorTheme { get; private set; } = DEFAULT_THEME;

    public IThemeService ThemeService => _themeService;

    public IReadOnlyList<string> AvailableColorThemes => new List<string>
    {
        DEFAULT_THEME,
        "Windows311",
        "MacOS9",
        "DOS",
        "Amiga",
        "CDE",
        "CDESolaris"
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

        // Store the existing merged dictionaries (except color overrides)
        var existingDictionaries = app.Resources.MergedDictionaries
                                      .Where(d => d.Source?.OriginalString?.Contains("ColorPalette") != true)
                                      .ToList();

        // Clear all merged dictionaries
        app.Resources.MergedDictionaries.Clear();

        // Re-add the existing dictionaries
        foreach(ResourceDictionary dict in existingDictionaries) app.Resources.MergedDictionaries.Add(dict);

        // Add the new color theme if not default
        if(themeName != DEFAULT_THEME)
        {
            ResourceDictionary newDictionary = null;

            try
            {
                newDictionary = themeName switch
                                {
                                    "Windows311" => new Marechai.App.Styles.Win311ColorPalette(),
                                    "MacOS9"     => new Marechai.App.Styles.MacOS9ColorPalette(),
                                    "DOS"        => new Marechai.App.Styles.DOSColorPalette(),
                                    "Amiga"      => new Marechai.App.Styles.AmigaColorPalette(),
                                    "CDE"        => new Marechai.App.Styles.CDEColorPalette(),
                                    "CDESolaris" => new Marechai.App.Styles.CDESolarisColorPalette(),
                                    _            => null
                                };
            }
            catch
            {
                // Palette class might not exist
            }

            if(newDictionary != null)
                app.Resources.MergedDictionaries.Add(newDictionary);
        }

        // Force UI refresh by toggling the theme temporarily
        ForceThemeRefresh();
    }

    private void LoadSavedTheme()
    {
        try
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

            if(localSettings.Values.ContainsKey(COLOR_THEME_KEY))
            {
                var savedTheme = localSettings.Values[COLOR_THEME_KEY] as string;

                if(!string.IsNullOrEmpty(savedTheme) && AvailableColorThemes.Contains(savedTheme))
                {
                    CurrentColorTheme = savedTheme;
                    ApplyColorTheme(CurrentColorTheme);
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