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
using System.Text.Json.Serialization;

namespace Marechai.Data.Dtos;

public class CriticReviewSummaryDto
{
    [JsonPropertyName("average_score")]
    public double? AverageScore { get; set; }
    [JsonPropertyName("total_reviews")]
    public int TotalReviews { get; set; }
    [JsonPropertyName("by_platform")]
    public List<PlatformReviewSummaryDto> ByPlatform { get; set; }
}

public class PlatformReviewSummaryDto
{
    [JsonPropertyName("platform_id")]
    public ulong? PlatformId { get; set; }
    [JsonPropertyName("platform_name")]
    public string PlatformName { get; set; }
    [JsonPropertyName("average_score")]
    public double? AverageScore { get; set; }
    [JsonPropertyName("review_count")]
    public int ReviewCount { get; set; }
}
