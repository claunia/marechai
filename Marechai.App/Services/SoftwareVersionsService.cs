#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;

namespace Marechai.App.Services;

public class SoftwareVersionsService
{
    private readonly Client                          _apiClient;
    private readonly ILogger<SoftwareVersionsService>   _logger;

    public SoftwareVersionsService(Client apiClient, ILogger<SoftwareVersionsService> logger)
    {
        _apiClient = apiClient;
        _logger    = logger;
    }

    // --- CRUD ---

    public async Task<List<SoftwareVersionDto>> GetAllAsync()
    {
        try
        {
            List<SoftwareVersionDto>? items = await _apiClient.Software.Versions.GetAsync();

            return items ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching software versions");

            return [];
        }
    }

    public async Task<List<SoftwareVersionDto>> GetBySoftwareAsync(int softwareId)
    {
        try
        {
            List<SoftwareVersionDto>? items =
                await _apiClient.Software[softwareId].Versions.GetAsync();

            return items ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching versions for software {SoftwareId}", softwareId);

            return [];
        }
    }

    public async Task<SoftwareVersionDto?> GetByIdAsync(int id)
    {
        try
        {
            return await _apiClient.Software.Versions[id].GetAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching software version {Id}", id);

            return null;
        }
    }

    public async Task<int?> CreateAsync(SoftwareVersionDto dto)
    {
        try
        {
            return await _apiClient.Software.Versions.PostAsync(dto);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error creating software version");

            return null;
        }
    }

    public async Task<bool> UpdateAsync(SoftwareVersionDto dto)
    {
        try
        {
            await _apiClient.Software.Versions[dto.Id.GetValueOrDefault()].PutAsync(dto);

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error updating software version {Id}", dto.Id);

            return false;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            await _apiClient.Software.Versions[id].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting software version {Id}", id);

            return false;
        }
    }

    // --- Requirements ---

    public async Task<List<SoftwareRequirementDto>> GetRequirementsAsync(int versionId)
    {
        try
        {
            List<SoftwareRequirementDto>? items =
                await _apiClient.Software.Versions[versionId].Requirements.GetAsync();

            return items ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching requirements for version {VersionId}", versionId);

            return [];
        }
    }

    public async Task<bool> AddRequirementAsync(SoftwareRequirementDto dto)
    {
        try
        {
            await _apiClient.Software.Requirements.PostAsync(dto);

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error adding requirement");

            return false;
        }
    }

    public async Task<bool> RemoveRequirementAsync(string versionId, string requiredVersionId, string requirementType)
    {
        try
        {
            await _apiClient.Software.Requirements[versionId][requiredVersionId][requirementType].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error removing requirement {VersionId}/{RequiredVersionId}/{RequirementType}",
                versionId, requiredVersionId, requirementType);

            return false;
        }
    }

    // --- OS Compatibility ---

    public async Task<List<SoftwareOSCompatibilityDto>> GetOSCompatibilityAsync(int versionId)
    {
        try
        {
            List<SoftwareOSCompatibilityDto>? items =
                await _apiClient.Software.Versions[versionId].OsCompatibility.GetAsync();

            return items ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching OS compatibility for version {VersionId}", versionId);

            return [];
        }
    }

    public async Task<bool> AddOSCompatibilityAsync(SoftwareOSCompatibilityDto dto)
    {
        try
        {
            await _apiClient.Software.OsCompatibility.PostAsync(dto);

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error adding OS compatibility");

            return false;
        }
    }

    public async Task<bool> RemoveOSCompatibilityAsync(string versionId, string osVersionId)
    {
        try
        {
            await _apiClient.Software.OsCompatibility[versionId][osVersionId].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error removing OS compatibility {VersionId}/{OsVersionId}",
                versionId, osVersionId);

            return false;
        }
    }

    // --- Companies by Version ---

    public async Task<List<CompanyBySoftwareVersionDto>> GetCompaniesAsync(int versionId)
    {
        try
        {
            List<CompanyBySoftwareVersionDto>? items =
                await _apiClient.Software.Versions[versionId].Companies.GetAsync();

            return items ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching companies for version {VersionId}", versionId);

            return [];
        }
    }

    public async Task<int?> AddCompanyAsync(CompanyBySoftwareVersionDto dto)
    {
        try
        {
            return await _apiClient.Software.Versions.Companies.PostAsync(dto);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error adding company to version");

            return null;
        }
    }

    public async Task<bool> RemoveCompanyAsync(int id)
    {
        try
        {
            await _apiClient.Software.Versions.Companies[id].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error removing company from version {Id}", id);

            return false;
        }
    }
}
