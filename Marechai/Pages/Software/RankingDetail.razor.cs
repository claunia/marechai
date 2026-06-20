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
using System.Linq;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Marechai.Data;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Marechai.Pages.Software;

public partial class RankingDetail
{
    /// <summary><c>RankingDefinitions.Id</c> from the URL (numeric route only).</summary>
    [Parameter]
    public int Id { get; set; }

    /// <summary>Route slug: "top250" for the overall ranking.</summary>
    [Parameter]
    public string Slug { get; set; } = string.Empty;

    bool                     _loading  = true;
    bool                     _notFound;
    string                   _title    = string.Empty;
    List<SoftwareRankingDto> _results  = [];
    int                      _resolvedRankingId;

    protected override async Task OnParametersSetAsync()
    {
        _loading  = true;
        _notFound = false;
        _title    = string.Empty;
        _results  = [];
        _resolvedRankingId = 0;

        Task<RankingIndexResponseDto> indexTask = Service.GetRankingsIndexAsync();

        // Determine the effective ranking ID: either from the route parameter (numeric
        // route) or by looking up the "Dimension == 0" entry (top250 route).
        int effectiveId;

        if(!string.IsNullOrEmpty(Slug) && Slug == "top250")
        {
            // top250 route: fetch index to resolve the current overall ranking ID.
            RankingIndexResponseDto indexResp = await indexTask;
            RankingIndexEntryDto overallEntry =
                indexResp?.Rankings?.FirstOrDefault(r => r.Dimension == 0);

            if(overallEntry is null)
            {
                _notFound = true;
                _title    = L["Ranking not found"];
                _loading  = false;
                return;
            }

            effectiveId = (int)overallEntry.Id;
            _resolvedRankingId = effectiveId;
        }
        else
        {
            // Numeric route: use the Id parameter.
            effectiveId = Id;
        }

        // Fetch the ranking results and index in parallel.
        Task<List<SoftwareRankingDto>> resultsTask = Service.GetRankingAsync(effectiveId);

        if(string.IsNullOrEmpty(Slug))
            // Already started for the top250 route; await again if not yet done.
            await Task.WhenAll(indexTask, resultsTask);
        else
            await resultsTask;

        _results = resultsTask.Result ?? [];

        RankingIndexEntryDto entry =
            indexTask.Result?.Rankings?.FirstOrDefault(r => r.Id == effectiveId);

        if(entry is not null)
        {
            // Overall ranking (Dimension byte == 0) gets the promoted "Top 250 software
            // of all time" label — matches the prominent card on the Rankings index page.
            // For genre / platform dimensions the server-supplied DimensionName already
            // carries the translated display string.
            _title = entry.Dimension == 0
                         ? L["Top 250 software of all time"]
                         : entry.DimensionName ?? string.Format(L["Ranking #{0}"], effectiveId);

            // If invoked via the numeric route and this is the overall ranking,
            // redirect to the canonical static URL.
            if(string.IsNullOrEmpty(Slug) && entry.Dimension == 0)
            {
                Navigation.NavigateTo("/software/rankings/top250", replace: true);
                _loading = false;
                return;
            }

            _resolvedRankingId = effectiveId;
        }
        else if(_results.Count == 0)
        {
            // No matching index entry AND zero results — treat as not-found.
            _notFound = true;
            _title    = L["Ranking not found"];
        }
        else
        {
            _title = string.Format(L["Ranking #{0}"], effectiveId);
            _resolvedRankingId = effectiveId;
        }

        _loading = false;
    }

    static (string label, Color color) KindChip(SoftwareKind kind) => kind switch
    {
        SoftwareKind.OperatingSystem     => ("OS", Color.Info),
        SoftwareKind.Game                => ("Game", Color.Success),
        SoftwareKind.Dlc                 => ("DLC / Addon", Color.Warning),
        SoftwareKind.SystemSoftware      => ("System software", Color.Default),
        SoftwareKind.Application         => ("Application", Color.Primary),
        SoftwareKind.DevelopmentSoftware => ("Development software", Color.Secondary),
        SoftwareKind.ServerSoftware      => ("Server software", Color.Tertiary),
        SoftwareKind.Middleware          => ("Middleware", Color.Info),
        SoftwareKind.Firmware            => ("Firmware", Color.Default),
        SoftwareKind.EmbeddedSoftware    => ("Embedded software", Color.Default),
        _                                => ("Software", Color.Default)
    };

    string ReviewCountSummary(SoftwareRankingDto item)
    {
        int reviews = item.CriticReviewCount ?? 0;
        int ratings = item.UserRatingCount   ?? 0;

        return (reviews, ratings) switch
        {
            (0, 0) => string.Empty,
            (_, 0) => string.Format(L["{0} reviews"], reviews),
            (0, _) => string.Format(L["{0} ratings"], ratings),
            _      => string.Format(L["{0} reviews / {1} ratings"], reviews, ratings)
        };
    }
}
