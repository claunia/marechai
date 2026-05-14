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
///     One translated <see cref="PeopleBySoftware.Role" /> per
///     (<see cref="RoleText" />, <see cref="LanguageCode" />) pair. The table is a flat string
///     pool keyed by the canonical English role text — there is NO foreign key to
///     <see cref="PeopleBySoftware" />, multiple credits sharing the same role (e.g.
///     "Programmer", "Composer") reuse the same translation row. English (<c>eng</c>) is
///     treated as the identity copy of the canonical role and is NEVER stored here — read
///     endpoints fall back to the parent <see cref="PeopleBySoftware.Role" /> column when no
///     translation row exists for the requested language. The background
///     <c>TranslationWorker</c> populates this table by calling OpenAI / NLLB; rows are
///     append-only. NBSP (<c>U+00A0</c>) characters present in the canonical role text are
///     preserved on the <see cref="RoleText" /> key, but are stripped at the
///     <c>TranslationService.TranslateAsync</c> entry point before the wire is sent to the
///     translation backend.
/// </summary>
public class PeopleBySoftwareRoleTranslation : BaseModel<int>
{
    [StringLength(256)]
    [Required]
    public string RoleText { get; set; }

    [StringLength(3)]
    [Required]
    public string LanguageCode { get; set; }

    [StringLength(256)]
    [Required]
    public string Translation { get; set; }

    public virtual Iso639 Language { get; set; }
}
