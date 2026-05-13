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
using Marechai.ApiClient.Models;
using Marechai.Data;
using Marechai.Pages.Suggestions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Marechai.Pages.Magazines;

public partial class IssuesByYear
{
    string                       _lastKey;
    bool                         _loaded;
    bool                         _invalidYear;
    int?                         _yearValue;
    MagazineDto                  _magazine;
    List<MagazineIssueDto>       _issues = [];

    [Inject]
    NavigationManager Nav { get; set; }

    [Parameter]
    public long MagazineId { get; set; }

    /// <summary>
    /// Either a 4-digit year (e.g. <c>"2001"</c>) or the literal string
    /// <c>"others"</c> for issues with no <see cref="MagazineIssueDto.Published"/> date.
    /// </summary>
    [Parameter]
    public string Year { get; set; }

    protected override void OnParametersSet()
    {
        string key = $"{MagazineId}/{Year}";

        if(key == _lastKey) return;

        _lastKey = key;
        _loaded  = false;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(_loaded) return;

        _invalidYear = false;
        _yearValue   = null;

        Task<List<MagazineIssueDto>> issuesTask;

        if(string.Equals(Year, "others", StringComparison.OrdinalIgnoreCase))
        {
            issuesTask = Service.GetIssuesNoYearAsync(MagazineId);
        }
        else if(int.TryParse(Year, out int parsed))
        {
            _yearValue = parsed;
            issuesTask = Service.GetIssuesByYearAsync(MagazineId, parsed);
        }
        else
        {
            _invalidYear = true;
            _loaded      = true;
            StateHasChanged();

            return;
        }

        Task<MagazineDto> magazineTask = Service.GetMagazineAsync(MagazineId);

        await Task.WhenAll(magazineTask, issuesTask);

        _magazine = magazineTask.Result;
        _issues   = issuesTask.Result ?? [];
        _loaded   = true;
        StateHasChanged();
    }

    /// <summary>
    /// Format a publication date according to its precision: year-only, month-and-year, or full date.
    /// Mirrors the inline ternary used on the magazine view.
    /// </summary>
    static string FormatDate(DateTimeOffset published, int? precision) => (precision ?? 0) switch
    {
        2 => published.Year.ToString(),
        1 => published.ToString("MMMM yyyy"),
        _ => published.DateTime.ToShortDateString()
    };

    /// <summary>
    ///     Open the dedicated MagazineIssue suggestion dialog in addition mode (entityId=null,
    ///     parent magazine id pre-filled). When the route year is a 4-digit value, also
    ///     pre-fills the published date to Jan 1 of that year and the precision to
    ///     <see cref="DatePrecision.Year" /> so the user lands in the dialog with the year
    ///     already populated and only needs to refine it (or accept it as-is). For the
    ///     literal "others" route, no date prefill is supplied.
    /// </summary>
    async Task OpenSuggestNewIssueDialog()
    {
        if(_magazine is null) return;

        DateTime?       prefillPublished  = null;
        DatePrecision?  prefillPrecision  = null;
        if(_yearValue.HasValue)
        {
            prefillPublished = new DateTime(_yearValue.Value, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            prefillPrecision = DatePrecision.YearOnly;
        }

        var parameters = new DialogParameters<MagazineIssueSuggestionDialog>
        {
            { x => x.EntityId,                  0L                       },
            { x => x.CurrentDto,                (MagazineIssueDto)null   },
            { x => x.IsCreation,                true                     },
            { x => x.PrefillMagazineId,         (long?)MagazineId        },
            { x => x.PrefillMagazineLabel,      _magazine.Title          },
            { x => x.PrefillPublished,          prefillPublished         },
            { x => x.PrefillPublishedPrecision, prefillPrecision         }
        };

        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            FullWidth        = true,
            MaxWidth         = MaxWidth.Large
        };

        await DialogService.ShowAsync<MagazineIssueSuggestionDialog>(
            L["Suggest new magazine issue"], parameters, options);
    }
}
