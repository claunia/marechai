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
///     <see cref="GetName" /> falls back to the English name when no translation exists for the
///     requested language. Lookup with an unknown <paramref name="genreId" /> returns
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
    ///     translation exists for that language. Returns <see cref="string.Empty" /> when the genreId is
    ///     unknown — never throws. The cache MUST have been loaded first (callers in the request
    ///     pipeline rely on the eager-warm in <c>Program.cs</c>).
    /// </summary>
    public string GetName(int genreId, string languageCode)
    {
        if(string.IsNullOrEmpty(languageCode) || string.Equals(languageCode, "eng", StringComparison.Ordinal))
            return _englishNames.TryGetValue(genreId, out string english) ? english : string.Empty;

        if(_translations.TryGetValue(genreId, out ConcurrentDictionary<string, string> langs) &&
           langs.TryGetValue(languageCode, out string translated)                              &&
           !string.IsNullOrWhiteSpace(translated))
            return translated;

        return _englishNames.TryGetValue(genreId, out string fallback) ? fallback : string.Empty;
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
