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

namespace Marechai.Helpers;

public static class JaroWinkler
{
    const double WinklerPrefixWeight = 0.1;
    const int    MaxPrefixLength     = 4;

    /// <summary>Returns a similarity score between 0.0 (no similarity) and 1.0 (identical).</summary>
    public static double Similarity(string s1, string s2)
    {
        if(string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
            return 0.0;

        if(string.Equals(s1, s2, StringComparison.OrdinalIgnoreCase))
            return 1.0;

        string a = s1.ToUpperInvariant();
        string b = s2.ToUpperInvariant();

        double jaro = JaroSimilarity(a, b);

        // Winkler modification: boost score for common prefix
        int prefixLen = 0;

        for(int i = 0; i < Math.Min(Math.Min(a.Length, b.Length), MaxPrefixLength); i++)
        {
            if(a[i] == b[i])
                prefixLen++;
            else
                break;
        }

        return jaro + prefixLen * WinklerPrefixWeight * (1.0 - jaro);
    }

    static double JaroSimilarity(string a, string b)
    {
        int matchWindow = Math.Max(a.Length, b.Length) / 2 - 1;

        if(matchWindow < 0)
            matchWindow = 0;

        var aMatched = new bool[a.Length];
        var bMatched = new bool[b.Length];

        int matches       = 0;
        int transpositions = 0;

        // Find matches
        for(int i = 0; i < a.Length; i++)
        {
            int start = Math.Max(0, i - matchWindow);
            int end   = Math.Min(i + matchWindow + 1, b.Length);

            for(int j = start; j < end; j++)
            {
                if(bMatched[j] || a[i] != b[j])
                    continue;

                aMatched[i] = true;
                bMatched[j] = true;
                matches++;

                break;
            }
        }

        if(matches == 0)
            return 0.0;

        // Count transpositions
        int k = 0;

        for(int i = 0; i < a.Length; i++)
        {
            if(!aMatched[i])
                continue;

            while(!bMatched[k])
                k++;

            if(a[i] != b[k])
                transpositions++;

            k++;
        }

        double m = matches;

        return (m / a.Length + m / b.Length + (m - transpositions / 2.0) / m) / 3.0;
    }

    /// <summary>
    ///     Finds all items with similarity above the threshold, sorted by descending score.
    ///     Returns the best matches with their scores.
    /// </summary>
    public static List<(T item, double score)> FindMatches<T>(
        string             input,
        IEnumerable<T>     candidates,
        Func<T, string>   nameSelector,
        double             threshold = 0.85)
    {
        var results = new List<(T item, double score)>();

        foreach(T candidate in candidates)
        {
            string name = nameSelector(candidate);

            if(string.IsNullOrWhiteSpace(name))
                continue;

            double score = Similarity(input, name);

            if(score >= threshold)
                results.Add((candidate, score));
        }

        return results.OrderByDescending(r => r.score).ToList();
    }
}
