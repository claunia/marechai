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

public class SoftwareAlternativeTitleDto : BaseDto<long>
{
    [JsonPropertyName("software_id")]
    [Required]
    public ulong SoftwareId { get; set; }

    [JsonPropertyName("title")]
    [Required]
    public string Title { get; set; }

    /// <summary>
    ///     Comment shown to the end user. Localized to the requested language when the read
    ///     endpoint receives <c>?lang=</c> (or via the <c>Accept-Language</c> header) and a
    ///     translation row exists in <c>SoftwareAlternativeTitleCommentTranslations</c>; falls
    ///     back to the canonical English comment (= <see cref="CanonicalComment" />) otherwise.
    /// </summary>
    [JsonPropertyName("comment")]
    public string? Comment { get; set; }

    /// <summary>
    ///     The canonical English comment from <c>SoftwareAlternativeTitles.Comment</c>. Always
    ///     populated identically to the underlying column regardless of the requested language
    ///     so admin edit-path UIs can rewrite the source-of-truth value rather than a localized
    ///     copy.
    /// </summary>
    [JsonPropertyName("canonical_comment")]
    public string? CanonicalComment { get; set; }
}
