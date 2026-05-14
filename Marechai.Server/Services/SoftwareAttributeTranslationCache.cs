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
///     Process-lifetime singleton holding every <see cref="SoftwareAttributeString" /> (the pool of
///     unique non-Rating attribute keys/values) plus every <see cref="SoftwareAttributeStringTranslation" />.
///     Loaded ONCE on first access (the server eager-warms it on startup), then kept current entirely
///     in memory: the <see cref="SoftwareAttributeTranslationProvider" /> mutates it via
///     <see cref="RegisterString" /> / <see cref="Upsert" />, and the controllers READ it for every
///     request — eliminating the per-RTT DB cost on the attribute endpoints.
/// </summary>
/// <remarks>
///     <para>English (<c>"eng"</c>) is treated as the identity copy of the canonical pool
///     <see cref="SoftwareAttributeString.Text" /> and is NEVER stored in the translations dictionary;
///     <see cref="GetTranslated" /> falls back to the original normalised text when no translation
///     exists for the requested language. Looking up an unknown text returns the input verbatim
///     (i.e. text not yet discovered by the worker — typical for fresh DB rows between worker ticks).</para>
///     <para>All inputs are NBSP-normalised via <see cref="NormalizeText" /> (U+00A0 → space, trim)
///     so MobyGames-imported rows merge with hand-curated rows in the same pool entry.</para>
/// </remarks>
public sealed class SoftwareAttributeTranslationCache(IServiceScopeFactory                          scopeFactory,
                                                      ILogger<SoftwareAttributeTranslationCache> logger)
{
    /// <summary>normalised Text → StringId. Case-sensitive; MariaDB default collation handles case folding at the unique index.</summary>
    readonly ConcurrentDictionary<string, int> _textToId = new(StringComparer.Ordinal);

    /// <summary>StringId → canonical (normalised) pool Text.</summary>
    readonly ConcurrentDictionary<int, string> _idToText = new();

    /// <summary>StringId → (langCode → translated text). langCode is ISO-639-3 (e.g. "spa", "deu"); "eng" is NOT stored.</summary>
    readonly ConcurrentDictionary<int, ConcurrentDictionary<string, string>> _translations = new();

    readonly SemaphoreSlim _loadGate = new(1, 1);
    bool                   _loaded;

    /// <summary>
    ///     Highest <see cref="SoftwareAttributeString.Id" /> seen at the last load. The provider
    ///     queries only rows with <c>Id &gt; LastSeenStringId</c> on each tick to discover rows
    ///     inserted by the discovery pass without re-streaming the whole table.
    /// </summary>
    public int LastSeenStringId { get; private set; }

    /// <summary>
    ///     Replaces non-breaking spaces (U+00A0) with regular spaces and trims surrounding whitespace.
    ///     Used at every cache boundary (insert / lookup / endpoint read) so MobyGames-imported strings
    ///     merge with hand-curated ones. Returns <see cref="string.Empty" /> for null input.
    /// </summary>
    public static string NormalizeText(string text)
    {
        if(string.IsNullOrEmpty(text)) return string.Empty;

        return text.Replace('\u00A0', ' ').Trim();
    }

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

            var allStrings = await ctx.SoftwareAttributeStrings
                                      .AsNoTracking()
                                      .OrderBy(s => s.Id)
                                      .Select(s => new { s.Id, s.Text })
                                      .ToListAsync(ct);

            foreach(var s in allStrings)
            {
                string normalised = NormalizeText(s.Text);
                if(string.IsNullOrEmpty(normalised)) continue;

                _idToText[s.Id]       = normalised;
                _textToId[normalised] = s.Id;

                if(s.Id > LastSeenStringId) LastSeenStringId = s.Id;
            }

            List<SoftwareAttributeStringTranslation> allTranslations =
                await ctx.SoftwareAttributeStringTranslations
                         .AsNoTracking()
                         .ToListAsync(ct);

            foreach(SoftwareAttributeStringTranslation t in allTranslations)
            {
                if(string.IsNullOrEmpty(t.LanguageCode) || t.Translation is null) continue;

                ConcurrentDictionary<string, string> langs =
                    _translations.GetOrAdd(t.StringId, _ => new ConcurrentDictionary<string, string>());

                langs[t.LanguageCode] = t.Translation;
            }

            logger.LogInformation(
                "SoftwareAttributeTranslationCache loaded {StringCount} pool strings and {TranslationCount} translations.",
                allStrings.Count, allTranslations.Count);

            _loaded = true;
        }
        finally
        {
            _loadGate.Release();
        }
    }

    /// <summary>
    ///     Returns the translated text for <paramref name="text" /> in
    ///     <paramref name="languageCode" />, falling back to the (NBSP-normalised) original text if no
    ///     translation exists. NEVER throws — unknown strings return the normalised input verbatim
    ///     (typical between worker ticks for newly inserted attributes).
    /// </summary>
    public string GetTranslated(string text, string languageCode)
    {
        string normalised = NormalizeText(text);

        if(string.IsNullOrEmpty(normalised))                                                            return normalised;
        if(string.IsNullOrEmpty(languageCode) || string.Equals(languageCode, "eng", StringComparison.Ordinal))
            return normalised;

        if(!_textToId.TryGetValue(normalised, out int id)) return normalised;

        if(_translations.TryGetValue(id, out ConcurrentDictionary<string, string> langs) &&
           langs.TryGetValue(languageCode, out string translated)                        &&
           !string.IsNullOrWhiteSpace(translated))
            return translated;

        return normalised;
    }

    /// <summary>
    ///     Adds or updates a single translation in-memory. Used by the provider after each successful
    ///     DB insert so subsequent requests see the new translation immediately.
    /// </summary>
    public void Upsert(int stringId, string languageCode, string translation)
    {
        if(string.IsNullOrEmpty(languageCode) || string.Equals(languageCode, "eng", StringComparison.Ordinal) ||
           translation is null)
            return;

        ConcurrentDictionary<string, string> langs =
            _translations.GetOrAdd(stringId, _ => new ConcurrentDictionary<string, string>());

        langs[languageCode] = translation;
    }

    /// <summary>
    ///     Registers a brand-new pool string discovered by the provider. Updates
    ///     <see cref="LastSeenStringId" /> if the new id is higher.
    /// </summary>
    public void RegisterString(int id, string text)
    {
        string normalised = NormalizeText(text);
        if(string.IsNullOrEmpty(normalised)) return;

        _idToText[id]         = normalised;
        _textToId[normalised] = id;

        if(id > LastSeenStringId) LastSeenStringId = id;
    }

    /// <summary>
    ///     Returns true and sets <paramref name="id" /> if the (normalised) text is already in the
    ///     pool. Used by the discovery pass to skip strings that already exist.
    /// </summary>
    public bool TryGetIdByText(string normalisedText, out int id) => _textToId.TryGetValue(normalisedText, out id);

    /// <summary>
    ///     Snapshot of every pool string that lacks a translation for <paramref name="languageCode" />.
    ///     Returned as a fresh list so subsequent cache mutation does not affect the snapshot. Returns
    ///     an empty list for <c>"eng"</c>.
    /// </summary>
    public IReadOnlyList<(int Id, string Text)> SnapshotMissingByLanguage(string languageCode)
    {
        if(string.IsNullOrEmpty(languageCode) || string.Equals(languageCode, "eng", StringComparison.Ordinal))
            return [];

        var missing = new List<(int Id, string Text)>();

        foreach(KeyValuePair<int, string> kv in _idToText)
        {
            if(string.IsNullOrWhiteSpace(kv.Value)) continue;

            bool hasLang = _translations.TryGetValue(kv.Key, out ConcurrentDictionary<string, string> langs) &&
                           langs.ContainsKey(languageCode);

            if(!hasLang) missing.Add((kv.Key, kv.Value));
        }

        return missing;
    }
}
