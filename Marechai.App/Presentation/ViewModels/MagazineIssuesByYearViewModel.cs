#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Models;
using Marechai.App.Presentation.Views;
using Marechai.App.Services;
using Marechai.App.Services.Caching;
using Microsoft.UI.Xaml.Data;

namespace Marechai.App.Presentation.ViewModels;

[Bindable]
public partial class MagazineIssuesByYearViewModel : ObservableObject, IRegionAware
{
    private readonly MagazinesService                    _magazinesService;
    private readonly MagazineIssueCoverCache              _coverCache;
    private readonly ImageSourceFactory                   _imageSourceFactory;
    private readonly IStringLocalizer                     _localizer;
    private readonly ILogger<MagazineIssuesByYearViewModel> _logger;
    private readonly IRegionManager                       _regionManager;

    private string? _navigationSource;
    private long    _magazineId;
    private int?    _year;

    [ObservableProperty]
    private ObservableCollection<MagazineIssueListItem> _issuesList = [];

    [ObservableProperty]
    private string _pageTitle = string.Empty;

    [ObservableProperty]
    private string _filterDescription = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private bool _isDataLoaded;

    [ObservableProperty]
    private bool _isLoading;

    public MagazineIssuesByYearViewModel(ILogger<MagazineIssuesByYearViewModel> logger,
                                          IRegionManager                        regionManager,
                                          MagazinesService                      magazinesService,
                                          MagazineIssueCoverCache               coverCache,
                                          ImageSourceFactory                    imageSourceFactory,
                                          IStringLocalizer                      localizer)
    {
        _logger             = logger;
        _regionManager      = regionManager;
        _magazinesService   = magazinesService;
        _coverCache         = coverCache;
        _imageSourceFactory = imageSourceFactory;
        _localizer          = localizer;
    }

    public bool IsNavigationTarget(NavigationContext navigationContext) => false;

    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        if(navigationContext.Parameters.TryGetValue<string>(NavParamKeys.NavigationSource, out string? source))
            _navigationSource = source;

        if(navigationContext.Parameters.TryGetValue<long>(NavParamKeys.MagazineId, out long magazineId))
            _magazineId = magazineId;

        if(navigationContext.Parameters.TryGetValue<string>(NavParamKeys.Year, out string? yearParam) &&
           int.TryParse(yearParam, out int parsedYear))
            _year = parsedYear;
        else
            _year = null;

        _ = LoadIssuesAsync();
    }

    [RelayCommand]
    public Task GoBack()
    {
        if(_regionManager.TryGoBack(RegionNames.Content))
            return Task.CompletedTask;

        var parameters = new NavigationParameters
        {
            { NavParamKeys.MagazineId, _magazineId },
            { NavParamKeys.NavigationSource, nameof(MagazineIssuesByYearViewModel) }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(MagazineViewPage), parameters);

        return Task.CompletedTask;
    }

    [RelayCommand]
    public Task LoadData() => LoadIssuesAsync();

    [RelayCommand]
    public Task NavigateToIssue(MagazineIssueListItem? issue)
    {
        if(issue is null) return Task.CompletedTask;

        var parameters = new NavigationParameters
        {
            { NavParamKeys.MagazineIssueId, issue.Id },
            { NavParamKeys.NavigationSource, nameof(MagazineIssuesByYearViewModel) }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(MagazineIssueViewPage), parameters);

        return Task.CompletedTask;
    }

    private async Task LoadIssuesAsync()
    {
        try
        {
            IsLoading    = true;
            IsDataLoaded = false;
            HasError     = false;
            ErrorMessage = string.Empty;
            IssuesList.Clear();

            MagazineDto? magazine = await _magazinesService.GetMagazineAsync(_magazineId);
            string       magazineTitle = magazine?.Title ?? string.Empty;
            string       yearLabel     = _year.HasValue ? _year.Value.ToString() : _localizer["Others"];

            PageTitle          = $"{magazineTitle} - {yearLabel}";
            FilterDescription = _year.HasValue
                                     ? string.Format(_localizer["Showing issues published in {0}"], _year.Value)
                                     : _localizer["Showing issues with no recorded publication year"];

            List<MagazineIssueDto> issues = _year.HasValue
                                                 ? await _magazinesService.GetIssuesByYearAsync(_magazineId, _year.Value)
                                                 : await _magazinesService.GetIssuesNoYearAsync(_magazineId);

            foreach(MagazineIssueDto issue in issues)
            {
                var item = new MagazineIssueListItem
                {
                    Id               = issue.Id ?? 0,
                    Caption          = issue.Caption ?? string.Empty,
                    IssueNumber      = issue.IssueNumber,
                    PublishedDisplay = DatePrecisionFormatter.Format(issue.Published, issue.PublishedPrecision, null),
                    CoverGuid        = issue.CoverGuid
                };

                if(issue.CoverGuid.HasValue)
                    _ = LoadCoverThumbnailAsync(item, issue.CoverGuid.Value);

                IssuesList.Add(item);
            }

            if(IssuesList.Count == 0)
            {
                ErrorMessage = _localizer["No issues found for this year"].Value;
                HasError     = true;
            }
            else
                IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading issues for magazine {MagazineId} year {Year}", _magazineId, _year);
            ErrorMessage = _localizer["Failed to load issues. Please try again later."].Value;
            HasError     = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadCoverThumbnailAsync(MagazineIssueListItem item, Guid coverGuid)
    {
        try
        {
            Stream stream = await _coverCache.GetThumbnailAsync(coverGuid);
            item.CoverThumbnailSource = await _imageSourceFactory.CreateBitmapImageSourceAsync(stream);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading cover thumbnail for magazine issue {IssueId}", item.Id);
        }
    }
}
