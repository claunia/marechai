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
///     <see cref="ITranslationProvider" /> for <see cref="SoftwareScreenshotGroup" />. Mirrors
///     the <see cref="SoftwarePromoArtGroupTranslationProvider" /> pattern: no in-memory cache,
///     each sweep anti-joins the groups table against
///     <see cref="SoftwareScreenshotGroupTranslation" /> for the target language and translates
///     the missing rows serially via <see cref="TranslationService" />, flushing in batches.
/// </summary>
public sealed class SoftwareScreenshotGroupTranslationProvider(
    IServiceScopeFactory                                  scopeFactory,
    TranslationService                                    translationService,
    IOptionsMonitor<TranslationOptions>                   options,
    ILogger<SoftwareScreenshotGroupTranslationProvider>   logger) : ITranslationProvider
{
    /// <summary>
    ///     Number of translations to accumulate before flushing to the database. Keeps the
    ///     transaction window short so a worker cancellation mid-sweep doesn't lose more than
    ///     this many already-translated rows.
    /// </summary>
    const int FlushBatchSize = 25;

    public string Name => "SoftwareScreenshotGroup";

    public Task EnsureCacheLoadedAsync(CancellationToken ct) => Task.CompletedTask;

    public Task<int> DiscoverNewItemsAsync(CancellationToken ct) => Task.FromResult(0);

    /// <summary>
    ///     Anti-join query for groups lacking a translation row in <paramref name="languageCode" />,
    ///     translate them in bounded waves, then bulk-insert in
    ///     <see cref="FlushBatchSize" />-sized chunks. Returns the number of rows inserted.
    /// </summary>
    public async Task<int> TranslateMissingAsync(string languageCode, CancellationToken ct)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        MarechaiContext               ctx   = scope.ServiceProvider.GetRequiredService<MarechaiContext>();

        List<(int Id, string Name)> missing = (await ctx.SoftwareScreenshotGroups
                                                        .AsNoTracking()
                                                        .Where(g => !ctx.SoftwareScreenshotGroupTranslations
                                                                        .Any(t => t.GroupId      == g.Id &&
                                                                                  t.LanguageCode == languageCode))
                                                        .OrderBy(g => g.Id)
                                                        .Select(g => new { g.Id, g.Name })
                                                        .ToListAsync(ct))
                                              .Select(x => (x.Id, x.Name))
                                              .ToList();

        if(missing.Count == 0) return 0;

        logger.LogInformation(
            "SoftwareScreenshotGroup provider: {Count} groups missing translation into {Lang}.",
            missing.Count, languageCode);

        // Domain hint passed to OpenAI so short label translations stay in the right register
        // (e.g. "Title Screen", "Gameplay", "Cutscene", "Main Menu").
        const string domainContext =
            "The text is the name of a group of screenshots from a piece of software " +
            "(e.g. 'Title Screen', 'Gameplay', 'Cutscene', 'Main Menu', 'Game Over'). " +
            "Keep brand names, product names and proper nouns unchanged.";

        var inserted = 0;
        var batch    = new List<SoftwareScreenshotGroupTranslation>(FlushBatchSize);

        for(var index = 0; index < missing.Count;)
        {
            int maxParallelTranslations =
                await BackgroundTranslationSettings.WaitForAvailableParallelismAsync(options, ct);
            var wave = new List<Task<SoftwareScreenshotGroupTranslation>>(
                System.Math.Min(maxParallelTranslations, missing.Count - index));

            for(var launched = 0; launched < maxParallelTranslations && index < missing.Count; launched++, index++)
            {
                (int id, string englishName) = missing[index];

                wave.Add(TranslateOneAsync(id, englishName));
            }

            SoftwareScreenshotGroupTranslation[] translatedWave = await Task.WhenAll(wave);

            foreach(SoftwareScreenshotGroupTranslation row in translatedWave)
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

        async Task<SoftwareScreenshotGroupTranslation> TranslateOneAsync(int id, string englishName)
        {
            ct.ThrowIfCancellationRequested();

            if(string.IsNullOrWhiteSpace(englishName)) return null;

            (string translated, string error) = await translationService.TranslateAsync(
                englishName, languageCode, progress: null, plainText: true, domainContext: domainContext);

            if(string.IsNullOrWhiteSpace(translated))
            {
                logger.LogWarning(
                    "SoftwareScreenshotGroup provider: failed to translate group {GroupId} (\"{English}\") into {Lang}: {Error}",
                    id, englishName, languageCode, error ?? "no result");

                return null;
            }

            if(translated.Length > 256) translated = translated[..256];

            return new SoftwareScreenshotGroupTranslation
            {
                GroupId      = id,
                LanguageCode = languageCode,
                Name         = translated
            };
        }

        async Task PersistBatchAsync(List<SoftwareScreenshotGroupTranslation> rows, CancellationToken saveCt)
        {
            ctx.SoftwareScreenshotGroupTranslations.AddRange(rows);
            await ctx.SaveChangesAsync(saveCt);
        }
    }
}
