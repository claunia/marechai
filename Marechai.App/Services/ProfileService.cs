#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;

namespace Marechai.App.Services;

public class ProfileService
{
    private readonly Client                  _apiClient;
    private readonly ILogger<ProfileService> _logger;

    public ProfileService(Client apiClient, ILogger<ProfileService> logger)
    {
        _apiClient = apiClient;
        _logger    = logger;
    }

    public async Task<PublicProfileDto?> GetPublicProfileAsync(string username)
    {
        try
        {
            return await _apiClient.Profile[username].GetAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching public profile for {Username}", username);

            return null;
        }
    }

    public async Task<List<SoftwareUserReviewDto>> GetUserReviewsAsync(string username)
    {
        try
        {
            List<SoftwareUserReviewDto>? reviews = await _apiClient.Profile[username].Reviews.GetAsync();

            return reviews ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching reviews for {Username}", username);

            return [];
        }
    }

    public async Task<UserCollectionSummaryDto?> GetCollectionSummaryAsync(string username)
    {
        try
        {
            return await _apiClient.Profile[username].Collection.GetAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching collection summary for {Username}", username);

            return null;
        }
    }

    public async Task<List<CollectedBookDto>> GetCollectedBooksAsync(string username)
    {
        try
        {
            List<CollectedBookDto>? books = await _apiClient.Profile[username].Collection.Books.GetAsync();

            return books ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching collected books for {Username}", username);

            return [];
        }
    }

    public async Task<List<CollectedDocumentDto>> GetCollectedDocumentsAsync(string username)
    {
        try
        {
            List<CollectedDocumentDto>? documents = await _apiClient.Profile[username].Collection.Documents.GetAsync();

            return documents ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching collected documents for {Username}", username);

            return [];
        }
    }

    public async Task<List<CollectedMachineDto>> GetCollectedMachinesAsync(string username)
    {
        try
        {
            List<CollectedMachineDto>? machines = await _apiClient.Profile[username].Collection.Machines.GetAsync();

            return machines ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching collected machines for {Username}", username);

            return [];
        }
    }

    public async Task<List<CollectedSoftwareReleaseDto>> GetCollectedSoftwareReleasesAsync(string username)
    {
        try
        {
            List<CollectedSoftwareReleaseDto>? releases =
                await _apiClient.Profile[username].Collection.SoftwareReleases.GetAsync();

            return releases ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching collected software releases for {Username}", username);

            return [];
        }
    }

    public async Task<List<CollectedMagazineIssueDto>> GetCollectedMagazineIssuesAsync(string username)
    {
        try
        {
            List<CollectedMagazineIssueDto>? issues =
                await _apiClient.Profile[username].Collection.MagazineIssues.GetAsync();

            return issues ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error fetching collected magazine issues for {Username}", username);

            return [];
        }
    }
}
