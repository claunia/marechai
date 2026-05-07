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

public class Message : BaseModel<long>
{
    public const int MaxBodyLength = 5000;

    [Required]
    public long ConversationId { get; set; }

    /// <summary>
    ///     Message author. Nullable so deleted users keep their messages visible (rendered as "(deleted user)"). System
    ///     messages reference the seeded `system` user instead of being null.
    /// </summary>
    public string SenderId { get; set; }

    /// <summary>
    ///     Markdown source of the message body. Hard-capped to <see cref="MaxBodyLength" /> characters for user-authored
    ///     messages; system-authored messages bypass the cap so quoted reports stay intact.
    /// </summary>
    [Required]
    [MaxLength(MaxBodyLength)]
    public string Body { get; set; }

    public bool IsSystemAuthored { get; set; }

    public long? ParentMessageId { get; set; }

    public virtual Conversation         Conversation  { get; set; }
    public virtual ApplicationUser      Sender        { get; set; }
    public virtual Message              ParentMessage { get; set; }
    public virtual ICollection<MessageState> States   { get; set; }
}
