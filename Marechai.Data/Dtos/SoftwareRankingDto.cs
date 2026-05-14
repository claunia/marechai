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
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Marechai.Data.Dtos;

public class SoftwareRankingDto
{
    [JsonPropertyName("rank")]
    public int Rank { get; set; }
    [JsonPropertyName("software_id")]
    public ulong SoftwareId { get; set; }
    [JsonPropertyName("name")]
    [Required]
    public required string Name { get; set; }
    [JsonPropertyName("kind")]
    public SoftwareKind Kind { get; set; }
    [JsonPropertyName("front_cover_id")]
    public Guid? FrontCoverId { get; set; }
    [JsonPropertyName("family")]
    public string? Family { get; set; }
    [JsonPropertyName("marechai_score")]
    public double MarechaiScore { get; set; }
    [JsonPropertyName("critic_average")]
    public double? CriticAverage { get; set; }
    [JsonPropertyName("user_star_average")]
    public double? UserStarAverage { get; set; }
    [JsonPropertyName("critic_review_count")]
    public int CriticReviewCount { get; set; }
    [JsonPropertyName("user_rating_count")]
    public int UserRatingCount { get; set; }
}
