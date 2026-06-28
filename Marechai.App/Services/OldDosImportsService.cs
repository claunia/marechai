#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Marechai.Data;
using Microsoft.Kiota.Abstractions;

namespace Marechai.App.Services;

/// <summary>
///     Service for the admin old-dos import queue and review workflow.
/// </summary>
public sealed class OldDosImportsService
{
    private readonly Client _apiClient;
    private readonly ILogger<OldDosImportsService> _logger;

    public OldDosImportsService(Client apiClient, ILogger<OldDosImportsService> logger)
    {
        _apiClient = apiClient;
        _logger    = logger;
    }

    public async Task<List<OldDosPendingListItemDto>> GetPendingAsync(int skip, int take,
                                                                      OldDosSoftwareStatus? status,
                                                                      string? search,
                                                                      bool hasError = false,
                                                                      string? sortBy = null,
                                                                      bool sortDescending = false)
    {
        try
        {
            List<OldDosPendingListItemDto>? items = await _apiClient.OldDos.Pending.GetAsync(config =>
            {
                config.QueryParameters.Skip           = skip;
                config.QueryParameters.Take           = take;
                config.QueryParameters.Status         = status.HasValue ? (int)status.Value : null;
                config.QueryParameters.Search         = search;
                config.QueryParameters.HasError       = hasError;
                config.QueryParameters.SortBy         = sortBy;
                config.QueryParameters.SortDescending = sortDescending;
            });

            return items ?? [];
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to load old-dos pending queue");

            return [];
        }
    }

    public async Task<int> GetPendingCountAsync(OldDosSoftwareStatus? status, string? search, bool hasError = false)
    {
        try
        {
            int? count = await _apiClient.OldDos.Pending.Count.GetAsync(config =>
            {
                config.QueryParameters.Status   = status.HasValue ? (int)status.Value : null;
                config.QueryParameters.Search   = search;
                config.QueryParameters.HasError = hasError;
            });

            return count ?? 0;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to load old-dos pending count");

            return 0;
        }
    }

    public async Task<OldDosPendingDetailDto?> GetByIdAsync(long id)
    {
        try
        {
            return await _apiClient.OldDos.Pending[(int)id].GetAsync();
        }
        catch(Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load old-dos pending #{Id}", id);

            return null;
        }
    }

    public async Task<OldDosPendingDetailDto?> GetNextAsync(long afterId)
    {
        try
        {
            return await _apiClient.OldDos.Pending.Next.GetAsync(config => config.QueryParameters.AfterId = afterId);
        }
        catch(ApiException ex) when (ex.ResponseStatusCode == 204)
        {
            return null;
        }
        catch(Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch next old-dos pending after #{Id}", afterId);

            return null;
        }
    }

    public async Task<OldDosNameMatchCandidatesDto> GetNameMatchesAsync(long id, string? nameOverride)
    {
        try
        {
            return await _apiClient.OldDos.Pending[(int)id].NameMatches.GetAsync(config =>
                       config.QueryParameters.Name = nameOverride) ??
                   new OldDosNameMatchCandidatesDto();
        }
        catch(Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch name matches for old-dos #{Id}", id);

            return new OldDosNameMatchCandidatesDto();
        }
    }

    public async Task<(AcceptOldDosImportResultDto? Result, string? Error)> AcceptAsync(long id, AcceptOldDosImportDto dto)
    {
        string endpoint = $"POST /old-dos/pending/{id}/accept";

        try
        {
            AcceptOldDosImportResultDto? result = await _apiClient.OldDos.Pending[(int)id].Accept.PostAsync(dto);

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
            await _apiClient.OldDos.Pending[(int)id].Skip.PostAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogWarning(ex, "Failed to skip old-dos #{Id}", id);

            return false;
        }
    }

    public async Task<bool> DiscardAsync(long id)
    {
        try
        {
            await _apiClient.OldDos.Pending[(int)id].Discard.PostAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogWarning(ex, "Failed to discard old-dos #{Id}", id);

            return false;
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
