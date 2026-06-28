#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Views.Admin;
using Marechai.App.Services;
using Marechai.App.Services.Authentication;
using Marechai.Data;

namespace Marechai.App.Presentation.ViewModels.Admin;

public partial class AdminOldDosImportsViewModel : ObservableObject, IRegionAware
{
    private readonly OldDosImportsService _service;
    private readonly IJwtService _jwtService;
    private readonly ITokenService _tokenService;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<AdminOldDosImportsViewModel> _logger;
    private readonly IRegionManager _regionManager;

    [ObservableProperty] private ObservableCollection<OldDosQueueItemViewModel> _imports = [];
    [ObservableProperty] private ObservableCollection<OldDosQueueItemViewModel> _filteredImports = [];
    [ObservableProperty] private ObservableCollection<OldDosEnumOption> _statusOptions = [];
    [ObservableProperty] private OldDosQueueItemViewModel? _selectedImport;
    [ObservableProperty] private OldDosEnumOption? _selectedStatusOption;
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private bool _hasEnrichmentError;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isDataLoaded;
    [ObservableProperty] private bool _hasError;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool _isAdmin;
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private bool _statusIsSuccess;

    public AdminOldDosImportsViewModel(OldDosImportsService service,
                                       IJwtService jwtService,
                                       ITokenService tokenService,
                                       IStringLocalizer localizer,
                                       ILogger<AdminOldDosImportsViewModel> logger,
                                       IRegionManager regionManager)
    {
        _service       = service;
        _jwtService    = jwtService;
        _tokenService  = tokenService;
        _localizer     = localizer;
        _logger        = logger;
        _regionManager = regionManager;

        LoadCommand = new AsyncRelayCommand(LoadAsync);
        ReviewCommand = new RelayCommand<OldDosQueueItemViewModel>(OpenReview);
        ClearStatusMessageCommand = new RelayCommand(() => StatusMessage = string.Empty);

        BuildOptions();
        CheckAdminRole();
    }

    public IAsyncRelayCommand LoadCommand { get; }
    public IRelayCommand<OldDosQueueItemViewModel> ReviewCommand { get; }
    public IRelayCommand ClearStatusMessageCommand { get; }

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

        IEnumerable<OldDosQueueItemViewModel> source = Imports;

        if(!string.IsNullOrWhiteSpace(SearchText))
        {
            source = source.Where(item =>
                                      item.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                                      item.DeveloperName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                                      item.RussianCategoryPath.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        }

        foreach(OldDosQueueItemViewModel item in source)
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

            OldDosSoftwareStatus? status = GetSelectedStatus();
            string? search = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText;

            int count = await _service.GetPendingCountAsync(status, search, HasEnrichmentError);
            int take = Math.Clamp(count, 1, 5000);

            List<OldDosPendingListItemDto> rows = await _service.GetPendingAsync(0, take, status, search, HasEnrichmentError);

            Imports.Clear();

            foreach(OldDosPendingListItemDto row in rows)
            {
                var rowStatus = (OldDosSoftwareStatus)(row.Status ?? 0);
                Imports.Add(new OldDosQueueItemViewModel
                {
                    Item = row,
                    StatusLabel = StatusLabel(rowStatus)
                });
            }

            ApplyFilters();
            IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading old-dos imports");
            HasError     = true;
            ErrorMessage = _localizer["FailedToLoadOldDosImports"];
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task<bool> RejectAsync(OldDosQueueItemViewModel? item)
    {
        if(item is null) return false;

        bool ok = await _service.DiscardAsync(item.Id);

        if(ok)
        {
            SetOperationMessage(true, string.Format(_localizer["RejectedOldDosImportMessage"].Value, item.Name));
            await LoadAsync();
        }
        else
        {
            SetOperationMessage(false, _localizer["FailedToRejectOldDosImport"]);
        }

        return ok;
    }

    public void OpenReview(OldDosQueueItemViewModel? item)
    {
        if(item?.Id <= 0) return;

        var parameters = new NavigationParameters
        {
            { NavParamKeys.OldDosImportId, item.Id }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(AdminOldDosImportReviewPage), parameters);
    }

    public string StatusLabel(OldDosSoftwareStatus status) => status switch
    {
        OldDosSoftwareStatus.ReadyForReview => _localizer["OldDosStatusReadyForReview"],
        OldDosSoftwareStatus.Skipped        => _localizer["OldDosStatusSkipped"],
        OldDosSoftwareStatus.Accepted       => _localizer["OldDosStatusAccepted"],
        OldDosSoftwareStatus.Discarded      => _localizer["OldDosStatusDiscarded"],
        OldDosSoftwareStatus.Crawled        => _localizer["OldDosStatusCrawled"],
        OldDosSoftwareStatus.Translated     => _localizer["OldDosStatusTranslated"],
        OldDosSoftwareStatus.Described      => _localizer["OldDosStatusDescribed"],
        OldDosSoftwareStatus.Categorized    => _localizer["OldDosStatusCategorized"],
        _                                   => status.ToString()
    };

    public string AbsoluteOldDosUrl(string? url)
    {
        if(string.IsNullOrWhiteSpace(url)) return string.Empty;
        if(url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
           url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return url;

        return "https://old-dos.ru" + (url.StartsWith('/') ? url : "/" + url);
    }

    partial void OnSearchTextChanged(string value) => _ = LoadCommand.ExecuteAsync(null);
    partial void OnSelectedStatusOptionChanged(OldDosEnumOption? value) => _ = LoadCommand.ExecuteAsync(null);
    partial void OnHasEnrichmentErrorChanged(bool value) => _ = LoadCommand.ExecuteAsync(null);

    private void BuildOptions()
    {
        StatusOptions =
        [
            new OldDosEnumOption { Label = _localizer["OldDosStatusReadyOrSkipped"], Value = null },
            new OldDosEnumOption { Label = _localizer["OldDosStatusReadyForReview"], Value = (int)OldDosSoftwareStatus.ReadyForReview },
            new OldDosEnumOption { Label = _localizer["OldDosStatusSkipped"], Value = (int)OldDosSoftwareStatus.Skipped },
            new OldDosEnumOption { Label = _localizer["OldDosStatusAccepted"], Value = (int)OldDosSoftwareStatus.Accepted },
            new OldDosEnumOption { Label = _localizer["OldDosStatusDiscarded"], Value = (int)OldDosSoftwareStatus.Discarded },
            new OldDosEnumOption { Label = _localizer["OldDosStatusCrawled"], Value = (int)OldDosSoftwareStatus.Crawled },
            new OldDosEnumOption { Label = _localizer["OldDosStatusTranslated"], Value = (int)OldDosSoftwareStatus.Translated },
            new OldDosEnumOption { Label = _localizer["OldDosStatusDescribed"], Value = (int)OldDosSoftwareStatus.Described },
            new OldDosEnumOption { Label = _localizer["OldDosStatusCategorized"], Value = (int)OldDosSoftwareStatus.Categorized }
        ];

        SelectedStatusOption = StatusOptions.FirstOrDefault(option => option.Value == (int)OldDosSoftwareStatus.ReadyForReview);
    }

    private OldDosSoftwareStatus? GetSelectedStatus() =>
        SelectedStatusOption?.Value.HasValue == true
            ? (OldDosSoftwareStatus)SelectedStatusOption.Value.Value
            : HasEnrichmentError ? null : OldDosSoftwareStatus.ReadyForReview;

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
}
