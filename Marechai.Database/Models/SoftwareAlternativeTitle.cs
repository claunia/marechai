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
///     A regional, script, or otherwise alternative title for a <see cref="Software" /> (e.g. a
///     Japanese Kanji/Romaji title alongside the Western release name). <see cref="Comment" /> is
///     a short English explanation of why the alternative title exists; it is translated into the
///     other UI languages via <see cref="SoftwareAlternativeTitleCommentTranslation" /> by the
///     background <c>TranslationWorker</c>.
/// </summary>
public class SoftwareAlternativeTitle : BaseModel<long>
{
    public ulong SoftwareId { get; set; }

    [Required]
    [StringLength(255)]
    public string Title { get; set; }

    [StringLength(500)]
    public string Comment { get; set; }

    public virtual Software Software { get; set; }
}
