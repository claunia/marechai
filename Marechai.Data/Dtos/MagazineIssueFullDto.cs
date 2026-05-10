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
/// Consolidated payload for the public /magazine/issue/{Id} view page. Returns the issue head
/// plus the parent magazine title (so the page header can link back without an extra round-trip)
/// and all three issue-level junction collections (machines, machine families, software) in one
/// HTTP response.
/// </summary>
public class MagazineIssueFullDto
{
    // [Required] is needed in addition to the non-nullable type so the generated
    // OpenAPI schema is a plain $ref instead of `oneOf:[null, $ref]`. Without it,
    // Kiota generates a "composed type wrapper" that requires a discriminator
    // field that doesn't exist in the JSON, leaving the inner DTO un-populated.
    [JsonPropertyName("issue")]
    [Required]
    public MagazineIssueDto Issue { get; set; }

    /// <summary>Title of the parent magazine, denormalized for the page header link.</summary>
    [JsonPropertyName("magazine_title")]
    public string? MagazineTitle { get; set; }

    [JsonPropertyName("machines")]
    public List<MagazineByMachineDto> Machines { get; set; } = new();

    [JsonPropertyName("machine_families")]
    public List<MagazineByMachineFamilyDto> MachineFamilies { get; set; } = new();

    [JsonPropertyName("software")]
    public List<MagazineBySoftwareDto> Software { get; set; } = new();
}
