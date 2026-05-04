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

public class ProcessorDescriptionDto : BaseDto<int>
{
    [JsonPropertyName("markdown")]
    [Required]
    public required string Markdown { get; set; }
    [JsonPropertyName("html")]
    public string? Html { get; set; }
    [JsonPropertyName("processor_id")]
    [Required]
    public int ProcessorId { get; set; }
    [JsonPropertyName("language_code")]
    [Required]
    public required string LanguageCode { get; set; }
    [JsonPropertyName("language")]
    public string? Language { get; set; }
}
