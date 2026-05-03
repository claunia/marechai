using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Marechai.App.Services;

/// <summary>
///     Service for fetching and managing smartphones from the Marechai API
/// </summary>
public class SmartphonesService
{
    private readonly Client                    _apiClient;
    private readonly ILogger<SmartphonesService> _logger;

    public SmartphonesService(Client apiClient, ILogger<SmartphonesService> logger)
    {
        _apiClient = apiClient;
        _logger    = logger;
    }

    /// <summary>
    ///     Fetches the total count of smartphones from the API
    /// </summary>
    /// <returns>Total number of smartphones, or 0 if API call fails</returns>
    public async Task<int> GetSmartphonesCountAsync()
    {
        try
        {
            _logger.LogInformation("Fetching smartphones count from API");
            int? result = await _apiClient.Smartphones.Count.GetAsync();

            int count = result ?? 0;
            _logger.LogInformation("Successfully fetched smartphones count: {Count}", count);

            return count;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching smartphones count from API");

            return 0;
        }
    }

    /// <summary>
    ///     Fetches the minimum year of smartphones from the API
    /// </summary>
    /// <returns>Minimum year, or 0 if API call fails</returns>
    public async Task<int> GetMinimumYearAsync()
    {
        try
        {
            _logger.LogInformation("Fetching minimum year from API");
            int? result = await _apiClient.Smartphones.MinimumYear.GetAsync();

            int year = result ?? 0;
            _logger.LogInformation("Successfully fetched minimum year: {Year}", year);

            return year;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching minimum year from API");

            return 0;
        }
    }

    /// <summary>
    ///     Fetches the maximum year of smartphones from the API
    /// </summary>
    /// <returns>Maximum year, or 0 if API call fails</returns>
    public async Task<int> GetMaximumYearAsync()
    {
        try
        {
            _logger.LogInformation("Fetching maximum year from API");
            int? result = await _apiClient.Smartphones.MaximumYear.GetAsync();

            int year = result ?? 0;
            _logger.LogInformation("Successfully fetched maximum year: {Year}", year);

            return year;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching maximum year from API");

            return 0;
        }
    }

    /// <summary>
    ///     Fetches smartphones filtered by starting letter from the API
    /// </summary>
    public async Task<List<MachineDto>> GetSmartphonesByLetterAsync(char letter)
    {
        try
        {
            _logger.LogInformation("Fetching smartphones starting with '{Letter}' from API", letter);

            List<MachineDto> smartphones = await _apiClient.Smartphones.ByLetter[letter.ToString()].GetAsync();

            if(smartphones == null) return [];

            _logger.LogInformation("Successfully fetched {Count} smartphones starting with '{Letter}'",
                                   smartphones.Count,
                                   letter);

            return smartphones;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching smartphones by letter '{Letter}' from API", letter);

            return [];
        }
    }

    /// <summary>
    ///     Fetches smartphones filtered by year from the API
    /// </summary>
    public async Task<List<MachineDto>> GetSmartphonesByYearAsync(int year)
    {
        try
        {
            _logger.LogInformation("Fetching smartphones from year {Year} from API", year);

            List<MachineDto> smartphones = await _apiClient.Smartphones.ByYear[year].GetAsync();

            if(smartphones == null) return [];

            _logger.LogInformation("Successfully fetched {Count} smartphones from year {Year}", smartphones.Count, year);

            return smartphones;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching smartphones by year {Year} from API", year);

            return [];
        }
    }

    /// <summary>
    ///     Fetches all smartphones from the API
    /// </summary>
    public async Task<List<MachineDto>> GetAllSmartphonesAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all smartphones from API");

            List<MachineDto> smartphones = await _apiClient.Smartphones.GetAsync();

            if(smartphones == null) return [];

            _logger.LogInformation("Successfully fetched {Count} total smartphones", smartphones.Count);

            return smartphones;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching all smartphones from API");

            return [];
        }
    }

    /// <summary>
    ///     Fetches prototype smartphones from the API
    /// </summary>
    public async Task<List<MachineDto>> GetPrototypesAsync()
    {
        try
        {
            _logger.LogInformation("Fetching prototype smartphones from API");

            List<MachineDto> smartphones = await _apiClient.Smartphones.Prototypes.GetAsync();

            if(smartphones == null) return [];

            _logger.LogInformation("Successfully fetched {Count} prototype smartphones", smartphones.Count);

            return smartphones;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching prototype smartphones from API");

            return [];
        }
    }

    /// <summary>
    ///     Fetches a single machine with full details by ID from the API
    /// </summary>
    public async Task<MachineDto?> GetMachineByIdAsync(int machineId)
    {
        try
        {
            _logger.LogInformation("Fetching machine {MachineId} from API", machineId);

            MachineDto? machine = await _apiClient.Machines[machineId].Full.GetAsync();

            if(machine == null)
            {
                _logger.LogWarning("Machine {MachineId} not found", machineId);

                return null;
            }

            _logger.LogInformation("Successfully fetched machine {MachineId}: {MachineName}", machineId, machine.Name);

            return machine;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching machine {MachineId} from API", machineId);

            return null;
        }
    }
}
