#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Marechai.App.Services;

/// <summary>
///     Service for fetching and managing Processors from the Marechai API
/// </summary>
public class ProcessorsService
{
    private readonly ApiClient                  _apiClient;
    private readonly ILogger<ProcessorsService> _logger;

    public ProcessorsService(ApiClient apiClient, ILogger<ProcessorsService> logger)
    {
        _apiClient = apiClient;
        _logger    = logger;
    }

    /// <summary>
    ///     Fetches all Processors from the API
    /// </summary>
    public async Task<List<ProcessorDto>> GetAllProcessorsAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all Processors from API");

            List<ProcessorDto>? processors = await _apiClient.Processors.GetAsync();

            if(processors == null) return [];

            _logger.LogInformation("Successfully fetched {Count} total Processors", processors.Count);

            return processors;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching all Processors from API");

            return [];
        }
    }

    /// <summary>
    ///     Fetches a single Processor by ID from the API
    /// </summary>
    public async Task<ProcessorDto?> GetProcessorByIdAsync(int processorId)
    {
        try
        {
            _logger.LogInformation("Fetching Processor {ProcessorId} from API", processorId);

            ProcessorDto? processor = await _apiClient.Processors[processorId].GetAsync();

            if(processor == null)
            {
                _logger.LogWarning("Processor {ProcessorId} not found", processorId);

                return null;
            }

            _logger.LogInformation("Successfully fetched Processor {ProcessorId}: {ProcessorName}",
                                   processorId,
                                   processor.Name);

            return processor;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching Processor {ProcessorId} from API", processorId);

            return null;
        }
    }

    /// <summary>
    ///     Fetches machines that use a specific Processor
    /// </summary>
    public async Task<List<MachineDto>> GetMachinesByProcessorAsync(int processorId)
    {
        try
        {
            _logger.LogInformation("Fetching machines for Processor {ProcessorId}", processorId);

            // Fetch from the processors-by-machine/by-processor/{processorId} endpoint
            List<ProcessorByMachineDto>? processorMachineRelationships =
                await _apiClient.ProcessorsByMachine.ByProcessor[processorId].GetAsync();

            if(processorMachineRelationships == null || processorMachineRelationships.Count == 0) return [];

            // Fetch full machine details for each to get Type information
            var machines = new List<MachineDto>();

            foreach(ProcessorByMachineDto pm in processorMachineRelationships)
            {
                if(pm.MachineId.HasValue)
                {
                    try
                    {
                        MachineDto? machine = await _apiClient.Machines[pm.MachineId.Value].GetAsync();
                        if(machine != null) machines.Add(machine);
                    }
                    catch(Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to fetch machine {MachineId}", pm.MachineId);
                    }
                }
            }

            _logger.LogInformation("Successfully fetched {Count} machines for Processor {ProcessorId}",
                                   machines.Count,
                                   processorId);

            return machines;
        }
        catch(Exception ex)
        {
            _logger.LogWarning(ex, "Error fetching machines for Processor {ProcessorId}", processorId);

            return [];
        }
    }
}