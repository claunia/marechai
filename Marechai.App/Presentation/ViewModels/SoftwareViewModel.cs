#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Marechai.App.Models;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Views;
using Marechai.App.Services;
using Microsoft.UI.Xaml.Data;

namespace Marechai.App.Presentation.ViewModels;

[Bindable]
public partial class SoftwareViewModel : ObservableObject
{
    private readonly SoftwareBrowsingService      _browsingService;
    private readonly ISoftwareListFilterContext    _filterContext;
    private readonly IStringLocalizer              _localizer;
    private readonly ILogger<SoftwareViewModel>    _logger;
    private readonly IRegionManager                _regionManager;

    [ObservableProperty]
    private int _softwareCount;

    [ObservableProperty]
    private string _softwareCountText = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private bool _isDataLoaded;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private ObservableCollection<char> _lettersList = [];

    [ObservableProperty]
    private int _maximumYear;

    [ObservableProperty]
    private int _minimumYear;

    [ObservableProperty]
    private string _yearsGridTitle = string.Empty;

    [ObservableProperty]
    private ObservableCollection<int> _yearsList = [];

    [ObservableProperty]
    private string _platformsGridTitle = string.Empty;

    [ObservableProperty]
    private ObservableCollection<SoftwarePlatformDto> _platformsList = [];

    [ObservableProperty]
    private string _specificationsGridTitle = string.Empty;

    [ObservableProperty]
    private ObservableCollection<SoftwareSpecGroup> _specificationGroups = [];

    public SoftwareViewModel(SoftwareBrowsingService      browsingService, IStringLocalizer localizer,
                             ILogger<SoftwareViewModel>    logger,          IRegionManager   regionManager,
                             ISoftwareListFilterContext    filterContext)
    {
        _browsingService            = browsingService;
        _localizer                  = localizer;
        _logger                     = logger;
        _regionManager              = regionManager;
        _filterContext              = filterContext;
        LoadData                    = new AsyncRelayCommand(LoadDataAsync);
        GoBackCommand               = new AsyncRelayCommand(GoBackAsync);
        NavigateByLetterCommand     = new AsyncRelayCommand<char>(NavigateByLetterAsync);
        NavigateByYearCommand       = new AsyncRelayCommand<int>(NavigateByYearAsync);
        NavigateByPlatformCommand   = new AsyncRelayCommand<SoftwarePlatformDto>(NavigateByPlatformAsync);
        NavigateBySpecCommand       = new AsyncRelayCommand<SoftwareSpecValueItem>(NavigateBySpecAsync);
        NavigateAllSoftwareCommand  = new AsyncRelayCommand(NavigateAllSoftwareAsync);
        Title = _localizer["Software"];

        InitializeLetters();
    }

    public IAsyncRelayCommand                         LoadData                   { get; }
    public ICommand                                   GoBackCommand              { get; }
    public IAsyncRelayCommand<char>                   NavigateByLetterCommand    { get; }
    public IAsyncRelayCommand<int>                    NavigateByYearCommand      { get; }
    public IAsyncRelayCommand<SoftwarePlatformDto>    NavigateByPlatformCommand  { get; }
    public IAsyncRelayCommand<SoftwareSpecValueItem>  NavigateBySpecCommand      { get; }
    public IAsyncRelayCommand                         NavigateAllSoftwareCommand { get; }
    public string                                     Title                      { get; }

    private void InitializeLetters()
    {
        LettersList.Clear();

        for(var c = 'A'; c <= 'Z'; c++) LettersList.Add(c);
    }

    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading    = true;
            ErrorMessage = string.Empty;
            HasError     = false;
            IsDataLoaded = false;
            YearsList.Clear();
            PlatformsList.Clear();
            SpecificationGroups.Clear();

            Task<int>                        countTask     = _browsingService.GetSoftwareCountAsync();
            Task<int>                        minYearTask   = _browsingService.GetMinimumYearAsync();
            Task<int>                        maxYearTask   = _browsingService.GetMaximumYearAsync();
            Task<List<SoftwarePlatformDto>>  platformsTask = _browsingService.GetAllPlatformsAsync();
            Task<List<SoftwareSpecKeyDto>>   specsTask     = _browsingService.GetSpecificationsAsync();
            await Task.WhenAll(countTask, minYearTask, maxYearTask, platformsTask, specsTask);

            SoftwareCount = countTask.Result;
            MinimumYear   = minYearTask.Result;
            MaximumYear   = maxYearTask.Result;

            SoftwareCountText = _localizer["Software in the database"];

            if(MinimumYear > 0 && MaximumYear > 0)
            {
                for(int year = MinimumYear; year <= MaximumYear; year++) YearsList.Add(year);

                YearsGridTitle = string.Format(_localizer["Browse by Year ({0} - {1})"], MinimumYear, MaximumYear);
            }

            List<SoftwarePlatformDto> platforms = platformsTask.Result;

            if(platforms.Count > 0)
            {
                PlatformsGridTitle = _localizer["Browse by Platform"];

                foreach(SoftwarePlatformDto platform in platforms) PlatformsList.Add(platform);
            }

            List<SoftwareSpecKeyDto> specs = specsTask.Result;

            if(specs.Count > 0)
            {
                SpecificationsGridTitle = _localizer["Browse by Specifications"];

                foreach(SoftwareSpecKeyDto spec in specs)
                {
                    var values = new ObservableCollection<SoftwareSpecValueItem>();

                    foreach(string val in spec.Values ?? [])
                    {
                        values.Add(new SoftwareSpecValueItem
                        {
                            Key          = spec.Key ?? string.Empty,
                            Value        = val,
                            DisplayValue = _localizer[val]
                        });
                    }

                    SpecificationGroups.Add(new SoftwareSpecGroup
                    {
                        Key        = spec.Key ?? string.Empty,
                        DisplayKey = _localizer[spec.Key ?? string.Empty],
                        Values     = values
                    });
                }
            }

            if(SoftwareCount == 0)
            {
                ErrorMessage = _localizer["No software found"].Value;
                HasError     = true;
            }
            else
                IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError("Error loading software data: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to load software data. Please try again later."].Value;
            HasError     = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private Task GoBackAsync()
    {
        _regionManager.RequestNavigate(RegionNames.Content, nameof(NewsPage));

        return Task.CompletedTask;
    }

    private Task NavigateByLetterAsync(char letter)
    {
        try
        {
            _logger.LogInformation("Navigating to software by letter: {Letter}", letter);
            _filterContext.FilterType  = SoftwareListFilterType.Letter;
            _filterContext.FilterValue = letter.ToString();
            _regionManager.RequestNavigate(RegionNames.Content, nameof(SoftwareListPage));
        }
        catch(Exception ex)
        {
            _logger.LogError("Error navigating to letter software: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to navigate. Please try again."].Value;
            HasError     = true;
        }

        return Task.CompletedTask;
    }

    private Task NavigateByYearAsync(int year)
    {
        try
        {
            _logger.LogInformation("Navigating to software by year: {Year}", year);
            _filterContext.FilterType  = SoftwareListFilterType.Year;
            _filterContext.FilterValue = year.ToString();
            _regionManager.RequestNavigate(RegionNames.Content, nameof(SoftwareListPage));
        }
        catch(Exception ex)
        {
            _logger.LogError("Error navigating to year software: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to navigate. Please try again."].Value;
            HasError     = true;
        }

        return Task.CompletedTask;
    }

    private Task NavigateByPlatformAsync(SoftwarePlatformDto? platform)
    {
        if(platform is null) return Task.CompletedTask;

        try
        {
            _logger.LogInformation("Navigating to software by platform: {Platform}", platform.Name);
            _filterContext.FilterType  = SoftwareListFilterType.Platform;
            _filterContext.FilterValue = platform.Id?.ToString() ?? string.Empty;
            _regionManager.RequestNavigate(RegionNames.Content, nameof(SoftwareListPage));
        }
        catch(Exception ex)
        {
            _logger.LogError("Error navigating to platform software: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to navigate. Please try again."].Value;
            HasError     = true;
        }

        return Task.CompletedTask;
    }

    private Task NavigateBySpecAsync(SoftwareSpecValueItem? specItem)
    {
        if(specItem is null) return Task.CompletedTask;

        try
        {
            _logger.LogInformation("Navigating to software by spec: {Key}={Value}", specItem.Key, specItem.Value);
            _filterContext.FilterType  = SoftwareListFilterType.Spec;
            _filterContext.FilterValue = $"{specItem.Key}|{specItem.Value}";
            _regionManager.RequestNavigate(RegionNames.Content, nameof(SoftwareListPage));
        }
        catch(Exception ex)
        {
            _logger.LogError("Error navigating to spec software: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to navigate. Please try again."].Value;
            HasError     = true;
        }

        return Task.CompletedTask;
    }

    private Task NavigateAllSoftwareAsync()
    {
        try
        {
            _logger.LogInformation("Navigating to all software");
            _filterContext.FilterType  = SoftwareListFilterType.All;
            _filterContext.FilterValue = string.Empty;
            _regionManager.RequestNavigate(RegionNames.Content, nameof(SoftwareListPage));
        }
        catch(Exception ex)
        {
            _logger.LogError("Error navigating to all software: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to navigate. Please try again."].Value;
            HasError     = true;
        }

        return Task.CompletedTask;
    }
}
