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
///     The identifier a <see cref="Software" /> entry has on a given <see cref="ExternalSite" />
///     (e.g. its MobyGames slug or IGDB numeric ID). The pair (<see cref="ExternalSiteId" />,
///     <see cref="ExternalId" />) is unique, preventing the same external identifier from being
///     registered twice for the same site. This is a first-class, general-purpose record kept
///     alongside (not instead of) the site-specific mirror/match tables such as
///     <see cref="MobyGamesImportState" /> and <see cref="IgdbGame" />.
/// </summary>
public class SoftwareExternalId : BaseModel<long>
{
    public ulong SoftwareId { get; set; }

    public virtual Software Software { get; set; }

    public long ExternalSiteId { get; set; }

    public virtual ExternalSite ExternalSite { get; set; }

    [Required]
    [StringLength(255)]
    public string ExternalId { get; set; }
}
