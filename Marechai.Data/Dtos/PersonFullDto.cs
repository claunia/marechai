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
/// Consolidated payload for the public /person/{Id} view page. Returns the
/// person head plus all five child collections (companies, books, documents,
/// magazines, software credits) in a single HTTP response so the frontend can
/// replace 6 sequential round-trips with one call.
/// </summary>
public class PersonFullDto
{
    // [Required] is needed in addition to the non-nullable type so the
    // generated OpenAPI schema is a plain $ref instead of `oneOf:[null, $ref]`.
    // With the oneOf-with-null shape Kiota generates a "composed type wrapper"
    // that requires a discriminator field that doesn't exist in the JSON,
    // leaving the inner DTO un-populated. Same pattern as BookFullDto.Book.
    [JsonPropertyName("person")]
    [Required]
    public PersonDto Person { get; set; }

    [JsonPropertyName("companies")]
    public List<PersonByCompanyDto> Companies { get; set; } = new();

    [JsonPropertyName("books")]
    public List<PersonByBookDto> Books { get; set; } = new();

    [JsonPropertyName("documents")]
    public List<PersonByDocumentDto> Documents { get; set; } = new();

    [JsonPropertyName("magazines")]
    public List<PersonByMagazineDto> Magazines { get; set; } = new();

    [JsonPropertyName("software_credits")]
    public List<PersonBySoftwareDto> SoftwareCredits { get; set; } = new();
}
