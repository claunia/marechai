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
///     <see cref="ITranslationProvider" /> for <see cref="PeopleBySoftware.Role" />. Carries NO
///     in-memory cache singleton: read endpoints project the localized role directly from the
///     DB via a LEFT JOIN sub-query against <see cref="PeopleBySoftwareRoleTranslation" />. The
///     worker still fills the table in the background so subsequent requests in each non-English
///     language find a row instead of falling back to the canonical English role text.
/// </summary>
/// <remarks>
///     <see cref="EnsureCacheLoadedAsync" /> and <see cref="DiscoverNewItemsAsync" /> are no-ops
///     because <see cref="TranslateMissingAsync" /> queries the DB every tick anyway with an
///     anti-join over distinct role strings. The string pool is keyed by the role text itself
///     (not by a parent FK) so multiple credits sharing the same role (e.g. "Programmer",
///     "Composer", "Designer") reuse a single translation row per language.
///     <para />
///     NBSP (<c>U+00A0</c>) characters present in canonical role strings are preserved here:
///     <see cref="TranslationService.TranslateAsync" /> normalises them to regular spaces before
///     forwarding to OpenAI / NLLB, but the DB key remains NBSP-bearing so the LEFT JOIN in the
///     read endpoint matches the parent <see cref="PeopleBySoftware.Role" /> column exactly.
/// </remarks>
public sealed class PeopleBySoftwareRoleTranslationProvider(
    IServiceScopeFactory                              scopeFactory,
    TranslationService                                translationService,
    ILogger<PeopleBySoftwareRoleTranslationProvider> logger) : ITranslationProvider
{
    /// <summary>
    ///     Number of translations to accumulate before flushing to the database. Keeps the
    ///     transaction window short so a worker cancellation mid-sweep doesn't lose more than
    ///     this many already-translated rows.
    /// </summary>
    const int FlushBatchSize = 25;

    public string Name => "PeopleBySoftwareRole";

    public Task EnsureCacheLoadedAsync(CancellationToken ct) => Task.CompletedTask;

    public Task<int> DiscoverNewItemsAsync(CancellationToken ct) => Task.FromResult(0);

    /// <summary>
    ///     Anti-join query for distinct credit role strings lacking a translation row in
    ///     <paramref name="languageCode" />, translate each via
    ///     <see cref="TranslationService.TranslateAsync" /> serially (preserves OpenAI rate-limit
    ///     safety), then bulk-insert in <see cref="FlushBatchSize" />-sized chunks. Returns the
    ///     number of rows inserted.
    /// </summary>
    public async Task<int> TranslateMissingAsync(string languageCode, CancellationToken ct)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        MarechaiContext               ctx   = scope.ServiceProvider.GetRequiredService<MarechaiContext>();

        // Distinct non-empty roles that have no translation row in the requested language.
        // Joining on the free-form text means multiple credits sharing the role "Programmer"
        // produce ONE row in the missing list — exactly the dedup the user asked for.
        List<string> missing = await ctx.PeopleBySoftware
                                        .AsNoTracking()
                                        .Where(p => p.Role != null && p.Role != "")
                                        .Where(p => !ctx.PeopleBySoftwareRoleTranslations
                                                        .Any(t => t.RoleText     == p.Role &&
                                                                  t.LanguageCode == languageCode))
                                        .Select(p => p.Role)
                                        .Distinct()
                                        .OrderBy(s => s)
                                        .ToListAsync(ct);

        if(missing.Count == 0) return 0;

        logger.LogInformation(
            "PeopleBySoftwareRole provider: {Count} roles missing translation into {Lang}.",
            missing.Count, languageCode);

        // Domain hint passed to OpenAI so short role translations stay in the right register
        // (e.g. "Director" → "Director" film/credit role, NOT a corporate executive title).
        const string domainContext =
            "The text is a credit role from a video-game / software product (e.g. 'Programmer', " +
            "'Composer', 'Designer', 'Producer', 'Writer', 'Director', 'Artist', 'Tester', " +
            "'Voice acting', 'Motion capture', 'Special thanks'). " +
            "Translate to the conventional credit-role wording used in the target language's " +
            "video-game / software industry. " +
            "Keep brand names, studio names, version numbers, acronyms (e.g. QA, AI, UI, SFX) " +
            "and copyright notices unchanged.";

        var inserted = 0;
        var batch    = new List<PeopleBySoftwareRoleTranslation>(FlushBatchSize);

        foreach(string englishRole in missing)
        {
            ct.ThrowIfCancellationRequested();

            if(string.IsNullOrWhiteSpace(englishRole)) continue;

            (string translated, string error) = await translationService.TranslateAsync(
                englishRole, languageCode, progress: null, plainText: true,
                domainContext: domainContext);

            if(string.IsNullOrWhiteSpace(translated))
            {
                logger.LogWarning(
                    "PeopleBySoftwareRole provider: failed to translate role \"{English}\" into {Lang}: {Error}",
                    englishRole, languageCode, error ?? "no result");

                continue;
            }

            // Defensive: trim to the column max length (varchar(256)).
            if(translated.Length > 256) translated = translated[..256];

            string canonical = englishRole.Length > 256 ? englishRole[..256] : englishRole;

            batch.Add(new PeopleBySoftwareRoleTranslation
            {
                RoleText     = canonical,
                LanguageCode = languageCode,
                Translation  = translated
            });

            if(batch.Count < FlushBatchSize) continue;

            ctx.PeopleBySoftwareRoleTranslations.AddRange(batch);
            await ctx.SaveChangesAsync(ct);
            inserted += batch.Count;
            batch.Clear();
        }

        if(batch.Count > 0)
        {
            ctx.PeopleBySoftwareRoleTranslations.AddRange(batch);
            await ctx.SaveChangesAsync(ct);
            inserted += batch.Count;
        }

        return inserted;
    }
}
