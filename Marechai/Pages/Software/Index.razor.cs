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
    int                                        _count;
    Dictionary<string, List<SoftwareGenreDto>> _genresByType;
    bool                                       _loaded;
    int                                        _maxYear;
    int                                        _minYear;
    List<SoftwarePlatformDto>                  _platforms  = [];
    List<SoftwareSpecKeyDto>                   _specsByKey = [];

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(_loaded) return;

        // Fan out: every call is independent, so kick them off in parallel
        // instead of awaiting each one sequentially. Was 6 round-trips
        // (~6 × DB RTT) on the same critical path.
        Task<int>                       countTask     = Service.GetSoftwareCountAsync();
        Task<int>                       minYearTask   = Service.GetMinimumYearAsync();
        Task<int>                       maxYearTask   = Service.GetMaximumYearAsync();
        Task<List<SoftwarePlatformDto>> platformsTask = Service.GetPlatformsAsync();
        Task<List<SoftwareGenreDto>>    genresTask    = Service.GetAllGenresAsync();
        Task<List<SoftwareSpecKeyDto>>  specsTask     = Service.GetSpecificationsAsync();

        await Task.WhenAll(countTask, minYearTask, maxYearTask, platformsTask, genresTask, specsTask);

        _count     = countTask.Result;
        _minYear   = minYearTask.Result;
        _maxYear   = maxYearTask.Result;
        _platforms = platformsTask.Result;

        _genresByType = genresTask.Result
                                  .GroupBy(g => g.TypeName ?? "Genre")
                                  .ToDictionary(g => g.Key, g => g.ToList());

        _specsByKey = specsTask.Result;

        _loaded = true;
        StateHasChanged();
    }
}
