#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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
    private readonly List<CompanyListItem>       _allCompanies = [];
    private readonly CompaniesService            _companiesService;
    private readonly IStringLocalizer            _localizer;
    private readonly ILogger<CompaniesViewModel> _logger;
    private readonly CompanyLogoCache            _logoCache;
    private readonly IRegionManager              _regionManager;

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
    private string _searchQuery = string.Empty;

    public CompaniesViewModel(CompaniesService companiesService, CompanyLogoCache logoCache, IStringLocalizer localizer,
                              ILogger<CompaniesViewModel> logger, IRegionManager regionManager)
    {
        _companiesService        = companiesService;
        _logoCache               = logoCache;
        _localizer               = localizer;
        _logger                  = logger;
        _regionManager           = regionManager;
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
        // Automatically filter when SearchQuery changes
        UpdateFilter(value);
    }

    /// <summary>
    ///     Loads companies count and list from the API
    /// </summary>
    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading    = true;
            ErrorMessage = string.Empty;
            HasError     = false;
            IsDataLoaded = false;
            CompaniesList.Clear();
            _allCompanies.Clear();

            // Load companies
            List<CompanyDto> companies = await _companiesService.GetAllCompaniesAsync();

            // Set count
            CompanyCount     = companies.Count;
            CompanyCountText = _localizer["Companies in the database"];

            // Build the full list in memory
            foreach(CompanyDto company in companies)
            {
                // Extract id from company
                int companyId = company.Id ?? 0;

                // Convert DateTimeOffset? to DateTime?
                DateTime? foundedDate = company.Founded?.DateTime;

                // Load logo if available
                SvgImageSource? logoSource = null;

                if(company.LastLogo.HasValue)
                {
                    try
                    {
                        Stream? logoStream = await _logoCache.GetLogoAsync(company.LastLogo.Value);
                        logoSource = new SvgImageSource();
                        await logoSource.SetSourceAsync(logoStream.AsRandomAccessStream());
                    }
                    catch(Exception ex)
                    {
                        _logger.LogWarning("Failed to load logo for company {CompanyId}: {Exception}",
                                           companyId,
                                           ex.Message);
                    }
                }

                _allCompanies.Add(new CompanyListItem
                {
                    Id              = companyId,
                    Name            = company.Name ?? string.Empty,
                    FoundationDate  = foundedDate,
                    LogoImageSource = logoSource
                });
            }

            // Apply current filter (will show all if SearchQuery is empty)
            UpdateFilter(SearchQuery);

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
            IsLoading = false;
        }
    }

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

    /// <summary>
    ///     Updates the filtered list based on search query
    /// </summary>
    private void UpdateFilter(string? query)
    {
        string lowerQuery = string.IsNullOrWhiteSpace(query) ? string.Empty : query.Trim().ToLowerInvariant();

        CompaniesList.Clear();

        if(string.IsNullOrEmpty(lowerQuery))
        {
            // No filter, show all companies
            foreach(CompanyListItem company in _allCompanies) CompaniesList.Add(company);
        }
        else
        {
            // Filter companies by name (case-insensitive)
            var filtered = _allCompanies.Where(c => c.Name.Contains(lowerQuery, StringComparison.OrdinalIgnoreCase))
                                        .ToList();

            foreach(CompanyListItem company in filtered) CompaniesList.Add(company);
        }
    }
}