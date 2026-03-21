#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.App.Models;

namespace Marechai.App.Services;

public class SoftwareReleasesService
{
    private readonly ApiClient                          _apiClient;
    private readonly ILogger<SoftwareReleasesService>   _logger;

    public SoftwareReleasesService(ApiClient apiClient, ILogger<SoftwareReleasesService> logger)
    {
        _apiClient = apiClient;
        _logger    = logger;
    }

    // --- CRUD ---

    public async Task<List<SoftwareReleaseDto>> GetAllAsync()
    {
        try
        {
            List<SoftwareReleaseDto>? items = await _apiClient.Software.Releases.GetAsync();

            return items ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching software releases");

            return [];
        }
    }

    public async Task<List<SoftwareReleaseDto>> GetByVersionAsync(int versionId)
    {
        try
        {
            List<SoftwareReleaseDto>? items =
                await _apiClient.Software.Versions[versionId].Releases.GetAsync();

            return items ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching releases for version {VersionId}", versionId);

            return [];
        }
    }

    public async Task<SoftwareReleaseDto?> GetByIdAsync(int id)
    {
        try
        {
            return await _apiClient.Software.Releases[id].GetAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching software release {Id}", id);

            return null;
        }
    }

    public async Task<int?> CreateAsync(SoftwareReleaseDto dto)
    {
        try
        {
            return await _apiClient.Software.Releases.PostAsync(dto);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error creating software release");

            return null;
        }
    }

    public async Task<bool> UpdateAsync(SoftwareReleaseDto dto)
    {
        try
        {
            await _apiClient.Software.Releases[dto.Id.GetValueOrDefault()].PutAsync(dto);

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error updating software release {Id}", dto.Id);

            return false;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            await _apiClient.Software.Releases[id].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting software release {Id}", id);

            return false;
        }
    }

    // --- Barcodes ---

    public async Task<List<SoftwareBarcodeDto>> GetBarcodesAsync(int releaseId)
    {
        try
        {
            List<SoftwareBarcodeDto>? items =
                await _apiClient.Software.Releases[releaseId].Barcodes.GetAsync();

            return items ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching barcodes for release {ReleaseId}", releaseId);

            return [];
        }
    }

    public async Task<int?> AddBarcodeAsync(SoftwareBarcodeDto dto)
    {
        try
        {
            return await _apiClient.Software.Barcodes.PostAsync(dto);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error adding barcode to release");

            return null;
        }
    }

    public async Task<bool> RemoveBarcodeAsync(int id)
    {
        try
        {
            await _apiClient.Software.Barcodes[id].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error removing barcode {Id}", id);

            return false;
        }
    }

    // --- Product Codes ---

    public async Task<List<SoftwareProductCodeDto>> GetProductCodesAsync(int releaseId)
    {
        try
        {
            List<SoftwareProductCodeDto>? items =
                await _apiClient.Software.Releases[releaseId].ProductCodes.GetAsync();

            return items ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching product codes for release {ReleaseId}", releaseId);

            return [];
        }
    }

    public async Task<int?> AddProductCodeAsync(SoftwareProductCodeDto dto)
    {
        try
        {
            return await _apiClient.Software.ProductCodes.PostAsync(dto);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error adding product code to release");

            return null;
        }
    }

    public async Task<bool> RemoveProductCodeAsync(int id)
    {
        try
        {
            await _apiClient.Software.ProductCodes[id].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error removing product code {Id}", id);

            return false;
        }
    }

    // --- Minimum GPUs ---

    public async Task<List<GpuBySoftwareReleaseDto>> GetMinimumGpusAsync(int releaseId)
    {
        try
        {
            List<GpuBySoftwareReleaseDto>? items =
                await _apiClient.Software.Releases[releaseId].MinimumGpus.GetAsync();

            return items ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching minimum GPUs for release {ReleaseId}", releaseId);

            return [];
        }
    }

    public async Task<bool> AddMinimumGpuAsync(GpuBySoftwareReleaseDto dto)
    {
        try
        {
            await _apiClient.Software.Releases.MinimumGpus.PostAsync(dto);

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error adding minimum GPU to release");

            return false;
        }
    }

    public async Task<bool> RemoveMinimumGpuAsync(string releaseId, string gpuId)
    {
        try
        {
            await _apiClient.Software.Releases.MinimumGpus[releaseId][gpuId].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error removing minimum GPU from release");

            return false;
        }
    }

    // --- Recommended GPUs ---

    public async Task<List<GpuBySoftwareReleaseDto>> GetRecommendedGpusAsync(int releaseId)
    {
        try
        {
            List<GpuBySoftwareReleaseDto>? items =
                await _apiClient.Software.Releases[releaseId].RecommendedGpus.GetAsync();

            return items ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching recommended GPUs for release {ReleaseId}", releaseId);

            return [];
        }
    }

    public async Task<bool> AddRecommendedGpuAsync(GpuBySoftwareReleaseDto dto)
    {
        try
        {
            await _apiClient.Software.Releases.RecommendedGpus.PostAsync(dto);

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error adding recommended GPU to release");

            return false;
        }
    }

    public async Task<bool> RemoveRecommendedGpuAsync(string releaseId, string gpuId)
    {
        try
        {
            await _apiClient.Software.Releases.RecommendedGpus[releaseId][gpuId].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error removing recommended GPU from release");

            return false;
        }
    }

    // --- Sound Synths ---

    public async Task<List<SoundSynthBySoftwareReleaseDto>> GetSoundSynthsAsync(int releaseId)
    {
        try
        {
            List<SoundSynthBySoftwareReleaseDto>? items =
                await _apiClient.Software.Releases[releaseId].SoundSynths.GetAsync();

            return items ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching sound synths for release {ReleaseId}", releaseId);

            return [];
        }
    }

    public async Task<bool> AddSoundSynthAsync(SoundSynthBySoftwareReleaseDto dto)
    {
        try
        {
            await _apiClient.Software.Releases.SoundSynths.PostAsync(dto);

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error adding sound synth to release");

            return false;
        }
    }

    public async Task<bool> RemoveSoundSynthAsync(string releaseId, string soundSynthId)
    {
        try
        {
            await _apiClient.Software.Releases.SoundSynths[releaseId][soundSynthId].DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error removing sound synth from release");

            return false;
        }
    }
}
