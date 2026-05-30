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
///     Per-image metadata supplied at admin sound-synth-photo batch-commit time. The
///     pending image was previously staged via
///     <c>POST /sound-synths/photos/admin/pending</c> and its server-side guid is the
///     <see cref="PendingId" /> here.
/// </summary>
public class AdminSoundSynthPhotoBatchCommitItemDto
{
    [JsonPropertyName("pending_id")]
    [Required]
    public Guid PendingId { get; set; }

    /// <summary>Optional source URL recorded on the resulting <c>SoundSynthPhoto.Source</c> column.</summary>
    [JsonPropertyName("source")]
    public string? Source { get; set; }
}

/// <summary>
///     Request payload for the sound-synth-photo admin batch-commit endpoint. The caller
///     has staged 1..25 pending images and now wants every one of them promoted into a
///     permanent <c>SoundSynthPhoto</c> row plus the standard 8-variant conversion run,
///     with progress reported back via the batch-status polling endpoint. A single
///     <see cref="LicenseId" /> applies to every image in the batch.
/// </summary>
public class AdminSoundSynthPhotoBatchCommitRequestDto
{
    [JsonPropertyName("sound_synth_id")]
    [Required]
    public int SoundSynthId { get; set; }

    [JsonPropertyName("license_id")]
    [Required]
    public int LicenseId { get; set; }

    [JsonPropertyName("items")]
    [Required]
    public required List<AdminSoundSynthPhotoBatchCommitItemDto> Items { get; set; }
}
