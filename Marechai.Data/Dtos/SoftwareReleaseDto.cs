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

public class SoftwareReleaseDto : BaseDto<ulong>
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }
    [JsonPropertyName("is_compilation")]
    public bool IsCompilation { get; set; }
    [JsonPropertyName("software_id")]
    public ulong? SoftwareId { get; set; }
    [JsonPropertyName("software")]
    public string? Software { get; set; }
    [JsonPropertyName("software_version_id")]
    public ulong? SoftwareVersionId { get; set; }
    [JsonPropertyName("software_version")]
    public string? SoftwareVersion { get; set; }
    [JsonPropertyName("platform_id")]
    public ulong? PlatformId { get; set; }
    [JsonPropertyName("platform")]
    public string? Platform { get; set; }
    [JsonPropertyName("regions")]
    public List<UnM49BySoftwareReleaseDto>? Regions { get; set; }
    [JsonPropertyName("languages")]
    public List<LanguageBySoftwareReleaseDto>? Languages { get; set; }
    [JsonPropertyName("publisher_id")]
    [Required]
    public int PublisherId { get; set; }
    [JsonPropertyName("publisher")]
    public string? Publisher { get; set; }
    [JsonPropertyName("release_date")]
    public DateTime? ReleaseDate { get; set; }
    [JsonPropertyName("release_date_precision")]
    public DatePrecision ReleaseDatePrecision { get; set; }
}
