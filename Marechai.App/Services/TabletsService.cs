using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Marechai.App.Services;

/// <summary>
///     Service for fetching and managing Tablets from the Marechai API
/// </summary>
public class TabletsService
{
    private readonly Client                   _apiClient;
    private readonly ILogger<TabletsService> _logger;

    public TabletsService(Client apiClient, ILogger<TabletsService> logger)
    {
        _apiClient = apiClient;
        _logger    = logger;
    }

    /// <summary>
    ///     Fetches the total count of Tablets from the API
    /// </summary>
    /// <returns>Total number of Tablets, or 0 if API call fails</returns>
    public async Task<int> GetTabletsCountAsync()
    {
        try
        {
            _logger.LogInformation("Fetching Tablets count from API");
            int? result = await _apiClient.Tablets.Count.GetAsync();

            int count = result ?? 0;
            _logger.LogInformation("Successfully fetched Tablets count: {Count}", count);

            return count;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching Tablets count from API");

            return 0;
        }
    }

    /// <summary>
    ///     Fetches the minimum year of Tablets from the API
    /// </summary>
    /// <returns>Minimum year, or 0 if API call fails</returns>
    public async Task<int> GetMinimumYearAsync()
    {
        try
        {
            _logger.LogInformation("Fetching minimum year from API");
            int? result = await _apiClient.Tablets.MinimumYear.GetAsync();

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
    ///     Fetches the maximum year of Tablets from the API
    /// </summary>
    /// <returns>Maximum year, or 0 if API call fails</returns>
    public async Task<int> GetMaximumYearAsync()
    {
        try
        {
            _logger.LogInformation("Fetching maximum year from API");
            int? result = await _apiClient.Tablets.MaximumYear.GetAsync();

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
    ///     Fetches Tablets filtered by starting letter from the API
    /// </summary>
    public async Task<List<MachineDto>> GetTabletsByLetterAsync(char letter)
    {
        try
        {
            _logger.LogInformation("Fetching Tablets starting with '{Letter}' from API", letter);

            List<MachineDto> tablets = await _apiClient.Tablets.ByLetter[letter.ToString()].GetAsync();

            if(tablets == null) return [];

            _logger.LogInformation("Successfully fetched {Count} Tablets starting with '{Letter}'",
                                   tablets.Count,
                                   letter);

            return tablets;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching Tablets by letter '{Letter}' from API", letter);

            return [];
        }
    }

    /// <summary>
    ///     Fetches Tablets filtered by year from the API
    /// </summary>
    public async Task<List<MachineDto>> GetTabletsByYearAsync(int year)
    {
        try
        {
            _logger.LogInformation("Fetching Tablets from year {Year} from API", year);

            List<MachineDto> tablets = await _apiClient.Tablets.ByYear[year].GetAsync();

            if(tablets == null) return [];

            _logger.LogInformation("Successfully fetched {Count} Tablets from year {Year}", tablets.Count, year);

            return tablets;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching Tablets by year {Year} from API", year);

            return [];
        }
    }

    /// <summary>
    ///     Fetches all Tablets from the API
    /// </summary>
    public async Task<List<MachineDto>> GetAllTabletsAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all Tablets from API");

            List<MachineDto> tablets = await _apiClient.Tablets.GetAsync();

            if(tablets == null) return [];

            _logger.LogInformation("Successfully fetched {Count} total Tablets", tablets.Count);

            return tablets;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching all Tablets from API");

            return [];
        }
    }

    /// <summary>
    ///     Fetches prototype Tablets from the API
    /// </summary>
    public async Task<List<MachineDto>> GetPrototypesAsync()
    {
        try
        {
            _logger.LogInformation("Fetching prototype Tablets from API");

            List<MachineDto> tablets = await _apiClient.Tablets.Prototypes.GetAsync();

            if(tablets == null) return [];

            _logger.LogInformation("Successfully fetched {Count} prototype Tablets", tablets.Count);

            return tablets;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching prototype Tablets from API");

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
