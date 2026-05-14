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

namespace Marechai.Server.Services;

/// <summary>
///     <see cref="ITranslationProvider" /> for <see cref="SoftwareGenre" />. Wraps the existing
///     <see cref="SoftwareGenreTranslationCache" /> and writes to <see cref="SoftwareGenreTranslation" />.
///     Logic is identical to the pre-refactor <c>TranslationWorker</c> body — extracted verbatim.
/// </summary>
public sealed class SoftwareGenreTranslationProvider(IServiceScopeFactory                       scopeFactory,
                                                     TranslationService                         translationService,
                                                     SoftwareGenreTranslationCache              cache,
                                                     ILogger<SoftwareGenreTranslationProvider> logger)
    : ITranslationProvider
{
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
    ///     Snapshot missing genres from the cache (no DB query), translate them one by one
    ///     (OpenAI/NLLB serialised), then bulk-insert the resulting rows in a single
    ///     <c>SaveChangesAsync</c>. Successful rows are also pushed to the cache so subsequent
    ///     requests see them immediately.
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

        var batch = new List<SoftwareGenreTranslation>(missing.Count);

        foreach((int id, string englishName) in missing)
        {
            ct.ThrowIfCancellationRequested();

            if(string.IsNullOrWhiteSpace(englishName)) continue;

            (string translated, string error) = await translationService.TranslateAsync(
                englishName, languageCode, progress: null, plainText: true, domainContext: domainContext);

            if(string.IsNullOrWhiteSpace(translated))
            {
                logger.LogWarning(
                    "SoftwareGenre provider: failed to translate genre {GenreId} (\"{English}\") into {Lang}: {Error}",
                    id, englishName, languageCode, error ?? "no result");

                continue;
            }

            // Defensive: trim to the column max length (varchar(128)).
            if(translated.Length > 128) translated = translated[..128];

            batch.Add(new SoftwareGenreTranslation
            {
                GenreId      = id,
                LanguageCode = languageCode,
                Name         = translated
            });
        }

        if(batch.Count == 0) return 0;

        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        MarechaiContext               ctx   = scope.ServiceProvider.GetRequiredService<MarechaiContext>();

        ctx.SoftwareGenreTranslations.AddRange(batch);
        await ctx.SaveChangesAsync(ct);

        foreach(SoftwareGenreTranslation row in batch) cache.Upsert(row.GenreId, row.LanguageCode, row.Name);

        return batch.Count;
    }
}
