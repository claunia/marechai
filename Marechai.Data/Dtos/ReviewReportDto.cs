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

public class ReviewReportDto : BaseDto<long>
{
    [JsonPropertyName("reporter_id")]
    public string ReporterId { get; set; }
    [JsonPropertyName("reporter_name")]
    public string ReporterName { get; set; }
    [JsonPropertyName("review_id")]
    public long ReviewId { get; set; }
    [JsonPropertyName("software_id")]
    public ulong SoftwareId { get; set; }
    [JsonPropertyName("software_name")]
    public string SoftwareName { get; set; }
    [JsonPropertyName("reviewer_name")]
    public string ReviewerName { get; set; }
    [JsonPropertyName("reason")]
    public ReviewReportReason Reason { get; set; }
    [JsonPropertyName("explanation")]
    public string Explanation { get; set; }
    [JsonPropertyName("is_resolved")]
    public bool IsResolved { get; set; }
    [JsonPropertyName("resolved_by_user_name")]
    public string ResolvedByUserName { get; set; }
    [JsonPropertyName("resolved_on")]
    public DateTime? ResolvedOn { get; set; }
    [JsonPropertyName("created_on")]
    public DateTime CreatedOn { get; set; }
}
