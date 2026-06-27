#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Marechai.App.Services;

public sealed class SoftwareCompilationsService
{
    private readonly Client                               _apiClient;
    private readonly ILogger<SoftwareCompilationsService> _logger;

    public SoftwareCompilationsService(Client apiClient, ILogger<SoftwareCompilationsService> logger)
    {
        _apiClient = apiClient;
        _logger    = logger;
    }

    public async Task<SoftwareCompilationDto?> GetAsync(int id)
    {
        try
        {
            return await _apiClient.SoftwareCompilations[id].GetAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching software compilation {CompilationId}", id);

            return null;
        }
    }

    public async Task<List<SoftwareReleaseDto>> GetReleasesAsync(int id)
    {
        try
        {
            List<SoftwareReleaseDto>? releases = await _apiClient.SoftwareCompilations[id].Releases.GetAsync();

            return releases ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching releases for software compilation {CompilationId}", id);

            return [];
        }
    }

    public async Task<List<SoftwareBySoftwareCompilationDto>> GetIncludedSoftwareAsync(int id)
    {
        try
        {
            List<SoftwareBySoftwareCompilationDto>? software =
                await _apiClient.SoftwareCompilations[id].Software.GetAsync();

            return software ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching included software for software compilation {CompilationId}", id);

            return [];
        }
    }

    public async Task<List<SoftwareVersionBySoftwareCompilationDto>> GetIncludedVersionsAsync(int id)
    {
        try
        {
            List<SoftwareVersionBySoftwareCompilationDto>? versions =
                await _apiClient.SoftwareCompilations[id].Versions.GetAsync();

            return versions ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching included versions for software compilation {CompilationId}", id);

            return [];
        }
    }

    public async Task<List<SoftwareCompilationDto>> GetIncludedCompilationsAsync(int id)
    {
        try
        {
            List<SoftwareCompilationDto>? compilations =
                await _apiClient.SoftwareCompilations[id].Compilations.GetAsync();

            return compilations ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching included compilations for software compilation {CompilationId}", id);

            return [];
        }
    }
}
