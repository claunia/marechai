#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Microsoft.Kiota.Abstractions;

namespace Marechai.App.Services;

/// <summary>
///     Service for managing Machine videos from the Marechai API
/// </summary>
public class MachinesService
{
    private readonly Client                  _apiClient;
    private readonly ILogger<MachinesService> _logger;

    public MachinesService(Client apiClient, ILogger<MachinesService> logger)
    {
        _apiClient = apiClient;
        _logger    = logger;
    }

    /// <summary>
    ///     Fetches a single Machine by ID from the API
    /// </summary>
    public async Task<MachineDto?> GetMachineByIdAsync(int machineId)
    {
        try
        {
            _logger.LogInformation("Fetching Machine {MachineId} from API", machineId);

            MachineDto? machine = await _apiClient.Machines[machineId].GetAsync();

            if(machine == null)
            {
                _logger.LogWarning("Machine {MachineId} not found", machineId);

                return null;
            }

            return machine;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching Machine {MachineId} from API", machineId);

            return null;
        }
    }

    /// <summary>
    ///     Fetches videos for a specific Machine
    /// </summary>
    public async Task<List<MachineVideoDto>> GetVideosByMachineAsync(int machineId)
    {
        try
        {
            _logger.LogInformation("Fetching videos for Machine {MachineId}", machineId);

            List<MachineVideoDto>? videos = await _apiClient.Machines[machineId].Videos.GetAsync();

            if(videos == null) return [];

            _logger.LogInformation("Successfully fetched {Count} videos for Machine {MachineId}",
                                   videos.Count,
                                   machineId);

            return videos;
        }
        catch(Exception ex)
        {
            _logger.LogWarning(ex, "Error fetching videos for Machine {MachineId}", machineId);

            return [];
        }
    }

    /// <summary>
    ///     Creates a new video link for a Machine.
    /// </summary>
    public async Task<(MachineVideoDto? dto, string? error)> CreateVideoAsync(int machineId, string provider,
                                                                                string videoId, string? title)
    {
        try
        {
            _logger.LogInformation("Creating video link for Machine {MachineId}", machineId);

            MachineVideoDto? dto = await _apiClient.Machines[machineId]
                                                   .Videos
                                                   .PostAsync(new CreateMachineVideoRequest
                                                    {
                                                        Provider = provider,
                                                        VideoId  = videoId,
                                                        Title    = title
                                                    });

            return (dto, null);
        }
        catch(ApiException ex)
        {
            _logger.LogWarning(ex, "API error creating machine video for {MachineId}", machineId);

            return (null, ExtractDetail(ex));
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error creating machine video for {MachineId}", machineId);

            return (null, ex.Message);
        }
    }

    /// <summary>
    ///     Updates a Machine video title.
    /// </summary>
    public async Task<(bool succeeded, string? error)> UpdateVideoTitleAsync(long id, string? title)
    {
        try
        {
            _logger.LogInformation("Updating machine video {VideoId}", id);

            await _apiClient.Machines.Videos[id]
                            .PutAsync(new UpdateMachineVideoRequest
                             {
                                 Title = title
                             });

            return (true, null);
        }
        catch(ApiException ex)
        {
            _logger.LogWarning(ex, "API error updating machine video {VideoId}", id);

            return (false, ExtractDetail(ex));
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error updating machine video {VideoId}", id);

            return (false, ex.Message);
        }
    }

    /// <summary>
    ///     Deletes a Machine video.
    /// </summary>
    public async Task<(bool succeeded, string? error)> DeleteVideoAsync(long id)
    {
        try
        {
            _logger.LogInformation("Deleting machine video {VideoId}", id);

            await _apiClient.Machines.Videos[id].DeleteAsync();

            return (true, null);
        }
        catch(ApiException ex)
        {
            _logger.LogWarning(ex, "API error deleting machine video {VideoId}", id);

            return (false, ExtractDetail(ex));
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting machine video {VideoId}", id);

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
