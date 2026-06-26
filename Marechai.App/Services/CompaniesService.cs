#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.ApiClient.Companies;
using Marechai.ApiClient.Companies.Count;
using Marechai.ApiClient.Models;

namespace Marechai.App.Services;

/// <summary>
///     Service for fetching companies data from the API
/// </summary>
public class CompaniesService
{
    private readonly Client                 _apiClient;
    private readonly ILogger<CompaniesService> _logger;

    public CompaniesService(Client apiClient, ILogger<CompaniesService> logger)
    {
        _apiClient = apiClient;
        _logger    = logger;
    }

    /// <summary>
    ///     Gets all companies
    /// </summary>
    public async Task<List<CompanyDto>> GetAllCompaniesAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all companies from API");

            List<CompanyDto>? companies = await _apiClient.Companies.GetAsync();

            if(companies == null) return [];

            _logger.LogInformation("Successfully fetched {Count} total companies", companies.Count);

            return companies;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching all companies from API");

            return [];
        }
    }

    /// <summary>
    ///     Gets a page of companies with optional search
    /// </summary>
    public async Task<List<CompanyDto>> GetCompaniesPageAsync(int skip, int take, string? search = null)
    {
        try
        {
            _logger.LogInformation("Fetching companies page skip={Skip} take={Take} search={Search}", skip, take, search);

            List<CompanyDto>? companies = await _apiClient.Companies.GetAsync(cfg =>
            {
                cfg.QueryParameters.Skip = skip;
                cfg.QueryParameters.Take = take;

                if(!string.IsNullOrWhiteSpace(search)) cfg.QueryParameters.Search = search;
            });

            if(companies == null) return [];

            _logger.LogInformation("Successfully fetched {Count} companies", companies.Count);

            return companies;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching companies page from API");

            return [];
        }
    }

    /// <summary>
    ///     Gets the total count of companies with optional search filter
    /// </summary>
    public async Task<int> GetCompaniesCountAsync(string? search = null)
    {
        try
        {
            int? count = await _apiClient.Companies.Count.GetAsync(cfg =>
            {
                if(!string.IsNullOrWhiteSpace(search)) cfg.QueryParameters.Search = search;
            });

            return count ?? 0;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching companies count from API");

            return 0;
        }
    }

    /// <summary>
    ///     Gets a single company by ID
    /// </summary>
    public async Task<CompanyDto?> GetCompanyByIdAsync(int companyId)
    {
        try
        {
            _logger.LogInformation("Fetching company {CompanyId} from API", companyId);

            CompanyDto? company = await _apiClient.Companies[companyId].GetAsync();

            if(company == null)
            {
                _logger.LogWarning("Company {CompanyId} not found", companyId);

                return null;
            }

            _logger.LogInformation("Successfully fetched company {CompanyId}: {CompanyName}", companyId, company.Name);

            return company;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching company {CompanyId} from API", companyId);

            return null;
        }
    }

    public async Task<int?> CreateAsync(CompanyDto dto)
    {
        try
        {
            return await _apiClient.Companies.PostAsync(dto);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error creating company");

            return null;
        }
    }
}
