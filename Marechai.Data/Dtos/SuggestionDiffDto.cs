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
using System.Text.Json.Serialization;

namespace Marechai.Data.Dtos;

/// <summary>
///     Diff payload returned to the admin reviewing a suggestion. <c>CurrentValuesJson</c> is
///     freshly recomputed from the latest entity state so the comparison is always live.
/// </summary>
public class SuggestionDiffDto
{
    [JsonPropertyName("suggestion")]
    [Required]
    public required SuggestionDto Suggestion { get; set; }

    /// <summary>
    ///     JSON object: { "fieldName": currentValue, ... } recomputed from the entity at GET-time.
    ///     Empty / missing keys mean the field has no current value (e.g. nullable optional fields).
    /// </summary>
    [JsonPropertyName("current_values_json")]
    public string? CurrentValuesJson { get; set; }

    /// <summary>True if the targeted entity no longer exists (only possible briefly between delete and stale-mark).</summary>
    [JsonPropertyName("entity_missing")]
    public bool EntityMissing { get; set; }

    /// <summary>
    ///     Optional secondary label for the entity, e.g. <c>"(Spanish description)"</c> for a
    ///     <see cref="SuggestionEntityType.CompanyDescription" /> review. Lets the queue and
    ///     review dialog tag the row without re-querying.
    /// </summary>
    [JsonPropertyName("entity_secondary_label")]
    public string? EntitySecondaryLabel { get; set; }

    /// <summary>
    ///     Optional per-field display labels for the CURRENT side of the diff. Populated by the
    ///     server only for foreign-key fields (e.g. <c>{ "country_id": "Spain", "sold_to_id": "Apple" }</c>),
    ///     so the diff panel can render the resolved entity name instead of the raw integer id.
    ///     Empty/missing key → fall back to <c>SuggestionMetadata.FormatDisplayValue</c>.
    /// </summary>
    [JsonPropertyName("current_labels")]
    public Dictionary<string, string>? CurrentLabels { get; set; }

    /// <summary>
    ///     Optional per-field display labels for the SUGGESTED side of the diff. Same semantics
    ///     as <see cref="CurrentLabels" /> but for the proposed values. Populated only for
    ///     foreign-key fields.
    /// </summary>
    [JsonPropertyName("suggested_labels")]
    public Dictionary<string, string>? SuggestedLabels { get; set; }
}
