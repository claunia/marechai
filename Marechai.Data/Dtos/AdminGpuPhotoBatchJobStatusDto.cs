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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Marechai.Data.Dtos;

public class AdminGpuPhotoBatchJobItemResultDto
{
    [JsonPropertyName("pending_id")]
    [Required]
    public Guid PendingId { get; set; }

    [JsonPropertyName("succeeded")]
    public bool Succeeded { get; set; }

    [JsonPropertyName("photo_id")]
    public Guid? PhotoId { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }
}

/// <summary>
///     Polling response for an in-flight GPU-photo admin batch-commit job. The client
///     polls <c>GET /gpus/photos/admin/batch/{jobId}/status</c> every ~750 ms until
///     <see cref="State" /> is <see cref="BatchJobState.Completed" /> or
///     <see cref="BatchJobState.Failed" />.
/// </summary>
public class AdminGpuPhotoBatchJobStatusDto
{
    [JsonPropertyName("job_id")]
    [Required]
    public Guid JobId { get; set; }

    [JsonPropertyName("state")]
    [Required]
    public int State { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("processed")]
    public int Processed { get; set; }

    /// <summary>Pending id of the image being processed right now, when applicable.</summary>
    [JsonPropertyName("current_pending_id")]
    public Guid? CurrentPendingId { get; set; }

    [JsonPropertyName("results")]
    public List<AdminGpuPhotoBatchJobItemResultDto> Results { get; set; } = new();
}
