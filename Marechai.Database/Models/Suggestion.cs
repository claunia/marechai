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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Marechai.Data;

namespace Marechai.Database.Models;

/// <summary>
///     A user-submitted suggestion to add or edit a catalog entity. Suggestions enter the
///     moderation queue (<see cref="SuggestionStatus.Pending" />) until reviewed by an admin,
///     who can accept individual fields. Reviewed rows are retained for audit history.
/// </summary>
public class Suggestion : BaseModel<long>
{
    /// <summary>The catalog entity type this suggestion targets.</summary>
    public SuggestionEntityType EntityType { get; set; }

    /// <summary>
    ///     ID of the targeted entity. <c>null</c> means a brand-new entity is being suggested
    ///     (reserved for a future "additions" feature; the pilot only supports edits).
    /// </summary>
    public long? EntityId { get; set; }

    /// <summary>
    ///     Optional discriminator within an entity for per-sub-key suggestions (e.g. the
    ///     ISO-639-3 language code on a <see cref="SuggestionEntityType.CompanyDescription" />
    ///     suggestion). Used together with <see cref="EntityType" /> and <see cref="EntityId" />
    ///     for dedupe and review lookups. <c>null</c> for entity types that do not need it.
    /// </summary>
    [StringLength(32)]
    public string Subkey { get; set; }

    /// <summary>Current lifecycle status of the suggestion.</summary>
    public SuggestionStatus Status { get; set; }

    /// <summary>Suggesting user. Set by the controller from the authenticated principal.</summary>
    [Required]
    public string CreatedById { get; set; }

    /// <summary>Reviewing admin (null while Pending).</summary>
    public string ReviewedById { get; set; }

    /// <summary>Timestamp the suggestion entered a terminal state (null while Pending).</summary>
    public DateTime? ReviewedOn { get; set; }

    /// <summary>Optional free-text comment from the suggesting user explaining the change.</summary>
    [MaxLength(2000)]
    public string UserComment { get; set; }

    /// <summary>
    ///     Optional free-text comment from the reviewing admin (e.g. a reason for rejection
    ///     or partial acceptance). Set on review; shown to the suggesting user in the
    ///     dispatched system message and on their <c>/profile</c> "My Suggestions" list.
    /// </summary>
    [MaxLength(2000)]
    public string AdminReviewComment { get; set; }

    /// <summary>
    ///     Heterogeneous payload of suggested values, keyed by canonical field name as defined
    ///     in the per-entity <c>SuggestionMetadata</c>. Values are stored as raw JSON (string,
    ///     int, Guid string, ISO date string, enum-as-int, or null).
    /// </summary>
    [Column(TypeName = "json")]
    public Dictionary<string, object> SuggestedValues { get; set; }

    /// <summary>
    ///     Set on review: keys are the field names the admin accepted; values are an optional
    ///     reason/note (typically empty). <c>null</c> while Pending.
    /// </summary>
    [Column(TypeName = "json")]
    public Dictionary<string, string> AppliedFields { get; set; }

    public virtual ApplicationUser CreatedBy  { get; set; }
    public virtual ApplicationUser ReviewedBy { get; set; }
}
