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
using Marechai.Database.Helpers;

namespace Marechai.Server.Services;

/// <summary>
///     Pure helpers for the site-wide search ranking. Uses Jaro-Winkler similarity (0..1, higher = better)
///     which is specifically tuned for short strings with a common-prefix bonus — the gold standard for
///     name / title fuzzy matching. Combined in <see cref="Score"/> with prefix-bonus, substring-bonus and
///     a length-ratio penalty.
/// </summary>
public sealed class FuzzySearchService
{
    /// <summary>
    ///     Jaro similarity in [0,1]. Identical strings → 1.0. Returns 0 if either string is empty.
    /// </summary>
    public double Jaro(string a, string b)
    {
        if(string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b)) return 0d;
        if(a == b) return 1d;

        int la = a.Length;
        int lb = b.Length;

        int matchWindow = Math.Max(la, lb) / 2 - 1;
        if(matchWindow < 0) matchWindow = 0;

        bool[] aMatched = new bool[la];
        bool[] bMatched = new bool[lb];

        int matches = 0;
        for(int i = 0; i < la; i++)
        {
            int start = Math.Max(0, i - matchWindow);
            int end   = Math.Min(i + matchWindow + 1, lb);
            for(int j = start; j < end; j++)
            {
                if(bMatched[j] || a[i] != b[j]) continue;
                aMatched[i] = true;
                bMatched[j] = true;
                matches++;
                break;
            }
        }

        if(matches == 0) return 0d;

        // Count transpositions.
        int t = 0;
        int k = 0;
        for(int i = 0; i < la; i++)
        {
            if(!aMatched[i]) continue;
            while(!bMatched[k]) k++;
            if(a[i] != b[k]) t++;
            k++;
        }

        double m = matches;
        return (m / la + m / lb + (m - t / 2d) / m) / 3d;
    }

    /// <summary>
    ///     Jaro-Winkler similarity in [0,1]. Adds a prefix bonus (up to first 4 chars) for strings that
    ///     start the same way — empirically best for names/titles. Scaling factor 0.1 (Winkler's original).
    /// </summary>
    public double JaroWinkler(string a, string b)
    {
        double j = Jaro(a, b);
        if(j < 0.7d) return j; // Per Winkler, only boost when base similarity is already high.

        int prefix = 0;
        int max    = Math.Min(4, Math.Min(a?.Length ?? 0, b?.Length ?? 0));
        for(int i = 0; i < max; i++)
        {
            if(a![i] == b![i]) prefix++;
            else break;
        }

        return j + prefix * 0.1d * (1d - j);
    }

    /// <summary>
    ///     Composite relevance score in the rough range [0, 3]. Higher is better.
    ///     Combines: prefix bonus (0..1), full-substring bonus (0..0.5), Jaro-Winkler similarity (0..1),
    ///     length-ratio penalty.
    /// </summary>
    public double Score(string normalizedQuery, string normalizedCandidate)
    {
        if(string.IsNullOrEmpty(normalizedQuery) || string.IsNullOrEmpty(normalizedCandidate)) return 0d;

        double score = 0d;

        if(normalizedCandidate.StartsWith(normalizedQuery, StringComparison.Ordinal)) score += 1.0d;
        else if(normalizedCandidate.Contains(normalizedQuery, StringComparison.Ordinal)) score += 0.5d;

        // Jaro-Winkler against the whole candidate AND each token; take the best.
        double bestJw = JaroWinkler(normalizedQuery, normalizedCandidate);
        if(normalizedCandidate.Contains(' '))
        {
            foreach(string token in normalizedCandidate.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                double jw = JaroWinkler(normalizedQuery, token);
                if(jw > bestJw) bestJw = jw;
            }
        }

        score += bestJw;

        // Length-ratio penalty: dramatically-longer candidates score slightly lower.
        double ratio = (double)normalizedQuery.Length / Math.Max(normalizedCandidate.Length, 1);
        if(ratio < 0.1d) ratio = 0.1d;
        score *= 0.5d + 0.5d * ratio;

        return score;
    }

    /// <summary>Convenience: normalize the raw query then score.</summary>
    public double ScoreRaw(string rawQuery, string normalizedCandidate) =>
        Score(SearchIndexUpdater.Normalize(rawQuery), normalizedCandidate);
}
