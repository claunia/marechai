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
using Marechai.Server.Suggestions;
using Marechai.Translation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Marechai.Server.Services;

/// <summary>
///     <see cref="ITranslationProvider" /> for the <see cref="SoftwareAttributeString" /> pool. Both
///     the <c>Key</c> and <c>Value</c> columns of every non-Rating <see cref="SoftwareAttribute" /> are
///     pooled into this single table, and translated once per language.
/// </summary>
/// <remarks>
///     The Rating category is NEVER pooled — rating system codes (ESRB "T", PEGI "16+", etc.) are
///     language-independent and the controllers return them verbatim.
/// </remarks>
public sealed class SoftwareAttributeTranslationProvider(
    IServiceScopeFactory                              scopeFactory,
    TranslationService                                translationService,
    SoftwareAttributeTranslationCache                 cache,
    ILogger<SoftwareAttributeTranslationProvider>    logger) : ITranslationProvider
{
    public string Name => "SoftwareAttribute";

    public Task EnsureCacheLoadedAsync(CancellationToken ct) => cache.EnsureLoadedAsync(ct);

    /// <summary>
    ///     Discovers new pool strings: scans every distinct (non-Rating) <c>Key</c> and <c>Value</c>
    ///     across the <c>SoftwareAttributes</c> table, normalises each, and inserts any text not yet
    ///     in the pool. Newly inserted rows are registered in the cache so the per-language sweep
    ///     picks them up immediately. Returns the count of inserted rows.
    /// </summary>
    public async Task<int> DiscoverNewItemsAsync(CancellationToken ct)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        MarechaiContext               ctx   = scope.ServiceProvider.GetRequiredService<MarechaiContext>();

        // Two cheap distinct projections + in-memory union. Rating attributes are excluded — they're
        // language-independent rating codes.
        const string ratingCategory = SoftwareReleaseSuggestionApplier.AttributeCategoryRating;

        List<string> distinctKeys = await ctx.SoftwareAttributes
                                             .AsNoTracking()
                                             .Where(a => a.Category != ratingCategory)
                                             .Select(a => a.Key)
                                             .Distinct()
                                             .ToListAsync(ct);

        List<string> distinctValues = await ctx.SoftwareAttributes
                                               .AsNoTracking()
                                               .Where(a => a.Category != ratingCategory)
                                               .Select(a => a.Value)
                                               .Distinct()
                                               .ToListAsync(ct);

        // Normalise + de-duplicate against the cache. Use a HashSet to coalesce keys/values that
        // collide after normalisation (NBSP variants, casing collapse via DB collation).
        var unseen = new HashSet<string>(StringComparer.Ordinal);

        foreach(string raw in distinctKeys.Concat(distinctValues))
        {
            string n = SoftwareAttributeTranslationCache.NormalizeText(raw);
            if(string.IsNullOrEmpty(n)) continue;

            if(cache.TryGetIdByText(n, out _)) continue;

            unseen.Add(n);
        }

        if(unseen.Count == 0) return 0;

        // Re-check against the DB in case a previous tick inserted these strings before the cache
        // was warmed (defensive — should never trip in steady state). Also handles MariaDB's
        // case-insensitive collation: an in-memory ordinal HashSet may surface "color" + "Color"
        // as two unseen entries, but the DB unique index treats them as one — let the DB tell us.
        List<string> existingInDb = await ctx.SoftwareAttributeStrings
                                             .AsNoTracking()
                                             .Where(s => unseen.Contains(s.Text))
                                             .Select(s => s.Text)
                                             .ToListAsync(ct);

        foreach(string already in existingInDb) unseen.Remove(already);

        if(unseen.Count == 0) return 0;

        var batch = unseen
                   .Select(t => new SoftwareAttributeString { Text = t.Length > 512 ? t[..512] : t })
                   .ToList();

        ctx.SoftwareAttributeStrings.AddRange(batch);

        try
        {
            await ctx.SaveChangesAsync(ct);
        }
        catch(DbUpdateException ex)
        {
            // Race against a concurrent tick or a manual insert — log and skip; next tick will
            // discover whatever survived the unique-index collision.
            logger.LogWarning(ex,
                              "SoftwareAttribute provider: bulk insert of {Count} pool strings hit a unique-index collision; will retry next tick.",
                              batch.Count);

            return 0;
        }

        foreach(SoftwareAttributeString s in batch) cache.RegisterString(s.Id, s.Text);

        return batch.Count;
    }

    /// <summary>
    ///     Snapshot pool strings missing translation for <paramref name="languageCode" />, translate
    ///     each via OpenAI/NLLB serially, then bulk-insert the results in a single
    ///     <c>SaveChangesAsync</c>. Successful rows are pushed to the cache.
    /// </summary>
    public async Task<int> TranslateMissingAsync(string languageCode, CancellationToken ct)
    {
        IReadOnlyList<(int Id, string Text)> missing = cache.SnapshotMissingByLanguage(languageCode);

        if(missing.Count == 0) return 0;

        logger.LogInformation("SoftwareAttribute provider: {Count} pool strings missing translation into {Lang}.",
                              missing.Count, languageCode);
        // Domain hint for OpenAI — the pool mixes spec keys ("Minimum RAM Required",
        // "Video Resolutions Supported") AND spec values ("1 MB", "Mouse, Keyboard",
        // "Windows 95"). Without this context, short labels translate in the wrong register
        // (e.g. "Mouse" → the animal in some languages instead of the input device).
        const string domainContext =
            "The text is a video-game / software technical specification — either the NAME of a "
          + "specification (e.g. \"Minimum RAM Required\", \"Operating System\") or its VALUE "
          + "(e.g. \"1 MB\", \"Mouse, Keyboard\", \"Windows 95\"). Use computer / video-game "
          + "terminology, NOT general-purpose translations. Keep brand names, version numbers, "
          + "units (MB, GB, MHz) and product titles unchanged.";
        var batch = new List<SoftwareAttributeStringTranslation>(missing.Count);

        foreach((int id, string text) in missing)
        {
            ct.ThrowIfCancellationRequested();

            if(string.IsNullOrWhiteSpace(text)) continue;

            (string translated, string error) = await translationService.TranslateAsync(
                text, languageCode, progress: null, plainText: true);

            if(string.IsNullOrWhiteSpace(translated))
            {
                logger.LogWarning(
                    "SoftwareAttribute provider: failed to translate pool string {Id} (\"{Text}\") into {Lang}: {Error}",
                    id, text, languageCode, error ?? "no result");

                continue;
            }

            // Defensive: trim to the column max length (varchar(512)).
            if(translated.Length > 512) translated = translated[..512];

            batch.Add(new SoftwareAttributeStringTranslation
            {
                StringId     = id,
                LanguageCode = languageCode,
                Translation  = translated
            });
        }

        if(batch.Count == 0) return 0;

        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        MarechaiContext               ctx   = scope.ServiceProvider.GetRequiredService<MarechaiContext>();

        ctx.SoftwareAttributeStringTranslations.AddRange(batch);
        await ctx.SaveChangesAsync(ct);

        foreach(SoftwareAttributeStringTranslation row in batch)
            cache.Upsert(row.StringId, row.LanguageCode, row.Translation);

        return batch.Count;
    }
}
