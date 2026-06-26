#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;

namespace Marechai.App.Services;

/// <summary>
///     Service for global/advanced search against the Marechai API
/// </summary>
public class SearchService
{
    private readonly Client                  _apiClient;
    private readonly ILogger<SearchService>   _logger;

    public SearchService(Client apiClient, ILogger<SearchService> logger)
    {
        _apiClient = apiClient;
        _logger    = logger;
    }

    /// <summary>
    ///     Fetches autocomplete suggestions for a query, optionally restricted to a single entity type
    /// </summary>
    public async Task<List<SearchResultDto>> AutocompleteAsync(string query, int? entityType = null, int take = 8)
    {
        try
        {
            _logger.LogInformation("Fetching search autocomplete for '{Query}'", query);

            List<SearchResultDto>? results = await _apiClient.Search.Autocomplete.GetAsync(config =>
            {
                config.QueryParameters.Q          = query;
                config.QueryParameters.EntityType = entityType;
                config.QueryParameters.Take       = take;
            });

            return results ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching search autocomplete for '{Query}'", query);

            return [];
        }
    }

    /// <summary>
    ///     Runs an advanced search and returns the windowed results plus per-type totals
    /// </summary>
    public async Task<SearchResultsPageDto?> SearchAsync(SearchRequestDto request)
    {
        try
        {
            _logger.LogInformation("Running advanced search");

            return await _apiClient.Search.Results.PostAsync(request);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error running advanced search");

            return null;
        }
    }
}
