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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Marechai.Server.Services.Rankings;

/// <summary>
///     Background worker that keeps the persisted Marechai rankings fresh. On startup it
///     reads the latest <see cref="RankingDefinition.ComputedAt" /> from the database to
///     seed <see cref="RankingsComputationState.LastComputedAt" />; if the snapshot is
///     stale (older than <see cref="_recomputeInterval" />) or has never been computed,
///     the worker triggers <see cref="MarechaiRankingsCalculator.ComputeAllAsync" />
///     immediately in the background — the HTTP listener is NOT blocked. After the first
///     check (and after each subsequent recompute) the worker sleeps until the next 24 h
///     tick. The <see cref="RankingsComputationState.Gate" /> semaphore serializes the
///     periodic tick with the optional admin-recompute endpoint so two computations can
///     never run in parallel.
/// </summary>
public sealed class MarechaiRankingsWorker(IDbContextFactory<MarechaiContext> dbFactory,
                                           MarechaiRankingsCalculator         calculator,
                                           RankingsComputationState           state,
                                           ILogger<MarechaiRankingsWorker>    logger)
    : BackgroundService
{
    /// <summary>How often to recompute the rankings. 24 h matches the user's specification.</summary>
    static readonly TimeSpan _recomputeInterval = TimeSpan.FromHours(24);

    /// <summary>
    ///     Initial defer interval — gives the server a moment to finish migrations + DI
    ///     resolution before the calculator starts hammering the DB. Short enough to feel
    ///     instant on a fresh install.
    /// </summary>
    static readonly TimeSpan _startupDeferral = TimeSpan.FromSeconds(10);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Yield once so the host's StartAsync can finish before we touch the DB. EF migrations
        // run synchronously in Program.Main BEFORE the host StartAsync completes, so by the
        // time this worker runs the schema is guaranteed to be present.
        await Task.Yield();

        try
        {
            // -------- Phase 1: seed in-memory freshness from DB --------
            DateTime? latest = await ReadLatestComputedAtAsync(stoppingToken).ConfigureAwait(false);
            state.SeedLastComputedAt(latest);

            DateTime now = DateTime.UtcNow;
            TimeSpan firstWait;

            if(latest is null)
            {
                logger.LogInformation(
                    "MarechaiRankingsWorker: no persisted rankings found, computing immediately after a short defer.");
                firstWait = _startupDeferral;
            }
            else
            {
                TimeSpan age = now - latest.Value;

                if(age >= _recomputeInterval)
                {
                    logger.LogInformation(
                        "MarechaiRankingsWorker: persisted rankings are {Age} old (>= {Threshold}), computing immediately after a short defer.",
                        age, _recomputeInterval);
                    firstWait = _startupDeferral;
                }
                else
                {
                    TimeSpan nextRun = _recomputeInterval - age;

                    logger.LogInformation(
                        "MarechaiRankingsWorker: persisted rankings are {Age} old, next recompute in {NextRun}.",
                        age, nextRun);

                    firstWait = nextRun;
                }
            }

            await Task.Delay(firstWait, stoppingToken).ConfigureAwait(false);

            // -------- Phase 2: periodic loop --------
            while(!stoppingToken.IsCancellationRequested)
            {
                await RunOnceAsync(stoppingToken).ConfigureAwait(false);

                // Sleep regardless of success — failed runs retry on the next 24 h tick.
                try
                {
                    await Task.Delay(_recomputeInterval, stoppingToken).ConfigureAwait(false);
                }
                catch(OperationCanceledException) { return; }
            }
        }
        catch(OperationCanceledException) when(stoppingToken.IsCancellationRequested)
        {
            // Host shutting down — clean exit.
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "MarechaiRankingsWorker: unrecoverable failure, worker exiting.");
        }
    }

    /// <summary>
    ///     Acquires the global gate (waits up to 1 minute; skips this tick on contention),
    ///     runs the calculator, releases. The admin-recompute endpoint uses
    ///     <c>WaitAsync(TimeSpan.Zero)</c> so contention there is signalled as 409.
    /// </summary>
    async Task RunOnceAsync(CancellationToken ct)
    {
        if(!await state.Gate.WaitAsync(TimeSpan.FromMinutes(1), ct).ConfigureAwait(false))
        {
            logger.LogWarning("MarechaiRankingsWorker: another computation held the gate for >1 min, skipping this tick.");
            return;
        }

        try
        {
            await calculator.ComputeAllAsync(ct).ConfigureAwait(false);
        }
        finally
        {
            state.Gate.Release();
        }
    }

    async Task<DateTime?> ReadLatestComputedAtAsync(CancellationToken ct)
    {
        try
        {
            await using MarechaiContext ctx = await dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);

            // Single-column projection on a small table — no FROM-side join overhead.
            return await ctx.RankingDefinitions
                            .AsNoTracking()
                            .Select(d => (DateTime?)d.ComputedAt)
                            .OrderByDescending(d => d)
                            .FirstOrDefaultAsync(ct)
                            .ConfigureAwait(false);
        }
        catch(Exception ex)
        {
            logger.LogWarning(ex, "MarechaiRankingsWorker: could not read latest ComputedAt; assuming fresh install.");
            return null;
        }
    }
}
