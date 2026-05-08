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
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Marechai.Data.Dtos;

/// <summary>
/// Consolidated payload for the public /book/{Id} view page. Returns the book head plus
/// previous-edition / source-book references (id+title only — page only renders the link),
/// the language-aware synopsis (with English fallback collapsed into a single ordered query),
/// and all four child collections (people, companies, machines, machine families) in one HTTP
/// response so the frontend can replace 6–8 sequential round-trips with one call.
/// </summary>
public class BookFullDto
{
    // [Required] is needed in addition to the non-nullable type so the generated
    // OpenAPI schema is a plain $ref instead of `oneOf:[null, $ref]`. With the
    // oneOf-with-null shape Kiota generates a "composed type wrapper" that
    // requires a discriminator field that doesn't exist in the JSON, leaving the
    // inner DTO un-populated. See pattern in SoundSynthVideoDto.Provider/VideoId.
    [JsonPropertyName("book")]
    [Required]
    public BookDto Book { get; set; }

    /// <summary>Title of the previous-edition book referenced by <see cref="BookDto.PreviousId"/>, or null.</summary>
    [JsonPropertyName("previous_book_title")]
    public string? PreviousBookTitle { get; set; }

    /// <summary>Title of the source book referenced by <see cref="BookDto.SourceId"/>, or null.</summary>
    [JsonPropertyName("source_book_title")]
    public string? SourceBookTitle { get; set; }

    /// <summary>
    /// Synopsis in the requested language with English fallback already applied
    /// server-side. May be null at runtime when the book has no synopsis at all.
    /// Marked <c>[Required]</c> only so the OpenAPI schema emits a direct $ref
    /// (avoiding Kiota's broken composed-type wrapper for <c>oneOf:[null,$ref]</c>);
    /// the wire value can still legitimately be JSON null.
    /// </summary>
    [JsonPropertyName("synopsis")]
    [Required]
    public DocumentSynopsisDto Synopsis { get; set; }

    [JsonPropertyName("people")]
    public List<PersonByBookDto> People { get; set; } = new();

    [JsonPropertyName("companies")]
    public List<CompanyByBookDto> Companies { get; set; } = new();

    [JsonPropertyName("machines")]
    public List<BookByMachineDto> Machines { get; set; } = new();

    [JsonPropertyName("machine_families")]
    public List<BookByMachineFamilyDto> MachineFamilies { get; set; } = new();
}
