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

namespace Marechai.Database.Models;

/// <summary>
///     One translated copy of a <see cref="SoftwareAlternativeTitle.Comment" /> per
///     (<see cref="CommentText" />, <see cref="LanguageCode" />) pair. String-pool keyed by the
///     comment text itself (not by a parent FK) so multiple alternative titles sharing the same
///     comment (e.g. "Japanese title") reuse a single translation row per language, mirroring
///     <see cref="SoftwareCoverCaptionTranslation" />. English (<c>eng</c>) is the identity copy
///     and is never stored here.
/// </summary>
public class SoftwareAlternativeTitleCommentTranslation : BaseModel<int>
{
    [StringLength(500)]
    [Required]
    public string CommentText { get; set; }

    [StringLength(3)]
    [Required]
    public string LanguageCode { get; set; }

    [StringLength(500)]
    [Required]
    public string Translation { get; set; }

    public virtual Iso639 Language { get; set; }
}
