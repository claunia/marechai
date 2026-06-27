using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Marechai.App.Services;

/// <summary>
///     Service for fetching and managing GPUs from the Marechai API
/// </summary>
public class GpusService
{
    private readonly Client            _apiClient;
    private readonly ILogger<GpusService> _logger;

    public GpusService(Client apiClient, ILogger<GpusService> logger)
    {
        _apiClient = apiClient;
        _logger    = logger;
    }

    /// <summary>
    ///     Fetches all GPUs from the API
    /// </summary>
    public async Task<List<GpuDto>> GetAllGpusAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all GPUs from API");

            List<GpuDto> gpus = await _apiClient.Gpus.GetAsync();

            if(gpus == null) return [];

            _logger.LogInformation("Successfully fetched {Count} total GPUs", gpus.Count);

            return gpus;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching all GPUs from API");

            return [];
        }
    }

    /// <summary>
    ///     Fetches a single GPU by ID from the API
    /// </summary>
    public async Task<GpuDto> GetGpuByIdAsync(int gpuId)
    {
        try
        {
            _logger.LogInformation("Fetching GPU {GpuId} from API", gpuId);

            GpuDto gpu = await _apiClient.Gpus[gpuId].GetAsync();

            if(gpu == null)
            {
                _logger.LogWarning("GPU {GpuId} not found", gpuId);

                return null;
            }

            _logger.LogInformation("Successfully fetched GPU {GpuId}: {GpuName}", gpuId, gpu.Name);

            return gpu;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching GPU {GpuId} from API", gpuId);

            return null;
        }
    }

    /// <summary>
    ///     Fetches resolutions supported by a GPU
    /// </summary>
    public async Task<List<ResolutionByGpuDto>> GetResolutionsByGpuAsync(int gpuId)
    {
        try
        {
            _logger.LogInformation("Fetching resolutions for GPU {GpuId}", gpuId);

            // Fetch from the resolutions-by-gpu/gpus/{gpuId}/resolutions endpoint
            List<ResolutionByGpuDto> resolutions = await _apiClient.ResolutionsByGpu.Gpus[gpuId].Resolutions.GetAsync();

            if(resolutions == null) return [];

            _logger.LogInformation("Successfully fetched {Count} resolutions for GPU {GpuId}",
                                   resolutions.Count,
                                   gpuId);

            return resolutions;
        }
        catch(Exception ex)
        {
            _logger.LogWarning(ex, "Error fetching resolutions for GPU {GpuId}", gpuId);

            return [];
        }
    }

    /// <summary>
    ///     Fetches machines that use a specific GPU
    /// </summary>
    public async Task<List<MachineDto>> GetMachinesByGpuAsync(int gpuId)
    {
        try
        {
            _logger.LogInformation("Fetching machines for GPU {GpuId}", gpuId);

            // Fetch from the gpus/{gpuId}/machines endpoint
            List<MachineDto> machines = await _apiClient.Gpus[gpuId].Machines.GetAsync();

            if(machines == null) return [];

            _logger.LogInformation("Successfully fetched {Count} machines for GPU {GpuId}", machines.Count, gpuId);

            return machines;
        }
        catch(Exception ex)
        {
            _logger.LogWarning(ex, "Error fetching machines for GPU {GpuId}", gpuId);

            return [];
        }
    }

    /// <summary>
    ///     Fetches a single resolution by ID from the API
    /// </summary>
    public async Task<ResolutionDto> GetResolutionByIdAsync(int resolutionId)
    {
        try
        {
            _logger.LogInformation("Fetching resolution {ResolutionId} from API", resolutionId);

            ResolutionDto resolution = await _apiClient.Resolutions[resolutionId].GetAsync();

            if(resolution == null)
            {
                _logger.LogWarning("Resolution {ResolutionId} not found", resolutionId);

                return null;
            }

            _logger.LogInformation("Successfully fetched resolution {ResolutionId}: {Width}x{Height}",
                                   resolutionId,
                                   resolution.Width,
                                   resolution.Height);

            return resolution;
        }
        catch(Exception ex)
        {
            _logger.LogWarning(ex, "Error fetching resolution {ResolutionId} from API", resolutionId);

            return null;
        }
    }

    /// <summary>
    ///     Fetches a localized description for a GPU
    /// </summary>
    public async Task<GpuDescriptionDto> GetDescriptionAsync(int gpuId, string languageCode)
    {
        try
        {
            _logger.LogInformation("Fetching description for GPU {GpuId} lang {Lang}", gpuId, languageCode);

            GpuDescriptionDto desc = await _apiClient.Gpus[gpuId].Description.GetAsync(
                config => config.QueryParameters.Lang = languageCode);

            return desc;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching description for GPU {GpuId}", gpuId);

            return null;
        }
    }

    /// <summary>
    ///     Fetches full photo details for a GPU photo
    /// </summary>
    public async Task<GpuPhotoDto> GetGpuPhotoDetailsAsync(Guid photoId)
    {
        try
        {
            _logger.LogInformation("Fetching GPU photo {PhotoId} from API", photoId);

            GpuPhotoDto photo = await _apiClient.Gpus.Photos[photoId].GetAsync();

            if(photo == null)
            {
                _logger.LogWarning("GPU photo {PhotoId} not found", photoId);

                return null;
            }

            return photo;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching GPU photo {PhotoId} from API", photoId);

            return null;
        }
    }

    /// <summary>
    ///     Fetches photo IDs for a GPU
    /// </summary>
    public async Task<List<Guid>> GetGpuPhotosAsync(int gpuId)
    {
        try
        {
            _logger.LogInformation("Fetching photos for GPU {GpuId}", gpuId);

            List<Guid?> photoIds = await _apiClient.Gpus[gpuId].Photos.GetAsync();

            if(photoIds == null) return [];

            _logger.LogInformation("Successfully fetched {Count} photos for GPU {GpuId}", photoIds.Count, gpuId);

            return photoIds.Where(id => id.HasValue).Select(id => id!.Value).ToList();
        }
        catch(Exception ex)
        {
            _logger.LogWarning(ex, "Error fetching photos for GPU {GpuId}", gpuId);

            return [];
        }
    }
}