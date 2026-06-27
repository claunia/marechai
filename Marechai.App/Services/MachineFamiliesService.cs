using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Marechai.App.Services;

/// <summary>
///     Service for fetching machine families from the Marechai API
/// </summary>
public class MachineFamiliesService
{
    private readonly Client                       _apiClient;
    private readonly ILogger<MachineFamiliesService> _logger;

    public MachineFamiliesService(Client apiClient, ILogger<MachineFamiliesService> logger)
    {
        _apiClient = apiClient;
        _logger    = logger;
    }

    /// <summary>
    ///     Fetches a machine family by ID from the API
    /// </summary>
    public async Task<MachineFamilyDto> GetByIdAsync(int familyId)
    {
        try
        {
            _logger.LogInformation("Fetching machine family {FamilyId} from API", familyId);

            MachineFamilyDto family = await _apiClient.MachineFamilies[familyId].GetAsync();

            if(family == null)
            {
                _logger.LogWarning("Machine family {FamilyId} not found", familyId);

                return null;
            }

            _logger.LogInformation("Successfully fetched machine family {FamilyId}: {FamilyName}",
                                   familyId,
                                   family.Name);

            return family;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching machine family {FamilyId} from API", familyId);

            return null;
        }
    }

    /// <summary>
    ///     Fetches the machines belonging to a machine family from the API
    /// </summary>
    public async Task<List<MachineDto>> GetMachinesAsync(int familyId)
    {
        try
        {
            _logger.LogInformation("Fetching machines for family {FamilyId} from API", familyId);

            List<MachineDto> machines = await _apiClient.MachineFamilies[familyId].Machines.GetAsync();

            if(machines == null) return [];

            _logger.LogInformation("Successfully fetched {Count} machines for family {FamilyId}",
                                   machines.Count,
                                   familyId);

            return machines;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching machines for family {FamilyId} from API", familyId);

            return [];
        }
    }
}
