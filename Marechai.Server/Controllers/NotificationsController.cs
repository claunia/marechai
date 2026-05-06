/*******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ---------------------------------------------------------------------------
//
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// --[ License ] -----------------------------------------------------------
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
// ---------------------------------------------------------------------------
// Copyright © 2003-2026 Natalia Portillo
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Marechai.Data.Dtos;
using Marechai.Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Server.Controllers;

[Route("notifications")]
[ApiController]
[Authorize(Roles = "Admin, UberAdmin")]
public class NotificationsController(MarechaiContext context) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<List<AdminNotificationDto>> GetNotificationsAsync([FromQuery] bool unreadOnly = false)
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        IQueryable<AdminNotification> query = context.AdminNotifications
                                                      .Where(n => n.TargetUserId == null || n.TargetUserId == userId);

        if(unreadOnly)
            query = query.Where(n => !n.IsRead);

        return await query.OrderByDescending(n => n.CreatedOn)
                          .Take(100)
                          .Select(n => new AdminNotificationDto
                           {
                               Id               = n.Id,
                               Title            = n.Title,
                               Message          = n.Message,
                               LinkUrl          = n.LinkUrl,
                               LinkText         = n.LinkText,
                               IsRead           = n.IsRead,
                               NotificationType = n.NotificationType,
                               CreatedOn        = n.CreatedOn
                           })
                          .ToListAsync();
    }

    [HttpGet("unread-count")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<int> GetUnreadCountAsync()
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        return await context.AdminNotifications
                            .CountAsync(n => !n.IsRead && (n.TargetUserId == null || n.TargetUserId == userId));
    }

    [HttpPut("{id:long}/read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsReadAsync(long id)
    {
        AdminNotification notification = await context.AdminNotifications.FindAsync(id);

        if(notification is null) return NotFound();

        notification.IsRead = true;
        await context.SaveChangesAsync();

        return Ok();
    }

    [HttpPut("read-all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkAllAsReadAsync()
    {
        string userId = User.FindFirstValue(ClaimTypes.Sid);

        await context.AdminNotifications
                     .Where(n => !n.IsRead && (n.TargetUserId == null || n.TargetUserId == userId))
                     .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));

        return Ok();
    }

    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteNotificationAsync(long id)
    {
        AdminNotification notification = await context.AdminNotifications.FindAsync(id);

        if(notification is not null)
        {
            context.AdminNotifications.Remove(notification);
            await context.SaveChangesAsync();
        }

        return NoContent();
    }
}
