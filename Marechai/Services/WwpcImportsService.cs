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

/// <summary>Client wrapper around the <c>/wwpc</c> admin endpoints.</summary>
public sealed class WwpcImportsService(Client client, ILogger<WwpcImportsService> logger)
{
    public async Task<List<WwpcPendingListItemDto>> GetPendingAsync(int skip, int take,
                                                                    WwpcSoftwareStatus? status,
                                                                    WwpcProductType?    productType,
                                                                    string search,
                                                                    bool hasError = false,
                                                                    string sortBy = null,
                                                                    bool sortDescending = false)
    {
        try
        {
            return await client.Wwpc.Pending.GetAsync(c =>
            {
                c.QueryParameters.Skip           = skip;
                c.QueryParameters.Take           = take;
                c.QueryParameters.Status         = status.HasValue ? (int)status.Value : null;
                c.QueryParameters.ProductType    = productType.HasValue ? (int)productType.Value : null;
                c.QueryParameters.Search         = search;
                c.QueryParameters.HasError       = hasError;
                c.QueryParameters.SortBy         = sortBy;
                c.QueryParameters.SortDescending = sortDescending;
            });
        }
        catch(System.Exception ex)
        {
            logger.LogError(ex, "Failed to load wwpc pending queue");
            return new List<WwpcPendingListItemDto>();
        }
    }

    public async Task<int> GetPendingCountAsync(WwpcSoftwareStatus? status, WwpcProductType? productType, string search,
                                                bool hasError = false)
    {
        try
        {
            int? count = await client.Wwpc.Pending.Count.GetAsync(c =>
            {
                c.QueryParameters.Status      = status.HasValue ? (int)status.Value : null;
                c.QueryParameters.ProductType = productType.HasValue ? (int)productType.Value : null;
                c.QueryParameters.Search      = search;
                c.QueryParameters.HasError    = hasError;
            });
            return count ?? 0;
        }
        catch(System.Exception ex)
        {
            logger.LogError(ex, "Failed to load wwpc pending count");
            return 0;
        }
    }

    public async Task<WwpcPendingDetailDto> GetByIdAsync(long id)
    {
        try { return await client.Wwpc.Pending[(int)id].GetAsync(); }
        catch(System.Exception ex)
        {
            logger.LogWarning(ex, "Failed to load wwpc pending #{Id}", id);
            return null;
        }
    }

    public async Task<WwpcPendingDetailDto> GetNextAsync(long afterId)
    {
        try
        {
            return await client.Wwpc.Pending.Next.GetAsync(c => c.QueryParameters.AfterId = afterId);
        }
        catch(ApiException ex) when (ex.ResponseStatusCode == 204) { return null; }
        catch(System.Exception ex)
        {
            logger.LogWarning(ex, "Failed to fetch next wwpc pending after #{Id}", afterId);
            return null;
        }
    }

    public async Task<WwpcNameMatchCandidatesDto> GetNameMatchesAsync(long id, string nameOverride)
    {
        try
        {
            return await client.Wwpc.Pending[(int)id].NameMatches.GetAsync(c =>
                c.QueryParameters.Name = nameOverride);
        }
        catch(System.Exception ex)
        {
            logger.LogWarning(ex, "Failed to fetch name matches for wwpc #{Id}", id);
            return new WwpcNameMatchCandidatesDto();
        }
    }

    public async Task<WwpcCompanyMatchCandidatesDto> GetVendorMatchesAsync(long id, string vendorOverride)
    {
        try
        {
            return await client.Wwpc.Pending[(int)id].VendorMatches.GetAsync(c =>
                c.QueryParameters.Vendor = vendorOverride);
        }
        catch(System.Exception ex)
        {
            logger.LogWarning(ex, "Failed to fetch vendor matches for wwpc #{Id}", id);
            return new WwpcCompanyMatchCandidatesDto();
        }
    }

    public async Task<(AcceptWwpcImportResultDto result, string error)> AcceptAsync(long id, AcceptWwpcImportDto dto)
    {
        try
        {
            AcceptWwpcImportResultDto result = await client.Wwpc.Pending[(int)id].Accept.PostAsync(dto);
            return (result, result?.Success == false ? result.Error : null);
        }
        catch(ApiException ex)
        {
            logger.LogWarning(ex, "Server rejected wwpc accept #{Id}", id);
            return (null, $"HTTP {ex.ResponseStatusCode}");
        }
        catch(System.Exception ex)
        {
            logger.LogError(ex, "Error accepting wwpc #{Id}", id);
            return (null, ex.Message);
        }
    }

    public async Task<bool> SkipAsync(long id)
    {
        try { await client.Wwpc.Pending[(int)id].Skip.PostAsync(); return true; }
        catch(System.Exception ex)
        {
            logger.LogWarning(ex, "Failed to skip wwpc #{Id}", id);
            return false;
        }
    }

    public async Task<bool> DiscardAsync(long id)
    {
        try { await client.Wwpc.Pending[(int)id].Discard.PostAsync(); return true; }
        catch(System.Exception ex)
        {
            logger.LogWarning(ex, "Failed to discard wwpc #{Id}", id);
            return false;
        }
    }
}
