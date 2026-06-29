#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Microsoft.Kiota.Abstractions;

namespace Marechai.App.Services;

public class SoftwareAttributesService
{
    readonly Client                             _apiClient;
    readonly ILogger<SoftwareAttributesService> _logger;

    public SoftwareAttributesService(Client apiClient, ILogger<SoftwareAttributesService> logger)
    {
        _apiClient = apiClient;
        _logger    = logger;
    }

    public async Task<SoftwareAttributePageDto?> GetPagedAsync(int? softwareId, int? releaseId, string? category,
                                                               string? key, int page, int pageSize)
    {
        try
        {
            return await _apiClient.Software.Attributes.GetAsync(config =>
            {
                config.QueryParameters.SoftwareId = softwareId;
                config.QueryParameters.ReleaseId  = releaseId;
                config.QueryParameters.Category   = category;
                config.QueryParameters.Key        = key;
                config.QueryParameters.Page       = page;
                config.QueryParameters.PageSize   = pageSize;
            });
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading software attributes page {Page}", page);

            return new SoftwareAttributePageDto { Items = [], TotalCount = 0 };
        }
    }

    public async Task<List<string>> GetDistinctCategoriesAsync()
    {
        try
        {
            List<string>? items = await _apiClient.Software.Attributes.DistinctCategories.GetAsync();

            return items ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading software attribute categories");

            return [];
        }
    }

    public async Task<List<string>> GetDistinctKeysAsync(string? category)
    {
        try
        {
            List<string>? items = await _apiClient.Software.Attributes.DistinctKeys.GetAsync(config =>
            {
                config.QueryParameters.Category = category;
            });

            return items ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading software attribute keys for category {Category}", category);

            return [];
        }
    }

    public async Task<List<string>> GetDistinctValuesAsync(string? category, string? key)
    {
        try
        {
            List<string>? items = await _apiClient.Software.Attributes.DistinctValues.GetAsync(config =>
            {
                config.QueryParameters.Category = category;
                config.QueryParameters.Key      = key;
            });

            return items ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading software attribute values for {Category}/{Key}", category, key);

            return [];
        }
    }

    public async Task<List<SoftwareReleaseLookupDto>> LookupReleasesAsync(int softwareId)
    {
        try
        {
            List<SoftwareReleaseLookupDto>? items = await _apiClient.Software.Attributes.LookupReleases.GetAsync(
                config => config.QueryParameters.SoftwareId = softwareId);

            return items ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error loading releases for software {SoftwareId}", softwareId);

            return [];
        }
    }

    public async Task<(long? id, string? error)> CreateAsync(CreateSoftwareAttributeRequest request)
    {
        try
        {
            long? id = await _apiClient.Software.Attributes.PostAsync(request);

            return (id, null);
        }
        catch(ApiException ex)
        {
            _logger.LogWarning(ex, "API error creating software attribute");

            return (null, ExtractDetail(ex));
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error creating software attribute");

            return (null, ex.Message);
        }
    }

    public async Task<(bool succeeded, string? error)> UpdateAsync(long id, UpdateSoftwareAttributeRequest request)
    {
        try
        {
            await _apiClient.Software.Attributes[id].PutAsync(request);

            return (true, null);
        }
        catch(ApiException ex)
        {
            _logger.LogWarning(ex, "API error updating software attribute {AttributeId}", id);

            return (false, ExtractDetail(ex));
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error updating software attribute {AttributeId}", id);

            return (false, ex.Message);
        }
    }

    public async Task<(bool succeeded, string? error)> DeleteAsync(long id)
    {
        try
        {
            await _apiClient.Software.Attributes[id].DeleteAsync();

            return (true, null);
        }
        catch(ApiException ex)
        {
            _logger.LogWarning(ex, "API error deleting software attribute {AttributeId}", id);

            return (false, ExtractDetail(ex));
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting software attribute {AttributeId}", id);

            return (false, ex.Message);
        }
    }

    public async Task<(SplitSoftwareAttributeResultDto? result, string? error)> PreviewSplitAsync(long id,
        string separator)
    {
        try
        {
            SplitSoftwareAttributeResultDto? result = await _apiClient.Software.Attributes[id].SplitPreview.PostAsync(
                new SplitSoftwareAttributeRequest { Separator = separator });

            return (result, null);
        }
        catch(ApiException ex)
        {
            _logger.LogWarning(ex, "API error previewing split for software attribute {AttributeId}", id);

            return (null, ExtractDetail(ex));
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error previewing split for software attribute {AttributeId}", id);

            return (null, ex.Message);
        }
    }

    public async Task<(SplitSoftwareAttributeResultDto? result, string? error)> SplitAsync(long id, string separator)
    {
        try
        {
            SplitSoftwareAttributeResultDto? result = await _apiClient.Software.Attributes[id].Split.PostAsync(
                new SplitSoftwareAttributeRequest { Separator = separator });

            return (result, null);
        }
        catch(ApiException ex)
        {
            _logger.LogWarning(ex, "API error applying split for software attribute {AttributeId}", id);

            return (null, ExtractDetail(ex));
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error applying split for software attribute {AttributeId}", id);

            return (null, ex.Message);
        }
    }

    static string? ExtractDetail(ApiException exception)
    {
        if(exception is ProblemDetails problemDetails)
        {
            if(!string.IsNullOrWhiteSpace(problemDetails.Detail)) return problemDetails.Detail;
            if(!string.IsNullOrWhiteSpace(problemDetails.Title))  return problemDetails.Title;
        }

        return string.IsNullOrWhiteSpace(exception.Message) ? "Unknown error" : exception.Message;
    }
}
