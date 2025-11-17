using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Marechai.App.Services;
using Marechai.App.Services.Authentication;
using Uno.Extensions.Authentication;
using Uno.Extensions.Navigation;
using Uno.Extensions.Toolkit;

namespace Marechai.App.Presentation.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IAuthenticationService _authService;
    private readonly IJwtService            _jwtService;
    private readonly IStringLocalizer       _localizer;
    private readonly INavigator             _navigator;
    private readonly ITokenService          _tokenService;
    [ObservableProperty]
    private bool _isSidebarOpen = true;
    [ObservableProperty]
    private bool _isUberadminUser;
    [ObservableProperty]
    private string _loginLogoutButtonText = "";

    [ObservableProperty]
    private string? _name;
    [ObservableProperty]
    private NewsViewModel? _newsViewModel;
    [ObservableProperty]
    private bool _sidebarContentVisible = true;

    public MainViewModel(IStringLocalizer localizer, IOptions<AppConfig> appInfo, INavigator navigator,
                         NewsViewModel newsViewModel, IColorThemeService colorThemeService, IThemeService themeService,
                         IAuthenticationService authService, IJwtService jwtService, ITokenService tokenService)
    {
        _navigator    =  navigator;
        _localizer    =  localizer;
        _authService  =  authService;
        _jwtService   =  jwtService;
        _tokenService =  tokenService;
        NewsViewModel =  newsViewModel;
        Title         =  localizer["ApplicationName"];
        if(appInfo?.Value?.Environment != null) Title += $" - {appInfo.Value.Environment}";

        // Initialize color theme service with theme service
        _ = InitializeThemeServicesAsync(colorThemeService, themeService);

        // Initialize commands
        NavigateToNewsCommand                     = new AsyncRelayCommand(NavigateToMainAsync);
        NavigateToBooksCommand                    = new AsyncRelayCommand(() => NavigateTo("books"));
        NavigateToCompaniesCommand                = new AsyncRelayCommand(() => NavigateTo("companies"));
        NavigateToComputersCommand                = new AsyncRelayCommand(() => NavigateTo("computers"));
        NavigateToConsolesCommand                 = new AsyncRelayCommand(() => NavigateTo("consoles"));
        NavigateToDocumentsCommand                = new AsyncRelayCommand(() => NavigateTo("documents"));
        NavigateToDumpsCommand                    = new AsyncRelayCommand(() => NavigateTo("dumps"));
        NavigateToGraphicalProcessingUnitsCommand = new AsyncRelayCommand(() => NavigateTo("gpus"));
        NavigateToMagazinesCommand                = new AsyncRelayCommand(() => NavigateTo("magazines"));
        NavigateToPeopleCommand                   = new AsyncRelayCommand(() => NavigateTo("people"));
        NavigateToProcessorsCommand               = new AsyncRelayCommand(() => NavigateTo("processors"));
        NavigateToSoftwareCommand                 = new AsyncRelayCommand(() => NavigateTo("software"));
        NavigateToSoundSynthesizersCommand        = new AsyncRelayCommand(() => NavigateTo("sound-synths"));
        NavigateToUsersCommand                    = new AsyncRelayCommand(() => NavigateTo("users"));
        NavigateToSettingsCommand                 = new AsyncRelayCommand(() => NavigateTo("settings"));
        LoginLogoutCommand                        = new RelayCommand(HandleLoginLogout);
        ToggleSidebarCommand                      = new RelayCommand(() => IsSidebarOpen = !IsSidebarOpen);

        // Subscribe to authentication events
        _authService.LoggedOut += OnLoggedOut;

        UpdateLoginLogoutButtonText();
        UpdateUberadminStatus();
    }

    public string? Title { get; }

    public ICommand NavigateToNewsCommand                     { get; }
    public ICommand NavigateToBooksCommand                    { get; }
    public ICommand NavigateToCompaniesCommand                { get; }
    public ICommand NavigateToComputersCommand                { get; }
    public ICommand NavigateToConsolesCommand                 { get; }
    public ICommand NavigateToDocumentsCommand                { get; }
    public ICommand NavigateToDumpsCommand                    { get; }
    public ICommand NavigateToGraphicalProcessingUnitsCommand { get; }
    public ICommand NavigateToMagazinesCommand                { get; }
    public ICommand NavigateToPeopleCommand                   { get; }
    public ICommand NavigateToProcessorsCommand               { get; }
    public ICommand NavigateToSoftwareCommand                 { get; }
    public ICommand NavigateToSoundSynthesizersCommand        { get; }
    public ICommand NavigateToUsersCommand                    { get; }
    public ICommand NavigateToSettingsCommand                 { get; }
    public ICommand LoginLogoutCommand                        { get; }
    public ICommand ToggleSidebarCommand                      { get; }

    private async Task InitializeThemeServicesAsync(IColorThemeService colorThemeService, IThemeService themeService)
    {
        try
        {
            // Wait for theme service to be ready
            await themeService.InitializeAsync();

            // Set the theme service reference and reapply the saved theme
            colorThemeService.SetThemeService(themeService);
        }
        catch
        {
            // Silently fail - theme will work but without refresh on startup
        }
    }

    private async void UpdateLoginLogoutButtonText()
    {
        bool isAuthenticated = await _authService.IsAuthenticated(CancellationToken.None);
        LoginLogoutButtonText = isAuthenticated ? _localizer["Logout"] : _localizer["Login"];
    }

    private void UpdateUberadminStatus()
    {
        try
        {
            string token = _tokenService.GetToken();

            if(!string.IsNullOrWhiteSpace(token))
            {
                IEnumerable<string> roles = _jwtService.GetRoles(token);
                IsUberadminUser = roles.Contains("Uberadmin", StringComparer.OrdinalIgnoreCase);
            }
            else
                IsUberadminUser = false;
        }
        catch
        {
            IsUberadminUser = false;
        }
    }

    private void OnLoggedOut(object? sender, EventArgs e)
    {
        // Update button text when user logs out
        UpdateLoginLogoutButtonText();
        UpdateUberadminStatus();
    }

    public void RefreshAuthenticationState()
    {
        // Public method to refresh authentication state (called after login)
        UpdateLoginLogoutButtonText();
        UpdateUberadminStatus();
    }

    private async void HandleLoginLogout()
    {
        bool isAuthenticated = await _authService.IsAuthenticated(CancellationToken.None);

        if(isAuthenticated)
        {
            // Logout
            await _authService.LogoutAsync(null, CancellationToken.None);
            UpdateLoginLogoutButtonText();
            UpdateUberadminStatus();
        }
        else
        {
            // Navigate to login page - use absolute path starting from root
            await _navigator.NavigateRouteAsync(this, "/Login");
        }
    }

    private async Task NavigateTo(string destination)
    {
        try
        {
            // Navigate within the Main region using relative navigation
            // The "./" prefix means navigate within the current page's region
            await _navigator.NavigateRouteAsync(this, $"./{destination}");
        }
        catch(Exception)
        {
            // Navigation error - fail silently for now
            // TODO: Add error handling/logging
        }
    }

    private async Task NavigateToMainAsync()
    {
        // Navigate to News page (the default/home page)
        await NavigateTo("News");
    }
}