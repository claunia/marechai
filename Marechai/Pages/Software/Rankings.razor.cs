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
using System.Threading;
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Marechai.Data;
using MudBlazor;

namespace Marechai.Pages.Software;

public partial class Rankings
{
    const int                            _topCount = 250;
    bool                                 _filtersExpanded;
    bool                                 _loading = true;
    SoftwareKind?                        _selectedKind;
    SoftwareGenreDto                     _selectedGenre;
    SoftwarePlatformDto                  _selectedPlatform;
    List<SoftwareGenreDto>               _genres    = [];
    List<SoftwarePlatformDto>            _platforms = [];
    List<SoftwareRankingDto>             _results   = [];

    int _activeFilterCount =>
        (_selectedKind.HasValue ? 1 : 0) +
        (_selectedGenre is not null ? 1 : 0) +
        (_selectedPlatform is not null ? 1 : 0);

    protected override async Task OnInitializedAsync()
    {
        Task<List<SoftwareGenreDto>>    genresTask    = Service.GetAllGenresAsync();
        Task<List<SoftwarePlatformDto>> platformsTask = Service.GetPlatformsAsync();
        Task<List<SoftwareRankingDto>>  rankingsTask  = Service.GetRankingsAsync(take: _topCount);

        await Task.WhenAll(genresTask, platformsTask, rankingsTask);

        _genres    = genresTask.Result    ?? [];
        _platforms = platformsTask.Result ?? [];
        _results   = rankingsTask.Result  ?? [];
        _loading   = false;
    }

    async Task ReloadAsync()
    {
        _loading = true;
        StateHasChanged();

        _results = await Service.GetRankingsAsync(_selectedKind, _selectedGenre?.Id, _selectedPlatform?.Id,
                                                  _topCount);

        _loading = false;
        StateHasChanged();
    }

    async Task OnKindChanged(SoftwareKind? kind)
    {
        _selectedKind = kind;
        await ReloadAsync();
    }

    async Task OnGenreChanged(SoftwareGenreDto genre)
    {
        _selectedGenre = genre;
        await ReloadAsync();
    }

    async Task OnPlatformChanged(SoftwarePlatformDto platform)
    {
        _selectedPlatform = platform;
        await ReloadAsync();
    }

    async Task ClearFilters()
    {
        _selectedKind     = null;
        _selectedGenre    = null;
        _selectedPlatform = null;
        await ReloadAsync();
    }

    Task<IEnumerable<SoftwareGenreDto>> SearchGenres(string value, CancellationToken cancellationToken)
    {
        if(string.IsNullOrWhiteSpace(value)) return Task.FromResult<IEnumerable<SoftwareGenreDto>>(_genres);

        IEnumerable<SoftwareGenreDto> filtered = _genres.Where(g =>
            (!string.IsNullOrEmpty(g.Name) &&
             g.Name.Contains(value, StringComparison.OrdinalIgnoreCase)) ||
            (!string.IsNullOrEmpty(L[g.Name]) &&
             L[g.Name].Value.Contains(value, StringComparison.OrdinalIgnoreCase)));

        return Task.FromResult(filtered);
    }

    Task<IEnumerable<SoftwarePlatformDto>> SearchPlatforms(string value, CancellationToken cancellationToken)
    {
        if(string.IsNullOrWhiteSpace(value)) return Task.FromResult<IEnumerable<SoftwarePlatformDto>>(_platforms);

        IEnumerable<SoftwarePlatformDto> filtered = _platforms.Where(p =>
            !string.IsNullOrEmpty(p.Name) &&
            p.Name.Contains(value, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(filtered);
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
