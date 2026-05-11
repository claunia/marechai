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
}
