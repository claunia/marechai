using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Models;
using Marechai.App.Presentation.Views;
using Marechai.App.Services;
using Marechai.App.Services.Authentication;
using Microsoft.UI.Xaml.Media.Imaging;
using QRCoder;
using Uno.Extensions.Toolkit;

namespace Marechai.App.Presentation.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly AuthService? _authService;
    private readonly IColorThemeService _colorThemeService;
    private readonly IStringLocalizer   _localizer;
    private readonly IRegionManager     _regionManager;
    private readonly TwoFactorService?  _twoFactorService;
    private readonly ITokenService?     _tokenService;
    private          IThemeService     _themeService;

    [ObservableProperty]
    private List<ColorThemeOption> _availableColorThemes = new();

    [ObservableProperty]
    private List<ThemeOption> _availableThemes = new();

    [ObservableProperty]
    private ColorThemeOption _selectedColorTheme;

    [ObservableProperty]
    private ThemeOption _selectedTheme;

    // ── Security / 2FA state ──
    [ObservableProperty] private bool   _isAuthenticated;
    [ObservableProperty] private bool   _twoFactorEnabled;
    [ObservableProperty] private bool   _authenticatorEnabled;
    [ObservableProperty] private bool   _emailTwoFactorEnabled;
    [ObservableProperty] private int    _recoveryCodesRemaining;
    [ObservableProperty] private string _securityMessage = string.Empty;
    [ObservableProperty] private bool   _isSecurityBusy;

    // Authenticator setup
    [ObservableProperty] private bool         _authSetupVisible;
    [ObservableProperty] private string       _authSharedKey = string.Empty;
    [ObservableProperty] private string       _authUri       = string.Empty;
    [ObservableProperty] private BitmapImage? _authQrImage;
    [ObservableProperty] private string       _authVerifyCode = string.Empty;

    // Email enable flow
    [ObservableProperty] private bool   _emailEnableVisible;
    [ObservableProperty] private string _emailEnablePassword = string.Empty;
    [ObservableProperty] private string _emailEnableCode     = string.Empty;

    // Recovery codes display (one-shot)
    [ObservableProperty] private List<string> _displayedRecoveryCodes = new();
    public bool ShowRecoveryCodes => DisplayedRecoveryCodes.Count > 0;

    // Messaging notification preferences
    [ObservableProperty] private bool _notifyOnNewMessage = true;
    [ObservableProperty] private string _notificationPreferencesMessage = string.Empty;
    [ObservableProperty] private bool _isNotificationPreferencesBusy;
    public bool HasNotificationPreferencesMessage => !string.IsNullOrWhiteSpace(NotificationPreferencesMessage);

    public SettingsViewModel(IStringLocalizer   localizer,
                             IColorThemeService colorThemeService,
                             IRegionManager     regionManager,
                             TwoFactorService?  twoFactorService = null,
                             ITokenService?     tokenService     = null,
                             AuthService?       authService      = null)
    {
        _localizer         = localizer;
        _colorThemeService = colorThemeService;
        _regionManager     = regionManager;
        _twoFactorService  = twoFactorService;
        _tokenService      = tokenService;
        _authService       = authService;
        Title              = _localizer["Settings"];

        IsAuthenticated = !string.IsNullOrWhiteSpace(_tokenService?.GetToken());

        // Initialize immediately to ensure UI is populated
        InitializeOptions();

        // Wait for theme service to initialize
        _ = InitializeThemeServiceAsync();

        if(IsAuthenticated) _ = RefreshTwoFactorStatusAsync();
        if(IsAuthenticated) _ = LoadNotificationPreferencesAsync();
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

        // Initialize Color Themes — IDs are kebab-case to match the Blazor
        // ThemeIds catalog (Marechai.Data.Constants.ThemeIds), so a single
        // server-side preference can be shared by both clients.
        AvailableColorThemes = new List<ColorThemeOption>
        {
            new()
            {
                ThemeName   = "default",
                DisplayName = _localizer["DefaultColorTheme"]
            },
            new()
            {
                ThemeName   = "windows311",
                DisplayName = _localizer["Windows311Theme"]
            },
            new()
            {
                ThemeName   = "windows95",
                DisplayName = _localizer["Windows95Theme"]
            },
            new()
            {
                ThemeName   = "macos9",
                DisplayName = _localizer["MacOS9Theme"]
            },
            new()
            {
                ThemeName   = "dos",
                DisplayName = _localizer["DOSTheme"]
            },
            new()
            {
                ThemeName   = "amigaos",
                DisplayName = _localizer["AmigaTheme"]
            },
            new()
            {
                ThemeName   = "cde",
                DisplayName = _localizer["CDETheme"]
            },
            new()
            {
                ThemeName   = "cde-solaris",
                DisplayName = _localizer["CDESolarisTheme"]
            },
            new()
            {
                ThemeName   = "cyberpunk",
                DisplayName = _localizer["CyberpunkTheme"]
            },
            new()
            {
                ThemeName   = "phosphor",
                DisplayName = _localizer["PhosphorTheme"]
            },
            new()
            {
                ThemeName   = "phosphor-amber",
                DisplayName = _localizer["PhosphorAmberTheme"]
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

    // ──────────────────────────────────────────────────────────────────
    //  2FA / Security methods
    // ──────────────────────────────────────────────────────────────────

    partial void OnDisplayedRecoveryCodesChanged(List<string> value) => OnPropertyChanged(nameof(ShowRecoveryCodes));

    public async Task RefreshTwoFactorStatusAsync()
    {
        if(_twoFactorService is null) return;

        var status = await _twoFactorService.GetStatusAsync();
        if(status is null) return;

        TwoFactorEnabled       = status.Enabled       ?? false;
        AuthenticatorEnabled   = status.AuthenticatorEnabled   ?? false;
        EmailTwoFactorEnabled  = status.EmailEnabled           ?? false;
        RecoveryCodesRemaining = status.RecoveryCodesRemaining ?? 0;
    }

    [RelayCommand]
    private void ChangePassword() => _regionManager.RequestNavigate(RegionNames.Content, nameof(ChangePasswordPage));

    [RelayCommand]
    private async Task StartAuthenticatorSetupAsync()
    {
        if(_twoFactorService is null) return;

        IsSecurityBusy = true;
        SecurityMessage = string.Empty;

        var setup = await _twoFactorService.SetupAuthenticatorAsync();

        IsSecurityBusy = false;

        if(setup is null)
        {
            SecurityMessage = _localizer["Failed to generate authenticator setup."];

            return;
        }

        AuthSharedKey     = setup.SharedKey ?? string.Empty;
        AuthUri           = setup.AuthenticatorUri ?? string.Empty;
        AuthQrImage       = await BuildQrImageAsync(AuthUri);
        AuthVerifyCode    = string.Empty;
        AuthSetupVisible  = true;
    }

    static async Task<BitmapImage?> BuildQrImageAsync(string uri)
    {
        try
        {
            using var generator = new QRCodeGenerator();
            QRCodeData data = generator.CreateQrCode(uri, QRCodeGenerator.ECCLevel.Q);
            var       png  = new PngByteQRCode(data);
            byte[]    bytes = png.GetGraphic(8);

            var bitmap = new BitmapImage();
            using var ms = new MemoryStream(bytes);
            using var ras = ms.AsRandomAccessStream();
            await bitmap.SetSourceAsync(ras);

            return bitmap;
        }
        catch
        {
            return null;
        }
    }

    [RelayCommand]
    private async Task EnableAuthenticatorAsync()
    {
        if(_twoFactorService is null) return;
        if(string.IsNullOrWhiteSpace(AuthVerifyCode))
        {
            SecurityMessage = _localizer["Verification code is required."];

            return;
        }

        IsSecurityBusy = true;

        var (ok, codes, err) = await _twoFactorService.EnableAuthenticatorAsync(AuthVerifyCode.Trim());

        IsSecurityBusy = false;

        if(!ok)
        {
            SecurityMessage = err ?? _localizer["Invalid verification code."];

            return;
        }

        AuthSetupVisible = false;
        SecurityMessage  = _localizer["Authenticator enabled."];

        if(codes is { Count: > 0 }) DisplayedRecoveryCodes = codes.ToList();

        await RefreshTwoFactorStatusAsync();
    }

    [RelayCommand]
    private void CancelAuthenticatorSetup()
    {
        AuthSetupVisible = false;
        AuthSharedKey    = string.Empty;
        AuthUri          = string.Empty;
        AuthQrImage      = null;
        AuthVerifyCode   = string.Empty;
    }

    [RelayCommand]
    private async Task StartEmailEnableAsync()
    {
        if(_twoFactorService is null) return;

        IsSecurityBusy = true;

        var (ok, err) = await _twoFactorService.StartEmailEnableAsync();

        IsSecurityBusy = false;

        if(ok)
        {
            EmailEnableVisible = true;
            SecurityMessage    = _localizer["Code sent to your email."];
        }
        else
        {
            SecurityMessage = err ?? _localizer["Could not send code."];
        }
    }

    [RelayCommand]
    private async Task EnableEmailAsync()
    {
        if(_twoFactorService is null) return;

        if(string.IsNullOrWhiteSpace(EmailEnablePassword) || string.IsNullOrWhiteSpace(EmailEnableCode))
        {
            SecurityMessage = _localizer["Password and code are required."];

            return;
        }

        IsSecurityBusy = true;

        var (ok, codes, err) = await _twoFactorService.EnableEmailAsync(EmailEnablePassword,
                                                                         EmailEnableCode.Trim());

        IsSecurityBusy = false;

        if(!ok)
        {
            SecurityMessage = err ?? _localizer["Invalid verification code."];

            return;
        }

        EmailEnableVisible   = false;
        EmailEnablePassword  = string.Empty;
        EmailEnableCode      = string.Empty;
        SecurityMessage      = _localizer["Email two-factor enabled."];

        if(codes is { Count: > 0 }) DisplayedRecoveryCodes = codes.ToList();

        await RefreshTwoFactorStatusAsync();
    }

    [RelayCommand]
    private void DismissRecoveryCodes() => DisplayedRecoveryCodes = new List<string>();

    [RelayCommand]
    private async Task SaveNotificationPreferencesAsync()
    {
        if(_authService is null) return;

        IsNotificationPreferencesBusy = true;
        NotificationPreferencesMessage = string.Empty;

        var (succeeded, errorMessage) = await _authService.UpdateNotificationPreferencesAsync(
            new UpdateNotificationPreferencesRequest
            {
                NotifyOnNewMessage = NotifyOnNewMessage
            });

        IsNotificationPreferencesBusy = false;
        NotificationPreferencesMessage = succeeded
            ? "Notification preferences updated successfully."
            : errorMessage ?? "Failed to update notification preferences.";
        OnPropertyChanged(nameof(HasNotificationPreferencesMessage));
    }

    async Task LoadNotificationPreferencesAsync()
    {
        if(_authService is null) return;

        NotificationPreferencesDto? preferences = await _authService.GetNotificationPreferencesAsync();
        if(preferences is not null)
            NotifyOnNewMessage = preferences.NotifyOnNewMessage ?? true;
    }
}
