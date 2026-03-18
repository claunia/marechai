using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Views;
using Marechai.App.Services;
using Marechai.App.Services.Authentication;
using Uno.Extensions.Authentication;

namespace Marechai.App.Presentation.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IAuthenticationService _authService;
    private readonly IJwtService            _jwtService;
    private readonly IStringLocalizer       _localizer;
    private readonly IRegionManager         _regionManager;
    private readonly ITokenService          _tokenService;
    [ObservableProperty]
    private bool _isSidebarOpen = true;
    [ObservableProperty]
    private bool _isUberadminUser;
    [ObservableProperty]
    private string _loginLogoutButtonText = "";

    // Sidebar localized labels (x:Uid doesn't work under PrismApplication)
    public string SidebarTitleText               => _localizer["SidebarTitle.Text"];
    public string NewsButtonText                 => _localizer["NewsButton.Content"];
    public string BooksButtonText                => _localizer["BooksButton.Content"];
    public string CompaniesButtonText            => _localizer["CompaniesButton.Content"];
    public string ComputersButtonText            => _localizer["ComputersButton.Content"];
    public string ConsolesButtonText             => _localizer["ConsolesButton.Content"];
    public string DocumentsButtonText            => _localizer["DocumentsButton.Content"];
    public string DumpsButtonText                => _localizer["DumpsButton.Content"];
    public string GpuButtonText                  => _localizer["GraphicalProcessingUnitsButton.Content"];
    public string MagazinesButtonText            => _localizer["MagazinesButton.Content"];
    public string PeopleButtonText               => _localizer["PeopleButton.Content"];
    public string ProcessorsButtonText           => _localizer["ProcessorsButton.Content"];
    public string SoftwareButtonText             => _localizer["SoftwareButton.Content"];
    public string SoundSynthesizersButtonText    => _localizer["SoundSynthesizersButton.Content"];
    public string UserManagementButtonText       => _localizer["UserManagementButton.Content"];
    public string SettingsButtonText             => _localizer["SettingsButton.Content"];

    [ObservableProperty]
    private string? _name;
    [ObservableProperty]
    private NewsViewModel? _newsViewModel;
    [ObservableProperty]
    private bool _sidebarContentVisible = true;

    public MainViewModel(IStringLocalizer localizer, IOptions<AppConfig> appInfo, IRegionManager regionManager,
                         NewsViewModel newsViewModel,
                         IAuthenticationService authService, IJwtService jwtService, ITokenService tokenService)
    {
        _regionManager = regionManager;
        _localizer     = localizer;
        _authService   = authService;
        _jwtService    = jwtService;
        _tokenService  = tokenService;
        NewsViewModel  = newsViewModel;
        Title          = localizer["ApplicationName"];
        if(appInfo?.Value?.Environment != null) Title += $" - {appInfo.Value.Environment}";

        // Initialize commands
        NavigateToNewsCommand                     = new RelayCommand(() => NavigateTo(nameof(NewsPage)));
        NavigateToBooksCommand                    = new RelayCommand(() => NavigateTo("books"));
        NavigateToCompaniesCommand                = new RelayCommand(() => NavigateTo(nameof(CompaniesPage)));
        NavigateToComputersCommand                = new RelayCommand(() => NavigateTo(nameof(ComputersPage)));
        NavigateToConsolesCommand                 = new RelayCommand(() => NavigateTo(nameof(ConsolesPage)));
        NavigateToDocumentsCommand                = new RelayCommand(() => NavigateTo("documents"));
        NavigateToDumpsCommand                    = new RelayCommand(() => NavigateTo("dumps"));
        NavigateToGraphicalProcessingUnitsCommand = new RelayCommand(() => NavigateTo(nameof(GpuListPage)));
        NavigateToMagazinesCommand                = new RelayCommand(() => NavigateTo("magazines"));
        NavigateToPeopleCommand                   = new RelayCommand(() => NavigateTo("people"));
        NavigateToProcessorsCommand               = new RelayCommand(() => NavigateTo(nameof(ProcessorListPage)));
        NavigateToSoftwareCommand                 = new RelayCommand(() => NavigateTo("software"));
        NavigateToSoundSynthesizersCommand        = new RelayCommand(() => NavigateTo(nameof(SoundSynthListPage)));
        NavigateToUsersCommand                    = new RelayCommand(() => NavigateTo(nameof(UsersPage)));
        NavigateToSettingsCommand                 = new RelayCommand(() => NavigateTo(nameof(SettingsPage)));
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
            // Navigate to login page in the shell region
            _regionManager.RequestNavigate(RegionNames.Shell, nameof(LoginPage));
        }
    }

    private void NavigateTo(string viewName)
    {
        _regionManager.RequestNavigate(RegionNames.Content, viewName);
    }
}