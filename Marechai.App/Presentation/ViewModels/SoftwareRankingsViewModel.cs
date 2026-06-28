#nullable enable

using System;
using System.Collections.Generic;
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
public partial class SoftwareRankingsViewModel : ObservableObject, IRegionAware
{
    private readonly SoftwareBrowsingService               _browsingService;
    private readonly IStringLocalizer                       _localizer;
    private readonly ILogger<SoftwareRankingsViewModel>     _logger;
    private readonly IRegionManager                         _regionManager;

    [ObservableProperty]
    private ObservableCollection<RankingGroup> _rankingGroups = [];

    [ObservableProperty]
    private RankingIndexEntryDto? _overallRanking;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private bool _isDataLoaded;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusText = string.Empty;

    public SoftwareRankingsViewModel(SoftwareBrowsingService              browsingService,
                                      IStringLocalizer                    localizer,
                                      ILogger<SoftwareRankingsViewModel>  logger,
                                      IRegionManager                      regionManager)
    {
        _browsingService          = browsingService;
        _localizer                = localizer;
        _logger                   = logger;
        _regionManager            = regionManager;
        LoadData                  = new AsyncRelayCommand(LoadDataAsync);
        GoBackCommand             = new AsyncRelayCommand(GoBackAsync);
        NavigateToRankingCommand  = new AsyncRelayCommand<RankingIndexEntryDto>(NavigateToRankingAsync);
    }

    public IAsyncRelayCommand                               LoadData                 { get; }
    public ICommand                                         GoBackCommand            { get; }
    public IAsyncRelayCommand<RankingIndexEntryDto>         NavigateToRankingCommand { get; }

    public bool IsNavigationTarget(NavigationContext navigationContext) => false;

    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading    = true;
            ErrorMessage = string.Empty;
            HasError     = false;
            IsDataLoaded = false;
            RankingGroups.Clear();
            OverallRanking = null;

            RankingIndexResponseDto? response = await _browsingService.GetRankingsIndexAsync();

            if(response?.Rankings is null || response.Rankings.Count == 0)
            {
                ErrorMessage = _localizer["No rankings found"].Value;
                HasError     = true;

                return;
            }

            UpdateStatusText(response.Status);

            OverallRanking = response.Rankings.FirstOrDefault(r => r.Dimension == 0);

            List<RankingIndexEntryDto> byGenre = response.Rankings.Where(r => r.Dimension == 1).ToList();
            List<RankingIndexEntryDto> byPlatform = response.Rankings.Where(r => r.Dimension == 2).ToList();

            AddGenreGroup(byGenre, genreType: 2, _localizer["By Gameplay"]);
            AddGenreGroup(byGenre, genreType: 0, _localizer["By Genre"]);
            AddGenreGroup(byGenre, genreType: 3, _localizer["By Setting"]);
            AddGenreGroup(byGenre, genreType: 4, _localizer["By Category"]);

            if(byPlatform.Count > 0)
            {
                RankingGroups.Add(new RankingGroup
                {
                    Title   = _localizer["By Platform"],
                    Entries = new ObservableCollection<RankingIndexEntryDto>(
                        byPlatform.OrderBy(r => r.DimensionName))
                });
            }

            IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading rankings index");
            ErrorMessage = _localizer["Failed to load rankings. Please try again later."].Value;
            HasError     = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void AddGenreGroup(List<RankingIndexEntryDto> byGenre, byte genreType, string title)
    {
        List<RankingIndexEntryDto> entries = byGenre.Where(r => r.GenreType == genreType)
                                                     .OrderBy(r => r.DimensionName)
                                                     .ToList();

        if(entries.Count == 0) return;

        RankingGroups.Add(new RankingGroup
        {
            Title   = title,
            Entries = new ObservableCollection<RankingIndexEntryDto>(entries)
        });
    }

    private void UpdateStatusText(RankingsStatusDto? status)
    {
        if(status is null) return;

        StatusText = status.IsComputing == true
                         ? _localizer["Rankings are being computed"]
                         : status.LastComputedAt.HasValue
                             ? string.Format(_localizer["Last computed: {0}"], status.LastComputedAt.Value.LocalDateTime)
                             : string.Empty;
    }

    private Task GoBackAsync()
    {
        _regionManager.RequestNavigate(RegionNames.Content, nameof(SoftwarePage));

        return Task.CompletedTask;
    }

    private Task NavigateToRankingAsync(RankingIndexEntryDto? entry)
    {
        if(entry?.Id is null) return Task.CompletedTask;

        var parameters = new NavigationParameters
        {
            { NavParamKeys.RankingId, entry.Id.Value },
            { NavParamKeys.NavigationSource, nameof(SoftwareRankingsViewModel) }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(SoftwareRankingDetailPage), parameters);

        return Task.CompletedTask;
    }
}

public class RankingGroup
{
    public string Title { get; set; } = string.Empty;

    public ObservableCollection<RankingIndexEntryDto> Entries { get; set; } = [];
}
