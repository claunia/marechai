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

/// <summary>
///     Per-image metadata supplied at admin software-screenshot batch-commit time. The
///     pending image was previously staged via
///     <c>POST /software/screenshots/admin/pending</c> and its server-side guid is the
///     <see cref="PendingId" /> here.
/// </summary>
public class AdminSoftwareScreenshotBatchCommitItemDto
{
    [JsonPropertyName("pending_id")]
    [Required]
    public Guid PendingId { get; set; }

    /// <summary>Optional per-image canonical English caption written to <c>SoftwareScreenshot.Caption</c>.</summary>
    [JsonPropertyName("caption")]
    public string? Caption { get; set; }
}

/// <summary>
///     Request payload for the software-screenshot admin batch-commit endpoint. The
///     caller has staged 1..50 pending images and now wants every one of them promoted
///     into a permanent <c>SoftwareScreenshot</c> row plus the standard 8-variant
///     conversion run, with progress reported back via the batch-status polling endpoint.
///     A single <see cref="SoftwarePlatformId" /> and <see cref="CanonicalGroupName" />
///     (both optional) apply to every image in the batch; per-card overrides remain
///     available in the admin grid after upload.
/// </summary>
public class AdminSoftwareScreenshotBatchCommitRequestDto
{
    [JsonPropertyName("software_id")]
    [Required]
    public int SoftwareId { get; set; }

    /// <summary>Optional shared platform applied to every screenshot in the batch.</summary>
    [JsonPropertyName("software_platform_id")]
    public int? SoftwarePlatformId { get; set; }

    /// <summary>
    ///     Optional shared software version applied to every screenshot in the batch.
    ///     Server validates that the version belongs to <see cref="SoftwareId" /> and
    ///     assigns its id to <c>SoftwareScreenshot.SoftwareVersionId</c> on every row.
    ///     Null leaves the version FK null on every screenshot.
    /// </summary>
    [JsonPropertyName("software_version_id")]
    public int? SoftwareVersionId { get; set; }

    /// <summary>
    ///     Optional shared canonical English group name applied to every screenshot in
    ///     the batch. Server resolves-or-creates the corresponding
    ///     <c>SoftwareScreenshotGroup</c> row once and assigns the resulting id to every
    ///     screenshot. Null / empty leaves <c>GroupId</c> null for every screenshot.
    /// </summary>
    [JsonPropertyName("canonical_group_name")]
    public string? CanonicalGroupName { get; set; }

    [JsonPropertyName("items")]
    [Required]
    public required List<AdminSoftwareScreenshotBatchCommitItemDto> Items { get; set; }
}
