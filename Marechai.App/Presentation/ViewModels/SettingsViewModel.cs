using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.App.Presentation.Models;
using Marechai.App.Services;
using Uno.Extensions.Toolkit;

namespace Marechai.App.Presentation.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IColorThemeService _colorThemeService;
    private readonly IStringLocalizer   _localizer;
    private          IThemeService     _themeService;

    [ObservableProperty]
    private List<ColorThemeOption> _availableColorThemes = new();

    [ObservableProperty]
    private List<ThemeOption> _availableThemes = new();

    [ObservableProperty]
    private ColorThemeOption _selectedColorTheme;

    [ObservableProperty]
    private ThemeOption _selectedTheme;

    public SettingsViewModel(IStringLocalizer   localizer,
                             IColorThemeService colorThemeService)
    {
        _localizer         = localizer;
        _colorThemeService = colorThemeService;
        Title              = _localizer["Settings"];

        // Initialize immediately to ensure UI is populated
        InitializeOptions();

        // Wait for theme service to initialize
        _ = InitializeThemeServiceAsync();
    }

    public string Title { get; }

    private async Task InitializeThemeServiceAsync()
    {
        try
        {
            // Get IThemeService from ColorThemeService (initialized by App.OnInitialized)
            _themeService = _colorThemeService.ThemeService;

            if(_themeService != null)
                await _themeService.InitializeAsync();
        }
        catch
        {
            // Theme service might already be initialized
        }
    }

    private void InitializeOptions()
    {
        // Initialize Light/Dark/System Themes
        AvailableThemes = new List<ThemeOption>
        {
            new()
            {
                Theme       = AppTheme.Light,
                DisplayName = _localizer["LightTheme"]
            },
            new()
            {
                Theme       = AppTheme.Dark,
                DisplayName = _localizer["DarkTheme"]
            },
            new()
            {
                Theme       = AppTheme.System,
                DisplayName = _localizer["SystemTheme"]
            }
        };

        // Initialize Color Themes
        AvailableColorThemes = new List<ColorThemeOption>
        {
            new()
            {
                ThemeName   = "Default",
                DisplayName = _localizer["DefaultColorTheme"]
            },
            new()
            {
                ThemeName   = "Windows311",
                DisplayName = _localizer["Windows311Theme"]
            },
            new()
            {
                ThemeName   = "MacOS9",
                DisplayName = _localizer["MacOS9Theme"]
            },
            new()
            {
                ThemeName   = "DOS",
                DisplayName = _localizer["DOSTheme"]
            },
            new()
            {
                ThemeName   = "Amiga",
                DisplayName = _localizer["AmigaTheme"]
            },
            new()
            {
                ThemeName   = "CDE",
                DisplayName = _localizer["CDETheme"]
            }
        };

        // Try to load saved preferences
        LoadSavedPreferences();
    }

    private async void LoadSavedPreferences()
    {
        try
        {
            // Load current theme from ThemeService
            AppTheme currentTheme = _themeService?.Theme ?? AppTheme.System;

            SelectedTheme = AvailableThemes.FirstOrDefault(t => t.Theme == currentTheme) ??
                            AvailableThemes.FirstOrDefault(t => t.Theme == AppTheme.System) ?? AvailableThemes.First();

            // Load current color theme
            string currentColorTheme = _colorThemeService.CurrentColorTheme;

            SelectedColorTheme = AvailableColorThemes.FirstOrDefault(t => t.ThemeName == currentColorTheme) ??
                                 AvailableColorThemes.First();
        }
        catch
        {
            // If loading fails, use defaults
            SelectedTheme = AvailableThemes.FirstOrDefault(t => t.Theme == AppTheme.System) ?? AvailableThemes.First();
            SelectedColorTheme = AvailableColorThemes.First();
        }
    }

    partial void OnSelectedThemeChanged(ThemeOption value)
    {
        if(value != null) ApplyTheme(value);
    }

    partial void OnSelectedColorThemeChanged(ColorThemeOption value)
    {
        if(value != null) ApplyColorTheme(value);
    }

    private async void ApplyTheme(ThemeOption theme)
    {
        try
        {
            if(_themeService != null)
                await _themeService.SetThemeAsync(theme.Theme);
        }
        catch
        {
            // Silently fail
        }
    }

    private void ApplyColorTheme(ColorThemeOption colorTheme)
    {
        try
        {
            _colorThemeService.ApplyColorTheme(colorTheme.ThemeName);
        }
        catch
        {
            // Silently fail
        }
    }
}