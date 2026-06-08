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
using Marechai.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Marechai.Services;

/// <summary>
///     Thread-safe in-memory queue for IndexNow URL submissions. Singleton service that
///     accepts relative URLs from admin CRUD operations and exposes them for the background
///     worker to drain and POST to each participating search engine.
/// </summary>
public sealed class IndexNowService(IOptions<IndexNowOptions> options, ILogger<IndexNowService> logger)
{
    readonly ConcurrentQueue<string> _queue = new();

    /// <summary>Enqueue a relative URL (e.g. <c>/machine/42</c>) for submission to IndexNow.</summary>
    public void EnqueueUrl(string relativeUrl)
    {
        IndexNowOptions opts = options.Value;

        if(!opts.Enabled || string.IsNullOrWhiteSpace(opts.Key))
            return;

        string absoluteUrl = $"{SeoMeta.CanonicalHost}{relativeUrl}";

        _queue.Enqueue(absoluteUrl);

        logger.LogDebug("IndexNow: enqueued {Url}", absoluteUrl);
    }

    /// <summary>Drain all queued URLs into a deduplicated list. Called by the background worker.</summary>
    internal List<string> DrainQueue()
    {
        var urls = new HashSet<string>(StringComparer.Ordinal);

        while(_queue.TryDequeue(out string url))
            urls.Add(url);

        return [.. urls];
    }
}
