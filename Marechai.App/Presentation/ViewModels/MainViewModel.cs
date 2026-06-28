using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Marechai.ApiClient.Models;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Views.Admin;
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
    private readonly SearchService          _searchService;
    private readonly IRegionManager         _regionManager;
    private readonly ITokenService          _tokenService;
    private readonly MessageNotificationStateService _messageNotificationStateService;
    private readonly ToastActivationService _toastActivationService;
    [ObservableProperty]
    private bool _isAdminSidebarActive;
    [ObservableProperty]
    private bool _isSidebarOpen = true;
    [ObservableProperty]
    private bool _isAdminUser;
    [ObservableProperty]
    private string _loginLogoutButtonText = "";

    [ObservableProperty]
    private string _globalSearchQuery = "";

    [ObservableProperty]
    private ObservableCollection<SearchResultDto> _globalSearchSuggestions = [];

    // Sidebar localized labels
    public string SidebarTitleText               => _localizer["SidebarTitle"];
    public string NewsButtonText                 => _localizer["NewsButton"];
    public string AdvancedSearchButtonText        => _localizer["AdvancedSearchButton"];
    public string BooksButtonText                => _localizer["BooksButton"];
    public string CompaniesButtonText            => _localizer["CompaniesButton"];
    public string ComputersButtonText            => _localizer["ComputersButton"];
    public string ConsolesButtonText             => _localizer["ConsolesButton"];
    public string SmartphonesButtonText           => _localizer["SmartphonesButton"];
    public string PdasButtonText                  => _localizer["PdasButton"];
    public string TabletsButtonText               => _localizer["TabletsButton"];
    public string DocumentsButtonText            => _localizer["DocumentsButton"];
    public string DumpsButtonText                => _localizer["DumpsButton"];
    public string GpuButtonText                  => _localizer["GraphicalProcessingUnitsButton"];
    public string MagazinesButtonText            => _localizer["MagazinesButton"];
    public string PeopleButtonText               => _localizer["PeopleButton"];
    public string ProcessorsButtonText           => _localizer["ProcessorsButton"];
    public string SoftwareButtonText             => _localizer["SoftwareButton"];
    public string SoundSynthesizersButtonText    => _localizer["SoundSynthesizersButton"];
    public string MessagesButtonText             => "Messages";
    public string UserManagementButtonText       => _localizer["UserManagementButton"];
    public string CompanyManagementButtonText    => _localizer["CompanyManagementButton"];
    public string GpuManagementButtonText         => _localizer["GpuManagementButton"];
    public string ProcessorManagementButtonText   => _localizer["ProcessorManagementButton"];
    public string InstructionSetManagementButtonText => _localizer["InstructionSetManagementButton"];
    public string ISExtensionManagementButtonText   => _localizer["ISExtensionManagementButton"];
    public string ResolutionManagementButtonText    => _localizer["ResolutionManagementButton"];
    public string SoundSynthManagementButtonText    => _localizer["SoundSynthManagementButton"];
    public string ScreenManagementButtonText         => _localizer["ScreenManagementButton"];
    public string MachineManagementButtonText        => _localizer["MachineManagementButton"];
    public string MachineFamilyManagementButtonText  => _localizer["MachineFamilyManagementButton"];
    public string PeopleManagementButtonText       => _localizer["PeopleManagementButton"];
    public string BookManagementButtonText          => _localizer["BookManagementButton"];
    public string DocumentManagementButtonText      => _localizer["DocumentManagementButton"];
    public string MagazineManagementButtonText      => _localizer["MagazineManagementButton"];
    public string SoftwareAdminManagementButtonText   => _localizer["SoftwareAdminManagementButton"];
    public string SoftwareReleaseManagementButtonText    => _localizer["SoftwareReleaseManagementButton"];
    public string SoftwareScreenshotManagementButtonText => _localizer["SoftwareScreenshotManagementButton"];
    public string SoftwarePlatformManagementButtonText => _localizer["SoftwarePlatformManagementButton"];
    public string ExternalSiteManagementButtonText     => _localizer["ExternalSiteManagementButton"];
    public string SoftwareFamilyManagementButtonText  => _localizer["SoftwareFamilyManagementButton"];
    public string WwpcImportsManagementButtonText     => _localizer["WwpcImportsManagementButton"];
    public string MessageReportsManagementButtonText  => "Message Reports";
    public string SettingsButtonText             => _localizer["SettingsButton"];
    public string AboutButtonText                => _localizer["AboutButton"];
    public string ContactButtonText              => _localizer["ContactButton"];
    public string AdminSidebarTitleText          => _localizer["AdminSidebarTitle"];
    public string AdminSwitchButtonText          => _localizer["AdminSwitchButton"];
    public string BackToMainButtonText           => _localizer["BackToMainButton"];

    [ObservableProperty]
    private string _name;
    [ObservableProperty]
    private NewsViewModel _newsViewModel;
    [ObservableProperty]
    private bool _sidebarContentVisible = true;
    [ObservableProperty]
    private bool _isAuthenticatedUser;
    [ObservableProperty]
    private int _unreadMessagesCount;

    public MainViewModel(IStringLocalizer localizer, IOptions<AppConfig> appInfo, IRegionManager regionManager,
                         NewsViewModel newsViewModel, SearchService searchService,
                         IAuthenticationService authService, IJwtService jwtService, ITokenService tokenService,
                         MessageNotificationStateService messageNotificationStateService,
                         ToastActivationService toastActivationService)
    {
        _regionManager = regionManager;
        _localizer     = localizer;
        _authService   = authService;
        _jwtService    = jwtService;
        _tokenService  = tokenService;
        _searchService = searchService;
        _messageNotificationStateService = messageNotificationStateService;
        _toastActivationService          = toastActivationService;
        NewsViewModel  = newsViewModel;
        Title          = localizer["ApplicationName"];
        if(appInfo?.Value?.Environment != null) Title += $" - {appInfo.Value.Environment}";

        // Initialize commands
        NavigateToNewsCommand                     = new RelayCommand(() => NavigateTo(nameof(NewsPage)));
        NavigateToBooksCommand                    = new RelayCommand(() => NavigateTo(nameof(BooksPage)));
        NavigateToCompaniesCommand                = new RelayCommand(() => NavigateTo(nameof(CompaniesPage)));
        NavigateToComputersCommand                = new RelayCommand(() => NavigateTo(nameof(ComputersPage)));
        NavigateToConsolesCommand                 = new RelayCommand(() => NavigateTo(nameof(ConsolesPage)));
        NavigateToSmartphonesCommand              = new RelayCommand(() => NavigateTo(nameof(SmartphonesPage)));
        NavigateToPdasCommand                      = new RelayCommand(() => NavigateTo(nameof(PdasPage)));
        NavigateToTabletsCommand                   = new RelayCommand(() => NavigateTo(nameof(TabletsPage)));
        NavigateToDocumentsCommand                = new RelayCommand(() => NavigateTo(nameof(DocumentsPage)));
        NavigateToDumpsCommand                    = new RelayCommand(() => NavigateTo("dumps"));
        NavigateToGraphicalProcessingUnitsCommand = new RelayCommand(() => NavigateTo(nameof(GpuListPage)));
        NavigateToMagazinesCommand                = new RelayCommand(() => NavigateTo(nameof(MagazinesPage)));
        NavigateToPeopleCommand                   = new RelayCommand(() => NavigateTo(nameof(PeoplePage)));
        NavigateToProcessorsCommand               = new RelayCommand(() => NavigateTo(nameof(ProcessorListPage)));
        NavigateToSoftwareCommand                 = new RelayCommand(() => NavigateTo(nameof(SoftwarePage)));
        NavigateToSoundSynthesizersCommand        = new RelayCommand(() => NavigateTo(nameof(SoundSynthListPage)));
        NavigateToMessagesCommand                 = new RelayCommand(() => NavigateTo(nameof(MessagesPage)));
        NavigateToUsersCommand                    = new RelayCommand(() => NavigateTo(nameof(UsersPage)));
        NavigateToAdminCompaniesCommand            = new RelayCommand(() => NavigateTo(nameof(AdminCompaniesPage)));
        NavigateToAdminGpusCommand                 = new RelayCommand(() => NavigateTo(nameof(AdminGpusPage)));
        NavigateToAdminProcessorsCommand            = new RelayCommand(() => NavigateTo(nameof(AdminProcessorsPage)));
        NavigateToAdminInstructionSetsCommand        = new RelayCommand(() => NavigateTo(nameof(AdminInstructionSetsPage)));
        NavigateToAdminISExtensionsCommand           = new RelayCommand(() => NavigateTo(nameof(AdminInstructionSetExtensionsPage)));
        NavigateToAdminResolutionsCommand             = new RelayCommand(() => NavigateTo(nameof(AdminResolutionsPage)));
        NavigateToAdminSoundSynthsCommand              = new RelayCommand(() => NavigateTo(nameof(AdminSoundSynthsPage)));
        NavigateToAdminScreensCommand                   = new RelayCommand(() => NavigateTo(nameof(AdminScreensPage)));
        NavigateToAdminMachinesCommand                  = new RelayCommand(() => NavigateTo(nameof(AdminMachinesPage)));
        NavigateToAdminMachineFamiliesCommand            = new RelayCommand(() => NavigateTo(nameof(AdminMachineFamiliesPage)));
        NavigateToAdminPeopleCommand                        = new RelayCommand(() => NavigateTo(nameof(AdminPeoplePage)));
        NavigateToAdminBooksCommand                         = new RelayCommand(() => NavigateTo(nameof(AdminBooksPage)));
        NavigateToAdminDocumentsCommand                     = new RelayCommand(() => NavigateTo(nameof(AdminDocumentsPage)));
        NavigateToAdminMagazinesCommand                     = new RelayCommand(() => NavigateTo(nameof(AdminMagazinesPage)));
        NavigateToAdminSoftwareCommand                      = new RelayCommand(() => NavigateTo(nameof(AdminSoftwarePage)));
        NavigateToAdminSoftwareReleasesCommand              = new RelayCommand(() => NavigateTo(nameof(AdminSoftwareReleasesPage)));
        NavigateToAdminSoftwareScreenshotsCommand             = new RelayCommand(() => NavigateTo(nameof(AdminSoftwareScreenshotsPage)));
        NavigateToAdminSoftwarePlatformsCommand             = new RelayCommand(() => NavigateTo(nameof(AdminSoftwarePlatformsPage)));
        NavigateToAdminExternalSitesCommand                 = new RelayCommand(() => NavigateTo(nameof(AdminExternalSitesPage)));
        NavigateToAdminSoftwareFamiliesCommand              = new RelayCommand(() => NavigateTo(nameof(AdminSoftwareFamiliesPage)));
        NavigateToAdminWwpcImportsCommand                   = new RelayCommand(() => NavigateTo(nameof(AdminWwpcImportsPage)));
        NavigateToAdminMessageReportsCommand               = new RelayCommand(() => NavigateTo(nameof(AdminMessageReportsPage)));
        NavigateToSettingsCommand                 = new RelayCommand(() => NavigateTo(nameof(SettingsPage)));
        NavigateToAboutCommand                    = new RelayCommand(() => NavigateTo(nameof(AboutPage)));
        NavigateToContactCommand                  = new RelayCommand(() => NavigateTo(nameof(ContactPage)));
        NavigateToAdvancedSearchCommand            = new RelayCommand(() => NavigateToAdvancedSearch(null));
        LoginLogoutCommand                        = new RelayCommand(HandleLoginLogout);
        ToggleSidebarCommand                      = new RelayCommand(() => IsSidebarOpen = !IsSidebarOpen);
        SwitchToAdminSidebarCommand               = new RelayCommand(() => IsAdminSidebarActive = true);
        SwitchToMainSidebarCommand                = new RelayCommand(() => IsAdminSidebarActive = false);
        NavigateToSearchResultCommand              = new RelayCommand<SearchResultDto>(NavigateToSearchResult);
        SubmitGlobalSearchCommand                  = new RelayCommand(() => NavigateToAdvancedSearch(GlobalSearchQuery));

        // Subscribe to authentication events
        _authService.LoggedOut += OnLoggedOut;

        if(_authService is AuthService concreteAuthService)
            concreteAuthService.LoggedIn += OnLoggedIn;

        _messageNotificationStateService.UnreadCountChanged += OnUnreadCountChanged;
        _toastActivationService.ConversationActivated       += OnConversationActivated;
        TryOpenPendingConversation();

        UpdateLoginLogoutButtonText();
        UpdateAdminStatus();
        UpdateAuthenticatedStatus();
    }

    public string Title { get; }

    public ICommand NavigateToNewsCommand                     { get; }
    public ICommand NavigateToBooksCommand                    { get; }
    public ICommand NavigateToCompaniesCommand                { get; }
    public ICommand NavigateToComputersCommand                { get; }
    public ICommand NavigateToConsolesCommand                 { get; }
    public ICommand NavigateToSmartphonesCommand              { get; }
    public ICommand NavigateToPdasCommand                     { get; }
    public ICommand NavigateToTabletsCommand                  { get; }
    public ICommand NavigateToDocumentsCommand                { get; }
    public ICommand NavigateToDumpsCommand                    { get; }
    public ICommand NavigateToGraphicalProcessingUnitsCommand { get; }
    public ICommand NavigateToMagazinesCommand                { get; }
    public ICommand NavigateToPeopleCommand                   { get; }
    public ICommand NavigateToProcessorsCommand               { get; }
    public ICommand NavigateToSoftwareCommand                 { get; }
    public ICommand NavigateToSoundSynthesizersCommand        { get; }
    public ICommand NavigateToMessagesCommand                 { get; }
    public ICommand NavigateToUsersCommand                    { get; }
    public ICommand NavigateToAdminCompaniesCommand           { get; }
    public ICommand NavigateToAdminGpusCommand                { get; }
    public ICommand NavigateToAdminProcessorsCommand          { get; }
    public ICommand NavigateToAdminInstructionSetsCommand     { get; }
    public ICommand NavigateToAdminISExtensionsCommand        { get; }
    public ICommand NavigateToAdminResolutionsCommand         { get; }
    public ICommand NavigateToAdminSoundSynthsCommand         { get; }
    public ICommand NavigateToAdminScreensCommand              { get; }
    public ICommand NavigateToAdminMachinesCommand             { get; }
    public ICommand NavigateToAdminMachineFamiliesCommand      { get; }
    public ICommand NavigateToAdminPeopleCommand              { get; }
    public ICommand NavigateToAdminBooksCommand               { get; }
    public ICommand NavigateToAdminDocumentsCommand           { get; }
    public ICommand NavigateToAdminMagazinesCommand           { get; }
    public ICommand NavigateToAdminSoftwareCommand            { get; }
    public ICommand NavigateToAdminSoftwareReleasesCommand    { get; }
    public ICommand NavigateToAdminSoftwareScreenshotsCommand { get; }
    public ICommand NavigateToAdminSoftwarePlatformsCommand   { get; }
    public ICommand NavigateToAdminExternalSitesCommand       { get; }
    public ICommand NavigateToAdminSoftwareFamiliesCommand    { get; }
    public ICommand NavigateToAdminWwpcImportsCommand         { get; }
    public ICommand NavigateToAdminMessageReportsCommand      { get; }
    public ICommand NavigateToSettingsCommand                 { get; }
    public ICommand NavigateToAboutCommand                    { get; }
    public ICommand NavigateToContactCommand                  { get; }
    public ICommand NavigateToAdvancedSearchCommand           { get; }
    public ICommand LoginLogoutCommand                        { get; }
    public ICommand ToggleSidebarCommand                      { get; }
    public ICommand SwitchToAdminSidebarCommand               { get; }
    public ICommand SwitchToMainSidebarCommand                { get; }
    public ICommand NavigateToSearchResultCommand              { get; }
    public ICommand SubmitGlobalSearchCommand                  { get; }

    private async void UpdateLoginLogoutButtonText()
    {
        bool isAuthenticated = await _authService.IsAuthenticated(CancellationToken.None);
        LoginLogoutButtonText = isAuthenticated ? _localizer["Logout"] : _localizer["Login"];
        IsAuthenticatedUser   = isAuthenticated;
    }

    private void UpdateAdminStatus()
    {
        try
        {
            string token = _tokenService.GetToken();

            if(!string.IsNullOrWhiteSpace(token))
            {
                IEnumerable<string> roles = _jwtService.GetRoles(token);

                IsAdminUser = roles.Contains("Uberadmin", StringComparer.OrdinalIgnoreCase) ||
                              roles.Contains("Admin",     StringComparer.OrdinalIgnoreCase);
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

    private void OnLoggedOut(object sender, EventArgs e)
    {
        UpdateLoginLogoutButtonText();
        UpdateAdminStatus();
        UpdateAuthenticatedStatus();
    }

    private void OnLoggedIn(object sender, EventArgs e)
    {
        UpdateLoginLogoutButtonText();
        UpdateAdminStatus();
        UpdateAuthenticatedStatus();
        _ = _messageNotificationStateService.RefreshAsync(seedBaseline: true);
    }

    public void RefreshAuthenticationState()
    {
        // Public method to refresh authentication state (called after login)
        UpdateLoginLogoutButtonText();
        UpdateAdminStatus();
        UpdateAuthenticatedStatus();
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

    void OnUnreadCountChanged(object? sender, int count) => UnreadMessagesCount = count;

    void OnConversationActivated(object? sender, long conversationId)
    {
        var parameters = new NavigationParameters
        {
            { NavParamKeys.ConversationId, conversationId }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(MessageThreadPage), parameters);
        _toastActivationService.ClearPendingConversation();
    }

    void TryOpenPendingConversation()
    {
        if(_toastActivationService.TryConsumePendingConversation(out long conversationId))
            OnConversationActivated(this, conversationId);
    }

    private async void UpdateAuthenticatedStatus()
    {
        IsAuthenticatedUser = await _authService.IsAuthenticated(CancellationToken.None);
    }

    partial void OnGlobalSearchQueryChanged(string value)
    {
        _ = UpdateGlobalSearchSuggestionsAsync(value);
    }

    private async Task UpdateGlobalSearchSuggestionsAsync(string query)
    {
        string trimmed = query?.Trim() ?? string.Empty;

        if(trimmed.Length < 3)
        {
            GlobalSearchSuggestions = [];

            return;
        }

        List<SearchResultDto> results = await _searchService.AutocompleteAsync(trimmed);
        GlobalSearchSuggestions = new ObservableCollection<SearchResultDto>(results);
    }

    private void NavigateToSearchResult(SearchResultDto? result)
    {
        if(result is null) return;

        SearchResultNavigator.NavigateTo(_regionManager, result);
        GlobalSearchQuery = string.Empty;
    }

    private void NavigateToAdvancedSearch(string? query)
    {
        var parameters = new NavigationParameters();

        if(!string.IsNullOrWhiteSpace(query)) parameters.Add(NavParamKeys.SearchQuery, query);

        _regionManager.RequestNavigate(RegionNames.Content, nameof(AdvancedSearchPage), parameters);
        GlobalSearchQuery = string.Empty;
    }

    public Task InitializeMessagingAsync() => _messageNotificationStateService.InitializeAsync();
}
