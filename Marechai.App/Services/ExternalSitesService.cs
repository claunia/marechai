#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;

namespace Marechai.App.Services;

public class ExternalSitesService
{
    private readonly Client                         _apiClient;
    private readonly ILogger<ExternalSitesService>  _logger;

    public ExternalSitesService(Client apiClient, ILogger<ExternalSitesService> logger)
    {
        _apiClient = apiClient;
        _logger    = logger;
    }

    public async Task<List<ExternalSiteDto>> GetAllAsync()
    {
        try
        {
            List<ExternalSiteDto>? items = await _apiClient.ExternalSites.GetAsync();

            return items ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching external sites");

            return [];
        }
    }

    public async Task<ExternalSiteDto?> CreateAsync(ExternalSiteDto dto)
    {
        try
        {
            return await _apiClient.ExternalSites.PostAsync(dto);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error creating external site");

            return null;
        }
    }

    public async Task<bool> UpdateAsync(ExternalSiteDto dto)
    {
        try
        {
            await _apiClient.ExternalSites[dto.Id.GetValueOrDefault()].PutAsync(dto);

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error updating external site {Id}", dto.Id);

            return false;
        }
    }

    public async Task<bool> DeleteAsync(long id)
    {
        try
        {
            await _apiClient.ExternalSites[id].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting external site {Id}", id);

            return false;
        }
    }
}
