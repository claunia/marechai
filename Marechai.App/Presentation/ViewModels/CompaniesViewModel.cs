#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Marechai.App.Helpers;
using Marechai.App.Presentation.Models;
using Marechai.App.Services;
using Uno.Extensions.Navigation;

namespace Marechai.App.Presentation.ViewModels;

public partial class CompaniesViewModel : ObservableObject
{
    private readonly List<CompanyListItem>       _allCompanies = [];
    private readonly CompaniesService            _companiesService;
    private readonly IStringLocalizer            _localizer;
    private readonly ILogger<CompaniesViewModel> _logger;
    private readonly INavigator                  _navigator;

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

    public CompaniesViewModel(CompaniesService            companiesService, IStringLocalizer localizer,
                              ILogger<CompaniesViewModel> logger,           INavigator       navigator)
    {
        _companiesService        = companiesService;
        _localizer               = localizer;
        _logger                  = logger;
        _navigator               = navigator;
        LoadData                 = new AsyncRelayCommand(LoadDataAsync);
        GoBackCommand            = new AsyncRelayCommand(GoBackAsync);
        NavigateToCompanyCommand = new AsyncRelayCommand<CompanyListItem>(NavigateToCompanyAsync);
    }

    public IAsyncRelayCommand                  LoadData                 { get; }
    public ICommand                            GoBackCommand            { get; }
    public IAsyncRelayCommand<CompanyListItem> NavigateToCompanyCommand { get; }
    public string                              Title                    { get; } = "Companies";

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
                // Extract id from UntypedNode
                int companyId = UntypedNodeExtractor.ExtractInt(company.Id);

                // Convert DateTimeOffset? to DateTime?
                DateTime? foundedDate = company.Founded?.DateTime;

                _allCompanies.Add(new CompanyListItem
                {
                    Id             = companyId,
                    Name           = company.Name ?? string.Empty,
                    FoundationDate = foundedDate
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
    private async Task GoBackAsync()
    {
        await _navigator.NavigateViewModelAsync<MainViewModel>(this);
    }

    /// <summary>
    ///     Navigates to company detail view
    /// </summary>
    private async Task NavigateToCompanyAsync(CompanyListItem? company)
    {
        if(company is null) return;

        _logger.LogInformation("Navigating to company: {CompanyName} (ID: {CompanyId})", company.Name, company.Id);

        // Navigate to company detail view with navigation parameter
        var navParam = new CompanyDetailNavigationParameter
        {
            CompanyId        = company.Id,
            NavigationSource = this
        };

        await _navigator.NavigateViewModelAsync<CompanyDetailViewModel>(this, data: navParam);
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

/// <summary>
///     Data model for a company in the list
/// </summary>
public class CompanyListItem
{
    public int       Id             { get; set; }
    public string    Name           { get; set; } = string.Empty;
    public DateTime? FoundationDate { get; set; }

    public string FoundationDateDisplay =>
        FoundationDate.HasValue ? FoundationDate.Value.ToString("MMMM d, yyyy") : string.Empty;
}