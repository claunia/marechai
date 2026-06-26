#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Marechai.Data;
using Microsoft.Kiota.Abstractions;

namespace Marechai.App.Services;

/// <summary>
///     Service for the admin WinWorldPC import queue and review workflow.
/// </summary>
public sealed class WwpcImportsService
{
    private readonly Client                         _apiClient;
    private readonly ILogger<WwpcImportsService>    _logger;

    public WwpcImportsService(Client apiClient, ILogger<WwpcImportsService> logger)
    {
        _apiClient = apiClient;
        _logger    = logger;
    }

    public async Task<List<WwpcPendingListItemDto>> GetPendingAsync(int skip, int take,
                                                                    WwpcSoftwareStatus? status,
                                                                    WwpcProductType? productType,
                                                                    string? search,
                                                                    bool hasError = false,
                                                                    string? sortBy = null,
                                                                    bool sortDescending = false)
    {
        try
        {
            List<WwpcPendingListItemDto>? items = await _apiClient.Wwpc.Pending.GetAsync(config =>
            {
                config.QueryParameters.Skip           = skip;
                config.QueryParameters.Take           = take;
                config.QueryParameters.Status         = status.HasValue ? (int)status.Value : null;
                config.QueryParameters.ProductType    = productType.HasValue ? (int)productType.Value : null;
                config.QueryParameters.Search         = search;
                config.QueryParameters.HasError       = hasError;
                config.QueryParameters.SortBy         = sortBy;
                config.QueryParameters.SortDescending = sortDescending;
            });

            return items ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to load wwpc pending queue");

            return [];
        }
    }

    public async Task<int> GetPendingCountAsync(WwpcSoftwareStatus? status, WwpcProductType? productType,
                                                string? search, bool hasError = false)
    {
        try
        {
            int? count = await _apiClient.Wwpc.Pending.Count.GetAsync(config =>
            {
                config.QueryParameters.Status      = status.HasValue ? (int)status.Value : null;
                config.QueryParameters.ProductType = productType.HasValue ? (int)productType.Value : null;
                config.QueryParameters.Search      = search;
                config.QueryParameters.HasError    = hasError;
            });

            return count ?? 0;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to load wwpc pending count");

            return 0;
        }
    }

    public async Task<WwpcPendingDetailDto?> GetByIdAsync(long id)
    {
        try
        {
            return await _apiClient.Wwpc.Pending[(int)id].GetAsync();
        }
        catch(Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load wwpc pending #{Id}", id);

            return null;
        }
    }

    public async Task<WwpcPendingDetailDto?> GetNextAsync(long afterId)
    {
        try
        {
            return await _apiClient.Wwpc.Pending.Next.GetAsync(config => config.QueryParameters.AfterId = afterId);
        }
        catch(ApiException ex) when (ex.ResponseStatusCode == 204)
        {
            return null;
        }
        catch(Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch next wwpc pending after #{Id}", afterId);

            return null;
        }
    }

    public async Task<WwpcNameMatchCandidatesDto> GetNameMatchesAsync(long id, string? nameOverride)
    {
        try
        {
            return await _apiClient.Wwpc.Pending[(int)id].NameMatches.GetAsync(config =>
                       config.QueryParameters.Name = nameOverride) ??
                   new WwpcNameMatchCandidatesDto();
        }
        catch(Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch name matches for wwpc #{Id}", id);

            return new WwpcNameMatchCandidatesDto();
        }
    }

    public async Task<WwpcCompanyMatchCandidatesDto> GetVendorMatchesAsync(long id, string? vendorOverride)
    {
        try
        {
            return await _apiClient.Wwpc.Pending[(int)id].VendorMatches.GetAsync(config =>
                       config.QueryParameters.Vendor = vendorOverride) ??
                   new WwpcCompanyMatchCandidatesDto();
        }
        catch(Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch vendor matches for wwpc #{Id}", id);

            return new WwpcCompanyMatchCandidatesDto();
        }
    }

    public async Task<(AcceptWwpcImportResultDto? Result, string? Error)> AcceptAsync(long id, AcceptWwpcImportDto dto)
    {
        string endpoint = $"POST /wwpc/pending/{id}/accept";

        try
        {
            AcceptWwpcImportResultDto? result = await _apiClient.Wwpc.Pending[(int)id].Accept.PostAsync(dto);

            return (result, result?.Success == false ? result.Error : null);
        }
        catch(ApiException ex)
        {
            _logger.LogWarning(ex, "Server rejected {Endpoint} (status {Status}): {Message}",
                               endpoint, ex.ResponseStatusCode, ex.Message);

            return (null, BuildErrorMessage(endpoint, ex));
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error calling {Endpoint}", endpoint);

            return (null, ex.Message);
        }
    }

    public async Task<bool> SkipAsync(long id)
    {
        try
        {
            await _apiClient.Wwpc.Pending[(int)id].Skip.PostAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogWarning(ex, "Failed to skip wwpc #{Id}", id);

            return false;
        }
    }

    public async Task<bool> DiscardAsync(long id)
    {
        try
        {
            await _apiClient.Wwpc.Pending[(int)id].Discard.PostAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogWarning(ex, "Failed to discard wwpc #{Id}", id);

            return false;
        }
    }

    public async Task<(long? NewId, string? Error)> DuplicateAsync(long id, string newName)
    {
        try
        {
            long? newId = await _apiClient.Wwpc.Pending[(int)id].Duplicate.PostAsync(new DuplicateWwpcImportDto
            {
                NewName = newName
            });

            return (newId, null);
        }
        catch(ApiException ex)
        {
            _logger.LogWarning(ex, "Server rejected duplicate for wwpc #{Id}", id);

            return (null, ExtractDetail(ex));
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error duplicating wwpc #{Id}", id);

            return (null, ex.Message);
        }
    }

    private static string BuildErrorMessage(string endpoint, ApiException ex)
    {
        string status = ex.ResponseStatusCode > 0 ? $"HTTP {ex.ResponseStatusCode}" : "HTTP error";
        string msg    = ExtractDetail(ex);

        if(string.IsNullOrWhiteSpace(msg)) return $"{status} on {endpoint} (no response body)";
        if(msg.Length > 400) msg = msg[..400] + "…";

        return $"{status} on {endpoint} - {msg}";
    }

    private static string ExtractDetail(ApiException ex)
    {
        if(ex is ProblemDetails pd)
        {
            if(!string.IsNullOrWhiteSpace(pd.Detail)) return pd.Detail;
            if(!string.IsNullOrWhiteSpace(pd.Title))  return pd.Title;
        }

        if(ex is { ResponseStatusCode: 0 } || string.IsNullOrWhiteSpace(ex.Message)) return "Unknown error";

        return ex.Message;
    }
}
