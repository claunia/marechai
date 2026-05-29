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

public class SoftwareScreenshotGroupDto : BaseDto<int>
{
    /// <summary>
    ///     Display name in the language requested by the caller (server resolves the language via
    ///     <c>?lang=</c> → <c>Accept-Language</c> → <c>"eng"</c>). Falls back to the canonical English
    ///     <see cref="CanonicalName" /> when no translation row exists for the requested language.
    /// </summary>
    [JsonPropertyName("name")]
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Canonical English name as stored in <c>SoftwareScreenshotGroups.Name</c>. Edit-path
    ///     autocompletes (admin uploader, suggestion dialog) display <see cref="Name" /> for the
    ///     user but submit <see cref="CanonicalName" /> back to the server so the get-or-create
    ///     keys on the same English row regardless of the user's locale.
    /// </summary>
    [JsonPropertyName("canonical_name")]
    [Required]
    public string CanonicalName { get; set; } = string.Empty;
}
