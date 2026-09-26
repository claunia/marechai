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
using Marechai.Data;

namespace Marechai.Database.Models;

/// <summary>
///     Records that a given per-game media <see cref="Pass" /> has already visited a given
///     MobyGames game, so the next run of that pass can skip it.
///     <para>
///         Without this the media passes restart from the head of the catalogue on every run:
///         they order by slug and take the first N, so with ~150K imported games they re-check
///         the same alphabetical head forever and never reach the rest. The per-URL state tables
///         (covers, promo art, screenshots, videos) cannot serve this purpose because a game with
///         no media of that kind never gets a row in them, and so would be re-checked eternally.
///     </para>
/// </summary>
public class MobyGamesMediaPassState : BaseModel<long>
{
    [Required]
    [StringLength(64)]
    public string MobyGameId { get; set; }

    [Required]
    public MobyGamesMediaPass Pass { get; set; }

    public DateTime ProcessedOn { get; set; }
}
