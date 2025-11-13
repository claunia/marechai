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
// Copyright © 2003-2021 Natalia Portillo
*******************************************************************************/

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Marechai.Data.Dtos;

public class ProcessorDto : BaseDto<int>
{
    [JsonPropertyName("name")]
    [Required]
    public required string Name { get; set; }
    [JsonPropertyName("company")]
    public string? CompanyName { get; set; }
    [JsonPropertyName("model_code")]
    public string? ModelCode { get; set; }
    [JsonPropertyName("introduced")]
    public DateTime? Introduced { get; set; }
    [JsonPropertyName("speed")]
    public double? Speed { get; set; }
    [JsonPropertyName("package")]
    public string? Package { get; set; }
    [JsonPropertyName("gprs")]
    public int? Gprs { get; set; }
    [JsonPropertyName("gpr_size")]
    public int? GprSize { get; set; }
    [JsonPropertyName("fprs")]
    public int? Fprs { get; set; }
    [JsonPropertyName("fpr_size")]
    public int? FprSize { get; set; }
    [JsonPropertyName("cores")]
    public int? Cores { get; set; }
    [JsonPropertyName("threads_per_core")]
    public int? ThreadsPerCore { get; set; }
    [JsonPropertyName("process")]
    public string? Process { get; set; }
    [JsonPropertyName("process_nm")]
    public float? ProcessNm { get; set; }
    [JsonPropertyName("die_size")]
    public float? DieSize { get; set; }
    [JsonPropertyName("transistors")]
    public long? Transistors { get; set; }
    [JsonPropertyName("data_bus")]
    public int? DataBus { get; set; }
    [JsonPropertyName("address_bus")]
    public int? AddrBus { get; set; }
    [JsonPropertyName("simd_registers")]
    public int? SimdRegisters { get; set; }
    [JsonPropertyName("simd_size")]
    public int? SimdSize { get; set; }
    [JsonPropertyName("l1_instruction")]
    public float? L1Instruction { get; set; }
    [JsonPropertyName("l1_data")]
    public float? L1Data { get; set; }
    [JsonPropertyName("l2")]
    public float? L2 { get; set; }
    [JsonPropertyName("l3")]
    public float? L3 { get; set; }
    [JsonPropertyName("instruction_set")]
    public string? InstructionSet { get; set; }
    [JsonPropertyName("instruction_set_extensions")]
    public List<string>? InstructionSetExtensions { get; set; }
    [JsonPropertyName("company_id")]
    public int? CompanyId { get; set; }
    [JsonPropertyName("instruction_set_id")]
    public int? InstructionSetId { get; set; }
    [JsonIgnore]
    public string IntroducedView => Introduced?.ToShortDateString() ?? "Unknown";
}