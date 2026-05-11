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
using System.Text.Json.Serialization;

namespace Marechai.Data.Dtos;

/// <summary>
///     A user-submitted suggestion to add or edit a catalog entity. <c>SuggestedValues</c> is a
///     JSON-encoded object whose keys are canonical field names per the per-entity metadata.
///     The values use raw JSON (string / number / null / etc.). The format is stable on the
///     wire because Kiota handles strings reliably.
/// </summary>
public class SuggestionDto : BaseDto<long>
{
    [JsonPropertyName("entity_type")]
    public SuggestionEntityType EntityType { get; set; }

    /// <summary><c>null</c> when suggesting a brand-new entity (reserved for future).</summary>
    [JsonPropertyName("entity_id")]
    public long? EntityId { get; set; }

    [JsonPropertyName("entity_display_name")]
    public string? EntityDisplayName { get; set; }

    [JsonPropertyName("status")]
    public SuggestionStatus Status { get; set; }

    [JsonPropertyName("created_by_id")]
    public string? CreatedById { get; set; }

    [JsonPropertyName("created_by_user_name")]
    public string? CreatedByUserName { get; set; }

    [JsonPropertyName("created_by_display_name")]
    public string? CreatedByDisplayName { get; set; }

    [JsonPropertyName("created_on")]
    public DateTime CreatedOn { get; set; }

    [JsonPropertyName("reviewed_by_id")]
    public string? ReviewedById { get; set; }

    [JsonPropertyName("reviewed_by_display_name")]
    public string? ReviewedByDisplayName { get; set; }

    [JsonPropertyName("reviewed_on")]
    public DateTime? ReviewedOn { get; set; }

    [JsonPropertyName("user_comment")]
    public string? UserComment { get; set; }

    /// <summary>JSON object: { "fieldName": value, ... }. Empty rejection-state suggestions never reach the wire.</summary>
    [JsonPropertyName("suggested_values_json")]
    public string? SuggestedValuesJson { get; set; }

    /// <summary>JSON object: { "fieldName": "reason", ... }. Populated on review; null while Pending.</summary>
    [JsonPropertyName("applied_fields_json")]
    public string? AppliedFieldsJson { get; set; }
}
