#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Marechai.App.Presentation.Views;
using Marechai.App.Services;
using Marechai.App.Services.Authentication;

namespace Marechai.App.Presentation.ViewModels.Admin;

public partial class AdminSoftwareDuplicatesViewModel : ObservableObject, IRegionAware
{
    private readonly Client                                   _apiClient;
    private readonly IJwtService                              _jwtService;
    private readonly IStringLocalizer                         _localizer;
    private readonly ILogger<AdminSoftwareDuplicatesViewModel> _logger;
    private readonly IRegionManager                            _regionManager;
    private readonly SoftwareService                           _softwareService;
    private readonly ITokenService                             _tokenService;

    private readonly Dictionary<string, int> _masterByGroup = new(StringComparer.Ordinal);

    // --- List state ---
    [ObservableProperty]
    private ObservableCollection<DuplicateGroupItem> _duplicateGroups = [];

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isDataLoaded;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isAdmin;

    // --- Filters ---
    [ObservableProperty]
    private int _kindFilterIndex;

    [ObservableProperty]
    private bool _excludeDlc;

    [ObservableProperty]
    private List<string> _kindFilterItems = [];

    public bool IsExcludeDlcEnabled => KindFilterIndex == 0;

    partial void OnKindFilterIndexChanged(int value)
    {
        OnPropertyChanged(nameof(IsExcludeDlcEnabled));
        _ = ReloadAsync();
    }

    partial void OnExcludeDlcChanged(bool value) => _ = ReloadAsync();

    // --- Pagination (over groups) ---
    [ObservableProperty]
    private int _currentSkip;

    private const int PageSize = 25;

    [ObservableProperty]
    private int _totalGroups;

    public bool CanGoPrevious => CurrentSkip > 0;
    public bool CanGoNext     => CurrentSkip + PageSize < TotalGroups;

    public string PageInfoText =>
        string.Format(_localizer["SoftwareDuplicatesPageInfo"],
                      TotalGroups == 0 ? 0 : (CurrentSkip / PageSize) + 1,
                      TotalGroups == 0 ? 0 : (int)Math.Ceiling(TotalGroups / (double)PageSize));

    // --- Merge panel state ---
    [ObservableProperty]
    private bool _isMerging;

    [ObservableProperty]
    private SoftwareDuplicateItemDto? _mergeSourceItem;

    [ObservableProperty]
    private string _mergeTargetSearchText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<SoftwareDto> _mergeTargetSuggestions = [];

    [ObservableProperty]
    private SoftwareDto? _selectedMergeTargetSoftware;

    [ObservableProperty]
    private SoftwareMergePreviewDto? _mergePreview;

    [ObservableProperty]
    private string _releaseTitle = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> _mergeRelationshipSummaryLines = [];

    [ObservableProperty]
    private bool _isMergeTargetSelected;

    [ObservableProperty]
    private bool _isMergeTargetLocked;

    public string MergeSummaryText =>
        SelectedMergeTargetSoftware is { Name: { } targetName }
            ? string.Format(_localizer["MergeSummaryTextFormat"], MergeSourceItem?.Name, targetName)
            : MergeSourceItem?.Name ?? string.Empty;

    public AdminSoftwareDuplicatesViewModel(Client                                   apiClient,
                                            IJwtService                              jwtService,
                                            ITokenService                            tokenService,
                                            ILogger<AdminSoftwareDuplicatesViewModel> logger,
                                            IStringLocalizer                         localizer,
                                            IRegionManager                           regionManager,
                                            SoftwareService                          softwareService)
    {
        _apiClient       = apiClient;
        _jwtService      = jwtService;
        _tokenService    = tokenService;
        _logger          = logger;
        _localizer       = localizer;
        _regionManager   = regionManager;
        _softwareService = softwareService;

        KindFilterItems =
        [
            localizer["SoftwareKindFilterAll"],
            localizer["SoftwareKindSoftware"],
            localizer["SoftwareKindOperatingSystem"],
            localizer["SoftwareKindGame"],
            localizer["SoftwareKindDlc"],
            localizer["SoftwareKindSystemSoftware"],
            localizer["SoftwareKindApplication"],
            localizer["SoftwareKindDevelopmentSoftware"],
            localizer["SoftwareKindServerSoftware"],
            localizer["SoftwareKindMiddleware"],
            localizer["SoftwareKindFirmware"],
            localizer["SoftwareKindEmbeddedSoftware"]
        ];

        LoadDuplicateGroupsCommand = new AsyncRelayCommand(LoadDuplicateGroupsAsync);
        NextPageCommand             = new AsyncRelayCommand(NextPageAsync);
        PreviousPageCommand         = new AsyncRelayCommand(PreviousPageAsync);
        SetMasterCommand            = new RelayCommand<DuplicateItemRow>(SetMaster);
        OpenMergeIntoMasterCommand  = new AsyncRelayCommand<DuplicateItemRow>(OpenMergeIntoMasterAsync);
        OpenMergeCommand            = new RelayCommand<DuplicateItemRow>(OpenMerge);
        ConfirmMergeCommand         = new AsyncRelayCommand(ConfirmMergeAsync);
        CancelMergeCommand          = new RelayCommand(CancelMerge);
        OpenInNewWindowCommand      = new RelayCommand<DuplicateItemRow>(OpenInNewWindow);
        UnlockMergeTargetCommand    = new RelayCommand(UnlockMergeTarget);

        CheckAdminRole();
    }

    // --- Commands ---
    public IAsyncRelayCommand                    LoadDuplicateGroupsCommand { get; }
    public IAsyncRelayCommand                    NextPageCommand            { get; }
    public IAsyncRelayCommand                    PreviousPageCommand        { get; }
    public IRelayCommand<DuplicateItemRow>       SetMasterCommand           { get; }
    public IAsyncRelayCommand<DuplicateItemRow>  OpenMergeIntoMasterCommand { get; }
    public IRelayCommand<DuplicateItemRow>       OpenMergeCommand           { get; }
    public IAsyncRelayCommand                    ConfirmMergeCommand        { get; }
    public IRelayCommand                          CancelMergeCommand         { get; }
    public IRelayCommand<DuplicateItemRow>       OpenInNewWindowCommand     { get; }
    public IRelayCommand                          UnlockMergeTargetCommand   { get; }

    public string MergeConfirmDialogTitle   => _localizer["MergeConfirmDialogTitle"];
    public string MergeConfirmDialogMessage => _localizer["MergeConfirmDialogMessage"];
    public string MergeButtonText           => _localizer["MergeButton"];
    public string CancelButtonText          => _localizer["CancelButton"];

    // --- IRegionAware ---
    public bool IsNavigationTarget(NavigationContext navigationContext) => true;

    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        CheckAdminRole();

        if(IsAdmin) _ = LoadDuplicateGroupsCommand.ExecuteAsync(null);
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

            if(!_jwtService.IsTokenValid(token))
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

        await LoadDuplicateGroupsAsync();
    }

    private async Task NextPageAsync()
    {
        if(!CanGoNext) return;

        CurrentSkip += PageSize;
        await LoadDuplicateGroupsAsync();
    }

    private async Task PreviousPageAsync()
    {
        if(!CanGoPrevious) return;

        CurrentSkip = Math.Max(0, CurrentSkip - PageSize);
        await LoadDuplicateGroupsAsync();
    }

    private int? CurrentKind => KindFilterIndex == 0 ? null : KindFilterIndex - 1;

    private async Task LoadDuplicateGroupsAsync()
    {
        try
        {
            IsLoading    = true;
            HasError     = false;
            ErrorMessage = string.Empty;

            int?   kind       = CurrentKind;
            bool   excludeDlc = ExcludeDlc;
            int    skip       = CurrentSkip;

            Task<int?> countTask = _apiClient.Software.Admin.Duplicates.Count.GetAsync(cfg =>
            {
                cfg.QueryParameters.Kind       = kind;
                cfg.QueryParameters.ExcludeDlc = kind == null ? excludeDlc : null;
            });

            Task<List<SoftwareDuplicateGroupDto>?> dataTask = _apiClient.Software.Admin.Duplicates.GetAsync(cfg =>
            {
                cfg.QueryParameters.Skip       = skip;
                cfg.QueryParameters.Take       = PageSize;
                cfg.QueryParameters.Kind       = kind;
                cfg.QueryParameters.ExcludeDlc = kind == null ? excludeDlc : null;
            });

            await Task.WhenAll(countTask, dataTask);

            TotalGroups = countTask.Result ?? 0;

            DuplicateGroups.Clear();

            if(dataTask.Result != null)
                foreach(SoftwareDuplicateGroupDto group in dataTask.Result)
                    DuplicateGroups.Add(BuildGroupItem(group));

            OnPropertyChanged(nameof(CanGoPrevious));
            OnPropertyChanged(nameof(CanGoNext));
            OnPropertyChanged(nameof(PageInfoText));
            IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading software duplicate groups");
            ErrorMessage = _localizer["FailedToLoadDuplicateGroups"];
            HasError     = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private DuplicateGroupItem BuildGroupItem(SoftwareDuplicateGroupDto group)
    {
        var item = new DuplicateGroupItem
        {
            Group           = group,
            EntriesCountText = (group.Items?.Count ?? 0).ToString()
        };

        if(group.Items != null)
            foreach(SoftwareDuplicateItemDto duplicateItem in group.Items)
                item.Add(BuildRow(group, duplicateItem));

        return item;
    }

    private DuplicateItemRow BuildRow(SoftwareDuplicateGroupDto group, SoftwareDuplicateItemDto duplicateItem)
    {
        int? master = GetMaster(group);
        bool isMaster = master.HasValue && duplicateItem.Id == master.Value;

        return new DuplicateItemRow
        {
            Group           = group,
            Item            = duplicateItem,
            IsMaster        = isMaster,
            CanMergeIntoMaster = master.HasValue && !isMaster,
            KindLabel       = KindLabel(duplicateItem.Kind),
            EarliestReleaseYearText = duplicateItem.EarliestReleaseYear?.ToString() ?? "—",
            PlatformsText   = string.IsNullOrWhiteSpace(duplicateItem.Platforms) ? "—" : duplicateItem.Platforms
        };
    }

    private int? GetMaster(SoftwareDuplicateGroupDto group)
    {
        if(group.NormalizedName is null) return null;

        return _masterByGroup.TryGetValue(group.NormalizedName, out int id) ? id : null;
    }

    private void SetMaster(DuplicateItemRow? row)
    {
        if(row?.Group.NormalizedName is null || row.Item.Id is not int itemId) return;

        _masterByGroup[row.Group.NormalizedName] = itemId;

        // Refresh master/merge-eligibility flags for every row in this group
        DuplicateGroupItem? groupItem = DuplicateGroups.FirstOrDefault(g => g.Group.NormalizedName == row.Group.NormalizedName);

        if(groupItem is null) return;

        foreach(DuplicateItemRow groupRow in groupItem)
        {
            groupRow.IsMaster           = groupRow.Item.Id == itemId;
            groupRow.CanMergeIntoMaster = groupRow.Item.Id != itemId;
        }
    }

    // Mirrors Marechai.Data.Enums.SoftwareKind's underlying int values (not referenceable from this project).
    private string KindLabel(int? kind) => kind switch
    {
        1 => _localizer["SoftwareKindOperatingSystem"],
        2 => _localizer["SoftwareKindGame"],
        3 => _localizer["SoftwareKindDlc"],
        4 => _localizer["SoftwareKindSystemSoftware"],
        5 => _localizer["SoftwareKindApplication"],
        6 => _localizer["SoftwareKindDevelopmentSoftware"],
        7 => _localizer["SoftwareKindServerSoftware"],
        8 => _localizer["SoftwareKindMiddleware"],
        9 => _localizer["SoftwareKindFirmware"],
        10 => _localizer["SoftwareKindEmbeddedSoftware"],
        _ => _localizer["SoftwareKindSoftware"]
    };

    private void OpenInNewWindow(DuplicateItemRow? row)
    {
        if(row?.Item.Id is not int softwareId) return;

        var parameters = new NavigationParameters
        {
            { NavParamKeys.SoftwareId, softwareId },
            { NavParamKeys.SoftwareName, row.Item.Name ?? string.Empty }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(SoftwareViewPage), parameters);
    }

    // --- Merge ---
    private void OpenMerge(DuplicateItemRow? row)
    {
        if(row?.Item.Id is not int sourceId) return;

        MergeSourceItem        = row.Item;
        MergeTargetSearchText  = string.Empty;
        MergeTargetSuggestions.Clear();
        SelectedMergeTargetSoftware = null;
        MergePreview                = null;
        ReleaseTitle                = string.Empty;
        MergeRelationshipSummaryLines.Clear();
        IsMergeTargetSelected = false;
        IsMergeTargetLocked    = false;
        HasError               = false;
        ErrorMessage           = string.Empty;
        IsMerging              = true;
    }

    private async Task OpenMergeIntoMasterAsync(DuplicateItemRow? row)
    {
        if(row?.Item.Id is not int sourceId) return;
        if(GetMaster(row.Group) is not { } masterId || masterId == sourceId) return;

        SoftwareDuplicateItemDto? master = row.Group.Items?.FirstOrDefault(i => i.Id == masterId);

        if(master is null) return;

        OpenMerge(row);

        IsMergeTargetLocked          = true;
        SelectedMergeTargetSoftware = new SoftwareDto { Id = masterId, Name = master.Name };
        MergeTargetSearchText       = master.Name ?? string.Empty;

        await LoadMergePreviewAsync(sourceId, masterId);
    }

    public async Task UpdateMergeTargetSuggestions(string query)
    {
        MergeTargetSuggestions.Clear();

        if(MergeSourceItem?.Id is not int sourceId) return;

        List<SoftwareDto> results = await _softwareService.SearchForPickerAsync(query);

        foreach(SoftwareDto match in results.Where(s => s.Id != sourceId))
            MergeTargetSuggestions.Add(match);
    }

    partial void OnSelectedMergeTargetSoftwareChanged(SoftwareDto? value)
    {
        MergePreview = null;
        ReleaseTitle = string.Empty;
        MergeRelationshipSummaryLines.Clear();
        IsMergeTargetSelected = false;
        OnPropertyChanged(nameof(MergeSummaryText));

        if(value?.Id is not int targetId || MergeSourceItem?.Id is not int sourceId || targetId == sourceId) return;

        _ = LoadMergePreviewAsync(sourceId, targetId);
    }

    partial void OnMergeSourceItemChanged(SoftwareDuplicateItemDto? value) => OnPropertyChanged(nameof(MergeSummaryText));

    private void UnlockMergeTarget()
    {
        IsMergeTargetLocked = false;
    }

    private async Task LoadMergePreviewAsync(int sourceId, int targetId)
    {
        try
        {
            MergePreview = await _apiClient.Software[targetId].MergePreview[sourceId].GetAsync();

            if(MergePreview != null)
            {
                ReleaseTitle = MergePreview.SuggestedReleaseTitle ?? string.Empty;
                BuildMergeRelationshipSummary(MergePreview);
            }

            IsMergeTargetSelected = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading merge preview for {SourceId} -> {TargetId}", sourceId, targetId);
            ErrorMessage = _localizer["FailedToLoadMergePreview"];
            HasError     = true;
        }
    }

    private void BuildMergeRelationshipSummary(SoftwareMergePreviewDto preview)
    {
        MergeRelationshipSummaryLines.Clear();

        void AddLine(string labelKey, int? count)
        {
            if(count is > 0) MergeRelationshipSummaryLines.Add($"{_localizer[labelKey]}: {count}");
        }

        void AddLineWithDuplicates(string labelKey, int? total, int? duplicates)
        {
            if(total is > 0)
                MergeRelationshipSummaryLines.Add(duplicates is > 0
                                                       ? $"{_localizer[labelKey]}: {total} ({duplicates} {_localizer["MergeDuplicatesSuffix"]})"
                                                       : $"{_localizer[labelKey]}: {total}");
        }

        AddLine("MergeSoftwareVersionsCountLabel", preview.VersionsCount);
        AddLine("MergeSoftwareDirectReleasesLabel", preview.DirectReleasesCount);
        AddLine("MergeSoftwareScreenshotsLabel", preview.ScreenshotsCount);
        AddLine("MergeSoftwarePromoArtLabel", preview.PromoArtCount);
        AddLineWithDuplicates("MergeSoftwareCompanyRolesLabel", preview.CompanyRolesTotal, preview.CompanyRolesDuplicates);
        AddLineWithDuplicates("MergeDescriptionsLabel", preview.DescriptionsTotal, preview.DescriptionsDuplicates);
        AddLineWithDuplicates("MergeSoftwareGenresLabel", preview.GenresTotal, preview.GenresDuplicates);
        AddLineWithDuplicates("MergeSoftwareCreditsLabel", preview.CreditsTotal, preview.CreditsDuplicates);
        AddLineWithDuplicates("MergeSoftwareCompilationsLabel", preview.CompilationReferencesTotal,
                              preview.CompilationReferencesDuplicates);
        AddLineWithDuplicates("MergeSoftwareVideosLabel", preview.VideosTotal, preview.VideosDuplicates);

        if(MergeRelationshipSummaryLines.Count == 0)
            MergeRelationshipSummaryLines.Add(_localizer["MergeNoRelationshipsLabel"]);
    }

    private async Task ConfirmMergeAsync()
    {
        if(MergeSourceItem?.Id is not int sourceId || SelectedMergeTargetSoftware?.Id is not int targetId ||
           sourceId == targetId)
            return;

        try
        {
            string? releaseTitle = string.IsNullOrWhiteSpace(ReleaseTitle) ? null : ReleaseTitle;

            await _apiClient.Software[targetId].Merge[sourceId].PostAsync(cfg =>
                cfg.QueryParameters.ReleaseTitle = releaseTitle);

            CancelMerge();
            await LoadDuplicateGroupsAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error merging software {SourceId} into {TargetId}", sourceId, targetId);
            ErrorMessage = _localizer["FailedToMergeSoftware"];
            HasError     = true;
        }
    }

    private void CancelMerge()
    {
        IsMerging                   = false;
        MergeSourceItem              = null;
        MergeTargetSearchText        = string.Empty;
        MergeTargetSuggestions.Clear();
        SelectedMergeTargetSoftware = null;
        MergePreview                 = null;
        ReleaseTitle                 = string.Empty;
        MergeRelationshipSummaryLines.Clear();
        IsMergeTargetSelected       = false;
        IsMergeTargetLocked          = false;
        HasError                    = false;
        ErrorMessage                 = string.Empty;
    }
}

/// <summary>
/// One duplicate group: deriving from <see cref="ObservableCollection{T}"/> lets the group card's
/// nested rows repeater bind directly to the group instance (`ItemsSource="{Binding}"`) while its own
/// properties back the card header.
/// </summary>
public partial class DuplicateGroupItem : ObservableCollection<DuplicateItemRow>
{
    public SoftwareDuplicateGroupDto Group { get; set; } = null!;

    public string NormalizedName => Group.NormalizedName ?? string.Empty;

    public string EntriesCountText { get; set; } = "0";
}

/// <summary>One Software row within a duplicate group, with UI-facing derived state.</summary>
public partial class DuplicateItemRow : ObservableObject
{
    public SoftwareDuplicateGroupDto Group { get; set; } = null!;
    public SoftwareDuplicateItemDto  Item  { get; set; } = null!;

    public string KindLabel { get; set; } = string.Empty;
    public string EarliestReleaseYearText { get; set; } = "—";
    public string PlatformsText { get; set; } = "—";

    [ObservableProperty]
    private bool _isMaster;

    [ObservableProperty]
    private bool _canMergeIntoMaster;
}
