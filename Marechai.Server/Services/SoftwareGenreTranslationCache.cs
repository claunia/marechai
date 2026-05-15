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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Marechai.Server.Services;

/// <summary>
///     Process-lifetime singleton holding every <see cref="SoftwareGenre" /> name plus every
///     translation pair from <see cref="SoftwareGenreTranslation" />. Loaded ONCE on first access (the
///     server eager-warms it on startup), then kept current entirely in memory: the
///     <see cref="TranslationWorker" /> mutates it via <see cref="RegisterGenre" /> /
///     <see cref="Upsert" />, and the controllers READ it for every request — eliminating the per-RTT
///     DB cost on the genre endpoints.
/// </summary>
/// <remarks>
///     <para>English (<c>"eng"</c>) is treated as the identity copy of the canonical English
///     <see cref="SoftwareGenre.Name" /> column and is NEVER stored in the translations dictionary;
///     <see cref="GetNameAsync" /> falls back to the English name when no translation exists for
///     the requested language. If the <paramref name="genreId" /> is not yet known to the cache
///     (e.g. inserted by the importer between worker ticks), <see cref="GetNameAsync" /> performs a
///     single DB round-trip to load the row + every translation it has, registers them, and
///     retries the lookup. Only when the DB also has no matching row does it return
///     <see cref="string.Empty" />.</para>
///     <para>Mutation is concurrent-safe (all state is in <see cref="ConcurrentDictionary{TKey,TValue}" />)
///     so the worker can keep updating while controllers read.</para>
/// </remarks>
public sealed class SoftwareGenreTranslationCache(IServiceScopeFactory                scopeFactory,
                                                  ILogger<SoftwareGenreTranslationCache> logger)
{
    /// <summary>genreId → canonical English Name (mirror of <see cref="SoftwareGenre.Name" />).</summary>
    readonly ConcurrentDictionary<int, string> _englishNames = new();

    /// <summary>genreId → (langCode → translated Name). langCode is ISO-639-3 (e.g. "spa", "deu"); "eng" is NOT stored.</summary>
    readonly ConcurrentDictionary<int, ConcurrentDictionary<string, string>> _translations = new();

    readonly SemaphoreSlim _loadGate = new(1, 1);
    bool                   _loaded;

    /// <summary>
    ///     Highest <see cref="SoftwareGenre.Id" /> seen at the last load. The
    ///     <see cref="TranslationWorker" /> queries only rows with <c>Id &gt; LastSeenGenreId</c> on
    ///     each tick to discover importer-added genres without re-streaming the whole table.
    /// </summary>
    public int LastSeenGenreId { get; private set; }

    /// <summary>
    ///     Loads the cache from the DB if not yet loaded. Idempotent &amp; thread-safe; the second
    ///     concurrent caller waits on the semaphore and then returns immediately. Safe to call from
    ///     both the eager-warm code path in <c>Program.cs</c> and from any reader.
    /// </summary>
    public async Task EnsureLoadedAsync(CancellationToken ct = default)
    {
        if(_loaded) return;

        await _loadGate.WaitAsync(ct);

        try
        {
            if(_loaded) return;

            await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
            MarechaiContext               ctx   = scope.ServiceProvider.GetRequiredService<MarechaiContext>();

            // Two queries (both no-tracking, projected). Streaming all genres + all translations into
            // memory once at startup costs O(N+M) RAM and saves O(2) RTTs per request lifetime.
            var allGenres = await ctx.SoftwareGenres
                                     .AsNoTracking()
                                     .OrderBy(g => g.Id)
                                     .Select(g => new { g.Id, g.Name })
                                     .ToListAsync(ct);

            foreach(var g in allGenres)
            {
                _englishNames[g.Id] = g.Name ?? string.Empty;

                if(g.Id > LastSeenGenreId) LastSeenGenreId = g.Id;
            }

            List<SoftwareGenreTranslation> allTranslations =
                await ctx.SoftwareGenreTranslations
                         .AsNoTracking()
                         .ToListAsync(ct);

            foreach(SoftwareGenreTranslation t in allTranslations)
            {
                if(string.IsNullOrEmpty(t.LanguageCode) || t.Name is null) continue;

                ConcurrentDictionary<string, string> langs =
                    _translations.GetOrAdd(t.GenreId, _ => new ConcurrentDictionary<string, string>());

                langs[t.LanguageCode] = t.Name;
            }

            logger.LogInformation(
                "SoftwareGenreTranslationCache loaded {GenreCount} genres and {TranslationCount} translations.",
                allGenres.Count, allTranslations.Count);

            _loaded = true;
        }
        finally
        {
            _loadGate.Release();
        }
    }

    /// <summary>
    ///     Returns the translated name for <paramref name="genreId" /> in
    ///     <paramref name="languageCode" />, falling back to the canonical English name if no
    ///     translation exists for that language. If the id is not yet known to the cache (e.g. the
    ///     row was inserted after the eager warm and before the next worker tick), performs a single
    ///     DB round-trip to load the row + every translation it has and retries the lookup. Returns
    ///     <see cref="string.Empty" /> only when the DB also has no matching row — never throws.
    /// </summary>
    public async Task<string> GetNameAsync(int genreId, string languageCode, CancellationToken ct = default)
    {
        if(string.IsNullOrEmpty(languageCode) || string.Equals(languageCode, "eng", StringComparison.Ordinal))
        {
            if(_englishNames.TryGetValue(genreId, out string english)) return english;

            // English miss → check the DB. If the loader registered the row, _englishNames now has it.
            if(await LoadGenreFromDbAsync(genreId, ct).ConfigureAwait(false) &&
               _englishNames.TryGetValue(genreId, out english))
                return english;

            return string.Empty;
        }

        if(_translations.TryGetValue(genreId, out ConcurrentDictionary<string, string> langs) &&
           langs.TryGetValue(languageCode, out string translated)                              &&
           !string.IsNullOrWhiteSpace(translated))
            return translated;

        if(_englishNames.TryGetValue(genreId, out string fallback)) return fallback;

        // Both translations AND English are missing → DB miss-load. Retry the lookups in the same
        // order (translation first, then English fallback) before giving up.
        if(!await LoadGenreFromDbAsync(genreId, ct).ConfigureAwait(false)) return string.Empty;

        if(_translations.TryGetValue(genreId, out langs) &&
           langs.TryGetValue(languageCode, out translated)                                 &&
           !string.IsNullOrWhiteSpace(translated))
            return translated;

        return _englishNames.TryGetValue(genreId, out fallback) ? fallback : string.Empty;
    }

    /// <summary>
    ///     Loads a single genre row + every translation it has from the DB and pushes them into
    ///     the in-memory dictionaries via <see cref="RegisterGenre" /> / <see cref="Upsert" />.
    ///     Returns <c>true</c> if a row was found, <c>false</c> if the id is not in the DB
    ///     (data-integrity gap or transient race). Idempotent under concurrent calls — the
    ///     <see cref="ConcurrentDictionary{TKey,TValue}" /> upserts coalesce duplicate writes for
    ///     the same id. Does NOT take <see cref="_loadGate" />; that semaphore is for the bulk warm
    ///     only.
    /// </summary>
    async Task<bool> LoadGenreFromDbAsync(int genreId, CancellationToken ct)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        MarechaiContext               ctx   = scope.ServiceProvider.GetRequiredService<MarechaiContext>();

        var genre = await ctx.SoftwareGenres
                             .AsNoTracking()
                             .Where(g => g.Id == genreId)
                             .Select(g => new { g.Id, g.Name })
                             .FirstOrDefaultAsync(ct)
                             .ConfigureAwait(false);

        if(genre is null) return false;

        RegisterGenre(genre.Id, genre.Name);

        List<SoftwareGenreTranslation> translations =
            await ctx.SoftwareGenreTranslations
                     .AsNoTracking()
                     .Where(t => t.GenreId == genreId)
                     .ToListAsync(ct)
                     .ConfigureAwait(false);

        foreach(SoftwareGenreTranslation t in translations)
        {
            if(string.IsNullOrEmpty(t.LanguageCode) || t.Name is null) continue;

            Upsert(t.GenreId, t.LanguageCode, t.Name);
        }

        logger.LogDebug(
            "SoftwareGenreTranslationCache miss-loaded genre {GenreId} (\"{Name}\") + {TranslationCount} translations.",
            genre.Id, genre.Name, translations.Count);

        return true;
    }

    /// <summary>
    ///     Adds or updates a single translation in-memory. Used by <see cref="TranslationWorker" />
    ///     after each successful DB insert so subsequent requests see the new translation immediately.
    /// </summary>
    public void Upsert(int genreId, string languageCode, string translatedName)
    {
        if(string.IsNullOrEmpty(languageCode) || string.Equals(languageCode, "eng", StringComparison.Ordinal) ||
           translatedName is null)
            return;

        ConcurrentDictionary<string, string> langs =
            _translations.GetOrAdd(genreId, _ => new ConcurrentDictionary<string, string>());

        langs[languageCode] = translatedName;
    }

    /// <summary>
    ///     Registers a brand-new genre (typically discovered via the worker's delta query
    ///     <c>WHERE Id &gt; LastSeenGenreId</c>) so subsequent
    ///     <see cref="SnapshotGenresMissingLanguage" /> calls include it. Updates
    ///     <see cref="LastSeenGenreId" /> if the new id is higher.
    /// </summary>
    public void RegisterGenre(int genreId, string englishName)
    {
        _englishNames[genreId] = englishName ?? string.Empty;

        if(genreId > LastSeenGenreId) LastSeenGenreId = genreId;
    }

    /// <summary>
    ///     Returns the snapshot of all known genres that currently have NO translation for
    ///     <paramref name="languageCode" />. Callers receive a fresh <see cref="List{T}" /> so further
    ///     mutation in the cache during translation does not affect the snapshot. Returns an empty
    ///     list for <c>"eng"</c> (English never needs translation).
    /// </summary>
    public IReadOnlyList<(int Id, string EnglishName)> SnapshotGenresMissingLanguage(string languageCode)
    {
        if(string.IsNullOrEmpty(languageCode) || string.Equals(languageCode, "eng", StringComparison.Ordinal))
            return [];

        var missing = new List<(int Id, string EnglishName)>();

        foreach(KeyValuePair<int, string> kv in _englishNames)
        {
            if(string.IsNullOrWhiteSpace(kv.Value)) continue;

            bool hasLang = _translations.TryGetValue(kv.Key, out ConcurrentDictionary<string, string> langs) &&
                           langs.ContainsKey(languageCode);

            if(!hasLang) missing.Add((kv.Key, kv.Value));
        }

        return missing;
    }
}
