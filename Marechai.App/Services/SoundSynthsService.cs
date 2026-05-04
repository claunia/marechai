#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
}