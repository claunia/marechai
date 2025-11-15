using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Marechai.App.Services;

/// <summary>
///     Service for fetching and managing news from the Marechai API
/// </summary>
public class NewsService
{
    private readonly ApiClient            _apiClient;
    private readonly ILogger<NewsService> _logger;

    public NewsService(ApiClient apiClient, ILogger<NewsService> logger)
    {
        _apiClient = apiClient;
        _logger    = logger;
    }

    /// <summary>
    ///     Fetches the latest news from the API
    /// </summary>
    /// <returns>List of latest news items, or empty list if API call fails</returns>
    public async Task<List<NewsDto>> GetLatestNewsAsync()
    {
        try
        {
            _logger.LogInformation("Fetching latest news from API");
            List<NewsDto> news = await _apiClient.News.Latest.GetAsync();
            _logger.LogInformation("Successfully fetched {Count} news items", news?.Count ?? 0);

            return news ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching latest news from API");

            return [];
        }
    }
}