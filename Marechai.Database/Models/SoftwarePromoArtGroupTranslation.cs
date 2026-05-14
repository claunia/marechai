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
///     One translated <see cref="SoftwarePromoArtGroup.Name" /> per (<see cref="GroupId" />,
///     <see cref="LanguageCode" />) pair. English (<c>eng</c>) is treated as the identity copy of
///     <see cref="SoftwarePromoArtGroup.Name" /> and is NEVER stored here — read endpoints fall back
///     to the parent <c>Name</c> column when no translation row exists for the requested language.
///     The background <c>TranslationWorker</c> populates this table by calling OpenAI / NLLB; rows
///     are append-only.
/// </summary>
public class SoftwarePromoArtGroupTranslation : BaseModel<int>
{
    public int GroupId { get; set; }

    [StringLength(3)]
    [Required]
    public string LanguageCode { get; set; }

    [StringLength(256)]
    [Required]
    public string Name { get; set; }

    public virtual SoftwarePromoArtGroup Group    { get; set; }
    public virtual Iso639                Language { get; set; }
}
