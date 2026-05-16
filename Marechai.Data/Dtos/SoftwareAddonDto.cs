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

/// <summary>
///     Row projection for the /admin/software/orphan-addons page. Surfaces both
///     <see cref="SoftwareKind.Dlc"/> rows with a broken/unset base software link and
///     <see cref="SoftwareKind.Game"/> rows that carry a "DLC / add-on" genre
///     (misclassified at import time). The <see cref="OrphanReason"/> column drives
///     the chip rendered in the grid.
/// </summary>
public class SoftwareAddonDto : BaseDto<ulong>
{
    /// <summary>
    ///     Display name of the add-on / DLC row.
    /// </summary>
    /// <remarks>
    ///     Marked <c>[Required]</c> so the OpenAPI schema emits a direct string property
    ///     and Kiota does not generate a composed-type wrapper around it.
    /// </remarks>
    [JsonPropertyName("name")]
    [Required]
    public required string Name { get; set; }

    /// <summary>
    ///     The DLC's actual <see cref="SoftwareKind"/> as stored in the DB. May be
    ///     <see cref="SoftwareKind.Game"/> when <see cref="OrphanReason"/> is
    ///     <see cref="AddonOrphanReason.MisclassifiedAsGame"/>; the link operation
    ///     flips this to <see cref="SoftwareKind.Dlc"/>.
    /// </summary>
    [JsonPropertyName("kind")]
    public SoftwareKind Kind { get; set; }

    /// <summary>
    ///     True when the row has at least one genre matching the "DLC / add-on" / "Add-on"
    ///     predicate. Always true for <see cref="AddonOrphanReason.MisclassifiedAsGame"/>;
    ///     informational on regular DLCs.
    /// </summary>
    [JsonPropertyName("has_dlc_genre")]
    public bool HasDlcGenre { get; set; }

    /// <summary>
    ///     Current value of <c>Software.BaseSoftwareId</c>; null when no link is set.
    /// </summary>
    [JsonPropertyName("base_software_id")]
    public ulong? BaseSoftwareId { get; set; }

    /// <summary>
    ///     Display name of the current base software. Null when no link is set or
    ///     when the linked row no longer exists (dangling FK).
    /// </summary>
    [JsonPropertyName("base_software_name")]
    public string? BaseSoftwareName { get; set; }

    /// <summary>
    ///     Why this row is on the orphan list. <see cref="AddonOrphanReason.Linked"/>
    ///     only appears when the page is showing all add-ons (not just orphans).
    /// </summary>
    /// <remarks>
    ///     The base software's own <c>Kind</c> was intentionally not exposed here: a
    ///     nullable enum property would trigger ASP.NET's OpenAPI emitter to produce
    ///     a <c>oneOf:[null,$ref]</c> schema, which Kiota turns into a broken
    ///     composed-type wrapper. The "chained DLC" case is already encoded in this
    ///     reason enum, so the UI never needs the parent's kind separately.
    /// </remarks>
    [JsonPropertyName("orphan_reason")]
    public AddonOrphanReason OrphanReason { get; set; }
}
