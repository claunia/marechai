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

public class ResolutionByGpuDto : BaseDto<long>
{
    [JsonPropertyName("resolution_id")]
    [Required]
    public int ResolutionId { get; set; }
    [JsonPropertyName("gpu_id")]
    [Required]
    public int GpuId { get; set; }

    /// <summary>
    /// The nested resolution payload. Marked <c>[Required]</c> on a non-nullable
    /// reference type so the OpenAPI schema emits a direct <c>$ref</c> to
    /// <c>ResolutionDto</c>. Without <c>[Required]</c> ASP.NET emits
    /// <c>oneOf:[{type:null},{$ref:...}]</c>, which makes Kiota generate a
    /// composed-type wrapper class whose discriminator-based factory cannot
    /// populate the inner DTO from our plain JSON, silently leaving it null on
    /// the client and forcing per-resolution N+1 fallback fetches in the page.
    /// See pattern in <see cref="BookFullDto.Book"/>. The wire value can still
    /// legitimately be JSON null at runtime when the join is missing.
    /// </summary>
    [JsonPropertyName("resolution")]
    [Required]
    public ResolutionDto Resolution { get; set; } = null!;
}