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
                                                                       bool hasError = false)
    {
        try
        {
            return await client.OldDos.Pending.GetAsync(c =>
            {
                c.QueryParameters.Skip     = skip;
                c.QueryParameters.Take     = take;
                c.QueryParameters.Status   = status.HasValue ? (int)status.Value : null;
                c.QueryParameters.Search   = search;
                c.QueryParameters.HasError = hasError;
            });
        }
        catch(System.Exception ex)
        {
            logger.LogError(ex, "Failed to load old-dos pending queue");
            return new List<OldDosPendingListItemDto>();
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
        try
        {
            AcceptOldDosImportResultDto result = await client.OldDos.Pending[(int)id].Accept.PostAsync(dto);
            return (result, result?.Success == false ? result.Error : null);
        }
        catch(ApiException ex)
        {
            logger.LogWarning(ex, "Server rejected old-dos accept #{Id}", id);
            return (null, $"HTTP {ex.ResponseStatusCode}");
        }
        catch(System.Exception ex)
        {
            logger.LogError(ex, "Error accepting old-dos #{Id}", id);
            return (null, ex.Message);
        }
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
