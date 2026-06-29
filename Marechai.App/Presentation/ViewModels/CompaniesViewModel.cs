#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Marechai.App.Navigation;
using Marechai.App.Presentation.Models;
using Marechai.App.Presentation.Views;
using Marechai.App.Services;
using Marechai.App.Services.Caching;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Marechai.App.Presentation.ViewModels;

public partial class CompaniesViewModel : ObservableObject
{
    private const int PageSize = 50;

    private readonly CompaniesService            _companiesService;
    private readonly ImageSourceFactory          _imageSourceFactory;
    private readonly IStringLocalizer            _localizer;
    private readonly ILogger<CompaniesViewModel> _logger;
    private readonly CompanyLogoCache            _logoCache;
    private readonly IRegionManager              _regionManager;

    private bool   _hasMoreItems = true;
    private bool   _isLoadingMore;
    private string _lastSearchQuery = string.Empty;

    [ObservableProperty]
    private ObservableCollection<CompanyListItem> _companiesList = [];

    [ObservableProperty]
    private int _companyCount;

    [ObservableProperty]
    private string _companyCountText = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private bool _isDataLoaded;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isLoadingNextPage;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    public CompaniesViewModel(CompaniesService companiesService, CompanyLogoCache logoCache, IStringLocalizer localizer,
                              ILogger<CompaniesViewModel> logger, IRegionManager regionManager,
                              ImageSourceFactory imageSourceFactory)
    {
        _companiesService        = companiesService;
        _logoCache               = logoCache;
        _localizer               = localizer;
        _logger                  = logger;
        _regionManager           = regionManager;
        _imageSourceFactory      = imageSourceFactory;
        LoadData                 = new AsyncRelayCommand(LoadDataAsync);
        GoBackCommand            = new AsyncRelayCommand(GoBackAsync);
        NavigateToCompanyCommand = new AsyncRelayCommand<CompanyListItem>(NavigateToCompanyAsync);

        Title = _localizer["Companies"];
    }

    public IAsyncRelayCommand                  LoadData                 { get; }
    public ICommand                            GoBackCommand            { get; }
    public IAsyncRelayCommand<CompanyListItem> NavigateToCompanyCommand { get; }
    public string                              Title                    { get; }

    partial void OnSearchQueryChanged(string value)
    {
        // Debounce: just trigger a fresh search
        _ = SearchAsync(value);
    }

    /// <summary>
    ///     Resets and loads from the beginning with the current search query
    /// </summary>
    private async Task SearchAsync(string query)
    {
        string trimmed = query?.Trim() ?? string.Empty;

        // Avoid re-fetching if query hasn't changed
        if(trimmed == _lastSearchQuery) return;

        _lastSearchQuery = trimmed;

        CompaniesList.Clear();
        _hasMoreItems = true;
        IsDataLoaded  = false;

        await LoadPageAsync(true);
    }

    /// <summary>
    ///     Initial data load
    /// </summary>
    private async Task LoadDataAsync()
    {
        _lastSearchQuery = string.Empty;
        SearchQuery      = string.Empty;
        CompaniesList.Clear();
        _hasMoreItems = true;
        await LoadPageAsync(true);
    }

    /// <summary>
    ///     Loads the next page of companies
    /// </summary>
    private async Task LoadPageAsync(bool isInitial)
    {
        if(_isLoadingMore || !_hasMoreItems) return;

        _isLoadingMore = true;

        try
        {
            if(isInitial)
            {
                IsLoading    = true;
                ErrorMessage = string.Empty;
                HasError     = false;
            }
            else
                IsLoadingNextPage = true;

            string? search = string.IsNullOrEmpty(_lastSearchQuery) ? null : _lastSearchQuery;

            // Fetch count on initial load
            if(isInitial)
            {
                int count = await _companiesService.GetCompaniesCountAsync(search);
                CompanyCount     = count;
                CompanyCountText = _localizer["Companies in the database"];
            }

            int skip = CompaniesList.Count;

            List<CompanyDto> companies = await _companiesService.GetCompaniesPageAsync(skip, PageSize, search);

            if(companies.Count < PageSize) _hasMoreItems = false;

            foreach(CompanyDto company in companies)
            {
                int      companyId   = company.Id ?? 0;
                DateTime? foundedDate = company.Founded?.DateTime;

                BitmapImage? logoSource = null;

                if(company.LastLogo.HasValue)
                {
                    try
                    {
                        Stream? logoStream = await _logoCache.GetLogoAsync(company.LastLogo.Value);
                        logoSource = await _imageSourceFactory.CreateSvgImageSourceAsync(logoStream);
                    }
                    catch(Exception ex)
                    {
                        _logger.LogWarning("Failed to load logo for company {CompanyId}: {Exception}",
                                           companyId,
                                           ex.Message);
                    }
                }

                CompaniesList.Add(new CompanyListItem
                {
                    Id                  = companyId,
                    Name                = company.Name ?? string.Empty,
                    FoundationDate      = foundedDate,
                    FoundationPrecision = company.FoundedPrecision,
                    LogoImageSource     = logoSource
                });
            }

            if(CompaniesList.Count == 0)
            {
                ErrorMessage = _localizer["No companies found"].Value;
                HasError     = true;
            }
            else
                IsDataLoaded = true;
        }
        catch(Exception ex)
        {
            _logger.LogError("Error loading companies data: {Exception}", ex.Message);
            ErrorMessage = _localizer["Failed to load companies data. Please try again later."].Value;
            HasError     = true;
        }
        finally
        {
            IsLoading         = false;
            IsLoadingNextPage = false;
            _isLoadingMore    = false;
        }
    }

    /// <summary>
    ///     Called by the view when the user scrolls near the end
    /// </summary>
    public Task LoadMoreAsync() => LoadPageAsync(false);

    /// <summary>
    ///     Handles back navigation
    /// </summary>
    private Task GoBackAsync()
    {
        _regionManager.RequestNavigate(RegionNames.Content, nameof(NewsPage));

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Navigates to company detail view
    /// </summary>
    private Task NavigateToCompanyAsync(CompanyListItem? company)
    {
        if(company is null) return Task.CompletedTask;

        _logger.LogInformation("Navigating to company: {CompanyName} (ID: {CompanyId})", company.Name, company.Id);

        var parameters = new NavigationParameters
        {
            { NavParamKeys.CompanyId, company.Id },
            { NavParamKeys.NavigationSource, nameof(CompaniesViewModel) }
        };

        _regionManager.RequestNavigate(RegionNames.Content, nameof(CompanyDetailPage), parameters);

        return Task.CompletedTask;
    }
}
