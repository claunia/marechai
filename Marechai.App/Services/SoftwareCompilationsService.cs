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

    public async Task<List<SoftwareCompilationDto>> GetPagedAsync(int skip, int take, string? search = null)
    {
        try
        {
            List<SoftwareCompilationDto>? compilations = await _apiClient.SoftwareCompilations.GetAsync(config =>
            {
                config.QueryParameters.Skip   = skip;
                config.QueryParameters.Take   = take;
                config.QueryParameters.Search = search;
            });

            return compilations ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching paged software compilations");

            return [];
        }
    }

    public async Task<int> GetCountAsync(string? search = null)
    {
        try
        {
            int? count = await _apiClient.SoftwareCompilations.Count.GetAsync(config =>
            {
                config.QueryParameters.Search = search;
            });

            return count ?? 0;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching software compilation count");

            return 0;
        }
    }

    public Task<List<SoftwareCompilationDto>> SearchAsync(string search, int take = 20) =>
        GetPagedAsync(0, take, search);

    public async Task<List<SoftwareDto>> SearchSoftwareAsync(string search, int take = 20)
    {
        if(string.IsNullOrWhiteSpace(search))
            return [];

        try
        {
            List<SoftwareDto>? software = await _apiClient.Software.GetAsync(config =>
            {
                config.QueryParameters.Search = search;
                config.QueryParameters.Take   = take;
            });

            return software ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error searching software for {Search}", search);

            return [];
        }
    }

    public async Task<List<MachineDto>> GetAllMachinesAsync()
    {
        try
        {
            List<MachineDto>? machines = await _apiClient.Machines.GetAsync();

            return machines ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching machines");

            return [];
        }
    }

    public async Task<List<SoftwareVersionDto>> GetSoftwareVersionsAsync(int softwareId)
    {
        try
        {
            List<SoftwareVersionDto>? versions = await _apiClient.Software[softwareId].Versions.GetAsync();

            return versions ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching versions for software {SoftwareId}", softwareId);

            return [];
        }
    }

    public async Task<int?> CreateAsync(SoftwareCompilationDto dto)
    {
        try
        {
            return await _apiClient.SoftwareCompilations.PostAsync(dto);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error creating software compilation");

            return null;
        }
    }

    public async Task<bool> UpdateAsync(SoftwareCompilationDto dto)
    {
        try
        {
            await _apiClient.SoftwareCompilations[(int)dto.Id.GetValueOrDefault()].PutAsync(dto);

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error updating software compilation {Id}", dto.Id);

            return false;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            await _apiClient.SoftwareCompilations[id].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting software compilation {Id}", id);

            return false;
        }
    }

    public async Task<bool> AddIncludedSoftwareAsync(SoftwareBySoftwareCompilationDto dto)
    {
        try
        {
            await _apiClient.SoftwareCompilations[(int)(dto.SoftwareCompilationId ?? 0)].Software.PostAsync(dto);

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error adding included software to compilation");

            return false;
        }
    }

    public async Task<bool> RemoveIncludedSoftwareAsync(int id, int softwareId)
    {
        try
        {
            await _apiClient.SoftwareCompilations[id].Software[softwareId].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error removing included software {SoftwareId} from compilation {Id}", softwareId, id);

            return false;
        }
    }

    public async Task<bool> AddIncludedVersionAsync(SoftwareVersionBySoftwareCompilationDto dto)
    {
        try
        {
            await _apiClient.SoftwareCompilations[(int)(dto.SoftwareCompilationId ?? 0)].Versions.PostAsync(dto);

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error adding included version to compilation");

            return false;
        }
    }

    public async Task<bool> RemoveIncludedVersionAsync(int id, int versionId)
    {
        try
        {
            await _apiClient.SoftwareCompilations[id].Versions[versionId].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error removing included version {VersionId} from compilation {Id}", versionId, id);

            return false;
        }
    }

    public async Task<bool> AddIncludedCompilationAsync(int id, int childId)
    {
        try
        {
            await _apiClient.SoftwareCompilations[id].Compilations[childId].PostAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error adding sub-compilation {ChildId} to compilation {Id}", childId, id);

            return false;
        }
    }

    public async Task<bool> RemoveIncludedCompilationAsync(int id, int childId)
    {
        try
        {
            await _apiClient.SoftwareCompilations[id].Compilations[childId].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error removing sub-compilation {ChildId} from compilation {Id}", childId, id);

            return false;
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
