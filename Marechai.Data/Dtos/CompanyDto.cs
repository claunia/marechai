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

public class CompanyDto : BaseDto<int>
{
    [JsonPropertyName("name")]
    [Required]
    public required string Name { get; set; }
    [JsonPropertyName("founded")]
    public DateTime? Founded { get; set; }
    [JsonPropertyName("website")]
    public string? Website { get; set; }
    [JsonPropertyName("twitter")]
    public string? Twitter { get; set; }
    [JsonPropertyName("facebook")]
    public string? Facebook { get; set; }
    [JsonPropertyName("sold")]
    public DateTime? Sold { get; set; }
    [JsonPropertyName("sold_to_id")]
    public int? SoldToId { get; set; }
    [JsonPropertyName("address")]
    public string? Address { get; set; }
    [JsonPropertyName("city")]
    public string? City { get; set; }
    [JsonPropertyName("province")]
    public string? Province { get; set; }
    [JsonPropertyName("postal_code")]
    public string? PostalCode { get; set; }
    [JsonPropertyName("country_id")]
    public short? CountryId { get; set; }
    [JsonPropertyName("status")]
    public CompanyStatus Status { get; set; }
    [JsonPropertyName("last_logo")]
    public Guid? LastLogo { get; set; }
    [JsonPropertyName("sold_to")]
    public string? SoldTo { get; set; }
    [JsonPropertyName("country")]
    public string? Country { get; set; }
    [JsonPropertyName("founded_precision")]
    public DatePrecision FoundedPrecision { get; set; }
    [JsonPropertyName("sold_precision")]
    public DatePrecision SoldPrecision { get; set; }
    [JsonPropertyName("legal_name")]
    public string? LegalName { get; set; }
    [JsonIgnore]
    public string SoldView => Status != CompanyStatus.Active && Status != CompanyStatus.Unknown
                                  ? Sold?.ToShortDateString() ?? "Unknown"
                                  : Sold?.ToShortDateString() ?? (SoldTo is null ? "" : "Unknown");
}