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
///     <see cref="ITranslationProvider" /> for <see cref="SoftwarePromoArtGroup" />. Unlike the
///     genre / attribute providers, this one carries NO in-memory cache singleton: read endpoints
///     project the localized name directly from the DB via a LEFT JOIN sub-query (the per-request
///     RTT was already paid to fetch the group name). The worker still fills
///     <see cref="SoftwarePromoArtGroupTranslation" /> in the background so subsequent requests in
///     each non-English language find a row instead of falling back to English.
/// </summary>
/// <remarks>
///     <see cref="EnsureCacheLoadedAsync" /> and <see cref="DiscoverNewItemsAsync" /> are no-ops
///     because <see cref="TranslateMissingAsync" /> queries the DB every tick anyway with an
///     anti-join. The trade-off vs. the cached providers: each tick costs one extra anti-join query
///     per language, but startup carries no eager-warm cost and the controllers never hold cache
///     state for this entity.
/// </remarks>
public sealed class SoftwarePromoArtGroupTranslationProvider(
    IServiceScopeFactory                                 scopeFactory,
    TranslationService                                   translationService,
    ILogger<SoftwarePromoArtGroupTranslationProvider>    logger) : ITranslationProvider
{
    public string Name => "SoftwarePromoArtGroup";

    public Task EnsureCacheLoadedAsync(CancellationToken ct) => Task.CompletedTask;

    public Task<int> DiscoverNewItemsAsync(CancellationToken ct) => Task.FromResult(0);

    /// <summary>
    ///     Anti-join query for groups lacking a translation row in <paramref name="languageCode" />,
    ///     translate each via <see cref="TranslationService.TranslateAsync" /> serially (preserves
    ///     OpenAI rate-limit safety), then bulk-insert. Returns the number of rows inserted.
    /// </summary>
    public async Task<int> TranslateMissingAsync(string languageCode, CancellationToken ct)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        MarechaiContext               ctx   = scope.ServiceProvider.GetRequiredService<MarechaiContext>();

        List<(int Id, string Name)> missing = (await ctx.SoftwarePromoArtGroups
                                                        .AsNoTracking()
                                                        .Where(g => !ctx.SoftwarePromoArtGroupTranslations
                                                                        .Any(t => t.GroupId      == g.Id &&
                                                                                  t.LanguageCode == languageCode))
                                                        .OrderBy(g => g.Id)
                                                        .Select(g => new { g.Id, g.Name })
                                                        .ToListAsync(ct))
                                              .Select(x => (x.Id, x.Name))
                                              .ToList();

        if(missing.Count == 0) return 0;

        logger.LogInformation(
            "SoftwarePromoArtGroup provider: {Count} groups missing translation into {Lang}.",
            missing.Count, languageCode);

        // Domain hint passed to OpenAI so short label translations stay in the right register
        // (e.g. "Box Art", "Magazine Advertisements", "Trade Show Flyers").
        const string domainContext =
            "The text is the name of a group of promotional art / marketing material for a piece of software " +
            "(e.g. 'Box Art', 'Magazine Advertisements', 'Trade Show Flyers'). " +
            "Keep brand names, product names and proper nouns unchanged.";

        var batch = new List<SoftwarePromoArtGroupTranslation>(missing.Count);

        foreach((int id, string englishName) in missing)
        {
            ct.ThrowIfCancellationRequested();

            if(string.IsNullOrWhiteSpace(englishName)) continue;

            (string translated, string error) = await translationService.TranslateAsync(
                englishName, languageCode, progress: null, plainText: true, domainContext: domainContext);

            if(string.IsNullOrWhiteSpace(translated))
            {
                logger.LogWarning(
                    "SoftwarePromoArtGroup provider: failed to translate group {GroupId} (\"{English}\") into {Lang}: {Error}",
                    id, englishName, languageCode, error ?? "no result");

                continue;
            }

            // Defensive: trim to the column max length (varchar(256)).
            if(translated.Length > 256) translated = translated[..256];

            batch.Add(new SoftwarePromoArtGroupTranslation
            {
                GroupId      = id,
                LanguageCode = languageCode,
                Name         = translated
            });
        }

        if(batch.Count == 0) return 0;

        ctx.SoftwarePromoArtGroupTranslations.AddRange(batch);
        await ctx.SaveChangesAsync(ct);

        return batch.Count;
    }
}
