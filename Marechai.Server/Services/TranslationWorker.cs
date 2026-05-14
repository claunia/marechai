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
using System.Threading;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Marechai.Translation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Marechai.Server.Services;

/// <summary>
///     Hosted service that fills missing rows in <see cref="SoftwareGenreTranslation" /> by calling
///     <see cref="TranslationService" /> (OpenAI preferred, NLLB fallback). Designed to grow per-entity
///     branches in the future (e.g. SoftwareAttributes) without renaming — see the architecture note
///     in the session plan.
/// </summary>
/// <remarks>
///     <para>Lifecycle:</para>
///     <list type="number">
///         <item>On start, exits permanently if <see cref="TranslationService.IsAvailable" /> is false.</item>
///         <item>Loop: refresh the in-memory cache for newly-added genres (one delta query
///         <c>WHERE Id &gt; LastSeenGenreId</c>); for each non-<c>eng</c> language, snapshot the missing
///         genres, translate them, and bulk-insert the rows in a single DB roundtrip; sleep 1h.</item>
///     </list>
///     <para>The DB is touched once per language per tick (one read implicitly via the in-memory snapshot
///     + one bulk insert). The OpenAI/NLLB calls are serialised inside the worker so multiple entity
///     types added later cannot collide on the rate limit.</para>
/// </remarks>
public sealed class TranslationWorker(IServiceScopeFactory                scopeFactory,
                                      TranslationService                  translationService,
                                      SoftwareGenreTranslationCache       genreCache,
                                      ILogger<TranslationWorker>          logger) : BackgroundService
{
    static readonly TimeSpan _interval = TimeSpan.FromHours(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if(!translationService.IsAvailable)
        {
            logger.LogInformation(
                "Translation providers (OpenAI / NLLB) are not configured; TranslationWorker exiting.");

            return;
        }

        // Make sure the cache is populated before the first sweep — even if the eager warm-up in
        // Program.cs failed, this still gives the worker a consistent view of all genres.
        try
        {
            await genreCache.EnsureLoadedAsync(stoppingToken);
        }
        catch(OperationCanceledException) when(stoppingToken.IsCancellationRequested)
        {
            return;
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "TranslationWorker initial cache load failed; will retry next tick.");
        }

        while(!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DiscoverNewGenresAsync(stoppingToken);
                await TranslateMissingAsync(stoppingToken);
            }
            catch(OperationCanceledException) when(stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "TranslationWorker tick failed; will retry next interval.");
            }

            try { await Task.Delay(_interval, stoppingToken); }
            catch(OperationCanceledException) { break; }
        }
    }

    /// <summary>
    ///     One delta query against <c>SoftwareGenres</c> for rows added since the cache was last
    ///     refreshed. Newly seen genres are registered in the cache so the per-language snapshot
    ///     pass picks them up. Updates <see cref="SoftwareGenreTranslationCache.LastSeenGenreId" />.
    /// </summary>
    async Task DiscoverNewGenresAsync(CancellationToken ct)
    {
        int lastSeen = genreCache.LastSeenGenreId;

        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        MarechaiContext               ctx   = scope.ServiceProvider.GetRequiredService<MarechaiContext>();

        List<(int Id, string Name)> added = await ctx.SoftwareGenres
                                                     .AsNoTracking()
                                                     .Where(g => g.Id > lastSeen)
                                                     .OrderBy(g => g.Id)
                                                     .Select(g => new { g.Id, g.Name })
                                                     .ToListAsync(ct)
                                                     .ContinueWith(t => t.Result.Select(x => (x.Id, x.Name)).ToList(),
                                                                   ct);

        if(added.Count == 0) return;

        foreach((int id, string name) in added)
            genreCache.RegisterGenre(id, name);

        logger.LogInformation("TranslationWorker discovered {Count} new SoftwareGenre rows.", added.Count);
    }

    /// <summary>
    ///     For each non-<c>eng</c> language: snapshot missing genres from the cache (no DB query),
    ///     translate them one by one (OpenAI/NLLB serialised), then bulk-insert the resulting rows
    ///     in a single <see cref="DbContext.SaveChangesAsync(CancellationToken)" /> call. Successful
    ///     rows are also pushed to the cache so subsequent requests see them immediately.
    /// </summary>
    async Task TranslateMissingAsync(CancellationToken ct)
    {
        foreach(string lang in TranslationService.SupportedLanguageCodes)
        {
            if(string.Equals(lang, "eng", StringComparison.Ordinal)) continue;

            ct.ThrowIfCancellationRequested();

            IReadOnlyList<(int Id, string EnglishName)> missing = genreCache.SnapshotGenresMissingLanguage(lang);

            if(missing.Count == 0) continue;

            logger.LogInformation("TranslationWorker: {Count} genres missing translation into {Lang}.",
                                  missing.Count, lang);

            var batch = new List<SoftwareGenreTranslation>(missing.Count);

            foreach((int id, string englishName) in missing)
            {
                ct.ThrowIfCancellationRequested();

                if(string.IsNullOrWhiteSpace(englishName)) continue;

                (string translated, string error) = await translationService.TranslateAsync(
                    englishName, lang, progress: null, plainText: true);

                if(string.IsNullOrWhiteSpace(translated))
                {
                    logger.LogWarning(
                        "TranslationWorker: failed to translate genre {GenreId} (\"{English}\") into {Lang}: {Error}",
                        id, englishName, lang, error ?? "no result");

                    continue;
                }

                // Defensive: trim to the column max length (varchar(128)).
                if(translated.Length > 128) translated = translated[..128];

                batch.Add(new SoftwareGenreTranslation
                {
                    GenreId      = id,
                    LanguageCode = lang,
                    Name         = translated
                });
            }

            if(batch.Count == 0) continue;

            await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
            MarechaiContext               ctx   = scope.ServiceProvider.GetRequiredService<MarechaiContext>();

            ctx.SoftwareGenreTranslations.AddRange(batch);
            await ctx.SaveChangesAsync(ct);

            foreach(SoftwareGenreTranslation row in batch)
                genreCache.Upsert(row.GenreId, row.LanguageCode, row.Name);

            logger.LogInformation("TranslationWorker: inserted {Count} translations into {Lang}.",
                                  batch.Count, lang);
        }
    }
}
