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
using Microsoft.Extensions.Logging;

namespace Marechai.Server.Services;

/// <summary>
///     <see cref="ITranslationProvider" /> for <see cref="SoftwareScreenshot.Caption" />. Carries
///     NO in-memory cache singleton: read endpoints project the localized caption directly from
///     the DB via a LEFT JOIN sub-query (the per-request RTT was already paid to fetch the
///     screenshot row). The worker fills <see cref="SoftwareScreenshotCaptionTranslation" /> in
///     the background so subsequent non-English requests find a row instead of falling back to
///     the canonical English caption.
/// </summary>
/// <remarks>
///     <see cref="EnsureCacheLoadedAsync" /> and <see cref="DiscoverNewItemsAsync" /> are no-ops
///     because <see cref="TranslateMissingAsync" /> queries the DB every tick anyway with an
///     anti-join. Captions can be very numerous (one per screenshot, no cross-screenshot dedup),
///     so the worker batches the SaveChanges to keep the transaction moderate.
/// </remarks>
public sealed class SoftwareScreenshotCaptionTranslationProvider(
    IServiceScopeFactory                                       scopeFactory,
    TranslationService                                         translationService,
    ILogger<SoftwareScreenshotCaptionTranslationProvider>      logger) : ITranslationProvider
{
    /// <summary>
    ///     Number of translations to accumulate before flushing to the database. Keeps the
    ///     transaction window short so a worker cancellation mid-sweep doesn't lose more than
    ///     this many already-translated rows.
    /// </summary>
    const int FlushBatchSize = 25;

    public string Name => "SoftwareScreenshotCaption";

    public Task EnsureCacheLoadedAsync(CancellationToken ct) => Task.CompletedTask;

    public Task<int> DiscoverNewItemsAsync(CancellationToken ct) => Task.FromResult(0);

    /// <summary>
    ///     Anti-join query for screenshots whose <see cref="SoftwareScreenshot.Caption" /> is
    ///     non-empty AND that lack a translation row in <paramref name="languageCode" />,
    ///     translate each via <see cref="TranslationService.TranslateAsync" /> serially (preserves
    ///     OpenAI rate-limit safety), then bulk-insert in <see cref="FlushBatchSize" /> chunks.
    ///     Returns the number of rows inserted.
    /// </summary>
    public async Task<int> TranslateMissingAsync(string languageCode, CancellationToken ct)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        MarechaiContext               ctx   = scope.ServiceProvider.GetRequiredService<MarechaiContext>();

        List<(Guid Id, string Caption)> missing = (await ctx.SoftwareScreenshots
                                                            .AsNoTracking()
                                                            .Where(s => s.Caption != null && s.Caption != "" &&
                                                                        !ctx.SoftwareScreenshotCaptionTranslations
                                                                            .Any(t => t.ScreenshotId == s.Id &&
                                                                                      t.LanguageCode == languageCode))
                                                            .OrderBy(s => s.CreatedOn)
                                                            .ThenBy(s => s.Id)
                                                            .Select(s => new { s.Id, s.Caption })
                                                            .ToListAsync(ct))
                                                  .Select(x => (x.Id, x.Caption))
                                                  .ToList();

        if(missing.Count == 0) return 0;

        logger.LogInformation(
            "SoftwareScreenshotCaption provider: {Count} captions missing translation into {Lang}.",
            missing.Count, languageCode);

        // Domain hint passed to OpenAI so short label translations stay in the right register
        // (e.g. in-game / in-application screenshot captions).
        const string domainContext =
            "The text is a caption / descriptive label for an in-game or in-application screenshot of a piece of " +
            "software (e.g. 'Title screen', 'Main menu', 'Game over screen', 'Boss fight at level 3', " +
            "'Settings dialog', 'Inventory view'). Keep brand names, character names, level names, version " +
            "numbers and copyright notices unchanged.";

        var inserted = 0;
        var batch    = new List<SoftwareScreenshotCaptionTranslation>(FlushBatchSize);

        foreach((Guid id, string englishCaption) in missing)
        {
            ct.ThrowIfCancellationRequested();

            if(string.IsNullOrWhiteSpace(englishCaption)) continue;

            (string translated, string error) = await translationService.TranslateAsync(
                englishCaption, languageCode, progress: null, plainText: true, domainContext: domainContext);

            if(string.IsNullOrWhiteSpace(translated))
            {
                logger.LogWarning(
                    "SoftwareScreenshotCaption provider: failed to translate screenshot {ScreenshotId} (\"{English}\") into {Lang}: {Error}",
                    id, englishCaption, languageCode, error ?? "no result");

                continue;
            }

            batch.Add(new SoftwareScreenshotCaptionTranslation
            {
                ScreenshotId = id,
                LanguageCode = languageCode,
                Caption      = translated
            });

            if(batch.Count < FlushBatchSize) continue;

            ctx.SoftwareScreenshotCaptionTranslations.AddRange(batch);
            await ctx.SaveChangesAsync(ct);
            inserted += batch.Count;
            batch.Clear();
        }

        if(batch.Count > 0)
        {
            ctx.SoftwareScreenshotCaptionTranslations.AddRange(batch);
            await ctx.SaveChangesAsync(ct);
            inserted += batch.Count;
        }

        return inserted;
    }
}
