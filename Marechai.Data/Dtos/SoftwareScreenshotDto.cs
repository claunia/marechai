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

public class SoftwareScreenshotDto : BaseDto<Guid>
{
    [JsonPropertyName("software_id")]
    [Required]
    public ulong SoftwareId { get; set; }

    [JsonPropertyName("software_name")]
    public string? SoftwareName { get; set; }

    [JsonPropertyName("software_platform_id")]
    public ulong? SoftwarePlatformId { get; set; }

    [JsonPropertyName("platform_name")]
    public string? PlatformName { get; set; }

    [JsonPropertyName("software_version_id")]
    public ulong? SoftwareVersionId { get; set; }

    [JsonPropertyName("version_string")]
    public string? VersionString { get; set; }

    /// <summary>
    ///     Caption shown to the end user. Localized to the requested language when the read
    ///     endpoint receives <c>?lang=</c> (or via the <c>Accept-Language</c> header) and a
    ///     translation row exists in <c>SoftwareScreenshotCaptionTranslations</c>; falls back to
    ///     the canonical English caption (= <see cref="CanonicalCaption" />) otherwise.
    /// </summary>
    [JsonPropertyName("caption")]
    public string? Caption { get; set; }

    /// <summary>
    ///     The canonical English caption from <c>SoftwareScreenshots.Caption</c>. Always populated
    ///     identically to the underlying column regardless of the requested language so admin
    ///     edit-path UIs can rewrite the source-of-truth value rather than a localized copy.
    /// </summary>
    [JsonPropertyName("canonical_caption")]
    public string? CanonicalCaption { get; set; }

    /// <summary>
    ///     Optional FK into <c>SoftwareScreenshotGroups</c>. Multiple screenshots in the same
    ///     software (or across the entire catalog) share the same row when they belong to the
    ///     same group, so the translation worker only renders each unique group name once per
    ///     supported language.
    /// </summary>
    [JsonPropertyName("group_id")]
    public int? GroupId { get; set; }

    /// <summary>
    ///     Display name of the screenshot group, localized to the requested language with fallback
    ///     to the canonical English value (= <see cref="CanonicalGroupName" />). Null when this
    ///     screenshot has no group assigned.
    /// </summary>
    [JsonPropertyName("group_name")]
    public string? GroupName { get; set; }

    /// <summary>
    ///     Canonical English name of the screenshot group as stored in
    ///     <c>SoftwareScreenshotGroups.Name</c>. Always populated identically to the underlying
    ///     column regardless of the requested language so admin / suggestion edit paths can
    ///     rewrite the source-of-truth value via get-or-create. Null when this screenshot has no
    ///     group assigned.
    /// </summary>
    [JsonPropertyName("canonical_group_name")]
    public string? CanonicalGroupName { get; set; }

    [JsonPropertyName("original_extension")]
    [Required]
    public string OriginalExtension { get; set; }
}
