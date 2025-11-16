using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.App.Services;
using Uno.Extensions.Toolkit;

namespace Marechai.App.Presentation.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IColorThemeService _colorThemeService;
    private readonly IStringLocalizer   _localizer;
    private readonly IThemeService      _themeService;

    [ObservableProperty]
    private List<ColorThemeOption> _availableColorThemes = new();

    [ObservableProperty]
    private List<ThemeOption> _availableThemes = new();

    [ObservableProperty]
    private ColorThemeOption _selectedColorTheme;

    [ObservableProperty]
    private ThemeOption _selectedTheme;

    public SettingsViewModel(IStringLocalizer   localizer, IThemeService themeService,
                             IColorThemeService colorThemeService)
    {
        _localizer         = localizer;
        _themeService      = themeService;
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
            await _themeService.InitializeAsync();

            // Ensure the color theme service has a reference to the theme service
            _colorThemeService.SetThemeService(_themeService);
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
            AppTheme currentTheme = _themeService.Theme;

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
            // Apply theme immediately using ThemeService
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

public class ThemeOption
{
    public AppTheme Theme       { get; set; }
    public string   DisplayName { get; set; } = string.Empty;
}

public class ColorThemeOption
{
    public string ThemeName   { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}