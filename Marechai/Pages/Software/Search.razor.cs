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
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Marechai.ApiClient.Models;
using Marechai.Data;
using Microsoft.AspNetCore.Components;

namespace Marechai.Pages.Software;

public partial class Search
{
    char?             _character;
    int?              _lastGenreId;
    string            _lastKindString;
    int?              _lastPlatformId;
    string            _lastStartingCharacter;
    int?              _lastYear;
    bool              _loaded;
    string            _genreName;
    SoftwareKind?     _kind;
    string            _platformName;
    List<SoftwareDto> _software;

    [Inject]
    NavigationManager NavigationManager { get; set; }

    [Parameter]
    public int? Year { get; set; }

    [Parameter]
    public string StartingCharacter { get; set; }

    [Parameter]
    public int? PlatformId { get; set; }

    [Parameter]
    public int? GenreId { get; set; }

    [SupplyParameterFromQuery(Name = "key")]
    public string SpecKey { get; set; }

    [SupplyParameterFromQuery(Name = "value")]
    public string SpecValue { get; set; }

    [SupplyParameterFromQuery(Name = "kind")]
    public string KindString { get; set; }

    protected override void OnParametersSet()
    {
        if(Year == _lastYear && StartingCharacter == _lastStartingCharacter &&
           PlatformId == _lastPlatformId && GenreId == _lastGenreId &&
           string.Equals(KindString, _lastKindString, StringComparison.OrdinalIgnoreCase)) return;

        _lastYear              = Year;
        _lastStartingCharacter = StartingCharacter;
        _lastPlatformId        = PlatformId;
        _lastGenreId           = GenreId;
        _lastKindString        = KindString;
        _loaded                = false;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(_loaded) return;

        _character = null;
        _kind      = null;
        _software  = null;

        if(!string.IsNullOrWhiteSpace(KindString) &&
           Enum.TryParse<SoftwareKind>(KindString, true, out SoftwareKind parsed) &&
           Enum.IsDefined(typeof(SoftwareKind), parsed))
            _kind = parsed;

        if(!string.IsNullOrWhiteSpace(StartingCharacter) && StartingCharacter.Length == 1)
        {
            _character = StartingCharacter[0];

            // ToUpper()
            if(_character >= 'a' && _character <= 'z') _character -= (char)32;

            // Check if not letter or number
            if(_character < '0' || _character > '9' && _character < 'A' || _character > 'Z') _character = null;
        }

        if(_character.HasValue) _software = await Service.GetSoftwareByLetterAsync(_character.Value, _kind);

        if(Year.HasValue && _software is null) _software = await Service.GetSoftwareByYearAsync(Year.Value, _kind);

        if(PlatformId.HasValue && _software is null)
        {
            _software = await Service.GetSoftwareByPlatformAsync(PlatformId.Value, _kind);

            // Get platform name from first result or from platforms list
            List<SoftwarePlatformDto> platforms = await Service.GetPlatformsAsync();
            _platformName = platforms.FirstOrDefault(p => p.Id == PlatformId.Value)?.Name;
        }

        if(GenreId.HasValue && _software is null)
        {
            _software = await Service.GetSoftwareByGenreAsync(GenreId.Value, _kind);

            // Get genre name from the genres list
            List<SoftwareGenreDto> genres = await Service.GetAllGenresAsync();
            _genreName = genres.FirstOrDefault(g => g.Id == GenreId.Value)?.Name;
        }

        if(!string.IsNullOrEmpty(SpecKey) && !string.IsNullOrEmpty(SpecValue) && _software is null)
            _software = await Service.GetSoftwareBySpecAsync(SpecKey, SpecValue, _kind);

        _software ??= await Service.GetAllSoftwareAsync(_kind);
        _loaded   =   true;
        StateHasChanged();
    }

    void OnKindChanged(SoftwareKind? value)
    {
        if(value == _kind) return;

        // Rewrite the current URL preserving path + non-kind query params, replacing
        // (or removing) the kind value, and force-reload via NavigateTo.
        Uri    uri      = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
        NameValueCollection q = HttpUtility.ParseQueryString(uri.Query);
        q.Remove("kind");

        if(value.HasValue) q["kind"] = value.Value.ToString();

        string query  = q.Count == 0 ? string.Empty : "?" + q;
        string target = uri.GetLeftPart(UriPartial.Path) + query;
        NavigationManager.NavigateTo(target);
    }
}
