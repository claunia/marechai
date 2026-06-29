#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Microsoft.Kiota.Abstractions;

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
    ///     Fetches all stored description translations for a processor.
    /// </summary>
    public async Task<List<ProcessorDescriptionDto>> GetDescriptionsAsync(int processorId)
    {
        try
        {
            _logger.LogInformation("Fetching descriptions for processor {ProcessorId}", processorId);

            List<ProcessorDescriptionDto>? descriptions = await _apiClient.Processors[processorId].Descriptions.GetAsync();

            return descriptions ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching descriptions for processor {ProcessorId}", processorId);

            return [];
        }
    }

    /// <summary>
    ///     Creates or updates a processor description translation.
    /// </summary>
    public async Task<(bool Succeeded, string? Error)> CreateOrUpdateDescriptionAsync(int processorId,
        ProcessorDescriptionDto dto)
    {
        try
        {
            await _apiClient.Processors[processorId].Description.PostAsync(dto);

            return (true, null);
        }
        catch(ApiException ex)
        {
            _logger.LogError(ex, "Error saving description for processor {ProcessorId} in {LanguageCode}",
                             processorId,
                             dto.LanguageCode);

            return (false, ExtractDetail(ex));
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error saving description for processor {ProcessorId} in {LanguageCode}",
                             processorId,
                             dto.LanguageCode);

            return (false, ex.Message);
        }
    }

    /// <summary>
    ///     Deletes a processor description translation.
    /// </summary>
    public async Task<(bool Succeeded, string? Error)> DeleteDescriptionAsync(int processorId, string languageCode)
    {
        try
        {
            await _apiClient.Processors[processorId].Description[languageCode].DeleteAsync();

            return (true, null);
        }
        catch(ApiException ex)
        {
            _logger.LogError(ex, "Error deleting description for processor {ProcessorId} in {LanguageCode}",
                             processorId,
                             languageCode);

            return (false, ExtractDetail(ex));
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting description for processor {ProcessorId} in {LanguageCode}",
                             processorId,
                             languageCode);

            return (false, ex.Message);
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

    /// <summary>
    ///     Fetches public videos for a processor
    /// </summary>
    public async Task<List<ProcessorVideoDto>> GetVideosByProcessorAsync(int processorId)
    {
        try
        {
            _logger.LogInformation("Fetching videos for Processor {ProcessorId}", processorId);

            List<ProcessorVideoDto>? videos = await _apiClient.Processors[processorId].Videos.GetAsync();

            if(videos == null) return [];

            _logger.LogInformation("Successfully fetched {Count} videos for Processor {ProcessorId}",
                                   videos.Count,
                                   processorId);

            return videos;
        }
        catch(Exception ex)
        {
            _logger.LogWarning(ex, "Error fetching videos for Processor {ProcessorId}", processorId);

            return [];
        }
    }

    /// <summary>
    ///     Creates a new video link for a processor.
    /// </summary>
    public async Task<(ProcessorVideoDto? dto, string? error)> CreateVideoAsync(int processorId, string provider,
                                                                                 string videoId, string? title)
    {
        try
        {
            _logger.LogInformation("Creating video link for Processor {ProcessorId}", processorId);

            ProcessorVideoDto? dto = await _apiClient.Processors[processorId]
                                                     .Videos
                                                     .PostAsync(new CreateProcessorVideoRequest
                                                      {
                                                          Provider = provider,
                                                          VideoId  = videoId,
                                                          Title    = title
                                                      });

            return (dto, null);
        }
        catch(ApiException ex)
        {
            _logger.LogWarning(ex, "API error creating processor video for {ProcessorId}", processorId);

            return (null, ExtractDetail(ex));
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error creating processor video for {ProcessorId}", processorId);

            return (null, ex.Message);
        }
    }

    /// <summary>
    ///     Updates a processor video title.
    /// </summary>
    public async Task<(bool succeeded, string? error)> UpdateVideoTitleAsync(long id, string? title)
    {
        try
        {
            _logger.LogInformation("Updating processor video {VideoId}", id);

            await _apiClient.Processors.Videos[id]
                            .PutAsync(new UpdateProcessorVideoRequest
                             {
                                 Title = title
                             });

            return (true, null);
        }
        catch(ApiException ex)
        {
            _logger.LogWarning(ex, "API error updating processor video {VideoId}", id);

            return (false, ExtractDetail(ex));
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error updating processor video {VideoId}", id);

            return (false, ex.Message);
        }
    }

    /// <summary>
    ///     Deletes a processor video.
    /// </summary>
    public async Task<(bool succeeded, string? error)> DeleteVideoAsync(long id)
    {
        try
        {
            _logger.LogInformation("Deleting processor video {VideoId}", id);

            await _apiClient.Processors.Videos[id].DeleteAsync();

            return (true, null);
        }
        catch(ApiException ex)
        {
            _logger.LogWarning(ex, "API error deleting processor video {VideoId}", id);

            return (false, ExtractDetail(ex));
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting processor video {VideoId}", id);

            return (false, ex.Message);
        }
    }

    static string? ExtractDetail(ApiException exception)
    {
        if(exception is ProblemDetails problemDetails)
        {
            if(!string.IsNullOrWhiteSpace(problemDetails.Detail)) return problemDetails.Detail;
            if(!string.IsNullOrWhiteSpace(problemDetails.Title)) return problemDetails.Title;
        }

        return string.IsNullOrWhiteSpace(exception.Message) ? "Unknown error" : exception.Message;
    }
}
