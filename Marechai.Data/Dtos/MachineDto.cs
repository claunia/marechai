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

public class MachineDto : BaseDto<int>
{
    [JsonPropertyName("name")]
    [Required]
    public required string Name { get; set; }

    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("company_id")]
    public int CompanyId { get; set; }

    [JsonPropertyName("company_logo")]
    public Guid? CompanyLogo { get; set; }

    [JsonPropertyName("introduced")]
    public DateTime? Introduced { get; set; }
    [JsonPropertyName("introduced_precision")]
    public DatePrecision IntroducedPrecision { get; set; }

    [JsonPropertyName("prototype")]
    public bool Prototype { get; set; }

    [JsonPropertyName("family_id")]
    public int? FamilyId { get; set; }

    [JsonPropertyName("family_name")]
    public string? FamilyName { get; set; }

    public List<GpuDto>? Gpus { get; set; }

    public List<MemoryDto>? Memory { get; set; }

    public List<ProcessorDto>? Processors { get; set; }

    public List<SoundSynthDto>? SoundSynthesizers { get; set; }

    public List<StorageDto>? Storage { get; set; }

    [JsonPropertyName("company")]
    public string? Company { get; set; }

    [JsonPropertyName("type")]
    public MachineType Type { get; set; }

    [JsonPropertyName("family")]
    public string? Family { get; set; }

    [JsonIgnore]
    public string IntroducedView =>
        Prototype ? "Prototype" : Introduced?.ToShortDateString() ?? "Unknown";
}