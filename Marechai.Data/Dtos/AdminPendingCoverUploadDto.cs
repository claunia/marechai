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

/// <summary>
///     Response payload for a successful admin batch-upload staging POST. The server has
///     content-sniffed the image with ImageMagick, accepted it into the pending folder, and
///     generated a 256x256 thumbnail returned inline as a data URL so the dialog can render
///     a preview without a second round-trip.
/// </summary>
public class AdminPendingCoverUploadDto
{
    [JsonPropertyName("id")]
    [Required]
    public Guid Id { get; set; }

    /// <summary>Lower-case file extension WITHOUT leading dot (e.g. <c>"jpg"</c>, <c>"webp"</c>, <c>"avif"</c>).</summary>
    [JsonPropertyName("extension")]
    [Required]
    public required string Extension { get; set; }

    /// <summary>Base64-encoded JPEG data URL of the 256x256 thumbnail (aspect ratio preserved).</summary>
    [JsonPropertyName("thumbnail_base64")]
    [Required]
    public required string ThumbnailBase64 { get; set; }

    [JsonPropertyName("size_bytes")]
    public long SizeBytes { get; set; }

    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }
}
