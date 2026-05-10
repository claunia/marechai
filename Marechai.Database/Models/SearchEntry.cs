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
using Marechai.Data;

namespace Marechai.Database.Models;

/// <summary>
///     Denormalized index row used by the site-wide fuzzy search.
///     One row per searchable entity (Company, Machine, Book, etc.).
///     Maintained by the migration seed (initial population) and by
///     <see cref="Marechai.Database.Interceptors.SearchIndexInterceptor"/> for live updates.
/// </summary>
public class SearchEntry : BaseModel<long>
{
    [Required]
    public SearchEntityType EntityType { get; set; }

    /// <summary>The PK of the source row, coerced to long (covers int / ulong / long ranges).</summary>
    [Required]
    public long EntityId { get; set; }

    /// <summary>Primary user-facing label (e.g. Company.Name, Machine.Name, Book.Title).</summary>
    [Required]
    [StringLength(512)]
    public string DisplayName { get; set; }

    /// <summary>Secondary label (e.g. Company.LegalName, Person.Alias, DocumentBase.NativeTitle / SortTitle).</summary>
    [StringLength(512)]
    public string AltName { get; set; }

    /// <summary>Lowercased + diacritics-stripped + whitespace-normalized concatenation of DisplayName + AltName. FULLTEXT-indexed (ngram parser).</summary>
    [Required]
    [StringLength(1024)]
    public string NormalizedName { get; set; }

    /// <summary>Year (introduced / founded / published) for filtering. Null when unknown.</summary>
    public int? Year { get; set; }

    /// <summary>Country FK (Iso31661Numeric.NumCode) for country filtering. Null when N/A.</summary>
    public short? CountryId { get; set; }

    /// <summary>Primary company FK for company filtering (Machine, Gpu, Processor, SoundSynth, SoftwareRelease publisher). Null when N/A.</summary>
    public int? CompanyId { get; set; }

    /// <summary>True when the source row has at least one cover/photo/logo image.</summary>
    public bool HasImage { get; set; }

    /// <summary>
    ///     Numeric value of <see cref="SoftwareKind"/> when this row indexes a Software (or a
    ///     SoftwareCompilation, where the value is forced to <c>Game</c>). NULL for all other entity types.
    /// </summary>
    public byte? Kind { get; set; }

    /// <summary>MariaDB SOUNDEX(DisplayName) cached for cheap phonetic prefilter (max 10 chars).</summary>
    [StringLength(10)]
    public string Soundex { get; set; }
}
