using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Microsoft.Kiota.Abstractions;

namespace Marechai.App.Services;

/// <summary>
///     Service for fetching and managing Software videos from the Marechai API
/// </summary>
public class SoftwareVideosService
{
    private readonly Client                          _apiClient;
    private readonly ILogger<SoftwareVideosService> _logger;

    public SoftwareVideosService(Client apiClient, ILogger<SoftwareVideosService> logger)
    {
        _apiClient = apiClient;
        _logger    = logger;
    }

    /// <summary>
    ///     Fetches videos for a software entry
    /// </summary>
    public async Task<List<SoftwareVideoDto>> GetVideosBySoftwareAsync(int softwareId)
    {
        try
        {
            _logger.LogInformation("Fetching videos for software {SoftwareId}", softwareId);

            List<SoftwareVideoDto> videos = await _apiClient.Software[softwareId].Videos.GetAsync();

            if(videos == null) return [];

            _logger.LogInformation("Successfully fetched {Count} videos for software {SoftwareId}",
                                   videos.Count,
                                   softwareId);

            return videos;
        }
        catch(Exception ex)
        {
            _logger.LogWarning(ex, "Error fetching videos for software {SoftwareId}", softwareId);

            return [];
        }
    }

    /// <summary>
    ///     Creates a new video link for a software entry.
    /// </summary>
    public async Task<(SoftwareVideoDto? dto, string? error)> CreateVideoAsync(int    softwareId,
        string provider, string videoId, string? title)
    {
        try
        {
            _logger.LogInformation("Creating video link for software {SoftwareId}", softwareId);

            SoftwareVideoDto? dto = await _apiClient.Software[softwareId]
                                                     .Videos
                                                     .PostAsync(new CreateSoftwareVideoRequest
                                                      {
                                                          Provider = provider,
                                                          VideoId  = videoId,
                                                          Title    = title
                                                      });

            return (dto, null);
        }
        catch(ApiException ex)
        {
            _logger.LogWarning(ex, "API error creating software video for {SoftwareId}", softwareId);

            return (null, ExtractDetail(ex));
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error creating software video for {SoftwareId}", softwareId);

            return (null, ex.Message);
        }
    }

    /// <summary>
    ///     Updates a software video title.
    /// </summary>
    public async Task<(bool succeeded, string? error)> UpdateVideoTitleAsync(long id, string? title)
    {
        try
        {
            _logger.LogInformation("Updating software video {VideoId}", id);

            await _apiClient.Software.Videos[id]
                            .PutAsync(new UpdateSoftwareVideoRequest
                             {
                                 Title = title
                             });

            return (true, null);
        }
        catch(ApiException ex)
        {
            _logger.LogWarning(ex, "API error updating software video {VideoId}", id);

            return (false, ExtractDetail(ex));
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error updating software video {VideoId}", id);

            return (false, ex.Message);
        }
    }

    /// <summary>
    ///     Deletes a software video.
    /// </summary>
    public async Task<(bool succeeded, string? error)> DeleteVideoAsync(long id)
    {
        try
        {
            _logger.LogInformation("Deleting software video {VideoId}", id);

            await _apiClient.Software.Videos[id].DeleteAsync();

            return (true, null);
        }
        catch(ApiException ex)
        {
            _logger.LogWarning(ex, "API error deleting software video {VideoId}", id);

            return (false, ExtractDetail(ex));
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting software video {VideoId}", id);

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
