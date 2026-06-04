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
using System.Text.Json.Serialization;

namespace Marechai.Data.Dtos;

/// <summary>
///     One entry in the index of available Marechai rankings (<c>GET /software/rankings/index</c>).
///     The <see cref="Dimension" /> byte encodes which axis this ranking groups by:
///     <list type="bullet">
///         <item><c>0</c> — Overall (every eligible software). <see cref="DimensionId" /> is null.</item>
///         <item><c>1</c> — Per genre. <see cref="DimensionId" /> = <c>SoftwareGenre.Id</c>.</item>
///         <item><c>2</c> — Per platform. <see cref="DimensionId" /> = <c>SoftwarePlatform.Id</c>.</item>
///     </list>
///     <see cref="DimensionName" /> is the server-resolved, request-language-translated display
///     name for the grouping entity ("Overall" / translated genre name / raw platform name).
///     The numeric byte is deliberate: encoding as a typed enum would tickle the kiota
///     empty-class trap documented in the marechai-kiota memory.
/// </summary>
public class RankingIndexEntryDto
{
    [JsonPropertyName("id")]
    public uint Id { get; set; }

    [JsonPropertyName("dimension")]
    public byte Dimension { get; set; }

    [JsonPropertyName("dimension_id")]
    public long? DimensionId { get; set; }

    /// <summary>
    ///     Marked <see cref="RequiredAttribute" /> so the OpenAPI schema emits a plain
    ///     <c>string</c> instead of <c>oneOf:[null,string]</c> which would tickle the kiota
    ///     composed-type-wrapper trap on the client.
    /// </summary>
    [Required]
    [JsonPropertyName("dimension_name")]
    public string DimensionName { get; set; }

    [JsonPropertyName("entry_count")]
    public int EntryCount { get; set; }

    /// <summary>
    ///     For <see cref="Dimension" /> = 1 (Genre) only: the underlying
    ///     <c>SoftwareGenre.Type</c> byte (0=Genre, 2=Gameplay, 3=Setting, 4=Category;
    ///     Perspective=1 is intentionally excluded from rankings). Null for the
    ///     Overall and Platform dimensions. Lets the index page split the per-genre
    ///     rankings into "By gameplay" / "By genre" / "By setting" / "By category"
    ///     sub-sections without an extra round-trip.
    /// </summary>
    [JsonPropertyName("genre_type")]
    public byte? GenreType { get; set; }

    [JsonPropertyName("computed_at")]
    public DateTime ComputedAt { get; set; }
}
