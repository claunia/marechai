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
///     Pool of unique strings that appear as either <see cref="SoftwareAttribute.Key" /> or
///     <see cref="SoftwareAttribute.Value" /> across non-Rating attribute rows. Keys and values share
///     the same pool — the same text is stored once even if it appeared as both. The
///     <c>SoftwareAttributeTranslationCache</c> normalizes incoming text (NBSP → space, trim) before
///     pool lookup so MobyGames-imported rows merge with hand-curated rows. Default MariaDB
///     <c>utf8mb4_general_ci</c> collation is intentional — case variants like <c>"Color"</c> and
///     <c>"color"</c> merge into one pool entry.
/// </summary>
public class SoftwareAttributeString : BaseModel<int>
{
    [StringLength(512)]
    [Required]
    public string Text { get; set; }
}
