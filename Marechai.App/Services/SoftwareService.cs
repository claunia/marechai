#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;

namespace Marechai.App.Services;

public class SoftwareService
{
    private readonly Client                  _apiClient;
    private readonly ILogger<SoftwareService>   _logger;

    public SoftwareService(Client apiClient, ILogger<SoftwareService> logger)
    {
        _apiClient = apiClient;
        _logger    = logger;
    }

    // --- CRUD ---

    public async Task<List<SoftwareDto>> GetAllAsync()
    {
        try
        {
            List<SoftwareDto>? items = await _apiClient.Software.GetAsync();

            return items ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching software");

            return [];
        }
    }

    public async Task<SoftwareDto?> GetByIdAsync(int id)
    {
        try
        {
            return await _apiClient.Software[id].GetAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching software {Id}", id);

            return null;
        }
    }

    public async Task<int?> CreateAsync(SoftwareDto dto)
    {
        try
        {
            return await _apiClient.Software.PostAsync(dto);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error creating software");

            return null;
        }
    }

    public async Task<bool> UpdateAsync(SoftwareDto dto)
    {
        try
        {
            await _apiClient.Software[dto.Id.GetValueOrDefault()].PutAsync(dto);

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error updating software {Id}", dto.Id);

            return false;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            await _apiClient.Software[id].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting software {Id}", id);

            return false;
        }
    }

    // --- Company Roles ---

    public async Task<List<SoftwareCompanyRoleDto>> GetCompanyRolesAsync(int softwareId)
    {
        try
        {
            List<SoftwareCompanyRoleDto>? items =
                await _apiClient.Software[softwareId].CompanyRoles.GetAsync();

            return items ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching company roles for software {SoftwareId}", softwareId);

            return [];
        }
    }

    public async Task<bool> AddCompanyRoleAsync(SoftwareCompanyRoleDto dto)
    {
        try
        {
            await _apiClient.Software.CompanyRoles.PostAsync(dto);

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error adding company role to software");

            return false;
        }
    }

    public async Task<bool> RemoveCompanyRoleAsync(string softwareId, string companyId, string roleId)
    {
        try
        {
            await _apiClient.Software.CompanyRoles[softwareId][companyId][roleId].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error removing company role from software");

            return false;
        }
    }

    // --- Software Roles (lookup) ---

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
