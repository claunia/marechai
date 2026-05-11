/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2003-2026 Natalia Portillo
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Marechai.ApiClient;
using Marechai.ApiClient.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Abstractions;

namespace Marechai.Services;

/// <summary>
///     Client wrapper around the <c>/suggestions</c> and <c>/auth/me/suggestions</c> endpoints.
///     Returns <c>null</c> / empty / error tuples on failure rather than throwing — matches the
///     convention of every other Marechai service.
/// </summary>
public sealed class SuggestionsService(Client client, ILogger<SuggestionsService> logger)
{
    /// <summary>Submit a new suggestion. Returns the persisted DTO or <c>(null, error)</c>.</summary>
    public async Task<(SuggestionDto created, string error)> CreateAsync(SuggestionDto dto)
    {
        try
        {
            SuggestionDto created = await client.Suggestions.PostAsync(dto);

            return (created, null);
        }
        catch(ApiException ex)
        {
            logger.LogWarning(ex, "API rejected suggestion (status {Status})", ex.ResponseStatusCode);

            return (null, ExtractDetail(ex));
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Error submitting suggestion");

            return (null, ex.Message);
        }
    }

    /// <summary>Admin queue. <paramref name="includeHistory" /> only honored for UberAdmins server-side.</summary>
    public async Task<List<SuggestionDto>> GetQueueAsync(bool includeHistory = false,
                                                          int  page           = 1,
                                                          int  pageSize       = 50)
    {
        try
        {
            return await client.Suggestions.Queue.GetAsync(c =>
            {
                c.QueryParameters.IncludeHistory = includeHistory;
                c.QueryParameters.Page           = page;
                c.QueryParameters.PageSize       = pageSize;
            });
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Error loading suggestion queue");

            return new List<SuggestionDto>();
        }
    }

    /// <summary>Admin pending-suggestion count for the top-bar badge. Returns 0 on any failure.</summary>
    public async Task<int> GetPendingCountAsync()
    {
        try
        {
            return await client.Suggestions.Queue.Count.GetAsync() ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>Admin diff fetch (live current values + suggestion).</summary>
    public async Task<SuggestionDiffDto> GetDiffAsync(long id)
    {
        try
        {
            return await client.Suggestions[id].Diff.GetAsync();
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Error loading diff for suggestion {Id}", id);

            return null;
        }
    }

    /// <summary>Admin review action — accept selected fields. Returns updated suggestion or error.</summary>
    public async Task<(SuggestionDto updated, string error)> ReviewAsync(long id, SuggestionReviewDto review)
    {
        try
        {
            SuggestionDto updated = await client.Suggestions[id].Review.PostAsync(review);

            return (updated, null);
        }
        catch(ApiException ex)
        {
            logger.LogWarning(ex, "API error reviewing suggestion {Id} (status {Status})", id, ex.ResponseStatusCode);

            return (null, ExtractDetail(ex));
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Error reviewing suggestion {Id}", id);

            return (null, ex.Message);
        }
    }

    /// <summary>Authenticated user's own suggestion list, all statuses.</summary>
    public async Task<List<SuggestionDto>> GetMyAsync()
    {
        try
        {
            return await client.Auth.Me.Suggestions.GetAsync();
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Error loading my suggestions");

            return new List<SuggestionDto>();
        }
    }

    /// <summary>Withdraw a pending suggestion the caller created.</summary>
    public async Task<(bool success, string error)> WithdrawAsync(long id)
    {
        try
        {
            await client.Auth.Me.Suggestions[id].DeleteAsync();

            return (true, null);
        }
        catch(ApiException ex)
        {
            logger.LogWarning(ex, "API error withdrawing suggestion {Id}", id);

            return (false, ExtractDetail(ex));
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Error withdrawing suggestion {Id}", id);

            return (false, ex.Message);
        }
    }

    static string ExtractDetail(ApiException ex)
    {
        // ProblemDetails is what the server returns; expose the Detail string when available.
        if(ex is { ResponseStatusCode: 0 } || ex.Message is null) return "Unknown error";

        return ex.Message;
    }
}
