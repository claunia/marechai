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
using System.Threading;
using System.Threading.Tasks;
using Marechai.Server.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Marechai.Server.Services;

/// <summary>
///     Hosted service that sweeps every <c>photos/&lt;item&gt;/pending/</c> directory once
///     a day and removes any pending image (admin batch staging or collaborator suggestion)
///     whose recorded <c>UploadedOn</c> (or filesystem mtime when the sidecar is unusable)
///     is older than <see cref="_maxAge" />. Keeps the pending tree from accumulating
///     abandoned uploads after dialog dismissals, browser crashes, server restarts mid-batch
///     or any other path that leaves orphan stagings behind.
/// </summary>
public sealed class PendingImagePurgeService(IConfiguration                    configuration,
                                             ILogger<PendingImagePurgeService> logger) : BackgroundService
{
    // Canonical list of pending-folder kinds. Mirrors every itemFolder string passed to
    // PendingImageStore by the controllers and SuggestionsController. Update when a new
    // item kind grows a pending/ folder.
    static readonly string[] _itemFolders =
    [
        "book-covers", "magazine-issue-covers", "people", "gpus", "processors", "sound-synths", "machines",
        "software-promo-art", "software-covers", "software-screenshots"
    ];

    static readonly TimeSpan _interval = TimeSpan.FromHours(24);
    static readonly TimeSpan _maxAge   = TimeSpan.FromHours(12);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // First pass on startup: wipe EVERY pending entry regardless of age. Any file that
        // survived a restart is presumed orphan — an admin won't be mid-upload across a
        // process boundary. After this initial scrub, the regular 12h age threshold applies.
        if(!stoppingToken.IsCancellationRequested)
        {
            try { Sweep(TimeSpan.Zero); }
            catch(OperationCanceledException) when(stoppingToken.IsCancellationRequested) { return; }
            catch(Exception ex)
            {
                logger.LogError(ex, "PendingImagePurgeService startup sweep failed; will retry next interval");
            }
        }

        while(!stoppingToken.IsCancellationRequested)
        {
            try { await Task.Delay(_interval, stoppingToken); }
            catch(OperationCanceledException) { break; }

            try { Sweep(_maxAge); }
            catch(OperationCanceledException) when(stoppingToken.IsCancellationRequested) { break; }
            catch(Exception ex)
            {
                logger.LogError(ex, "PendingImagePurgeService sweep failed; will retry next interval");
            }
        }
    }

    void Sweep(TimeSpan olderThan)
    {
        string assetRoot = configuration["AssetRootPath"];
        if(string.IsNullOrEmpty(assetRoot))
        {
            logger.LogWarning("PendingImagePurgeService skipped: AssetRootPath is not configured.");
            return;
        }

        int total = 0;

        foreach(string folder in _itemFolders)
        {
            try
            {
                int removed = PendingImageStore.PurgeStale(assetRoot, folder, olderThan);
                if(removed > 0)
                    logger.LogInformation(
                        "PendingImagePurge removed {Count} stale entr{Suffix} from {Folder}",
                        removed, removed == 1 ? "y" : "ies", folder);
                total += removed;
            }
            catch(Exception ex)
            {
                logger.LogWarning(ex, "PendingImagePurge failed for folder {Folder}; continuing with the rest",
                                  folder);
            }
        }

        if(total > 0)
            logger.LogInformation("PendingImagePurge sweep complete: removed {Total} stale pending entries.",
                                  total);
    }
}
