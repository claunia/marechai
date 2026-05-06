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
using System.Threading.Tasks;
using Marechai.ApiClient.Models;
using Microsoft.Extensions.Logging;

namespace Marechai.Services;

public sealed class NotificationService(Marechai.ApiClient.Client client, ILogger<NotificationService> logger)
{
    public async Task<int> GetUnreadCountAsync()
    {
        try
        {
            return await client.Notifications.UnreadCount.GetAsync() ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    public async Task<List<AdminNotificationDto>> GetNotificationsAsync(bool unreadOnly = false)
    {
        try
        {
            List<AdminNotificationDto> notifications = await client.Notifications.GetAsync(config =>
            {
                config.QueryParameters.UnreadOnly = unreadOnly;
            });

            return notifications ?? [];
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Error loading notifications");

            return [];
        }
    }

    public async Task<bool> MarkAsReadAsync(long id)
    {
        try
        {
            await client.Notifications[id].Read.PutAsync();

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> MarkAllAsReadAsync()
    {
        try
        {
            await client.Notifications.ReadAll.PutAsync();

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteNotificationAsync(long id)
    {
        try
        {
            await client.Notifications[id].DeleteAsync();

            return true;
        }
        catch
        {
            return false;
        }
    }
}
