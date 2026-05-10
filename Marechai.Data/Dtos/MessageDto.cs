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

public class MessageDto : BaseDto<long>
{
    [JsonPropertyName("conversation_id")]
    public long ConversationId { get; set; }

    /// <summary>
    ///     Marked <see cref="RequiredAttribute" /> only so the OpenAPI schema emits a direct <c>$ref</c> instead of
    ///     <c>oneOf:[null,$ref]</c> (which Kiota turns into a discriminator-based wrapper that never populates). The wire
    ///     value can still legitimately be JSON null when the originating account has been deleted.
    /// </summary>
    [Required]
    [JsonPropertyName("sender")]
    public UserSummaryDto Sender { get; set; }

    [JsonPropertyName("is_system_authored")]
    public bool IsSystemAuthored { get; set; }

    [JsonPropertyName("body")]
    public string? Body { get; set; }

    [JsonPropertyName("is_read")]
    public bool IsRead { get; set; }

    [JsonPropertyName("parent_message_id")]
    public long? ParentMessageId { get; set; }

    [JsonPropertyName("created_on")]
    public DateTime CreatedOn { get; set; }
}
