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
    /// <summary><c>RankingDefinitions.Id</c> from the URL.</summary>
    [Parameter]
    public int Id { get; set; }

    bool                     _loading  = true;
    bool                     _notFound;
    string                   _title    = string.Empty;
    List<SoftwareRankingDto> _results  = [];

    protected override async Task OnParametersSetAsync()
    {
        _loading  = true;
        _notFound = false;
        _title    = string.Empty;
        _results  = [];

        // Two parallel hits: pull the matching index entry (for the localised title +
        // entry count) and the actual top-N rows. The index call is cheap (one DB pass
        // for ~all rankings) and lets us label the page before falling back to a
        // generic "Ranking #{Id}" caption.
        Task<RankingIndexResponseDto>  indexTask = Service.GetRankingsIndexAsync();
        Task<List<SoftwareRankingDto>> resultsTask = Service.GetRankingAsync(Id);

        await Task.WhenAll(indexTask, resultsTask);

        _results = resultsTask.Result ?? [];

        RankingIndexEntryDto entry =
            indexTask.Result?.Rankings?.FirstOrDefault(r => r.Id == (uint)Id);

        if(entry is not null)
        {
            // Overall ranking (Dimension byte == 0) gets the promoted "Top 250 software
            // of all time" label — matches the prominent card on the Rankings index page.
            // For genre / platform dimensions the server-supplied DimensionName already
            // carries the translated display string.
            _title = entry.Dimension == 0
                         ? L["Top 250 software of all time"]
                         : entry.DimensionName ?? string.Format(L["Ranking #{0}"], Id);
        }
        else if(_results.Count == 0)
        {
            // No matching index entry AND zero results — treat as not-found.
            _notFound = true;
            _title    = L["Ranking not found"];
        }
        else
            _title = string.Format(L["Ranking #{0}"], Id);

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
