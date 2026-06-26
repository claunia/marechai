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

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Marechai.Database.Models;

/// <summary>
///     A website that catalogs software and can be linked to from a <see cref="Software" /> entry
///     (e.g. MobyGames, IGDB). <see cref="Name" /> is a free-text site name, kept unique so it can be
///     used as a stable reference from <see cref="SoftwareExternalId" />. <see cref="UrlTemplate" />
///     is an optional pattern (e.g. <c>"https://www.mobygames.com/game/{id}"</c>) for building a link
///     out of a stored external ID; rendering it is handled by a later phase.
/// </summary>
public class ExternalSite : BaseModel<long>
{
    [Required]
    [StringLength(128)]
    public string Name { get; set; }

    [StringLength(512)]
    public string UrlTemplate { get; set; }

    public virtual ICollection<SoftwareExternalId> SoftwareExternalIds { get; set; }
}
