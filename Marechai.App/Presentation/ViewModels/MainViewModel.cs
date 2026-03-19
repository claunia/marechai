using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Views;
using Marechai.App.Presentation.Views.Admin;
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
    private bool _isAdminSidebarActive;
    [ObservableProperty]
    private bool _isSidebarOpen = true;
    [ObservableProperty]
    private bool _isAdminUser;
    [ObservableProperty]
    private string _loginLogoutButtonText = "";

    // Sidebar localized labels
    public string SidebarTitleText               => _localizer["SidebarTitle"];
    public string NewsButtonText                 => _localizer["NewsButton"];
    public string BooksButtonText                => _localizer["BooksButton"];
    public string CompaniesButtonText            => _localizer["CompaniesButton"];
    public string ComputersButtonText            => _localizer["ComputersButton"];
    public string ConsolesButtonText             => _localizer["ConsolesButton"];
    public string DocumentsButtonText            => _localizer["DocumentsButton"];
    public string DumpsButtonText                => _localizer["DumpsButton"];
    public string GpuButtonText                  => _localizer["GraphicalProcessingUnitsButton"];
    public string MagazinesButtonText            => _localizer["MagazinesButton"];
    public string PeopleButtonText               => _localizer["PeopleButton"];
    public string ProcessorsButtonText           => _localizer["ProcessorsButton"];
    public string SoftwareButtonText             => _localizer["SoftwareButton"];
    public string SoundSynthesizersButtonText    => _localizer["SoundSynthesizersButton"];
    public string UserManagementButtonText       => _localizer["UserManagementButton"];
    public string CompanyManagementButtonText    => _localizer["CompanyManagementButton"];
    public string GpuManagementButtonText         => _localizer["GpuManagementButton"];
    public string ProcessorManagementButtonText   => _localizer["ProcessorManagementButton"];
    public string InstructionSetManagementButtonText => _localizer["InstructionSetManagementButton"];
    public string ISExtensionManagementButtonText   => _localizer["ISExtensionManagementButton"];
    public string ResolutionManagementButtonText    => _localizer["ResolutionManagementButton"];
    public string SettingsButtonText             => _localizer["SettingsButton"];
    public string AdminSidebarTitleText          => _localizer["AdminSidebarTitle"];
    public string AdminSwitchButtonText          => _localizer["AdminSwitchButton"];
    public string BackToMainButtonText           => _localizer["BackToMainButton"];

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
        NavigateToAdminCompaniesCommand            = new RelayCommand(() => NavigateTo(nameof(AdminCompaniesPage)));
        NavigateToAdminGpusCommand                 = new RelayCommand(() => NavigateTo(nameof(AdminGpusPage)));
        NavigateToAdminProcessorsCommand            = new RelayCommand(() => NavigateTo(nameof(AdminProcessorsPage)));
        NavigateToAdminInstructionSetsCommand        = new RelayCommand(() => NavigateTo(nameof(AdminInstructionSetsPage)));
        NavigateToAdminISExtensionsCommand           = new RelayCommand(() => NavigateTo(nameof(AdminInstructionSetExtensionsPage)));
        NavigateToAdminResolutionsCommand             = new RelayCommand(() => NavigateTo(nameof(AdminResolutionsPage)));
        NavigateToSettingsCommand                 = new RelayCommand(() => NavigateTo(nameof(SettingsPage)));
        LoginLogoutCommand                        = new RelayCommand(HandleLoginLogout);
        ToggleSidebarCommand                      = new RelayCommand(() => IsSidebarOpen = !IsSidebarOpen);
        SwitchToAdminSidebarCommand               = new RelayCommand(() => IsAdminSidebarActive = true);
        SwitchToMainSidebarCommand                = new RelayCommand(() => IsAdminSidebarActive = false);

        // Subscribe to authentication events
        _authService.LoggedOut += OnLoggedOut;

        if(_authService is AuthService concreteAuthService)
            concreteAuthService.LoggedIn += OnLoggedIn;

        UpdateLoginLogoutButtonText();
        UpdateAdminStatus();
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
    public ICommand NavigateToAdminCompaniesCommand           { get; }
    public ICommand NavigateToAdminGpusCommand                { get; }
    public ICommand NavigateToAdminProcessorsCommand          { get; }
    public ICommand NavigateToAdminInstructionSetsCommand     { get; }
    public ICommand NavigateToAdminISExtensionsCommand        { get; }
    public ICommand NavigateToAdminResolutionsCommand         { get; }
    public ICommand NavigateToSettingsCommand                 { get; }
    public ICommand LoginLogoutCommand                        { get; }
    public ICommand ToggleSidebarCommand                      { get; }
    public ICommand SwitchToAdminSidebarCommand               { get; }
    public ICommand SwitchToMainSidebarCommand                { get; }

    private async void UpdateLoginLogoutButtonText()
    {
        bool isAuthenticated = await _authService.IsAuthenticated(CancellationToken.None);
        LoginLogoutButtonText = isAuthenticated ? _localizer["Logout"] : _localizer["Login"];
    }

    private void UpdateAdminStatus()
    {
        try
        {
            string token = _tokenService.GetToken();

            if(!string.IsNullOrWhiteSpace(token))
            {
                IEnumerable<string> roles = _jwtService.GetRoles(token);

                IsAdminUser = roles.Contains("Uberadmin",      StringComparer.OrdinalIgnoreCase) ||
                              roles.Contains("Administrator", StringComparer.OrdinalIgnoreCase);
            }
            else
            {
                IsAdminUser            = false;
                IsAdminSidebarActive   = false;
            }
        }
        catch
        {
            IsAdminUser            = false;
            IsAdminSidebarActive   = false;
        }
    }

    private void OnLoggedOut(object? sender, EventArgs e)
    {
        UpdateLoginLogoutButtonText();
        UpdateAdminStatus();
    }

    private void OnLoggedIn(object? sender, EventArgs e)
    {
        UpdateLoginLogoutButtonText();
        UpdateAdminStatus();
    }

    public void RefreshAuthenticationState()
    {
        // Public method to refresh authentication state (called after login)
        UpdateLoginLogoutButtonText();
        UpdateAdminStatus();
    }

    private async void HandleLoginLogout()
    {
        bool isAuthenticated = await _authService.IsAuthenticated(CancellationToken.None);

        if(isAuthenticated)
        {
            // Logout
            await _authService.LogoutAsync(null, CancellationToken.None);
            UpdateLoginLogoutButtonText();
            UpdateAdminStatus();
        }
        else
        {
            // Navigate to login page in the content region
            _regionManager.RequestNavigate(RegionNames.Content, nameof(LoginPage));
        }
    }

    private void NavigateTo(string viewName)
    {
        _regionManager.RequestNavigate(RegionNames.Content, viewName);
    }
}