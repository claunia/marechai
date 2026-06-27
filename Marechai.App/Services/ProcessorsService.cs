#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Marechai.App.Services;

/// <summary>
///     Service for fetching and managing Processors from the Marechai API
/// </summary>
public class ProcessorsService
{
    private readonly Client                  _apiClient;
    private readonly ILogger<ProcessorsService> _logger;

    public ProcessorsService(Client apiClient, ILogger<ProcessorsService> logger)
    {
        _apiClient = apiClient;
        _logger    = logger;
    }

    /// <summary>
    ///     Fetches all Processors from the API
    /// </summary>
    public async Task<List<ProcessorDto>> GetAllProcessorsAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all Processors from API");

            List<ProcessorDto>? processors = await _apiClient.Processors.GetAsync();

            if(processors == null) return [];

            _logger.LogInformation("Successfully fetched {Count} total Processors", processors.Count);

            return processors;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching all Processors from API");

            return [];
        }
    }

    /// <summary>
    ///     Fetches a single Processor by ID from the API
    /// </summary>
    public async Task<ProcessorDto?> GetProcessorByIdAsync(int processorId)
    {
        try
        {
            _logger.LogInformation("Fetching Processor {ProcessorId} from API", processorId);

            ProcessorDto? processor = await _apiClient.Processors[processorId].GetAsync();

            if(processor == null)
            {
                _logger.LogWarning("Processor {ProcessorId} not found", processorId);

                return null;
            }

            _logger.LogInformation("Successfully fetched Processor {ProcessorId}: {ProcessorName}",
                                   processorId,
                                   processor.Name);

            return processor;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching Processor {ProcessorId} from API", processorId);

            return null;
        }
    }

    /// <summary>
    ///     Fetches machines that use a specific Processor
    /// </summary>
    public async Task<List<MachineDto>> GetMachinesByProcessorAsync(int processorId)
    {
        try
        {
            _logger.LogInformation("Fetching machines for Processor {ProcessorId}", processorId);

            List<MachineDto>? machines = await _apiClient.Processors[processorId].Machines.GetAsync();

            _logger.LogInformation("Successfully fetched {Count} machines for Processor {ProcessorId}",
                                   machines?.Count ?? 0,
                                   processorId);

            return machines ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogWarning(ex, "Error fetching machines for Processor {ProcessorId}", processorId);

            return [];
        }
    }

    /// <summary>
    ///     Fetches a localized description for a processor
    /// </summary>
    public async Task<ProcessorDescriptionDto?> GetDescriptionAsync(int processorId, string languageCode)
    {
        try
        {
            _logger.LogInformation("Fetching description for processor {ProcessorId} lang {Lang}", processorId, languageCode);

            ProcessorDescriptionDto? desc = await _apiClient.Processors[processorId].Description.GetAsync(
                config => config.QueryParameters.Lang = languageCode);

            return desc;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching description for processor {ProcessorId}", processorId);

            return null;
        }
    }

    /// <summary>
    ///     Fetches photo IDs for a processor
    /// </summary>
    public async Task<List<Guid>> GetProcessorPhotosAsync(int processorId)
    {
        try
        {
            _logger.LogInformation("Fetching photos for Processor {ProcessorId}", processorId);

            List<Guid?> photoIds = await _apiClient.Processors[processorId].Photos.GetAsync();

            if(photoIds == null) return [];

            _logger.LogInformation("Successfully fetched {Count} photos for Processor {ProcessorId}", photoIds.Count, processorId);

            return photoIds.Where(id => id.HasValue).Select(id => id!.Value).ToList();
        }
        catch(Exception ex)
        {
            _logger.LogWarning(ex, "Error fetching photos for Processor {ProcessorId}", processorId);

            return [];
        }
    }

    /// <summary>
    ///     Fetches full photo details for a Processor photo
    /// </summary>
    public async Task<ProcessorPhotoDto> GetProcessorPhotoDetailsAsync(Guid photoId)
    {
        try
        {
            _logger.LogInformation("Fetching Processor photo {PhotoId} from API", photoId);

            ProcessorPhotoDto photo = await _apiClient.Processors.Photos[photoId].GetAsync();

            if(photo == null)
            {
                _logger.LogWarning("Processor photo {PhotoId} not found", photoId);

                return null;
            }

            return photo;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching Processor photo {PhotoId} from API", photoId);

            return null;
        }
    }
}