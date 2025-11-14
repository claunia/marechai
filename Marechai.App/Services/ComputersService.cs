using System;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions.Serialization;

namespace Marechai.App.Services;

/// <summary>
///     Service for fetching and managing computers from the Marechai API
/// </summary>
public class ComputersService
{
    private readonly ApiClient                 _apiClient;
    private readonly ILogger<ComputersService> _logger;

    public ComputersService(ApiClient apiClient, ILogger<ComputersService> logger)
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
            UntypedNode result = await _apiClient.Computers.Count.GetAsync();

            // Extract integer value from UntypedNode
            // UntypedNode wraps a JsonElement, we need to parse it
            int count = ExtractIntFromUntypedNode(result);
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
            UntypedNode result = await _apiClient.Computers.MinimumYear.GetAsync();

            // Extract integer value from UntypedNode
            int year = ExtractIntFromUntypedNode(result);
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
            UntypedNode result = await _apiClient.Computers.MaximumYear.GetAsync();

            // Extract integer value from UntypedNode
            int year = ExtractIntFromUntypedNode(result);
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
    private int ExtractIntFromUntypedNode(UntypedNode node)
    {
        if(node == null) return 0;

        try
        {
            // Cast to UntypedInteger to access the Value property
            if(node is UntypedInteger intNode) return intNode.GetValue();

            // Fallback: try to parse ToString() result
            var stringValue = node.ToString();

            if(!string.IsNullOrWhiteSpace(stringValue) && int.TryParse(stringValue, out int result)) return result;

            return 0;
        }
        catch
        {
            return 0;
        }
    }
}