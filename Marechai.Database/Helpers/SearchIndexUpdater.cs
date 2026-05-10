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
using System.Globalization;
using System.Linq;
using System.Text;
using Marechai.Data;
using Marechai.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Marechai.Database.Helpers;

/// <summary>
///     Pure helpers for the search-entry normalization + per-entity upsert/delete used by the
///     <see cref="Interceptors.SearchIndexInterceptor"/>. Kept side-effect-free and synchronous so
///     it can also be invoked from the seed migration via raw SQL fallback.
/// </summary>
public static class SearchIndexUpdater
{
    /// <summary>
    ///     Lowercases, strips diacritics, removes punctuation, collapses whitespace.
    ///     Matches the SQL normalization in the seed migration:
    ///         REGEXP_REPLACE(LOWER(CONVERT(CONCAT_WS(' ', display_name, alt_name) USING ascii)), '[^a-z0-9 ]+', ' ')
    ///     then collapses runs of spaces.
    /// </summary>
    public static string Normalize(string raw)
    {
        if(string.IsNullOrWhiteSpace(raw)) return string.Empty;

        // Diacritics strip (Unicode FormD then drop NonSpacingMark).
        string formD = raw.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(formD.Length);

        foreach(char ch in formD)
        {
            UnicodeCategory cat = CharUnicodeInfo.GetUnicodeCategory(ch);
            if(cat == UnicodeCategory.NonSpacingMark) continue;
            sb.Append(ch);
        }

        string ascii = sb.ToString().ToLowerInvariant();

        // Replace any non-alphanumeric with space, collapse multi-space.
        var collapsed = new StringBuilder(ascii.Length);
        bool prevSpace = true;

        foreach(char ch in ascii)
        {
            bool isAlphaNum = ch is >= 'a' and <= 'z' or >= '0' and <= '9';

            if(isAlphaNum)
            {
                collapsed.Append(ch);
                prevSpace = false;
            }
            else if(!prevSpace)
            {
                collapsed.Append(' ');
                prevSpace = true;
            }
        }

        string result = collapsed.ToString().Trim();
        if(result.Length > 1024) result = result[..1024];
        return result;
    }

    /// <summary>Compose the text that will be FULLTEXT-indexed: display + alt + tokenized variants (e.g. "c64" splitting).</summary>
    public static string BuildNormalized(string displayName, string altName)
    {
        string baseText = string.IsNullOrWhiteSpace(altName)
                              ? displayName ?? string.Empty
                              : $"{displayName} {altName}";
        return Normalize(baseText);
    }

    /// <summary>
    ///     C# port of MariaDB SOUNDEX(). Algorithm:
    ///     1. Take first ASCII letter, uppercase it.
    ///     2. For remaining letters: ignore non-letters and vowels (A,E,I,O,U) + H,W,Y;
    ///        map consonants to Soundex codes.
    ///     3. Collapse adjacent duplicate codes.
    ///     4. If the first code matches the first letter's own code, drop it
    ///        (e.g. "Nintendo" → first letter N(=5), then N(=5) is dropped → "N353" not "N5353").
    ///     5. Pad to a minimum of 4 chars with '0'. NO truncation.
    /// </summary>
    public static string Soundex(string raw)
    {
        if(string.IsNullOrWhiteSpace(raw)) return string.Empty;

        // Uppercase ASCII (after Normalize() the input is already lowercase ASCII; for raw Unicode
        // input we apply the same normalization so accents don't break the algorithm).
        string upper = Normalize(raw).ToUpperInvariant();
        if(string.IsNullOrEmpty(upper)) return string.Empty;

        // Find first letter.
        int i = 0;
        while(i < upper.Length && !(upper[i] >= 'A' && upper[i] <= 'Z')) i++;
        if(i >= upper.Length) return string.Empty;

        char firstLetter = upper[i];
        char firstCode   = SoundexCode(firstLetter);

        // Build digits for everything after the first letter.
        var digits   = new System.Text.StringBuilder(8);
        char lastCode = '\0';
        for(int j = i + 1; j < upper.Length; j++)
        {
            char ch = upper[j];
            if(ch < 'A' || ch > 'Z') continue;
            char code = SoundexCode(ch);
            if(code == '0') continue;          // vowel / h / w / y
            if(code == lastCode) continue;     // collapse adjacent duplicates
            digits.Append(code);
            lastCode = code;
        }

        // If the very first digit equals the first-letter's own code, drop it (MariaDB quirk).
        int start = digits.Length > 0 && digits[0] == firstCode ? 1 : 0;

        var sb = new System.Text.StringBuilder(8);
        sb.Append(firstLetter);
        for(int k = start; k < digits.Length; k++) sb.Append(digits[k]);

        // Pad to a minimum of 4 chars with '0' (matches MariaDB short-name output like "A140").
        while(sb.Length < 4) sb.Append('0');

        return sb.Length > 10 ? sb.ToString(0, 10) : sb.ToString();
    }

    static char SoundexCode(char ch) => ch switch
    {
        'B' or 'F' or 'P' or 'V'                             => '1',
        'C' or 'G' or 'J' or 'K' or 'Q' or 'S' or 'X' or 'Z' => '2',
        'D' or 'T'                                           => '3',
        'L'                                                  => '4',
        'M' or 'N'                                           => '5',
        'R'                                                  => '6',
        _                                                    => '0' // A,E,I,O,U,H,W,Y, anything else
    };

    /// <summary>Insert or update a search entry. Caller is responsible for SaveChanges.</summary>
    public static void Upsert(MarechaiContext ctx, SearchEntityType type, long entityId, string displayName,
                              string altName, int? year, short? countryId, int? companyId, bool hasImage,
                              byte? kind = null)
    {
        if(string.IsNullOrWhiteSpace(displayName)) return;

        SearchEntry existing = ctx.SearchEntries.FirstOrDefault(e => e.EntityType == type && e.EntityId == entityId);
        string normalized   = BuildNormalized(displayName, altName);
        string soundex      = Soundex(displayName);

        if(existing == null)
        {
            ctx.SearchEntries.Add(new SearchEntry
            {
                EntityType     = type,
                EntityId       = entityId,
                DisplayName    = Trim(displayName, 512),
                AltName        = Trim(altName,     512),
                NormalizedName = normalized,
                Year           = year,
                CountryId      = countryId,
                CompanyId      = companyId,
                HasImage       = hasImage,
                Kind           = kind,
                Soundex        = soundex
            });
        }
        else
        {
            existing.DisplayName    = Trim(displayName, 512);
            existing.AltName        = Trim(altName,     512);
            existing.NormalizedName = normalized;
            existing.Year           = year;
            existing.CountryId      = countryId;
            existing.CompanyId      = companyId;
            existing.HasImage       = hasImage;
            existing.Kind           = kind;
            existing.Soundex        = soundex;
        }
    }

    /// <summary>Delete a search entry. Safe if the row doesn't exist.</summary>
    public static void Delete(MarechaiContext ctx, SearchEntityType type, long entityId)
    {
        SearchEntry existing = ctx.SearchEntries.FirstOrDefault(e => e.EntityType == type && e.EntityId == entityId);
        if(existing != null) ctx.SearchEntries.Remove(existing);
    }

    static string Trim(string s, int max)
    {
        if(string.IsNullOrEmpty(s)) return null;
        return s.Length > max ? s[..max] : s;
    }

    /// <summary>Extract the year from a nullable DateTime, respecting precision (any precision still has a year).</summary>
    public static int? YearOf(DateTime? dt) => dt?.Year;
}
