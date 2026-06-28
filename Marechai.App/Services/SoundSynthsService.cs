#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Microsoft.Kiota.Abstractions;

namespace Marechai.App.Services;

/// <summary>
///     Service for fetching and managing Sound Synthesizers from the Marechai API
/// </summary>
public class SoundSynthsService
{
    private readonly Client                   _apiClient;
    private readonly ILogger<SoundSynthsService> _logger;

    public SoundSynthsService(Client apiClient, ILogger<SoundSynthsService> logger)
    {
        _apiClient = apiClient;
        _logger    = logger;
    }

    /// <summary>
    ///     Fetches all Sound Synthesizers from the API
    /// </summary>
    public async Task<List<SoundSynthDto>> GetAllSoundSynthsAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all Sound Synthesizers from API");

            List<SoundSynthDto>? soundSynths = await _apiClient.SoundSynths.GetAsync();

            if(soundSynths == null) return [];

            _logger.LogInformation("Successfully fetched {Count} total Sound Synthesizers", soundSynths.Count);

            return soundSynths;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching all Sound Synthesizers from API");

            return [];
        }
    }

    /// <summary>
    ///     Fetches a single Sound Synthesizer by ID from the API
    /// </summary>
    public async Task<SoundSynthDto?> GetSoundSynthByIdAsync(int soundSynthId)
    {
        try
        {
            _logger.LogInformation("Fetching Sound Synthesizer {SoundSynthId} from API", soundSynthId);

            SoundSynthDto? soundSynth = await _apiClient.SoundSynths[soundSynthId].GetAsync();

            if(soundSynth == null)
            {
                _logger.LogWarning("Sound Synthesizer {SoundSynthId} not found", soundSynthId);

                return null;
            }

            _logger.LogInformation("Successfully fetched Sound Synthesizer {SoundSynthId}: {SoundSynthName}",
                                   soundSynthId,
                                   soundSynth.Name);

            return soundSynth;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching Sound Synthesizer {SoundSynthId} from API", soundSynthId);

            return null;
        }
    }

    /// <summary>
    ///     Fetches photo IDs for a specific Sound Synthesizer
    /// </summary>
    public async Task<List<Guid>> GetSoundSynthPhotosAsync(int soundSynthId)
    {
        try
        {
            _logger.LogInformation("Fetching photos for Sound Synthesizer {SoundSynthId}", soundSynthId);

            List<Guid?>? guids = await _apiClient.SoundSynths[soundSynthId].Photos.GetAsync();

            return guids?.Where(g => g.HasValue).Select(g => g!.Value).ToList() ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching photos for Sound Synthesizer {SoundSynthId}", soundSynthId);

            return [];
        }
    }

    /// <summary>
    ///     Fetches machines that use a specific Sound Synthesizer
    /// </summary>
    public async Task<List<MachineDto>> GetMachinesBySoundSynthAsync(int soundSynthId)
    {
        try
        {
            _logger.LogInformation("Fetching machines for Sound Synthesizer {SoundSynthId}", soundSynthId);

            List<MachineDto>? machines = await _apiClient.SoundSynths[soundSynthId].Machines.GetAsync();

            _logger.LogInformation("Successfully fetched {Count} machines for Sound Synthesizer {SoundSynthId}",
                                   machines?.Count ?? 0,
                                   soundSynthId);

            return machines ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogWarning(ex, "Error fetching machines for Sound Synthesizer {SoundSynthId}", soundSynthId);

            return [];
        }
    }

    /// <summary>
    ///     Fetches full photo details for a Sound Synthesizer photo
    /// </summary>
    public async Task<SoundSynthPhotoDto?> GetSoundSynthPhotoDetailsAsync(Guid photoId)
    {
        try
        {
            _logger.LogInformation("Fetching Sound Synthesizer photo {PhotoId} from API", photoId);

            SoundSynthPhotoDto? photo = await _apiClient.SoundSynths.Photos[photoId].GetAsync();

            if(photo == null)
            {
                _logger.LogWarning("Sound Synthesizer photo {PhotoId} not found", photoId);

                return null;
            }

            return photo;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching Sound Synthesizer photo {PhotoId} from API", photoId);

            return null;
        }
    }

    /// <summary>
    ///     Fetches a localized description for a sound synthesizer
    /// </summary>
    public async Task<SoundSynthDescriptionDto?> GetDescriptionAsync(int soundSynthId, string languageCode)
    {
        try
        {
            _logger.LogInformation("Fetching description for sound synth {SoundSynthId} lang {Lang}", soundSynthId, languageCode);

            SoundSynthDescriptionDto? desc = await _apiClient.SoundSynths[soundSynthId].Description.GetAsync(
                config => config.QueryParameters.Lang = languageCode);

            return desc;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching description for sound synth {SoundSynthId}", soundSynthId);

            return null;
        }
    }

    /// <summary>
    ///     Fetches public videos for a sound synthesizer
    /// </summary>
    public async Task<List<SoundSynthVideoDto>> GetVideosBySoundSynthAsync(int soundSynthId)
    {
        try
        {
            _logger.LogInformation("Fetching videos for Sound Synthesizer {SoundSynthId}", soundSynthId);

            List<SoundSynthVideoDto>? videos = await _apiClient.SoundSynths[soundSynthId].Videos.GetAsync();

            if(videos == null) return [];

            _logger.LogInformation("Successfully fetched {Count} videos for Sound Synthesizer {SoundSynthId}",
                                   videos.Count,
                                   soundSynthId);

            return videos;
        }
        catch(Exception ex)
        {
            _logger.LogWarning(ex, "Error fetching videos for Sound Synthesizer {SoundSynthId}", soundSynthId);

            return [];
        }
    }

    /// <summary>
    ///     Creates a new video link for a sound synthesizer.
    /// </summary>
    public async Task<(SoundSynthVideoDto? dto, string? error)> CreateVideoAsync(int soundSynthId, string provider,
                                                                                  string videoId, string? title)
    {
        try
        {
            _logger.LogInformation("Creating video link for Sound Synthesizer {SoundSynthId}", soundSynthId);

            SoundSynthVideoDto? dto = await _apiClient.SoundSynths[soundSynthId]
                                                      .Videos
                                                      .PostAsync(new CreateSoundSynthVideoRequest
                                                       {
                                                           Provider = provider,
                                                           VideoId  = videoId,
                                                           Title    = title
                                                       });

            return (dto, null);
        }
        catch(ApiException ex)
        {
            _logger.LogWarning(ex, "API error creating sound synth video for {SoundSynthId}", soundSynthId);

            return (null, ExtractDetail(ex));
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error creating sound synth video for {SoundSynthId}", soundSynthId);

            return (null, ex.Message);
        }
    }

    /// <summary>
    ///     Updates a sound synthesizer video title.
    /// </summary>
    public async Task<(bool succeeded, string? error)> UpdateVideoTitleAsync(long id, string? title)
    {
        try
        {
            _logger.LogInformation("Updating sound synth video {VideoId}", id);

            await _apiClient.SoundSynths.Videos[id]
                            .PutAsync(new UpdateSoundSynthVideoRequest
                             {
                                 Title = title
                             });

            return (true, null);
        }
        catch(ApiException ex)
        {
            _logger.LogWarning(ex, "API error updating sound synth video {VideoId}", id);

            return (false, ExtractDetail(ex));
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error updating sound synth video {VideoId}", id);

            return (false, ex.Message);
        }
    }

    /// <summary>
    ///     Deletes a sound synthesizer video.
    /// </summary>
    public async Task<(bool succeeded, string? error)> DeleteVideoAsync(long id)
    {
        try
        {
            _logger.LogInformation("Deleting sound synth video {VideoId}", id);

            await _apiClient.SoundSynths.Videos[id].DeleteAsync();

            return (true, null);
        }
        catch(ApiException ex)
        {
            _logger.LogWarning(ex, "API error deleting sound synth video {VideoId}", id);

            return (false, ExtractDetail(ex));
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting sound synth video {VideoId}", id);

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
