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

public class SoftwareUserReviewDto : BaseDto<long>
{
    [JsonPropertyName("user_id")]
    public string? UserId { get; set; }
    [JsonPropertyName("user_name")]
    public string? UserName { get; set; }
    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }
    [JsonPropertyName("avatar_url")]
    public string? AvatarUrl { get; set; }
    [JsonPropertyName("software_id")]
    public ulong SoftwareId { get; set; }
    [JsonPropertyName("software_name")]
    public string? SoftwareName { get; set; }
    [JsonPropertyName("the_good")]
    public string? TheGood { get; set; }
    [JsonPropertyName("the_bad")]
    public string? TheBad { get; set; }
    [JsonPropertyName("the_ugly")]
    public string? TheUgly { get; set; }
    [JsonPropertyName("is_anonymous")]
    public bool IsAnonymous { get; set; }
    [JsonPropertyName("rating")]
    public float? Rating { get; set; }
    [JsonPropertyName("thumbs_up")]
    public int ThumbsUp { get; set; }
    [JsonPropertyName("thumbs_down")]
    public int ThumbsDown { get; set; }
    [JsonPropertyName("current_user_vote")]
    public bool? CurrentUserVote { get; set; }
    [JsonPropertyName("report_count")]
    public int ReportCount { get; set; }
    [JsonPropertyName("created_on")]
    public DateTime CreatedOn { get; set; }
    [JsonPropertyName("updated_on")]
    public DateTime UpdatedOn { get; set; }
    [JsonPropertyName("is_collaborator")]
    public bool IsCollaborator { get; set; }
}
