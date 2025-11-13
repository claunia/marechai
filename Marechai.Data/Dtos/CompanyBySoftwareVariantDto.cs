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

using System.Text.Json.Serialization;

namespace Marechai.Data.Dtos;

public class CompanyBySoftwareVariantDto : BaseDto<ulong>
{
    [JsonPropertyName("company_id")]
    public int CompanyId { get; set; }
    [JsonPropertyName("software_variant_id")]
    public ulong SoftwareVariantId { get; set; }
    [JsonPropertyName("role_id")]
    public string RoleId { get; set; }
    [JsonPropertyName("company")]
    public string Company { get; set; }
    [JsonPropertyName("role")]
    public string Role { get; set; }
}