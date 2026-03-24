#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;

namespace Marechai.App.Services;

public class SoftwareSubvariantsService
{
    private readonly Client                              _apiClient;
    private readonly ILogger<SoftwareSubvariantsService>    _logger;

    public SoftwareSubvariantsService(Client apiClient, ILogger<SoftwareSubvariantsService> logger)
    {
        _apiClient = apiClient;
        _logger    = logger;
    }

    // --- CRUD ---

    public async Task<List<SoftwareSubvariantDto>> GetByVariantAsync(int variantId)
    {
        try
        {
            List<SoftwareSubvariantDto>? items =
                await _apiClient.Software.Variants[variantId].Subvariants.GetAsync();

            return items ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching subvariants for variant {VariantId}", variantId);

            return [];
        }
    }

    public async Task<SoftwareSubvariantDto?> GetByIdAsync(int id)
    {
        try
        {
            return await _apiClient.Software.Subvariants[id].GetAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching software subvariant {Id}", id);

            return null;
        }
    }

    public async Task<int?> CreateAsync(SoftwareSubvariantDto dto)
    {
        try
        {
            return await _apiClient.Software.Subvariants.PostAsync(dto);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error creating software subvariant");

            return null;
        }
    }

    public async Task<bool> UpdateAsync(SoftwareSubvariantDto dto)
    {
        try
        {
            await _apiClient.Software.Subvariants[dto.Id.GetValueOrDefault()].PutAsync(dto);

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error updating software subvariant {Id}", dto.Id);

            return false;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            await _apiClient.Software.Subvariants[id].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting software subvariant {Id}", id);

            return false;
        }
    }

    // --- Languages ---

    public async Task<List<SoftwareSubvariantLanguageDto>> GetLanguagesAsync(int subvariantId)
    {
        try
        {
            List<SoftwareSubvariantLanguageDto>? items =
                await _apiClient.Software.Subvariants[subvariantId].Languages.GetAsync();

            return items ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching languages for subvariant {SubvariantId}", subvariantId);

            return [];
        }
    }

    public async Task<bool> AddLanguageAsync(SoftwareSubvariantLanguageDto dto)
    {
        try
        {
            await _apiClient.Software.SubvariantLanguages.PostAsync(dto);

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error adding language to subvariant");

            return false;
        }
    }

    public async Task<bool> RemoveLanguageAsync(string subvariantId, string languageCode)
    {
        try
        {
            await _apiClient.Software.SubvariantLanguages[subvariantId][languageCode].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error removing language from subvariant {SubvariantId}/{LanguageCode}",
                subvariantId, languageCode);

            return false;
        }
    }
}
