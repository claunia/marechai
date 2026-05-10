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

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Marechai.Data.Dtos;

/// <summary>Body of POST /search/results — used by the results page and the advanced-search form.</summary>
public class SearchRequestDto
{
    [JsonPropertyName("query")]
    public string Query { get; set; } = string.Empty;

    /// <summary>If non-empty, restrict results to these entity types (numeric <see cref="SearchEntityType"/> values).</summary>
    [JsonPropertyName("entity_types")]
    public int[]? EntityTypes { get; set; }

    [JsonPropertyName("year_from")]
    public int? YearFrom { get; set; }

    [JsonPropertyName("year_to")]
    public int? YearTo { get; set; }

    [JsonPropertyName("country_id")]
    public short? CountryId { get; set; }

    /// <summary>Post-filter: result.NormalizedName must contain this phrase.</summary>
    [JsonPropertyName("contains")]
    public string? Contains { get; set; }

    /// <summary>Post-filter: result.NormalizedName must NOT contain this phrase.</summary>
    [JsonPropertyName("not_contains")]
    public string? NotContains { get; set; }

    /// <summary>If true, only return rows whose normalized name contains the exact normalized query as a substring.</summary>
    [JsonPropertyName("exact_match")]
    public bool ExactMatch { get; set; }

    [JsonPropertyName("has_image")]
    public bool? HasImage { get; set; }

    /// <summary>Filter by first letter of NormalizedName. Use '#' for non-alphabetic.</summary>
    [JsonPropertyName("letter")]
    public string? Letter { get; set; }

    /// <summary>Filter by primary company FK (Machine/Gpu/Processor/SoundSynth/SoftwareCompilation publisher).</summary>
    [JsonPropertyName("company_id")]
    public int? CompanyId { get; set; }

    /// <summary>True → only rows with Year IS NOT NULL. False → only rows with Year IS NULL. Null → no constraint.</summary>
    [JsonPropertyName("year_known")]
    public bool? YearKnown { get; set; }

    /// <summary>True → only rows with CountryId IS NOT NULL. False → only rows with CountryId IS NULL. Null → no constraint.</summary>
    [JsonPropertyName("country_known")]
    public bool? CountryKnown { get; set; }

    /// <summary>
    ///     When set and the request is authenticated, intersect (true) or exclude (false) the user's collection.
    ///     Applies to Books, Documents, Magazines (via MagazineIssue collection), Software releases (via SoftwareCompilation),
    ///     Machines (any of Computer/Console/Smartphone via OwnedMachine).
    /// </summary>
    [JsonPropertyName("in_collection")]
    public bool? InCollection { get; set; }

    /// <summary>Optional SoftwareKind filter (forces entity-type to Software / SoftwareCompilation when set).</summary>
    [JsonPropertyName("software_kind")]
    public int? SoftwareKind { get; set; }

    /// <summary>
    ///     When set, restrict results to SoftwareCompilation rows that include the given Software id
    ///     (via the <c>SoftwareBySoftwareRelease</c> join table). Forces entity-type to SoftwareCompilation.
    /// </summary>
    [JsonPropertyName("includes_software_id")]
    public long? IncludesSoftwareId { get; set; }

    /// <summary>Sort order: "relevance" (default with query), "name_asc", "name_desc", "year_desc", "year_asc".</summary>
    [JsonPropertyName("sort")]
    public string? Sort { get; set; }

    [JsonPropertyName("skip")]
    public int Skip { get; set; }

    [JsonPropertyName("take")]
    public int Take { get; set; } = 50;
}
