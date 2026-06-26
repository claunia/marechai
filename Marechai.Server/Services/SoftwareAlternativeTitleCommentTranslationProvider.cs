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
///     <see cref="ITranslationProvider" /> for <see cref="SoftwareAlternativeTitle.Comment" />.
///     Carries NO in-memory cache singleton: read endpoints project the localized comment
///     directly from the DB via a LEFT JOIN sub-query against
///     <see cref="SoftwareAlternativeTitleCommentTranslation" />. The worker still fills the
///     table in the background so subsequent requests in each non-English language find a row
///     instead of falling back to the canonical English comment.
/// </summary>
/// <remarks>
///     <see cref="EnsureCacheLoadedAsync" /> and <see cref="DiscoverNewItemsAsync" /> are no-ops
///     because <see cref="TranslateMissingAsync" /> queries the DB every tick anyway with an
///     anti-join over distinct comment strings. The string pool is keyed by the comment text
///     itself (not by a parent FK) so multiple alternative titles sharing the same comment (e.g.
///     "Japanese title") reuse a single translation row per language.
/// </remarks>
public sealed class SoftwareAlternativeTitleCommentTranslationProvider(
    IServiceScopeFactory                                            scopeFactory,
    TranslationService                                               translationService,
    ILogger<SoftwareAlternativeTitleCommentTranslationProvider> logger) : ITranslationProvider
{
    /// <summary>
    ///     Number of translations to accumulate before flushing to the database. Keeps the
    ///     transaction window short so a worker cancellation mid-sweep doesn't lose more than
    ///     this many already-translated rows.
    /// </summary>
    const int FlushBatchSize = 25;

    public string Name => "SoftwareAlternativeTitleComment";

    public Task EnsureCacheLoadedAsync(CancellationToken ct) => Task.CompletedTask;

    public Task<int> DiscoverNewItemsAsync(CancellationToken ct) => Task.FromResult(0);

    /// <summary>
    ///     Anti-join query for distinct alternative-title comments lacking a translation row in
    ///     <paramref name="languageCode" />, translate each via
    ///     <see cref="TranslationService.TranslateAsync" /> serially (preserves OpenAI rate-limit
    ///     safety), then bulk-insert in <see cref="FlushBatchSize" />-sized chunks. Returns the
    ///     number of rows inserted.
    /// </summary>
    public async Task<int> TranslateMissingAsync(string languageCode, CancellationToken ct)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        MarechaiContext               ctx   = scope.ServiceProvider.GetRequiredService<MarechaiContext>();

        // Distinct non-empty comments that have no translation row in the requested language.
        // Joining on the free-form text means multiple alternative titles sharing the comment
        // "Japanese title" produce ONE row in the missing list.
        List<string> missing = await ctx.SoftwareAlternativeTitles
                                        .AsNoTracking()
                                        .Where(t => t.Comment != null && t.Comment != "")
                                        .Where(t => !ctx.SoftwareAlternativeTitleCommentTranslations
                                                        .Any(c => c.CommentText  == t.Comment &&
                                                                  c.LanguageCode == languageCode))
                                        .Select(t => t.Comment)
                                        .Distinct()
                                        .OrderBy(s => s)
                                        .ToListAsync(ct);

        if(missing.Count == 0) return 0;

        logger.LogInformation(
            "SoftwareAlternativeTitleComment provider: {Count} comments missing translation into {Lang}.",
            missing.Count, languageCode);

        // Domain hint passed to OpenAI so short comments stay in the right register.
        const string domainContext =
            "The text is a short editorial comment explaining why a video game / software " +
            "alternative title exists (e.g. region, script, or language variant), such as " +
            "'Japanese title', 'European title', 'Working title', 'US release name'.";

        var inserted = 0;
        var batch    = new List<SoftwareAlternativeTitleCommentTranslation>(FlushBatchSize);

        foreach(string englishComment in missing)
        {
            ct.ThrowIfCancellationRequested();

            if(string.IsNullOrWhiteSpace(englishComment)) continue;

            (string translated, string error) = await translationService.TranslateAsync(
                englishComment, languageCode, progress: null, plainText: true,
                domainContext: domainContext);

            if(string.IsNullOrWhiteSpace(translated))
            {
                logger.LogWarning(
                    "SoftwareAlternativeTitleComment provider: failed to translate comment \"{English}\" into {Lang}: {Error}",
                    englishComment, languageCode, error ?? "no result");

                continue;
            }

            // Defensive: trim to the column max length (varchar(500)).
            if(translated.Length > 500) translated = translated[..500];

            string canonical = englishComment.Length > 500 ? englishComment[..500] : englishComment;

            batch.Add(new SoftwareAlternativeTitleCommentTranslation
            {
                CommentText  = canonical,
                LanguageCode = languageCode,
                Translation  = translated
            });

            if(batch.Count < FlushBatchSize) continue;

            ctx.SoftwareAlternativeTitleCommentTranslations.AddRange(batch);
            await ctx.SaveChangesAsync(ct);
            inserted += batch.Count;
            batch.Clear();
        }

        if(batch.Count > 0)
        {
            ctx.SoftwareAlternativeTitleCommentTranslations.AddRange(batch);
            await ctx.SaveChangesAsync(ct);
            inserted += batch.Count;
        }

        return inserted;
    }
}
