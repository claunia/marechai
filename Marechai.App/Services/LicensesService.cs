#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;

namespace Marechai.App.Services;

public class LicensesService
{
    private readonly Client                   _apiClient;
    private readonly ILogger<LicensesService> _logger;

    public LicensesService(Client apiClient, ILogger<LicensesService> logger)
    {
        _apiClient = apiClient;
        _logger    = logger;
    }

    public async Task<List<LicenseDto>> GetLicensesAsync()
    {
        try
        {
            _logger.LogInformation("Fetching licenses from API");
            List<LicenseDto>? licenses = await _apiClient.Licenses.GetAsync();

            return licenses ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching licenses from API");

            return [];
        }
    }
}
