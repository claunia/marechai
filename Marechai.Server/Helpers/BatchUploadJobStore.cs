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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Marechai.Data.Dtos;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Marechai.Server.Helpers;

/// <summary>
///     Entity-agnostic per-item result recorded on a <see cref="BatchUploadJobStore.BatchJob" />.
///     Each controller maps this to its own response DTO (e.g. cover_id, photo_id) when
///     building a status snapshot.
/// </summary>
public sealed class BatchJobItemResult
{
    public Guid   PendingId  { get; init; }
    public bool   Succeeded  { get; init; }
    public Guid?  AssignedId { get; init; }
    public string Error      { get; init; }
}

/// <summary>
///     In-memory registry for in-flight admin batch-upload commit jobs. The controller
///     hands a request off to <see cref="StartJob" /> which spawns a worker on the thread
///     pool; the client polls a controller-specific status endpoint that snapshots the
///     job under <see cref="BatchJob.Lock" /> until the job is in a terminal state
///     (Completed / Failed). Owner authorisation is enforced via the user id stored with
///     each job.
/// </summary>
public sealed class BatchUploadJobStore
{
    readonly ConcurrentDictionary<Guid, BatchJob> _jobs = new();

    public sealed class BatchJob
    {
        public Guid                       JobId            { get; init; }
        public string                     OwnerUserId      { get; init; }
        public long                       ParentEntityId   { get; init; }
        public int                        Total            { get; init; }
        public DateTime                   CreatedOn        { get; init; }
        public DateTime                   LastTouchedOn    { get; set; }
        public BatchJobState              State            { get; set; }
        public int                        Processed        { get; set; }
        public Guid?                      CurrentPendingId { get; set; }
        public List<BatchJobItemResult>   Results          { get; } = new();
        public object                     Lock             { get; } = new();
    }

    /// <summary>
    ///     Create a new job record (state = Queued) and return its id. The caller is
    ///     expected to spawn the worker immediately (the store does not own the worker).
    ///     <paramref name="parentEntityId" /> is a feature-agnostic parent id (release id,
    ///     machine id, etc.); controllers cast their own typed id into long at the call
    ///     site.
    /// </summary>
    public Guid StartJob(string ownerUserId, long parentEntityId, int total)
    {
        var job = new BatchJob
        {
            JobId          = Guid.NewGuid(),
            OwnerUserId    = ownerUserId,
            ParentEntityId = parentEntityId,
            Total          = total,
            CreatedOn      = DateTime.UtcNow,
            LastTouchedOn  = DateTime.UtcNow,
            State          = BatchJobState.Queued
        };

        _jobs[job.JobId] = job;
        return job.JobId;
    }

    /// <summary>
    ///     Returns the job for the given id IFF the requesting user owns it. Null otherwise
    ///     (controller should translate to 404). Touches <see cref="BatchJob.LastTouchedOn" />
    ///     on a successful lookup so the reaper doesn't kill an actively-polled job.
    /// </summary>
    public BatchJob GetForOwner(Guid jobId, string callerUserId)
    {
        if(!_jobs.TryGetValue(jobId, out BatchJob job)) return null;
        if(!string.Equals(job.OwnerUserId, callerUserId, StringComparison.Ordinal)) return null;
        job.LastTouchedOn = DateTime.UtcNow;
        return job;
    }

    /// <summary>
    ///     Remove jobs that have been in a terminal state for more than 10 minutes OR have
    ///     been alive in a non-terminal state without polling for more than 1 hour. Called
    ///     by the reaper hosted service.
    /// </summary>
    public int Reap()
    {
        DateTime now      = DateTime.UtcNow;
        var      doomed   = new List<Guid>();

        foreach(KeyValuePair<Guid, BatchJob> kv in _jobs)
        {
            BatchJob job = kv.Value;
            bool     terminal = job.State == BatchJobState.Completed || job.State == BatchJobState.Failed;

            TimeSpan age = now - job.LastTouchedOn;
            if(terminal && age > TimeSpan.FromMinutes(10)) doomed.Add(kv.Key);
            else if(!terminal && age > TimeSpan.FromHours(1)) doomed.Add(kv.Key);
        }

        foreach(Guid id in doomed) _jobs.TryRemove(id, out _);

        return doomed.Count;
    }
}

/// <summary>
///     Periodic background worker that purges stale entries from
///     <see cref="BatchUploadJobStore" />. Runs once every 2 minutes.
/// </summary>
public sealed class BatchUploadJobReaperService(BatchUploadJobStore store, ILogger<BatchUploadJobReaperService> logger)
    : BackgroundService
{
    static readonly TimeSpan _interval = TimeSpan.FromMinutes(2);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while(!stoppingToken.IsCancellationRequested)
        {
            try
            {
                int removed = store.Reap();
                if(removed > 0)
                    logger.LogInformation("BatchUploadJobReaper removed {Count} stale job(s).", removed);
            }
            catch(Exception ex)
            {
                logger.LogWarning(ex, "BatchUploadJobReaper iteration failed.");
            }

            try { await Task.Delay(_interval, stoppingToken); }
            catch(TaskCanceledException) { break; }
        }
    }
}
