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

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Marechai.Translation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Marechai.Server.Services;

/// <summary>
///     <see cref="ITranslationProvider" /> for <see cref="SoftwareGenre" />. Wraps the existing
///     <see cref="SoftwareGenreTranslationCache" /> and writes to <see cref="SoftwareGenreTranslation" />.
///     Logic is identical to the pre-refactor <c>TranslationWorker</c> body — extracted verbatim.
/// </summary>
public sealed class SoftwareGenreTranslationProvider(IServiceScopeFactory                       scopeFactory,
                                                     TranslationService                         translationService,
                                                     SoftwareGenreTranslationCache              cache,
                                                     IOptionsMonitor<TranslationOptions>        options,
                                                     ILogger<SoftwareGenreTranslationProvider> logger)
    : ITranslationProvider
{
    /// <summary>
    ///     Number of translations to accumulate before flushing to the database. Keeps the
    ///     transaction window short so a worker cancellation mid-sweep doesn't lose more than
    ///     this many already-translated rows.
    /// </summary>
    const int FlushBatchSize = 25;

    public string Name => "SoftwareGenre";

    public Task EnsureCacheLoadedAsync(CancellationToken ct) => cache.EnsureLoadedAsync(ct);

    /// <summary>
    ///     Delta query for genres added to the DB since the cache was last refreshed. Newly seen
    ///     genres are registered in the cache so the per-language snapshot pass picks them up. Updates
    ///     <see cref="SoftwareGenreTranslationCache.LastSeenGenreId" />.
    /// </summary>
    public async Task<int> DiscoverNewItemsAsync(CancellationToken ct)
    {
        int lastSeen = cache.LastSeenGenreId;

        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        MarechaiContext               ctx   = scope.ServiceProvider.GetRequiredService<MarechaiContext>();

        List<(int Id, string Name)> added = (await ctx.SoftwareGenres
                                                      .AsNoTracking()
                                                      .Where(g => g.Id > lastSeen)
                                                      .OrderBy(g => g.Id)
                                                      .Select(g => new { g.Id, g.Name })
                                                      .ToListAsync(ct))
                                            .Select(x => (x.Id, x.Name))
                                            .ToList();

        if(added.Count == 0) return 0;

        foreach((int id, string name) in added) cache.RegisterGenre(id, name);

        return added.Count;
    }

    /// <summary>
    ///     Snapshot missing genres from the cache (no DB query), translate them in bounded waves,
    ///     then bulk-insert the resulting rows in
    ///     <see cref="FlushBatchSize" />-sized chunks. Successful rows are also pushed to the cache
    ///     so subsequent requests see them immediately.
    /// </summary>
    public async Task<int> TranslateMissingAsync(string languageCode, CancellationToken ct)
    {
        IReadOnlyList<(int Id, string EnglishName)> missing = cache.SnapshotGenresMissingLanguage(languageCode);

        if(missing.Count == 0) return 0;

        logger.LogInformation("SoftwareGenre provider: {Count} genres missing translation into {Lang}.",
                              missing.Count, languageCode);

        // Domain hint passed to OpenAI so short labels translate in the right register
        // (e.g. "Action" → action genre, not the noun "act"; "Adventure" → genre, not noun).
        const string domainContext =
            "The text is the name of a video-game / software genre or sub-genre (entertainment classification).";

        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        MarechaiContext               ctx   = scope.ServiceProvider.GetRequiredService<MarechaiContext>();

        var inserted = 0;
        var batch    = new List<SoftwareGenreTranslation>(FlushBatchSize);

        for(var index = 0; index < missing.Count;)
        {
            int maxParallelTranslations =
                await BackgroundTranslationSettings.WaitForAvailableParallelismAsync(options, ct);
            var wave = new List<Task<SoftwareGenreTranslation>>(
                System.Math.Min(maxParallelTranslations, missing.Count - index));

            for(var launched = 0; launched < maxParallelTranslations && index < missing.Count; launched++, index++)
            {
                (int id, string englishName) = missing[index];

                wave.Add(TranslateOneAsync(id, englishName));
            }

            SoftwareGenreTranslation[] translatedWave = await Task.WhenAll(wave);

            foreach(SoftwareGenreTranslation row in translatedWave)
            {
                if(row is null) continue;
                batch.Add(row);
            }

            inserted += await BackgroundTranslationSettings.FlushFullBatchesAsync(batch, FlushBatchSize,
                                                                                  PersistBatchAsync, ct);
        }

        if(batch.Count > 0)
        {
            await PersistBatchAsync(batch, ct);
            inserted += batch.Count;
        }

        return inserted;

        async Task<SoftwareGenreTranslation> TranslateOneAsync(int id, string englishName)
        {
            ct.ThrowIfCancellationRequested();

            if(string.IsNullOrWhiteSpace(englishName)) return null;

            (string translated, string error) = await translationService.TranslateAsync(
                englishName, languageCode, progress: null, plainText: true, domainContext: domainContext);

            if(string.IsNullOrWhiteSpace(translated))
            {
                logger.LogWarning(
                    "SoftwareGenre provider: failed to translate genre {GenreId} (\"{English}\") into {Lang}: {Error}",
                    id, englishName, languageCode, error ?? "no result");

                return null;
            }

            if(translated.Length > 128) translated = translated[..128];

            return new SoftwareGenreTranslation
            {
                GenreId      = id,
                LanguageCode = languageCode,
                Name         = translated
            };
        }

        async Task PersistBatchAsync(List<SoftwareGenreTranslation> rows, CancellationToken saveCt)
        {
            ctx.SoftwareGenreTranslations.AddRange(rows);
            await ctx.SaveChangesAsync(saveCt);

            foreach(SoftwareGenreTranslation row in rows)
                cache.Upsert(row.GenreId, row.LanguageCode, row.Name);
        }
    }
}
