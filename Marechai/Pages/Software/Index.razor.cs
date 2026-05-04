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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;

namespace Marechai.Pages.Software;

public partial class Index
{
    int                       _count;
    Dictionary<string, List<SoftwareGenreDto>> _genresByType;
    bool                      _loaded;
    int                       _maxYear;
    int                       _minYear;
    List<SoftwarePlatformDto> _platforms = [];

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(_loaded) return;

        _count    = await Service.GetSoftwareCountAsync();
        _minYear  = await Service.GetMinimumYearAsync();
        _maxYear  = await Service.GetMaximumYearAsync();
        _platforms = await Service.GetPlatformsAsync();

        List<SoftwareGenreDto> genres = await Service.GetAllGenresAsync();

        _genresByType = genres.GroupBy(g => g.TypeName ?? "Genre")
                              .ToDictionary(g => g.Key, g => g.ToList());

        _loaded = true;
        StateHasChanged();
    }
}
