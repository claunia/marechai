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
using System.Text.Json.Serialization;

namespace Marechai.Data.Dtos;

public class AdminNotificationDto : BaseDto<long>
{
    [JsonPropertyName("title")]
    public string Title { get; set; }
    [JsonPropertyName("message")]
    public string Message { get; set; }
    [JsonPropertyName("link_url")]
    public string LinkUrl { get; set; }
    [JsonPropertyName("link_text")]
    public string LinkText { get; set; }
    [JsonPropertyName("is_read")]
    public bool IsRead { get; set; }
    [JsonPropertyName("notification_type")]
    public string NotificationType { get; set; }
    [JsonPropertyName("created_on")]
    public DateTime CreatedOn { get; set; }
}
