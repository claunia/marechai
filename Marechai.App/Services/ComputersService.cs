using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Marechai.App.Services;

/// <summary>
///     Service for fetching and managing computers from the Marechai API
/// </summary>
public class ComputersService
{
    private readonly Client                 _apiClient;
    private readonly ILogger<ComputersService> _logger;

    public ComputersService(Client apiClient, ILogger<ComputersService> logger)
    {
        _apiClient = apiClient;
        _logger    = logger;
    }

    /// <summary>
    ///     Fetches the total count of computers from the API
    /// </summary>
    /// <returns>Total number of computers, or 0 if API call fails</returns>
    public async Task<int> GetComputersCountAsync()
    {
        try
        {
            _logger.LogInformation("Fetching computers count from API");
            int? result = await _apiClient.Computers.Count.GetAsync();

            int count = result ?? 0;
            _logger.LogInformation("Successfully fetched computers count: {Count}", count);

            return count;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching computers count from API");

            return 0;
        }
    }

    /// <summary>
    ///     Fetches the minimum year of computers from the API
    /// </summary>
    /// <returns>Minimum year, or 0 if API call fails</returns>
    public async Task<int> GetMinimumYearAsync()
    {
        try
        {
            _logger.LogInformation("Fetching minimum year from API");
            int? result = await _apiClient.Computers.MinimumYear.GetAsync();

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
    ///     Fetches the maximum year of computers from the API
    /// </summary>
    /// <returns>Maximum year, or 0 if API call fails</returns>
    public async Task<int> GetMaximumYearAsync()
    {
        try
        {
            _logger.LogInformation("Fetching maximum year from API");
            int? result = await _apiClient.Computers.MaximumYear.GetAsync();

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
    ///     Helper method to extract an integer from an UntypedNode
    /// </summary>
    /// <summary>
    ///     Fetches computers filtered by starting letter from the API
    /// </summary>
    public async Task<List<MachineDto>> GetComputersByLetterAsync(char letter)
    {
        try
        {
            _logger.LogInformation("Fetching computers starting with '{Letter}' from API", letter);

            List<MachineDto> computers = await _apiClient.Computers.ByLetter[letter.ToString()].GetAsync();

            if(computers == null) return [];

            _logger.LogInformation("Successfully fetched {Count} computers starting with '{Letter}'",
                                   computers.Count,
                                   letter);

            return computers;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching computers by letter '{Letter}' from API", letter);

            return [];
        }
    }

    /// <summary>
    ///     Fetches computers filtered by year from the API
    /// </summary>
    public async Task<List<MachineDto>> GetComputersByYearAsync(int year)
    {
        try
        {
            _logger.LogInformation("Fetching computers from year {Year} from API", year);

            List<MachineDto> computers = await _apiClient.Computers.ByYear[year].GetAsync();

            if(computers == null) return [];

            _logger.LogInformation("Successfully fetched {Count} computers from year {Year}", computers.Count, year);

            return computers;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching computers by year {Year} from API", year);

            return [];
        }
    }

    /// <summary>
    ///     Fetches all computers from the API
    /// </summary>
    public async Task<List<MachineDto>> GetAllComputersAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all computers from API");

            List<MachineDto> computers = await _apiClient.Computers.GetAsync();

            if(computers == null) return [];

            _logger.LogInformation("Successfully fetched {Count} total computers", computers.Count);

            return computers;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching all computers from API");

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

    /// <summary>
    ///     Fetches the list of photo GUIDs for a machine from the API
    /// </summary>
    public async Task<List<Guid>> GetMachinePhotosAsync(int machineId)
    {
        try
        {
            _logger.LogInformation("Fetching photos for machine {MachineId} from API", machineId);

            List<Guid?>? photos = await _apiClient.Machines[machineId].Photos.GetAsync();

            if(photos == null || photos.Count == 0)
            {
                _logger.LogInformation("No photos found for machine {MachineId}", machineId);

                return [];
            }

            // Filter out null values
            var validPhotos = photos.Where(p => p.HasValue).Select(p => p!.Value).ToList();

            _logger.LogInformation("Successfully fetched {Count} photos for machine {MachineId}",
                                   validPhotos.Count,
                                   machineId);

            return validPhotos;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching photos for machine {MachineId} from API", machineId);

            return [];
        }
    }

    /// <summary>
    ///     Fetches detailed information for a specific photo from the API
    /// </summary>
    public async Task<MachinePhotoDto?> GetMachinePhotoDetailsAsync(Guid photoId)
    {
        try
        {
            _logger.LogInformation("Fetching photo details for {PhotoId} from API", photoId);

            MachinePhotoDto? photo = await _apiClient.Machines.Photos[photoId].GetAsync();

            if(photo == null)
            {
                _logger.LogWarning("Photo {PhotoId} not found", photoId);

                return null;
            }

            _logger.LogInformation("Successfully fetched photo details {PhotoId}", photoId);

            return photo;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching photo details for {PhotoId} from API", photoId);

            return null;
        }
    }

    /// <summary>
    ///     Fetches all software available for a machine's supported platforms
    /// </summary>
    public async Task<List<SoftwareDto>> GetSoftwareByMachineAsync(int machineId)
    {
        try
        {
            _logger.LogInformation("Fetching software for machine {MachineId} from API", machineId);

            List<SoftwareDto>? software = await _apiClient.Machines[machineId].Software.GetAsync();

            return software ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching software for machine {MachineId} from API", machineId);

            return [];
        }
    }

    /// <summary>
    ///     Fetches a localized description for a machine
    /// </summary>
    public async Task<MachineDescriptionDto?> GetDescriptionAsync(int machineId, string languageCode)
    {
        try
        {
            _logger.LogInformation("Fetching description for machine {MachineId} lang {Lang}", machineId, languageCode);

            MachineDescriptionDto? desc = await _apiClient.Machines[machineId].Description.GetAsync(
                config => config.QueryParameters.Lang = languageCode);

            return desc;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching description for machine {MachineId}", machineId);

            return null;
        }
    }
}