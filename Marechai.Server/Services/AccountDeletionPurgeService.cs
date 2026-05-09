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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Marechai.Server.Services;

/// <summary>
///     Hosted service that hard-deletes any account whose <c>DeletionRequestedAt</c> is more than 30 days
///     in the past. Runs once an hour. Errors purging individual users are logged per-user and do not
///     abort the loop.
/// </summary>
public sealed class AccountDeletionPurgeService(IServiceScopeFactory                scopeFactory,
                                                ILogger<AccountDeletionPurgeService> logger) : BackgroundService
{
    static readonly TimeSpan _interval     = TimeSpan.FromHours(1);
    static readonly TimeSpan _gracePeriod  = TimeSpan.FromDays(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // First tick: run immediately on startup so an admin restarting the service after a crash
        // doesn't have to wait an hour to clear the backlog.
        while(!stoppingToken.IsCancellationRequested)
        {
            try { await PurgeExpiredAsync(stoppingToken); }
            catch(OperationCanceledException) when(stoppingToken.IsCancellationRequested) { break; }
            catch(Exception ex)
            {
                logger.LogError(ex, "AccountDeletionPurgeService tick failed; will retry next interval");
            }

            try { await Task.Delay(_interval, stoppingToken); }
            catch(OperationCanceledException) { break; }
        }
    }

    async Task PurgeExpiredAsync(CancellationToken ct)
    {
        DateTime cutoff = DateTime.UtcNow - _gracePeriod;

        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();

        MarechaiContext              ctx     = scope.ServiceProvider.GetRequiredService<MarechaiContext>();
        UserAccountDeletionService   service = scope.ServiceProvider.GetRequiredService<UserAccountDeletionService>();

        System.Collections.Generic.List<string> expiredIds = await ctx.Users
                                                                     .AsNoTracking()
                                                                     .Where(u => u.DeletionRequestedAt != null &&
                                                                                 u.DeletionRequestedAt <= cutoff)
                                                                     .Select(u => u.Id)
                                                                     .ToListAsync(ct);

        if(expiredIds.Count == 0) return;

        logger.LogInformation("Purging {Count} accounts whose 30-day grace window has elapsed", expiredIds.Count);

        foreach(string id in expiredIds)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                await service.PurgeAsync(id, actorUserIdForLog: "system");
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Failed to purge expired account {UserId}; continuing with the rest", id);
            }
        }
    }
}
