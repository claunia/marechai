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
///     <see cref="ITranslationProvider" /> for <see cref="SoftwareCover.Caption" />. Carries NO
///     in-memory cache singleton: read endpoints project the localized caption directly from the
///     DB via a LEFT JOIN sub-query against <see cref="SoftwareCoverCaptionTranslation" />. The
///     worker still fills the table in the background so subsequent requests in each non-English
///     language find a row instead of falling back to the canonical English caption.
/// </summary>
/// <remarks>
///     <see cref="EnsureCacheLoadedAsync" /> and <see cref="DiscoverNewItemsAsync" /> are no-ops
///     because <see cref="TranslateMissingAsync" /> queries the DB every tick anyway with an
///     anti-join over distinct caption strings. The string pool is keyed by the caption text
///     itself (not by a parent FK) so multiple covers sharing the same caption (e.g. "Front
///     cover") reuse a single translation row per language.
/// </remarks>
public sealed class SoftwareCoverCaptionTranslationProvider(
    IServiceScopeFactory                              scopeFactory,
    TranslationService                                translationService,
    IOptionsMonitor<TranslationOptions>               options,
    ILogger<SoftwareCoverCaptionTranslationProvider> logger) : ITranslationProvider
{
    /// <summary>
    ///     Number of translations to accumulate before flushing to the database. Keeps the
    ///     transaction window short so a worker cancellation mid-sweep doesn't lose more than
    ///     this many already-translated rows.
    /// </summary>
    const int FlushBatchSize = 25;

    public string Name => "SoftwareCoverCaption";

    public Task EnsureCacheLoadedAsync(CancellationToken ct) => Task.CompletedTask;

    public Task<int> DiscoverNewItemsAsync(CancellationToken ct) => Task.FromResult(0);

    /// <summary>
    ///     Anti-join query for distinct cover captions lacking a translation row in
    ///     <paramref name="languageCode" />, translate them in bounded waves, then bulk-insert in
    ///     <see cref="FlushBatchSize" />-sized chunks. Returns the
    ///     number of rows inserted.
    /// </summary>
    public async Task<int> TranslateMissingAsync(string languageCode, CancellationToken ct)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        MarechaiContext               ctx   = scope.ServiceProvider.GetRequiredService<MarechaiContext>();

        // Distinct non-empty captions that have no translation row in the requested language.
        // Joining on the free-form text means multiple covers sharing the caption "Front cover"
        // produce ONE row in the missing list — exactly the dedup the user asked for.
        List<string> missing = await ctx.SoftwareCovers
                                        .AsNoTracking()
                                        .Where(c => c.Caption != null && c.Caption != "")
                                        .Where(c => !ctx.SoftwareCoverCaptionTranslations
                                                        .Any(t => t.CaptionText  == c.Caption &&
                                                                  t.LanguageCode == languageCode))
                                        .Select(c => c.Caption)
                                        .Distinct()
                                        .OrderBy(s => s)
                                        .ToListAsync(ct);

        if(missing.Count == 0) return 0;

        logger.LogInformation(
            "SoftwareCoverCaption provider: {Count} captions missing translation into {Lang}.",
            missing.Count, languageCode);

        // Domain hint passed to OpenAI so short caption translations stay in the right register
        // (e.g. "Front cover", "Back cover", "Spine", "Box art", "Manual", "Disc label").
        const string domainContext =
            "The text is a caption or descriptive label for a software cover image " +
            "(e.g. 'Front cover', 'Back cover', 'Spine', 'Box art', 'Manual', 'Disc label', " +
            "'Inlay card', 'Limited edition slipcase'). " +
            "Keep brand names, publisher names, version numbers, edition labels and copyright " +
            "notices unchanged.";

        var inserted = 0;
        var batch    = new List<SoftwareCoverCaptionTranslation>(FlushBatchSize);

        for(var index = 0; index < missing.Count;)
        {
            int maxParallelTranslations =
                await BackgroundTranslationSettings.WaitForAvailableParallelismAsync(options, ct);
            var wave = new List<Task<SoftwareCoverCaptionTranslation>>(
                System.Math.Min(maxParallelTranslations, missing.Count - index));

            for(var launched = 0; launched < maxParallelTranslations && index < missing.Count; launched++, index++)
            {
                string englishCaption = missing[index];

                wave.Add(TranslateOneAsync(englishCaption));
            }

            SoftwareCoverCaptionTranslation[] translatedWave = await Task.WhenAll(wave);

            foreach(SoftwareCoverCaptionTranslation row in translatedWave)
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

        async Task<SoftwareCoverCaptionTranslation> TranslateOneAsync(string englishCaption)
        {
            ct.ThrowIfCancellationRequested();

            if(string.IsNullOrWhiteSpace(englishCaption)) return null;

            (string translated, string error) = await translationService.TranslateAsync(
                englishCaption, languageCode, progress: null, plainText: true,
                domainContext: domainContext);

            if(string.IsNullOrWhiteSpace(translated))
            {
                logger.LogWarning(
                    "SoftwareCoverCaption provider: failed to translate caption \"{English}\" into {Lang}: {Error}",
                    englishCaption, languageCode, error ?? "no result");

                return null;
            }

            if(translated.Length > 500) translated = translated[..500];

            string canonical = englishCaption.Length > 500 ? englishCaption[..500] : englishCaption;

            return new SoftwareCoverCaptionTranslation
            {
                CaptionText  = canonical,
                LanguageCode = languageCode,
                Translation  = translated
            };
        }

        async Task PersistBatchAsync(List<SoftwareCoverCaptionTranslation> rows, CancellationToken saveCt)
        {
            ctx.SoftwareCoverCaptionTranslations.AddRange(rows);
            await ctx.SaveChangesAsync(saveCt);
        }
    }
}
