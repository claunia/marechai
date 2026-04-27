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
using System.Text.Json.Serialization;

namespace Marechai.Data.Dtos;

public class CollectedMachineDto
{
    [JsonPropertyName("owned_machine_id")]
    public long OwnedMachineId { get; set; }

    [JsonPropertyName("machine_id")]
    public int MachineId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("company_name")]
    public string? CompanyName { get; set; }

    [JsonPropertyName("type")]
    public int? Type { get; set; }

    [JsonPropertyName("collected_on")]
    public DateTimeOffset CollectedOn { get; set; }

    [JsonPropertyName("acquisition_date")]
    public DateTimeOffset? AcquisitionDate { get; set; }

    [JsonPropertyName("acquisition_date_precision")]
    public int AcquisitionDatePrecision { get; set; }

    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("trade")]
    public bool Trade { get; set; }

    [JsonPropertyName("boxed")]
    public bool Boxed { get; set; }

    [JsonPropertyName("manuals")]
    public bool Manuals { get; set; }

    [JsonPropertyName("serial_number")]
    public string? SerialNumber { get; set; }

    [JsonPropertyName("serial_number_visible")]
    public bool SerialNumberVisible { get; set; }
}
