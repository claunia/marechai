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
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Marechai.Helpers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Marechai.Services;

/// <summary>
///     Background worker that periodically drains the <see cref="IndexNowService" /> queue
///     and POSTs batches of changed URLs to every IndexNow-participating search engine
///     endpoint in parallel.
/// </summary>
public sealed class IndexNowBackgroundService(
    IndexNowService              indexNowService,
    IHttpClientFactory           httpClientFactory,
    IOptions<IndexNowOptions>    options,
    ILogger<IndexNowBackgroundService> logger) : BackgroundService
{
    // How often we drain the queue and submit.
    static readonly TimeSpan Interval = TimeSpan.FromSeconds(30);

    // IndexNow caps a single POST at 10 000 URLs.
    const int MaxBatchSize = 10_000;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        IndexNowOptions opts = options.Value;

        if(!opts.Enabled || string.IsNullOrWhiteSpace(opts.Key))
        {
            logger.LogInformation("IndexNow is disabled or no key configured — background worker will not run");

            return;
        }

        logger.LogInformation("IndexNow background worker started, submitting to {Count} endpoints every {Seconds}s",
                              opts.Endpoints.Length, Interval.TotalSeconds);

        while(!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(Interval, stoppingToken);
            }
            catch(TaskCanceledException)
            {
                break;
            }

            List<string> urls = indexNowService.DrainQueue();

            if(urls.Count == 0)
                continue;

            // Split into IndexNow-legal batches of 10 000.
            for(int i = 0; i < urls.Count; i += MaxBatchSize)
            {
                List<string> batch = urls.GetRange(i, Math.Min(MaxBatchSize, urls.Count - i));

                await SubmitBatchAsync(batch, opts, stoppingToken);
            }
        }
    }

    async Task SubmitBatchAsync(List<string> urls, IndexNowOptions opts, CancellationToken ct)
    {
        // Parse the canonical host to extract just the hostname for the payload.
        string host = new Uri(SeoMeta.CanonicalHost).Host;

        string keyLocation = $"{SeoMeta.CanonicalHost}/{opts.Key}.txt";

        var payload = new IndexNowPayload
        {
            Host        = host,
            Key         = opts.Key,
            KeyLocation = keyLocation,
            UrlList     = urls
        };

        logger.LogInformation("IndexNow: submitting {Count} URL(s) to {Endpoints} endpoints",
                              urls.Count, opts.Endpoints.Length);

        // Fire requests to all endpoints in parallel.
        Task[] tasks = opts.Endpoints.Select(endpoint => SubmitToEndpointAsync(endpoint, payload, ct)).ToArray();

        await Task.WhenAll(tasks);
    }

    async Task SubmitToEndpointAsync(string endpoint, IndexNowPayload payload, CancellationToken ct)
    {
        try
        {
            HttpClient client = httpClientFactory.CreateClient("IndexNow");

            string json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(endpoint, content, ct);

            if(response.IsSuccessStatusCode)
            {
                logger.LogInformation("IndexNow: {Endpoint} accepted {Count} URL(s) (HTTP {Status})",
                                      endpoint, payload.UrlList.Count, (int)response.StatusCode);
            }
            else
            {
                logger.LogWarning("IndexNow: {Endpoint} returned HTTP {Status} for {Count} URL(s)",
                                  endpoint, (int)response.StatusCode, payload.UrlList.Count);
            }
        }
        catch(Exception ex)
        {
            logger.LogWarning(ex, "IndexNow: failed to submit to {Endpoint}", endpoint);
        }
    }

    /// <summary>JSON payload shape for the IndexNow POST API.</summary>
    sealed class IndexNowPayload
    {
        [JsonPropertyName("host")]
        public string Host { get; init; }

        [JsonPropertyName("key")]
        public string Key { get; init; }

        [JsonPropertyName("keyLocation")]
        public string KeyLocation { get; init; }

        [JsonPropertyName("urlList")]
        public List<string> UrlList { get; init; }
    }
}
