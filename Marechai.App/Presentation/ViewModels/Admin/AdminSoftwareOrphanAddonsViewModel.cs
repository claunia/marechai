#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Marechai.App.Presentation.Views;
using Marechai.App.Services.Authentication;

namespace Marechai.App.Presentation.ViewModels.Admin;

public partial class AdminSoftwareOrphanAddonsViewModel : ObservableObject, IRegionAware
{
    private readonly Client                                      _apiClient;
    private readonly IJwtService                                 _jwtService;
    private readonly IStringLocalizer                            _localizer;
    private readonly ILogger<AdminSoftwareOrphanAddonsViewModel> _logger;
    private readonly IRegionManager                              _regionManager;
    private readonly ITokenService                               _tokenService;

    private List<SoftwareDto>? _baseSoftwareCandidates;

    // --- List state ---
    [ObservableProperty]
    private ObservableCollection<OrphanAddonRow> _orphanAddons = [];

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isDataLoaded;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string? _successMessage;

    [ObservableProperty]
    private bool _isAdmin;

    // --- Filters ---
    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _onlyOrphans = true;

    partial void OnSearchTextChanged(string value) => _ = ReloadAsync();

    partial void OnOnlyOrphansChanged(bool value) => _ = ReloadAsync();

    // --- Pagination ---
    [ObservableProperty]
    private int _currentSkip;

    private const int PageSize = 25;

    [ObservableProperty]
    private int _totalAddons;

    public bool CanGoPrevious => CurrentSkip > 0;
    public bool CanGoNext     => CurrentSkip + PageSize < TotalAddons;

    public string PageInfoText =>
        string.Format(_localizer["SoftwareOrphanAddonsPageInfo"],
                      TotalAddons == 0 ? 0 : (CurrentSkip / PageSize) + 1,
                      TotalAddons == 0 ? 0 : (int)Math.Ceiling(TotalAddons / (double)PageSize));

    public int  SelectedCount => OrphanAddons.Count(r => r.IsSelected);
    public bool HasSelection  => SelectedCount > 0;

    public string BulkLinkButtonText =>
        string.Format(_localizer["SoftwareOrphanAddonsBulkLinkButton"], SelectedCount);

    // --- Link panel state ---
    [ObservableProperty]
    private bool _isLinking;

    [ObservableProperty]
    private List<OrphanAddonRow> _linkTargetRows = [];

    [ObservableProperty]
    private string _linkSearchText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<SoftwareDto> _linkCandidates = [];

    [ObservableProperty]
    private SoftwareDto? _selectedLinkCandidate;

    public bool HasSelectedLinkCandidate => SelectedLinkCandidate != null;

    partial void OnSelectedLinkCandidateChanged(SoftwareDto? value) => OnPropertyChanged(nameof(HasSelectedLinkCandidate));

    public string LinkPanelTitle =>
        LinkTargetRows.Count == 1
            ? _localizer["SoftwareOrphanAddonsLinkSingleTitle"]
            : string.Format(_localizer["SoftwareOrphanAddonsLinkBulkTitle"], LinkTargetRows.Count);

    public AdminSoftwareOrphanAddonsViewModel(Client                                      apiClient,
                                              IJwtService                                 jwtService,
                                              ITokenService                                tokenService,
                                              ILogger<AdminSoftwareOrphanAddonsViewModel> logger,
                                              IStringLocalizer                            localizer,
                                              IRegionManager                              regionManager)
    {
        _apiClient     = apiClient;
        _jwtService    = jwtService;
        _tokenService  = tokenService;
        _logger        = logger;
        _localizer     = localizer;
        _regionManager = regionManager;

        LoadOrphanAddonsCommand = new AsyncRelayCommand(LoadOrphanAddonsAsync);
        NextPageCommand         = new AsyncRelayCommand(NextPageAsync);
        PreviousPageCommand     = new AsyncRelayCommand(PreviousPageAsync);
        OpenLinkDialogCommand   = new RelayCommand<OrphanAddonRow>(OpenLinkDialog);
        OpenBulkLinkDialogCommand = new RelayCommand(OpenBulkLinkDialog);
        ConfirmLinkCommand      = new AsyncRelayCommand(ConfirmLinkAsync);
        CancelLinkCommand       = new RelayCommand(CancelLink);
        OpenInNewWindowCommand  = new RelayCommand<OrphanAddonRow>(OpenInNewWindow);

        CheckAdminRole();
    }

    // --- Commands ---
    public IAsyncRelayCommand              LoadOrphanAddonsCommand   { get; }
    public IAsyncRelayCommand              NextPageCommand           { get; }
    public IAsyncRelayCommand              PreviousPageCommand       { get; }
    public IRelayCommand<OrphanAddonRow>   OpenLinkDialogCommand     { get; }
    public IRelayCommand                   OpenBulkLinkDialogCommand { get; }
    public IAsyncRelayCommand              ConfirmLinkCommand        { get; }
    public IRelayCommand                   CancelLinkCommand         { get; }
    public IRelayCommand<OrphanAddonRow>   OpenInNewWindowCommand    { get; }

    // --- IRegionAware ---
    public bool IsNavigationTarget(NavigationContext navigationContext) => true;

    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        CheckAdminRole();

        if(IsAdmin) _ = LoadOrphanAddonsCommand.ExecuteAsync(null);
    }

    private void CheckAdminRole()
    {
        try
        {
            string token = _tokenService.GetToken();

            if(string.IsNullOrWhiteSpace(token))
            {
                IsAdmin = false;

                return;
            }

            IEnumerable<string> roles = _jwtService.GetRoles(token);

            IsAdmin = roles.Contains("Uberadmin", StringComparer.OrdinalIgnoreCase) ||
                      roles.Contains("Admin",     StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            IsAdmin = false;
        }
    }

    private async Task ReloadAsync()
    {
        CurrentSkip = 0;

        await LoadOrphanAddonsAsync();
    }

    private async Task NextPageAsync()
    {
        if(!CanGoNext) return;

        CurrentSkip += PageSize;
        await LoadOrphanAddonsAsync();
    }

    private async Task PreviousPageAsync()
    {
        if(!CanGoPrevious) return;

        CurrentSkip = Math.Max(0, CurrentSkip - PageSize);
        await LoadOrphanAddonsAsync();
    }

    private async Task LoadOrphanAddonsAsync()
    {
        try
        {
            IsLoading    = true;
            HasError     = false;
            ErrorMessage = string.Empty;

            string? search     = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText;
            bool    onlyOrphans = OnlyOrphans;
            int     skip        = CurrentSkip;

            Task<int?> countTask = _apiClient.Software.Admin.Addons.Count.GetAsync(cfg =>
            {
                cfg.QueryParameters.Search      = search;
                cfg.QueryParameters.OnlyOrphans = onlyOrphans;
            });

            Task<List<SoftwareAddonDto>?> dataTask = _apiClient.Software.Admin.Addons.GetAsync(cfg =>
            {
                cfg.QueryParameters.Skip        = skip;
                cfg.QueryParameters.Take        = PageSize;
                cfg.QueryParameters.Search      = search;
                cfg.QueryParameters.OnlyOrphans = onlyOrphans;
            });

            await Task.WhenAll(countTask, dataTask);

            TotalAddons = countTask.Result ?? 0;

            OrphanAddons.Clear();

            if(dataTask.Result != null)
                foreach(SoftwareAddonDto addon in dataTask.Result)
                    OrphanAddons.Add(BuildRow(addon));

            OnPropertyChanged(nameof(CanGoPrevious));
            OnPropertyChanged(nameof(CanGoNext));
            OnPropertyChanged(nameof(PageInfoText));
            OnPropertyChanged(nameof(SelectedCount));
            OnPropertyChanged(nameof(HasSelection));
            OnPropertyChanged(nameof(BulkLinkButtonText));
            IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading software orphan add-ons");
            ErrorMessage = _localizer["SoftwareOrphanAddonsLoadFailed"];
            HasError     = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private OrphanAddonRow BuildRow(SoftwareAddonDto addon) =>
        new()
        {
            Item             = addon,
            KindLabel        = KindLabel(addon.Kind),
            ReasonLabel      = ReasonLabel(addon.OrphanReason),
            HasDlcGenreBadge = addon.HasDlcGenre == true && addon.Kind != 3
        };

    public void OnRowSelectionChanged()
    {
        OnPropertyChanged(nameof(SelectedCount));
        OnPropertyChanged(nameof(HasSelection));
        OnPropertyChanged(nameof(BulkLinkButtonText));
    }

    // Mirrors Marechai.Data.Enums.SoftwareKind's underlying int values (not referenceable from this project).
    private string KindLabel(int? kind) => kind switch
    {
        1  => _localizer["SoftwareKindOperatingSystem"],
        2  => _localizer["SoftwareKindGame"],
        3  => _localizer["SoftwareKindDlc"],
        4  => _localizer["SoftwareKindSystemSoftware"],
        5  => _localizer["SoftwareKindApplication"],
        6  => _localizer["SoftwareKindDevelopmentSoftware"],
        7  => _localizer["SoftwareKindServerSoftware"],
        8  => _localizer["SoftwareKindMiddleware"],
        9  => _localizer["SoftwareKindFirmware"],
        10 => _localizer["SoftwareKindEmbeddedSoftware"],
        _  => _localizer["SoftwareKindSoftware"]
    };

    // Mirrors Marechai.Data's AddonOrphanReason enum (not referenceable from this project).
    private string ReasonLabel(int? reason) => reason switch
    {
        0 => _localizer["SoftwareOrphanReasonLinked"],
        1 => _localizer["SoftwareOrphanReasonNoBase"],
        2 => _localizer["SoftwareOrphanReasonSelfReference"],
        3 => _localizer["SoftwareOrphanReasonDanglingFk"],
        4 => _localizer["SoftwareOrphanReasonChainedDlc"],
        5 => _localizer["SoftwareOrphanReasonNameMismatch"],
        6 => _localizer["SoftwareOrphanReasonMisclassifiedAsGame"],
        _ => _localizer["SoftwareOrphanReasonUnknown"]
    };

    private void OpenInNewWindow(OrphanAddonRow? row)
    {
        if(row?.Item.Id is not int softwareId) return;

        var parameters = new NavigationParameters
        {
            { NavParamKeys.SoftwareId, softwareId },
            { NavParamKeys.SoftwareName, row.Item.Name ?? string.Empty }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(SoftwareViewPage), parameters);
    }

    // --- Link panel ---
    private void OpenLinkDialog(OrphanAddonRow? row)
    {
        if(row is null) return;

        OpenLinkPanel([row]);
    }

    private void OpenBulkLinkDialog()
    {
        List<OrphanAddonRow> selected = OrphanAddons.Where(r => r.IsSelected).ToList();

        if(selected.Count == 0) return;

        OpenLinkPanel(selected);
    }

    private void OpenLinkPanel(List<OrphanAddonRow> rows)
    {
        LinkTargetRows       = rows;
        LinkSearchText       = string.Empty;
        LinkCandidates.Clear();
        SelectedLinkCandidate = null;
        HasError              = false;
        ErrorMessage          = string.Empty;
        SuccessMessage        = null;
        IsLinking             = true;
        OnPropertyChanged(nameof(LinkPanelTitle));
    }

    public async void UpdateLinkCandidates(string query)
    {
        try
        {
            List<SoftwareDto>? results = await _apiClient.Software.Admin.Addons.Candidates.GetAsync(cfg =>
            {
                cfg.QueryParameters.Search = string.IsNullOrWhiteSpace(query) ? null : query;
                cfg.QueryParameters.Take   = 50;
            });

            LinkCandidates.Clear();

            if(results != null)
                foreach(SoftwareDto candidate in results)
                    LinkCandidates.Add(candidate);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading base software link candidates");
        }
    }

    private async Task ConfirmLinkAsync()
    {
        if(SelectedLinkCandidate?.Id is not int baseSoftwareId || LinkTargetRows.Count == 0) return;

        try
        {
            if(LinkTargetRows.Count == 1)
            {
                int rowId = LinkTargetRows[0].Item.Id ?? 0;

                await _apiClient.Software[rowId].BaseSoftware.PatchAsync(new SetBaseSoftwareRequestDto
                {
                    BaseSoftwareId = baseSoftwareId
                });

                SuccessMessage = _localizer["SoftwareOrphanAddonsLinkSuccess"];
            }
            else
            {
                BulkSetBaseSoftwareResultDto? result = await _apiClient.Software.Admin.Addons.BaseSoftwareBulk.PatchAsync(
                    new BulkSetBaseSoftwareRequestDto
                    {
                        SoftwareIds    = LinkTargetRows.Select(r => r.Item.Id).ToList(),
                        BaseSoftwareId = baseSoftwareId
                    });

                int failedCount = result?.Failed?.Count ?? 0;

                SuccessMessage = failedCount == 0
                                     ? string.Format(_localizer["SoftwareOrphanAddonsBulkLinkSuccess"], result?.Updated ?? 0)
                                     : string.Format(_localizer["SoftwareOrphanAddonsBulkLinkPartial"], result?.Updated ?? 0,
                                                      failedCount);
            }

            CancelLink();
            await LoadOrphanAddonsAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error linking add-on(s) to base software {BaseSoftwareId}", baseSoftwareId);
            ErrorMessage = _localizer["SoftwareOrphanAddonsLinkFailed"];
            HasError     = true;
        }
    }

    private void CancelLink()
    {
        IsLinking             = false;
        LinkTargetRows        = [];
        LinkSearchText        = string.Empty;
        LinkCandidates.Clear();
        SelectedLinkCandidate = null;
    }
}

/// <summary>One Software add-on row, with UI-facing derived state and multiselect support.</summary>
public partial class OrphanAddonRow : ObservableObject
{
    public SoftwareAddonDto Item { get; set; } = null!;

    public string KindLabel        { get; set; } = string.Empty;
    public string ReasonLabel      { get; set; } = string.Empty;
    public bool   HasDlcGenreBadge { get; set; }

    [ObservableProperty]
    private bool _isSelected;
}
