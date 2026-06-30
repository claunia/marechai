#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Views.Admin;
using Marechai.App.Services;
using Marechai.App.Services.Authentication;
using Marechai.Data;

namespace Marechai.App.Presentation.ViewModels.Admin;

public partial class AdminWwpcImportsViewModel : ObservableObject, IRegionAware
{
    private readonly WwpcImportsService                    _service;
    private readonly IJwtService                           _jwtService;
    private readonly ITokenService                         _tokenService;
    private readonly IStringLocalizer                      _localizer;
    private readonly ILogger<AdminWwpcImportsViewModel>    _logger;
    private readonly IRegionManager                        _regionManager;

    [ObservableProperty] private ObservableCollection<WwpcQueueItemViewModel> _imports = [];
    [ObservableProperty] private ObservableCollection<WwpcQueueItemViewModel> _filteredImports = [];
    [ObservableProperty] private ObservableCollection<int> _pageSizeOptions = [10, 25, 50, 100];
    [ObservableProperty] private ObservableCollection<WwpcEnumOption> _statusOptions = [];
    [ObservableProperty] private ObservableCollection<WwpcEnumOption> _productTypeOptions = [];
    [ObservableProperty] private WwpcQueueItemViewModel? _selectedImport;
    [ObservableProperty] private WwpcEnumOption? _selectedStatusOption;
    [ObservableProperty] private WwpcEnumOption? _selectedProductTypeOption;
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private bool _hasEnrichmentError;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isDataLoaded;
    [ObservableProperty] private bool _hasError;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool _isAdmin;
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private bool _statusIsSuccess;
    [ObservableProperty] private int _currentPage = 1;
    [ObservableProperty] private int _pageSize = 25;
    [ObservableProperty] private int _totalCount;

    public AdminWwpcImportsViewModel(WwpcImportsService service,
                                     IJwtService jwtService,
                                     ITokenService tokenService,
                                     IStringLocalizer localizer,
                                     ILogger<AdminWwpcImportsViewModel> logger,
                                     IRegionManager regionManager)
    {
        _service       = service;
        _jwtService    = jwtService;
        _tokenService  = tokenService;
        _localizer     = localizer;
        _logger        = logger;
        _regionManager = regionManager;

        LoadCommand = new AsyncRelayCommand(LoadAsync);
        NextPageCommand = new AsyncRelayCommand(NextPageAsync);
        PreviousPageCommand = new AsyncRelayCommand(PreviousPageAsync);
        ReviewCommand = new RelayCommand<WwpcQueueItemViewModel>(OpenReview);
        ClearStatusMessageCommand = new RelayCommand(() => StatusMessage = string.Empty);

        BuildOptions();
        CheckAdminRole();
    }

    public IAsyncRelayCommand LoadCommand { get; }
    public IAsyncRelayCommand NextPageCommand { get; }
    public IAsyncRelayCommand PreviousPageCommand { get; }
    public IRelayCommand<WwpcQueueItemViewModel> ReviewCommand { get; }
    public IRelayCommand ClearStatusMessageCommand { get; }

    public bool CanGoPrevious => CurrentPage > 1;
    public bool CanGoNext => CurrentPage * PageSize < TotalCount;
    public string PageSummary => TotalCount == 0
                                     ? _localizer["SoftwareAttributesPaginationEmpty"]
                                     : string.Format(_localizer["MessageReportsPaginationFormat"],
                                                     (CurrentPage - 1) * PageSize + 1,
                                                     Math.Min(CurrentPage * PageSize, TotalCount),
                                                     TotalCount);

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;
    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        CheckAdminRole();

        if(IsAdmin)
            _ = LoadCommand.ExecuteAsync(null);
    }

    public void ApplyFilters()
    {
        FilteredImports.Clear();

        foreach(WwpcQueueItemViewModel item in Imports)
            FilteredImports.Add(item);
    }

    public async Task LoadAsync()
    {
        if(!IsAdmin) return;

        try
        {
            IsLoading    = true;
            HasError     = false;
            ErrorMessage = string.Empty;

            WwpcSoftwareStatus? status = GetSelectedStatus();
            WwpcProductType? productType = GetSelectedProductType();
            string? search = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText;

            TotalCount = await _service.GetPendingCountAsync(status, productType, search, HasEnrichmentError);

            int maxPage = Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));

            if(CurrentPage > maxPage)
                CurrentPage = maxPage;

            int skip = (CurrentPage - 1) * PageSize;
            List<WwpcPendingListItemDto> rows = await _service.GetPendingAsync(skip, PageSize, status, productType,
                                                                               search, HasEnrichmentError);

            Imports.Clear();

            foreach(WwpcPendingListItemDto row in rows)
                Imports.Add(new WwpcQueueItemViewModel { Item = row });

            ApplyFilters();
            IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading wwpc imports");
            HasError     = true;
            ErrorMessage = _localizer["FailedToLoadWwpcImports"];
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task<bool> RejectAsync(WwpcQueueItemViewModel? item)
    {
        if(item is null) return false;

        bool ok = await _service.DiscardAsync(item.Id);

        if(ok)
        {
            SetOperationMessage(true, string.Format(_localizer["RejectedWwpcImportMessage"].Value, item.Name));
            await LoadAsync();
        }
        else
        {
            SetOperationMessage(false, _localizer["FailedToRejectWwpcImport"]);
        }

        return ok;
    }

    public async Task<(bool Success, string Message)> DuplicateAsync(WwpcQueueItemViewModel? item, string? newName)
    {
        if(item is null || string.IsNullOrWhiteSpace(newName))
            return (false, _localizer["WwpcDuplicateNameRequired"]);

        (long? newId, string? error) = await _service.DuplicateAsync(item.Id, newName.Trim());

        if(newId.HasValue)
        {
            string message = string.Format(_localizer["DuplicatedWwpcImportMessage"].Value, item.Name, newId.Value);
            SetOperationMessage(true, message);
            await LoadAsync();

            return (true, message);
        }

        string failure = error ?? _localizer["FailedToDuplicateWwpcImport"];
        SetOperationMessage(false, failure);

        return (false, failure);
    }

    public void OpenReview(WwpcQueueItemViewModel? item)
    {
        if(item?.Id <= 0) return;

        var parameters = new NavigationParameters
        {
            { NavParamKeys.WwpcImportId, item.Id }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(AdminWwpcImportReviewPage), parameters);
    }

    public string ProductTypeLabel(WwpcProductType type) => type switch
    {
        WwpcProductType.Application => _localizer["WwpcProductTypeApplication"],
        WwpcProductType.DevTool     => _localizer["WwpcProductTypeDevTool"],
        WwpcProductType.System      => _localizer["WwpcProductTypeSystem"],
        _                           => type.ToString()
    };

    public string StatusLabel(WwpcSoftwareStatus status) => status switch
    {
        WwpcSoftwareStatus.ReadyForReview => _localizer["WwpcStatusReadyForReview"],
        WwpcSoftwareStatus.Skipped        => _localizer["WwpcStatusSkipped"],
        WwpcSoftwareStatus.Accepted       => _localizer["WwpcStatusAccepted"],
        WwpcSoftwareStatus.Discarded      => _localizer["WwpcStatusDiscarded"],
        WwpcSoftwareStatus.Crawled        => _localizer["WwpcStatusCrawled"],
        WwpcSoftwareStatus.Described      => _localizer["WwpcStatusDescribed"],
        WwpcSoftwareStatus.Categorized    => _localizer["WwpcStatusCategorized"],
        _                                 => status.ToString()
    };

    public string AbsoluteWwpcUrl(string? url)
    {
        if(string.IsNullOrWhiteSpace(url)) return string.Empty;
        if(url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
           url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return url;

        return "https://winworldpc.com" + (url.StartsWith('/') ? url : "/" + url);
    }

    partial void OnSearchTextChanged(string value) => _ = ReloadFromFirstPageAsync();
    partial void OnSelectedStatusOptionChanged(WwpcEnumOption? value) => _ = ReloadFromFirstPageAsync();
    partial void OnSelectedProductTypeOptionChanged(WwpcEnumOption? value) => _ = ReloadFromFirstPageAsync();
    partial void OnHasEnrichmentErrorChanged(bool value) => _ = ReloadFromFirstPageAsync();
    partial void OnCurrentPageChanged(int value) => NotifyPageStateChanged();
    partial void OnPageSizeChanged(int value) => NotifyPageStateChanged();
    partial void OnTotalCountChanged(int value) => NotifyPageStateChanged();

    private void BuildOptions()
    {
        StatusOptions =
        [
            new WwpcEnumOption { Label = _localizer["WwpcStatusReadyOrSkipped"], Value = null },
            new WwpcEnumOption { Label = _localizer["WwpcStatusReadyForReview"], Value = (int)WwpcSoftwareStatus.ReadyForReview },
            new WwpcEnumOption { Label = _localizer["WwpcStatusSkipped"], Value = (int)WwpcSoftwareStatus.Skipped },
            new WwpcEnumOption { Label = _localizer["WwpcStatusAccepted"], Value = (int)WwpcSoftwareStatus.Accepted },
            new WwpcEnumOption { Label = _localizer["WwpcStatusDiscarded"], Value = (int)WwpcSoftwareStatus.Discarded },
            new WwpcEnumOption { Label = _localizer["WwpcStatusCrawled"], Value = (int)WwpcSoftwareStatus.Crawled },
            new WwpcEnumOption { Label = _localizer["WwpcStatusDescribed"], Value = (int)WwpcSoftwareStatus.Described },
            new WwpcEnumOption { Label = _localizer["WwpcStatusCategorized"], Value = (int)WwpcSoftwareStatus.Categorized }
        ];

        ProductTypeOptions =
        [
            new WwpcEnumOption { Label = _localizer["All"], Value = null },
            new WwpcEnumOption { Label = _localizer["WwpcProductTypeApplication"], Value = (int)WwpcProductType.Application },
            new WwpcEnumOption { Label = _localizer["WwpcProductTypeDevTool"], Value = (int)WwpcProductType.DevTool },
            new WwpcEnumOption { Label = _localizer["WwpcProductTypeSystem"], Value = (int)WwpcProductType.System }
        ];

        SelectedStatusOption      = StatusOptions.FirstOrDefault(option => option.Value == (int)WwpcSoftwareStatus.ReadyForReview);
        SelectedProductTypeOption = ProductTypeOptions.FirstOrDefault();
    }

    private WwpcSoftwareStatus? GetSelectedStatus() =>
        SelectedStatusOption?.Value.HasValue == true
            ? (WwpcSoftwareStatus)SelectedStatusOption.Value.Value
            : HasEnrichmentError ? null : WwpcSoftwareStatus.ReadyForReview;

    private WwpcProductType? GetSelectedProductType() =>
        SelectedProductTypeOption?.Value.HasValue == true
            ? (WwpcProductType)SelectedProductTypeOption.Value.Value
            : null;

    private async Task NextPageAsync()
    {
        if(!CanGoNext) return;

        CurrentPage++;
        await LoadAsync();
    }

    private async Task PreviousPageAsync()
    {
        if(!CanGoPrevious) return;

        CurrentPage--;
        await LoadAsync();
    }

    private async Task ReloadFromFirstPageAsync()
    {
        CurrentPage = 1;
        await LoadAsync();
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
            IsAdmin = roles.Contains("UberAdmin", StringComparer.OrdinalIgnoreCase) ||
                      roles.Contains("Admin", StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            IsAdmin = false;
        }
    }

    private void SetOperationMessage(bool success, string message)
    {
        StatusIsSuccess = success;
        StatusMessage   = message;
    }

    private void NotifyPageStateChanged()
    {
        OnPropertyChanged(nameof(CanGoPrevious));
        OnPropertyChanged(nameof(CanGoNext));
        OnPropertyChanged(nameof(PageSummary));
    }
}
