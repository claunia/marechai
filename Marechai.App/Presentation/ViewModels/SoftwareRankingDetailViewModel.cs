#nullable enable

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Marechai.ApiClient.Models;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Views;
using Marechai.App.Services;
using Microsoft.UI.Xaml.Data;

namespace Marechai.App.Presentation.ViewModels;

[Bindable]
public partial class SoftwareRankingDetailViewModel : ObservableObject, IRegionAware
{
    private readonly SoftwareBrowsingService                  _browsingService;
    private readonly IStringLocalizer                          _localizer;
    private readonly ILogger<SoftwareRankingDetailViewModel>   _logger;
    private readonly IRegionManager                            _regionManager;

    private int _rankingId;

    [ObservableProperty]
    private ObservableCollection<SoftwareRankingDto> _entries = [];

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private bool _isDataLoaded;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isEmpty;

    [ObservableProperty]
    private string _pageTitle = string.Empty;

    public SoftwareRankingDetailViewModel(SoftwareBrowsingService                   browsingService,
                                           IStringLocalizer                         localizer,
                                           ILogger<SoftwareRankingDetailViewModel>  logger,
                                           IRegionManager                           regionManager)
    {
        _browsingService           = browsingService;
        _localizer                 = localizer;
        _logger                    = logger;
        _regionManager             = regionManager;
        LoadData                   = new AsyncRelayCommand(LoadDataAsync);
        GoBackCommand               = new AsyncRelayCommand(GoBackAsync);
        NavigateToSoftwareCommand  = new AsyncRelayCommand<SoftwareRankingDto>(NavigateToSoftwareAsync);
    }

    public IAsyncRelayCommand                              LoadData                  { get; }
    public ICommand                                        GoBackCommand             { get; }
    public IAsyncRelayCommand<SoftwareRankingDto>         NavigateToSoftwareCommand { get; }

    public bool IsNavigationTarget(NavigationContext navigationContext) => false;

    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        if(navigationContext.Parameters.TryGetValue<int>(NavParamKeys.RankingId, out int rankingId))
        {
            _rankingId = rankingId;
            _ = LoadDataAsync();
        }
    }

    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading    = true;
            ErrorMessage = string.Empty;
            HasError     = false;
            IsDataLoaded = false;
            IsEmpty      = false;
            Entries.Clear();

            await LoadTitleAsync();

            System.Collections.Generic.List<SoftwareRankingDto> entries =
                await _browsingService.GetRankingAsync(_rankingId);

            foreach(SoftwareRankingDto entry in entries) Entries.Add(entry);

            if(Entries.Count == 0)
                IsEmpty = true;
            else
                IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading ranking {RankingId}", _rankingId);
            ErrorMessage = _localizer["Failed to load ranking. Please try again later."].Value;
            HasError     = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadTitleAsync()
    {
        RankingIndexResponseDto? index = await _browsingService.GetRankingsIndexAsync();

        RankingIndexEntryDto? entry = index?.Rankings?.FirstOrDefault(r => r.Id == _rankingId);

        PageTitle = entry?.Dimension == 0
                        ? _localizer["Top250SoftwareLabel"]
                        : entry?.DimensionName ?? _localizer["RankingsTitle"];
    }

    private Task GoBackAsync()
    {
        _regionManager.RequestNavigate(RegionNames.Content, nameof(SoftwareRankingsPage));

        return Task.CompletedTask;
    }

    private Task NavigateToSoftwareAsync(SoftwareRankingDto? entry)
    {
        if(entry?.SoftwareId is null) return Task.CompletedTask;

        var parameters = new NavigationParameters
        {
            { NavParamKeys.SoftwareId, entry.SoftwareId.Value },
            { NavParamKeys.NavigationSource, nameof(SoftwareRankingDetailViewModel) }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(SoftwareViewPage), parameters);

        return Task.CompletedTask;
    }
}
