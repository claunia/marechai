#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Marechai.App.Services;

/// <summary>
///     Service for fetching companies data from the API
/// </summary>
public class CompaniesService
{
    private readonly ApiClient                 _apiClient;
    private readonly ILogger<CompaniesService> _logger;

    public CompaniesService(ApiClient apiClient, ILogger<CompaniesService> logger)
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
}