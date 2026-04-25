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

public abstract class DocumentBaseDto : BaseDto<long>
{
    [JsonPropertyName("title")]
    [Required]
    public required string Title { get; set; }
    [JsonPropertyName("native_title")]
    public string? NativeTitle { get; set; }
    [JsonPropertyName("sort_title")]
    public string? SortTitle { get; set; }
    [JsonPropertyName("published")]
    public DateTime? Published { get; set; }
    [JsonPropertyName("published_precision")]
    public DatePrecision PublishedPrecision { get; set; }
    [JsonPropertyName("country_id")]
    public short? CountryId { get; set; }
    [JsonPropertyName("country")]
    public string? Country { get; set; }
}