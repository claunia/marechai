/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
// Copyright © 2003-2026 Natalia Portillo
*******************************************************************************/

using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.ApiClient;
using Marechai.ApiClient.Models;
using Marechai.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Abstractions;

namespace Marechai.Services;

/// <summary>Client wrapper around the <c>/old-dos</c> admin endpoints.</summary>
public sealed class OldDosImportsService(Client client, ILogger<OldDosImportsService> logger)
{
    public async Task<List<OldDosPendingListItemDto>> GetPendingAsync(int skip, int take,
                                                                       OldDosSoftwareStatus? status,
                                                                       string search,
                                                                       bool hasError = false,
                                                                       string sortBy = null,
                                                                       bool sortDescending = false)
    {
        try
        {
            return await client.OldDos.Pending.GetAsync(c =>
            {
                c.QueryParameters.Skip           = skip;
                c.QueryParameters.Take           = take;
                c.QueryParameters.Status         = status.HasValue ? (int)status.Value : null;
                c.QueryParameters.Search         = search;
                c.QueryParameters.HasError       = hasError;
                c.QueryParameters.SortBy         = sortBy;
                c.QueryParameters.SortDescending = sortDescending;
            });
        }
        catch(System.Exception ex)
        {
            logger.LogError(ex, "Failed to load old-dos pending queue");
            return new List<OldDosPendingListItemDto>();
        }
    }

    /// <summary>Total row count matching the same filter set as <see cref="GetPendingAsync"/>.</summary>
    public async Task<int> GetPendingCountAsync(OldDosSoftwareStatus? status, string search,
                                                bool hasError = false)
    {
        try
        {
            int? count = await client.OldDos.Pending.Count.GetAsync(c =>
            {
                c.QueryParameters.Status   = status.HasValue ? (int)status.Value : null;
                c.QueryParameters.Search   = search;
                c.QueryParameters.HasError = hasError;
            });
            return count ?? 0;
        }
        catch(System.Exception ex)
        {
            logger.LogError(ex, "Failed to load old-dos pending count");
            return 0;
        }
    }

    public async Task<OldDosPendingDetailDto> GetByIdAsync(long id)
    {
        try { return await client.OldDos.Pending[(int)id].GetAsync(); }
        catch(System.Exception ex)
        {
            logger.LogWarning(ex, "Failed to load old-dos pending #{Id}", id);
            return null;
        }
    }

    public async Task<OldDosPendingDetailDto> GetNextAsync(long afterId)
    {
        try
        {
            return await client.OldDos.Pending.Next.GetAsync(c => c.QueryParameters.AfterId = afterId);
        }
        catch(ApiException ex) when (ex.ResponseStatusCode == 204)
        {
            return null;
        }
        catch(System.Exception ex)
        {
            logger.LogWarning(ex, "Failed to fetch next old-dos pending after #{Id}", afterId);
            return null;
        }
    }

    public async Task<OldDosNameMatchCandidatesDto> GetNameMatchesAsync(long id, string nameOverride)
    {
        try
        {
            return await client.OldDos.Pending[(int)id].NameMatches.GetAsync(c =>
            {
                c.QueryParameters.Name = nameOverride;
            });
        }
        catch(System.Exception ex)
        {
            logger.LogWarning(ex, "Failed to fetch name matches for old-dos #{Id}", id);
            return new OldDosNameMatchCandidatesDto();
        }
    }

    public async Task<(AcceptOldDosImportResultDto result, string error)> AcceptAsync(long id, AcceptOldDosImportDto dto)
    {
        string endpoint = $"POST /old-dos/pending/{id}/accept";
        try
        {
            AcceptOldDosImportResultDto result = await client.OldDos.Pending[(int)id].Accept.PostAsync(dto);
            return (result, result?.Success == false ? result.Error : null);
        }
        catch(ApiException ex)
        {
            // Surface as much detail as Kiota gives us. For non-mapped status codes (anything
            // other than 400 for this endpoint) ApiException.Message often carries the raw
            // response body or the Kiota "no factory registered" diagnostic. Logging the
            // headers too helps trace auth-redirect / WWW-Authenticate / proxy cases.
            logger.LogWarning(ex, "Server rejected {Endpoint} (status {Status}): {Message}",
                              endpoint, ex.ResponseStatusCode, ex.Message);
            return (null, BuildErrorMessage(endpoint, ex));
        }
        catch(System.Exception ex)
        {
            logger.LogError(ex, "Error calling {Endpoint}", endpoint);
            return (null, ex.Message);
        }
    }

    /// <summary>
    ///     Build a human-readable diagnostic from a Kiota <see cref="ApiException"/>. Includes the HTTP
    ///     status code plus, when present, a short slice of the extracted ProblemDetails detail / title
    ///     (which is the human-friendly text the server intended to surface) so the admin dialog shows
    ///     more than just <c>HTTP 404</c>. When the message is empty the endpoint path is appended so
    ///     it's clear which call failed.
    /// </summary>
    static string BuildErrorMessage(string endpoint, ApiException ex)
    {
        string status = ex.ResponseStatusCode > 0 ? $"HTTP {ex.ResponseStatusCode}" : "HTTP error";
        string msg    = ExtractDetail(ex);
        if(string.IsNullOrWhiteSpace(msg)) return $"{status} on {endpoint} (no response body)";
        if(msg.Length > 400) msg = msg.Substring(0, 400) + "…";
        return $"{status} on {endpoint} — {msg}";
    }

    static string ExtractDetail(ApiException ex)
    {
        // Kiota maps server error responses to a typed ProblemDetails (which inherits from
        // ApiException). The base Exception.Message just returns "Exception of type 'X' was
        // thrown." — the real, user-facing text lives on Detail / Title. Surface those when
        // present, falling back to Message only if the server gave us nothing useful.
        if(ex is ProblemDetails pd)
        {
            if(!string.IsNullOrWhiteSpace(pd.Detail)) return pd.Detail;
            if(!string.IsNullOrWhiteSpace(pd.Title))  return pd.Title;
        }

        if(ex is { ResponseStatusCode: 0 } || string.IsNullOrWhiteSpace(ex.Message)) return "Unknown error";

        return ex.Message;
    }

    public async Task<bool> SkipAsync(long id)
    {
        try { await client.OldDos.Pending[(int)id].Skip.PostAsync(); return true; }
        catch(System.Exception ex)
        {
            logger.LogWarning(ex, "Failed to skip old-dos #{Id}", id);
            return false;
        }
    }

    public async Task<bool> DiscardAsync(long id)
    {
        try { await client.OldDos.Pending[(int)id].Discard.PostAsync(); return true; }
        catch(System.Exception ex)
        {
            logger.LogWarning(ex, "Failed to discard old-dos #{Id}", id);
            return false;
        }
    }
}
