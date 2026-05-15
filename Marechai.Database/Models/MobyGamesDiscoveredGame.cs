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
using System.ComponentModel.DataAnnotations;

namespace Marechai.Database.Models;

/// <summary>
///     Tracks games discovered via the MobyGames yearly search index (<c>/game/from:Y/.../until:Y/page:N/</c>)
///     so they can later be fetched into <c>mobygames_raw</c> by the per-game raw fetcher. One row per slug.
///     Re-runs of the yearly crawl are idempotent: they update <see cref="LastSeenAt" /> on encountered slugs
///     without resetting <see cref="RawFetchedAt" />.
/// </summary>
public class MobyGamesDiscoveredGame : BaseModel<long>
{
    /// <summary>
    ///     MobyGames slug, e.g. <c>"sonic-the-hedgehog"</c>. Matches the <c>id</c> column of the
    ///     <c>mobygames_raw</c> table in the source database (which uses VARCHAR(255) slugs as primary key).
    /// </summary>
    [Required]
    [StringLength(255)]
    public string Slug { get; set; }

    /// <summary>
    ///     MobyGames numeric game ID extracted from the post-2024 URL pattern <c>/game/{numericId}/{slug}/</c>.
    ///     Used by the per-game raw fetcher to build sub-page URLs.
    /// </summary>
    public int NumericId { get; set; }

    /// <summary>Game title as shown on the index page card. Informational only.</summary>
    [StringLength(512)]
    public string Title { get; set; }

    /// <summary>
    ///     Calendar year the game was filed under in the yearly search (matches the <c>from:Y/until:Y</c>
    ///     URL window the entry was discovered through).
    /// </summary>
    public int ReleaseYear { get; set; }

    /// <summary>Primary developer/publisher string shown on the index card. Informational only.</summary>
    [StringLength(512)]
    public string Developer { get; set; }

    /// <summary>UTC timestamp of the first time this slug was encountered by a yearly index crawl.</summary>
    public DateTime FirstDiscoveredAt { get; set; }

    /// <summary>UTC timestamp of the most recent yearly index crawl that re-encountered this slug.</summary>
    public DateTime LastSeenAt { get; set; }

    /// <summary>
    ///     UTC timestamp the per-game raw fetcher completed (successfully or after permanent failure) for
    ///     this slug. NULL means still pending fetch. The <c>scrape-new-games</c> command queries rows with
    ///     <c>RawFetchedAt IS NULL AND ErrorCount &lt; max</c>.
    /// </summary>
    public DateTime? RawFetchedAt { get; set; }

    /// <summary>Most recent fetch error (HTTP failure, network error). NULL on success.</summary>
    [StringLength(1024)]
    public string FetchError { get; set; }

    /// <summary>
    ///     Number of consecutive fetch failures. The fetcher gives up on a slug once this exceeds the
    ///     command-line retry cap (default 5).
    /// </summary>
    public int ErrorCount { get; set; }

    /// <summary>
    ///     Non-NULL when the fetcher decided to skip this row instead of fetching, e.g. when the slug
    ///     already exists in the <c>mobygames_raw</c> dump (<c>"already-in-raw"</c>).
    /// </summary>
    [StringLength(64)]
    public string SkippedReason { get; set; }
}
