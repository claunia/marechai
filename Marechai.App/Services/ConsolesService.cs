using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Marechai.App.Services;

/// <summary>
///     Service for fetching and managing consoles from the Marechai API
/// </summary>
public class ConsolesService
{
    private readonly Client                _apiClient;
    private readonly ILogger<ConsolesService> _logger;

    public ConsolesService(Client apiClient, ILogger<ConsolesService> logger)
    {
        _apiClient = apiClient;
        _logger    = logger;
    }

    /// <summary>
    ///     Fetches the total count of consoles from the API
    /// </summary>
    /// <returns>Total number of consoles, or 0 if API call fails</returns>
    public async Task<int> GetConsolesCountAsync()
    {
        try
        {
            _logger.LogInformation("Fetching consoles count from API");
            int? result = await _apiClient.Consoles.Count.GetAsync();

            int count = result ?? 0;
            _logger.LogInformation("Successfully fetched consoles count: {Count}", count);

            return count;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching consoles count from API");

            return 0;
        }
    }

    /// <summary>
    ///     Fetches the minimum year of consoles from the API
    /// </summary>
    /// <returns>Minimum year, or 0 if API call fails</returns>
    public async Task<int> GetMinimumYearAsync()
    {
        try
        {
            _logger.LogInformation("Fetching minimum year from API");
            int? result = await _apiClient.Consoles.MinimumYear.GetAsync();

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
    ///     Fetches the maximum year of consoles from the API
    /// </summary>
    /// <returns>Maximum year, or 0 if API call fails</returns>
    public async Task<int> GetMaximumYearAsync()
    {
        try
        {
            _logger.LogInformation("Fetching maximum year from API");
            int? result = await _apiClient.Consoles.MaximumYear.GetAsync();

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
    ///     Fetches consoles filtered by starting letter from the API
    /// </summary>
    public async Task<List<MachineDto>> GetConsolesByLetterAsync(char letter)
    {
        try
        {
            _logger.LogInformation("Fetching consoles starting with '{Letter}' from API", letter);

            List<MachineDto> consoles = await _apiClient.Consoles.ByLetter[letter.ToString()].GetAsync();

            if(consoles == null) return [];

            _logger.LogInformation("Successfully fetched {Count} consoles starting with '{Letter}'",
                                   consoles.Count,
                                   letter);

            return consoles;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching consoles by letter '{Letter}' from API", letter);

            return [];
        }
    }

    /// <summary>
    ///     Fetches consoles filtered by year from the API
    /// </summary>
    public async Task<List<MachineDto>> GetConsolesByYearAsync(int year)
    {
        try
        {
            _logger.LogInformation("Fetching consoles from year {Year} from API", year);

            List<MachineDto> consoles = await _apiClient.Consoles.ByYear[year].GetAsync();

            if(consoles == null) return [];

            _logger.LogInformation("Successfully fetched {Count} consoles from year {Year}", consoles.Count, year);

            return consoles;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching consoles by year {Year} from API", year);

            return [];
        }
    }

    /// <summary>
    ///     Fetches all consoles from the API
    /// </summary>
    public async Task<List<MachineDto>> GetAllConsolesAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all consoles from API");

            List<MachineDto> consoles = await _apiClient.Consoles.GetAsync();

            if(consoles == null) return [];

            _logger.LogInformation("Successfully fetched {Count} total consoles", consoles.Count);

            return consoles;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching all consoles from API");

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