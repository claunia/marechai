#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Models;
using Marechai.App.Presentation.Views;
using Marechai.App.Services;
using Microsoft.UI.Xaml.Data;

namespace Marechai.App.Presentation.ViewModels;

[Bindable]
public partial class MagazinesListViewModel : ObservableObject
{
    private readonly MagazinesService                _magazinesService;
    private readonly IMagazinesListFilterContext      _filterContext;
    private readonly IStringLocalizer                _localizer;
    private readonly ILogger<MagazinesListViewModel> _logger;
    private readonly IRegionManager                  _regionManager;

    [ObservableProperty]
    private ObservableCollection<MagazineListItem> _magazinesList = [];

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string _filterDescription = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private bool _isDataLoaded;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _pageTitle = string.Empty;

    public MagazinesListViewModel(MagazinesService                magazinesService, IStringLocalizer localizer,
                                  ILogger<MagazinesListViewModel> logger,           IRegionManager   regionManager,
                                  IMagazinesListFilterContext      filterContext)
    {
        _magazinesService = magazinesService;
        _localizer        = localizer;
        _logger           = logger;
        _regionManager    = regionManager;
        _filterContext    = filterContext;
        LoadData                = new AsyncRelayCommand(LoadDataAsync);
        GoBackCommand           = new AsyncRelayCommand(GoBackAsync);
        NavigateToMagazineCommand = new AsyncRelayCommand<MagazineListItem>(NavigateToMagazineAsync);
    }

    public IAsyncRelayCommand                     LoadData                  { get; }
    public ICommand                               GoBackCommand             { get; }
    public IAsyncRelayCommand<MagazineListItem>   NavigateToMagazineCommand { get; }

    public MagazineListFilterType FilterType
    {
        get => _filterContext.FilterType;
        set => _filterContext.FilterType = value;
    }

    public string FilterValue
    {
        get => _filterContext.FilterValue;
        set => _filterContext.FilterValue = value;
    }

    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading    = true;
            ErrorMessage = string.Empty;
            HasError     = false;
            IsDataLoaded = false;
            MagazinesList.Clear();

            UpdateFilterDescription();

            List<MagazineDto> magazines = FilterType switch
            {
                MagazineListFilterType.Letter when FilterValue.Length == 1 =>
                    await _magazinesService.GetMagazinesByLetterAsync(FilterValue[0]),
                MagazineListFilterType.Year when int.TryParse(FilterValue, out int year) =>
                    await _magazinesService.GetMagazinesByYearAsync(year),
                _ => await _magazinesService.GetAllMagazinesAsync()
            };

            foreach(MagazineDto mag in magazines)
            {
                MagazinesList.Add(new MagazineListItem
                {
                    Id    = mag.Id ?? 0,
                    Title = mag.Title ?? string.Empty,
                    Year  = mag.FirstPublication?.Year
                });
            }

            if(MagazinesList.Count == 0)
            {
                ErrorMessage = _localizer["No magazines found for this filter"].Value;
                HasError     = true;
            }
            else
                IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading magazines");
            ErrorMessage = _localizer["Failed to load magazines. Please try again later."].Value;
            HasError     = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void UpdateFilterDescription()
    {
        switch(FilterType)
        {
            case MagazineListFilterType.All:
                PageTitle         = _localizer["All Magazines"];
                FilterDescription = _localizer["Browsing all magazines in the database"];

                break;

            case MagazineListFilterType.Letter:
                if(!string.IsNullOrEmpty(FilterValue) && FilterValue.Length == 1)
                {
                    PageTitle         = $"{_localizer["Magazines Starting with"]} {FilterValue}";
                    FilterDescription = $"{_localizer["Showing magazines that start with"]} {FilterValue}";
                }

                break;

            case MagazineListFilterType.Year:
                if(!string.IsNullOrEmpty(FilterValue) && int.TryParse(FilterValue, out int year))
                {
                    PageTitle         = $"{_localizer["Magazines from"]} {year}";
                    FilterDescription = $"{_localizer["Showing magazines first published in"]} {year}";
                }

                break;
        }
    }

    private Task GoBackAsync()
    {
        _regionManager.RequestNavigate(RegionNames.Content, nameof(MagazinesPage));

        return Task.CompletedTask;
    }

    private Task NavigateToMagazineAsync(MagazineListItem? magazine)
    {
        if(magazine is null) return Task.CompletedTask;

        var parameters = new NavigationParameters
        {
            { NavParamKeys.MagazineId, magazine.Id },
            { NavParamKeys.NavigationSource, nameof(MagazinesListViewModel) }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(MagazineViewPage), parameters);

        return Task.CompletedTask;
    }
}
