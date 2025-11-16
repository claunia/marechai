using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Marechai.App.Services;
using Uno.Extensions.Authentication;
using Uno.Extensions.Navigation;
using Uno.Extensions.Toolkit;

namespace Marechai.App.Presentation.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IAuthenticationService _authService;
    private readonly IStringLocalizer       _localizer;
    private readonly INavigator             _navigator;
    [ObservableProperty]
    private bool _isSidebarOpen = true;
    [ObservableProperty]
    private Dictionary<string, string> _localizedStrings = new();
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
                         IAuthenticationService authService)
    {
        _navigator    =  navigator;
        _localizer    =  localizer;
        _authService  =  authService;
        NewsViewModel =  newsViewModel;
        Title         =  "Marechai";
        Title         += $" - {localizer["ApplicationName"]}";
        if(appInfo?.Value?.Environment != null) Title += $" - {appInfo.Value.Environment}";

        GoToSecond = new AsyncRelayCommand(GoToSecondView);

        // Initialize color theme service with theme service
        _ = InitializeThemeServicesAsync(colorThemeService, themeService);

        // Initialize localized strings
        InitializeLocalizedStrings();

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
        NavigateToSettingsCommand                 = new AsyncRelayCommand(() => NavigateTo("settings"));
        LoginLogoutCommand                        = new RelayCommand(HandleLoginLogout);
        ToggleSidebarCommand                      = new RelayCommand(() => IsSidebarOpen = !IsSidebarOpen);

        // Subscribe to authentication events
        _authService.LoggedOut += OnLoggedOut;

        UpdateLoginLogoutButtonText();
    }

    public string? Title { get; }

    public ICommand GoToSecond { get; }

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

    private void InitializeLocalizedStrings()
    {
        LocalizedStrings = new Dictionary<string, string>
        {
            {
                "News", _localizer["News"]
            },
            {
                "Books", _localizer["Books"]
            },
            {
                "Companies", _localizer["Companies"]
            },
            {
                "Computers", _localizer["Computers"]
            },
            {
                "Consoles", _localizer["Consoles"]
            },
            {
                "Documents", _localizer["Documents"]
            },
            {
                "Dumps", _localizer["Dumps"]
            },
            {
                "GraphicalProcessingUnits", _localizer["GraphicalProcessingUnits"]
            },
            {
                "Magazines", _localizer["Magazines"]
            },
            {
                "People", _localizer["People"]
            },
            {
                "Processors", _localizer["Processors"]
            },
            {
                "Software", _localizer["Software"]
            },
            {
                "SoundSynthesizers", _localizer["SoundSynthesizers"]
            },
            {
                "Settings", _localizer["Settings"]
            },
            {
                "Login", _localizer["Login"]
            },
            {
                "Logout", _localizer["Logout"]
            }
        };
    }

    private async void UpdateLoginLogoutButtonText()
    {
        bool isAuthenticated = await _authService.IsAuthenticated(CancellationToken.None);
        LoginLogoutButtonText = isAuthenticated ? LocalizedStrings["Logout"] : LocalizedStrings["Login"];
    }

    private void OnLoggedOut(object? sender, EventArgs e)
    {
        // Update button text when user logs out
        UpdateLoginLogoutButtonText();
    }

    public void RefreshAuthenticationState()
    {
        // Public method to refresh authentication state (called after login)
        UpdateLoginLogoutButtonText();
    }

    private async void HandleLoginLogout()
    {
        bool isAuthenticated = await _authService.IsAuthenticated(CancellationToken.None);

        if(isAuthenticated)
        {
            // Logout
            await _authService.LogoutAsync(null, CancellationToken.None);
            UpdateLoginLogoutButtonText();
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

    private async Task GoToSecondView()
    {
        // Navigate to Second view model providing qualifier and data
        await _navigator.NavigateViewModelAsync<SecondViewModel>(this, "Second", new Entity(Name ?? ""));
    }
}