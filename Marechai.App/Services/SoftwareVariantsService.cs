#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;

namespace Marechai.App.Services;

public class SoftwareVariantsService
{
    private readonly Client                          _apiClient;
    private readonly ILogger<SoftwareVariantsService>   _logger;

    public SoftwareVariantsService(Client apiClient, ILogger<SoftwareVariantsService> logger)
    {
        _apiClient = apiClient;
        _logger    = logger;
    }

    // --- CRUD ---

    public async Task<List<SoftwareVariantDto>> GetAllAsync()
    {
        try
        {
            List<SoftwareVariantDto>? items = await _apiClient.Software.Variants.GetAsync();

            return items ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching software variants");

            return [];
        }
    }

    public async Task<List<SoftwareVariantDto>> GetBySoftwareAsync(int softwareId)
    {
        try
        {
            List<SoftwareVariantDto>? items =
                await _apiClient.Software[softwareId].Variants.GetAsync();

            return items ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching variants for software {SoftwareId}", softwareId);

            return [];
        }
    }

    public async Task<SoftwareVariantDto?> GetByIdAsync(int id)
    {
        try
        {
            return await _apiClient.Software.Variants[id].GetAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching software variant {Id}", id);

            return null;
        }
    }

    public async Task<int?> CreateAsync(SoftwareVariantDto dto)
    {
        try
        {
            return await _apiClient.Software.Variants.PostAsync(dto);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error creating software variant");

            return null;
        }
    }

    public async Task<bool> UpdateAsync(SoftwareVariantDto dto)
    {
        try
        {
            await _apiClient.Software.Variants[dto.Id.GetValueOrDefault()].PutAsync(dto);

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error updating software variant {Id}", dto.Id);

            return false;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            await _apiClient.Software.Variants[id].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting software variant {Id}", id);

            return false;
        }
    }

    // --- Languages ---

    public async Task<List<SoftwareVariantLanguageDto>> GetLanguagesAsync(int variantId)
    {
        try
        {
            List<SoftwareVariantLanguageDto>? items =
                await _apiClient.Software.Variants[variantId].Languages.GetAsync();

            return items ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching languages for variant {VariantId}", variantId);

            return [];
        }
    }

    public async Task<bool> AddLanguageAsync(SoftwareVariantLanguageDto dto)
    {
        try
        {
            await _apiClient.Software.VariantLanguages.PostAsync(dto);

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error adding language to variant");

            return false;
        }
    }

    public async Task<bool> RemoveLanguageAsync(string variantId, string languageCode)
    {
        try
        {
            await _apiClient.Software.VariantLanguages[variantId][languageCode].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error removing language from variant {VariantId}/{LanguageCode}",
                variantId, languageCode);

            return false;
        }
    }

    // --- Companies by Variant ---

    public async Task<List<CompanyBySoftwareVariantDto>> GetCompaniesAsync(int variantId)
    {
        try
        {
            List<CompanyBySoftwareVariantDto>? items =
                await _apiClient.Software.Variants[variantId].Companies.GetAsync();

            return items ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching companies for variant {VariantId}", variantId);

            return [];
        }
    }

    public async Task<int?> AddCompanyAsync(CompanyBySoftwareVariantDto dto)
    {
        try
        {
            return await _apiClient.Software.Variants.Companies.PostAsync(dto);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error adding company to variant");

            return null;
        }
    }

    public async Task<bool> RemoveCompanyAsync(int id)
    {
        try
        {
            await _apiClient.Software.Variants.Companies[id].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error removing company from variant {Id}", id);

            return false;
        }
    }
}
