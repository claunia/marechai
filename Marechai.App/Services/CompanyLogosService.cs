#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.ApiClient.Companies.Logos.ChangeYear.Item;
using Marechai.ApiClient.Companies.Logos.Upload;
using Marechai.ApiClient.Models;

namespace Marechai.App.Services;

public class CompanyLogosService
{
    private readonly Client                      _apiClient;
    private readonly ILogger<CompanyLogosService> _logger;

    public CompanyLogosService(Client apiClient, ILogger<CompanyLogosService> logger)
    {
        _apiClient = apiClient;
        _logger    = logger;
    }

    public async Task<List<CompanyLogoDto>> GetLogosAsync(int companyId)
    {
        try
        {
            _logger.LogInformation("Fetching logos for company {CompanyId}", companyId);
            List<CompanyLogoDto>? logos = await _apiClient.Companies[companyId].Logos.GetAsync();

            return logos ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching logos for company {CompanyId}", companyId);

            return [];
        }
    }

    public async Task<CompanyLogoDto?> UploadLogoAsync(int companyId, byte[] svgBytes, int? year)
    {
        try
        {
            _logger.LogInformation("Uploading logo for company {CompanyId}", companyId);

            var body = new UploadPostRequestBody
            {
                CompanyId = companyId,
                File      = svgBytes,
                Year      = year
            };

            CompanyLogoDto? result = await _apiClient.Companies.Logos.Upload.PostAsync(body);

            _logger.LogInformation("Successfully uploaded logo for company {CompanyId}", companyId);

            return result;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error uploading logo for company {CompanyId}", companyId);

            return null;
        }
    }

    public async Task<bool> ChangeYearAsync(int logoId, int? year)
    {
        try
        {
            _logger.LogInformation("Changing year for logo {LogoId} to {Year}", logoId, year);

            var body = new ChangeYearItemRequestBuilder.ChangeYearPutRequestBody
            {
                Integer = year
            };

            await _apiClient.Companies.Logos.ChangeYear[logoId].PutAsync(body);

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error changing year for logo {LogoId}", logoId);

            return false;
        }
    }

    public async Task<bool> DeleteLogoAsync(int logoId)
    {
        try
        {
            _logger.LogInformation("Deleting logo {LogoId}", logoId);
            await _apiClient.Companies.Logos[logoId].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting logo {LogoId}", logoId);

            return false;
        }
    }
}
