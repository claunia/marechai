using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Uno.Extensions.Navigation;

namespace Marechai.App.Presentation.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IStringLocalizer _localizer;
    private readonly INavigator       _navigator;
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
                         NewsViewModel    newsViewModel)
    {
        _navigator    =  navigator;
        _localizer    =  localizer;
        NewsViewModel =  newsViewModel;
        Title         =  "Marechai";
        Title         += $" - {localizer["ApplicationName"]}";
        if(appInfo?.Value?.Environment != null) Title += $" - {appInfo.Value.Environment}";

        GoToSecond = new AsyncRelayCommand(GoToSecondView);

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
        NavigateToGraphicalProcessingUnitsCommand = new AsyncRelayCommand(() => NavigateTo("gpus/list-gpus"));
        NavigateToMagazinesCommand                = new AsyncRelayCommand(() => NavigateTo("magazines"));
        NavigateToPeopleCommand                   = new AsyncRelayCommand(() => NavigateTo("people"));
        NavigateToProcessorsCommand               = new AsyncRelayCommand(() => NavigateTo("processors"));
        NavigateToSoftwareCommand                 = new AsyncRelayCommand(() => NavigateTo("software"));
        NavigateToSoundSynthesizersCommand        = new AsyncRelayCommand(() => NavigateTo("soundsynthesizers"));
        NavigateToSettingsCommand                 = new AsyncRelayCommand(() => NavigateTo("settings"));
        LoginLogoutCommand                        = new RelayCommand(HandleLoginLogout);
        ToggleSidebarCommand                      = new RelayCommand(() => IsSidebarOpen = !IsSidebarOpen);

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

    private void UpdateLoginLogoutButtonText()
    {
        // TODO: Check if user is logged in
        // For now, always show "Login"
        LoginLogoutButtonText = LocalizedStrings["Login"];
    }

    private static void HandleLoginLogout()
    {
        // TODO: Implement login/logout logic
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