using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Marechai.App.Services;

/// <summary>
///     Service for fetching and managing PDAs from the Marechai API
/// </summary>
public class PdasService
{
    private readonly Client                _apiClient;
    private readonly ILogger<PdasService> _logger;

    public PdasService(Client apiClient, ILogger<PdasService> logger)
    {
        _apiClient = apiClient;
        _logger    = logger;
    }

    /// <summary>
    ///     Fetches the total count of PDAs from the API
    /// </summary>
    /// <returns>Total number of PDAs, or 0 if API call fails</returns>
    public async Task<int> GetPdasCountAsync()
    {
        try
        {
            _logger.LogInformation("Fetching PDAs count from API");
            int? result = await _apiClient.Pdas.Count.GetAsync();

            int count = result ?? 0;
            _logger.LogInformation("Successfully fetched PDAs count: {Count}", count);

            return count;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching PDAs count from API");

            return 0;
        }
    }

    /// <summary>
    ///     Fetches the minimum year of PDAs from the API
    /// </summary>
    /// <returns>Minimum year, or 0 if API call fails</returns>
    public async Task<int> GetMinimumYearAsync()
    {
        try
        {
            _logger.LogInformation("Fetching minimum year from API");
            int? result = await _apiClient.Pdas.MinimumYear.GetAsync();

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
    ///     Fetches the maximum year of PDAs from the API
    /// </summary>
    /// <returns>Maximum year, or 0 if API call fails</returns>
    public async Task<int> GetMaximumYearAsync()
    {
        try
        {
            _logger.LogInformation("Fetching maximum year from API");
            int? result = await _apiClient.Pdas.MaximumYear.GetAsync();

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
    ///     Fetches PDAs filtered by starting letter from the API
    /// </summary>
    public async Task<List<MachineDto>> GetPdasByLetterAsync(char letter)
    {
        try
        {
            _logger.LogInformation("Fetching PDAs starting with '{Letter}' from API", letter);

            List<MachineDto> pdas = await _apiClient.Pdas.ByLetter[letter.ToString()].GetAsync();

            if(pdas == null) return [];

            _logger.LogInformation("Successfully fetched {Count} PDAs starting with '{Letter}'",
                                   pdas.Count,
                                   letter);

            return pdas;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching PDAs by letter '{Letter}' from API", letter);

            return [];
        }
    }

    /// <summary>
    ///     Fetches PDAs filtered by year from the API
    /// </summary>
    public async Task<List<MachineDto>> GetPdasByYearAsync(int year)
    {
        try
        {
            _logger.LogInformation("Fetching PDAs from year {Year} from API", year);

            List<MachineDto> pdas = await _apiClient.Pdas.ByYear[year].GetAsync();

            if(pdas == null) return [];

            _logger.LogInformation("Successfully fetched {Count} PDAs from year {Year}", pdas.Count, year);

            return pdas;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching PDAs by year {Year} from API", year);

            return [];
        }
    }

    /// <summary>
    ///     Fetches all PDAs from the API
    /// </summary>
    public async Task<List<MachineDto>> GetAllPdasAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all PDAs from API");

            List<MachineDto> pdas = await _apiClient.Pdas.GetAsync();

            if(pdas == null) return [];

            _logger.LogInformation("Successfully fetched {Count} total PDAs", pdas.Count);

            return pdas;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching all PDAs from API");

            return [];
        }
    }

    /// <summary>
    ///     Fetches prototype PDAs from the API
    /// </summary>
    public async Task<List<MachineDto>> GetPrototypesAsync()
    {
        try
        {
            _logger.LogInformation("Fetching prototype PDAs from API");

            List<MachineDto> pdas = await _apiClient.Pdas.Prototypes.GetAsync();

            if(pdas == null) return [];

            _logger.LogInformation("Successfully fetched {Count} prototype PDAs", pdas.Count);

            return pdas;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching prototype PDAs from API");

            return [];
        }
    }

    /// <summary>
    ///     Fetches a single machine with full details by ID from the API
    /// </summary>
    public async Task<MachineDto> GetMachineByIdAsync(int machineId)
    {
        try
        {
            _logger.LogInformation("Fetching machine {MachineId} from API", machineId);

            MachineDto machine = await _apiClient.Machines[machineId].Full.GetAsync();

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
