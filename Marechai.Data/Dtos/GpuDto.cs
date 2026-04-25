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

public class GpuDto : BaseDto<int>
{
    [JsonPropertyName("name")]
    [Required]
    public required string Name { get; set; }
    [JsonPropertyName("company_id")]
    public int? CompanyId { get; set; }
    [JsonPropertyName("company")]
    public string? Company { get; set; }
    [JsonPropertyName("model_code")]
    public string? ModelCode { get; set; }
    [JsonPropertyName("introduced")]
    public DateTime? Introduced { get; set; }
    [JsonPropertyName("introduced_precision")]
    public DatePrecision IntroducedPrecision { get; set; }
    [JsonPropertyName("package")]
    public string? Package { get; set; }
    [JsonPropertyName("process")]
    public string? Process { get; set; }
    [JsonPropertyName("process_nm")]
    public float? ProcessNm { get; set; }
    [JsonPropertyName("die_size")]
    public float? DieSize { get; set; }
    [JsonPropertyName("transistors")]
    public long? Transistors { get; set; }
    [JsonIgnore]
    public string IntroducedView => Introduced?.ToShortDateString() ?? "Unknown";
}