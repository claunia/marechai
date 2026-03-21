#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.App.Models;

namespace Marechai.App.Services;

public class SoftwareFamiliesService
{
    private readonly ApiClient                          _apiClient;
    private readonly ILogger<SoftwareFamiliesService>   _logger;

    public SoftwareFamiliesService(ApiClient apiClient, ILogger<SoftwareFamiliesService> logger)
    {
        _apiClient = apiClient;
        _logger    = logger;
    }

    // --- CRUD ---

    public async Task<List<SoftwareFamilyDto>> GetAllAsync()
    {
        try
        {
            List<SoftwareFamilyDto>? items = await _apiClient.Software.Families.GetAsync();

            return items ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching software families");

            return [];
        }
    }

    public async Task<SoftwareFamilyDto?> GetByIdAsync(int id)
    {
        try
        {
            return await _apiClient.Software.Families[id].GetAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching software family {Id}", id);

            return null;
        }
    }

    public async Task<int?> CreateAsync(SoftwareFamilyDto dto)
    {
        try
        {
            return await _apiClient.Software.Families.PostAsync(dto);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error creating software family");

            return null;
        }
    }

    public async Task<bool> UpdateAsync(SoftwareFamilyDto dto)
    {
        try
        {
            await _apiClient.Software.Families[dto.Id.GetValueOrDefault()].PutAsync(dto);

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error updating software family {Id}", dto.Id);

            return false;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            await _apiClient.Software.Families[id].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting software family {Id}", id);

            return false;
        }
    }

    // --- Companies by Software Family ---

    public async Task<List<CompanyBySoftwareFamilyDto>> GetCompaniesAsync(int familyId)
    {
        try
        {
            List<CompanyBySoftwareFamilyDto>? items =
                await _apiClient.Software.Families[familyId].Companies.GetAsync();

            return items ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching companies for software family {FamilyId}", familyId);

            return [];
        }
    }

    public async Task<int?> AddCompanyAsync(CompanyBySoftwareFamilyDto dto)
    {
        try
        {
            return await _apiClient.Software.Families.Companies.PostAsync(dto);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error adding company to software family");

            return null;
        }
    }

    public async Task<bool> RemoveCompanyAsync(int id)
    {
        try
        {
            await _apiClient.Software.Families.Companies[id].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error removing company from software family {Id}", id);

            return false;
        }
    }

    // --- Software Roles ---

    public async Task<List<SoftwareRoleDto>> GetRolesAsync()
    {
        try
        {
            List<SoftwareRoleDto>? roles = await _apiClient.Software.Roles.Enabled.GetAsync();

            return roles ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching software roles");

            return [];
        }
    }
}
