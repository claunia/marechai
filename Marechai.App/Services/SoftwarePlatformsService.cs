#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Microsoft.Kiota.Abstractions;

namespace Marechai.App.Services;

public class SoftwarePlatformsService
{
    private readonly Client                          _apiClient;
    private readonly ILogger<SoftwarePlatformsService>  _logger;

    public SoftwarePlatformsService(Client apiClient, ILogger<SoftwarePlatformsService> logger)
    {
        _apiClient = apiClient;
        _logger    = logger;
    }

    // --- CRUD ---

    public async Task<List<SoftwarePlatformDto>> GetAllAsync()
    {
        try
        {
            List<SoftwarePlatformDto>? items = await _apiClient.Software.Platforms.GetAsync();

            return items ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching software platforms");

            return [];
        }
    }

    public async Task<SoftwarePlatformDto?> GetByIdAsync(int id)
    {
        try
        {
            return await _apiClient.Software.Platforms[id].GetAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching software platform {Id}", id);

            return null;
        }
    }

    public async Task<int?> CreateAsync(SoftwarePlatformDto dto)
    {
        try
        {
            return await _apiClient.Software.Platforms.PostAsync(dto);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error creating software platform");

            return null;
        }
    }

    public async Task<bool> UpdateAsync(SoftwarePlatformDto dto)
    {
        try
        {
            await _apiClient.Software.Platforms[dto.Id.GetValueOrDefault()].PutAsync(dto);

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error updating software platform {Id}", dto.Id);

            return false;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            await _apiClient.Software.Platforms[id].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting software platform {Id}", id);

            return false;
        }
    }

    public async Task<SoftwarePlatformDto?> UploadLogoAsync(int id, byte[] fileBytes, string fileName,
                                                             string contentType)
    {
        try
        {
            var body = new MultipartBody();
            body.AddOrReplacePart("file", contentType, new MemoryStream(fileBytes), fileName);

            return await _apiClient.Software.Platforms[id].Logo.PostAsync(body);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error uploading logo for software platform {Id}", id);

            return null;
        }
    }

    public async Task<bool> DeleteLogoAsync(int id)
    {
        try
        {
            await _apiClient.Software.Platforms[id].Logo.DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting logo for software platform {Id}", id);

            return false;
        }
    }
}
