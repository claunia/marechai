using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Marechai.App.Services;

/// <summary>
///     Service for fetching and managing GPUs from the Marechai API
/// </summary>
public class GpusService
{
    private readonly ApiClient            _apiClient;
    private readonly ILogger<GpusService> _logger;

    public GpusService(ApiClient apiClient, ILogger<GpusService> logger)
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
    public async Task<GpuDto?> GetGpuByIdAsync(int gpuId)
    {
        try
        {
            _logger.LogInformation("Fetching GPU {GpuId} from API", gpuId);

            GpuDto? gpu = await _apiClient.Gpus[gpuId].GetAsync();

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
}