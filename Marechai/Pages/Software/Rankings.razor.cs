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

/// <summary>
///     Index page listing every available Marechai ranking grouped by axis: Overall
///     first (a single entry — "Top 250 Software"), then per-genre rankings, then
///     per-platform rankings. Empty groups are not rendered. Clicking a row navigates
///     to <c>RankingDetail</c> at <c>/software/rankings/{Id}</c>.
/// </summary>
public partial class Rankings
{
    // SoftwareGenreType byte mirror (Marechai.Data.SoftwareGenreType): 0=Genre,
    // 1=Perspective (excluded from rankings), 2=Gameplay, 3=Setting, 4=Category.
    // Kept as a private const set so the split below stays self-documenting and
    // robust to future enum additions (anything we don't recognise lands in the
    // "By genre" bucket as a safe default).
    const byte GENRE_TYPE_GENRE    = 0;
    const byte GENRE_TYPE_GAMEPLAY = 2;
    const byte GENRE_TYPE_SETTING  = 3;
    const byte GENRE_TYPE_CATEGORY = 4;

    bool                       _loading = true;
    RankingsStatusDto          _status;
    List<RankingIndexEntryDto> _overall   = [];
    List<RankingIndexEntryDto> _gameplay  = [];
    List<RankingIndexEntryDto> _genres    = [];
    List<RankingIndexEntryDto> _settings  = [];
    List<RankingIndexEntryDto> _categories = [];
    List<RankingIndexEntryDto> _platforms = [];

    protected override async Task OnInitializedAsync()
    {
        RankingIndexResponseDto resp = await Service.GetRankingsIndexAsync();

        _status = resp?.Status;

        List<RankingIndexEntryDto> rankings = resp?.Rankings ?? [];

        // Dimension byte: 0 = Overall, 1 = Genre, 2 = Platform. Already sorted server-side
        // by (Dimension, DimensionName) so we just split here. Per-genre rankings are then
        // further split by the SoftwareGenre.Type byte (carried on the DTO as GenreType).
        _overall   = rankings.Where(r => r.Dimension == 0).ToList();
        _platforms = rankings.Where(r => r.Dimension == 2).ToList();

        List<RankingIndexEntryDto> allGenres = rankings.Where(r => r.Dimension == 1).ToList();

        _gameplay   = allGenres.Where(r => r.GenreType == GENRE_TYPE_GAMEPLAY).ToList();
        _genres     = allGenres.Where(r => r.GenreType == GENRE_TYPE_GENRE).ToList();
        _settings   = allGenres.Where(r => r.GenreType == GENRE_TYPE_SETTING).ToList();
        _categories = allGenres.Where(r => r.GenreType == GENRE_TYPE_CATEGORY).ToList();

        _loading = false;
    }
}
